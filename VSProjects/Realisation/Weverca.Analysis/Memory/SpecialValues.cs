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
    }

    public abstract class AliasValue : SpecialValue
    {        
    }

    public class AnyValue : SpecialValue
    {
        internal AnyValue() { }
    }

    public class UndefinedValue : SpecialValue
    {
        internal UndefinedValue() { }
    }
}
