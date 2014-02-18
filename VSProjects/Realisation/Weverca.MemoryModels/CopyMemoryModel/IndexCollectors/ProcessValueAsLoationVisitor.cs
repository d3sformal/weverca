using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    abstract class ProcessValueAsLocationVisitor : ICollectedLocationVisitor
    {
        public bool IsMust { get; set; }

        MemoryAssistantBase assistant;
        public ProcessValueAsLocationVisitor(MemoryAssistantBase assistant)
        {
            this.assistant = assistant;
        }

        public abstract void ProcessValues(MemoryIndex parentIndex, IEnumerable<Value> values, bool isMust);

        public void VisitObjectValueLocation(ObjectValueLocation location)
        {
            IEnumerable<Value> values = location.ReadValues(assistant);
        }

        public void VisitObjectAnyValueLocation(ObjectAnyValueLocation location)
        {
            IEnumerable<Value> values = location.ReadValues(assistant);
            ProcessValues(location.ContainingIndex, values, IsMust);
        }

        public void VisitArrayValueLocation(ArrayValueLocation location)
        {
            IEnumerable<Value> values = location.ReadValues(assistant);
        }

        public void VisitArrayAnyValueLocation(ArrayAnyValueLocation location)
        {
            IEnumerable<Value> values = location.ReadValues(assistant);
            ProcessValues(location.ContainingIndex, values, IsMust);
        }

        public void VisitArrayStringValueLocation(ArrayStringValueLocation location)
        {
            IEnumerable<Value> values = location.ReadValues(assistant);
            ProcessValues(null, values, IsMust);
        }

        public void VisitArrayUndefinedValueLocation(ArrayUndefinedValueLocation location)
        {
            IEnumerable<Value> values = location.ReadValues(assistant);
            ProcessValues(null, values, IsMust);
        }

        public void VisitObjectUndefinedValueLocation(ObjectUndefinedValueLocation location)
        {
            IEnumerable<Value> values = location.ReadValues(assistant);
            ProcessValues(null, values, IsMust);
        }


        public void VisitInfoValueLocation(InfoValueLocation location)
        {
            IEnumerable<Value> values = location.ReadValues(assistant);
            ProcessValues(location.ContainingIndex, values, IsMust);
        }


        public void VisitAnyStringValueLocation(AnyStringValueLocation location)
        {
            IEnumerable<Value> values = location.ReadValues(assistant);
            ProcessValues(null, values, IsMust);
        }
    }
}
