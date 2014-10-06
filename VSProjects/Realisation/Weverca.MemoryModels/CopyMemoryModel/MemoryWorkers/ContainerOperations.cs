/*
Copyright (c) 2012-2014 Pavel Bastecky.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Allows to specify list of index containers and creates merge operations for all indexes within these containers.
    /// This collector supports both phase of analysis. For the first phase there is method to collect indexes into target
    /// index container which is not necessary for the second phase where the structure must not be changed.
    /// 
    /// For the both phases there is method to create merge operations for all indexes in target container. New operations
    /// are inserted directly to merge worker using its public interface.
    /// </summary>
    class ContainerOperations
    {
        private bool isUndefined = false;

        /// <summary>
        /// The merge worker where the operations are inserted into
        /// </summary>
        private IMergeWorker worker;

        /// <summary>
        /// The target container
        /// </summary>
        private IWriteableIndexContainer targetContainer;

        /// <summary>
        /// The target index when the merge is applied to array
        /// </summary>
        private MemoryIndex targetIndex;

        /// <summary>
        /// The unknown operation - for merge unknown locations from the collections
        /// </summary>
        private MergeOperation unknownOperation;

        /// <summary>
        /// List of source containers
        /// </summary>
        private List<Tuple<ReadonlyIndexContainer, Snapshot>> sources = new List<Tuple<ReadonlyIndexContainer, Snapshot>>();

        /// <summary>        
        /// List of strings for which there is no memory index which match target location.
        /// 
        /// During merge operation may happen that to some target index is merged data from different location
        /// (typycally some unknown location). Memory index from this location can not be used in target collection
        /// so collector has to create new index when there is no index which can be used.
        /// </summary>
        private HashSet<string> undefinedIndexes = new HashSet<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerOperations"/> class.
        /// </summary>
        /// <param name="worker">The worker.</param>
        /// <param name="targetContainer">The target container.</param>
        /// <param name="targetIndex">Index of the target.</param>
        /// <param name="unknownIndex">Index of the unknown.</param>
        public ContainerOperations(
            IMergeWorker worker,
            IWriteableIndexContainer targetContainer,
            MemoryIndex targetIndex,
            MemoryIndex unknownIndex)
        {
            this.targetContainer = targetContainer;
            this.worker = worker;
            this.targetIndex = targetIndex;

            unknownOperation = new MergeOperation(unknownIndex);
            worker.addOperation(unknownOperation);
        }

        /// <summary>
        /// Stores given collection as the source of indexes and adds its unknown index into unknown operation.
        /// Also gets all indexes from the source collection and inserts it into target collection.
        /// 
        /// This method is for the first phase of anaysis.
        /// </summary>
        /// <param name="sourceSnapshot">The source snapshot.</param>
        /// <param name="sourceIndex">Index of the source.</param>
        /// <param name="sourceContainer">The source container.</param>
        public void CollectIndexes(
            Snapshot sourceSnapshot,
            MemoryIndex sourceIndex,
            ReadonlyIndexContainer sourceContainer)
        {
            sources.Add(new Tuple<ReadonlyIndexContainer, Snapshot>(sourceContainer, sourceSnapshot));

            unknownOperation.Add(sourceContainer.UnknownIndex, sourceSnapshot);

            bool indexEquals = targetIndex.Equals(sourceIndex);

            foreach (var index in sourceContainer.Indexes)
            {
                MemoryIndex containerIndex;
                if (targetContainer.Indexes.TryGetValue(index.Key, out containerIndex))
                {
                    if (containerIndex == null && indexEquals)
                    {
                        targetContainer.Indexes[index.Key] = index.Value;
                        undefinedIndexes.Remove(index.Key);
                    }
                }
                else if (indexEquals)
                {
                    targetContainer.Indexes.Add(index.Key, index.Value);
                }
                else
                {
                    targetContainer.Indexes.Add(index.Key, null);
                    undefinedIndexes.Add(index.Key);
                }
            }
        }

        /// <summary>
        /// Stores given collection as the source of indexes and adds its unknown index into unknown operation.
        /// Target collection is not modified.
        /// 
        /// This method is for the second phase of anaysis.
        /// </summary>
        /// <param name="sourceContainer">The source container.</param>
        /// <param name="sourceSnapshot">The source snapshot.</param>
        public void AddContainer(ReadonlyIndexContainer sourceContainer, Snapshot sourceSnapshot)
        {
            sources.Add(new Tuple<ReadonlyIndexContainer, Snapshot>(sourceContainer, sourceSnapshot));

            unknownOperation.Add(sourceContainer.UnknownIndex, sourceSnapshot);
        }

        /// <summary>
        /// Creates merge operations for all indexes in the target collection. Source indexes for the operation are
        /// retreived from the source collections. When there is missing index in some collection the operation set
        /// to undefined.
        /// 
        /// This method is for both phases of analysis.
        /// </summary>
        public void MergeContainers()
        {
            // Process all names which has unassociated index and creates one
            foreach (string indexName in undefinedIndexes)
            {
                targetContainer.Indexes[indexName] = targetIndex.CreateIndex(indexName);
            }

            foreach (var index in targetContainer.Indexes)
            {
                MergeOperation operation = new MergeOperation(index.Value);
                worker.addOperation(operation);

                if (isUndefined)
                {
                    operation.SetUndefined();
                }

                foreach (var source in sources)
                {
                    ReadonlyIndexContainer container = source.Item1;
                    Snapshot snapshot = source.Item2;

                    MemoryIndex containerIndex;
                    if (container.Indexes.TryGetValue(index.Key, out containerIndex))
                    {
                        operation.Add(containerIndex, snapshot);
                    }
                    else
                    {
                        operation.Add(container.UnknownIndex, snapshot);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the undefined flag to true.
        /// </summary>
        internal void SetUndefined()
        {
            isUndefined = true;
        }
    }
}