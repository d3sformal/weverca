/*
Copyright (c) 2012-2014 David Hauzar, Miroslav Vodolan

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
}