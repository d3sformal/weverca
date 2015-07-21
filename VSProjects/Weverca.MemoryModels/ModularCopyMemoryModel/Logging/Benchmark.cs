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

    public class AlgorithmFamilies
    {
        public static readonly AlgorithmType[] Read = 
        {                          
            AlgorithmType.IS_DEFINED,
            AlgorithmType.READ,
            AlgorithmType.ITERATE_FIELDS,
            AlgorithmType.ITERATE_INDEXES,
            AlgorithmType.RESOLVE_TYPE,
            AlgorithmType.RESOLVE_METHOD                                    
        };

        public static readonly AlgorithmType[] Write = 
        {                       
            AlgorithmType.WRITE,
            AlgorithmType.WRITE_WITHOUT_COPY,   
            AlgorithmType.SET_ALIAS    
        };

        public static readonly AlgorithmType[] Extend = 
        {                              
            AlgorithmType.EXTEND_AS_CALL,
            AlgorithmType.EXTEND
        };

        public static readonly AlgorithmType[] Merge = 
        {                         
            AlgorithmType.MERGE_AT_SUBPROGRAM,   
            AlgorithmType.MERGE_WITH_CALL,   
            AlgorithmType.MERGE
        };

        public static readonly AlgorithmType[] Commit = 
        {                                             
            AlgorithmType.COMMIT,
            AlgorithmType.WIDEN_COMMIT
        };

        public static readonly AlgorithmType[] Memory = 
        {                          
            AlgorithmType.COPY_MEMORY,    
            AlgorithmType.DELETE_MEMORY,
            AlgorithmType.MERGE_TO_TEMPORARY   
        };
    }

    public interface IBenchmark
    {
        void ClearResults();

        void InitializeSnapshot(Snapshot snapshot);

        void StartTransaction(Snapshot snapshot);
        void FinishTransaction(Snapshot snapshot);

        void StartAlgorithm(Snapshot snapshot, AlgorithmType algorithmType);
        void FinishAlgorithm(Snapshot snapshot, AlgorithmType algorithmType);

        void StartOperation(Snapshot snapshot);
        void FinishOperation(Snapshot snapshot);


        IEnumerable<TransactionEntry> TransactionResults { get; }
        IReadOnlyDictionary<AlgorithmType, AlgorithmAggregationEntry> AlgorithmResults { get; }

        double TotalOperationTime { get; }
        double TotalAlgorithmTime { get; }

        int NumberOfOperations { get; }
        int NumberOfAlgorithms { get; }
        int NumberOfTransactions { get; }





        void WriteResultsToFile(string benchmarkFile);
    }
}