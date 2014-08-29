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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weverca.MemoryModels.CopyMemoryModel;
using Weverca.MemoryModels.UnitTest.SnapshotTestFramework;

namespace Weverca.MemoryModels.UnitTest.CopyMemoryModel
{
    [TestClass]
    public class AssignFromMemoryTest
    {
        [TestMethod]
        public void AssignScalarFromMemory()
        {
            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>();

            tester.Var("a").Write("A");
            tester.Var("d").Write("D");

            tester.Var("b").Write(tester.Var("a"));
            tester.Var("c").Index("1").Write(2);
            tester.Var("c").Index("2").Write(tester.Var("a"));
            tester.Var("c").Index().Write(tester.Var("d"));

            tester.SetLogger(new FileLogger("AssignScalarFromMemory.txt"));
            tester.Test();
        }

        [TestMethod]
        public void AssignUndefinedFromMemory()
        {
            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>();

            tester.Var("b").Write(1);
            tester.Var("b").Write(tester.Var("a"));

            tester.SetLogger(new FileLogger("AssignUndefinedFromMemory.txt"));
            tester.Test();
        }

        [TestMethod]
        public void AssignStrongArrayFromMemory()
        {
            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>();

            tester.Var("a").Index("1").Write(1);
            tester.Var("a").Index("2").Index("2").Write(2);
            tester.Var("a").Index("2").Index().Write(3);
            tester.Var("a").Index("2").Index().Index("4").Write(4);
            tester.Var("a").Index("5").Write(5);
            tester.Var("a").Index("6").Write(6);

            tester.Var("b").Write(tester.Var("a"));
            tester.Var("a").Index("1").Write(7);
            tester.Var("b").Index("1").Write(8);
            tester.Var("b").Index("2").Index("2").Write(9);

            tester.SetLogger(new FileLogger("AssignStrongArrayFromMemory.txt"));
            tester.Test();
        }

        [TestMethod]
        public void AssignWeakArrayFromMemory()
        {
            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>();

            tester.Var("a").Index("1").Write(1);
            tester.Var("a").Index("2").Index("2").Write(2);
            tester.Var("a").Index("2").Index().Write(3);
            tester.Var("a").Index("2").Index().Index("4").Write(4);
            tester.Var("a").Index("5").Write(5);
            tester.Var("a").Index("6").Write(6);

            tester.Var("b").Index("a").Write(7);
            tester.Var("b").Index().Write(tester.Var("a"));
            tester.Var("a").Index("1").Write(8);
            tester.Var("b").Index("a").Index("1").Write(9);
            tester.Var("b").Index("a").Index("2").Index("2").Write(10);

            tester.SetLogger(new FileLogger("AssignWeakArrayFromMemory.txt"));
            tester.Test();
        }

        [TestMethod]
        public void AssignObjectReferenceFromMemory()
        {
            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>();

            tester.Var("a").Field("f").Write("a.1");
            tester.Var("b").Write(tester.Var("a"));
            tester.Var("a").Field("g").Write("a.2");
            tester.Var("b").Field("h").Write("b.3");

            tester.Var("c").Field("f").Write("c.4");
            tester.Var("d").Write(tester.Var("c"));
            tester.Var("d").Field("g").Write("d.5");

            tester.Var("arr").Index("i").Write("i");

            tester.Var("arr").Index().Write(tester.Var("a"));
            tester.Var("arr").Index("i").Field("f").Write("arr[i].6");
            tester.Var("arr").Index().Field("g").Write("arr[?].7");

            tester.Var("arr").Index().Write(tester.Var("c"));
            tester.Var("arr").Index("i").Field("f").Write("arr[i].8");
            tester.Var("arr").Index().Field("g").Write("arr[?].9");

            tester.Var("a").Field("f").Write("a.10");


            tester.SetLogger(new FileLogger("AssignObjectReferenceFromMemory.txt"));
            tester.Test();
        }
    }
}