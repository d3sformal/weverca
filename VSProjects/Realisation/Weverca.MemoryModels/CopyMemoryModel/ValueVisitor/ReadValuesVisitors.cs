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
        public bool ContainsUndefinedValue { get; set; }
        public bool ContainsDefinedValue { get; set; }
        public bool ContainsArrayValue { get; set; }
        public bool ContainsAnyValue { get; set; }

        private MemberIdentifier index;
        private MemoryIndex containingIndex;
        private ICollection<CollectedLocation> locations;

        public ReadIndexVisitor(MemoryIndex containingIndex, IndexPathSegment indexSegment, ICollection<CollectedLocation> locations)
        {
            this.containingIndex = containingIndex;
            this.locations = locations;

            ContainsUndefinedValue = false;
            ContainsDefinedValue = false;
            ContainsArrayValue = false;
            ContainsAnyValue = false;

            index = new MemberIdentifier(indexSegment.Names);
        }

        public override void VisitValue(Value value)
        {
            ContainsDefinedValue = true;
            locations.Add(new ArrayValueLocation(containingIndex, index, value));
        }

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            ContainsUndefinedValue = true;
            locations.Add(new ArrayUndefinedValueLocation(containingIndex, index, value));
        }

        public override void VisitAnyArrayValue(AnyArrayValue value)
        {
            ContainsAnyValue = true;
            locations.Add(new ArrayAnyValueLocation(containingIndex, index, value));
        }

        public override void VisitAnyValue(AnyValue value)
        {
            ContainsAnyValue = true;
            locations.Add(new ArrayAnyValueLocation(containingIndex, index, value));
        }

        public override void VisitStringValue(StringValue value)
        {
            ContainsDefinedValue = true;
            locations.Add(new ArrayStringValueLocation(containingIndex, index, value));
        }

        public override void VisitAnyStringValue(AnyStringValue value)
        {
            ContainsDefinedValue = true;
            locations.Add(new AnyStringValueLocation(containingIndex, index, value));
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            ContainsArrayValue = true;
        }

        public override void VisitInfoValue(InfoValue value)
        {
            locations.Add(new InfoValueLocation(containingIndex, value));
        }
    }

    class ReadFieldVisitor : AbstractValueVisitor
    {
        private VariableIdentifier index;
        private MemoryIndex containingIndex;
        private ICollection<CollectedLocation> locations;

        public bool ContainsUndefinedValue { get; set; }
        public bool ContainsDefinedValue { get; set; }
        public bool ContainsObjectValue { get; set; }
        public bool ContainsAnyValue { get; set; }

        public ReadFieldVisitor(MemoryIndex containingIndex, FieldPathSegment fieldSegment, ICollection<CollectedLocation> locations)
        {
            this.containingIndex = containingIndex;
            this.locations = locations;

            ContainsUndefinedValue = false;
            ContainsDefinedValue = false;
            ContainsObjectValue = false;
            ContainsAnyValue = false;

            index = new VariableIdentifier(fieldSegment.Names);
        }

        public override void VisitValue(Value value)
        {
            ContainsDefinedValue = true;
            locations.Add(new ObjectValueLocation(containingIndex, index, value));
        }

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            ContainsUndefinedValue = true;
            locations.Add(new ObjectUndefinedValueLocation(containingIndex, index, value));
        }

        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            ContainsAnyValue = true;
            locations.Add(new ObjectAnyValueLocation(containingIndex, index, value));
        }

        public override void VisitAnyValue(AnyValue value)
        {
            ContainsAnyValue = true;
            locations.Add(new ObjectAnyValueLocation(containingIndex, index, value));
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            ContainsObjectValue = true;
        }

        public override void VisitInfoValue(InfoValue value)
        {
            locations.Add(new InfoValueLocation(containingIndex, value));
        }
    }
}
