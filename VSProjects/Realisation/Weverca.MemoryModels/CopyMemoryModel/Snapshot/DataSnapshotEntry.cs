using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

using Weverca.AnalysisFramework;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class DataSnapshotEntry : ReadWriteSnapshotEntryBase, ICopyModelSnapshotEntry
    {
        MemoryEntry dataEntry;
        MemoryEntry infoEntry;
        SnapshotEntry temporaryLocation = null;
        TemporaryIndex temporaryIndex = null;
        
        public DataSnapshotEntry(Snapshot snapshot, MemoryEntry entry)
        {
            switch (snapshot.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    dataEntry = entry;
                    infoEntry = new MemoryEntry();
                    break;

                case SnapshotMode.InfoLevel:
                    dataEntry = new MemoryEntry();
                    infoEntry = entry;
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + snapshot.CurrentMode);
            }
        }

        public override string ToString()
        {
            if (temporaryIndex != null)
            {
                return "temporary data: " + temporaryIndex.ToString();
            }
            else
            {
                return "temporary data: " + dataEntry.ToString();
            }
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
            Logger.append(context, "read index - " + this.ToString());

            return getTemporary(context).ReadIndex(context, index);
        }

        protected override ReadWriteSnapshotEntryBase readField(SnapshotBase context, AnalysisFramework.VariableIdentifier field)
        {
            Logger.append(context, "read index - " + this.ToString());

            return getTemporary(context).ReadField(context, field);
        }

        #endregion

        #region Update

        protected override void writeMemory(SnapshotBase context, MemoryEntry value, bool forceStrongWrite)
        {
            Logger.append(context, "write memory - " + this.ToString());
            Snapshot snapshot = SnapshotEntry.ToSnapshot(context);

            switch (snapshot.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    getTemporary(context).WriteMemory(context, value, forceStrongWrite);
                    break;

                case SnapshotMode.InfoLevel:
                    if (isTemporarySet(context))
                    {
                        getTemporary(context).WriteMemory(context, value, forceStrongWrite);
                    }
                    else
                    {
                        infoEntry = value;
                    }
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + snapshot.CurrentMode);
            }
        }

        protected override void setAliases(SnapshotBase context, ReadSnapshotEntryBase aliasedEntry)
        {
            Logger.append(context, "set aliases - " + this.ToString());

            getTemporary(context).SetAliases(context, aliasedEntry);
        }

        #endregion

        #region Read

        protected override bool isDefined(SnapshotBase context)
        {
            Logger.append(context, "is defined - " + this.ToString());

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
            Logger.append(context, "aliases - " + this.ToString());

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
            Logger.append(context, "read memory - " + this.ToString());

            if (isTemporarySet(context))
            {
                return temporaryLocation.ReadMemory(context);
            }
            else
            {
                Snapshot snapshot = SnapshotEntry.ToSnapshot(context);
                switch (snapshot.CurrentMode)
                {
                    case SnapshotMode.MemoryLevel:
                        return dataEntry;

                    case SnapshotMode.InfoLevel:
                        return infoEntry;

                    default:
                        throw new NotSupportedException("Current mode: " + snapshot.CurrentMode);
                }
            }
        }

        protected override IEnumerable<FunctionValue> resolveMethod(SnapshotBase context, PHP.Core.QualifiedName methodName)
        {
            Logger.append(context, "resolve method - " + this.ToString() + " method: " + methodName);

            if (isTemporarySet(context))
            {
                return temporaryLocation.ResolveMethod(context, methodName);
            }
            else
            {
                Snapshot snapshot = SnapshotEntry.ToSnapshot(context);
                return snapshot.resolveMethod(dataEntry, methodName);
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
            if (temporaryLocation == null)
            {
                dataEntry = value;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        protected override IEnumerable<VariableIdentifier> iterateFields(SnapshotBase context)
        {
            return SnapshotEntryHelper.IterateFields(context, this);
        }

        protected override IEnumerable<MemberIdentifier> iterateIndexes(SnapshotBase context)
        {
            return SnapshotEntryHelper.IterateIndexes(context, this);
        }

        protected override IEnumerable<TypeValue> resolveType(SnapshotBase context)
        {
            return SnapshotEntryHelper.ResolveType(context, this);
        }

        public MemoryEntry ReadMemory(Snapshot snapshot)
        {
            return this.readMemory(snapshot);
        }
    }
}
