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
    class MergeObjectsStructureWorker
    {
        private IWriteableSnapshotStructure writeableTargetStructure;
        private TrackingMergeStructureWorker worker;

        private HashSet<ObjectValue> objectValues;
        private bool hasObjects = false;

        public MergeObjectsStructureWorker(IWriteableSnapshotStructure writeableTargetStructure, TrackingMergeStructureWorker worker)
        {
            this.writeableTargetStructure = writeableTargetStructure;
            this.worker = worker;

            objectValues = new HashSet<ObjectValue>();
        }


        public void collectSourceObjects(IObjectValueContainer obejcts)
        {
            if (obejcts != null && obejcts.Count > 0)
            {
                hasObjects = true;
                CollectionMemoryUtils.AddAll(objectValues, obejcts.Values);
            }
        }

        public void MergeObjectsAndClear(MemoryIndex targetIndex)
        {
            if (hasObjects)
            {
                IObjectValueContainer objectsContainer = worker.Structure.CreateObjectValueContainer(objectValues);
                writeableTargetStructure.SetObjects(targetIndex, objectsContainer);

                hasObjects = false;
                objectValues.Clear();
            }
        }
    }
}
