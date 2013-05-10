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
        private Dictionary<AbstractValue, Reference> _items;


        public AssociativeContainer(Reference reference, Dictionary<AbstractValue, Reference> items)
            : base(reference)
        {
            _items = new Dictionary<AbstractValue, Reference>();
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
