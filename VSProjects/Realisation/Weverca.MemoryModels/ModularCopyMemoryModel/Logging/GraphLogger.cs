#define COPY_SNAPSHOT_GRAPH_LOG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Logging
{
    class GraphLogger
    {
#if COPY_SNAPSHOT_GRAPH_LOG
        static readonly string logFile = @"copy_memory_model.log";
#endif
    }
}
