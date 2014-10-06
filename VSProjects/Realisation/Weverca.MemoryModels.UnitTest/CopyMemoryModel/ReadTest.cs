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
using Weverca.MemoryModels.UnitTest.SnapshotTestFramework;

namespace Weverca.MemoryModels.UnitTest.CopyMemoryModel
{
    [TestClass]
    public class ReadTest
    {
        [TestMethod]
        public void readObjectTest()
        {
            SnapshotTester<Snapshot> prepare = new SnapshotTester<Snapshot>();

            prepare.Var("a").Index().Field("f").Write(1);
            prepare.Var("a").Index("1").Field("f").Write(2);
            prepare.Var("a").Index("1").Field("g").Write(3);
            prepare.Var("a").Index("2").Field("g").Write(4);

            prepare.Var("b").Index("1").Field("f").Write(5);
            prepare.Var("b").Index("1").Field("g").Write(6);

            prepare.Test();

            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>(prepare.Snapshot);
            tester.SetLogger(new FileLogger("readObjectTest.txt"));
            
            tester.Var("a").Index().Field("f").Read();
            tester.Var("a").Index("1").Field("f").Read();
            tester.Var("a").Index("1").Field("g").Read();
            tester.Var("a").Index("2").Field("g").Read();
            tester.Var("a").Index("2").Field("f").Read();
            tester.Var("a").Index("3").Field("f").Read();
            tester.Var("a").Index("3").Field("g").Read();
            tester.Var("a").Index("3").Field("u").Read();
            tester.Var("a").Read();
            tester.Var("c").Read();
            tester.Var("b").Index("i").Read();
            tester.Var("a").Index("1").Field("f", "g").Read();
            tester.Var("a").Index().Field("f", "g").Read();

            tester.Var().Index().Field("f").Read();
            tester.Var("b").Index().Field("f").Read();
            tester.Var("b").Index("1").Field("f").Read();
            tester.Var().Index("1").Field("f").Read();

            tester.Test();
        }
    }
}