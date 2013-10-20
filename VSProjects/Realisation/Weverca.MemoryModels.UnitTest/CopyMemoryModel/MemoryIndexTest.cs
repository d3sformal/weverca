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
        MemoryIndex doubleField;

        MemoryIndex indexInField;
        MemoryIndex fieldInIndex;

        public MemoryIndexTest()
        {
            variableA = MemoryIndex.MakeIndexVariable("a");
            variableA2 = MemoryIndex.MakeIndexVariable("a");
            variableB = MemoryIndex.MakeIndexVariable("b");
            undefinedVariable = MemoryIndex.MakeIndexAnyVariable();

            fieldA = MemoryIndex.MakeIndexField(variableA, "a");
            fieldA2 = MemoryIndex.MakeIndexField(variableA, "a");
            fieldB = MemoryIndex.MakeIndexField(variableA, "b");
            fieldInDifferentTree = MemoryIndex.MakeIndexField(variableB, "a");
            undefinedField = MemoryIndex.MakeIndexAnyField(variableA);

            indexA = MemoryIndex.MakeIndexIndex(variableA, "a");
            indexA2 = MemoryIndex.MakeIndexIndex(variableA, "a");
            indexB = MemoryIndex.MakeIndexIndex(variableA, "b");
            indexInDifferentTree = MemoryIndex.MakeIndexIndex(variableB, "a");
            undefinedIndex = MemoryIndex.MakeIndexAnyIndex(variableA);

            doubleIndex = MemoryIndex.MakeIndexIndex(indexA, "1");
            doubleField = MemoryIndex.MakeIndexField(fieldA, "1");

            indexInField = MemoryIndex.MakeIndexIndex(fieldB, "1");
            fieldInIndex = MemoryIndex.MakeIndexField(indexB, "1");
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
            hashSet.Add(doubleField);

            hashSet.Add(indexInField);
            hashSet.Add(fieldInIndex);


            Assert.AreEqual(15, hashSet.Count);

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
            testHashContains(hashSet, doubleField);

            testHashContains(hashSet, indexInField);
            testHashContains(hashSet, fieldInIndex);
        }


    }
}
