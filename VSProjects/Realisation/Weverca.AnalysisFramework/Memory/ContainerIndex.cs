using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Representation of index in container or object
    /// 
    /// NOTE:
    ///     Can be used as hash itself
    /// </summary>
    public class ContainerIndex
    {
        /// <summary>
        /// Container index identifier
        /// </summary>
        public readonly string Identifier;

        /// <summary>
        /// Creates container index identified by given identifier
        /// </summary>
        /// <param name="identifier">Index indentifier for container</param>
        internal ContainerIndex(string identifier)
        {
            Identifier = identifier;
        }
        
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }

        /// <inheritdoc />
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

            return o.Identifier == Identifier;
        }
    }
}
