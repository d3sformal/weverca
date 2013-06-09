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
        public abstract MemoryEntry GetField(ContainerIndex field);

        /// <summary>
        /// Set value of specified field
        /// </summary>
        /// <param name="field">Index of field</param>
        /// <param name="value">Value for given field</param>
        /// <returns>Copy of object with field containing given value</returns>
        public abstract ObjectValue SetField(ContainerIndex field, MemoryEntry value);

        /// <summary>
        /// Get alias for given field
        /// </summary>        
        /// <param name="index">Index determining field for created alias</param>
        /// <returns>Alias representation for given index</returns>
        /// <example>&$this->a</example>
        public abstract AliasValue CreateAlias(ContainerIndex field);

        /// <summary>
        /// Returns function defined by given name
        /// </summary>
        /// <param name="functionName">Name of function</param>
        /// <returns>Function declared for given name</returns>
        public abstract IEnumerable<FunctionDecl> GetFunction(string functionName);

        /// <summary>
        /// Creates copy of object with defined function
        /// </summary>        
        /// <param name="function">Function that is defined</param>
        /// <returns>Function defined for given name</returns>
        public abstract ObjectValue DefineFunction(string functionName, FunctionDecl function);

        /// <summary>
        /// NOTE:
        ///     Prevent creating objects outside of assembly
        /// </summary>
        internal ObjectValue()
        {
        }
    }

    public abstract class AssociativeArray : Value
    {
        /// <summary>
        /// Get value on specified index
        /// </summary>
        /// <param name="index">Index of field</param>
        /// <returns>Value for given field</returns>
        public abstract MemoryEntry GetIndex(ContainerIndex index);

        /// <summary>
        /// Set value of specified field
        /// </summary>
        /// <param name="index">Index of field</param>
        /// <param name="value">Value for given field</param>
        /// <returns>Copy of object with field containing given value</returns>
        public abstract ObjectValue SetIndex(ContainerIndex index, MemoryEntry value);

        /// <summary>
        /// Get alias for given index
        /// </summary>        
        /// <param name="index">Index determining index for created alias</param>
        /// <returns>Alias representation for given index</returns>
        /// <example>&$a[5]</example>
        public abstract AliasValue CreateAlias(ContainerIndex index);
        /// <summary>
        /// NOTE:
        ///     Prevent creating objects outside of assembly
        /// </summary>
        internal AssociativeArray()
        {
        }
    }
}
