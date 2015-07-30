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
    /// <summary>
    /// A wrapper object which contains informations connected with some source snapshot.
    /// This class is used when merge algorithm needs to work with input snapshots.
    /// </summary>
    public class SnapshotContext
    {
        /// <summary>
        /// Gets or sets the source snapshot.
        /// </summary>
        /// <value>
        /// The source snapshot.
        /// </value>
        public Snapshot SourceSnapshot { get; set; }

        /// <summary>
        /// Gets or sets the call level.
        /// </summary>
        /// <value>
        /// The call level.
        /// </value>
        public int CallLevel { get; set; }

        /// <summary>
        /// Gets or sets the source structure.
        /// </summary>
        /// <value>
        /// The source structure.
        /// </value>
        public IReadOnlySnapshotStructure SourceStructure { get; set; }

        /// <summary>
        /// Gets or sets the source data.
        /// </summary>
        /// <value>
        /// The source data.
        /// </value>
        public IReadOnlySnapshotData SourceData { get; set; }

        /// <summary>
        /// Gets or sets the changed indexes tree.
        /// </summary>
        /// <value>
        /// The changed indexes tree.
        /// </value>
        public MemoryIndexTree ChangedIndexesTree { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotContext"/> class.
        /// </summary>
        /// <param name="sourceSnapshot">The source snapshot.</param>
        public SnapshotContext(Snapshot sourceSnapshot)
        {
            this.SourceSnapshot = sourceSnapshot;
        }
    }

    /// <summary>
    /// A wrapper class which contains information connected with some container index.
    /// Instance is used when merge algorithm merges two distinct memory containers - 
    /// variables, array indexes, object fields.
    /// </summary>
    public class ContainerContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerContext"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="indexContainer">The index container.</param>
        /// <param name="operationType">Type of the operation.</param>
        public ContainerContext(SnapshotContext context, IReadonlyIndexContainer indexContainer, MergeOperationType operationType = MergeOperationType.ChangedOnly)
        {
            this.SnapshotContext = context;
            this.IndexContainer = indexContainer;
            this.OperationType = operationType;
        }

        /// <summary>
        /// Gets or sets the index container.
        /// </summary>
        /// <value>
        /// The index container.
        /// </value>
        public IReadonlyIndexContainer IndexContainer { get; set; }

        /// <summary>
        /// Gets or sets the snapshot context.
        /// </summary>
        /// <value>
        /// The snapshot context.
        /// </value>
        public SnapshotContext SnapshotContext { get; set; }

        /// <summary>
        /// Gets or sets the type of the operation.
        /// </summary>
        /// <value>
        /// The type of the operation.
        /// </value>
        public MergeOperationType OperationType { get; set; }
    }

    /// <summary>
    /// Represents a wrapper which holds snapshot context for merged memory index. 
    /// Stored within the merge operation.
    /// </summary>
    public class MergeOperationContext
    {
        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public MemoryIndex Index { get; set; }

        /// <summary>
        /// Gets or sets the snapshot context.
        /// </summary>
        /// <value>
        /// The snapshot context.
        /// </value>
        public SnapshotContext SnapshotContext { get; set; }

        /// <summary>
        /// Gets or sets the type of the operation.
        /// </summary>
        /// <value>
        /// The type of the operation.
        /// </value>
        public MergeOperationType OperationType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeOperationContext"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="snapshotContext">The snapshot context.</param>
        /// <param name="operationType">Type of the operation.</param>
        public MergeOperationContext(MemoryIndex index, SnapshotContext snapshotContext, MergeOperationType operationType = MergeOperationType.ChangedOnly)
        {
            this.Index = index;
            this.SnapshotContext = snapshotContext;
            this.OperationType = operationType;
        }
    }

    /// <summary>
    /// Inteface for objects mirroring specific container operations to the rest 
    /// of the merge algorithm.
    /// </summary>
    public interface ITargetContainerContext
    {
        /// <summary>
        /// Gets the source container.
        /// </summary>
        /// <returns>Readonly version of the container.</returns>
        IReadonlyIndexContainer getSourceContainer();

        /// <summary>
        /// Gets the writeable source container.
        /// </summary>
        /// <returns>Writeable version of source container</returns>
        IWriteableIndexContainer getWriteableSourceContainer();

        /// <summary>
        /// Creates the memory index in the container.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Created index.</returns>
        MemoryIndex createMemoryIndex(string name);
    }

    /// <summary>
    /// Implements specific operations for an object container.
    /// </summary>
    public class ObjectTargetContainerContext : ITargetContainerContext
    {
        private IObjectDescriptor objectDescriptor;
        private IObjectDescriptorBuilder builder;
        private IWriteableSnapshotStructure structure;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectTargetContainerContext"/> class.
        /// </summary>
        /// <param name="structure">The structure.</param>
        /// <param name="objectDescriptor">The object descriptor.</param>
        public ObjectTargetContainerContext(IWriteableSnapshotStructure structure, IObjectDescriptor objectDescriptor)
        {
            this.objectDescriptor = objectDescriptor;
            this.structure = structure;
        }

        /// <inheritdoc />
        public IReadonlyIndexContainer getSourceContainer()
        {
            return objectDescriptor;
        }

        /// <inheritdoc />
        public IWriteableIndexContainer getWriteableSourceContainer()
        {
            if (builder == null)
            {
                builder = objectDescriptor.Builder(structure);
            }

            return builder;
        }

        /// <summary>
        /// Gets the current descriptor.
        /// </summary>
        /// <returns>Descriptor</returns>
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


        /// <inheritdoc />
        public MemoryIndex createMemoryIndex(string name)
        {
            return ObjectIndex.Create(objectDescriptor.ObjectValue, name);
        }
    }

    /// <summary>
    /// Implements specific operations for a variable container.
    /// </summary>
    public class VariableTargetContainerContext : ITargetContainerContext
    {
        private IReadOnlySnapshotStructure targetStructure;
        private IWriteableSnapshotStructure writeabletargetStructure;
        private int stackLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableTargetContainerContext"/> class.
        /// </summary>
        /// <param name="targetStructure">The target structure.</param>
        /// <param name="writeabletargetStructure">The writeabletarget structure.</param>
        /// <param name="stackLevel">The stack level.</param>
        public VariableTargetContainerContext(IReadOnlySnapshotStructure targetStructure, IWriteableSnapshotStructure writeabletargetStructure, int stackLevel)
        {
            this.targetStructure = targetStructure;
            this.writeabletargetStructure = writeabletargetStructure;
            this.stackLevel = stackLevel;
        }


        /// <inheritdoc />
        public IReadonlyIndexContainer getSourceContainer()
        {
            return targetStructure.GetReadonlyStackContext(stackLevel).ReadonlyVariables;
        }

        /// <inheritdoc />
        public IWriteableIndexContainer getWriteableSourceContainer()
        {
            return writeabletargetStructure.GetWriteableStackContext(stackLevel).WriteableVariables;
        }

        /// <inheritdoc />
        public MemoryIndex createMemoryIndex(string name)
        {
            return VariableIndex.Create(name, stackLevel);
        }
    }

    /// <summary>
    /// Implements specific operations for a control variable container.
    /// </summary>
    public class ControllVariableTargetContainerContext : ITargetContainerContext
    {
        private IReadOnlySnapshotStructure targetStructure;
        private IWriteableSnapshotStructure writeabletargetStructure;
        private int stackLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControllVariableTargetContainerContext"/> class.
        /// </summary>
        /// <param name="targetStructure">The target structure.</param>
        /// <param name="writeabletargetStructure">The writeabletarget structure.</param>
        /// <param name="stackLevel">The stack level.</param>
        public ControllVariableTargetContainerContext(IReadOnlySnapshotStructure targetStructure, IWriteableSnapshotStructure writeabletargetStructure, int stackLevel)
        {
            this.targetStructure = targetStructure;
            this.writeabletargetStructure = writeabletargetStructure;
            this.stackLevel = stackLevel;
        }


        /// <inheritdoc />
        public IReadonlyIndexContainer getSourceContainer()
        {
            return targetStructure.GetReadonlyStackContext(stackLevel).ReadonlyControllVariables;
        }

        /// <inheritdoc />
        public IWriteableIndexContainer getWriteableSourceContainer()
        {
            return writeabletargetStructure.GetWriteableStackContext(stackLevel).WriteableControllVariables;
        }

        /// <inheritdoc />
        public MemoryIndex createMemoryIndex(string name)
        {
            return ControlIndex.Create(name, stackLevel);
        }
    }

    /// <summary>
    /// Implements specific operations for an array container.
    /// </summary>
    public class ArrayTargetContainerContext : ITargetContainerContext
    {
        private IArrayDescriptor arrayDescriptor;
        private IArrayDescriptorBuilder builder;
        private IWriteableSnapshotStructure strucure;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayTargetContainerContext"/> class.
        /// </summary>
        /// <param name="strucure">The strucure.</param>
        /// <param name="arrayDescriptor">The array descriptor.</param>
        public ArrayTargetContainerContext(IWriteableSnapshotStructure strucure, IArrayDescriptor arrayDescriptor)
        {
            this.arrayDescriptor = arrayDescriptor;
            this.strucure = strucure;
        }

        /// <inheritdoc />
        public IReadonlyIndexContainer getSourceContainer()
        {
            return arrayDescriptor;
        }

        /// <inheritdoc />
        public IWriteableIndexContainer getWriteableSourceContainer()
        {
            if (builder == null)
            {
                builder = arrayDescriptor.Builder(strucure);
            }

            return builder;
        }

        /// <summary>
        /// Gets the current descriptor.
        /// </summary>
        /// <returns>Descriptor</returns>
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


        /// <inheritdoc />
        public MemoryIndex createMemoryIndex(string name)
        {
            return arrayDescriptor.ParentIndex.CreateIndex(name);
        }
    }
}
