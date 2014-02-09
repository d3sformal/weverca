using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    interface IMergeWorker
    {
        void addOperation(MergeOperation operation);
    }
}
