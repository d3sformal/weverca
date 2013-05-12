using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel.ValueImplementations
{
    /// <summary>
    /// Simple example how could be associative container implemented
    /// (Arrays can(should) also be treated as associative containers)
    /// </summary>
    public class AssociativeContainer : AbstractValue
    {
        /// <summary>
        /// Items that are stored in container.
        /// </summary>
        private Dictionary<AbstractValue,IEnumerable< VirtualReference>> _items;


        public AssociativeContainer(Dictionary<AbstractValue, IEnumerable<VirtualReference>> items)            
        {
            _items = new Dictionary<AbstractValue, IEnumerable<VirtualReference>>(items);
        }

        public override int DeepGetHashCode()
        {
            throw new NotImplementedException();
        }

        public override bool DeepEquals(object other)
        {
            throw new NotImplementedException();
        }
    }
}
