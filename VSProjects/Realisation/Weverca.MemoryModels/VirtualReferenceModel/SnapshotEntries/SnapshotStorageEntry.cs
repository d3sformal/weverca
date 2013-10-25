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
    class SnapshotStorageEntry : ReadWriteSnapshotEntryBase
    {
        private readonly VariableKey[] _storages;

        /// <summary>
        /// If snapshot entry belongs to variable identifier, store it for next use
        /// </summary>
        private readonly VariableIdentifier _identifier;

        internal SnapshotStorageEntry(VariableIdentifier identifier, params VariableKey[] storage)
        {
            _identifier = identifier;
            _storages = storage;
        }

        protected override void writeMemory(SnapshotBase context, MemoryEntry value)
        {
            C(context).Write(_storages, value);
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
            foreach (var storage in _storages)
            {
                yield return new ReferenceAliasEntry(storage);
            }
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
            var aliases = from entry in aliasEntries select entry as ReferenceAliasEntry;

            snapshot.SetAliases(_storages, aliases);
        }

        protected override ReadWriteSnapshotEntryBase readIndex(SnapshotBase context, MemberIdentifier index)
        {
            var snapshot = C(context);
            var indexVisitor = new IndexStorageVisitor(this, snapshot, index);

            return new SnapshotStorageEntry(null, indexVisitor.Storages);
        }

        protected override ReadWriteSnapshotEntryBase readField(SnapshotBase context, VariableIdentifier field)
        {
            var snapshot = C(context);
            var fieldVisitor = new FieldStorageVisitor(this, snapshot, field);

            return new SnapshotStorageEntry(null, fieldVisitor.Storages);
        }

        protected override VariableIdentifier getVariableIdentifier(SnapshotBase context)
        {
            return _identifier;
        }
    }
}
