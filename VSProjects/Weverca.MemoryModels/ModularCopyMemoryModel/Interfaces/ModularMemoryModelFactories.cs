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
using Weverca.MemoryModels.ModularCopyMemoryModel.Logging;

namespace Weverca.MemoryModels.ModularCopyMemoryModel
{
    /// <summary>
    /// This class holds instances of all factories for all modules of the modular memory model.
    /// It also contains a factory of modular memory model snapshot which can be passed to analysis.
    /// Created snapshot will receive an instance of this class which should be use to obtain all
    /// factories and an algorithm instances.
    /// 
    /// This is an imutable class. For modifications use ModularMemoryModelFactoriesBuilder.
    /// </summary>
    public class ModularMemoryModelFactories
    {
        /// <summary>
        /// Gets the main factory of modular memory model. This factory creates the instances of
        /// the snapshot class. Can be passed to the analyzer.
        /// </summary>
        /// <value>
        /// The memory model snapshot factory.
        /// </value>
        public MemoryModelFactory MemoryModelSnapshotFactory { get; private set; }

        /// <summary>
        /// Gets the snapshot structure factory. This factory has to be used by snapshot to obtain
        /// an instances of the snapshot structure object.
        /// </summary>
        /// <value>
        /// The snapshot structure factory.
        /// </value>
        public ISnapshotStructureFactory SnapshotStructureFactory { get; private set; }

        /// <summary>
        /// Gets the structural containers factories. This instance has to be used by snapshot, algorithms
        /// and structure container to obtain representation of the inner memory objects.
        /// </summary>
        /// <value>
        /// The structural containers factories.
        /// </value>
        public StructuralContainersFactories StructuralContainersFactories { get; private set; }

        /// <summary>
        /// Gets the snapshot data factory. This instance is used by the snapshot to obtain an instance
        /// of snapshot data container.
        /// </summary>
        /// <value>
        /// The snapshot data factory.
        /// </value>
        public ISnapshotDataFactory SnapshotDataFactory { get; private set; }
        
        /// <summary>
        /// Gets the algorithm singletons for memory phase.
        /// </summary>
        /// <value>
        /// The algorithm singletons for memory phase.
        /// </value>
        public AlgorithmInstances MemoryAlgorithms { get; private set; }

        /// <summary>
        /// Gets the algorithm singletons for info phase.
        /// </summary>
        /// <value>
        /// The algorithm singletons for info phase.
        /// </value>
        public AlgorithmInstances InfoAlgorithms { get; private set; }
        
        /// <summary>
        /// Gets the algorithm factories for memory phase.
        /// </summary>
        /// <value>
        /// The algorithm factories for memory phase.
        /// </value>
        public AlgorithmFactories MemoryAlgorithmFactories { get; private set; }

        /// <summary>
        /// Gets the algorithm factories for info phase.
        /// </summary>
        /// <value>
        /// The algorithm factories for info phase.
        /// </value>
        public AlgorithmFactories InfoAlgorithmFactories { get; private set; }

        /// <summary>
        /// Gets the benchmark singleton.
        /// </summary>
        /// <value>
        /// The benchmark singleton.
        /// </value>
        public IBenchmark Benchmark { get; private set; }

        /// <summary>
        /// Gets the logger singleton.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public ILogger Logger { get; private set; }
        
        /// <summary>
        /// Gets the snapshot structure instance which can be used as common ancestor for all structures
        /// </summary>
        /// <value>
        /// The initial snapshot structure instance.
        /// </value>
        public ISnapshotStructureProxy InitialSnapshotStructureInstance { get; private set; }

        /// <summary>
        /// Gets the snapshot data instance for the memory phase which can be used as common 
        /// ancestor for all memory data instances
        /// </summary>
        /// <value>
        /// The initial snapshot data instance.
        /// </value>
        public ISnapshotDataProxy InitialSnapshotDataInstance { get; private set; }

