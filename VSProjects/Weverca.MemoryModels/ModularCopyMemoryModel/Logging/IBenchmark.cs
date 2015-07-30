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
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Logging
{
    /// <summary>
    /// Identifies the type of the algorithm. Used to measure algorithm
    /// statistics within the benchmark.
    /// </summary>
    public enum AlgorithmType
    {
        // supress documentation warnings
        #pragma warning disable 1591

        COMMIT,
        WIDEN_COMMIT,
        EXTEND_AS_CALL,
        MERGE_AT_SUBPROGRAM,
        MERGE_WITH_CALL,
        COPY_MEMORY,
        DELETE_MEMORY,
        EXTEND,
        MERGE,
        WRITE,
        WRITE_WITHOUT_COPY,
        SET_ALIAS,
        IS_DEFINED,
        READ,
        ITERATE_FIELDS,
        ITERATE_INDEXES,
        RESOLVE_TYPE,
        RESOLVE_METHOD,
        MERGE_TO_TEMPORARY

        // enable documentation warnings
        #pragma warning restore 1591
    }

    /// <summary>
    /// Intefface for the benchmark object. Following methods are used by thesnapshot to
    /// allow measuting of transactions, algorithm and operations.
    /// 
    /// Transaction is time period between the calls of startTransaction and 
    /// commitTransaction on the snapshot.
    /// 
    /// Operation is any read or update action requested by the analysis.
    /// 
    /// Algorithm is special operation which is handled by some memory algorithm.
    /// </summary>
    public interface IBenchmark
    {
        /// <summary>
        /// Notifies the benchmark that new snapshot was created.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        void InitializeSnapshot(Snapshot snapshot);

        /// <summary>
        ///  Notifies the benchmark that new transaction was started on given snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        void StartTransaction(Snapshot snapshot);

        /// <summary>
        ///  Notifies the benchmark that transaction on the given snapshot was finished.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        void FinishTransaction(Snapshot snapshot);

        /// <summary>
        ///  Notifies the benchmark that algorithm of the given type was started,
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="algorithmType">Type of the algorithm.</param>
        void StartAlgorithm(Snapshot snapshot, AlgorithmType algorithmType);

        /// <summary>
        ///  Notifies the benchmark that algorithm with given type was finished.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="algorithmType">Type of the algorithm.</param>
        void FinishAlgorithm(Snapshot snapshot, AlgorithmType algorithmType);

        /// <summary>
        ///  Notifies the benchmark that some operation was started on the snapshot. Includes algorithm calls.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        void StartOperation(Snapshot snapshot);

        /// <summary>
        /// Notifies the benchmark that some operation was finished on the snapshot. Includes algorithm calls.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        void FinishOperation(Snapshot snapshot);
    }
}