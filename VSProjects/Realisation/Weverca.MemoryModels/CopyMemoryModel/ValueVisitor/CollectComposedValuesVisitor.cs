using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;
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

        public IEnumerable<VariableIdentifier> CollectFields(Snapshot snapshot)
        {
            HashSet<VariableIdentifier> fields = new HashSet<VariableIdentifier>();
            foreach (ObjectValue objectValue in Objects)
            {
                ObjectDescriptor descriptor = snapshot.Structure.GetDescriptor(objectValue);
                foreach (string index in descriptor.Indexes.Keys)
                {
                    fields.Add(new VariableIdentifier(index));
                }
            }

            return fields;
        }

        public IEnumerable<MemberIdentifier> CollectIndexes(Snapshot snapshot)
        {
            HashSet<MemberIdentifier> indexes = new HashSet<MemberIdentifier>();
            foreach (AssociativeArray arrayValue in Arrays)
            {
                ArrayDescriptor descriptor = snapshot.Structure.GetDescriptor(arrayValue);
                foreach (string index in descriptor.Indexes.Keys)
                {
                    indexes.Add(new MemberIdentifier(index));
                }
            }

            return indexes;
        }

        public IEnumerable<TypeValue> ResolveObjectsTypes(Snapshot snapshot)
        {
            HashSet<TypeValue> types = new HashSet<TypeValue>();
            foreach (ObjectValue objectValue in Objects)
            {
                ObjectDescriptor descriptor = snapshot.Structure.GetDescriptor(objectValue);
                types.Add(descriptor.Type);
            }

            return types;
        }
    }
}
