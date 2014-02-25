using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Defines common methods for merge algorithms.
    /// </summary>
    public interface IMergeWorker
    {
        /// <summary>
        /// Adds operation into stack of merge worker.
        /// </summary>
        /// <param name="operation">The operation.</param>
        void addOperation(MergeOperation operation);
    }
}
