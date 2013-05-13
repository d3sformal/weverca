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
        private Dictionary<string, IEnumerable<VirtualReference>> _items;

        /// <summary>
        /// Unknown field of container
        /// </summary>
        private IEnumerable<VirtualReference> _unknown;


        public AssociativeContainer(Dictionary<string, IEnumerable<VirtualReference>> items, IEnumerable<VirtualReference> unknown)
        {
            _items = new Dictionary<string, IEnumerable<VirtualReference>>(items);
            _unknown = new List<VirtualReference>(unknown);
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
