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
