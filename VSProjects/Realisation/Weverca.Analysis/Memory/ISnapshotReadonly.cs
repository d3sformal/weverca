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
        /// Determine that variable exits in current snapshot
        /// <remarks>If global context is not forced, searches in local context, 
        /// or in global context in snapshot belonging to global code</remarks>
        /// </summary>
        /// <param name="variable">Tested variable</param>
        /// <param name="forceGlobalContext">Determine, that searching in global context has to be forced</param>
        /// <returns>True if variable exits, false otherwise</returns>
        bool VariableExists(VariableName variable, bool forceGlobalContext = false);

        /// <summary>
        /// Iterator for given object
        /// </summary>
        /// <param name="iteratedObject">Object which iterator will be created</param>
        /// <returns>Iterator for given object</returns>
        IEnumerable<ContainerIndex> IterateObject(ObjectValue iteratedObject);

        /// <summary>
        /// Create iterator for given array
        /// </summary>
        /// <param name="iteratedArray">Aray which iterator will be created</param>
        /// <returns>Iterators for given array</returns>
        IEnumerable<ContainerIndex> IterateArray(AssociativeArray iteratedArray);

        /// <summary>
        /// Get value from object at specified field
        /// </summary>
        /// <param name="objectValue">Object which field is resolved</param>
        /// <param name="field">Field where value will be searched</param>
        /// <returns>Value stored at given field in objectValue</returns>
        MemoryEntry GetField(ObjectValue objectValue, ContainerIndex field);

        /// <summary>
        /// Get value from array at specified index
        /// </summary>
        /// <param name="array">Array which index is resolved</param>
        /// <param name="index">Index where value will be searched</param>
        /// <returns>Value stored at given index in array</returns>
        MemoryEntry GetIndex(AssociativeArray array, ContainerIndex index);

        /// <summary>
        /// Variable where return value is stored
        /// </summary>
        VariableName ReturnValue { get; }

        /// <summary>
        /// Read info stored for given value
        /// </summary>
        /// <param name="value">value which info is readed</param>
        /// <returns>Stored info</returns>
        InfoValue[] ReadInfo(Value value);

        /// <summary>
        /// Read info stored for given variable
        /// </summary>
        /// <param name="variable">variable which info is readed</param>
        /// <returns>Stored info</returns>
        InfoValue[] ReadInfo(VariableName variable);

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

        /// <summary>
        /// Create alias for given variable
        /// </summary>
        /// <param name="sourceVar">Variable which alias will be created</param>
        /// <returns>Created alias</returns>
        AliasValue CreateAlias(VariableName sourceVar);


        /// <summary>
        /// Create alias for given index contained in array
        /// </summary>
        /// <param name="array">Array containing index</param>
        /// <param name="index">Aliased index</param>
        /// <returns>Created alias</returns>
        AliasValue CreateIndexAlias(AssociativeArray array, ContainerIndex index);

        /// <summary>
        /// Create alias for given field of objectValue
        /// </summary>
        /// <param name="objectValue">Value containing aliased field</param>
        /// <param name="field">Aliased field</param>
        /// <returns>Created alias</returns>
        AliasValue CreateFieldAlias(ObjectValue objectValue, ContainerIndex field);


        /// <summary>
        /// Resolves all possible functions for given functionName
        /// NOTE:
        ///     Multiple declarations for single functionName can happen for example because of branch merging
        /// </summary>
        /// <param name="functionName">Name of resolved function</param>
        /// <returns>Resolved functions</returns>
        IEnumerable<FunctionValue> ResolveFunction(QualifiedName functionName);

        /// <summary>
        /// Resolves all possible types for given typeName
        /// NOTE:
        ///     Multiple declarations for single typeName can happen for example because of branch merging
        /// </summary>
        /// <param name="typeName">Name of resolved type</param>
        /// <returns>Resolved types</returns>
        IEnumerable<TypeValue> ResolveType(QualifiedName typeName);
    }
}
