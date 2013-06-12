using System;
using System.Linq;
using System.Collections.Generic;
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

        string testStringValue1 = "TestString1";
        string testStringValue2 = "TestString2";

        [TestMethod]
        public void SimpleAssign()
        {
            var snapshot = createSnapshotWithValue(testVar1, testStringValue1);
            var storedValue = readFirstValue<StringValue>(snapshot, testVar1);

            Assert.AreEqual(testStringValue1, storedValue.Value);
        }

        [TestMethod]
        public void SimpleExtend()
        {
            var snapshot1 = createSnapshotWithValue(testVar1, testStringValue1);

            var snapshot2 = new Snapshot();
            snapshot2.StartTransaction();
            snapshot2.Extend(snapshot1);
            snapshot2.CommitTransaction();

            var storedValue = readFirstValue<StringValue>(snapshot2, testVar1);
            Assert.AreEqual(testStringValue1, storedValue.Value);
        }

        [TestMethod]
        public void MergedExtend()
        {
            var expected = new string[] { testStringValue1, testStringValue2 };
            var snapshot1 = createSnapshotWithValue(testVar1, testStringValue1);
            var snapshot2 = createSnapshotWithValue(testVar1, testStringValue2);

            var snapshot = new Snapshot();
            snapshot.StartTransaction();
            snapshot.Extend(snapshot1, snapshot2);

            var values = readStringValues(snapshot, testVar1);

            CollectionAssert.AreEquivalent(expected, values);
        }

        [TestMethod]
        public void AliasAssign()
        {
            var snapshot1 = createSnapshotWithValue(testVar1, testStringValue1, false);
            var alias = snapshot1.CreateAlias(testVar1);

            snapshot1.Assign(testVar2, alias);

            //read value from testVar1 accross testVar2
            var mustAliasRead = readFirstValue<StringValue>(snapshot1,testVar1);
            Assert.AreEqual(testStringValue1, mustAliasRead.Value,
                "Reading value from referencing must alias failed"
                );

            //set value to testVar2 and expect its presence in testVar1
            var testValue2 = snapshot1.CreateString(testStringValue2);
            snapshot1.Assign(testVar2,testValue2);
            
            var mustAliasAfterWrite = readFirstValue<StringValue>(snapshot1, testVar1);
            Assert.AreEqual(testStringValue2,mustAliasAfterWrite.Value,
                "Rerefernced alias variable was not updated after must alias write"
                );
        }

        private Snapshot createSnapshotWithValue(VariableName targetVar, string value, bool commit = true)
        {
            var snapshot = new Snapshot();
            snapshot.StartTransaction();

            var testString = snapshot.CreateString(value);
            snapshot.Assign(targetVar, testString);
            if (commit)
            {
                snapshot.CommitTransaction();
            }

            return snapshot;
        }

        private T readFirstValue<T>(Snapshot snapshot, VariableName variable)
            where T : Value
        {
            var entry = snapshot.ReadValue(variable);
            var enumerator = entry.PossibleValues.GetEnumerator();
            enumerator.MoveNext();
            var value = enumerator.Current;
            return value as T;
        }

        private T[] readValues<T>(Snapshot snapshot, VariableName variable)
            where T : Value
        {
            var entry = snapshot.ReadValue(variable);
            var castedEnumerable = entry.PossibleValues.AsEnumerable().Cast<T>();
            return castedEnumerable.ToArray();
        }

        private string[] readStringValues(Snapshot snapshot, VariableName variable)
        {
            var values = readValues<StringValue>(snapshot, variable);

            var data = from value in values select value.Value;

            return data.ToArray();
        }
    }
}
