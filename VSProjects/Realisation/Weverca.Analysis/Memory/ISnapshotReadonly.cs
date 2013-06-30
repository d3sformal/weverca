using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
namespace Weverca.Analysis.Memory
{
    public interface ISnapshotReadonly
    {
        /// <summary>
        /// Variable where return value is stored
        /// </summary>
        VariableName ReturnValue { get; }

        /// <summary>
        /// Returns variable where argument on zero based index position is stored
        /// </summary>
        /// <param name="index">Index of argument</param>
        /// <returns>Name of variable</returns>
        VariableName Argument(int index);

        /// <summary>
        /// Read value stored in snapshot for sourceVar
        /// </summary>
        /// <param name="sourceVar">Variable which value will be readed</param>
        /// <returns>Value stored for given variable</returns>
        MemoryEntry ReadValue(VariableName sourceVar);

         /// <summary>
        /// Creates index for given identifier
        /// </summary>
        /// <param name="identifier">Identifier of index</param>
        /// <returns>Created index</returns>
        ContainerIndex CreateIndex(string identifier);
    }
}
