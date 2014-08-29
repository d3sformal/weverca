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

    public class AliasOperation<T>
        : TestOperation<T>
        where T : SnapshotBase
    {
        private ReadWriteSnapshotEntryBase targetEntry;
        private ReadWriteSnapshotEntryBase sourceEntry;

        public AliasOperation(ReadWriteSnapshotEntryBase targetEntry, ReadWriteSnapshotEntryBase sourceEntry)
        {
            this.targetEntry = targetEntry;
            this.sourceEntry = sourceEntry;
        }

        public void DoOperation(T snapshot, TestOperationLogger logger)
        {
            MemoryEntry entry = sourceEntry.ReadMemory(snapshot);
            targetEntry.SetAliases(snapshot, sourceEntry);

            logger.WriteLine("ALIAS: {0} = &{1}", targetEntry, sourceEntry);
            logger.WriteLine("VALUES: {0}", entry);
            logger.WriteLine("------------------------------------------------\n");
            logger.WriteLine(snapshot.ToString());
            logger.WriteLine("------------------------------------------------");
        }
    }

    public class MergeOperation<T>
        : TestOperation<T>
        where T : SnapshotBase
    {
        private T[] snapshots;

        public MergeOperation(T[] snapshots)
        {
            this.snapshots = snapshots;
        }


        public void DoOperation(T snapshot, TestOperationLogger logger)
        {
            snapshot.Extend(snapshots);

            int x = 1;
            foreach (T snap in snapshots)
            {
                logger.WriteLine("MERGE: {0}", x++);
                logger.WriteLine("------------------------------------------------\n");
                logger.WriteLine(snap.ToString());
                logger.WriteLine("------------------------------------------------");
            }

            logger.WriteLine("RESULT:");
            logger.WriteLine("------------------------------------------------\n");
            logger.WriteLine(snapshot.ToString());
            logger.WriteLine("------------------------------------------------");

        }
    }
}