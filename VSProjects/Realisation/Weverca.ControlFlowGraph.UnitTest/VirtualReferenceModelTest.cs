using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;

using Weverca.ControlFlowGraph.Analysis.Memory;
using Weverca.ControlFlowGraph.VirtualReferenceModel;

namespace Weverca.ControlFlowGraph.UnitTest
{
    [TestClass]
    public class VirtualReferenceModelTest
    {
        VariableName testVar1 = new VariableName("Variable1");
        VariableName testVar2 = new VariableName("Variable2");

        string testStringValue1 = "TestString";

        [TestMethod]
        public void SimpleAssign()
        {
            var snapshot = new Snapshot();
            snapshot.StartTransaction();

            var testString=snapshot.CreateString(testStringValue1);
            snapshot.Assign(testVar1, testString);

            var storedValue=readFirstValue<StringValue>(snapshot,testVar1);
            snapshot.CommitTransaction();


            Assert.AreEqual(testStringValue1, storedValue.Value);
        }



        private T readFirstValue<T>(Snapshot snapshot,VariableName variable)
            where T: Value
        {
            var entry = snapshot.ReadValue(variable);
            var enumerator=entry.PossibleValues.GetEnumerator();
            enumerator.MoveNext();
            var value = enumerator.Current;
            return value as T;
        }
    }
}
