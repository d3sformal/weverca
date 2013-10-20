using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class SnapshotEntry : ReadWriteSnapshotEntryBase
    {
        MemoryPath path;

        public SnapshotEntry()
            : this(MemoryPath.MakePathAnyVariable())
        {
        }

        public SnapshotEntry(MemoryPath path)
        {
            this.path = path;
        }

        public SnapshotEntry(AnalysisFramework.VariableIdentifier variable)
        {
            if (variable.IsUnknown)
            {
                path = MemoryPath.MakePathAnyVariable();
            }
            else
            {
                var names = from name in variable.PossibleNames select name.Value;
                path = MemoryPath.MakePathVariable(names);
            }
        }

        #region ReadWriteSnapshotEntryBase Implementation

        #region Navigation

        protected override ReadWriteSnapshotEntryBase readIndex(SnapshotBase context, MemberIdentifier index)
        {
            MemoryPath newPath;
            if (index.IsUnknown)
            {
                newPath = MemoryPath.MakePathAnyIndex(path);
            }
            else
            {
                newPath = MemoryPath.MakePathIndex(path, index.PossibleNames);
            }

            return new SnapshotEntry(newPath);
        }

        protected override ReadWriteSnapshotEntryBase readField(SnapshotBase context, AnalysisFramework.VariableIdentifier field)
        {
            MemoryPath newPath;
            if (field.IsUnknown)
            {
                newPath = MemoryPath.MakePathAnyField(path);
            }
            else
            {
                var names = from name in field.PossibleNames select name.Value;
                newPath = MemoryPath.MakePathField(path, names);
            }

            return new SnapshotEntry(newPath);
        }

        #endregion

        #region Update

        protected override void writeMemory(SnapshotBase context, MemoryEntry value)
        {
            Snapshot snapshot = toSnapshot(context);

            AssignCollector collector = new AssignCollector();
            collector.ProcessPath(snapshot, path);

            AssignWorker worker = new AssignWorker(snapshot);
            worker.Assign(collector, value);
        }

        protected override void setAliases(SnapshotBase context, ReadSnapshotEntryBase aliasedEntry)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Read

        protected override bool isDefined(SnapshotBase context)
        {
            Snapshot snapshot = toSnapshot(context);

            ReadCollector collector = new ReadCollector();
            collector.ProcessPath(snapshot, path);

            return collector.IsDefined;
        }

        protected override IEnumerable<AliasEntry> aliases(SnapshotBase context)
        {
            throw new NotImplementedException();
        }

        protected override MemoryEntry readMemory(SnapshotBase context)
        {
            Snapshot snapshot = toSnapshot(context);

            ReadCollector collector = new ReadCollector();
            collector.ProcessPath(snapshot, path);

            ReadWorker worker = new ReadWorker(snapshot);
            return worker.ReadValue(collector);
        }

        protected override AnalysisFramework.VariableIdentifier getVariableIdentifier(SnapshotBase context)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        private static Snapshot toSnapshot(SnapshotBase context)
        {
            Snapshot snapshot = context as Snapshot;

            if (snapshot != null)
            {
                return snapshot;
            }
            else
            {
                throw new ArgumentException("Context parametter is not of type Weverca.MemoryModels.CopyMemoryModel.Snapshot");
            }
        }
    }
}
