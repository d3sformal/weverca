using PHP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.SnapshotEntries;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm
{
    /// <summary>
    /// Generec factory interface for algorithm type.
    /// 
    /// Algorithm factory always creates new instance of algorithm. Algorithm is used only for
    /// single job. Parameters for run of algorithm will be cpecified within algorithm public
    /// interface.
    /// </summary>
    /// <typeparam name="T">Type of algorithm to create.</typeparam>
    public interface IAlgorithmFactory<T>
    {
        T CreateInstance();
    }
    
    public interface IAssignAlgorithm
    {
        void Assign(Snapshot snapshot, MemoryPath path, MemoryEntry value, bool forceStrongWrite);

        void AssignAlias(Snapshot snapshot, MemoryPath path, ICopyModelSnapshotEntry entry);

        void WriteWithoutCopy(Snapshot snapshot, MemoryPath path, MemoryEntry value);
    }

    public interface IReadAlgorithm
    {
        void Read(Snapshot snapshot, Memory.MemoryPath path);
        
        MemoryEntry GetValue();

        bool IsDefined();

        IEnumerable<AnalysisFramework.VariableIdentifier> GetFields();

        IEnumerable<MemberIdentifier> GetIndexes();

        IEnumerable<FunctionValue> GetMethod(QualifiedName methodName);

        IEnumerable<TypeValue> GetObjectType();
    }

    public interface ICommitAlgorithm
    {
        void SetStructure(Structure.ISnapshotStructureProxy SnapshotStructure, Structure.ISnapshotStructureProxy oldStructure);

        void SetData(Data.ISnapshotDataProxy currentData, Data.ISnapshotDataProxy oldData);

        void CommitAndSimplify(Snapshot snapshot, int simplifyLimit);

        void CommitAndWiden(Snapshot snapshot, int simplifyLimit, MemoryAssistantBase MemoryAssistant);

        bool IsDifferent();
    }

    public interface ICopyAlgorithm
    {

        void CopyMemory(Snapshot snapshot, MemoryIndex sourceIndex, MemoryIndex targetIndex, bool isMust);
    }

    public interface IDeleteAlgorithm
    {

        void DestroyMemory(Snapshot snapshot, MemoryIndex index);
    }

    public interface IExtendAlgorithm
    {

        void Extend(Snapshot snapshot1, Snapshot snapshot2);
    }

    public interface IMergeAlgorithm
    {

        void Merge(Snapshot snapshot, List<Snapshot> snapshots);

        void MergeWithCall(Snapshot snapshot, List<Snapshot> snapshots);

        ISnapshotStructureProxy GetMergedStructure();

        ISnapshotDataProxy GetMergedData();


        void MergeMemoryEntry(Snapshot snapshot, TemporaryIndex temporaryIndex, MemoryEntry dataEntry);
    }
}
