using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using PHP.Core.AST;

namespace Weverca.Analysis.Memory
{
    /// <summary>
    /// ObjectValue is used as "ticket" that allows snapshot API to operate on represented object
    /// NOTE: 
    ///     Is supposed to be used as Hash key for getting stored info in snapshot
    /// </summary>
    public sealed class ObjectValue : Value
    {
     
        /// <summary>
        /// Prevent creating arrays from outside
        /// </summary>
        internal ObjectValue()
        {
        }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitObjectValue(this);
        }
    }

    /// <summary>
    /// AssociativeArray is used as "ticket" that allows snapshot API to operate on represented array
    /// NOTE: 
    ///     * Is supposed to be used as Hash key for getting stored info in snapshot
    /// </summary>
    public sealed class AssociativeArray : Value
    {
        
        /// <summary>
        /// Prevent creating arrays from outside        
        /// </summary>
        internal AssociativeArray()
        {
        }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAssociativeArray(this);
        }
    }
}
