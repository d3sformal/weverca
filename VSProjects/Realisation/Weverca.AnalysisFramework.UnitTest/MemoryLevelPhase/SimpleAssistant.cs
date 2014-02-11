using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework.UnitTest
{
    class SimpleAssistant : MemoryAssistantBase
    {
        public override MemoryEntry ReadAnyValueIndex(AnyValue value, MemberIdentifier index)
        {
            //copy info
            var info = value.GetInfo<SimpleInfo>();
            var indexed = Context.AnyValue.SetInfo(info);
            return new MemoryEntry(indexed);
        }

        public override MemoryEntry ReadField(AnyValue value, VariableIdentifier field)
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
    }
}
