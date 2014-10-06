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
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

using Weverca.MemoryModels.CopyMemoryModel;
using Weverca.MemoryModels.UnitTest.SnapshotTestFramework;

namespace Weverca.MemoryModels.UnitTest.CopyMemoryModel
{
    [TestClass]
    public class MergeTest
    {
        [TestMethod]
        public void MergeVariableTest()
        {
            SnapshotTester<Snapshot> prepareA = new SnapshotTester<Snapshot>();
            prepareA.Var("a").Write("A.1");
            prepareA.Var("b").Write("2");
            prepareA.Var("c").Write("A.3");
            prepareA.Test();

            SnapshotTester<Snapshot> prepareB = new SnapshotTester<Snapshot>();
            prepareB.Var("a").Write("B.1");
            prepareB.Var("b").Write("2");
            prepareB.Var("d").Write("B.3");
            prepareB.Test();

            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>();
            tester.SetLogger(new FileLogger("MergeVariableTest.txt"));

            tester.Merge(prepareA.Snapshot, prepareB.Snapshot);

            tester.Test();
        }


        [TestMethod]
        public void MergeArrayTest()
        {
            SnapshotTester<Snapshot> prepareA = new SnapshotTester<Snapshot>();
            prepareA.Var("v").Index().Write("A.?");
            prepareA.Var("v").Index("a").Write("A.1");
            prepareA.Var("v").Index("b").Write("2");
            prepareA.Var("v").Index("c").Write("A.3");
            prepareA.Var("v").Index("e").Index("1").Write("A.4");
            prepareA.Var("v").Index("f").Index("1").Write("A.5");
            prepareA.Var("v").Index("g").Index("1").Write("A.6");
            prepareA.Var().Index("1").Write("A.7");
            prepareA.Test();

            SnapshotTester<Snapshot> prepareB = new SnapshotTester<Snapshot>();
            prepareB.Var("v").Index("a").Write("B.1");
            prepareB.Var("v").Index("b").Write("2");
            prepareB.Var("v").Index("d").Write("B.3");
            prepareB.Var("v").Index("e").Index("1").Write("B.4");
            prepareB.Var("v").Index("g").Write("B.6");
            prepareB.Var("a").Index("1").Write("B.7");
            prepareB.Var("b").Write("B.8");
            prepareB.Test();

            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>();
            tester.SetLogger(new FileLogger("MergeArrayTest.txt"));

            tester.Merge(prepareA.Snapshot, prepareB.Snapshot);

            tester.Test();
        }
    }
}