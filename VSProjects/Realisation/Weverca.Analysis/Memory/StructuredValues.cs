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
        private static int _objectID;

        /// <summary>
        /// Global unique ID for object.
        /// NOTE:        
        ///     * It's OK to use ObjectValue as hash itself
        ///     * ObjectID is meant to be helper for object naming
        /// </summary>
        public readonly int ObjectID;
        /// <summary>
        /// Prevent creating arrays from outside
        /// NOTE:
        ///     * IS NOT THREAD SAFE
        /// </summary>
        internal ObjectValue()
        {
            ObjectID = ++_objectID;
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
        private static int _arrayID;

        /// <summary>
        /// Global unique ID for array.
        /// NOTE:
        ///     * It's OK to use array as hash itself
        ///     * ArrayID is meant to be helper for array naming
        /// </summary>
        public readonly int ArrayID;
        /// <summary>
        /// Prevent creating arrays from outside
        /// NOTE:
        ///     * IS NOT THREAD SAFE
        /// </summary>
        internal AssociativeArray()
        {
            ArrayID = ++_arrayID;
        }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAssociativeArray(this);
        }
    }
}
