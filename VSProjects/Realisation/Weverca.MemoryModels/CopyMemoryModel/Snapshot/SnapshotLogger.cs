using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Provides logging functionality for snapshots - appends log info into copy_memory_model.log file.
    /// Log file is cleared at the start of application.
    /// </summary>
    class SnapshotLogger
    {
        static readonly string logFile = @"copy_memory_model.log";

        static Snapshot oldOne = null;

        static SnapshotLogger()
        {
            System.IO.File.Delete(logFile);
        }

        public static void append(string message)
        {
            using (System.IO.StreamWriter w = System.IO.File.AppendText(logFile))
            {
                w.Write("\r\n" + message);
            }
        }

        public static void append(Snapshot snapshot)
        {
            using (System.IO.StreamWriter w = System.IO.File.AppendText(logFile))
            {
                w.Write("\r\n\r\n");
                w.WriteLine(snapshot.ToString());
                w.WriteLine("-------------------------------");
            }

            oldOne = snapshot;
        }

        public static void append(Snapshot snapshot, String message)
        {
            using (System.IO.StreamWriter w = System.IO.File.AppendText(logFile))
            {
                w.Write("\r\n" + snapshot.getSnapshotIdentification() + " > " + message);
            }
        }

        public static void append(SnapshotBase snapshotBase, String message)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(snapshotBase);
            append(snapshot, message);
        }

        public static void appendToSameLine(String message)
        {
            using (System.IO.StreamWriter w = System.IO.File.AppendText(logFile))
            {
                w.Write(message);
            }
        }
    }
}
