using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.IndexCollectors
{
    /// <summary>
    /// Process given memory entry and creates list of ValueLocation to acces indexes on non array values.
    /// </summary>
    class CollectIndexValueVisitor : AbstractValueVisitor
    {
        private MemberIdentifier index;
        private IndexPathSegment indexSegment;
        private TreeIndexCollector treeIndexCollector;
        private CollectorNode node;
        private bool isMust;

        public CollectIndexValueVisitor(IndexPathSegment indexSegment, TreeIndexCollector treeIndexCollector, CollectorNode node, bool isMust)
        {
            this.indexSegment = indexSegment;
            this.treeIndexCollector = treeIndexCollector;
            this.node = node;
            this.isMust = isMust;

            index = new MemberIdentifier(indexSegment.Names);
        }

        private void AddLocation(ValueLocation location)
        {
            CollectorNode nextNode = node.CreateValueChild(location);
            nextNode.IsMust = isMust;
            treeIndexCollector.AddNode(nextNode);
        }

        public override void VisitValue(Value value)
        {
            this.AddLocation(new ArrayValueLocation(null, index, value));
        }

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            this.AddLocation(new ArrayUndefinedValueLocation(null, index, value));
        }

        public override void VisitAnyArrayValue(AnyArrayValue value)
        {
            this.AddLocation(new ArrayAnyValueLocation(null, index, value));
        }

        public override void VisitAnyValue(AnyValue value)
        {
            this.AddLocation(new ArrayAnyValueLocation(null, index, value));
        }

        public override void VisitStringValue(StringValue value)
        {
            this.AddLocation(new ArrayStringValueLocation(null, index, value));
        }

        public override void VisitAnyStringValue(AnyStringValue value)
        {
            this.AddLocation(new AnyStringValueLocation(null, index, value));
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            // Skip visiting array value - already processed
        }

        public override void VisitInfoValue(InfoValue value)
        {
            this.AddLocation(new InfoValueLocation(null, value));
        }
    }

    /// <summary>
    /// Process given memory entry and creates list of ValueLocation to acces fields on non object values.
    /// </summary>
    class CollectFieldValueVisitor : AbstractValueVisitor
    {
        private VariableIdentifier index;
        private FieldPathSegment fieldSegment;
        private TreeIndexCollector treeIndexCollector;
        private CollectorNode node;
        private bool isMust;

        public CollectFieldValueVisitor(FieldPathSegment fieldSegment, TreeIndexCollector treeIndexCollector, CollectorNode node, bool isMust)
        {
            this.fieldSegment = fieldSegment;
            this.treeIndexCollector = treeIndexCollector;
            this.node = node;
            this.isMust = isMust;

            index = new VariableIdentifier(fieldSegment.Names);
        }

        private void AddLocation(ValueLocation location)
        {
            CollectorNode nextNode = node.CreateValueChild(location);
            nextNode.IsMust = isMust;
            treeIndexCollector.AddNode(nextNode);
        }

        public override void VisitValue(Value value)
        {
            this.AddLocation(new ObjectValueLocation(null, index, value));
        }

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            this.AddLocation(new ObjectUndefinedValueLocation(null, index, value));
        }

        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            this.AddLocation(new ObjectAnyValueLocation(null, index, value));
        }

        public override void VisitAnyValue(AnyValue value)
        {
            this.AddLocation(new ObjectAnyValueLocation(null, index, value));
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            // Skip visiting object value - already processed
        }

        public override void VisitInfoValue(InfoValue value)
        {
            this.AddLocation(new InfoValueLocation(null, value));
        }
    }
}
