/*
Copyright (c) 2012-2014 Marcel Kikta, David Skorvaga, Matyas Brenner, and David Hauzar

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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.Common;

namespace Weverca.Analysis.UnitTest
{
    /// <summary>
    /// Tests for resolution of includes.
    /// 
    /// Tests the following functionality:
    /// The path to the file to be included can be either absolute or relative to the directory:
    /// 1. Of the main (entry) script
    /// 2. Of a currently executed script / the script where the currently executed function or method is defined

    /// Does not test the functionality of indirect includes (include specified by a variable)
    /// </summary>
    [TestClass]
    public class IncludeTests
    {
        [TestMethod]
        public void IncludeTest1()
        {
            // Run analysis
            FileInfo entryFile = new FileInfo(TrunkStructure.PHP_SOURCES_DIR + @"\test_programs\include_tests\include_test_01\index.php");
            ProgramPointGraph ppGraph = Analyzer.Run(entryFile, MemoryModels.MemoryModels.VirtualReferenceMM);
            FlowOutputSet outSet = ppGraph.End.OutSet;

            // For each variable test whether it has given value
            testVariable(outSet, "include_main_dir", "In ./include_main_dir.php");
            testVariable(outSet, "include_main_dir2", "In ./include_main_dir2.php");
            testVariable(outSet, "include_included_dir", "In include_dir/include_included_dir.php");
            testVariable(outSet, "test_main", "In ./test.php");
            testVariable(outSet, "test_included", "");
            testVariable(outSet, "test2_included", "In included_dir/test2.php");
            testVariable(outSet, "test3_included", "In included_dir/test3.php");
            testVariable(outSet, "test4_included", "");
            testVariable(outSet, "test5_included", "In included_dir/test5.php");
            testVariable(outSet, "test6_included", "");
            testVariable(outSet, "test7_included", "");
            testVariable(outSet, "in_func1", "In function func1");
            testVariable(outSet, "in_func2", "");
            testVariable(outSet, "in_func3", "In function func3");
            testVariable(outSet, "in_meth1", "In method meth1 of class Cl");
            testVariable(outSet, "in_meth2", "");
            testVariable(outSet, "index1", "In ./index.php");
            testVariable(outSet, "index2", "In ./index.php");
            testVariable(outSet, "index3", "In ./index.php");
        }

        private static void testVariable(FlowOutputSet outSet, string variableName, string compareValue) 
        {
            var val = (ScalarValue<string>)outSet.GetVariable(new VariableIdentifier(variableName)).ReadMemory(outSet.Snapshot).PossibleValues.First();
            Assert.IsTrue(val.Value.Equals(compareValue));
        }
    }
}