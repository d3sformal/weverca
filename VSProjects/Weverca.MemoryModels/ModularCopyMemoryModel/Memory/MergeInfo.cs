using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Memory
{
    public class MergeInfo
    {
        private Dictionary<MemoryIndex, MemoryIndexDatasourcesContainer> containers = new Dictionary<MemoryIndex, MemoryIndexDatasourcesContainer>();

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

        public MemoryIndexDatasourcesContainer this[MemoryIndex memoryIndex]
        {
            get { return GetOrCreateDatasourcesContaier(memoryIndex); }
        }

        public IEnumerable<MemoryIndex> GetIndexes()
        {
            return containers.Keys;
        }
    }

    public class MemoryIndexDatasourcesContainer
    {
        private Dictionary<Snapshot, MemoryIndex> datasources = new Dictionary<Snapshot, MemoryIndex>();

        public bool TryGetDatasource(Snapshot snapshot, out MemoryIndex memoryIndex)
        {
            return datasources.TryGetValue(snapshot, out memoryIndex);
        }

        public void SetDatasource(Snapshot snapshot, MemoryIndex memoryIndex)
        {
            datasources[snapshot] = memoryIndex;
        }
    }

}
