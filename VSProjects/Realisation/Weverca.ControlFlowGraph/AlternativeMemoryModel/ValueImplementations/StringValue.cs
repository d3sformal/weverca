using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel.ValueImplementations
{
    /// <summary>
    /// Possible implementation of string value representation
    /// </summary>
    public class StringValue :AbstractValue
    {
        public readonly string Value;

        public StringValue(string value)            
        {
            Value = value;
        }

        public override int DeepGetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool DeepEquals(object other)
        {
            return Value.Equals(other);
        }

        protected override string toString()
        {
            return Value;
        }
    }
}
