using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel.Builders
{
    /// <summary>
    /// Simple prototype for container builder.
    /// </summary>
    public class ContainerBuilder
    {
        /// <summary>
        /// Set 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="possibleValues"></param>
        public void Set(AbstractValue key, IEnumerable<AbstractValue> possibleValues)
        {
            //copying values is only copyiing .NET references on AbstractValue and creating one entry in memory context with these references
            throw new NotImplementedException();
        }

        public void SetReferences(AbstractValue key, IEnumerable<Reference> possibleReferences)
        {
            //Associative container stores references natively, it's enough to associate key with given possible references
            throw new NotImplementedException();
        }
    }
}
