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

namespace Weverca.MemoryModels.UnitTest.SnapshotTestFramework
{
    public class SnapshotTester<T> where T : SnapshotBase, new()
    {
        List<TestOperation<T>> operations = new List<TestOperation<T>>();

        TestOperationLogger logger = new BlankLogger();

        public T Snapshot { get; set; }

        public SnapshotTester()
        {
            Snapshot = new T();
            Snapshot.StartTransaction();
        }

        public SnapshotTester(T snapshot)
        {
            Snapshot = snapshot;
            Snapshot.StartTransaction();
        }

        public void SetLogger(TestOperationLogger logger)
        {
            this.logger = logger;
        }

        public SnapshotEntryFactory<T> Var(params string[] names)
        {
            return new SnapshotEntryFactory<T>(this, Snapshot.GetVariable(new VariableIdentifier(names)));
        }

        public void AddOperation(TestOperation<T> operation)
        {
            operations.Add(operation);
        }

        public void Test()
        {
            logger.Init(Snapshot);

            logger.WriteLine("Empty snapshot");
            logger.WriteLine(Snapshot.ToString());
            logger.WriteLine("------------------------------------------------");


            foreach (TestOperation<T> operation in operations)
            {
                operation.DoOperation(Snapshot, logger);
            }

            Snapshot.CommitTransaction();
            logger.Close(Snapshot);
        }

        public void Read(ReadWriteSnapshotEntryBase snapshotEntry)
        {
            AddOperation(new ReadOperation<T>(snapshotEntry));
        }

        public void Write(ReadWriteSnapshotEntryBase snapshotEntry, Value value)
        {
            AddOperation(new WriteOperation<T>(snapshotEntry, value));
        }

        public void Write(ReadWriteSnapshotEntryBase snapshotEntry, int value)
        {
            Write(snapshotEntry, Snapshot.CreateInt(value));
        }

        public void Write(ReadWriteSnapshotEntryBase snapshotEntry, string value)
        {
            Write(snapshotEntry, Snapshot.CreateString(value));
        }

        internal void Write(ReadWriteSnapshotEntryBase targetEntry, ReadWriteSnapshotEntryBase sourceEntry)
        {
            AddOperation(new WriteFromMemoryOperation<T>(targetEntry, sourceEntry));
        }

        internal void Merge(params T[] snapshots)
        {
            AddOperation(new MergeOperation<T>(snapshots));
        }

        internal void Alias(ReadWriteSnapshotEntryBase targetEntry, ReadWriteSnapshotEntryBase sourceEntry)
        {
            AddOperation(new AliasOperation<T>(targetEntry, sourceEntry));
        }
    }
}
