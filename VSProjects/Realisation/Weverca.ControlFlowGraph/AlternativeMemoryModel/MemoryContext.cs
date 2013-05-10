using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Weverca.ControlFlowGraph.AlternativeMemoryModel.Builders;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel
{
    /// <summary>
    /// Represents context of memory. 
    /// Here are stored all variable and object's data.
    /// 
    /// NOTE: Is immutable
    /// </summary>
    public class MemoryContext:IDeepComparable
    {

        /// <summary>
        /// Create builder
        /// </summary>
        /// <returns></returns>
        public MemoryContextBuilder CreateDerivedContextBuilder()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get possible values that are stored on given reference
        /// </summary>
        /// <param name="reference">Reference where values are stored.</param>
        /// <returns>Possible values for reference.</returns>
        public IEnumerable<AbstractValue> GetPossibleValues(Reference reference)
        {
            throw new NotImplementedException();
        }

        public int DeepGetHashCode()
        {
            throw new NotImplementedException();
        }

        public bool DeepEquals(object other)
        {
            throw new NotImplementedException();
        }
    }
}
