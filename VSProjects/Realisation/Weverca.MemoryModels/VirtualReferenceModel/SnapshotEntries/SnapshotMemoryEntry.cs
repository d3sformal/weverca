using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel.SnapshotEntries
{
    internal class SnapshotMemoryEntry : ReadWriteSnapshotEntryBase
    {
        private MemoryEntry WrappedEntry;
        private MemoryEntry WrappedEntryInfoLevel = null;
        private static MemoryEntry UndefinedEntry = null;

        internal bool ForceStrong { get { return WrappedEntry.Count > 1; } }

        internal SnapshotMemoryEntry(SnapshotBase context, MemoryEntry wrappedEntry)
        {
            writeMemory(context, wrappedEntry, true);
        }

        protected override void writeMemory(SnapshotBase context, MemoryEntry value, bool forceStrongWrite)
        {
            switch (context.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    WrappedEntry = value;
                    break;
                case SnapshotMode.InfoLevel:
                    WrappedEntryInfoLevel = value;
                    break;
                default:
                    throw notSupportedMode(context.CurrentMode);
            }
        }

        protected override void writeMemoryWithoutCopy(SnapshotBase context, MemoryEntry value)
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

        protected override IEnumerable<FunctionValue> resolveMethod(SnapshotBase context, QualifiedName methodName)
        {
            return C(context).ResolveMethod(WrappedEntry, methodName);
        }

        protected override MemoryEntry readMemory(SnapshotBase context)
        {
            switch (context.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    return WrappedEntry;
                case SnapshotMode.InfoLevel:
                    if (WrappedEntryInfoLevel != null) return WrappedEntryInfoLevel;
                    return getUndefinedMemoryEntry(context);
                default:
                    throw notSupportedMode(context.CurrentMode);
            }
        }

        protected override ReadWriteSnapshotEntryBase readIndex(SnapshotBase context,
            MemberIdentifier index)
        {
            // TODO: The method should return SnapshotMemoryEntry of read indices

            var snapshot = C(context);
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

            var snapshot = C(context);
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

        protected override IEnumerable<VariableIdentifier> iterateFields(SnapshotBase context)
        {
            return C(context).IterateFields(WrappedEntry);
        }

        protected override IEnumerable<MemberIdentifier> iterateIndexes(SnapshotBase context)
        {
            return C(context).IterateIndexes(WrappedEntry);
        }

        private MemoryEntry getUndefinedMemoryEntry(SnapshotBase snapshot)
        {
            if (UndefinedEntry == null) UndefinedEntry = new MemoryEntry(snapshot.UndefinedValue);
            return UndefinedEntry;
        }

        private Exception notSupportedMode(SnapshotMode currentMode)
        {
            return new NotSupportedException("Current mode: " + currentMode);
        }

        private static Snapshot C(SnapshotBase context)
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
