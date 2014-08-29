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
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Logging
{
    /// <summary>
    /// Defines methods to log snapshot messages.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs the message for specified snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="message">The message.</param>
        void Log(Snapshot snapshot, string message);

        /// <summary>
        /// Logs the content of specified snapshot.
        /// </summary>
        /// <param name="snapshot"></param>
        void Log(Snapshot snapshot);

        /// <summary>
        /// Logs the message using given format string with given values for specified snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="format">The format.</param>
        /// <param name="values">The values.</param>
        void Log(Snapshot snapshot, string format, params object[] values);

        /// <summary>
        /// Logs the message for specified snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="message">The message.</param>
        void Log(SnapshotBase snapshot, string message);

        /// <summary>
        /// Logs the message using given format string with given values for specified snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="format">The format.</param>
        /// <param name="values">The values.</param>
        void Log(SnapshotBase snapshot, string format, params object[] values);

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Log(string message);

        /// <summary>
        /// Logs the specified message using given format string with given values.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="values">The values.</param>
        void Log(string format, params object[] values);

        /// <summary>
        /// Logs given mesage and append it to the end of prevoius file to same line.
        /// </summary>
        /// <param name="message">The message.</param>
        void LogToSameLine(string message);
    }
}