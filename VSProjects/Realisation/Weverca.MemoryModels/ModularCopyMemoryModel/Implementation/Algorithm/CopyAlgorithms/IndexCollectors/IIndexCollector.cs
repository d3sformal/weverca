using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.IndexCollectors
{
    /// <summary>
    /// Base interface for collection algorithms with definition of interface of collectors.
    /// 
    /// Collection algorithm is the first part of write/read operation. Collector's responsibility
    /// is to prepare all memory location which satisfies given access path. There are two types
    /// of collectors. Read collector which just traverse the memory tree and looks for defined
    /// locations. Then there is update collector which also traverse the memory tree but when there
    /// is missing memory location which should be added into output list the new location is created
    /// (new variable, associative array or implicit object is created).
    /// </summary>
    public interface IIndexCollector
    {
        /// <summary>
        /// Gets the list of must indexes to provide the strong operation.
        /// </summary>
        /// <value>
        /// The must indexes.
        /// </value>
        IEnumerable<MemoryIndex> MustIndexes { get; }

        /// <summary>
        /// Gets the list of may indexes to provide the weak operation.
        /// </summary>
        /// <value>
        /// The may indexes.
        /// </value>
        IEnumerable<MemoryIndex> MayIndexes { get; }

        /// <summary>
        /// Gets the list of must value location to provide the strong operation.
        /// </summary>
        /// <value>
        /// The must location.
        /// </value>
        IEnumerable<ValueLocation> MustLocation { get; }

        /// <summary>
        /// Gets the list of may value location to provide the weak operation.
        /// </summary>
        /// <value>
        /// The may locaton.
        /// </value>
        IEnumerable<ValueLocation> MayLocaton { get; }

        /// <summary>
        /// Gets the number of must indexes.
        /// </summary>
        /// <value>
        /// The must indexes count.
        /// </value>
        int MustIndexesCount { get; }

        /// <summary>
        /// Gets the number of may indexes.
        /// </summary>
        /// <value>
        /// The may indexes count.
        /// </value>
        int MayIndexesCount { get; }

        /// <summary>
        /// Gets a value indicating whether access path is defined or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is defined]; otherwise, <c>false</c>.
        /// </value>
        bool IsDefined { get; }

        /// <summary>
        /// Pocess the next segment.
        /// </summary>
        /// <param name="segment">The segment.</param>
        void Next(PathSegment segment);
    }

    /// <summary>
    /// Abstract implementation of <see cref="IIndexCollector"/> interface which defines basic interface and provides basic operation.
    /// </summary>
    public abstract class IndexCollector : IIndexCollector
    {
        /// <summary>
        /// Gets the information whether to look at global or local level.
        /// </summary>
        /// <value>
        /// The global.
        /// </value>
        public GlobalContext Global { get; private set; }

        /// <summary>
        /// Gets the call level.
        /// </summary>
        /// <value>
        /// The call level.
        /// </value>
        public int CallLevel { get; private set; }

        /// <summary>
        /// Processes the given access path.
        /// </summary>
        /// <param name="path">The path.</param>
        public void ProcessPath(MemoryPath path)
        {
            Global = path.Global;
            CallLevel = path.CallLevel;

            foreach (PathSegment segment in path.PathSegments)
            {
                Next(segment);
            }
        }

        /// <summary>
        /// Gets the list of must indexes to provide the strong operation.
        /// </summary>
        /// <value>
        /// The must indexes.
        /// </value>
        public abstract IEnumerable<MemoryIndex> MustIndexes { get; }

        /// <summary>
        /// Gets the list of may indexes to provide the weak operation.
        /// </summary>
        /// <value>
        /// The may indexes.
        /// </value>
        public abstract IEnumerable<MemoryIndex> MayIndexes { get; }

        /// <summary>
        /// Gets the list of must value location to provide the strong operation.
        /// </summary>
        /// <value>
        /// The must location.
        /// </value>
        public abstract IEnumerable<ValueLocation> MustLocation { get; }

        /// <summary>
        /// Gets the list of may value location to provide the weak operation.
        /// </summary>
        /// <value>
        /// The may locaton.
        /// </value>
        public abstract IEnumerable<ValueLocation> MayLocaton { get; }

        /// <summary>
        /// Gets the number of must indexes.
        /// </summary>
        /// <value>
        /// The must indexes count.
        /// </value>
        public abstract int MustIndexesCount { get; }

        /// <summary>
        /// Gets the number of may indexes.
        /// </summary>
        /// <value>
        /// The may indexes count.
        /// </value>
        public abstract int MayIndexesCount { get; }

        /// <summary>
        /// Gets a value indicating whether access path is defined or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is defined]; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsDefined { get; protected set; }

        /// <summary>
        /// Pocess the next segment.
        /// </summary>
        /// <param name="segment">The segment.</param>
        public abstract void Next(PathSegment segment);
    }
}
