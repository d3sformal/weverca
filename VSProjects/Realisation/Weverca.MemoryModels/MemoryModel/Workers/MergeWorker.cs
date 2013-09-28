using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Analysis.Memory;

namespace Weverca.MemoryModels.MemoryModel.Workers
{
    class MergeWorker
    {
        Dictionary<MemoryIndex, MemoryIndex> toBase = new Dictionary<MemoryIndex, MemoryIndex>();
        Dictionary<MemoryIndex, MemoryIndex> fromBase = new Dictionary<MemoryIndex, MemoryIndex>();

        LinkedList<IndexPair> indexQueue = new LinkedList<IndexPair>();
        List<IndexPair> indexList = new List<IndexPair>();

        Snapshot baseSnapshot, mergedSnapshot;

        public MergeWorker(Snapshot baseSnapshot, Snapshot mergedSnapshot)
        {
            this.baseSnapshot = baseSnapshot;
            this.mergedSnapshot = mergedSnapshot;
        }

        public void mergeSnapshots()
        {
            //Process first level
            mergeCollections(baseSnapshot.variables, mergedSnapshot.variables);

            //Merge indexes and other levels
            while (indexQueue.Count > 0)
            {
                processQueue();
            }

            //Process aliases and objects
        }

        private void processQueue()
        {
            IndexPair pair = indexQueue.First.Value;
            indexQueue.RemoveFirst();

            if (pair.BaseIndex == null)
            {
                transferMemoryEntryToBase(pair.MergedIndex);
            }
            else if (pair.MergedIndex == null)
            {
                addUndefined(pair.BaseIndex);
            }
            else
            {
                mergeEntries(pair.BaseIndex, pair.MergedIndex);
            }
        }

        #region Move to base

        /// <summary>
        /// Item is in the merged and not in the base - just copy to base
        /// </summary>
        /// <param name="memoryIndex"></param>
        private void transferMemoryEntryToBase(MemoryIndex memoryIndex)
        {
            MemoryEntry entry = mergedSnapshot.memoryEntries[memoryIndex];
            FindValueVisitor visitor = new FindValueVisitor();
            visitor.VisitMemoryEntry(entry);

            //Inserts undefined value
            if (!visitor.HasUndefined)
            {
                entry = extendMemoryEntry(entry, baseSnapshot.UndefinedValue);
            }

            baseSnapshot.memoryEntries[memoryIndex] = entry;

            if (visitor.ArrayValue != null)
            {
                moveArrayToBase(visitor.ArrayValue);
            }

            if (visitor.ObjectValue != null)
            {
                moveObjectToBase(visitor.ObjectValue);
            }
        }

        /// <summary>
        /// Go to the object level - just copy and enqueue indexes
        /// </summary>
        /// <param name="objectValue"></param>
        private void moveObjectToBase(ObjectValue objectValue)
        {
            ObjectDescriptor descriptor = mergedSnapshot.objects[objectValue];
            baseSnapshot.objects[objectValue] = descriptor;

            foreach (var item in descriptor.Fields)
            {
                enqueMovingToBase(item.Value);
            }
        }

        /// <summary>
        /// Go to the array level - just copy and enqueue indexes
        /// </summary>
        /// <param name="arayValue"></param>
        private void moveArrayToBase(AssociativeArray arrayValue)
        {
            ArrayDescriptor descriptor = mergedSnapshot.arrays[arrayValue];
            baseSnapshot.arrays[arrayValue] = descriptor;

            foreach (var item in descriptor.Indexes)
            {
                enqueMovingToBase(item.Value);
            }
        }

        #endregion

        #region Add undefined

        /// <summary>
        /// Item is in base and not in the other
        /// </summary>
        /// <param name="memoryIndex"></param>
        private void addUndefined(MemoryIndex memoryIndex)
        {
            MemoryEntry entry = mergedSnapshot.memoryEntries[memoryIndex];
            FindValueVisitor visitor = new FindValueVisitor();
            visitor.VisitMemoryEntry(entry);

            //Inserts undefined value
            if (!visitor.HasUndefined)
            {
                entry = extendMemoryEntry(entry, baseSnapshot.UndefinedValue);
            }

            baseSnapshot.memoryEntries[memoryIndex] = entry;

            if (visitor.ArrayValue != null)
            {
                addUndefinedArray(visitor.ArrayValue);
            }
            if (visitor.ObjectValue != null)
            {
                addUndefinedObject(visitor.ObjectValue);
            }
        }

