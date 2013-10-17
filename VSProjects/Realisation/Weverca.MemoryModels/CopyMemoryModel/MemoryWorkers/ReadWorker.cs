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
    class ReadWorker
    {
        private Snapshot snapshot;

        public ReadWorker(Snapshot snapshot)
        {
            this.snapshot = snapshot;
        }

        public MemoryEntry ReadValue(IIndexCollector collector)
        {
            throw new NotImplementedException();
        }
    }
}
