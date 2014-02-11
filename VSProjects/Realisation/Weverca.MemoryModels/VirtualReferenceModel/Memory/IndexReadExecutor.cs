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
        public readonly List<Value> Result = new List<Value>();

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

        public override void VisitStringValue(StringValue value)
        {
            var subResult = _assistant.ReadStringIndex(value, _index);
            reportSubResult(subResult);
        }
        
        private void reportSubResult(IEnumerable<Value> subResult)
        {
            Result.AddRange(subResult);
        }
    }
}
