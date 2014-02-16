using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    internal abstract class CollectedLocation
    {
        public abstract void Accept(ICollectedLocationVisitor visitor);

        public abstract IEnumerable<Value> ReadValues(MemoryAssistantBase assistant);
        public abstract IEnumerable<Value> WriteValues(MemoryAssistantBase assistant, MemoryEntry entry);
    }

    internal interface ICollectedLocationVisitor
    {
        void VisitObjectValueLocation(ObjectValueLocation location);
        void VisitObjectAnyValueLocation(ObjectAnyValueLocation location);

        void VisitArrayValueLocation(ArrayValueLocation location);
        void VisitArrayAnyValueLocation(ArrayAnyValueLocation location);
        void VisitArrayStringValueLocation(ArrayStringValueLocation location);


        void VisitArrayUndefinedValueLocation(ArrayUndefinedValueLocation location);

        void VisitObjectUndefinedValueLocation(ObjectUndefinedValueLocation location);
    }

    class ObjectValueLocation : CollectedLocation
    {
        private MemoryIndex containingIndex;
        private VariableIdentifier index;
        private Value value;

        public ObjectValueLocation(MemoryIndex containingIndex,VariableIdentifier index, Value value)
        {
            this.containingIndex = containingIndex;
            this.index = index;
            this.value = value;
        }

        public override void Accept(ICollectedLocationVisitor visitor)
        {
            visitor.VisitObjectValueLocation(this);
        }

        public override IEnumerable<Value> ReadValues(MemoryAssistantBase assistant)
        {
            return assistant.ReadValueField(value, index);
        }

        public override IEnumerable<Value> WriteValues(MemoryAssistantBase assistant, MemoryEntry entry)
        {
            return assistant.WriteValueField(value, index, entry);
        }
    }

    class ObjectUndefinedValueLocation : CollectedLocation
    {
        private MemoryIndex containingIndex;
        private VariableIdentifier index;
        private UndefinedValue value;

        public ObjectUndefinedValueLocation(MemoryIndex containingIndex, VariableIdentifier index, UndefinedValue value)
        {
            this.containingIndex = containingIndex;
            this.index = index;
            this.value = value;
        }

        public override void Accept(ICollectedLocationVisitor visitor)
        {
            visitor.VisitObjectUndefinedValueLocation(this);
        }

        public override IEnumerable<Value> ReadValues(MemoryAssistantBase assistant)
        {
            return assistant.ReadValueField(value, index);
        }

        public override IEnumerable<Value> WriteValues(MemoryAssistantBase assistant, MemoryEntry entry)
        {
            return assistant.WriteValueField(value, index, entry);
        }
    }

    class ObjectAnyValueLocation : CollectedLocation
    {
        private MemoryIndex containingIndex;
        private VariableIdentifier index;
        private AnyValue value;

        public ObjectAnyValueLocation(MemoryIndex containingIndex, VariableIdentifier index, AnyValue value)
        {
            this.containingIndex = containingIndex;
            this.index = index;
            this.value = value;
        }

        public override void Accept(ICollectedLocationVisitor visitor)
        {
            visitor.VisitObjectAnyValueLocation(this);
        }

        public override IEnumerable<Value> ReadValues(MemoryAssistantBase assistant)
        {
            return assistant.ReadValueField(value, index);
        }

        public override IEnumerable<Value> WriteValues(MemoryAssistantBase assistant, MemoryEntry entry)
        {
            return assistant.WriteValueField(value, index, entry);
        }
    }

    class ArrayValueLocation : CollectedLocation
    {
        private MemoryIndex containingIndex;
        private MemberIdentifier index;
        private Value value;

        public ArrayValueLocation(MemoryIndex containingIndex, MemberIdentifier index, Value value)
        {
            this.containingIndex = containingIndex;
            this.index = index;
            this.value = value;
        }

        public override void Accept(ICollectedLocationVisitor visitor)
        {
            visitor.VisitArrayValueLocation(this);
        }

        public override IEnumerable<Value> ReadValues(MemoryAssistantBase assistant)
        {
            return assistant.ReadValueIndex(value, index);
        }

        public override IEnumerable<Value> WriteValues(MemoryAssistantBase assistant, MemoryEntry entry)
        {
            return assistant.WriteValueIndex(value, index, entry);
        }
    }

    class ArrayAnyValueLocation : CollectedLocation
    {
        private MemoryIndex containingIndex;
        private MemberIdentifier index;
        private AnyValue value;

        public ArrayAnyValueLocation(MemoryIndex containingIndex, MemberIdentifier index, AnyValue value)
        {
            this.containingIndex = containingIndex;
            this.index = index;
            this.value = value;
        }

        public override void Accept(ICollectedLocationVisitor visitor)
        {
            visitor.VisitArrayAnyValueLocation(this);
        }

        public override IEnumerable<Value> ReadValues(MemoryAssistantBase assistant)
        {
            return assistant.ReadAnyValueIndex(value, index).PossibleValues;
        }

        public override IEnumerable<Value> WriteValues(MemoryAssistantBase assistant, MemoryEntry entry)
        {
            return assistant.WriteValueIndex(value, index, entry);
        }
    }

    class ArrayUndefinedValueLocation : CollectedLocation
    {
        private MemoryIndex containingIndex;
        private MemberIdentifier index;
        private UndefinedValue value;

        public ArrayUndefinedValueLocation(MemoryIndex containingIndex, MemberIdentifier index, UndefinedValue value)
        {
            this.containingIndex = containingIndex;
            this.index = index;
            this.value = value;
        }

        public override void Accept(ICollectedLocationVisitor visitor)
        {
            visitor.VisitArrayUndefinedValueLocation(this);
        }

        public override IEnumerable<Value> ReadValues(MemoryAssistantBase assistant)
        {
            return assistant.ReadValueIndex(value, index);
        }

        public override IEnumerable<Value> WriteValues(MemoryAssistantBase assistant, MemoryEntry entry)
        {
            return assistant.WriteValueIndex(value, index, entry);
        }
    }

    class ArrayStringValueLocation : CollectedLocation
    {
        private MemoryIndex containingIndex;
        private MemberIdentifier index;
        private StringValue value;

        public MemoryIndex ContainingIndex { get { return containingIndex; } }
        public StringValue Value { get { return value; } }

        public ArrayStringValueLocation(MemoryIndex containingIndex, MemberIdentifier index, StringValue value)
        {
            this.containingIndex = containingIndex;
            this.index = index;
            this.value = value;
        }
        public override void Accept(ICollectedLocationVisitor visitor)
        {
            visitor.VisitArrayStringValueLocation(this);
        }

        public override IEnumerable<Value> ReadValues(MemoryAssistantBase assistant)
        {
            return assistant.ReadStringIndex(value, index);
        }

        public override IEnumerable<Value> WriteValues(MemoryAssistantBase assistant, MemoryEntry entry)
        {
            return assistant.WriteStringIndex(value, index, entry);
        }
    }
}
