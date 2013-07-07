using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Weverca.Analysis.Memory
{
    /// <summary>
    /// All implementations of value has to be immutable
    /// </summary>
    public abstract class Value
    {

        public virtual void Accept(IValueVisitor visitor)
        {
            visitor.VisitValue(this);
        }
    }
}
