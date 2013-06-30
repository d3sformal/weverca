using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.Analysis.Memory
{
    /// <summary>
    /// Representation of index in container or object
    /// 
    /// NOTE:
    ///     Can be used for hashing
    /// </summary>
    public class ContainerIndex
    {
        /// <summary>
        /// Container index identifier
        /// </summary>
        private readonly string _identifier;


        internal ContainerIndex(string identifier)
        {
            _identifier = identifier;
        }

        public override int GetHashCode()
        {
            return _identifier.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            var o = obj as ContainerIndex;

            if (o == null)
            {
                return false;
            }

            return o._identifier == _identifier;
        }

    }
}
