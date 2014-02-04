using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

using Weverca.MemoryModels.VirtualReferenceModel.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel.SnapshotEntries
{
    class SnapshotStorageEntry : ReadWriteSnapshotEntryBase
    {
        private readonly VariableKey[] _storages;

        /// <summary>
        /// If snapshot entry belongs to variable identifier, store it for next use
        /// </summary>
        private readonly VariableIdentifier _identifier;

        internal readonly bool ForceStrong;

        internal bool HasDirectIdentifier { get { return _identifier != null && _identifier.IsDirect; } }

        internal SnapshotStorageEntry(VariableIdentifier identifier, bool forceStrong, params VariableKey[] storage)
        {
            _identifier = identifier;
            _storages = storage;
            ForceStrong = forceStrong;
        }

        protected override void writeMemory(SnapshotBase context, MemoryEntry value, bool forceStrongWrite)
        {
            forceStrongWrite |= ForceStrong;

            C(context).Write(_storages, value, forceStrongWrite, false);
        }

        protected override void writeMemoryWithoutCopy(SnapshotBase context, MemoryEntry value)
        {
            C(context).Write(_storages, value, false, true);
        }

        protected override bool isDefined(SnapshotBase context)
        {
            foreach (var storage in _storages)
            {
                if (C(context).IsDefined(storage))
                    return true;
            }
            return false;
        }

        protected override IEnumerable<AliasEntry> aliases(SnapshotBase context)
        {
            return C(context).Aliases(_storages);
        }

        protected override MemoryEntry readMemory(SnapshotBase context)
        {
            var target = C(context);
            switch (_storages.Length)
            {
                case 0:
                    return new MemoryEntry(target.UndefinedValue);
                case 1:
                    return target.ReadValue(_storages[0]);
                default:
                    var entries = new List<MemoryEntry>();
                    foreach (var storage in _storages)
                    {
                        entries.Add(target.ReadValue(storage));
                    }
                    return MemoryEntry.Merge(entries);
            }
        }

        private Snapshot C(SnapshotBase context)
        {
            return context as Snapshot;
        }

        protected override void setAliases(SnapshotBase context, ReadSnapshotEntryBase aliasedEntry)
        {
            var snapshot = C(context);
            var aliasEntries = aliasedEntry.Aliases(context);

            snapshot.SetAliases(_storages, aliasEntries);
        }

        protected override ReadWriteSnapshotEntryBase readIndex(SnapshotBase context, MemberIdentifier index)
        {
            var snapshot = C(context);

            var indexVisitor = new IndexStorageVisitor(this, snapshot, index);

            return indexVisitor.IndexedValue;
        }

        protected override ReadWriteSnapshotEntryBase readField(SnapshotBase context, VariableIdentifier field)
        {
            var snapshot = C(context);
            var fieldVisitor = new FieldStorageVisitor(this, snapshot, field);

            var forceStrong = false;
            if (HasDirectIdentifier && _identifier.DirectName == "this")
                forceStrong = true;

            return new SnapshotStorageEntry(null, forceStrong, fieldVisitor.Storages);
        }

        protected override VariableIdentifier getVariableIdentifier(SnapshotBase context)
        {
            return _identifier;
        }

        protected override IEnumerable<FunctionValue> resolveMethod(SnapshotBase context, QualifiedName methodName)
        {
            var memory = readMemory(context);

            return C(context).ResolveMethod(memory, methodName);
        }

        protected override IEnumerable<VariableIdentifier> iterateFields(SnapshotBase context)
        {
            var memory = readMemory(context);

            return C(context).IterateFields(memory);
        }

        protected override IEnumerable<MemberIdentifier> iterateIndexes(SnapshotBase context)
        {
            var memory = readMemory(context);

            return C(context).IterateIndexes(memory);
        }

        protected override IEnumerable<TypeValue> resolveType(SnapshotBase context)
        {
            var memory = readMemory(context);

            return C(context).ResolveObjectTypes(memory);
        }
    }
}
