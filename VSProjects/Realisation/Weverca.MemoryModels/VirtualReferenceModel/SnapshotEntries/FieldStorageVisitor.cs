using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

using Weverca.MemoryModels.VirtualReferenceModel.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel.SnapshotEntries
{
    class FieldStorageVisitor : AbstractValueVisitor
    {
        private readonly Snapshot _context;
        private readonly List<VariableKey> _indexStorages = new List<VariableKey>();
        private readonly VariableIdentifier _field;

        private ObjectValue _implicitObject;

        internal readonly VariableKey[] Storages;

        internal FieldStorageVisitor(ReadWriteSnapshotEntryBase fieldedEntry, Snapshot context, VariableIdentifier field)
        {
            _context = context;
            _field = field;

            var fieldedValues = fieldedEntry.ReadMemory(context);
            foreach (var fieldedValue in fieldedValues.PossibleValues)
            {
                fieldedValue.Accept(this);
            }

            if (_implicitObject != null)
                //TODO replace only undefined values
                fieldedEntry.WriteMemory(context, new MemoryEntry(_implicitObject));

            Storages = _indexStorages.ToArray();
        }

        public override void VisitValue(Value value)
        {
            throw new NotImplementedException("Reading field of given value type is not implemented yet");
        }

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            var obj = getImplicitObject();
            applyField(obj);
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            applyField(value);
        }

        public override void VisitAnyValue(AnyValue value)
        {
            var fielded = _context.MemoryAssistant.ReadField(value, _field);

            var storages = _context.FieldStorages(value, _field).ToArray();
            _context.Write(storages, fielded, _field.PossibleNames.Count() > 1);
            _indexStorages.AddRange(storages);
        }

        private void applyField(ObjectValue objectValue)
        {
            _indexStorages.AddRange(_context.FieldStorages(objectValue, _field));
        }

        private ObjectValue getImplicitObject()
        {
            if (_implicitObject == null)
                //TODO type for implicit object ?
                //_implicitObject = _context.CreateObject(null);
                throw new NotImplementedException("What is type of implicit object ? ");

            return _implicitObject;
        }
    }
}