        /// <summary>
        /// Go to the object level - just enque undefined
        /// </summary>
        /// <param name="objectValue"></param>
        private void addUndefinedObject(ObjectValue objectValue)
        {
            ObjectDescriptor descriptor = mergedSnapshot.objects[objectValue];
            foreach (var item in descriptor.Fields)
            {
                enqueUndefined(item.Value);
            }
        }

        /// <summary>
        /// Go to the array level - just enque undefined
        /// </summary>
        /// <param name="associativeArray"></param>
        private void addUndefinedArray(AssociativeArray arrayValue)
        {
            ArrayDescriptor descriptor = mergedSnapshot.arrays[arrayValue];
            foreach (var item in descriptor.Indexes)
            {
                enqueUndefined(item.Value);
            }
        }

        #endregion

        #region Merge entries

        private void mergeEntries(MemoryIndex baseIndex, MemoryIndex mergedIndex)
        {
            MergeEntriesVisitor visitor = new MergeEntriesVisitor();

            MemoryEntry baseMemoryEntry = baseSnapshot.memoryEntries[baseIndex];
            visitor.VisitMemoryEntry(baseMemoryEntry);
            AssociativeArray baseArrayValue = visitor.ArrayValue;
            ObjectValue baseObjectValue = visitor.ObjectValue;

            MemoryEntry mergedMemoryEntry = mergedSnapshot.memoryEntries[baseIndex];
            visitor.VisitMemoryEntry(mergedMemoryEntry);
            AssociativeArray mergedArrayValue = visitor.ArrayValue;
            ObjectValue mergedObjectValue = visitor.ObjectValue;

            if (visitor.HasUndefined)
            {
                visitor.Values.Add(baseSnapshot.UndefinedValue);
            }

            mergeArrays(visitor.Values, baseArrayValue, mergedArrayValue);
            mergeObjects(visitor.Values, baseObjectValue, mergedObjectValue);
        }

        private void mergeObjects(List<Value> values, ObjectValue baseObjectValue, ObjectValue mergedObjectValue)
        {
            if (baseObjectValue != null && mergedObjectValue != null)
            {
                values.Add(baseObjectValue);
                mergeObjects(baseObjectValue, mergedObjectValue);
            }
            else if (baseObjectValue != null)
            {
                addUndefinedObject(baseObjectValue);
                values.Add(baseObjectValue);
            }
            else if (mergedObjectValue != null)
            {
                moveObjectToBase(mergedObjectValue);
                values.Add(mergedObjectValue);
            }
        }

        private void mergeObjects(ObjectValue baseObjectValue, ObjectValue mergedObjectValue)
        {
            ObjectDescriptor baseDescriptor = baseSnapshot.objects[baseObjectValue];
            ObjectDescriptor mergedDescriptor = baseSnapshot.objects[mergedObjectValue];

            ObjectDescriptorBuilder baseDescriptorBuilder = baseDescriptor.Builder();

            mergeCollections(baseDescriptorBuilder.Fields, mergedDescriptor.Fields);
            //TODO - vyresit ParentVariable

            ObjectDescriptor newDescriptor = baseDescriptorBuilder.Build();

            baseSnapshot.objects[baseObjectValue] = newDescriptor;
        }

        private void mergeArrays(List<Value> values, AssociativeArray baseArrayValue, AssociativeArray mergedArrayValue)
        {
            if (baseArrayValue != null && mergedArrayValue != null)
            {
                values.Add(baseArrayValue);
                mergeArrays(baseArrayValue, mergedArrayValue);
            }
            else if (baseArrayValue != null)
            {
                addUndefinedArray(baseArrayValue);
                values.Add(baseArrayValue);
            }
            else if (mergedArrayValue != null)
            {
                moveArrayToBase(mergedArrayValue);
                values.Add(mergedArrayValue);
            }
        }

