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
    public interface TestOperation<T> where T : SnapshotBase
    {
        void DoOperation(T snapshot, TestOperationLogger logger);
    }

    public class ReadOperation<T>
        : TestOperation<T>
        where T : SnapshotBase
    {
        private ReadWriteSnapshotEntryBase snapshotEntry;

        public ReadOperation(ReadWriteSnapshotEntryBase snapshotEntry)
        {
            // TODO: Complete member initialization
            this.snapshotEntry = snapshotEntry;
        }

        public void DoOperation(T snapshot, TestOperationLogger logger)
        {
            MemoryEntry entry = snapshotEntry.ReadMemory(snapshot);

            logger.WriteLine("READ: {0}", snapshotEntry);
            logger.WriteLine(entry.ToString());
            logger.WriteLine("------------------------------------------------");
        }
    }

    public class WriteOperation<T>
        : TestOperation<T>
        where T : SnapshotBase
    {
        private ReadWriteSnapshotEntryBase snapshotEntry;
        private Value value;

        public WriteOperation(ReadWriteSnapshotEntryBase snapshotEntry, Value value)
        {
            // TODO: Complete member initialization
            this.snapshotEntry = snapshotEntry;
            this.value = value;
        }

        public void DoOperation(T snapshot, TestOperationLogger logger)
        {
            MemoryEntry entry = new MemoryEntry(value);
            snapshotEntry.WriteMemory(snapshot, entry);

            logger.WriteLine("WRITE: {0} = {1}", snapshotEntry, value);
            logger.WriteLine("------------------------------------------------\n");
            logger.WriteLine(snapshot.ToString());
            logger.WriteLine("------------------------------------------------");
        }
    }

    public class WriteFromMemoryOperation<T>
        : TestOperation<T>
        where T : SnapshotBase
    {
        private ReadWriteSnapshotEntryBase targetEntry;
        private ReadWriteSnapshotEntryBase sourceEntry;

        public WriteFromMemoryOperation(ReadWriteSnapshotEntryBase targetEntry, ReadWriteSnapshotEntryBase sourceEntry)
        {
            this.targetEntry = targetEntry;
            this.sourceEntry = sourceEntry;
        }

        public void DoOperation(T snapshot, TestOperationLogger logger)
        {
            MemoryEntry entry = sourceEntry.ReadMemory(snapshot);
            targetEntry.WriteMemory(snapshot, entry);

            logger.WriteLine("WRITE: {0} = {1}", targetEntry, sourceEntry);
            logger.WriteLine("VALUES: {0}", entry);
            logger.WriteLine("------------------------------------------------\n");
            logger.WriteLine(snapshot.ToString());
            logger.WriteLine("------------------------------------------------");
        }
    }
}
