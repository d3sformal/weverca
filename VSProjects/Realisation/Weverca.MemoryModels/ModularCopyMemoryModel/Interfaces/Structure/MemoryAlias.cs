using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{    
    /// <summary>
    /// Contains information about alias structure for given memory location
    /// 
    /// Imutable class 
    ///     For modification use builder object 
    ///         descriptor.Builder().modify().Build() //Creates new modified object
    /// </summary>
    public interface IMemoryAlias
    {
        /// <summary>
        /// Gets the memory index which the aliases is binded to.
        /// </summary>
        /// <value>
        /// The memory index which the aliases is binded to.
        /// </value>
        MemoryIndex SourceIndex { get; }

        /// <summary>
        /// Gets the collection of may aliases.
        /// </summary>
        /// <value>
        /// The collection of may aliases.
        /// </value>
        IReadonlySet<MemoryIndex> MayAliases { get; }

        /// <summary>
        /// Gets the collection of must aliases.
        /// </summary>
        /// <value>
        /// The collection of must aliases.
        /// </value>
        IReadonlySet<MemoryIndex> MustAliases { get; }
        
        /// <summary>
        /// Creates new builder to modify this object 
        /// </summary>
        /// <returns>New builder to modify this object.</returns>
        IMemoryAliasBuilder Builder();
    }

    /// <summary>
    /// Mutable variant of MemoryAlias - use for creating new structure
    /// </summary>
    public interface IMemoryAliasBuilder
    {
        /// <summary>
        /// Gets the memory index which the aliases is binded to.
        /// </summary>
        /// <value>
        /// The memory index which the aliases is binded to.
        /// </value>
        MemoryIndex SourceIndex { get; }

        /// <summary>
        /// Gets the collection of may aliases.
        /// </summary>
        /// <value>
        /// The collection of may aliases.
        /// </value>
        IWriteableSet<MemoryIndex> MayAliases { get; }

        /// <summary>
        /// Gets the collection of must aliases.
        /// </summary>
        /// <value>
        /// The collection of must aliases.
        /// </value>
        IWriteableSet<MemoryIndex> MustAliases { get; }

        /// <summary>
        /// Sets the memory index which the aliases is binded to.
        /// </summary>
        /// <param name="index">The index.</param>
        void SetSourceIndex(MemoryIndex index);
        
        /// <summary>
        /// Builds new info object from this instance.
        /// </summary>
        /// <returns>New imutable instance with data from this builder.</returns>
        IMemoryAlias Build();
    }
}
