using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Utils;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers.Merge
{
    /// <summary>
    /// Provides merge operations connected with objects.
    /// 
    /// Collects set of objects for a single target index and stores them within the structure.
    /// </summary>
    class MergeObjectsStructureWorker
    {
        private IWriteableSnapshotStructure writeableTargetStructure;
        private TrackingMergeStructureWorker worker;

        private HashSet<ObjectValue> objectValues;
        private bool hasObjects = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeObjectsStructureWorker"/> class.
        /// </summary>
        /// <param name="writeableTargetStructure">The writeable target structure.</param>
        /// <param name="worker">The worker.</param>
        public MergeObjectsStructureWorker(IWriteableSnapshotStructure writeableTargetStructure, TrackingMergeStructureWorker worker)
        {
            this.writeableTargetStructure = writeableTargetStructure;
            this.worker = worker;

            objectValues = new HashSet<ObjectValue>();
        }


        /// <summary>
        /// Collects all objects from given collection of objects.
        /// </summary>
        /// <param name="obejcts">The obejcts.</param>
        public void collectSourceObjects(IObjectValueContainer obejcts)
        {
            if (obejcts != null && obejcts.Count > 0)
            {
                hasObjects = true;
                CollectionMemoryUtils.AddAll(objectValues, obejcts.Values);
            }
        }

        /// <summary>
        /// Stores collected objects into the structure and clear inner container.
        /// </summary>
        /// <param name="targetIndex">Index of the target.</param>
        public void MergeObjectsAndClear(MemoryIndex targetIndex)
        {
            if (hasObjects)
            {
                IObjectValueContainer objectsContainer = worker.Factories.StructuralContainersFactories.ObjectValueContainerFactory.CreateObjectValueContainer(writeableTargetStructure, objectValues);
                writeableTargetStructure.SetObjects(targetIndex, objectsContainer);

                hasObjects = false;
                objectValues.Clear();
            }
        }
    }
}
