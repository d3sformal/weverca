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

namespace Weverca.MemoryModels.UnitTest.CopyMemoryModel
{
    [TestClass]
    class MemoryIndexTest
    {
        MemoryIndex variableA;
        MemoryIndex variableB;
        MemoryIndex undefinedVariable;

        MemoryIndex fieldA;
        MemoryIndex fieldB;
        MemoryIndex fieldInDifferentTree;
        MemoryIndex undefinedField;

        MemoryIndex indexA;
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
            variableB = MemoryIndex.MakeIndexVariable("b");
            undefinedVariable = MemoryIndex.MakeIndexAnyVariable();

            fieldA = MemoryIndex.MakeIndexField(variableA, "a");
            fieldB = MemoryIndex.MakeIndexField(variableA, "b");
            fieldInDifferentTree = MemoryIndex.MakeIndexField(variableB, "a");
            undefinedField = MemoryIndex.MakeIndexAnyField(variableA);

            indexA = MemoryIndex.MakeIndexIndex(variableA, "a");
            indexB = MemoryIndex.MakeIndexIndex(variableA, "b");
            indexInDifferentTree = MemoryIndex.MakeIndexIndex(variableB, "a");
            undefinedIndex = MemoryIndex.MakeIndexAnyIndex(variableA);

            doubleIndex = MemoryIndex.MakeIndexIndex(indexA, "1");
            doubleField = MemoryIndex.MakeIndexField(fieldA, "1");

            indexInField = MemoryIndex.MakeIndexIndex(fieldB, "1");
            fieldInIndex = MemoryIndex.MakeIndexField(indexB, "1");
        }

        [TestMethod]
        public void indexEqualityTest()
        {

        }
    }
}
