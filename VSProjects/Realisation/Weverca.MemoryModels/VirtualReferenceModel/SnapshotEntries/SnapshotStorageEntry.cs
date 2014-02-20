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
    /// <summary>
    /// Implementation of <see cref="ReadWriteSnapshotEntryBase"/> providing access to snapshot services
    /// </summary>
    class SnapshotStorageEntry : ReadWriteSnapshotEntryBase
    {
        /// <summary>
        /// If snapshot entry belongs to variable identifier, store it for next use
        /// </summary>
        private readonly VariableIdentifier _identifier;

        /// <summary>
        /// Has not be modified from outside
        /// </summary>
        internal readonly VariableKeyBase[] Storages;

        /// <summary>
        /// Determine that writes has to be forced strong
        /// </summary>
        internal readonly bool ForceStrong;

        /// <summary>
        /// Determine that non-direct identifier is contained
        /// </summary>
        internal bool HasDirectIdentifier { get { return _identifier != null && _identifier.IsDirect; } }

        internal SnapshotStorageEntry(VariableIdentifier identifier, bool forceStrong, params VariableKeyBase[] storage)
        {
            _identifier = identifier;
            Storages = storage;
            ForceStrong = forceStrong;
        }

        #region ReadWriteSnapshotEntryBase overrides

        /// <inheritdoc />
        protected override void writeMemory(SnapshotBase context, MemoryEntry value, bool forceStrongWrite)
        {
            forceStrongWrite |= ForceStrong;

            C(context).Write(Storages, value, forceStrongWrite, false);
        }

        /// <inheritdoc />
        protected override void writeMemoryWithoutCopy(SnapshotBase context, MemoryEntry value)
        {
            C(context).Write(Storages, value, false, true);
        }

        /// <inheritdoc />
        protected override bool isDefined(SnapshotBase context)
        {
            foreach (var storage in Storages)
            {
                if (C(context).IsDefined(storage))
                    return true;
            }
            return false;
        }

        /// <inheritdoc />
        protected override IEnumerable<AliasEntry> aliases(SnapshotBase context)
        {
            return C(context).Aliases(Storages);
        }

        /// <inheritdoc />
        protected override MemoryEntry readMemory(SnapshotBase context)
        {
            var target = C(context);
            switch (Storages.Length)
            {
                case 0:
                    return new MemoryEntry(target.UndefinedValue);
                case 1:
                    return target.ReadValue(Storages[0]);
                default:
                    var entries = new List<MemoryEntry>();
                    foreach (var storage in Storages)
                    {
                        entries.Add(target.ReadValue(storage));
                    }
                    return MemoryEntry.Merge(entries);
            }
        }


        /// <inheritdoc />
        protected override void setAliases(SnapshotBase context, ReadSnapshotEntryBase aliasedEntry)
        {
            var snapshot = C(context);
            var aliasEntries = aliasedEntry.Aliases(context);

            snapshot.SetAliases(Storages, aliasEntries);
        }

        /// <inheritdoc />
        protected override ReadWriteSnapshotEntryBase readIndex(SnapshotBase context, MemberIdentifier index)
        {
            var snapshot = C(context);

            var indexVisitor = new IndexStorageVisitor(this, snapshot, index);

            return indexVisitor.IndexedValue;
        }

        /// <inheritdoc />
        protected override ReadWriteSnapshotEntryBase readField(SnapshotBase context, VariableIdentifier field)
        {
            var snapshot = C(context);
            var fieldVisitor = new FieldStorageVisitor(this, snapshot, field);

            var forceStrong = false;
            if (HasDirectIdentifier && _identifier.DirectName == "this")
                forceStrong = true;

            return new SnapshotStorageEntry(null, forceStrong, fieldVisitor.Storages);
        }

        /// <inheritdoc />
        protected override VariableIdentifier getVariableIdentifier(SnapshotBase context)
        {
            return _identifier;
        }

        /// <inheritdoc />
        protected override IEnumerable<FunctionValue> resolveMethod(SnapshotBase context, QualifiedName methodName)
        {
            var memory = readMemory(context);

            return C(context).ResolveMethod(memory, methodName);
        }

        /// <inheritdoc />
        protected override IEnumerable<VariableIdentifier> iterateFields(SnapshotBase context)
        {
            var memory = readMemory(context);

            return C(context).IterateFields(memory);
        }

        /// <inheritdoc />
        protected override IEnumerable<MemberIdentifier> iterateIndexes(SnapshotBase context)
        {
            var memory = readMemory(context);

            return C(context).IterateIndexes(memory);
        }

        /// <inheritdoc />
        protected override IEnumerable<TypeValue> resolveType(SnapshotBase context)
        {
            var memory = readMemory(context);

            return C(context).ResolveObjectTypes(memory);
        }

        #endregion

        #region Private helpers

        private Snapshot C(SnapshotBase context)
        {
            return context as Snapshot;
        }

        #endregion
    }
}
