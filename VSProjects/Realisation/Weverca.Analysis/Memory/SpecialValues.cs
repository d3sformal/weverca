using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Weverca.Analysis.Memory
{

    public class SpecialValue : Value
    {
        public override int GetHashCode()
        {
            return this.GetType().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.GetType() == obj.GetType();
        }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitSpecialValue(this);
        }
    }

    public abstract class AliasValue : SpecialValue
    {
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAliasValue(this);
        }
    }

    public class AnyValue : SpecialValue
    {
        internal AnyValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyValue(this);
        }
    }

    public class UndefinedValue : SpecialValue
    {
        internal UndefinedValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitUndefinedValue(this);
        }
    }

    public class AnyStringValue : AnyValue
    {
        internal AnyStringValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyStringValue(this);
        }
    }

    public class AnyIntegerValue : AnyValue
    {
        internal AnyIntegerValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyIntegerValue(this);
        }
    }

    public class AnyLongintValue : AnyValue
    {
        internal AnyLongintValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyLongintValue(this);
        }
    }

    public class AnyBooleanValue : AnyValue
    {
        internal AnyBooleanValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyBooleanValue(this);
        }
    }

    public class AnyObjectValue : AnyValue
    {
        internal AnyObjectValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyObjectValue(this);
        }
    }

    public class AnyArrayValue : AnyValue
    {
        internal AnyArrayValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyArrayValue(this);
        }
    }

   

}
