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
}
