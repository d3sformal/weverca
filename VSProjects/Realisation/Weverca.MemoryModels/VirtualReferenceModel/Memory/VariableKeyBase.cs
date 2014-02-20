using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.VirtualReferenceModel.Memory
{
    /// <summary>
    /// Base class for keys of variables stored within <see cref="Snapshot"/>
    /// </summary>
    abstract class VariableKeyBase
    {
        /// <summary>
        /// Get or create variable  in context of given snapshot
        /// </summary>
        /// <param name="snapshot">Context snapshot</param>
        /// <returns>Created or obtained variable info</returns>
        internal abstract VariableInfo GetOrCreateVariable(Snapshot snapshot);

        /// <summary>
        /// Get variable in context of given snapshot
        /// </summary>
        /// <param name="snapshot">Context snapshot</param>
        /// <returns>Obtained variable info</returns>
        internal abstract VariableInfo GetVariable(Snapshot snapshot);

        /// <summary>
        /// Create implicit reference of belonging variable in context of given snapshot
        /// </summary>
        /// <param name="snapshot">Context snapshot</param>
        /// <returns>Created or obtained variable info</returns>
        internal abstract VirtualReference CreateImplicitReference(Snapshot snapshot);
    }
}
