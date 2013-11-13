using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class CollectComposedValuesVisitor : AbstractValueVisitor
    {
        public readonly HashSet<AssociativeArray> Arrays = new HashSet<AssociativeArray>();
        public readonly HashSet<ObjectValue> Objects = new HashSet<ObjectValue>();
        public readonly HashSet<Value> Values = new HashSet<Value>();

        public Snapshot Snapshot { get; set; }

        public override void VisitValue(Value value)
        {
            Values.Add(value);
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            Objects.Add(value);
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            Arrays.Add(value);
        }
    }
}
