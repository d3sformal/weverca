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
    class CopyWithinSnapshotWorker
    {
        private Snapshot snapshot;
        private bool isMust;

        public CopyWithinSnapshotWorker(Snapshot snapshot, bool isMust)
        {
            this.snapshot = snapshot;
            this.isMust = isMust;
        }

        public void Copy(MemoryIndex sourceIndex, MemoryIndex targetIndex)
        {
            MemoryEntry entry = snapshot.GetMemoryEntry(sourceIndex);

            CopyWithinSnapshotVisitor visitor = new CopyWithinSnapshotVisitor(this, targetIndex);
            visitor.VisitMemoryEntry(entry);

            if (!isMust)
            {
                visitor.AddValue(snapshot.UndefinedValue);
            }

            snapshot.CopyAliases(sourceIndex, targetIndex, isMust);
            snapshot.CopyInfos(sourceIndex, targetIndex, isMust);

            snapshot.SetMemoryEntry(targetIndex, visitor.GetCopiedEntry());
        }

        internal AssociativeArray ProcessArrayValue(MemoryIndex targetIndex, AssociativeArray value)
        {
            AssociativeArray arrayValue = snapshot.CreateArray(targetIndex, isMust);

            ArrayDescriptor sourceDescriptor = snapshot.GetDescriptor(value);
            ArrayDescriptor targetDescriptor = snapshot.GetDescriptor(arrayValue);

            Copy(sourceDescriptor.UnknownIndex, targetDescriptor.UnknownIndex);

            foreach (var index in sourceDescriptor.Indexes)
            {
                MemoryIndex newIndex = snapshot.CreateIndex(index.Key, arrayValue, false, false);
                Copy(index.Value, newIndex);
            }

            return arrayValue;
        }

        internal ObjectValue ProcessObjectValue(MemoryIndex targetIndex, ObjectValue value)
        {
            ObjectValue objectValue = snapshot.CreateObject(targetIndex, isMust);
            snapshot.SameObjectReference(value, objectValue, isMust);

            ObjectDescriptor sourceDescriptor = snapshot.GetDescriptor(value);
            ObjectDescriptor targetDescriptor = snapshot.GetDescriptor(objectValue);

            Copy(sourceDescriptor.UnknownField, targetDescriptor.UnknownField);

            foreach (var field in sourceDescriptor.Fields)
            {
                MemoryIndex newField = snapshot.CreateField(field.Key, objectValue, false, false);
                Copy(field.Value, newField);
            }

            return objectValue;
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
