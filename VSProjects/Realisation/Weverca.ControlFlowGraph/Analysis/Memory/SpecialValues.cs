using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Weverca.ControlFlowGraph.Analysis.Memory
{

    public class SpecialValue : Value
    {
    }

    public class AliasValue : SpecialValue
    {
        internal AliasValue(){}
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
