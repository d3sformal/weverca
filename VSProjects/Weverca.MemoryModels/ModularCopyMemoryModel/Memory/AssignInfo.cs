using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Memory
{
    /// <summary>
    /// Special container which allows to store data from an assign algorithm to the snapshot. 
    /// Stored data are reused in the second phase to speed up an assign algorithm. 
    /// 
    /// This container also solves problem of the second phase assign which didn't propagate
    /// all possible values to the newly created location. Problem was that second phase has
    /// all location already defined - no unknown location is used to get values from.
    /// </summary>
    public class AssignInfo
    {
        /// <summary>
        /// Gets the alias assign modifications.
        /// </summary>
        /// <value>
        /// The alias assign modifications.
        /// </value>
        public MemoryIndexModificationList AliasAssignModifications { get; private set; }
        
        /// <summary>
        /// The assigned paths
        /// </summary>
        private Dictionary<MemoryPath, MemoryIndexModificationList> assignedPaths = new Dictionary<MemoryPath,MemoryIndexModificationList>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignInfo"/> class.
        /// </summary>
        public AssignInfo()
        {
            AliasAssignModifications = new MemoryIndexModificationList();
        }

        /// <summary>
        /// Gets the or create path modification.
        /// </summary>
        /// <param name="modifiedPath">The modified path.</param>
        /// <returns>Collection to store all modified indexes which belongs to given path</returns>
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

    /// <summary>
    /// Collection of modified indexes. Stores an instance of MemoryIndexModification for each
    /// modified index. Each instance of MemoryIndexModification contains the set of datasources
    /// and infor mation whether the index was collected.
    /// </summary>
    public class MemoryIndexModificationList
    {
        /// <summary>
        /// Gets the modifications.
        /// </summary>
        /// <value>
        /// The modifications.
        /// </value>
        public IEnumerable<KeyValuePair<MemoryIndex, MemoryIndexModification>> Modifications { get { return modifications; } }

        private readonly Dictionary<MemoryIndex, MemoryIndexModification> modifications;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryIndexModificationList"/> class.
        /// </summary>
        public MemoryIndexModificationList()
        {
            modifications = new Dictionary<MemoryIndex, MemoryIndexModification>();
        }

        /// <summary>
        /// Gets the or create modification for given index.
        /// </summary>
        /// <param name="modifiedIndex">Index which was modified.</param>
        /// <returns>A modification container for given index stored within this object.</returns>
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
    }

    /// <summary>
    /// Represents single memory location which were assigned into. Contains the memory index
    /// and set of all datasources where to get additional data for an assign (unknown indexes).
    /// </summary>
    public class MemoryIndexModification
    {
        /// <summary>
        /// Gets the target memory index of this modification.
        /// </summary>
        /// <value>
        /// The target index.
        /// </value>
        public MemoryIndex TargetIndex { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the target index in this instance was collected. 
        /// If is collected then the new data should be assigned into. Otherwise an index was
        /// implicitly created during the assign operation. 
        /// </summary>
        /// <value>
        /// <c>true</c> if the target index in this instance was collected; otherwise, <c>false</c>.
        /// </value>
        public bool IsCollectedIndex { get; private set; }

        /// <summary>
        /// Gets the list of the datasources for current modification.
        /// </summary>
        /// <value>
        /// The list of datasources.
        /// </value>
        public IEnumerable<MemoryIndexDataSource> Datasources { get { return datasources; } }

        private readonly HashSet<MemoryIndexDataSource> datasources;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryIndexModification"/> class.
        /// </summary>
        /// <param name="targetIndex">Index of the target.</param>
        public MemoryIndexModification(MemoryIndex targetIndex)
        {
            this.TargetIndex = targetIndex;
            IsCollectedIndex = false;
            datasources = new HashSet<MemoryIndexDataSource>();
        }

        /// <summary>
        /// Adds the datasource.
        /// </summary>
        /// <param name="sourceIndex">Index of the source.</param>
        /// <param name="sourceSnapshot">The source snapshot.</param>
        public void AddDatasource(MemoryIndex sourceIndex, Snapshot sourceSnapshot)
        {
            MemoryIndexDataSource datasource = new MemoryIndexDataSource(sourceIndex, sourceSnapshot);
            if (!datasources.Contains(datasource))
            {
                datasources.Add(datasource);
            }
        }

        /// <summary>
        /// Sets the IsCollectedIndex value to true. Cannot be reverted.
        /// </summary>
        public void SetCollectedIndex()
        {
            IsCollectedIndex = true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return TargetIndex.ToString();
        }
    }

    /// <summary>
    /// Represents information which memory location should be use as source of additional 
    /// data for an assigned index.
    /// </summary>
    public class MemoryIndexDataSource
    {
        /// <summary>
        /// Gets the source index.
        /// </summary>
        /// <value>
        /// The source index.
        /// </value>
        public MemoryIndex SourceIndex { get; private set; }

        /// <summary>
        /// Gets the source snapshot.
        /// </summary>
        /// <value>
        /// The source snapshot.
        /// </value>
        public Snapshot SourceSnapshot { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryIndexDataSource"/> class.
        /// </summary>
        /// <param name="sourceIndex">Index of the source.</param>
        /// <param name="sourceSnapshot">The source snapshot.</param>
        public MemoryIndexDataSource(MemoryIndex sourceIndex, Snapshot sourceSnapshot)
        {
            SourceIndex = sourceIndex;
            SourceSnapshot = sourceSnapshot;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return SourceIndex.GetHashCode() ^ SourceSnapshot.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("{0} - {1}", SourceIndex, SourceSnapshot.getSnapshotIdentification());
        }
    }
}
