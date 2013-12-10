using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class DataSnapshotEntry : ReadWriteSnapshotEntryBase, ICopyModelSnapshotEntry
    {
        MemoryEntry dataEntry;
        SnapshotEntry temporaryLocation = null;
        TemporaryIndex temporaryIndex = null;
        
        public DataSnapshotEntry(MemoryEntry entry)
        {
            dataEntry = entry;
        }

        private SnapshotEntry getTemporary(SnapshotBase context)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(context);

            if (temporaryLocation == null)
            {
                temporaryIndex = snapshot.CreateTemporary();
                MergeWithinSnapshotWorker mergeWorker = new MergeWithinSnapshotWorker(snapshot);
                mergeWorker.MergeMemoryEntry(temporaryIndex, dataEntry);

                temporaryLocation = new SnapshotEntry(MemoryPath.MakePathTemporary(temporaryIndex));
            }

            return temporaryLocation;
        }

        private bool isTemporarySet(SnapshotBase context)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(context);

            return temporaryIndex != null && snapshot.IsTemporarySet(temporaryIndex);
        }

        #region ReadWriteSnapshotEntryBase Implementation

        #region Navigation

        protected override ReadWriteSnapshotEntryBase readIndex(SnapshotBase context, MemberIdentifier index)
        {
            return getTemporary(context).ReadIndex(context, index);
        }

        protected override ReadWriteSnapshotEntryBase readField(SnapshotBase context, AnalysisFramework.VariableIdentifier field)
        {
            return getTemporary(context).ReadField(context, field);
        }

        #endregion

        #region Update

        protected override void writeMemory(SnapshotBase context, MemoryEntry value, bool forceStrongWrite)
        {
            getTemporary(context).WriteMemory(context, value, forceStrongWrite);
        }

        protected override void setAliases(SnapshotBase context, ReadSnapshotEntryBase aliasedEntry)
        {
            getTemporary(context).SetAliases(context, aliasedEntry);
        }

        #endregion

        #region Read

        protected override bool isDefined(SnapshotBase context)
        {
            if (isTemporarySet(context))
            {
                return temporaryLocation.IsDefined(context);
            }
            else
            {
                return true;
            }
        }

        protected override IEnumerable<AliasEntry> aliases(SnapshotBase context)
        {

            if (isTemporarySet(context))
            {
                return temporaryLocation.Aliases(context);
            }
            else
            {
                return new AliasEntry[] { };
            }
        }

        protected override MemoryEntry readMemory(SnapshotBase context)
        {
            if (isTemporarySet(context))
            {
                return temporaryLocation.ReadMemory(context);
            }
            else
            {
                return dataEntry;
            }
        }

        protected override AnalysisFramework.VariableIdentifier getVariableIdentifier(SnapshotBase context)
        {
            throw new Exception("No variable identifier can be get for memory entry snapshot.");
        }

        #endregion

        #endregion

        public AliasData CreateAliasToEntry(Snapshot snapshot)
        {
            return getTemporary(snapshot).CreateAliasToEntry(snapshot);
        }

        protected override void writeMemoryWithoutCopy(SnapshotBase context, MemoryEntry value)
        {
            throw new NotImplementedException();
        }
    }
}
