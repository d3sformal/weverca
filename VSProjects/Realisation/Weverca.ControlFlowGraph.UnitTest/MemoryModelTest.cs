using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;
using Weverca.ControlFlowGraph.AlternativeMemoryModel;
using Weverca.ControlFlowGraph.AlternativeMemoryModel.ValueImplementations;

namespace Weverca.ControlFlowGraph.UnitTest
{
    /// <summary>
    /// TODO Refactor
    /// </summary>
    [TestClass]
    public class MemoryModelTest
    {
        [TestMethod]
        public void SimpleMemoryUsage()
        {
            var rootContext = MemoryContext.CreateRoot();

            var builder = rootContext.CreateDerivedContextBuilder();

            var testName = new VariableName("testName");
            var var1B = builder.VariableCreator(testName);

            //assign variable with some possible values

            var val1 = new StringValue("1a");
            var val2 = new StringValue("1b");
            var1B.Assign(new AbstractValue[] { val1, val2 });

            //build first context
            var context1 = builder.BuildContext();
            //obtain created variable
            var var1 = var1B.Build();


            var testValues = var1.GetPossibleValues(context1);

            Debug.Assert(testValues.Count() == 2 && testValues.Contains(val1) && testValues.Contains(val2), "Stored information has been malformed");
        }

        [TestMethod]
        public void AliasTesting()
        {
            var rootContext = MemoryContext.CreateRoot();
            var builder = rootContext.CreateDerivedContextBuilder();
            var varAName = new VariableName("A");
            var varBName = new VariableName("B");

            //$A="val A";
            //$B="val B";
            var valA = new StringValue("val A");
            var valB = new StringValue("val B");
            var varABuilder = builder.Declare(varAName, valA);
            var varBBuilder = builder.Declare(varBName, valB);

            var contex1 = builder.BuildContext();
            var varA = varABuilder.Build();
            var varB = varBBuilder.Build();


            //if($some){
            // $A=&$B
            //}            
            var builder2 = contex1.CreateDerivedContextBuilder();
            var modifyA = builder2.ModificationBuilder(varA);
            var modifyB = builder2.ModificationBuilder(varB);

            var references = new HashSet<VirtualReference>(varA.PossibleReferences);
            references.UnionWith(varB.PossibleReferences);

            // $B="val B changed"
            var valBChanged = new StringValue("val B changed");
            modifyA.AssignReferences(references);
            modifyB.Assign(new AbstractValue[] { valBChanged });

            var context2 = builder2.BuildContext();
            var modifiedVarA = modifyA.Build();

            // $A now can contain {"val A","val B changed"}
            Debug.Assert(checkExpectedValues(modifiedVarA, context2, valA, valBChanged), "Incorrect VirtualReference handling");
        }

        private bool checkExpectedValues(Variable var, MemoryContext context, params AbstractValue[] expectedValues)
        {
            var values = new HashSet<AbstractValue>(var.GetPossibleValues(context));
            if (values.Count != expectedValues.Length)
            {
                return false;
            }

            foreach (var expected in expectedValues)
            {
                if (!values.Contains(expected))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
