using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.InfoPhase;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryPhase;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.InfoPhase;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.MemoryPhase;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.InfoPhase;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryPhase;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.CopyStructure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.DifferentialStructure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyStructure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Logging;

namespace Weverca.MemoryModels.ModularCopyMemoryModel
{
    /// <summary>
    /// Contains prepared instances of ModularMemoryModelFactories which contains all necesary 
    /// factories to proper work of the memory model. Each of prepared factories ilustrates
    /// some optimization applied to the mmeory model.
    /// 
    /// The sequence of variants is following:
    /// 
    ///	Copy – original implementation
    ///	Lazy algorithms – added basic laziness to reduce unnecessary copying during the extend 
    ///	                  and lazy variant of a commit algorithm
    ///	Lazy containers – added lazy behavior to inner containers
    ///	Tracking – added a change tracker and a parallel call stack with new tracking merge and 
    ///	           commit algorithms
    ///	Differential – added differential associative container to tracking algorithms
    /// </summary>
    public class ModularMemoryModelVariants
    {
        /// <summary>
        /// Gets the default variant which will be used in the Weverca when the modular memory 
        /// model is selected.
        /// 
        /// Currently set to the mostrecent variant: Differential
        /// </summary>
        /// <value>
        /// The default variant.
        /// </value>
        public static ModularMemoryModelFactories DefaultVariant { get { return TrackingDiffContainers; } }

        /// <summary>
        /// A copy implementation contains memory structure and algorithms based on the original 
        /// copy memory model. This variant is very inefficient and is used only to illustrate 
        /// the difference between the original and the new memory model.
        /// </summary>
        public static readonly ModularMemoryModelFactories Copy = createCopyImplementation();

        /// <summary>
        /// Improves original copy implementation by adding lazy behavior to memory containers and 
        /// commit algorithm
        /// </summary>
        public static readonly ModularMemoryModelFactories LazyExtendCommit = createLazyExtendCommitImplementation();

        /// <summary>
        /// The same as LazyExtendCommit but all inner containers were re-implemented to add lazy 
        /// behavior to their internal storages.
        /// </summary>
        public static readonly ModularMemoryModelFactories LazyContainers = createLazyContainersImplementation();
        
        /// <summary>
        /// Improves LazyContainers by adding change tracker and parallel call stack. Merge, commit
        /// and assign algorithm is reimplemented to be more efficient.
        /// </summary>
        public static readonly ModularMemoryModelFactories Tracking = createTrackingImplementation();

        /// <summary>
        /// Improves Tracking variant by using differential lazy container in the inner structure.
        /// 
        /// This is the most efficient variant. Currently set as default.
        /// </summary>
        public static readonly ModularMemoryModelFactories TrackingDiffContainers = createTrackingDiffContainersImplementation();

        
        private static ModularMemoryModelFactories createCopyImplementation()
        {
            return new ModularMemoryModelFactories(
                new CopySnapshotStructureFactory(),

                new StructuralContainersFactories(
                    new CopyArrayDescriptorFactory(),
                    new CopyIndexContainerFactory(),
                    new CopyIndexDefinitionFactory(),
                    new CopyMemoryAliasFactory(),
                    new CopyStackContextFactory(),
                    new CopyObjectDescriptorFactory(),
                    new CopyObjectValueContainerFactory(),
                    new CopyDeclarationContainerFactory(),
                    new CopyDictionaryAssociativeContainerFactory()
                    ),

                new CopySnapshotDataFactory(),

                new AlgorithmFactories(
                    new CopyAssignMemoryAlgorithmFactory(),
                    new CopyReadAlgorithmFactory(),
                    new CopyCommitMemoryAlgorithmFactory(),
                    new CopyMemoryAlgorithmFactory(),
                    new CopyMergeMemoryAlgorithmFactory(),
                    new PrintAlgorithmFactory()
                ),

                new AlgorithmFactories(
                    new CopyAssignInfoAlgorithmFactory(),
                    new CopyReadAlgorithmFactory(),
                    new CopyCommitInfoAlgorithmFactory(),
                    new CopyMemoryAlgorithmFactory(),
                    new CopyMergeInfoAlgorithmFactory(),
                    new PrintAlgorithmFactory()
                ),

                new EmptyMemoryModelBenchmark(),

                new EmptyMemoryModelLogger()
            );
        }

        private static ModularMemoryModelFactories createLazyExtendCommitImplementation()
        {
            return new ModularMemoryModelFactories(
                new LazyCopySnapshotStructureFactory(),

                new StructuralContainersFactories(
                    new CopyArrayDescriptorFactory(),
                    new CopyIndexContainerFactory(),
                    new CopyIndexDefinitionFactory(),
                    new CopyMemoryAliasFactory(),
                    new CopyStackContextFactory(),
                    new CopyObjectDescriptorFactory(),
                    new CopyObjectValueContainerFactory(),
                    new CopyDeclarationContainerFactory(),
                    new CopyDictionaryAssociativeContainerFactory()
                    ),

                new LazyCopySnapshotDataFactory(),

                new AlgorithmFactories(
                    new CopyAssignMemoryAlgorithmFactory(),
                    new CopyReadAlgorithmFactory(),
                    new LazyCommitMemoryAlgorithmFactory(),
                    new SimplifyingCopyMemoryAlgorithmFactory(),
                    new CopyMergeMemoryAlgorithmFactory(),
                    new PrintAlgorithmFactory()
                ),

                new AlgorithmFactories(
                    new CopyAssignInfoAlgorithmFactory(),
                    new CopyReadAlgorithmFactory(),
                    new LazyCommitInfoAlgorithmFactory(),
                    new SimplifyingCopyMemoryAlgorithmFactory(),
                    new CopyMergeInfoAlgorithmFactory(),
                    new PrintAlgorithmFactory()
                ),

                new EmptyMemoryModelBenchmark(),

                new EmptyMemoryModelLogger()
            );
        }

