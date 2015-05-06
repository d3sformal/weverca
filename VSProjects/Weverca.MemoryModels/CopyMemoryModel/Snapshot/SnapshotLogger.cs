/*
Copyright (c) 2012-2014 Pavel Bastecky.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


//#define COPY_SNAPSHOT_LOG

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
#if COPY_SNAPSHOT_LOG
        static readonly string logFile = @"copy_memory_model.log";
        static Snapshot oldOne = null;
#endif

        static SnapshotLogger()
        {
#if COPY_SNAPSHOT_LOG
            System.IO.File.Delete(logFile);
#endif
        }

        public static void append(string message)
        {
#if COPY_SNAPSHOT_LOG
            using (System.IO.StreamWriter w = System.IO.File.AppendText(logFile))
            {
                w.Write("\r\n" + message);
            }
#endif
        }

        public static void append(Snapshot snapshot)
        {
#if COPY_SNAPSHOT_LOG
            using (System.IO.StreamWriter w = System.IO.File.AppendText(logFile))
            {
                w.Write("\r\n\r\n");
                w.WriteLine(snapshot.ToString());
                w.WriteLine("-------------------------------");
            }

            oldOne = snapshot;
#endif
        }

        public static void append(Snapshot snapshot, String message)
        {
#if COPY_SNAPSHOT_LOG
            using (System.IO.StreamWriter w = System.IO.File.AppendText(logFile))
            {
                w.Write("\r\n" + snapshot.getSnapshotIdentification() + " > " + message);
            }
#endif
        }

        public static void append(SnapshotBase snapshotBase, String message)
        {
#if COPY_SNAPSHOT_LOG
            Snapshot snapshot = SnapshotEntry.ToSnapshot(snapshotBase);
            append(snapshot, message);
#endif
        }

        public static void appendToSameLine(String message)
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