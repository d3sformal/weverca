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

namespace Weverca.MemoryModels.UnitTest.CopyMemoryModel
{
    [TestClass]
    public class AssignTest
    {
        [TestMethod]
        public void assignTest()
        {
            Snapshot snapshot = new Snapshot();
            snapshot.StartTransaction();

            StreamWriter writer = new StreamWriter("assignTest.txt");

            writer.WriteLine("Blank snapshot");
            writer.WriteLine(snapshot.DumpSnapshot());
            writer.WriteLine("------------------------------------------------");

            ReadWriteSnapshotEntryBase snapshotEntry = snapshot.GetVariable(new VariableIdentifier("integer"));
            snapshotEntry.WriteMemory(snapshot, new MemoryEntry(snapshot.CreateInt(1)));

            writer.WriteLine("$integer = 1");
            writer.WriteLine(snapshot.DumpSnapshot());
            writer.WriteLine("------------------------------------------------");


            writer.Close();
            snapshot.CommitTransaction();
        }

        [TestMethod]
        public void assignArrayTest()
        {
            Snapshot snapshot = new Snapshot();
            snapshot.StartTransaction();

            ReadWriteSnapshotEntryBase seA = snapshot.GetVariable(new VariableIdentifier("a"));

            ReadWriteSnapshotEntryBase seA5 = seA.ReadIndex(snapshot, new MemberIdentifier(new String[] { "5" }));
            ReadWriteSnapshotEntryBase seA5U = seA5.ReadIndex(snapshot, new MemberIdentifier(new String[] { }));

            ReadWriteSnapshotEntryBase seAU = seA.ReadIndex(snapshot, new MemberIdentifier(new String[] { }));
            ReadWriteSnapshotEntryBase seAUU = seAU.ReadIndex(snapshot, new MemberIdentifier(new String[] { }));
            ReadWriteSnapshotEntryBase seAU3 = seAU.ReadIndex(snapshot, new MemberIdentifier(new String[] { "3" }));

            StreamWriter writer = new StreamWriter("assignArrayTest.txt");

            writer.WriteLine("Empty snapshot");
            writer.WriteLine(snapshot.DumpSnapshot());
            writer.WriteLine("------------------------------------------------");

            seA5U.WriteMemory(snapshot, new MemoryEntry(snapshot.CreateInt(1)));
            writer.WriteLine("$a[5][?] = 1");
            writer.WriteLine("------------------------------------------------\n");
            writer.WriteLine(snapshot.DumpSnapshot());
            writer.WriteLine("------------------------------------------------");

            seAUU.WriteMemory(snapshot, new MemoryEntry(snapshot.CreateInt(2)));
            writer.WriteLine("$a[?][?] = 2");
            writer.WriteLine("------------------------------------------------\n");
            writer.WriteLine(snapshot.DumpSnapshot());
            writer.WriteLine("------------------------------------------------");

            seAU3.WriteMemory(snapshot, new MemoryEntry(snapshot.CreateInt(3)));
            writer.WriteLine("$a[?][3] = 3");
            writer.WriteLine("------------------------------------------------\n");
            writer.WriteLine(snapshot.DumpSnapshot());
            writer.WriteLine("------------------------------------------------");

            writer.Close();
            snapshot.CommitTransaction();
        }