        private static ModularMemoryModelFactories createLazyContainersImplementation()
        {
            return new ModularMemoryModelFactories(
                new LazyCopySnapshotStructureFactory(),

                new StructuralContainersFactories(
                    new LazyCopyArrayDescriptorFactory(),
                    new LazyCopyIndexContainerFactory(),
                    new LazyCopyIndexDefinitionFactory(),
                    new LazyCopyMemoryAliasFactory(),
                    new LazyCopyStackContextFactory(),
                    new LazyCopyObjectDescriptorFactory(),
                    new LazyCopyObjectValueContainerFactory(),
                    new LazyCopyDeclarationContainerFactory(),
                    new LazyDictionaryAssociativeContainerFactory()
                    ),

                new LazyCopySnapshotDataFactory(),

                new AlgorithmFactories(
                    new CopyAssignMemoryAlgorithmFactory(),
                    new CopyReadAlgorithmFactory(),
                    new LazyCommitMemoryAlgorithmFactory(),
                    new SimplifyingCopyMemoryAlgorithmFactory(),
                    new CopyMergeMemoryAlgorithmFactory(),
                    new PrintAlgorithmFactory()
                ),

                new AlgorithmFactories(
                    new CopyAssignInfoAlgorithmFactory(),
                    new CopyReadAlgorithmFactory(),
                    new LazyCommitInfoAlgorithmFactory(),
                    new SimplifyingCopyMemoryAlgorithmFactory(),
                    new CopyMergeInfoAlgorithmFactory(),
                    new PrintAlgorithmFactory()
                ),

                new EmptyMemoryModelBenchmark(),

                new EmptyMemoryModelLogger()
            );
        }

        private static ModularMemoryModelFactories createTrackingImplementation()
        {
            return new ModularMemoryModelFactories(
                new TrackingSnapshotStructureFactory(),

                new StructuralContainersFactories(
                    new LazyCopyArrayDescriptorFactory(),
                    new LazyCopyIndexContainerFactory(),
                    new LazyCopyIndexDefinitionFactory(),
                    new LazyCopyMemoryAliasFactory(),
                    new LazyCopyStackContextFactory(),
                    new LazyCopyObjectDescriptorFactory(),
                    new LazyCopyObjectValueContainerFactory(),
                    new LazyCopyDeclarationContainerFactory(),
                    new LazyDictionaryAssociativeContainerFactory()
                    ),

                new TrackingSnapshotDataFactory(),

                new AlgorithmFactories(
                    new LazyAssignMemoryAlgorithmFactory(),
                    new CopyReadAlgorithmFactory(),
                    new TrackingCommitMemoryAlgorithmFactory(),
                    new SimplifyingCopyMemoryAlgorithmFactory(),
                    new TrackingMergeMemoryAlgorithmFactory(),
                    new PrintAlgorithmFactory()
                ),

                new AlgorithmFactories(
                    new LazyAssignInfoAlgorithmFactory(),
                    new CopyReadAlgorithmFactory(),
                    new TrackingCommitInfoAlgorithmFactory(),
                    new SimplifyingCopyMemoryAlgorithmFactory(),
                    new TrackingMergeInfoAlgorithmFactory(),
                    new PrintAlgorithmFactory()
                ),

                new EmptyMemoryModelBenchmark(),

                new EmptyMemoryModelLogger()

            );
        }

        private static ModularMemoryModelFactories createTrackingDiffContainersImplementation()
        {
            return new ModularMemoryModelFactories(
                new TrackingSnapshotStructureFactory(),

                new StructuralContainersFactories(
                    new LazyCopyArrayDescriptorFactory(),
                    new LazyCopyIndexContainerFactory(),
                    new LazyCopyIndexDefinitionFactory(),
                    new LazyCopyMemoryAliasFactory(),
                    new LazyCopyStackContextFactory(),
                    new LazyCopyObjectDescriptorFactory(),
                    new LazyCopyObjectValueContainerFactory(),
                    new LazyCopyDeclarationContainerFactory(),
                    new DifferentialDictionaryAssociativeContainerFactory()
                    ),

                new TrackingSnapshotDataFactory(),

                new AlgorithmFactories(
                    new LazyAssignMemoryAlgorithmFactory(),
                    new CopyReadAlgorithmFactory(),
                    new TrackingCommitMemoryAlgorithmFactory(),
                    new SimplifyingCopyMemoryAlgorithmFactory(),
                    new TrackingMergeMemoryAlgorithmFactory(),
                    new PrintAlgorithmFactory()
                ),

                new AlgorithmFactories(
                    new LazyAssignInfoAlgorithmFactory(),
                    new CopyReadAlgorithmFactory(),
                    new TrackingCommitInfoAlgorithmFactory(),
                    new SimplifyingCopyMemoryAlgorithmFactory(),
                    new TrackingMergeInfoAlgorithmFactory(),
                    new PrintAlgorithmFactory()
                ),

                new EmptyMemoryModelBenchmark(),

                new EmptyMemoryModelLogger()

            );
        }
    }
}
