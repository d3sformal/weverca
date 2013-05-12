using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Weverca.ControlFlowGraph.AlternativeMemoryModel.Builders;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel
{
    /// <summary>
    /// Represents context of memory.
    /// Provides abstraction that here are stored all variable and object's data.
    /// 
    /// NOTE: 
    ///     * Is immutable
    ///     * Real data are efficiently stored in MemoryStorage0
    /// </summary>
    public class MemoryContext:IDeepComparable
    {
        /// <summary>
        /// Version of memory context
        /// </summary>
        internal readonly MemoryContextVersion Version;

        /// <summary>
        /// Storage for data represented by context
        /// </summary>
        private readonly MemoryStorage _storage;



        internal MemoryContext(MemoryStorage storage, MemoryContextVersion version)
        {
            _storage = storage;
            Version = version;
        }

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
        /// NOTE:
        ///     This is analogy to dereferencing
        /// </summary>
        /// <param name="reference">Reference where values are stored.</param>
        /// <returns>Possible values for reference.</returns>
        public IEnumerable<AbstractValue> GetPossibleValues(VirtualReference reference)
        {
            return _storage.GetPossibleValues(this, reference);
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
