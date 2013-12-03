using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel.SnapshotEntries
{
    internal class SnapshotMemoryEntry : ReadWriteSnapshotEntryBase
    {
        internal readonly MemoryEntry WrappedEntry;

        internal bool ForceStrong { get { return WrappedEntry.Count > 1; } }

        internal SnapshotMemoryEntry(MemoryEntry wrappedEntry)
        {
            WrappedEntry = wrappedEntry;
        }

        protected override void writeMemory(SnapshotBase context, MemoryEntry value, bool forceStrongWrite)
        {
            throw new NotImplementedException();
        }

        protected override void setAliases(SnapshotBase context, ReadSnapshotEntryBase aliasedEntry)
        {
            throw new NotImplementedException();
        }

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
            return WrappedEntry;
        }

        protected override ReadWriteSnapshotEntryBase readIndex(SnapshotBase context,
            MemberIdentifier index)
        {
            // TODO: The method should return SnapshotMemoryEntry of read indices

            var snapshot = toSnapshot(context);
            var allKeys = new List<Memory.VariableKey>();

            foreach (var value in WrappedEntry.PossibleValues)
            {
                var array = value as AssociativeArray;
                if (array != null)
                {
                    var keys = snapshot.IndexStorages(array, index);
                    // TODO: Use snapshot.ReadValue to read values of every variable key
                    allKeys.AddRange(keys);
                }
                else
                {
                    // TODO: If it is not array, what to do?
                    throw new NotImplementedException();
                }
            }

            return new SnapshotStorageEntry(null, ForceStrong, allKeys.ToArray());
        }

        protected override ReadWriteSnapshotEntryBase readField(SnapshotBase context,
            VariableIdentifier field)
        {
            // TODO: The method should return SnapshotMemoryEntry of read fields

            var snapshot = toSnapshot(context);
            var allKeys = new List<Memory.VariableKey>();

            foreach (var value in WrappedEntry.PossibleValues)
            {
                var objectValue = value as ObjectValue;
                if (objectValue != null)
                {
                    var keys = snapshot.FieldStorages(objectValue, field);
                    // TODO: Use snapshot.ReadValue to read values of every variable key
                    allKeys.AddRange(keys);
                }
                else
                {
                    // TODO: If it is not object, what to do?
                    throw new NotImplementedException();
                }
            }

            return new SnapshotStorageEntry(null, ForceStrong, allKeys.ToArray());
        }

        protected override VariableIdentifier getVariableIdentifier(SnapshotBase context)
        {
            throw new NotImplementedException();
        }

        private static Snapshot toSnapshot(SnapshotBase context)
        {
            var snapshot = context as Snapshot;

            if (snapshot != null)
            {
                return snapshot;
            }
            else
            {
                throw new ArgumentException(
                    "Context parameter is not of type Weverca.MemoryModels.CopyMemoryModel.Snapshot");
            }
        }
    }
}
