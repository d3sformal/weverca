using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework.Memory;

using Weverca.MemoryModels.VirtualReferenceModel.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel.SnapshotEntries
{
    class IndexStorageVisitor : AbstractValueVisitor
    {
        private readonly Snapshot _context;
        private readonly List<VariableKey> _indexStorages = new List<VariableKey>();
        private readonly MemberIdentifier _index;

        private AssociativeArray implicitArray;


        internal readonly ReadWriteSnapshotEntryBase IndexedValue;


        internal IndexStorageVisitor(SnapshotStorageEntry indexedEntry, Snapshot context, MemberIdentifier index)
        {
            _context = context;
            _index = index;
            var indexedValues = indexedEntry.ReadMemory(context);
            foreach (var indexedValue in indexedValues.PossibleValues)
            {
                indexedValue.Accept(this);
            }

            if (implicitArray != null)
                //TODO replace only undefined values
                indexedEntry.WriteMemory(context, new MemoryEntry(implicitArray));

            IndexedValue = new SnapshotStorageEntry(null, indexedEntry.IsWeak, _indexStorages.ToArray());
        }

        public override void VisitValue(Value value)
        {
            throw new NotImplementedException("Reading index of given value type is not implemented yet");
        }

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            var array = getImplicitArray();

            applyIndex(array);
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            applyIndex(value);
        }

        public override void VisitAnyValue(AnyValue value)
        {
            var indexed = _context.MemoryAssistant.ReadIndex(value, _index);

            var storages = _context.IndexStorages(value, _index).ToArray();
            _context.Write(storages, indexed, false);
            _indexStorages.AddRange(storages);
        }

        private void applyIndex(AssociativeArray array)
        {
            _indexStorages.AddRange(_context.IndexStorages(array, _index));
        }

        private AssociativeArray getImplicitArray()
        {
            if (implicitArray == null)
                implicitArray = _context.CreateArray();

            return implicitArray;
        }
    }
}
