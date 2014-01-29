using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
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

        ContainerIndex indexX;
        ContainerIndex indexY;
        ContainerIndex indexZ;

        IntegerValue value5;
        IntegerValue value6;


        [TestInitialize]
        public void Prepare()
        {
            snapshot = new Snapshot();
            snapshot.StartTransaction();

            value5 = snapshot.CreateInt(5);
            value6 = snapshot.CreateInt(6);

            indexX = snapshot.CreateIndex("x");
            indexY = snapshot.CreateIndex("y");
            indexZ = snapshot.CreateIndex("z");
        }

        [TestCleanup]
        public void Close()
        {
            snapshot.CommitTransaction();
        }

        #region Variables

        private void singleVariableTester(VariableName variable, Value value)
        {
            MemoryEntry entry = readVariable(variable);
            List<Value> entryValues = new List<Value>(entry.PossibleValues);
            Assert.AreEqual(entryValues.Count, 1);
            Assert.AreEqual(entryValues[0], value);
        }

        private MemoryEntry readVariable(VariableName variable)
        {
            var entry = snapshot.GetVariable(new VariableIdentifier(variable)).ReadMemory(snapshot);
            return entry;
        }

   

        #endregion

    }
}
