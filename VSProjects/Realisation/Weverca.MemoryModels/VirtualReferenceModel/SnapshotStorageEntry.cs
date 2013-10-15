using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel
{
    class SnapshotStorageEntry:ReadWriteSnapshotEntryBase
    {
        private readonly VariableInfo[] _storages;

        internal SnapshotStorageEntry(params VariableInfo[] storage)
        {
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
            return _storages.Length > 0;
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
            }
            throw new NotImplementedException();
        }

        private Snapshot C(SnapshotBase context)
        {
            return context as Snapshot;
        }

        protected override void setAliases(SnapshotBase context, ReadSnapshotEntryBase aliasedEntry)
        {
            throw new NotImplementedException();
        }

        protected override ReadWriteSnapshotEntryBase readIndex(SnapshotBase context, MemberIdentifier index)
        {
            throw new NotImplementedException();
        }

        protected override ReadWriteSnapshotEntryBase readField(SnapshotBase context, VariableIdentifier field)
        {
            throw new NotImplementedException();
        }

        protected override VariableIdentifier getVariableIdentifier(SnapshotBase context)
        {
            throw new NotImplementedException();
        }
    }
}
