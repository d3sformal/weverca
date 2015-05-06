/*
Copyright (c) 2012-2014 Pavel Bastecky.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

using Weverca.MemoryModels.CopyMemoryModel;

namespace Weverca.MemoryModels.UnitTest
{
    [TestClass]
    public class MemoryIndexTest
    {
        MemoryIndex variableA;
        MemoryIndex variableA2;
        MemoryIndex variableB;
        MemoryIndex undefinedVariable;

        MemoryIndex fieldA;
        MemoryIndex fieldA2;
        MemoryIndex fieldB;
        MemoryIndex fieldInDifferentTree;
        MemoryIndex undefinedField;

        MemoryIndex indexA;
        MemoryIndex indexA2;
        MemoryIndex indexB;
        MemoryIndex indexInDifferentTree;
        MemoryIndex undefinedIndex;

        MemoryIndex doubleIndex;
        MemoryIndex indexInField;

        public MemoryIndexTest()
        {
            Snapshot snapshot = new Snapshot();
            snapshot.StartTransaction();
            ObjectValue object1 = snapshot.CreateObject(null);
            ObjectValue object2 = snapshot.CreateObject(null);
            snapshot.CommitTransaction();

            variableA = VariableIndex.Create("a", 0);
            variableA2 = VariableIndex.Create("a", 0);
            variableB = VariableIndex.Create("b", 0);
            undefinedVariable = VariableIndex.CreateUnknown(0);

            fieldA = ObjectIndex.Create(object1, "a");
            fieldA2 = ObjectIndex.Create(object1, "a");
            fieldB = ObjectIndex.Create(object1, "b");
            fieldInDifferentTree = ObjectIndex.Create(object2, "a");
            undefinedField = ObjectIndex.CreateUnknown(object1);

            indexA = variableA.CreateIndex("a");
            indexA2 = variableA.CreateIndex("a");
            indexB = variableA.CreateIndex("b");
            indexInDifferentTree = variableB.CreateIndex("a");
            undefinedIndex = variableA.CreateUnknownIndex();

            doubleIndex = indexA.CreateIndex("1");

            indexInField = fieldA.CreateIndex("1");
        }

        private void testEquality(MemoryIndex index1, MemoryIndex index2)
        {
            Assert.AreEqual(index1.GetHashCode(), index2.GetHashCode());
            Assert.IsTrue(variableA.Equals(variableA));
        }

        private void testUnEquality(MemoryIndex index1, MemoryIndex index2)
        {
            Assert.IsTrue(!index1.Equals(index2));
        }

        private void testHashContains(HashSet<MemoryIndex> hashSet, MemoryIndex index)
        {
            Assert.IsTrue(hashSet.Contains(index));
        }

        [TestMethod]
        public void indexEqualityTest()
        {
            testEquality(variableA, variableA);
            testEquality(variableA, variableA2);

            testEquality(fieldA, fieldA2);
            testEquality(indexA, indexA2);
        }

        // Started failing after commit of prototype allocation-site abstraction in commit 969.
        // Does not fail since some revision between 969-979
        [TestMethod]
        public void indexUnEqualityTest()
        {
            testUnEquality(variableA, variableB);

            testUnEquality(fieldA, fieldB);
            testUnEquality(fieldInDifferentTree, fieldA);

            testUnEquality(indexA, indexB);
            testUnEquality(indexInDifferentTree, indexA);

            testUnEquality(indexA, fieldA);

            testUnEquality(variableA, fieldA);
            testUnEquality(indexA, variableB);
        }

        // Started failing after commit of prototype allocation-site abstraction in commit 969.
        // Does not fail since some revision between 969-979
        [TestMethod]
        public void hashSetTest()
        {
            HashSet<MemoryIndex> hashSet = new HashSet<MemoryIndex>();

            hashSet.Add(variableA);
            hashSet.Add(variableA2);
            hashSet.Add(variableB);
            hashSet.Add(undefinedVariable);

            hashSet.Add(fieldA);
            hashSet.Add(fieldA2);
            hashSet.Add(fieldB);
            hashSet.Add(fieldInDifferentTree);
            hashSet.Add(undefinedField);

            hashSet.Add(indexA);
            hashSet.Add(indexA2);
            hashSet.Add(indexB);
            hashSet.Add(indexInDifferentTree);
            hashSet.Add(undefinedIndex);

            hashSet.Add(doubleIndex);

            hashSet.Add(indexInField);


            Assert.AreEqual(13, hashSet.Count);

            testHashContains(hashSet, variableA);
            testHashContains(hashSet, variableA2);
            testHashContains(hashSet, variableB);
            testHashContains(hashSet, undefinedVariable);

            testHashContains(hashSet, fieldA);
            testHashContains(hashSet, fieldA2);
            testHashContains(hashSet, fieldB);
            testHashContains(hashSet, fieldInDifferentTree);
            testHashContains(hashSet, undefinedField);

            testHashContains(hashSet, indexA);
            testHashContains(hashSet, indexA2);
            testHashContains(hashSet, indexB);
            testHashContains(hashSet, indexInDifferentTree);
            testHashContains(hashSet, undefinedIndex);

            testHashContains(hashSet, doubleIndex);

            testHashContains(hashSet, indexInField);
        }


    }
}