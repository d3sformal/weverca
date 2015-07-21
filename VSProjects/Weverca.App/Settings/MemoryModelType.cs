using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.App.Settings
{
    public enum MemoryModelType
    {
        Copy,
        LazyExtendCommit,
        LazyContainers,
        LazyAndDiffContainers,
        Tracking,
        TrackingDiff
    }
}
