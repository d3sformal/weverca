using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels
{
    /// <summary>
    /// Factory class to provide instances of memory model snapshot.
    /// 
    /// Implementation of this class should override to string method to provide name of memory model.
    /// </summary>
    public interface MemoryModelFactory
    {
        /// <summary>
        /// Creates a snapshot of given memory model.
        /// </summary>
        /// <returns>a snapshot of given memory model</returns>
        SnapshotBase CreateSnapshot();
    }
}
