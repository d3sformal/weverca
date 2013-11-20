using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weverca.MemoryModels.CopyMemoryModel;
using Weverca.MemoryModels.UnitTest.SnapshotTestFramework;

namespace Weverca.MemoryModels.UnitTest.CopyMemoryModel
{
    [TestClass]
    public class AssignAliasTest
    {
        [TestMethod]
        public void aliasAssignTest()
        {
            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>();

            tester.Var("a").Alias(tester.Var("b"));
            tester.Var("b").Write("b.1");

            tester.Var("c").Alias(tester.Var("b"));

            tester.Var("x", "y").Alias(tester.Var("b"));
            tester.Var("b").Write("b.2");
            tester.Var("x").Write("x.3");

            tester.Var("z").Alias(tester.Var("x"));

            tester.SetLogger(new FileLogger("aliasAssignTest.txt"));
            tester.Test();
        }

        [TestMethod]
        public void destroyAliasTest()
        {
            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>();

            tester.Var("a").Alias(tester.Var("b"));
            tester.Var("c").Alias(tester.Var("b"));
            tester.Var("b").Alias(tester.Var("x"));

            tester.SetLogger(new FileLogger("destroyAliasTest.txt"));
            tester.Test();
        }

        [TestMethod]
        public void aliasUnknownIndexTest()
        {
            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>();

            tester.Var("x").Write("x.1");
            tester.Var("a").Index("1").Write("a[1].2");
            tester.Var("a").Index().Alias(tester.Var("x"));
            tester.Var("a").Index("2").Write("a[2].3");
            tester.Var("a").Index("3").Alias(tester.Var("x"));
            tester.Var("a").Index("4", "5").Write("4");

            tester.SetLogger(new FileLogger("aliasUnknownIndexTest.txt"));
            tester.Test();
        }

        [TestMethod]
        public void aliasArrayTest()
        {
            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>();

            tester.Var("a").Index("1").Alias(tester.Var("x1"));
            tester.Var("a").Index().Index("2").Alias(tester.Var("x2"));
            tester.Var("a").Index("3").Index("4").Write("a-3-4 .1");

            tester.SetLogger(new FileLogger("aliasArrayTest.txt"));
            tester.Test();
        }

        [TestMethod]
        public void aliasCopyArrayTest()
        {
            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>();

            tester.Var("a").Index("1").Alias(tester.Var("x"));
            tester.Var("a").Index("2").Alias(tester.Var("y1", "y2"));
            tester.Var("a").Index("3").Write(1);
            tester.Var("a").Index("4").Index("4").Alias(tester.Var("z"));

            tester.Var("b").Write(tester.Var("a"));

            tester.Var("c").Write(tester.Var("a", "u"));

            tester.SetLogger(new FileLogger("aliasCopyArrayTest.txt"));
            tester.Test();
        }

        [TestMethod]
        public void aliasUnknownCopyArrayTest()
        {
            SnapshotTester<Snapshot> tester = new SnapshotTester<Snapshot>();

            tester.Var("a").Index("1").Alias(tester.Var("x"));
            tester.Var("a").Index("2").Write(1);
            tester.Var("a").Index().Alias(tester.Var("y"));
            tester.Var("a").Index("3").Write(2);

            tester.Var("b").Write(tester.Var("a"));
            tester.Var("b").Index("4").Write(3);
            tester.Var("a").Index("4").Write(4);
            tester.Var("a").Index("5").Write(5);

            tester.Var("c").Write(tester.Var("a", "u"));

            tester.SetLogger(new FileLogger("aliasUnknownCopyArrayTest.txt"));
            tester.Test();
        }
    }
}
