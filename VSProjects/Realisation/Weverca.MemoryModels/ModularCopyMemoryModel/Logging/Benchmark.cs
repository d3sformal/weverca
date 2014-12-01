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
    public enum AlgorithmType
    {
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

    }

    public interface IBenchmark
    {
        void InitializeSnapshot(Snapshot snapshot);

        void StartTransaction(Snapshot snapshot);

        void FinishTransaction(Snapshot snapshot);

        void StartAlgorithm(Snapshot snapshot, IAlgorithm algorithmInstance, AlgorithmType algorithmType);

        void FinishAlgorithm(Snapshot snapshot, IAlgorithm algorithmInstance, AlgorithmType algorithmType);

        void WriteResultsToFile(string benchmarkFile);
    }
}