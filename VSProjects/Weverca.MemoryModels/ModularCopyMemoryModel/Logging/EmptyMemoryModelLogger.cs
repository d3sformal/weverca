using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Logging
{
    /// <summary>
    /// Implementation of memory logger which prints no output.
    /// 
    /// This is the default logger.
    /// </summary>
    class EmptyMemoryModelLogger : ILogger
    {
        public void Log(Snapshot snapshot, string message)
        {
        }

        public void Log(Snapshot snapshot)
        {
        }

        public void Log(Snapshot snapshot, string format, params object[] values)
        {
        }

        public void Log(AnalysisFramework.Memory.SnapshotBase snapshot, string message)
        {
        }

        public void Log(AnalysisFramework.Memory.SnapshotBase snapshot, string format, params object[] values)
        {
        }

        public void Log(string message)
        {
        }

        public void Log(string format, params object[] values)
        {
        }

        public void LogToSameLine(string message)
        {
        }
    }
}
