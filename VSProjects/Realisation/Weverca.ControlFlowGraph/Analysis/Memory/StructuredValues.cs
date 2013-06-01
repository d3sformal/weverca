using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.ControlFlowGraph.Analysis.Memory
{
    public abstract class ObjectValue : Value
    {
        /// <summary>
        /// Get value of specified field
        /// </summary>
        /// <param name="field">Index of field</param>
        /// <returns>Value for given field</returns>
        public Value GetValue(ContainerIndex field)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Get alias for given index
        /// </summary>        
        /// <param name="index">Index determining field for created alias</param>
        /// <returns>Alias representation for given index</returns>
        /// <example>&$a[5]</example>
        /// <example>&$this->a</example>
        public AliasValue GetAlias(ContainerIndex field)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns function defined by given name
        /// </summary>
        /// <param name="functionName">Name of function</param>
        /// <returns>Function declared for given name</returns>
        public IEnumerable<FunctionDecl> GetFunction(string functionName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates copy of object with defined function
        /// </summary>        
        /// <param name="function">Function that is defined</param>
        /// <returns>Function defined for given name</returns>
        public ObjectValue DefineFunction(string functionName,FunctionDecl function){            
            throw new NotImplementedException();
        }
    }
}
