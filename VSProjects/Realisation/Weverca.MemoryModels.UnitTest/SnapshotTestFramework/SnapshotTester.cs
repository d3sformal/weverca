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
    class SimpleInfo : InfoDataBase
    {
        internal readonly bool XssSanitized;

        internal SimpleInfo(bool xssSanitized)
        {
            XssSanitized = xssSanitized;
        }

        protected override int getHashCode()
        {
            return XssSanitized.GetHashCode();
        }

        protected override bool equals(InfoDataBase other)
        {
            var o = other as SimpleInfo;
            if (o == null)
                return false;

            return o.XssSanitized == XssSanitized;
        }
    }

    class SimpleAssistant : MemoryAssistantBase
    {
        public override MemoryEntry ReadAnyValueIndex(AnyValue value, MemberIdentifier index)
        {
            //copy info
            var info = value.GetInfo<SimpleInfo>();
            var indexed = Context.AnyValue.SetInfo(info);
            return new MemoryEntry(indexed);
        }

        public override MemoryEntry ReadAnyField(AnyValue value, VariableIdentifier field)
        {
            var info = value.GetInfo<SimpleInfo>();
            var indexed = Context.AnyValue.SetInfo(info);
            return new MemoryEntry(indexed);
        }

        public override MemoryEntry Widen(MemoryEntry old, MemoryEntry current)
        {
            return new MemoryEntry(Context.AnyValue);
        }

        public override IEnumerable<FunctionValue> ResolveMethods(Value thisObject, TypeValue type, PHP.Core.QualifiedName methodName, IEnumerable<FunctionValue> objectMethods)
        {
            foreach (var method in objectMethods)
            {
                if (method.Name.Value == methodName.Name.Value)
                {
                    yield return method;
                }
            }
        }

        public override IEnumerable<FunctionValue> ResolveMethods(TypeValue value, PHP.Core.QualifiedName methodName, IEnumerable<FunctionValue> objectMethods)
        {
            throw new NotImplementedException();
        }

        public override ObjectValue CreateImplicitObject()
        {
            throw new NotImplementedException();
        }

        public override void TriedIterateFields(Value value)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Value> ReadStringIndex(StringValue value, MemberIdentifier index)
        {
            var indexNum = int.Parse(index.DirectName);

            yield return Context.CreateString(value.Value.Substring(indexNum, 1));
        }

        public override IEnumerable<Value> WriteStringIndex(StringValue indexed, MemberIdentifier index, MemoryEntry writtenValue)
        {
            var indexNum = int.Parse(index.DirectName);
            var writtenChar = writtenValue.PossibleValues.First() as StringValue;

            var result = new StringBuilder(indexed.Value);
            result[indexNum] = writtenChar.Value[0];

            var resultValue = Context.CreateString(result.ToString());
            yield return resultValue;
        }

        public override IEnumerable<Value> ReadValueIndex(Value value, MemberIdentifier index)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Value> WriteValueIndex(Value indexed, MemberIdentifier index, MemoryEntry writtenValue)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry Simplify(MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Value> WriteValueField(Value fielded, VariableIdentifier field, MemoryEntry writtenValue)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Value> ReadValueField(Value fielded, VariableIdentifier field)
        {
            throw new NotImplementedException();
        }

        public override void TriedIterateIndexes(Value value)
        {
            throw new NotImplementedException();
        }
    }

    public class SnapshotTester<T> where T : SnapshotBase, new()
    {
        List<TestOperation<T>> operations = new List<TestOperation<T>>();

        TestOperationLogger logger = new BlankLogger();

        public T Snapshot { get; set; }

        public SnapshotTester()
        {
            Snapshot = new T();
            Snapshot.InitAssistant(new SimpleAssistant());
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
