using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers.Merge
{
    /// <summary>
    /// Defines the type of processing the children of the processed node
    /// </summary>
    public enum MergeOperationType
    {
        /// <summary>
        /// Continue thru changes only, other locations will be skipped
        /// </summary>
        ChangedOnly,

        /// <summary>
        /// The merge will process whole subtree - used when subtree is deleted or to add undefined value to an array
        /// </summary>
        WholeSubtree
    }

    /// <summary>
    /// Represents data structure for information merge operation. Every instance contains set of source
    /// indexes and snapshot which contains these indexes and target index. Merge algorithm stores instances
    /// of this class in operation stack and merges data from source indexes into target indexes.
    /// </summary>
    public class MergeOperation
    {
        /// <summary>
        /// The collection of source indexes for this merge operation.
        /// </summary>
        public readonly List<MergeOperationContext> Indexes = new List<MergeOperationContext>();

        /// <summary>
        /// Gets a value indicating whether operation is undefined or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is undefined; otherwise, <c>false</c>.
        /// </value>
        public bool IsUndefined { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is delete operation.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is delete operation; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeleteOperation { get; private set; }

        /// <summary>
        /// Gets the target index of this operation
        /// </summary>
        /// <value>
        /// The index of the target.
        /// </value>
        public MemoryIndex TargetIndex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether index is at root level.
        /// </summary>
        /// <value>
        ///   <c>true</c> if index is at root level; otherwise, <c>false</c>.
        /// </value>
        public bool IsRoot { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeOperation"/> class.
        /// </summary>
        /// <param name="targetIndex">Index of the target.</param>
        public MergeOperation(MemoryIndex targetIndex)
        {
            IsUndefined = false;
            IsDeleteOperation = false;
            TargetIndex = targetIndex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeOperation"/> class.
        /// </summary>
        public MergeOperation()
        {
            IsUndefined = false;
        }

        /// <summary>
        /// Adds the specified memory index into sources for this operation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NullReferenceException"></exception>
        public void Add(MergeOperationContext context)
        {
            if (context == null)
            {
                throw new NullReferenceException();
            }

            Indexes.Add(context);
        }

        /// <summary>
        /// Sets the undefined to true.
        /// </summary>
        public void SetUndefined()
        {
            IsUndefined = true;
        }

        /// <summary>
        /// Sets the target indeg of this operation.
        /// </summary>
        /// <param name="targetIndex">Index of the target.</param>
        public void SetTargetIndex(MemoryIndex targetIndex)
        {
            TargetIndex = targetIndex;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return TargetIndex.ToString();
        }

        /// <summary>
        /// Gets or sets the tree node.
        /// </summary>
        /// <value>
        /// The tree node.
        /// </value>
        public MemoryIndexTreeNode TreeNode { get; set; }

        internal void SetDeleteOperation()
        {
            IsDeleteOperation = true;
        }
    }
}
