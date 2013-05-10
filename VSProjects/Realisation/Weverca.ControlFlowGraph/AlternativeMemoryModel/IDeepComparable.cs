using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel
{
    interface IDeepComparable
    {
        /// <summary>
        /// HashCode which condiser inner structure of represented value.
        /// </summary>
        /// <returns>HashCode</returns>
        int DeepGetHashCode();

        /// <summary>
        /// Equality comparison which consider inner structure of represented value.
        /// </summary>
        /// <param name="other">Object to be compared</param>
        /// <returns>True if objects are equal according to inner structure, false otherwise.</returns>
        bool DeepEquals(object other);
    }
}
