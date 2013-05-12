using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.ControlFlowGraph.AlternativeMemoryModel;
using Weverca.ControlFlowGraph.AlternativeMemoryModel.ValueImplementations;
using Weverca.ControlFlowGraph.AlternativeMemoryModel.Builders;

namespace Weverca.ControlFlowGraph.UnitTest
{
   static class MemoryModelUsage
    {

       static MemoryContext VariableAssign(MemoryContext context,Variable var1, Variable var2)
       {
           //we want to add new information into given context
           var builder = context.CreateDerivedContextBuilder();

           //builder for working with variable
           var var1Builder = builder.ModificationBuilder(var1);
           //assigning values into variable in given context
           var1Builder.Assign(var2.GetPossibleValues(context));


           //build new memory context with changes made via builders
           return builder.BuildContext();
       }

       static MemoryContext DeclareVariable(MemoryContext context, VariableName varName,AbstractValue assignedValue)
       {
           //we want to add new information into given context
           var builder = context.CreateDerivedContextBuilder();

           //declaring and assigning value into variable in given context
           var declaredVar = builder.Declare(varName, assignedValue);

           return builder.BuildContext();
       }


       static MemoryContext WorkWithValues(MemoryContext context, AssociativeContainer value,Variable someVariable)
       {
           //we want to add new information into given context
           var builder = context.CreateDerivedContextBuilder();

           var container=builder.ModificationBuilder(value);

           var key1 = new StringValue("key1");
           var key2 = new StringValue("key2");

           //assign by value
           container.Set(key1, someVariable.GetPossibleValues(context));
           //assign by reference
           container.SetReferences(key2, someVariable.PossibleReferences); 

           return builder.BuildContext();
       }

    }
}
