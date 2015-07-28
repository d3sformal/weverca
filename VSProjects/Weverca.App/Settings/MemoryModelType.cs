using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.App.Settings
{
    /// <summary>
    /// Identifies the variant of the memory model to perform the analysis
    /// </summary>
    public enum MemoryModelType
    {
        Copy,
        LazyExtendCommit,
        LazyContainers,
        Tracking,
        TrackingDiff
    }
}
