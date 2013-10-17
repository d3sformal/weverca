using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class AssignWorker
    {
        private Snapshot snapshot;

        public AssignWorker(Snapshot snapshot)
        {
            this.snapshot = snapshot;
        }
    }
}
