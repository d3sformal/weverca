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
        public override MemoryEntry ReadIndex(AnyValue value, MemberIdentifier index)
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

        public override IEnumerable<FunctionValue> ResolveMethods(Value thisObject, TypeValue type,PHP.Core.QualifiedName methodName, IEnumerable<FunctionValue> objectMethods)
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

        public override ObjectValue GetImplicitObject()
        {
            throw new NotImplementedException();
        }
    }
}
