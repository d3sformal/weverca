using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.IndexCollectors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Utils;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.MemoryWorkers.Assign
{
    class AssignWorker : AbstractAssignWorker
    {
        public bool ForceStrongWrite { get; set; }

        public bool AssignAliasesIntoCollectedIndexes { get; set; }



        private MemoryEntryCollector memoryEntryCollector;
        private LinkedList<AssignOperation> operationQueue = new LinkedList<AssignOperation>();
        private AssignValueLocationVisitor valueLocationVisitor;

        public AssignWorker(ModularMemoryModelFactories factories, Snapshot snapshot, MemoryEntryCollector memoryEntryCollector,
            TreeIndexCollector treeCollector, MemoryIndexModificationList pathModifications)
            : base(factories, snapshot, treeCollector, pathModifications)
        {
            this.memoryEntryCollector = memoryEntryCollector;

            ForceStrongWrite = false;
            AssignAliasesIntoCollectedIndexes = false;
        }

        public void Assign()
        {
            valueLocationVisitor = new AssignValueLocationVisitor(Snapshot, memoryEntryCollector.RootMemoryEntry, true);

            ProcessCollector();

            while (operationQueue.Count > 0)
            {
                AssignOperation operation = operationQueue.First.Value;
                operationQueue.RemoveFirst();

                operation.ProcessOperation();
            }
        }

        protected override void collectValueNode(ValueCollectorNode node)
        {
            MemoryIndexModification modification = PathModifications.GetOrCreateModification(node.TargetIndex);

            if (node.IsMust || ForceStrongWrite)
            {
                modification.SetCollectedIndex();

                valueLocationVisitor.IsMust = true;
                node.ValueLocation.ContainingIndex = node.TargetIndex;

                node.ValueLocation.Accept(valueLocationVisitor);
            }
            else
            {
                modification.SetCollectedIndex();

                valueLocationVisitor.IsMust = false;
                node.ValueLocation.ContainingIndex = node.TargetIndex;

                node.ValueLocation.Accept(valueLocationVisitor);
            }
        }

        protected override void collectMemoryIndexCollectorNode(MemoryIndexCollectorNode node)
        {
            PathModifications.GetOrCreateModification(node.TargetIndex).SetCollectedIndex();

            if (node.IsMust || ForceStrongWrite)
            {
                AddOperation(new MemoryIndexMustAssignOperation(this, node.TargetIndex, memoryEntryCollector.RootNode, AssignAliasesIntoCollectedIndexes));
            }
            else
            {
                AddOperation(new MemoryIndexMayAssignOperation(this, node.TargetIndex, memoryEntryCollector.RootNode, AssignAliasesIntoCollectedIndexes));
            }
        }

        protected override void collectUnknownIndexCollectorNode(UnknownIndexCollectorNode node)
        {
            PathModifications.GetOrCreateModification(node.TargetIndex).SetCollectedIndex();

            if (node.IsMust || ForceStrongWrite)
            {
                AddOperation(new UndefinedMustAssignOperation(this, node.TargetIndex, memoryEntryCollector.RootNode, AssignAliasesIntoCollectedIndexes));
            }
            else
            {
                AddOperation(new UnknownIndexMayAssign(this, node.SourceIndex, node.TargetIndex, memoryEntryCollector.RootNode, AssignAliasesIntoCollectedIndexes));
            }
        }

        protected override void collectUndefinedCollectorNode(UndefinedCollectorNode node)
        {
            PathModifications.GetOrCreateModification(node.TargetIndex).SetCollectedIndex();

            if (node.IsMust || ForceStrongWrite)
            {
                AddOperation(new UndefinedMustAssignOperation(this, node.TargetIndex, memoryEntryCollector.RootNode, AssignAliasesIntoCollectedIndexes));
            }
            else
            {
                AddOperation(new UndefinedMayAssignOperation(this, node.TargetIndex, memoryEntryCollector.RootNode, AssignAliasesIntoCollectedIndexes));
            }
        }

        internal void AddOperation(AssignOperation operation)
        {
            operationQueue.AddLast(operation);
        }
    }
}
