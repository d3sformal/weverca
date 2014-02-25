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
    /// <summary>
    /// Determines how to process aliased links between copied locations.
    /// </summary>
    enum CopyAliasState
    {
        /// <summary>
        /// Do not copy any alias link
        /// </summary>
        NotCopy,

        /// <summary>
        /// Copy alias definitions to new locations (new location is added into aliased group)
        /// </summary>
        OnlyAliases,

        /// <summary>
        /// Copy alias definitions and creates new alias links between indexes of array where some alias exists.
        /// This is standard PHP behavior on copying array with aliased indexes.
        /// </summary>
        LinkWithIndex
    }

    /// <summary>
    /// Provides deep copy between two memory locations within the same memory snapshot.
    /// </summary>
    class CopyWithinSnapshotWorker
    {
        private Snapshot snapshot;
        private bool isMust;

        /// <summary>
        /// Gets or sets the state of the alias processing.
        /// </summary>
        /// <value>
        /// The state of the alias.
        /// </value>
        public CopyAliasState AliasState { get; set; }

        private HashSet<ObjectValue> objectValues = new HashSet<ObjectValue>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyWithinSnapshotWorker"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="isMust">if set to <c>true</c> [is must].</param>
        public CopyWithinSnapshotWorker(Snapshot snapshot, bool isMust)
        {
            this.snapshot = snapshot;
            this.isMust = isMust;

            AliasState = CopyAliasState.OnlyAliases;
        }

        /// <summary>
        /// Deeply copies the specified source index into target index.
        /// </summary>
        /// <param name="sourceIndex">Index of the source.</param>
        /// <param name="targetIndex">Index of the target.</param>
        public void Copy(MemoryIndex sourceIndex, MemoryIndex targetIndex)
        {
            if (!sourceIndex.IsPrefixOf(targetIndex) && !targetIndex.IsPrefixOf(sourceIndex))
            {

                MemoryEntry entry = snapshot.Structure.GetMemoryEntry(sourceIndex);

                CopyWithinSnapshotVisitor visitor = new CopyWithinSnapshotVisitor(this, targetIndex);
                visitor.VisitMemoryEntry(entry);

                if (isMust && visitor.GetValuesCount() == 1 && objectValues.Count == 1)
                {
                    ObjectValueContainerBuilder objectsValues = snapshot.Structure.GetObjects(targetIndex).Builder();

                    ObjectValue value = objectValues.First();
                    objectsValues.Add(value);
                    snapshot.Structure.SetObjects(targetIndex, objectsValues.Build());
                }
                else if (objectValues.Count > 0)
                {
                    ObjectValueContainerBuilder objectsValues = snapshot.Structure.GetObjects(targetIndex).Builder();
                    foreach (ObjectValue value in objectValues)
                    {
                        objectsValues.Add(value);
                    }
                    snapshot.Structure.SetObjects(targetIndex, objectsValues.Build());
                }

                if (!isMust)
                {
                    visitor.AddValue(snapshot.UndefinedValue);
                }

                snapshot.CopyAliases(sourceIndex, targetIndex, isMust);

                snapshot.Structure.SetMemoryEntry(targetIndex, visitor.GetCopiedEntry());
            }
        }

        /// <summary>
        /// Processes the array value - create new copy of this level of memory tree.
        /// </summary>
        /// <param name="targetIndex">Index of the target.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        internal AssociativeArray ProcessArrayValue(MemoryIndex targetIndex, AssociativeArray value)
        {
            AssociativeArray arrayValue = snapshot.CreateArray(targetIndex, isMust);

            ArrayDescriptor sourceDescriptor = snapshot.Structure.GetDescriptor(value);
            ArrayDescriptor targetDescriptor = snapshot.Structure.GetDescriptor(arrayValue);

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

        /// <summary>
        /// Processes the object value - just move the reference.
        /// </summary>
        /// <param name="targetIndex">Index of the target.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        internal ObjectValue ProcessObjectValue(MemoryIndex targetIndex, ObjectValue value)
        {
            objectValues.Add(value);
            return value;
        }
    }

    /// <summary>
    /// Search for arrays and objects within the copied memory entry in order to provide deep copy of arrays.
    /// </summary>
    class CopyWithinSnapshotVisitor : AbstractValueVisitor
    {
        private CopyWithinSnapshotWorker worker;
        private MemoryIndex index;
        private HashSet<Value> values = new HashSet<Value>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyWithinSnapshotVisitor"/> class.
        /// </summary>
        /// <param name="worker">The worker.</param>
        /// <param name="index">The index.</param>
        public CopyWithinSnapshotVisitor(CopyWithinSnapshotWorker worker, MemoryIndex index)
        {
            this.worker = worker;
            this.index = index;
        }

        /// <summary>
        /// Gets the number of values in the set.
        /// </summary>
        /// <returns></returns>
        public int GetValuesCount()
        {
            return values.Count;
        }

        /// <summary>
        /// Adds the value into the set of collected values.
        /// </summary>
        /// <param name="value">The value.</param>
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

        /// <summary>
        /// Gets the copied entry.
        /// </summary>
        /// <returns></returns>
        internal MemoryEntry GetCopiedEntry()
        {
            return new MemoryEntry(values);
        }
    }
}
