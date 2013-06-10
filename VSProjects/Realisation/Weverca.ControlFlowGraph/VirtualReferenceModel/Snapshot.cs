using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using Weverca.ControlFlowGraph.Analysis;
using Weverca.ControlFlowGraph.Analysis.Memory;

namespace Weverca.ControlFlowGraph.VirtualReferenceModel
{
    class Snapshot:AbstractSnapshot
    {
        Dictionary<VariableName, VariableInfo> _variables = new Dictionary<VariableName, VariableInfo>();
        Dictionary<VirtualReference, MemoryEntry> _data = new Dictionary<VirtualReference, MemoryEntry>();
        bool _hasSemanticChange;

        protected override void startTransaction()
        {
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
                throw new NotImplementedException("Change can come from extend or from data assigns");
            }
        }

        protected override ObjectValue createObject()
        {
            throw new NotImplementedException();
        }

        protected override AssociativeArray createArray()
        {
            throw new NotImplementedException();
        }

        protected override AliasValue createAlias(VariableName sourceVar)
        {
            var info=getInfo(sourceVar);
            return new ReferenceAlias(info.References);
        }

        protected override AbstractSnapshot createCall(CallInfo callInfo)
        {
            throw new NotImplementedException();
        }

        protected override void assign(VariableName targetVar, MemoryEntry entry)
        {
            var references=getInfo(targetVar).References;

            switch (references.Count)
            {
                case 0:
                    throw new NotImplementedException("This shouldn't ever happend");

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
            var info = getInfo(targetVar);
            var refAlias=alias as ReferenceAlias;

            if (!hasSameReferences(info,refAlias.References))
            {
                reportSemanticChange();
                info.References.Clear();
                info.References.AddRange(refAlias.References);
            }
        }

        protected override void extend(ISnapshotReadonly[] inputs)
        {
            throw new NotImplementedException();
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
                    return new MemoryEntry(UndefinedValue);
                case 1:
                    ReportSimpleHashSearch();
                    return _data[references[0]];
                default:
                    throw new NotImplementedException("Merge all memory entries");
            }
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
    }
}
