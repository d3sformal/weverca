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


#define COPY_SNAPSHOT_LOG

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

        public void Log(Snapshot snapshot) 
        {
#if COPY_SNAPSHOT_LOG
            Log(snapshot.ToString());
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