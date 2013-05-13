using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel
{
    /// <summary>
    /// Representation of value that can be stored in some MemoryContext
    /// 
    /// WARNING: All implementations has to be immutable 
    /// NOTE: overrides standard hashing and equal function with their IDeepComparable equivalents, 
    ///     so it can be used with .NET containers
    /// </summary>
    public abstract class AbstractValue : IDeepComparable
    {

        /// <summary>
        /// When overriden returns string representation of PHP value
        /// </summary>
        /// <returns>String representation of PHP value</returns>
        protected abstract string toString();


        #region Abstract implementation of IDeepComparable
        public abstract int DeepGetHashCode();
        public abstract bool DeepEquals(object other);
        #endregion


        #region Standard method overrides
        public override bool Equals(object obj)
        {
            var hasSameReference = base.Equals(obj);
            return hasSameReference || DeepEquals(obj);
        }

        public override int GetHashCode()
        {
            return DeepGetHashCode();
        }

        public override string ToString()
        {
            return toString();
        }
        #endregion
    }
}
