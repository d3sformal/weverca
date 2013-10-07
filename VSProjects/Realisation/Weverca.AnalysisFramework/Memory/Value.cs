using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// All implementations of value has to be immutable
    /// </summary>
    public abstract class Value
    {
        /// <summary>
        /// Unique id for every instance of value
        /// </summary>
        public readonly int UID;

        private static int _lastValueUID = 0;

        internal Value()
        {
            UID = ++_lastValueUID;
        }

        public virtual void Accept(IValueVisitor visitor)
        {
            visitor.VisitValue(this);
        }
    }
}
