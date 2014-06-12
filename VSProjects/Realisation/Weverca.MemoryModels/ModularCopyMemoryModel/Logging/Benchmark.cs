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
