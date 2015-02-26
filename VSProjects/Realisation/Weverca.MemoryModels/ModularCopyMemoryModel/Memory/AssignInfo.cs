using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Memory
{
    public class AssignInfo
    {
        private Dictionary<MemoryPath, MemoryIndexModificationList> assignedPaths = new Dictionary<MemoryPath,MemoryIndexModificationList>();

        public MemoryIndexModificationList GetOrCreatePathModification(MemoryPath modifiedPath)
        {
            MemoryIndexModificationList modificationList;
            if (!assignedPaths.TryGetValue(modifiedPath, out modificationList))
            {
                modificationList = new MemoryIndexModificationList();
                assignedPaths.Add(modifiedPath, modificationList);
            }

            return modificationList;
        }
    }

    public class MemoryIndexModificationList
    {
        public IEnumerable<KeyValuePair<MemoryIndex, MemoryIndexModification>> Modifications { get { return modifications; } }
        private readonly Dictionary<MemoryIndex, MemoryIndexModification> modifications;

        public MemoryIndexModificationList()
        {
            modifications = new Dictionary<MemoryIndex, MemoryIndexModification>();
        }

        public MemoryIndexModification GetOrCreateModification(MemoryIndex modifiedIndex)
        {
            MemoryIndexModification modification;
            if (!modifications.TryGetValue(modifiedIndex, out modification))
            {
                modification = new MemoryIndexModification(modifiedIndex);
                modifications.Add(modifiedIndex, modification);
            }

            return modification;
        }

        public MemoryIndexModification this[MemoryIndex modifiedIndex]
        {
            get { return GetOrCreateModification(modifiedIndex); }
        }
    }

    public class MemoryIndexModification
    {
        private readonly MemoryIndex TargetIndex;

        public bool IsCollectedIndex { get; private set; }

        public IEnumerable<MemoryIndexDataSource> Datasources { get { return datasources; } }
        private readonly HashSet<MemoryIndexDataSource> datasources;

        public MemoryIndexModification(MemoryIndex targetIndex)
        {
            TargetIndex = targetIndex;
            IsCollectedIndex = false;
            datasources = new HashSet<MemoryIndexDataSource>();
        }

        public void AddDatasource(MemoryIndex sourceIndex, Snapshot sourceSnapshot)
        {
            MemoryIndexDataSource datasource = new MemoryIndexDataSource(sourceIndex, sourceSnapshot);
            if (!datasources.Contains(datasource))
            {
                datasources.Add(datasource);
            }
        }

        public void SetCollectedIndex()
        {
            IsCollectedIndex = true;
        }

        public override string ToString()
        {
            return TargetIndex.ToString();
        }
    }

    public class MemoryIndexDataSource
    {
        public readonly MemoryIndex SourceIndex;
        public readonly Snapshot SourceSnapshot;

        public MemoryIndexDataSource(MemoryIndex sourceIndex, Snapshot sourceSnapshot)
        {
            SourceIndex = sourceIndex;
            SourceSnapshot = sourceSnapshot;
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            MemoryIndexDataSource other = obj as MemoryIndexDataSource;
            if (other != null)
            {
                return SourceIndex.Equals(other.SourceIndex) && SourceSnapshot.Equals(other.SourceSnapshot);
            }
            else 
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return SourceIndex.GetHashCode() ^ SourceSnapshot.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", SourceIndex, SourceSnapshot.getSnapshotIdentification());
        }
    }
}
