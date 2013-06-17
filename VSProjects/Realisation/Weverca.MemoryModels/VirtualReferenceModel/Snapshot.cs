using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using Weverca.Analysis;
using Weverca.Analysis.Memory;

namespace Weverca.VirtualReferenceModel
{

    /// <summary>
    /// Simple (non efficient) implementation as proof of concept - will be heavily optimized, refactored
    /// </summary>
    public class Snapshot : AbstractSnapshot
    {
        Dictionary<VariableName, VariableInfo> _oldVariables;
        Dictionary<VariableName, VariableInfo> _variables = new Dictionary<VariableName, VariableInfo>();
        Dictionary<VirtualReference, MemoryEntry> _oldData;
        Dictionary<VirtualReference, MemoryEntry> _data = new Dictionary<VirtualReference, MemoryEntry>();



        bool _hasSemanticChange;

        protected override void startTransaction()
        {
            _oldData = _data;
            _oldVariables = _variables;

            _data = new Dictionary<VirtualReference, MemoryEntry>(_data);
            _variables = new Dictionary<VariableName, VariableInfo>(_variables);

            _hasSemanticChange = false;
        }

        protected override bool commitTransaction()
        {
            if (_hasSemanticChange)
            {
                return true;
            }
            else
            {
                _hasSemanticChange =
                    _data.Count != _oldData.Count ||
                    _variables.Count != _oldVariables.Count;

                if (_hasSemanticChange)
                {
                    //evident change
                    return true;
                }

                //check variables according to old ones
                foreach (var oldVar in _oldVariables)
                {
                    ReportSimpleHashSearch();
                    VariableInfo currVar;
                    if (!_variables.TryGetValue(oldVar.Key, out currVar))
                    {
                        //differ in some variable presence
                        return true;
                    }

                    if (!currVar.Equals(oldVar.Value))
                    {
                        //differ in variable definition
                        return true;
                    }
                }

                foreach (var oldData in _oldData)
                {
                    ReportSimpleHashSearch();
                    MemoryEntry currEntry;
                    if (!_data.TryGetValue(oldData.Key, out currEntry))
                    {
                        //differ in presence of some reference
                        return true;
                    }

                    ReportMemoryEntryComparison();
                    if (!currEntry.Equals(oldData.Value))
                    {
                        //differ in stored data
                        return true;
                    }
                }
            }

            return false;
        }


        protected override AliasValue createAlias(VariableName sourceVar)
        {
            var info = getInfo(sourceVar);
            return new ReferenceAlias(info.References);
        }

        protected override AbstractSnapshot createCall(CallInfo callInfo)
        {
            throw new NotImplementedException();
        }

        protected override void assign(VariableName targetVar, MemoryEntry entry)
        {
            var info = getOrCreate(targetVar);
            var references = info.References;

            switch (references.Count)
            {
                case 0:
                    //reserve new virtual reference
                    var allocatedReference = new VirtualReference(targetVar);
                    info.References.Add(allocatedReference);

                    ReportSimpleHashAssign();
                    _data[allocatedReference] = entry;
                    break;
                case 1:
                    ReportSimpleHashAssign();
                    _data[references[0]] = entry;
                    break;

                default:
                    throw new NotImplementedException("Weak update to all references");
            }

        }

        protected override void assignAlias(VariableName targetVar, AliasValue alias)
        {
            var info = getOrCreate(targetVar);
            var refAlias = alias as ReferenceAlias;

            if (!hasSameReferences(info, refAlias.References))
            {
                reportSemanticChange();
                info.References.Clear();
                info.References.AddRange(refAlias.References);
            }
        }

        /// <summary>
        /// TODO this implementation is inefficient
        /// </summary>
        /// <param name="inputs"></param>
        protected override void extend(ISnapshotReadonly[] inputs)
        {


            _data = new Dictionary<VirtualReference, MemoryEntry>();
            _variables = new Dictionary<VariableName, VariableInfo>();
            foreach (Snapshot input in inputs)
            {
                //merge info from extending inputs
                extendVariables(input);
                extendData(input);
            }
        }

