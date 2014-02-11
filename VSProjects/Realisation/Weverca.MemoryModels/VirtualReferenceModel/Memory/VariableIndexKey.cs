using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel.Memory
{
    /// <summary>
    /// Resolve indexing of non arrays
    /// </summary>
    class VariableIndexKey : VariableKeyBase
    {
        private readonly MemberIdentifier _index;

        private readonly VariableKeyBase _indexedVariable;

        /// <summary>
        /// Storage of indexed value - here is pointing current key
        /// </summary>
        private readonly VariableName _storageName;

        internal VariableIndexKey(VariableKeyBase indexedVariable, MemberIdentifier index)
        {
            _indexedVariable = indexedVariable;
            _index = index;

            //TODO what about multiple names ?
            _storageName = new VariableName(string.Format("{0}_index-{1}", _indexedVariable, _index.DirectName));
        }

        internal override VariableInfo GetOrCreateVariable(Snapshot snapshot)
        {
            var variable = snapshot.GetInfo(_storageName, VariableKind.Meta);
            if (variable == null)
            {
                variable = snapshot.CreateEmptyVar(_storageName, VariableKind.Meta);
                variable.References.Add(createProxyReference());
            }

            return variable;
        }

        internal override VariableInfo GetVariable(Snapshot snapshot)
        {
            return snapshot.GetInfo(_storageName, VariableKind.Meta);
        }

        internal override VirtualReference CreateImplicitReference(Snapshot snapshot)
        {
            //there shouldnt be needed for creating implicit reference
            return createProxyReference();
        }

        private VirtualReference createProxyReference()
        {
            return new CallbackReference(_storageName, getIndex, setIndex);
        }

        private MemoryEntry getIndex(Snapshot s)
        {
            var indexReader = new IndexReadExecutor(s.MemoryAssistant, _index);

            var values = getIndexedValues(s);
            indexReader.VisitMemoryEntry(values);

            return new MemoryEntry(indexReader.Result);
        }

        private void setIndex(Snapshot s, MemoryEntry value)
        {
            var indexWriter = new IndexWriteExecutor(s.MemoryAssistant, _index, value);

            var values = getIndexedValues(s);
            indexWriter.VisitMemoryEntry(values);

            var result = new MemoryEntry(indexWriter.Result);

            s.Write(new[] { _indexedVariable }, result, true, true);
        }

        private MemoryEntry getIndexedValues(Snapshot s)
        {
            var values = s.ReadValue(_indexedVariable);
            return values;
        }

        public override string ToString()
        {
            return _storageName.ToString();
        }
    }
}