        [TestMethod]
        public void assignArrayFromUnknownTest()
        {
            Snapshot snapshot = new Snapshot();
            snapshot.StartTransaction();

            ReadWriteSnapshotEntryBase seA = snapshot.GetVariable(new VariableIdentifier("a"));

            ReadWriteSnapshotEntryBase seA5 = seA.ReadIndex(snapshot, new MemberIdentifier(new String[] { "5" }));
            ReadWriteSnapshotEntryBase seA5U = seA5.ReadIndex(snapshot, new MemberIdentifier(new String[] { }));
            ReadWriteSnapshotEntryBase seA56 = seA5.ReadIndex(snapshot, new MemberIdentifier(new String[] { "6" }));

            ReadWriteSnapshotEntryBase seAU = seA.ReadIndex(snapshot, new MemberIdentifier(new String[] { }));
            ReadWriteSnapshotEntryBase seAU3 = seAU.ReadIndex(snapshot, new MemberIdentifier(new String[] { "3" }));
            ReadWriteSnapshotEntryBase seAUU = seAU.ReadIndex(snapshot, new MemberIdentifier(new String[] { }));
            ReadWriteSnapshotEntryBase seAUU9 = seAUU.ReadIndex(snapshot, new MemberIdentifier(new String[] { "9" }));

            StreamWriter writer = new StreamWriter("assignArrayFromUnknownTest.txt");

            writer.WriteLine("Empty snapshot");
            writer.WriteLine(snapshot.DumpSnapshot());
            writer.WriteLine("------------------------------------------------");

            seAUU.WriteMemory(snapshot, new MemoryEntry(snapshot.CreateInt(1)));
            writer.WriteLine("$a[?][?] = 1");
            writer.WriteLine("------------------------------------------------\n");
            writer.WriteLine(snapshot.DumpSnapshot());
            writer.WriteLine("------------------------------------------------");

            seAU3.WriteMemory(snapshot, new MemoryEntry(snapshot.CreateInt(2)));
            writer.WriteLine("$a[?][3] = 2");
            writer.WriteLine("------------------------------------------------\n");
            writer.WriteLine(snapshot.DumpSnapshot());
            writer.WriteLine("------------------------------------------------");

            seA5U.WriteMemory(snapshot, new MemoryEntry(snapshot.CreateInt(3)));
            writer.WriteLine("$a[5][?] = 3");
            writer.WriteLine("------------------------------------------------\n");
            writer.WriteLine(snapshot.DumpSnapshot());
            writer.WriteLine("------------------------------------------------");

            seA56.WriteMemory(snapshot, new MemoryEntry(snapshot.CreateInt(4)));
            writer.WriteLine("$a[5][6] = 4");
            writer.WriteLine("------------------------------------------------\n");
            writer.WriteLine(snapshot.DumpSnapshot());
            writer.WriteLine("------------------------------------------------");

            seAUU9.WriteMemory(snapshot, new MemoryEntry(snapshot.CreateInt(5)));
            writer.WriteLine("$a[?][?][9] = 5");
            writer.WriteLine("------------------------------------------------\n");
            writer.WriteLine(snapshot.DumpSnapshot());
            writer.WriteLine("------------------------------------------------");

            writer.Close();
            snapshot.CommitTransaction();
        }

        [TestMethod]
        public void assignArrayConcretizationTest()
        {
            Snapshot snapshot = new Snapshot();
            snapshot.StartTransaction();

            ReadWriteSnapshotEntryBase seA = snapshot.GetVariable(new VariableIdentifier("a"));

            ReadWriteSnapshotEntryBase seA1 = seA.ReadIndex(snapshot, new MemberIdentifier(new String[] { "1" }));
            
            ReadWriteSnapshotEntryBase seA1U = seA1.ReadIndex(snapshot, new MemberIdentifier(new String[] { }));
            ReadWriteSnapshotEntryBase seA12 = seA1.ReadIndex(snapshot, new MemberIdentifier(new String[] { "2" }));

            ReadWriteSnapshotEntryBase seA1U3 = seA1U.ReadIndex(snapshot, new MemberIdentifier(new String[] { "3" }));
            ReadWriteSnapshotEntryBase seA123 = seA12.ReadIndex(snapshot, new MemberIdentifier(new String[] { "3" }));

            ReadWriteSnapshotEntryBase seAU = seA.ReadIndex(snapshot, new MemberIdentifier(new String[] { }));
            ReadWriteSnapshotEntryBase seAU2 = seAU.ReadIndex(snapshot, new MemberIdentifier(new String[] { "2" }));
            ReadWriteSnapshotEntryBase seAU23 = seAU2.ReadIndex(snapshot, new MemberIdentifier(new String[] { "3" }));

            StreamWriter writer = new StreamWriter("assignArrayConcretizationTest.txt");

            writer.WriteLine("Empty snapshot");
            writer.WriteLine(snapshot.DumpSnapshot());
            writer.WriteLine("------------------------------------------------");

            seAU23.WriteMemory(snapshot, new MemoryEntry(snapshot.CreateInt(1)));
            writer.WriteLine("$a[?][2][3] = 1");
            writer.WriteLine("------------------------------------------------\n");
            writer.WriteLine(snapshot.DumpSnapshot());
            writer.WriteLine("------------------------------------------------");

            seA1U3.WriteMemory(snapshot, new MemoryEntry(snapshot.CreateInt(2)));
            writer.WriteLine("$a[1][?][3] = 2");
            writer.WriteLine("------------------------------------------------\n");
            writer.WriteLine(snapshot.DumpSnapshot());
            writer.WriteLine("------------------------------------------------");

            seA123.WriteMemory(snapshot, new MemoryEntry(snapshot.CreateInt(3)));
            writer.WriteLine("$a[1][2][3] = 3");
            writer.WriteLine("------------------------------------------------\n");
            writer.WriteLine(snapshot.DumpSnapshot());
            writer.WriteLine("------------------------------------------------");


            writer.Close();
            snapshot.CommitTransaction();
        }
    }
}