        private void mergeArrays(AssociativeArray baseArrayValue, AssociativeArray mergedArrayValue)
        {
            ArrayDescriptor mergedDescriptor = baseSnapshot.arrays[mergedArrayValue];
            ArrayDescriptor baseDescriptor = baseSnapshot.arrays[baseArrayValue];

            ArrayDescriptorBuilder baseDescriptorBuilder = baseDescriptor.Builder();

            mergeCollections(baseDescriptorBuilder.Indexes, mergedDescriptor.Indexes);
            //TODO - vyresit ParentVariable

            ArrayDescriptor newDescriptor = baseDescriptorBuilder.Build();

            baseSnapshot.arrays[baseArrayValue] = newDescriptor;
        }

        #endregion

        private MemoryEntry extendMemoryEntry(MemoryEntry oldEntry, Value extendValue)
        {
            List<Value> values = new List<Value>(oldEntry.PossibleValues);
            values.Add(baseSnapshot.UndefinedValue);

            return new MemoryEntry(values);
        }

        /// <summary>
        /// Collects memory indexes of the collection
        /// </summary>
        public void mergeCollections<T>(IDictionary<T, MemoryIndex> baseColection, IReadOnlyDictionary<T, MemoryIndex> mergedColection)
        {
            //For all in base
            foreach (var item in baseColection)
            {
                MemoryIndex baseIndex = item.Value;
                MemoryIndex mergedIndex;
                //Looks if it is included in the oposite variables
                if (mergedColection.TryGetValue(item.Key, out mergedIndex))
                {
                    toBase[mergedIndex] = baseIndex;
                    fromBase[baseIndex] = mergedIndex;

                    addPair(new IndexPair(baseIndex, mergedIndex));
                }
                //Or if there is not such variable there
                else
                {
                    enqueUndefined(baseIndex);
                }
            }

            //Finds all variables which are in the other snapshot and not in base
            foreach (var item in mergedColection)
            {
                MemoryIndex baseIndex;
                MemoryIndex mergedIndex = item.Value;
                if (!baseColection.TryGetValue(item.Key, out baseIndex))
                {
                    enqueMovingToBase(mergedIndex);
                    baseColection[item.Key] = mergedIndex;
                }
            }
        }

        private void addPair(IndexPair pair)
        {
            indexQueue.AddLast(pair);
            indexList.Add(pair); 
        }

        private void enqueMovingToBase(MemoryIndex mergedIndex)
        {
            toBase[mergedIndex] = null;
            addPair(new IndexPair(null, mergedIndex));
        }

        private void enqueUndefined(MemoryIndex baseIndex)
        {
            fromBase[baseIndex] = null;
            addPair(new IndexPair(baseIndex, null));
        }
    }

    class IndexPair
    {
        public MemoryIndex BaseIndex, MergedIndex;

        public IndexPair(MemoryIndex baseIndex, MemoryIndex mergedIndex)
        {
            // TODO: Complete member initialization
            this.BaseIndex = baseIndex;
            this.MergedIndex = mergedIndex;
        }
    }

    class ObjectPair
    {
        public ObjectValue BaseObject, MergedObject;

        public ObjectPair(ObjectValue baseIndex, ObjectValue mergedIndex)
        {
            // TODO: Complete member initialization
            this.BaseObject = baseIndex;
            this.MergedObject = mergedIndex;
        }
    }

    class FindValueVisitor : AbstractValueVisitor
    {
        public bool HasUndefined { get; private set; }
        public AssociativeArray ArrayValue { get; private set; }
        public ObjectValue ObjectValue { get; private set; }

        public FindValueVisitor() 
        {
            HasUndefined = false;
        }

        public override void VisitValue(Value value)
        {
        }

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            HasUndefined = true;
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            ArrayValue = value;
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            ObjectValue = value;
        }
    }

    class MergeEntriesVisitor : AbstractValueVisitor
    {
        public bool HasUndefined { get; private set; }
        public AssociativeArray ArrayValue { get; private set; }
        public ObjectValue ObjectValue { get; private set; }

        public List<Value> Values { get; private set; }

        public MergeEntriesVisitor() 
        {
            HasUndefined = false;
            Values = new List<Value>();
        }

        public override void VisitValue(Value value)
        {
            Values.Add(value);
        }

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            HasUndefined = true;
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            ArrayValue = value;
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            ObjectValue = value;
        }
    }
}
