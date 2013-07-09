using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;
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


        public override MemoryEntry ThisObject
        {
            get { return readValue(thisObjectStorage()); }
        }

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

        protected override AbstractSnapshot createCall(MemoryEntry thisObject, MemoryEntry[] arguments)
        {
            //TODO implement

            var snapshot = new Snapshot();

            if (thisObject != null)
            {
                snapshot.assign(thisObjectStorage(), thisObject);
            }
            return snapshot;
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

                    setEntry(allocatedReference, entry);
                    break;
                case 1:
                    setEntry(references[0], entry);
                    break;

                default:
                    weakUpdate(references, entry);
                    break;
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


        protected override void mergeWithCallLevel(ISnapshotReadonly[] callOutput)
        {
            //TODO this is dummy workaround
            foreach (Snapshot callInput in callOutput)
            {
                extendVariables(callInput);
                extendData(callInput);
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


        protected override MemoryEntry readValue(VariableName sourceVar)
        {
            var info = getInfo(sourceVar);

            if (info == null)
            {
                return UndefinedValueEntry;
            }

            return resolveReferences(info.References);
        }

        private void weakUpdate(List<VirtualReference> references, MemoryEntry update)
        {
            foreach (var reference in references)
            {
                var entry = getEntry(reference);
                ReportMemoryEntryMerge();
                var updated = MemoryEntry.Merge(entry, update);

                setEntry(reference, updated);
            }

        }

        private MemoryEntry resolveReferences(List<VirtualReference> references)
        {
            switch (references.Count)
            {
                case 0:
                    return UndefinedValueEntry;
                case 1:
                    return getEntry(references[0]);
                default:
                    var entries = from reference in references select getEntry(reference);

                    return MemoryEntry.Merge(entries);

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

        private void setEntry(VirtualReference reference, MemoryEntry entry)
        {
            if (entry == null)
            {
                throw new NotSupportedException("Entry cannot be null");
            }
            ReportSimpleHashAssign();
            _data[reference] = entry;
        }

        private VariableInfo getInfo(VariableName name)
        {
            ReportSimpleHashSearch();

            VariableInfo info;
            if (_variables.TryGetValue(name, out info))
            {
                return info;
            }

            return null;
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




        protected override void fetchFromGlobal(IEnumerable<VariableName> variables)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<VariableName> getGlobalVariables()
        {
            throw new NotImplementedException();
        }



        private VariableName functionStorage(string functionName)
        {
            return new VariableName("$function:" + functionName);
        }

        protected override void declareGlobal(FunctionValue function)
        {
            var storage = functionStorage(function.Declaration.Name.Value);

            ReportMemoryEntryCreation();
            //TODO assign into global scope
            assign(storage, new MemoryEntry(function));
        }
        protected override IEnumerable<FunctionValue> resolveFunction(QualifiedName functionName)
        {
            var storage = functionStorage(functionName.Name.Value);
            //TODO read from global scope
            var entry = readValue(storage);

            return from FunctionValue function in entry.PossibleValues select function;
        }

        protected VariableName typeStorage(string typeName)
        {
            return new VariableName("$type:" + typeName);
        }

        protected override void declareGlobal(TypeValue declaration)
        {
            var storage = typeStorage(declaration.Declaration.Type.QualifiedName.Name.Value);

            var entry = readValue(storage);

            ReportMemoryEntryCreation();
            //TODO assign into global scope
            assign(storage, new MemoryEntry(declaration));
        }

        protected override IEnumerable<TypeValue> resolveType(QualifiedName typeName)
        {
            var storage = typeStorage(typeName.Name.Value);
            //TODO read from global scope
            var entry = readValue(storage);

            return from TypeValue type in entry.PossibleValues select type;
        }

        #region Object operations
        protected override void setField(ObjectValue value, ContainerIndex index, MemoryEntry entry)
        {
            var storage = getFieldStorage(value, index);
            assign(storage, entry);
        }

        protected override void setFieldAlias(ObjectValue value, ContainerIndex index, AliasValue alias)
        {
            var storage = getFieldStorage(value, index);
            assignAlias(storage, alias);
        }

        protected override MemoryEntry getField(ObjectValue value, ContainerIndex index)
        {
            var storage = getFieldStorage(value, index);
            return readValue(storage);
        }
        protected override void initializeObject(ObjectValue createdObject, TypeValue type)
        {
            var info = getObjectInfoStorage(createdObject);
            //TODO set info
            ReportMemoryEntryCreation();
            assign(info, new MemoryEntry(type));
        }

        protected override IEnumerable<MethodDecl> resolveMethod(ObjectValue objectValue, QualifiedName methodName)
        {
            var info = getObjectInfoStorage(objectValue);
            var objInfo = readValue(info);
            if (objInfo.PossibleValues.Count() != 1)
            {
                throw new NotImplementedException();
            }

            var type = objInfo.PossibleValues.First() as TypeValue;
            foreach (var member in type.Declaration.Members)
            {
                var m = member as MethodDecl;
                if (m == null)
                {
                    continue;
                }

                if (m.Name == methodName.Name)
                {
                    yield return m;
                }                
            }            
        }

        private VariableName getFieldStorage(ObjectValue obj, ContainerIndex field)
        {
            var name = string.Format("$obj{0}->{1}", obj.ObjectID, field.Identifier);
            return new VariableName(name);
        }

        private VariableName getObjectInfoStorage(ObjectValue obj)
        {
            var name = string.Format("$obj{0}#info", obj.ObjectID);
            return new VariableName(name);
        }
        #endregion

        #region Array operations
        protected override void setIndex(AssociativeArray value, ContainerIndex index, MemoryEntry entry)
        {
            var storage = getIndexStorage(value, index);
            assign(storage, entry);
        }

        protected override void setIndexAlias(AssociativeArray value, ContainerIndex index, AliasValue alias)
        {
            var storage = getIndexStorage(value, index);
            assignAlias(storage, alias);
        }

        protected override MemoryEntry getIndex(AssociativeArray value, ContainerIndex index)
        {
            var storage = getIndexStorage(value, index);
            return readValue(storage);
        }

        protected override void initializeArray(AssociativeArray createdArray)
        {
            //TODO initialize array
            var info = getArrayInfoStorage(createdArray);
            ReportMemoryEntryCreation();
            assign(info, new MemoryEntry());
        }

        private VariableName getIndexStorage(AssociativeArray arr, ContainerIndex index)
        {
            var name = string.Format("$arr{0}[{1}]", arr.ArrayID, index.Identifier);
            return new VariableName(name);
        }

        private VariableName thisObjectStorage()
        {
            return new VariableName("this");
        }

        private VariableName getArrayInfoStorage(AssociativeArray arr)
        {
            var name = string.Format("$arr{0}#info", arr.ArrayID);
            return new VariableName(name);
        }
        #endregion

        public override string ToString()
        {
            var result = new StringBuilder();

            foreach (var variable in _variables.Keys)
            {
                result.AppendFormat("{0}: {{", variable);

                foreach (var value in readValue(variable).PossibleValues)
                {
                    result.AppendFormat("'{0}', ", value);
                }

                result.Length -= 2;
                result.AppendLine("}");
            }

            return result.ToString();
        }
    }
}
