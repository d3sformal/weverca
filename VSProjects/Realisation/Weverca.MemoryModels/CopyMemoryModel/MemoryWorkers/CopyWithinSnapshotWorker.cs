using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    enum CopyAliasState
    {
        NotCopy, OnlyAliases, LinkWithIndex
    }

    class CopyWithinSnapshotWorker
    {
        private Snapshot snapshot;
        private bool isMust;

        public CopyAliasState AliasState { get; set; }

        private HashSet<ObjectValue> objectValues = new HashSet<ObjectValue>();

        public CopyWithinSnapshotWorker(Snapshot snapshot, bool isMust)
        {
            this.snapshot = snapshot;
            this.isMust = isMust;

            AliasState = CopyAliasState.OnlyAliases;
        }

        public void Copy(MemoryIndex sourceIndex, MemoryIndex targetIndex)
        {
            MemoryEntry entry = snapshot.GetMemoryEntry(sourceIndex);

            CopyWithinSnapshotVisitor visitor = new CopyWithinSnapshotVisitor(this, targetIndex);
            visitor.VisitMemoryEntry(entry);

            if (isMust && visitor.GetValuesCount() == 1 && objectValues.Count == 1)
            {
                ObjectValueContainerBuilder objectsValues = snapshot.Data.GetObjects(targetIndex).Builder();

                ObjectValue value = objectValues.First();
                objectsValues.Add(value);
                snapshot.Data.SetObjects(targetIndex, objectsValues.Build());
            }
            else if (objectValues.Count > 0)
            {
                ObjectValueContainerBuilder objectsValues = snapshot.Data.GetObjects(targetIndex).Builder();
                foreach (ObjectValue value in objectValues)
                {
                    objectsValues.Add(value);
                }
                snapshot.Data.SetObjects(targetIndex, objectsValues.Build());
            } 
            
            if (!isMust)
            {
                visitor.AddValue(snapshot.UndefinedValue);
            }
            
            snapshot.CopyAliases(sourceIndex, targetIndex, isMust);

            snapshot.Data.SetMemoryEntry(targetIndex, visitor.GetCopiedEntry());
        }

        internal AssociativeArray ProcessArrayValue(MemoryIndex targetIndex, AssociativeArray value)
        {
            AssociativeArray arrayValue = snapshot.CreateArray(targetIndex, isMust);

            ArrayDescriptor sourceDescriptor = snapshot.Data.GetDescriptor(value);
            ArrayDescriptor targetDescriptor = snapshot.Data.GetDescriptor(arrayValue);

            CopyAliasState oldState = AliasState;

            AliasState = CopyAliasState.OnlyAliases;
            Copy(sourceDescriptor.UnknownIndex, targetDescriptor.UnknownIndex);

            AliasState = CopyAliasState.LinkWithIndex;
            foreach (var index in sourceDescriptor.Indexes)
            {
                MemoryIndex newIndex = snapshot.CreateIndex(index.Key, arrayValue, false, false);
                Copy(index.Value, newIndex);
            }

            AliasState = oldState;
            return arrayValue;
        }

        internal ObjectValue ProcessObjectValue(MemoryIndex targetIndex, ObjectValue value)
        {
            objectValues.Add(value);
            return value;
        }
    }

    class CopyWithinSnapshotVisitor : AbstractValueVisitor
    {
        private CopyWithinSnapshotWorker worker;
        private MemoryIndex index;
        private HashSet<Value> values = new HashSet<Value>();

        public CopyWithinSnapshotVisitor(CopyWithinSnapshotWorker worker, MemoryIndex index)
        {
            this.worker = worker;
            this.index = index;
        }

        public int GetValuesCount()
        {
            return values.Count;
        }

        public void AddValue(Value value)
        {
            values.Add(value);
        }

        public override void VisitValue(Value value)
        {
            values.Add(value);
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            AssociativeArray arrayValue = worker.ProcessArrayValue(index, value);
            values.Add(arrayValue);
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            ObjectValue objectValue = worker.ProcessObjectValue(index, value);
            values.Add(objectValue);
        }

        internal MemoryEntry GetCopiedEntry()
        {
            return new MemoryEntry(values);
        }
    }
}
