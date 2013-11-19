using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

using PHP.Core;

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

   //     [TestMethod]
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

 //       [TestMethod]
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

        #endregion

        #region Arrays

        [TestMethod]
        public void CreateSimpleArray()
        {
            AssociativeArray array = snapshot.CreateArray();
            snapshot.SetIndex(array, indexX, new MemoryEntry(value5));
            snapshot.SetIndex(array, indexY, new MemoryEntry(value6));
            snapshot.SetIndex(array, indexZ, new MemoryEntry(array));

            snapshot.Assign(variableX, array);

            MemoryEntry entry = snapshot.ReadValue(variableX);
            List<Value> entryValues = new List<Value>(entry.PossibleValues);
            Assert.AreEqual(entryValues.Count, 1);
            Assert.IsInstanceOfType(entryValues[0], typeof(AssociativeArray));

            AssociativeArray arrayX = (AssociativeArray)entryValues[0];
            List<Value> indexXValues = new List<Value>(snapshot.GetIndex(arrayX, indexX).PossibleValues);
            List<Value> indexYValues = new List<Value>(snapshot.GetIndex(arrayX, indexY).PossibleValues);

            Assert.AreEqual(indexXValues.Count, 1);
            Assert.AreEqual(indexXValues[0], value5);

            Assert.AreEqual(indexYValues.Count, 1);
            Assert.AreEqual(indexYValues[0], value6);


            List<Value> indexZValues = new List<Value>(snapshot.GetIndex(arrayX, indexZ).PossibleValues);
            Assert.AreEqual(indexZValues.Count, 1);
            Assert.IsInstanceOfType(indexZValues[0], typeof(AssociativeArray));

            AssociativeArray arrayZ = (AssociativeArray)indexZValues[0];
            List<Value> aZIndexXValues = new List<Value>(snapshot.GetIndex(arrayZ, indexX).PossibleValues);
            List<Value> aZIndexYValues = new List<Value>(snapshot.GetIndex(arrayZ, indexY).PossibleValues);

            Assert.AreEqual(aZIndexXValues.Count, 1);
            Assert.AreEqual(aZIndexXValues[0], value5);

            Assert.AreEqual(aZIndexYValues.Count, 1);
            Assert.AreEqual(aZIndexYValues[0], value6);
        }

        #endregion

        #region Objects

        [TestMethod]
        public void CreateSimpleObject()
        {
            ObjectValue obj = snapshot.CreateObject(null);
            snapshot.SetField(obj, indexX, new MemoryEntry(value5));
            snapshot.SetField(obj, indexY, new MemoryEntry(value6));
            snapshot.SetField(obj, indexZ, new MemoryEntry(obj));

            snapshot.Assign(variableX, obj);

            MemoryEntry entry = snapshot.ReadValue(variableX);
            List<Value> entryValues = new List<Value>(entry.PossibleValues);
            Assert.AreEqual(entryValues.Count, 1);
            Assert.IsInstanceOfType(entryValues[0], typeof(ObjectValue));

            ObjectValue objX = (ObjectValue)entryValues[0];
            List<Value> fieldXValues = new List<Value>(snapshot.GetField(objX, indexX).PossibleValues);
            List<Value> fieldYValues = new List<Value>(snapshot.GetField(objX, indexY).PossibleValues);

            Assert.AreEqual(fieldXValues.Count, 1);
            Assert.AreEqual(fieldXValues[0], value5);

            Assert.AreEqual(fieldYValues.Count, 1);
            Assert.AreEqual(fieldYValues[0], value6);


            List<Value> fieldZValues = new List<Value>(snapshot.GetField(objX, indexZ).PossibleValues);
            Assert.AreEqual(fieldZValues.Count, 1);
            Assert.IsInstanceOfType(fieldZValues[0], typeof(ObjectValue));

            ObjectValue objZ = (ObjectValue)fieldZValues[0];
            Assert.AreSame(objZ, objX);
            List<Value> oZFieldXValues = new List<Value>(snapshot.GetField(objZ, indexX).PossibleValues);
            List<Value> oZFieldYValues = new List<Value>(snapshot.GetField(objZ, indexY).PossibleValues);

            Assert.AreEqual(oZFieldXValues.Count, 1);
            Assert.AreEqual(oZFieldXValues[0], value5);

            Assert.AreEqual(oZFieldYValues.Count, 1);
            Assert.AreEqual(oZFieldYValues[0], value6);
        }

        #endregion

    }
}
