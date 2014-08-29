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
    public class AssignTest
    {

        [TestMethod]
        public void assignTest()
        {
            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>();

            tester.Var("a").Write(1);

            tester.SetLogger(new FileLogger("assignTest.txt"));
            tester.Test();
        }

        [TestMethod]
        public void assignArrayTest()
        {
            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>();

            tester.Var("a").Index("5").Index().Write(1);
            tester.Var("a").Index().Index().Write(2);
            tester.Var("a").Index().Index("3").Write(3);

            tester.SetLogger(new FileLogger("assignArrayTest.txt"));
            tester.Test();
        }

        [TestMethod]
        public void assignArrayFromUnknownTest()
        {
            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>();

            tester.Var("a").Index().Index().Write(1);
            tester.Var("a").Index().Index("3").Write(2);
            tester.Var("a").Index("5").Index().Write(3);
            tester.Var("a").Index("5").Index("6").Write(4);
            tester.Var("a").Index("5").Index("7").Write(7);
            tester.Var("a").Index("5").Index("8").Index("8").Write(8);
            tester.Var("a").Index().Write(5);
            tester.Var("a").Index("5").Index("6").Index("9").Write(6);

            tester.SetLogger(new FileLogger("assignArrayFromUnknownTest.txt"));
            tester.Test();
        }

        [TestMethod]
        public void assignArrayConcretizationTest()
        {
            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>();

            tester.Var("a").Index().Index("2").Index("3").Write(1);
            tester.Var("a").Index("1").Index().Index("3").Write(2);
            tester.Var("a").Index("1").Index("2").Index("3").Write(3);

            tester.SetLogger(new FileLogger("assignArrayConcretizationTest.txt"));
            tester.Test();
        }

        [TestMethod]
        public void OverrideArray()
        {
            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>();

            tester.Var("a").Index("1").Write(1);
            tester.Var("a").Index("2").Index("2").Write(2);
            tester.Var("a").Index("2").Index().Write(3);
            tester.Var("a").Index("2").Index().Index("4").Write(4);
            tester.Var("a").Index("5").Write(5);
            tester.Var("a").Index("6").Write(6);
            tester.Var("a").Write("OVERRIDE");

            tester.SetLogger(new FileLogger("OverrideArray.txt"));
            tester.Test();
        }

        [TestMethod]
        public void assignObjectTest()
        {
            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>();

            tester.Var("a").Index().Field("f").Write(1);
            tester.Var("a").Index("1").Field("f").Write(2);
            tester.Var("a").Index("1").Field("g").Write(4);
            tester.Var("a").Index("2").Field("g").Write(5);

            tester.SetLogger(new FileLogger("assignObjectTest.txt"));
            tester.Test();
        }

    }
}