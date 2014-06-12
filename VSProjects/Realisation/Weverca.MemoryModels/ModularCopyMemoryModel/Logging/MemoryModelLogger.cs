//#define COPY_SNAPSHOT_LOG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.SnapshotEntries;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Logging
{
    class MemoryModelLogger : ILogger
    {

#if COPY_SNAPSHOT_LOG
        static readonly string logFile = @"copy_memory_model.log";
        static Snapshot oldOne = null;
#endif

#if COPY_SNAPSHOT_LOG
        static MemoryModelLogger()
        {
            System.IO.File.Delete(logFile);
        }
#endif

        public void Log(Snapshot snapshot, string message)
        {
#if COPY_SNAPSHOT_LOG
            using (System.IO.StreamWriter w = System.IO.File.AppendText(logFile))
            {
                w.Write("\r\n" + snapshot.getSnapshotIdentification() + " > " + message);
            }
#endif
        }

        public void Log(Snapshot snapshot, string format, params object[] values)
        {
#if COPY_SNAPSHOT_LOG
            Log(snapshot, String.Format(format, values));
#endif
        }

        public void Log(SnapshotBase snapshot, string message)
        {
#if COPY_SNAPSHOT_LOG
            Snapshot s = SnapshotEntry.ToSnapshot(snapshot);
            Log(s, message);
#endif
        }

        public void Log(SnapshotBase snapshot, string format, params object[] values)
        {
#if COPY_SNAPSHOT_LOG
            Log(snapshot, String.Format(format, values));
#endif
        }

        public void Log(string message)
        {
#if COPY_SNAPSHOT_LOG
            using (System.IO.StreamWriter w = System.IO.File.AppendText(logFile))
            {
                w.Write("\r\n" + message);
            }
#endif
        }

        public void Log(string format, params object[] values)
        {
#if COPY_SNAPSHOT_LOG
            Log(String.Format(format, values));
#endif
        }

        public void LogToSameLine(string message)
        {
#if COPY_SNAPSHOT_LOG
            using (System.IO.StreamWriter w = System.IO.File.AppendText(logFile))
            {
                w.Write(message);
            }
#endif
        }
    }
}
