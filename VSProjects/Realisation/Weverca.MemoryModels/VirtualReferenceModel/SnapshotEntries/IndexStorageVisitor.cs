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
        private bool _needsTemporaryIndex = false;
        private bool _hasOnlyArrays = true;

        private readonly Snapshot _context;
        private readonly List<VariableKeyBase> _indexStorages = new List<VariableKeyBase>();
        private readonly MemberIdentifier _index;

        private AssociativeArray implicitArray;


        internal readonly ReadWriteSnapshotEntryBase IndexedValue;


        internal IndexStorageVisitor(SnapshotStorageEntry indexedEntry, Snapshot context, MemberIdentifier index)
        {
            _context = context;
            _index = index;
            var indexedValues = indexedEntry.ReadMemory(context);
            VisitMemoryEntry(indexedValues);

            if (implicitArray != null)
                //TODO replace only undefined values
                indexedEntry.WriteMemoryWithoutCopy(context, new MemoryEntry(implicitArray));

            var forceStrong = indexedEntry.ForceStrong;
            if (_hasOnlyArrays && indexedEntry.HasDirectIdentifier && index.IsDirect)
                //optimization
                forceStrong = true;

            if (_needsTemporaryIndex)
            {
                foreach (var key in indexedEntry.Storages)
                {
                    _indexStorages.Add(new VariableIndexKey(key, _index));
                }
            }

            IndexedValue = new SnapshotStorageEntry(null, forceStrong, _indexStorages.ToArray());
        }

        public override void VisitValue(Value value)
        {
            _hasOnlyArrays = false;
            _needsTemporaryIndex = true;
        }

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            _hasOnlyArrays = false;

            var array = getImplicitArray();

            applyIndex(array);
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            applyIndex(value);
        }

        public override void VisitAnyValue(AnyValue value)
        {
            //read indexed value through memory assistant
            var indexed = _context.MemoryAssistant.ReadAnyValueIndex(value, _index);

            applyIndexWithWriteBack(value, indexed);
        }

        private void applyIndex(AssociativeArray array)
        {
            _indexStorages.AddRange(_context.IndexStorages(array, _index));
        }

        private void applyIndexWithWriteBack(Value value, MemoryEntry indexedValue)
        {
            //write back indexed value so it can be read back later
            var storages = _context.IndexStorages(value, _index).ToArray();
            _context.Write(storages, indexedValue, false, false);

            //keep index storages because of overall reading
            _indexStorages.AddRange(storages);
        }

        private AssociativeArray getImplicitArray()
        {
            if (implicitArray == null)
                implicitArray = _context.CreateArray();

            return implicitArray;
        }
    }
}
