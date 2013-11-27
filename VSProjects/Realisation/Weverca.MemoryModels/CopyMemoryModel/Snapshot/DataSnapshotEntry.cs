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

        public DataSnapshotEntry(MemoryEntry entry)
        {
            dataEntry = entry;
        }

        #region ReadWriteSnapshotEntryBase Implementation

        #region Navigation

        protected override ReadWriteSnapshotEntryBase readIndex(SnapshotBase context, MemberIdentifier index)
        {
            throw new NotImplementedException();
        }

        protected override ReadWriteSnapshotEntryBase readField(SnapshotBase context, AnalysisFramework.VariableIdentifier field)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Update

        protected override void writeMemory(SnapshotBase context, MemoryEntry value)
        {
            throw new NotImplementedException();
        }

        protected override void setAliases(SnapshotBase context, ReadSnapshotEntryBase aliasedEntry)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Read

        protected override bool isDefined(SnapshotBase context)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<AliasEntry> aliases(SnapshotBase context)
        {
            throw new NotImplementedException();
        }

        protected override MemoryEntry readMemory(SnapshotBase context)
        {
            return dataEntry;
        }

        protected override AnalysisFramework.VariableIdentifier getVariableIdentifier(SnapshotBase context)
        {
            throw new Exception("No variable identifier can be get for memory entry snapshot.");
        }

        #endregion

        #endregion

        public AliasData CreateAliasToEntry(Snapshot snapshot)
        {
            throw new NotImplementedException();
        }
    }
}