        private void extendData(Snapshot input)
        {
            foreach (var dataPair in input._data)
            {
                MemoryEntry oldEntry;
                ReportSimpleHashSearch();

                if (!_data.TryGetValue(dataPair.Key, out oldEntry))
                {
                    //copy reference, because its immutable
                    ReportSimpleHashAssign();
                    _data[dataPair.Key] = dataPair.Value;
                }
                else
                {
                    ReportMemoryEntryComparison();
                    if (!dataPair.Value.Equals(oldEntry))
                    {
                        //merge two memory entries
                        ReportMemoryEntryMerge();
                        _data[dataPair.Key] = MemoryEntry.Merge(oldEntry, dataPair.Value);
                    }
                }
            }
        }

        private void extendVariables(Snapshot input)
        {
            foreach (var varPair in input._variables)
            {
                VariableInfo oldVar;
                ReportSimpleHashSearch();
                if (!_variables.TryGetValue(varPair.Key, out oldVar))
                {
                    //copy variable info, so we can process changes on it
                    ReportSimpleHashAssign();
                    _variables[varPair.Key] = varPair.Value.Clone();
                }
                else
                {
                    //merge variable references
                    foreach (var reference in varPair.Value.References)
                    {
                        if (!oldVar.References.Contains(reference))
                        {
                            oldVar.References.Add(reference);
                        }
                    }
                }
            }
        }

        protected override void mergeWithCall(CallResult result, ISnapshotReadonly callOutput)
        {
            throw new NotImplementedException();
        }

        protected override MemoryEntry readValue(VariableName sourceVar)
        {
            var info = getInfo(sourceVar);

            return resolveReferences(info.References);
        }

        private MemoryEntry resolveReferences(List<VirtualReference> references)
        {
            switch (references.Count)
            {
                case 0:
                    return UndefinedValueEntry;
                case 1:
                    ReportSimpleHashSearch();
                    return _data[references[0]];
                default:
                    var values = new List<Value>();
                    foreach (var reference in references)
                    {
                        values.AddRange(resolveValuesFrom(reference));
                    }
                    ReportMemoryEntryCreation();
                    return new MemoryEntry(values.ToArray());

            }
        }

        private IEnumerable<Value> resolveValuesFrom(VirtualReference reference)
        {
            var entry = getEntry(reference);
            if (entry == null)
            {
                return new Value[0];
            }

            return entry.PossibleValues;
        }

        private VariableInfo getOrCreate(VariableName name)
        {
            VariableInfo result;

            ReportSimpleHashSearch();
            if (!_variables.TryGetValue(name, out result))
            {
                _variables[name] = result = new VariableInfo();
                ReportSimpleHashAssign();
            }

            return result;
        }

        private MemoryEntry getEntry(VirtualReference reference)
        {
            MemoryEntry entry;
            ReportSimpleHashSearch();
            _data.TryGetValue(reference, out entry);

            return entry;
        }

        private VariableInfo getInfo(VariableName name)
        {
            ReportSimpleHashSearch();
            return _variables[name];
        }

        private bool hasSameReferences(VariableInfo info, List<VirtualReference> references)
        {
            if (info.References.Count != references.Count)
            {
                return false;
            }

            foreach (var reference in references)
            {
                if (!info.References.Contains(reference))
                    return false;
            }

            return true;
        }

        private void reportSemanticChange()
        {
            if (_hasSemanticChange)
                return;

            _hasSemanticChange = true;
        }

        protected override void setField(ObjectValue value, ContainerIndex index, MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        protected override void setIndex(AssociativeArray value, ContainerIndex index, MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        protected override void setFieldAlias(ObjectValue value, ContainerIndex index, AliasValue alias)
        {
            throw new NotImplementedException();
        }

        protected override void setIndexAlias(AssociativeArray value, ContainerIndex index, AliasValue alias)
        {
            throw new NotImplementedException();
        }

        protected override MemoryEntry getField(ObjectValue value, ContainerIndex index)
        {
            throw new NotImplementedException();
        }

        protected override MemoryEntry getIndex(AssociativeArray value, ContainerIndex index)
        {
            throw new NotImplementedException();
        }
    }
}
