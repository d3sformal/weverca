using System.Collections.Generic;

using PHP.Core;

namespace Weverca.Analysis.Memory
{
    public interface ISnapshotReadonly
    {
        /// <summary>
        /// Gets variable where return value is stored
        /// </summary>
        /// <value>Variable name of return value storage</value>
        VariableName ReturnValue { get; }

        /// <summary>
        /// Determines whether variable exists in current snapshot
        /// </summary>
        /// <remarks>
        /// If global context is not forced, searches in local context,
        /// or in global context in snapshot belonging to global code
        /// </remarks>
        /// <param name="variable">Tested variable</param>
        /// <param name="forceGlobalContext">Determine that searching in global context has to be forced</param>
        /// <returns><c>true</c> if variable exists, <c>false</c> otherwise</returns>
        bool VariableExists(VariableName variable, bool forceGlobalContext = false);

        /// <summary>
        /// Determines whether field for the given object exists in current snapshot
        /// </summary>
        /// <param name="objectValue">Object which field is resolved</param>
        /// <param name="field">Field where value will be searched</param>
        /// <returns><c>true</c> if field for given object exists, <c>false</c> otherwise</returns>
        bool ObjectFieldExists(ObjectValue objectValue, ContainerIndex field);

        /// <summary>
        /// Determines whether element of index for the given array exists in current snapshot
        /// </summary>
        /// <param name="array">Array which index is resolved</param>
        /// <param name="index">Index where value will be searched</param>
        /// <returns><c>true</c> if element of index exists in given array, <c>false</c> otherwise</returns>
        bool ArrayIndexExists(AssociativeArray array, ContainerIndex index);

        /// <summary>
        /// Iterates over the given object
        /// </summary>
        /// <param name="iteratedObject">Object which iterator will be created</param>
        /// <returns>Iterator for given object</returns>
        IEnumerable<ContainerIndex> IterateObject(ObjectValue iteratedObject);

        /// <summary>
        /// Create iterator for given array
        /// </summary>
        /// <param name="iteratedArray">Array which iterator will be created</param>
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
        /// Tries to get value from object at specified field stored in current snapshot
        /// </summary>
        /// <param name="objectValue">Object which field is resolved</param>
        /// <param name="field">Field where value will be searched</param>
        /// <param name="entry">Value stored at given object field if exists, otherwise undefined value</param>
        /// <returns><c>true</c> if field for given object exists, <c>false</c> otherwise</returns>
        bool TryGetField(ObjectValue objectValue, ContainerIndex field, out MemoryEntry entry);

        /// <summary>
        /// Get value from array at specified index
        /// </summary>
        /// <param name="array">Array which index is resolved</param>
        /// <param name="index">Index where value will be searched</param>
        /// <returns>Value stored at given index in array</returns>
        MemoryEntry GetIndex(AssociativeArray array, ContainerIndex index);

        /// <summary>
        /// Tries to get value from array at specified index stored in current snapshot
        /// </summary>
        /// <param name="array">Array which index is resolved</param>
        /// <param name="index">Index where value will be searched</param>
        /// <param name="entry">Value stored at given index in array if exists, otherwise undefined value</param>
        /// <returns><c>true</c> if element of index exists in given array, <c>false</c> otherwise</returns>
        bool TryGetIndex(AssociativeArray array, ContainerIndex index, out MemoryEntry entry);

        /// <summary>
        /// Read info stored for given value
        /// </summary>
        /// <param name="value">Value which info is read</param>
        /// <returns>Stored info</returns>
        InfoValue[] ReadInfo(Value value);

        /// <summary>
        /// Read info stored for given variable
        /// </summary>
        /// <param name="variable">Variable which info is read</param>
        /// <returns>Stored info</returns>
        InfoValue[] ReadInfo(VariableName variable);

        /// <summary>
        /// Read value stored in snapshot for <paramref name="sourceVar" />
        /// </summary>
        /// <param name="sourceVar">Variable which value will be read</param>
        /// <returns>Value stored for given variable</returns>
        MemoryEntry ReadValue(VariableName sourceVar);

        /// <summary>
        /// Tries to read value stored in current snapshot for <paramref name="sourceVar" />
        /// </summary>
        /// <param name="sourceVar">Variable which value will be attempted to read</param>
        /// <param name="entry">Value stored for given variable if exists, otherwise undefined value</param>
        /// <param name="forceGlobalContext">Determine that searching in global context has to be forced</param>
        /// <returns><c>true</c> if variable exists, <c>false</c> otherwise</returns>
        bool TryReadValue(VariableName sourceVar, out MemoryEntry entry, bool forceGlobalContext = false);

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
