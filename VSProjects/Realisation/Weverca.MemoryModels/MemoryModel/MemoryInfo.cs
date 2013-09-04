using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.MemoryModel
{
    /// <summary>
    /// Contains information about alias structure for given memory location
    /// 
    /// Imutable class 
    ///     For modification use builder object 
    ///         descriptor.Builder().modify().Build() //Creates new modified object
    /// </summary>
    public class MemoryInfo
    {
        /// <summary>
        /// Gets the may aliasses.
        /// </summary>
        /// <value>
        /// The may aliasses.
        /// </value>
        public ReadOnlyCollection<MemoryIndex> MayAliasses { get; private set; }

        /// <summary>
        /// Gets the must aliasses.
        /// </summary>
        /// <value>
        /// The must aliasses.
        /// </value>
        public ReadOnlyCollection<MemoryIndex> MustAliasses { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryInfo"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        internal MemoryInfo(MemoryIndex index)
        {
            MayAliasses = new ReadOnlyCollection<MemoryIndex>(new MemoryIndex[] { });
            MustAliasses = new ReadOnlyCollection<MemoryIndex>(new MemoryIndex[]{ index });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryInfo"/> class from the builder object.
        /// </summary>
        /// <param name="builder">The builder.</param>
        internal MemoryInfo(MemoryInfoBuilder builder)
        {
            MayAliasses = new ReadOnlyCollection<MemoryIndex>(builder.MayAliasses);
            MustAliasses = new ReadOnlyCollection<MemoryIndex>(builder.MustAliasses);
        }

        /// <summary>
        /// Creates new builder to modify this object 
        /// </summary>
        /// <returns></returns>
        public MemoryInfoBuilder Builder()
        {
            return new MemoryInfoBuilder(this);
        }
    }

    /// <summary>
    /// Mutable variant of MemoryInfo - use for creating new structure
    /// </summary>
    public class MemoryInfoBuilder
    {
        /// <summary>
        /// Gets the may aliasses.
        /// </summary>
        /// <value>
        /// The may aliasses.
        /// </value>
        public List<MemoryIndex> MayAliasses { get; private set; }

        /// <summary>
        /// Gets the must aliasses.
        /// </summary>
        /// <value>
        /// The must aliasses.
        /// </value>
        public List<MemoryIndex> MustAliasses { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryInfoBuilder"/> class.
        /// </summary>
        /// <param name="memoryInfo">The memory information.</param>
        public MemoryInfoBuilder(MemoryInfo memoryInfo)
        {
            MayAliasses = new List<MemoryIndex>(memoryInfo.MayAliasses);
            MustAliasses = new List<MemoryIndex>(memoryInfo.MustAliasses);
        }

        /// <summary>
        /// Builds new info object from this instance.
        /// </summary>
        /// <returns></returns>
        public MemoryInfo Build()
        {
            return new MemoryInfo(this);
        }

        /// <summary>
        /// Removes the must alias.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public MemoryInfoBuilder RemoveMustAlias(MemoryIndex index)
        {
            MustAliasses.Remove(index);
            return this;
        }

        /// <summary>
        /// Removes the may alias.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public MemoryInfoBuilder RemoveMayAlias(MemoryIndex index)
        {
            MayAliasses.Remove(index);
            return this;
        }

        /// <summary>
        /// Adds the must alias.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public MemoryInfoBuilder AddMustAlias(MemoryIndex index)
        {
            MustAliasses.Add(index);
            return this;
        }

        /// <summary>
        /// Adds the may alias.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public MemoryInfoBuilder AddMayAlias(MemoryIndex index)
        {
            MayAliasses.Add(index);
            return this;
        }
    }
}
