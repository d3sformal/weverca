using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel
{
    class FieldStorageVisitor : AbstractValueVisitor
    {
        private readonly Snapshot _context;
        private readonly List<VariableInfo> _indexStorages = new List<VariableInfo>();
        private readonly VariableIdentifier _field;

        internal readonly VariableInfo[] Storages;

        internal FieldStorageVisitor(ReadWriteSnapshotEntryBase fieldedEntry, Snapshot context, VariableIdentifier field)
        {
            _context = context;
            _field = field;

            var fieldedValues=fieldedEntry.ReadMemory(context);
            foreach (var fieldedValue in fieldedValues.PossibleValues)
            {
                fieldedValue.Accept(this);
            }

            Storages = _indexStorages.ToArray();
        }

        public override void VisitValue(Value value)
        {
            throw new NotImplementedException("Reading field of given value type is not implemented yet");
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            applyField(value);
        }

        private void applyField(ObjectValue objectValue)
        {
            _indexStorages.AddRange(_context.FieldStorages(objectValue, _field));
        }
    }
}
