using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers.Merge
{
    public class SnapshotContext
    {
        public Snapshot SourceSnapshot { get; set; }

        public int CallLevel { get; set; }

        public IReadOnlySnapshotStructure SourceStructure { get; set; }

        public IReadOnlySnapshotData SourceData { get; set; }

        public MemoryIndexTree ChangedIndexesTree { get; set; }

        public SnapshotContext(Snapshot sourceSnapshot)
        {
            this.SourceSnapshot = sourceSnapshot;
        }
    }

    public class ContainerContext
    {
        public ContainerContext(SnapshotContext context, IReadonlyIndexContainer indexContainer, MergeOperationType operationType = MergeOperationType.ChangedOnly)
        {
            this.SnapshotContext = context;
            this.IndexContainer = indexContainer;
            this.OperationType = operationType;
        }

        public IReadonlyIndexContainer IndexContainer { get; set; }
        public SnapshotContext SnapshotContext { get; set; }
        public MergeOperationType OperationType { get; set; }
    }

    public class MergeOperationContext
    {
        public MemoryIndex Index { get; set; }
        public SnapshotContext SnapshotContext { get; set; }
        public MergeOperationType OperationType { get; set; }

        public MergeOperationContext(MemoryIndex index, SnapshotContext snapshotContext, MergeOperationType operationType = MergeOperationType.ChangedOnly)
        {
            this.Index = index;
            this.SnapshotContext = snapshotContext;
            this.OperationType = operationType;
        }
    }

    public interface ITargetContainerContext
    {
        IReadonlyIndexContainer getSourceContainer();
        IWriteableIndexContainer getWriteableSourceContainer();

        MemoryIndex createMemoryIndex(string name);
    }

    public class ObjectTargetContainerContext : ITargetContainerContext
    {
        private IObjectDescriptor objectDescriptor;
        private IObjectDescriptorBuilder builder;
        private IWriteableSnapshotStructure structure;

        public ObjectTargetContainerContext(IWriteableSnapshotStructure structure, IObjectDescriptor objectDescriptor)
        {
            this.objectDescriptor = objectDescriptor;
            this.structure = structure;
        }

        public IReadonlyIndexContainer getSourceContainer()
        {
            return objectDescriptor;
        }

        public IWriteableIndexContainer getWriteableSourceContainer()
        {
            if (builder == null)
            {
                builder = objectDescriptor.Builder(structure);
            }

            return builder;
        }

        public IObjectDescriptor getCurrentDescriptor()
        {
            if (builder == null)
            {
                return objectDescriptor;
            }
            else
            {
                return builder.Build(structure);
            }
        }


        public MemoryIndex createMemoryIndex(string name)
        {
            return ObjectIndex.Create(objectDescriptor.ObjectValue, name);
        }
    }

    public class VariableTargetContainerContext : ITargetContainerContext
    {
        private IReadOnlySnapshotStructure targetStructure;
        private IWriteableSnapshotStructure writeabletargetStructure;
        private int stackLevel;

        public VariableTargetContainerContext(IReadOnlySnapshotStructure targetStructure, IWriteableSnapshotStructure writeabletargetStructure, int stackLevel)
        {
            this.targetStructure = targetStructure;
            this.writeabletargetStructure = writeabletargetStructure;
            this.stackLevel = stackLevel;
        }


        public IReadonlyIndexContainer getSourceContainer()
        {
            return targetStructure.GetReadonlyStackContext(stackLevel).ReadonlyVariables;
        }

        public IWriteableIndexContainer getWriteableSourceContainer()
        {
            return writeabletargetStructure.GetWriteableStackContext(stackLevel).WriteableVariables;
        }

        public MemoryIndex createMemoryIndex(string name)
        {
            return VariableIndex.Create(name, stackLevel);
        }
    }

    public class ControllVariableTargetContainerContext : ITargetContainerContext
    {
        private IReadOnlySnapshotStructure targetStructure;
        private IWriteableSnapshotStructure writeabletargetStructure;
        private int stackLevel;

        public ControllVariableTargetContainerContext(IReadOnlySnapshotStructure targetStructure, IWriteableSnapshotStructure writeabletargetStructure, int stackLevel)
        {
            this.targetStructure = targetStructure;
            this.writeabletargetStructure = writeabletargetStructure;
            this.stackLevel = stackLevel;
        }


        public IReadonlyIndexContainer getSourceContainer()
        {
            return targetStructure.GetReadonlyStackContext(stackLevel).ReadonlyControllVariables;
        }

        public IWriteableIndexContainer getWriteableSourceContainer()
        {
            return writeabletargetStructure.GetWriteableStackContext(stackLevel).WriteableControllVariables;
        }

        public MemoryIndex createMemoryIndex(string name)
        {
            return ControlIndex.Create(name, stackLevel);
        }
    }

    public class ArrayTargetContainerContext : ITargetContainerContext
    {
        private IArrayDescriptor arrayDescriptor;
        private IArrayDescriptorBuilder builder;
        private IWriteableSnapshotStructure strucure;

        public ArrayTargetContainerContext(IWriteableSnapshotStructure strucure, IArrayDescriptor arrayDescriptor)
        {
            this.arrayDescriptor = arrayDescriptor;
            this.strucure = strucure;
        }

        public IReadonlyIndexContainer getSourceContainer()
        {
            return arrayDescriptor;
        }

        public IWriteableIndexContainer getWriteableSourceContainer()
        {
            if (builder == null)
            {
                builder = arrayDescriptor.Builder(strucure);
            }

            return builder;
        }

        public IArrayDescriptor getCurrentDescriptor()
        {
            if (builder == null)
            {
                return arrayDescriptor;
            }
            else
            {
                return builder.Build(strucure);
            }
        }


        public MemoryIndex createMemoryIndex(string name)
        {
            return arrayDescriptor.ParentIndex.CreateIndex(name);
        }
    }
}
