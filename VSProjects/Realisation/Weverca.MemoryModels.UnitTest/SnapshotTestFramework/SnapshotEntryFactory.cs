using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.UnitTest.SnapshotTestFramework
{
    public class SnapshotEntryFactory<T> where T : SnapshotBase, new()
    {
        private ReadWriteSnapshotEntryBase snapshotEntry;
        private SnapshotTester<T> tester;

        public SnapshotEntryFactory(SnapshotTester<T> tester, ReadWriteSnapshotEntryBase readWriteSnapshotEntryBase)
        {
            this.snapshotEntry = readWriteSnapshotEntryBase;
            this.tester = tester;
        }

        public SnapshotEntryFactory<T> Index(params string[] indexes)
        {
            return new SnapshotEntryFactory<T>(tester, snapshotEntry.ReadIndex(tester.Snapshot, new MemberIdentifier(indexes)));
        }

        public SnapshotEntryFactory<T> Field(params string[] fields)
        {
            return new SnapshotEntryFactory<T>(tester, snapshotEntry.ReadField(tester.Snapshot, new VariableIdentifier(fields)));
        }

        public ReadWriteSnapshotEntryBase getEntry()
        {
            return snapshotEntry;
        }


        public void Read()
        {
            tester.Read(getEntry());
        }

        public void Write(SnapshotEntryFactory<T> source)
        {
            tester.Write(getEntry(), source.snapshotEntry);
        }

        public void Write(Value value)
        {
            tester.Write(getEntry(), value);
        }

        public void Write(int value)
        {
            tester.Write(getEntry(), value);
        }

        public void Write(string value)
        {
            tester.Write(getEntry(), value);
        }

        internal void Alias(SnapshotEntryFactory<T> source)
        {
            tester.Alias(getEntry(), source.snapshotEntry);
        }
    }
}
