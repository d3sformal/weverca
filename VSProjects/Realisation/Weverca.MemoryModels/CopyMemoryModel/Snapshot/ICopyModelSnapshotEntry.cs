using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    interface ICopyModelSnapshotEntry
    {
        AliasData CreateAliasToEntry(Snapshot snapshot);
        MemoryEntry ReadMemory(Snapshot snapshot);
    }

    class SnapshotEntryHelper
    {
        private SnapshotEntryHelper()
        {

        }

        public static IEnumerable<VariableIdentifier> IterateFields(SnapshotBase context, ICopyModelSnapshotEntry snapshotEntry)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(context);
            MemoryEntry entry = snapshotEntry.ReadMemory(snapshot);

            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            visitor.VisitMemoryEntry(entry);

            return visitor.CollectFields(snapshot);
        }

        public static IEnumerable<MemberIdentifier> IterateIndexes(SnapshotBase context, ICopyModelSnapshotEntry snapshotEntry)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(context);
            MemoryEntry entry = snapshotEntry.ReadMemory(snapshot);

            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            visitor.VisitMemoryEntry(entry);

            return visitor.CollectIndexes(snapshot);
        }

        public static IEnumerable<TypeValue> ResolveType(SnapshotBase context, ICopyModelSnapshotEntry snapshotEntry)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(context);
            MemoryEntry entry = snapshotEntry.ReadMemory(snapshot);

            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            visitor.VisitMemoryEntry(entry);

            return visitor.ResolveObjectsTypes(snapshot);
        }
    }

    class AliasData
    {
        private TemporaryIndex temporaryIndex;
        public IEnumerable<MemoryIndex> MustIndexes { get; private set; }
        public IEnumerable<MemoryIndex> MayIndexes { get; private set; }
        public MemoryIndex SourceIndex { get; private set; }

        public AliasData(IEnumerable<MemoryIndex> mustIndexes, IEnumerable<MemoryIndex> mayIndexes, MemoryIndex sourceIndex)
        {
            this.MayIndexes = mayIndexes;
            this.MustIndexes = mustIndexes;
            this.SourceIndex = sourceIndex;
        }

        internal void Release(Snapshot snapshot)
        {
            if (temporaryIndex != null)
            {
                snapshot.ReleaseTemporary(temporaryIndex);
            }
        }

        internal void TemporaryIndexToRealease(TemporaryIndex temporaryIndex)
        {
            this.temporaryIndex = temporaryIndex;
        }
    }
}
