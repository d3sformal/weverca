using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;

namespace Weverca.MemoryModels.ModularCopyMemoryModel
{
    public class ModularMemoryModelFactory : MemoryModelFactory
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
        public AlgorithmFactories MemoryAlgorithmFactories { get; private set; }

        /// <summary>
        /// Gets the algorithm factories for info phase.
        /// </summary>
        /// <value>
        /// The algorithm factories for info phase.
        /// </value>
        public AlgorithmFactories InfoAlgorithmFactories { get; private set; }


        private Snapshot initialSnapshot;

        internal ISnapshotStructureProxy InitialSnapshotStructureInstance { get; private set; }

        internal ISnapshotDataProxy InitialSnapshotDataInstance { get; private set; }



        /// <summary>
        /// Initializes a new instance of the <see cref="ModularMemoryModelFactory"/> class from builder instance.
        /// </summary>
        /// <param name="builder">The modular memory model factory builder.</param>
        internal ModularMemoryModelFactory(ModularMemoryModelFactoryBuilder builder)
        {
            SnapshotStructureFactory = builder.SnapshotStructureFactory;
            StructuralContainersFactories = builder.StructuralContainersFactories;
            SnapshotDataFactory = builder.SnapshotDataFactory;
            MemoryAlgorithmFactories = builder.MemoryAlgorithmFactories;
            InfoAlgorithmFactories = builder.InfoAlgorithmFactories;

            initialSnapshot = new Snapshot(this);
            InitialSnapshotStructureInstance = SnapshotStructureFactory.CreateGlobalContextInstance(initialSnapshot);
            InitialSnapshotDataInstance = SnapshotDataFactory.CreateEmptyInstance(initialSnapshot);
        }

        /// <inheritdoc />
        public SnapshotBase CreateSnapshot()
        {
            Snapshot snapshot = new Snapshot(this);
            snapshot.InitializeMemoryModel();

            return snapshot;
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
        public AlgorithmFactories GetAlgorithmFactories(SnapshotMode snapshotMode)
        {
            switch (snapshotMode)
            {
                case SnapshotMode.MemoryLevel:
                    return MemoryAlgorithmFactories;

                case SnapshotMode.InfoLevel:
                    return InfoAlgorithmFactories;

                default:
                    throw new NotSupportedException("Unsupported snapshot mode: " + snapshotMode);
            }
        }
    }

    public class ModularMemoryModelFactoryBuilder
    {
        /// <summary>
        /// Gets or sets the snapshot structure factory.
        /// </summary>
        /// <value>
        /// The snapshot structure factory.
        /// </value>
        public ISnapshotStructureFactory SnapshotStructureFactory { get; set; }

        public StructuralContainersFactories StructuralContainersFactories { get; set; }

        /// <summary>
        /// Gets or sets the snapshot data factory.
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
        /// Sets the algorithm factories for memory and info phase. Both phases will have the same algorithm factories object.
        /// </summary>
        /// <value>
        /// The algorithm factories.
        /// </value>
        public AlgorithmFactories AlgorithmFactories
        {
            set
            {
                MemoryAlgorithmFactories = value;
                InfoAlgorithmFactories = value;
            }
        }

        /// <summary>
        /// Creates new impementation variant object with all data stored in this builder instance.
        /// </summary>
        /// <returns>New implementation variant object.</returns>
        public ModularMemoryModelFactory Build()
        {
            return new ModularMemoryModelFactory(this);
        }
    }
}
