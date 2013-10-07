using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;

namespace Weverca.Analysis.Memory
{
    /// <summary>
    /// Represents iterator which is used for traversing arrays and objects
    /// </summary>
    public abstract class ContainerIteratorBase
    {
        /// <summary>
        /// Iterate through whole structure index by index.
        /// </summary>
        /// <returns>Next container index or null if not available</returns>
        protected abstract ContainerIndex getNextIndex();

        /// <summary>
        /// Determine that iteration has already ended (means that no other MoveNext calls are allowed)
        /// </summary>
        public bool IterationEnd { get; private set; }

        /// <summary>
        /// Current index pointed by iterator
        /// </summary>
        public ContainerIndex CurrentIndex { get; private set; }
            
        /// <summary>
        /// Moves to next container index in iteration
        /// </summary>
        /// <returns>Current container index</returns>
        public ContainerIndex MoveNext()
        {
            CurrentIndex= getNextIndex();
            if (CurrentIndex == null)
            {
                IterationEnd = true;
            }

            return CurrentIndex;
        }
    }
}
