using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel.Builders
{
    class ContainerOperation
    {
        internal readonly string Key;

        internal readonly IEnumerable<VirtualReference> AssignedReferences;

        internal readonly IEnumerable<AbstractValue> AssignedValues;

        private ContainerOperation(string key,IEnumerable<VirtualReference> references, IEnumerable<AbstractValue> values){
            Key = key;
            AssignedReferences = references;
            AssignedValues = values;
        }

        internal static ContainerOperation Assign(string key, IEnumerable<VirtualReference> references)
        {
            return new ContainerOperation(key, new List<VirtualReference>(references),null);
        }

        internal static ContainerOperation Assign(string key, IEnumerable<AbstractValue> values)
        {
            return new ContainerOperation(key,null, new List<AbstractValue>(values));
        }

    }
}
