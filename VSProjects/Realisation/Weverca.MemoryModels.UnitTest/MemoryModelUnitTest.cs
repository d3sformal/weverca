using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

using PHP.Core;

using Weverca.Analysis.Memory;
using Weverca.MemoryModels.MemoryModel;

namespace Weverca.MemoryModels.UnitTest
{
    [TestClass]
    public class MemoryModelUnitTest
    {
        Snapshot snapshot;

        VariableName variableX = new VariableName("x");
        VariableName variableY = new VariableName("y");
        VariableName variableZ = new VariableName("z");

        IntegerValue value5;
        IntegerValue value6;

        [TestInitialize]
        public void Prepare()
        {
            snapshot = new Snapshot();
            snapshot.StartTransaction();

            value5 = snapshot.CreateInt(5);
            value6 = snapshot.CreateInt(6);
        }

        [TestCleanup]
        public void Close()
        {
            snapshot.CommitTransaction();
        }

        private void singleVariableTester(VariableName variable, Value value)
        {
            MemoryEntry entry = snapshot.ReadValue(variable);
            List<Value> entryValues = new List<Value>(entry.PossibleValues);
            Assert.AreEqual(entryValues.Count, 1);
            Assert.AreEqual(entryValues[0], value);
        }

        [TestMethod]
        public void CreateSimpleVariable()
        {
            snapshot.Assign(variableX, value5);
            singleVariableTester(variableX, value5);

            snapshot.Assign(variableX, value6);
            singleVariableTester(variableX, value6);
        }

        [TestMethod]
        public void AssignFromVariable()
        {
            snapshot.Assign(variableX, value5);
            MemoryEntry entryX = snapshot.ReadValue(variableX);

            snapshot.Assign(variableY, entryX);
            singleVariableTester(variableY, value5);
        }

        [TestMethod]
        public void AssignAlias()
        {
            Value alias = snapshot.CreateAlias(variableY);
            snapshot.Assign(variableX, alias);

            snapshot.Assign(variableX, value5);
            singleVariableTester(variableX, value5);
            singleVariableTester(variableY, value5);

            snapshot.Assign(variableZ, snapshot.CreateAlias(variableX));
            singleVariableTester(variableX, value5);
            singleVariableTester(variableY, value5);
            singleVariableTester(variableZ, value5);

            snapshot.Assign(variableZ, value6);
            singleVariableTester(variableX, value6);
            singleVariableTester(variableY, value6);
            singleVariableTester(variableZ, value6);
        }

        [TestMethod]
        public void OverrideAlias()
        {
            snapshot.Assign(variableX, snapshot.CreateAlias(variableY));
            snapshot.Assign(variableX, value5);

            snapshot.Assign(variableX, snapshot.CreateAlias(variableZ));
            snapshot.Assign(variableX, value6);

            singleVariableTester(variableX, value6);
            singleVariableTester(variableY, value5);
            singleVariableTester(variableZ, value6);
        }
        
    }
}
