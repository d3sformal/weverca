using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Memory
{
    /// <summary>
    /// Special container which allows to store data for the merge algorithm within the snapshot.
    /// 
    /// This container solves the problem of merge in the second phase. Merge algorithm is not
    /// able to determine all possible sources of the merged index. All indexes exists in the
    /// second phase so merge algorithm will not use undefined indexes.
    /// </summary>
    public class MergeInfo
    {
        private Dictionary<MemoryIndex, MemoryIndexDatasourcesContainer> containers = new Dictionary<MemoryIndex, MemoryIndexDatasourcesContainer>();

        /// <summary>
        /// Gets the or create datasources contaier.
        /// </summary>
        /// <param name="memoryIndex">Index of the memory.</param>
        /// <returns>The data source container which stores datasources of the given index.</returns>
        public MemoryIndexDatasourcesContainer GetOrCreateDatasourcesContaier(MemoryIndex memoryIndex)
        {
            MemoryIndexDatasourcesContainer container;
            if (!containers.TryGetValue(memoryIndex, out container))
            {
                container = new MemoryIndexDatasourcesContainer();
                containers.Add(memoryIndex, container);
            }

            return container;
        }

        /// <summary>
        /// Gets all target index indexes which has some datasource.
        /// </summary>
        /// <returns>Collection of all target index indexes which has some datasource.</returns>
        public IEnumerable<MemoryIndex> GetIndexes()
        {
            return containers.Keys;
        }
    }

    /// <summary>
    /// Represents the container with the collection of indexes which should be used as source of the data
    /// for merged index.
    /// 
    /// A datasource is given by a combination of the source snapshot and memory index within this snapshot.
    /// </summary>
    public class MemoryIndexDatasourcesContainer
    {
        private Dictionary<Snapshot, MemoryIndex> datasources = new Dictionary<Snapshot, MemoryIndex>();

        /// <summary>
        /// Tries to the get datasource.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="memoryIndex">Index of the memory.</param>
        /// <returns>Returns true if the datasource exists.</returns>
        public bool TryGetDatasource(Snapshot snapshot, out MemoryIndex memoryIndex)
        {
            return datasources.TryGetValue(snapshot, out memoryIndex);
        }

        /// <summary>
        /// Sets the given index to be the source index of the given snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="memoryIndex">Index of the memory.</param>
        public void SetDatasource(Snapshot snapshot, MemoryIndex memoryIndex)
        {
            datasources[snapshot] = memoryIndex;
        }
    }

}
