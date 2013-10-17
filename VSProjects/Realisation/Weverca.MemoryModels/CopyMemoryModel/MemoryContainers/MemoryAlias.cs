using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Contains information about alias structure for given memory location
    /// 
    /// Imutable class 
    ///     For modification use builder object 
    ///         descriptor.Builder().modify().Build() //Creates new modified object
    /// </summary>
    class MemoryAlias
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
        /// Initializes a new instance of the <see cref="MemoryAlias"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        internal MemoryAlias(MemoryIndex index)
        {
            MayAliasses = new ReadOnlyCollection<MemoryIndex>(new MemoryIndex[] { });
            MustAliasses = new ReadOnlyCollection<MemoryIndex>(new MemoryIndex[] { index });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryAlias"/> class from the builder object.
        /// </summary>
        /// <param name="builder">The builder.</param>
        internal MemoryAlias(MemoryAliasBuilder builder)
        {
            MayAliasses = new ReadOnlyCollection<MemoryIndex>(builder.MayAliasses);
            MustAliasses = new ReadOnlyCollection<MemoryIndex>(builder.MustAliasses);
        }

        /// <summary>
        /// Creates new builder to modify this object 
        /// </summary>
        /// <returns></returns>
        public MemoryAliasBuilder Builder()
        {
            return new MemoryAliasBuilder(this);
        }
    }

    /// <summary>
    /// Mutable variant of MemoryAlias - use for creating new structure
    /// </summary>
    class MemoryAliasBuilder
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
        /// Initializes a new instance of the <see cref="MemoryAliasBuilder"/> class.
        /// </summary>
        /// <param name="MemoryAlias">The memory information.</param>
        public MemoryAliasBuilder(MemoryAlias MemoryAlias)
        {
            MayAliasses = new List<MemoryIndex>(MemoryAlias.MayAliasses);
            MustAliasses = new List<MemoryIndex>(MemoryAlias.MustAliasses);
        }

        /// <summary>
        /// Builds new info object from this instance.
        /// </summary>
        /// <returns></returns>
        public MemoryAlias Build()
        {
            return new MemoryAlias(this);
        }

        /// <summary>
        /// Removes the must alias.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public MemoryAliasBuilder RemoveMustAlias(MemoryIndex index)
        {
            MustAliasses.Remove(index);
            return this;
        }

        /// <summary>
        /// Removes the may alias.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public MemoryAliasBuilder RemoveMayAlias(MemoryIndex index)
        {
            MayAliasses.Remove(index);
            return this;
        }

        /// <summary>
        /// Adds the must alias.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public MemoryAliasBuilder AddMustAlias(MemoryIndex index)
        {
            MustAliasses.Add(index);
            return this;
        }

        /// <summary>
        /// Adds the may alias.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public MemoryAliasBuilder AddMayAlias(MemoryIndex index)
        {
            MayAliasses.Add(index);
            return this;
        }
    }
}
