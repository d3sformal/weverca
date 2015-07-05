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
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyCopyStructure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;

namespace Weverca.MemoryModels.ModularCopyMemoryModel
{
    public class ModularMemoryModelVariants
    {
        public static ModularMemoryModelFactories DefaultVariant { get { return TrackingImplementation; } }


        public static readonly ModularMemoryModelFactories CopyImplementation = createCopyImplementation();

        public static readonly ModularMemoryModelFactories LazyImplementation = createLazyImplementation();

        public static readonly ModularMemoryModelFactories TrackingImplementation = createTrackingImplementation();

        
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
                    new CopyObjectValueContainerFactory()
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
                )
            );
        }

        private static ModularMemoryModelFactories createLazyImplementation()
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
                    new CopyObjectValueContainerFactory()
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
                )
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
                    new LazyCopyObjectValueContainerFactory()
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
                )

            );
        }
    }
}
