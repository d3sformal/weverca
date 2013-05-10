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

        public StringValue(string value,Reference reference)
            :base(reference)
        {
            Value = value;
        }


        public override int DeepGetHashCode()
        {
            throw new NotImplementedException();
        }

        public override bool DeepEquals(object other)
        {
            throw new NotImplementedException();
        }
    }
}
