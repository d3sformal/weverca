/*
Copyright (c) 2012-2014 Miroslav Vodolan.

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

namespace Weverca.MemoryModels.VirtualReferenceModel.Memory
{
    /// <summary>
    /// Visitor providing index read operation on values forwarded to memory assistant
    /// </summary>
    class IndexReadExecutor : AbstractValueVisitor
    {
        /// <summary>
        /// Assistant available for operation execution
        /// </summary>
        private readonly MemoryAssistantBase _assistant;

        /// <summary>
        /// Written index determine where value will be written
        /// </summary>
        private readonly MemberIdentifier _index;

        /// <summary>
        /// Result of index reading (only non array indexes are considered)
        /// </summary>
        public readonly HashSet<Value> Result = new HashSet<Value>();

        public IndexReadExecutor(MemoryAssistantBase assistant, MemberIdentifier index)
        {
            _assistant = assistant;
            _index = index;
        }
        public override void VisitValue(Value value)
        {
            var subResult=_assistant.ReadValueIndex(value, _index);
            reportSubResult(subResult);
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            //Nothing to do, arrays are resolved in snapshot entries
            Result.Add(value);
        }

        public override void VisitAnyValue(AnyValue value)
        {
            //Nothing to do, any values are resolved similar to arrays
            Result.Add(value);
        }

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            //Nothing to do, undefined values are resolved as implicit arrays
            Result.Add(value);
        }

        public override void VisitStringValue(StringValue value)
        {
            var subResult = _assistant.ReadStringIndex(value, _index);
            reportSubResult(subResult);
        }
        
        private void reportSubResult(IEnumerable<Value> subResult)
        {
            Result.UnionWith(subResult);
        }
    }
}