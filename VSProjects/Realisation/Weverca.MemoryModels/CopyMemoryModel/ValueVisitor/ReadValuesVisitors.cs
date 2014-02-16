using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class ReadIndexVisitor : AbstractValueVisitor
    {
        public bool ContainsUndefined { get; set; }

        private MemberIdentifier index;
        private MemoryIndex containingIndex;
        private ICollection<CollectedLocation> locations;

        public ReadIndexVisitor(MemoryIndex containingIndex, IndexPathSegment indexSegment, ICollection<CollectedLocation> locations)
        {
            this.containingIndex = containingIndex;
            this.locations = locations;

            ContainsUndefined = false;

            index = new MemberIdentifier(indexSegment.Names);
        }

        public override void VisitValue(Value value)
        {
            locations.Add(new ArrayValueLocation(containingIndex, index, value));
        }

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            ContainsUndefined = true;
            locations.Add(new ArrayUndefinedValueLocation(containingIndex, index, value));
        }

        public override void VisitAnyArrayValue(AnyArrayValue value)
        {
            locations.Add(new ArrayAnyValueLocation(containingIndex, index, value));
        }

        public override void VisitAnyValue(AnyValue value)
        {
            locations.Add(new ArrayAnyValueLocation(containingIndex, index, value));
        }

        public override void VisitStringValue(StringValue value)
        {
            locations.Add(new ArrayStringValueLocation(containingIndex, index, value));
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            // Do nothing
        }
    }

    class ReadFieldVisitor : AbstractValueVisitor
    {
        private VariableIdentifier index;
        private MemoryIndex containingIndex;
        private ICollection<CollectedLocation> locations;

        public bool ContainsUndefined { get; set; }

        public ReadFieldVisitor(MemoryIndex containingIndex, FieldPathSegment fieldSegment, ICollection<CollectedLocation> locations)
        {
            this.containingIndex = containingIndex;
            this.locations = locations;

            ContainsUndefined = false;
            index = new VariableIdentifier(fieldSegment.Names);
        }

        public override void VisitValue(Value value)
        {
            locations.Add(new ObjectValueLocation(containingIndex, index, value));
        }

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            ContainsUndefined = true;
            locations.Add(new ObjectUndefinedValueLocation(containingIndex, index, value));
        }

        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            locations.Add(new ObjectAnyValueLocation(containingIndex, index, value));
        }

        public override void VisitAnyValue(AnyValue value)
        {
            locations.Add(new ObjectAnyValueLocation(containingIndex, index, value));
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            // Do nothing
        }
    }
}
