using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;

namespace Weverca.MemoryModels.ModularCopyMemoryModel
{
    public class ModularMemoryModelFactories
    {
        /// <summary>
        /// Gets the snapshot structure factory.
        /// </summary>
        /// <value>
        /// The snapshot structure factory.
        /// </value>
        public ISnapshotStructureFactory SnapshotStructureFactory { get; private set; }

        public StructuralContainersFactories StructuralContainersFactories { get; private set; }

        /// <summary>
        /// Gets the snapshot data factory.
        /// </summary>
        /// <value>
        /// The snapshot data factory.
        /// </value>
        public ISnapshotDataFactory SnapshotDataFactory { get; private set; }

        /// <summary>
        /// Gets the algorithm factories for memory phase.
        /// </summary>
        /// <value>
        /// The algorithm factories for memory phase.
        /// </value>
        public AlgorithmInstances MemoryAlgorithms { get; private set; }

        /// <summary>
        /// Gets the algorithm factories for info phase.
        /// </summary>
        /// <value>
        /// The algorithm factories for info phase.
        /// </value>
        public AlgorithmInstances InfoAlgorithms { get; private set; }

        public MemoryModelFactory MemoryModelSnapshotFactory { get; private set; }

        
        internal ISnapshotStructureProxy InitialSnapshotStructureInstance { get; private set; }
        internal ISnapshotDataProxy InitialSnapshotDataInstance { get; private set; }
        internal ISnapshotDataProxy InitialSnapshotInfoInstance { get; private set; }



        /// <summary>
        /// Initializes a new instance of the <see cref="ModularMemoryModelFactories"/> class from builder instance.
        /// </summary>
        /// <param name="builder">The modular memory model factory builder.</param>
        internal ModularMemoryModelFactories(
            ISnapshotStructureFactory snapshotStructureFactory,
            StructuralContainersFactories structuralContainersFactories,
            ISnapshotDataFactory snapshotDataFactory,
            AlgorithmFactories memoryAlgorithmFactories,
            AlgorithmFactories infoAlgorithmFactories
            )
        {
            SnapshotStructureFactory = snapshotStructureFactory;
            StructuralContainersFactories = structuralContainersFactories;
            SnapshotDataFactory = snapshotDataFactory;

            MemoryAlgorithms = memoryAlgorithmFactories.CreateInstances(this);
            InfoAlgorithms = infoAlgorithmFactories.CreateInstances(this);

            MemoryModelSnapshotFactory = new ModularMemoryModelSnapshotFactory(this);

            InitialSnapshotStructureInstance = SnapshotStructureFactory.CreateGlobalContextInstance(this);
            InitialSnapshotDataInstance = SnapshotDataFactory.CreateEmptyInstance(this);
            InitialSnapshotInfoInstance = SnapshotDataFactory.CreateEmptyInstance(this);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "Modular copy memory model";
        }

        /// <summary>
        /// Gets proper variant of algorithm factories for given snapshot mode.
        /// </summary>
        /// <param name="snapshotMode">The snapshot mode.</param>
        /// <returns>Reference to stored instance of algorithm factories.</returns>
        /// <exception cref="System.NotSupportedException">Unsupported snapshot mode</exception>
        public AlgorithmInstances GetAlgorithms(SnapshotMode snapshotMode)
        {
            switch (snapshotMode)
            {
                case SnapshotMode.MemoryLevel:
                    return MemoryAlgorithms;

                case SnapshotMode.InfoLevel:
                    return InfoAlgorithms;

                default:
                    throw new NotSupportedException("Unsupported snapshot mode: " + snapshotMode);
            }
        }

        public class ModularMemoryModelSnapshotFactory : MemoryModelFactory
        {
            private ModularMemoryModelFactories factories;

            public ModularMemoryModelSnapshotFactory(ModularMemoryModelFactories factories)
            {
                this.factories = factories;
            }

            /// <inheritdoc />
            public SnapshotBase CreateSnapshot()
            {
                return new Snapshot(factories);
            }
        }

    }
}
