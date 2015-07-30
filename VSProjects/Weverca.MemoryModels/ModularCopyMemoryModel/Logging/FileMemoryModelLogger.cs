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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.SnapshotEntries;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Logging
{
    /// <summary>
    /// Implementation of a logger interface which prints messages to theoutput file. 
    /// 
    /// This logger was used to support deploynment of the snapshot. Immediate output guarantees that logs 
    /// will be print even when computation is terminated by an exception.
    /// </summary>
    class FileMemoryModelLogger : ILogger
    {
        static readonly string logFile = @"copy_memory_model.log";

        static FileMemoryModelLogger()
        {
            System.IO.File.Delete(logFile);
        }

        public void Log(Snapshot snapshot, string message)
        {

            using (System.IO.StreamWriter w = System.IO.File.AppendText(logFile))
            {
                w.Write("\r\n" + snapshot.getSnapshotIdentification() + " > " + message);
            }

        }

        public void Log(Snapshot snapshot, string format, params object[] values)
        {

            Log(snapshot, String.Format(format, values));

        }

        public void Log(SnapshotBase snapshot, string message)
        {

            Snapshot s = SnapshotEntry.ToSnapshot(snapshot);
            Log(s, message);

        }

        public void Log(Snapshot snapshot) 
        {

            Log(snapshot.ToString());

        }

        public void Log(SnapshotBase snapshot, string format, params object[] values)
        {

            Log(snapshot, String.Format(format, values));

        }

        public void Log(string message)
        {

            using (System.IO.StreamWriter w = System.IO.File.AppendText(logFile))
            {
                w.Write("\r\n" + message);
            }

        }

        public void Log(string format, params object[] values)
        {

            Log(String.Format(format, values));

        }

        public void LogToSameLine(string message)
        {

            using (System.IO.StreamWriter w = System.IO.File.AppendText(logFile))
            {
                w.Write(message);
            }

        }
    }
}