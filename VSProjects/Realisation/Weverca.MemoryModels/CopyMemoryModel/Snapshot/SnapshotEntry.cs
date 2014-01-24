using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class SnapshotEntry : ReadWriteSnapshotEntryBase, ICopyModelSnapshotEntry
    {
        MemoryPath path;
        private AnalysisFramework.VariableIdentifier variableId;

        internal static ReadWriteSnapshotEntryBase CreateVariableEntry(AnalysisFramework.VariableIdentifier variable, GlobalContext global)
        {
            MemoryPath path;
            if (variable.IsUnknown)
            {
                path = MemoryPath.MakePathAnyVariable(global);
            }
            else
            {
                var names = from name in variable.PossibleNames select name.Value;
                path = MemoryPath.MakePathVariable(names, global);
            }

            return new SnapshotEntry(path, variable);
        }

        internal static ReadWriteSnapshotEntryBase CreateControlEntry(VariableName name, GlobalContext global)
        {
            MemoryPath path = MemoryPath.MakePathControl(new string[]{name.ToString()}, global);
            return new SnapshotEntry(path);
        }

        public SnapshotEntry()
            : this(MemoryPath.MakePathAnyVariable(GlobalContext.LocalOnly))
        {
        }

        public SnapshotEntry(MemoryPath path)
        {
            this.path = path;

            this.variableId = null;
        }

        private SnapshotEntry(MemoryPath path, AnalysisFramework.VariableIdentifier variableId)
        {
            this.path = path;
            this.variableId = variableId;
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

            return new SnapshotEntry(newPath, variableId);
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

            return new SnapshotEntry(newPath, variableId);
        }

        #endregion

        #region Update

        protected override void writeMemory(SnapshotBase context, MemoryEntry value, bool forceStrongWrite)
        {
            Snapshot snapshot = ToSnapshot(context);

            TemporaryIndex temporaryIndex = snapshot.CreateTemporary();
            MergeWithinSnapshotWorker mergeWorker = new MergeWithinSnapshotWorker(snapshot);
            mergeWorker.MergeMemoryEntry(temporaryIndex, value);

            AssignCollector collector = new AssignCollector(snapshot);
            collector.ProcessPath(path);

            if (forceStrongWrite)
            {
                collector.SetAllToMust();
            }

            AssignWorker worker = new AssignWorker(snapshot);
            worker.Assign(collector, temporaryIndex);

            snapshot.ReleaseTemporary(temporaryIndex);
        }

        protected override void writeMemoryWithoutCopy(SnapshotBase context, MemoryEntry value)
        {
            Snapshot snapshot = ToSnapshot(context);

            AssignCollector collector = new AssignCollector(snapshot);
            collector.ProcessPath(path);

            AssignWithoutCopyWorker worker = new AssignWithoutCopyWorker(snapshot);
            worker.Assign(collector, value);
        }

        protected override void setAliases(SnapshotBase context, ReadSnapshotEntryBase aliasedEntry)
        {
            Snapshot snapshot = ToSnapshot(context);

            ICopyModelSnapshotEntry entry = ToEntry(aliasedEntry);
            AliasData data = entry.CreateAliasToEntry(snapshot);

            AssignCollector collector = new AssignCollector(snapshot);
            collector.AliasesProcessing = AliasesProcessing.BeforeCollecting;
            collector.ProcessPath(path);

            AssignAliasWorker worker = new AssignAliasWorker(snapshot);
            worker.AssignAlias(collector, data);

            data.Release(snapshot);
        }

        #endregion

        #region Read

        protected override bool isDefined(SnapshotBase context)
        {
            Snapshot snapshot = ToSnapshot(context);

            ReadCollector collector = new ReadCollector(snapshot);
            collector.ProcessPath(path);

            return collector.IsDefined;
        }

        //TODO - zeptat se Mirka k cemu to je
        protected override IEnumerable<AliasEntry> aliases(SnapshotBase context)
        {
            throw new NotImplementedException();
        }

        protected override MemoryEntry readMemory(SnapshotBase context)
        {
            Snapshot snapshot = ToSnapshot(context);

            ReadCollector collector = new ReadCollector(snapshot);
            collector.ProcessPath(path);

            ReadWorker worker = new ReadWorker(snapshot);
            return worker.ReadValue(collector);
        }

        protected override AnalysisFramework.VariableIdentifier getVariableIdentifier(SnapshotBase context)
        {
            if (variableId != null)
            {
                return variableId;
            }
            else
            {
                throw new Exception("No variable identifier set for this object.");
            }
        }

        #endregion

        #endregion

        public static Snapshot ToSnapshot(ISnapshotReadonly context)
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

        public static ICopyModelSnapshotEntry ToEntry(ReadSnapshotEntryBase entry)
        {
            ICopyModelSnapshotEntry copyEntry = entry as ICopyModelSnapshotEntry;

            if (copyEntry != null)
            {
                return copyEntry;
            }
            else
            {
                throw new ArgumentException("Entry parametter is not of type Weverca.MemoryModels.CopyMemoryModel.ICopyModelSnapshotEntry");
            }
        }

        public override string ToString()
        {
            return path.ToString();
        }

        public AliasData CreateAliasToEntry(Snapshot snapshot)
        {
            //Collect alias indexes
            AssignCollector indexesCollector = new AssignCollector(snapshot);
            indexesCollector.ProcessPath(path);

            //Memory locations where to get data from
            ReadCollector valueCollector = new ReadCollector(snapshot);
            valueCollector.ProcessPath(path);

            //Get data from locations
            ReadWorker worker = new ReadWorker(snapshot);
            MemoryEntry value = worker.ReadValue(valueCollector);

            //Makes deep copy of data to prevent changes after assign alias
            TemporaryIndex temporaryIndex = snapshot.CreateTemporary();
            MergeWithinSnapshotWorker mergeWorker = new MergeWithinSnapshotWorker(snapshot);
            mergeWorker.MergeMemoryEntry(temporaryIndex, value);

            AliasData data = new AliasData(indexesCollector.MustIndexes, indexesCollector.MayIndexes, temporaryIndex);
            data.TemporaryIndexToRealease(temporaryIndex);

            return data;
        }


        protected override IEnumerable<FunctionValue> resolveMethod(SnapshotBase context, QualifiedName methodName)
        {
            throw new NotImplementedException();
        }
    }
}