        /// <summary>
        /// Gets the snapshot data instance for the info phase which can be used as common 
        /// ancestor for all info data instances
        /// </summary>
        /// <value>
        /// The initial snapshot data instance.
        /// </value>
        public ISnapshotDataProxy InitialSnapshotInfoInstance { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModularMemoryModelFactories" /> class.
        /// </summary>
        /// <param name="snapshotStructureFactory">The snapshot structure factory.</param>
        /// <param name="structuralContainersFactories">The structural containers factories.</param>
        /// <param name="snapshotDataFactory">The snapshot data factory.</param>
        /// <param name="memoryAlgorithmFactories">The memory algorithm factories.</param>
        /// <param name="infoAlgorithmFactories">The information algorithm factories.</param>
        /// <param name="benchmark">The benchmark.</param>
        /// <param name="logger">The logger.</param>
        internal ModularMemoryModelFactories(
            ISnapshotStructureFactory snapshotStructureFactory,
            StructuralContainersFactories structuralContainersFactories,
            ISnapshotDataFactory snapshotDataFactory,
            AlgorithmFactories memoryAlgorithmFactories,
            AlgorithmFactories infoAlgorithmFactories,
            IBenchmark benchmark,
            ILogger logger
            )
        {
            SnapshotStructureFactory = snapshotStructureFactory;
            StructuralContainersFactories = structuralContainersFactories;
            SnapshotDataFactory = snapshotDataFactory;
            MemoryAlgorithmFactories = memoryAlgorithmFactories;
            InfoAlgorithmFactories = infoAlgorithmFactories;
            Benchmark = benchmark;
            Logger = logger;

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

        /// <summary>
        /// Create a builder instance to create new factory object based on current values.
        /// </summary>
        /// <returns>New builder instance with all values from this instance</returns>
        public ModularMemoryModelFactoriesBuilder Builder()
        {
            return new ModularMemoryModelFactoriesBuilder(this);
        }

        /// <summary>
        /// Tthe main factory of modular memory model. This factory creates the instances of
        /// the snapshot class. Can be passed to the analyzer.
        /// 
        /// Create method passes an istance of the enclosing ModularMemoryModelFactories to the created
        /// snapshot.
        /// </summary>
        public class ModularMemoryModelSnapshotFactory : MemoryModelFactory
        {
            private ModularMemoryModelFactories factories;

            /// <summary>
            /// Initializes a new instance of the <see cref="ModularMemoryModelSnapshotFactory"/> class.
            /// </summary>
            /// <param name="factories">The factories which will be passed to the new object.</param>
            public ModularMemoryModelSnapshotFactory(ModularMemoryModelFactories factories)
            {
                this.factories = factories;
            }

            /// <inheritdoc />
            public SnapshotBase CreateSnapshot()
            {
                return new Snapshot(factories);
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return "Modular copy memory model";
            }
        }
    }

    /// <summary>
    /// Builder class for ModularMemoryModelFactories.
    /// </summary>
    public class ModularMemoryModelFactoriesBuilder
    {
        /// <summary>
        /// Gets or sets the snapshot structure factory. This factory has to be used by snapshot to obtain
        /// an instances of the snapshot structure object.
        /// </summary>
        /// <value>
        /// The snapshot structure factory.
        /// </value>
        public ISnapshotStructureFactory SnapshotStructureFactory { get; set; }

        /// <summary>
        /// Gets or sets the structural containers factories. This instance has to be used by snapshot, algorithms
        /// and structure container to obtain representation of the inner memory objects.
        /// </summary>
        /// <value>
        /// The structural containers factories.
        /// </value>
        public StructuralContainersFactories StructuralContainersFactories { get; set; }

        /// <summary>
        /// Gets or sets the snapshot data factory. This instance is used by the snapshot to obtain an instance
        /// of snapshot data container.
        /// </summary>
        /// <value>
        /// The snapshot data factory.
        /// </value>
        public ISnapshotDataFactory SnapshotDataFactory { get; set; }

        /// <summary>
        /// Gets or sets the algorithm factories for memory phase.
        /// </summary>
        /// <value>
        /// The algorithm factories for memory phase.
        /// </value>
        public AlgorithmFactories MemoryAlgorithmFactories { get; set; }

        /// <summary>
        /// Gets or sets the algorithm factories for info phase.
        /// </summary>
        /// <value>
        /// The algorithm factories for info phase.
        /// </value>
        public AlgorithmFactories InfoAlgorithmFactories { get; set; }
        
        /// <summary>
        /// Gets or sets the benchmark singleton.
        /// </summary>
        /// <value>
        /// The benchmark singleton.
        /// </value>
        public IBenchmark Benchmark { get; set; }

        /// <summary>
        /// Gets or sets the logger singleton.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModularMemoryModelFactoriesBuilder"/> class.
        /// </summary>
        /// <param name="factories">Internal properties of this instaince will be filled by the same
        /// propertis from given factory instance.</param>
        public ModularMemoryModelFactoriesBuilder(ModularMemoryModelFactories factories)
        {
            SnapshotStructureFactory = factories.SnapshotStructureFactory;
            StructuralContainersFactories = factories.StructuralContainersFactories;
            SnapshotDataFactory = factories.SnapshotDataFactory;
            MemoryAlgorithmFactories = factories.MemoryAlgorithmFactories;
            InfoAlgorithmFactories = factories.InfoAlgorithmFactories;
            Benchmark = factories.Benchmark;
            Logger = factories.Logger;
        }

        /// <summary>
        /// Creates new instance of the ModularMemoryModelFactories class with the date 
        /// from this builder instance.
        /// </summary>
        /// <returns>New instance of ModularMemoryModelFactories.</returns>
        public ModularMemoryModelFactories Build()
        {
            return new ModularMemoryModelFactories(
                SnapshotStructureFactory,
                StructuralContainersFactories,
                SnapshotDataFactory,
                MemoryAlgorithmFactories,
                InfoAlgorithmFactories,
                Benchmark,
                Logger
                );
        }
    }
}
