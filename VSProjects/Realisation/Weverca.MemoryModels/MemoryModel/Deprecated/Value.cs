using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Weverca.ControlFlowGraph;

namespace Weverca.MemoryModel.Deprecated
{

    abstract class Value
    {

        internal Value Copy()
        {
            throw new NotImplementedException();
        }

        internal virtual Value CopyStructure(CopyResolver resolver)
        {
            return this;
        }
    }

    class AnyValue : Value
    {

    }

    class ObjectReference : Value
    {
        Table table = new Table();
        //Marcel  - mozno je lepsi napad pouzit class respresentation z parsera
        //PhpType - ma viac informacii a ClassDeclaration uklada len confrloflow grafy metod, co nieje pre memory model uzitocne...
        ClassDeclaration classInfo;

        internal override Value CopyStructure(CopyResolver resolver)
        {
            ObjectReference newRef = new ObjectReference();
            newRef.table.CopyStructureFrom(this.table, resolver);
            return newRef;
        }
    }

    class IntegerValue : Value
    {
        public int value;
    }

    class StringValue : Value
    {
        public string value;

    }

    class BooleanValue : Value
    {
        public bool value;
    }

    class FloatValue : Value
    {
        public double value;
    }

    class UndefinedValue : Value
    {
    }
}
