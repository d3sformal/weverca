using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel.Builders
{
    /// <summary>
    /// Simple prototype for container builder.
    /// </summary>
    public class ContainerSubBuilder
    {
        List<ContainerOperation> _operations = new List<ContainerOperation>();

        internal IEnumerable<ContainerOperation> Operations { get { return _operations; } }

        /// <summary>
        /// Assign possible values to specified key
        /// 
        /// NOTE:
        ///     Copying values is only copyiing .NET references on AbstractValue and creating one entry in memory context with these references
        /// </summary>
        /// <param name="key"></param>
        /// <param name="possibleValues"></param>
        public void Assign(string key, IEnumerable<AbstractValue> possibleValues)
        {
            _operations.Add(ContainerOperation.Assign(key, possibleValues));
        }

        /// <summary>
        /// Assign possible references to specified key
        /// 
        /// NOTE: 
        ///     Associative container stores VirtualReferences natively, it's enough to associate key with given possible references
        /// </summary>
        /// <param name="key"></param>
        /// <param name="possibleReferences"></param>
        public void AssignReferences(string key, IEnumerable<VirtualReference> possibleReferences)
        {
            _operations.Add(ContainerOperation.Assign(key,possibleReferences));
        }
    }
}
