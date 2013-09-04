using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.Analysis.Memory;

namespace Weverca.MemoryModels.MemoryModel
{
    /// <summary>
    /// Stores index of memory alias
    /// </summary>
    public class MemoryAlias : AliasValue
    {
        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public MemoryIndex Index { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryAlias"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        public MemoryAlias(MemoryIndex index)
        {
            Index = index;
        }
    }
}
