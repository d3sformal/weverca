using System;
using System.Collections.Generic;

using PHP.Core;

namespace Weverca.AnalysisFramework.Memory
{
    public interface ISnapshotReadonly
    {
        #region Value singletons

        AnyValue AnyValue { get; }
        UndefinedValue UndefinedValue { get; }
        AnyStringValue AnyStringValue { get; }
        AnyBooleanValue AnyBooleanValue { get; }
        AnyIntegerValue AnyIntegerValue { get; }
        AnyLongintValue AnyLongintValue { get; }
        AnyObjectValue AnyObjectValue { get; }
        AnyArrayValue AnyArrayValue { get; }

        #endregion

        #region Snapshot entry API
        /// <summary>
        /// Get snapshot entry providing reading,... services for variable
        /// </summary>
        /// <remarks>
        /// If global context is not forced, searches in local context (there can be 
        /// fetched some variables from global context also),
        /// or in global context in snapshot belonging to global code
        /// </remarks>
        /// <param name="name">Name of variable</param>
        /// <param name="forceGlobalContext">Determine that searching in global context has to be forced</param>
        /// <returns>Readable snapshot entry for variable identifier</returns>
        ReadSnapshotEntryBase ReadVariable(VariableIdentifier variable, bool forceGlobalContext = false);

        /// <summary>
        /// Get snapshot entry for variable, used for extra info controlling. Control entries may share names with other variables,
        /// indexes or fields. Control entries are not affected by unknown fields, also they cannot be aliased to non-control
        /// entries and vica-versa.
        /// </summary>
        /// <param name="variable">Variable determining control entry</param>
        /// <returns>Created control entry</returns>
        ReadSnapshotEntryBase ReadControlVariable(VariableName variable);

        /// <summary>
        /// Get snapshot entry for variable, used for extra info controlling in local context. Control entries may share names with other variables,
        /// indexes or fields. Control entries are not affected by unknown fields, also they cannot be aliased to non-control
        /// entries.
        /// </summary>
        /// <param name="variable">Variable determining control entry</param>
        /// <returns>Created control entry</returns>
        ReadSnapshotEntryBase ReadLocalControlVariable(VariableName variable);

        #endregion
       
        [Obsolete("Use snapshot entry API instead")]
        /// <summary>
        /// Iterates over the given object
        /// </summary>
        /// <param name="iteratedObject">Object which iterator will be created</param>
        /// <returns>Iterator for given object</returns>
        IEnumerable<ContainerIndex> IterateObject(ObjectValue iteratedObject);

        [Obsolete("Use snapshot entry API instead")]
        /// <summary>
        /// Create iterator for given array
        /// </summary>
        /// <param name="iteratedArray">Array which iterator will be created</param>
        /// <returns>Iterators for given array</returns>
        IEnumerable<ContainerIndex> IterateArray(AssociativeArray iteratedArray);

        [Obsolete("Use snapshot entry API instead")]
        /// <summary>
        /// Determine type of given object
        /// </summary>
        /// <param name="objectValue">Object which type is resolved</param>
        /// <returns>Type of given object</returns>
        TypeValue ObjectType(ObjectValue objectValue);

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

        [Obsolete("Use snapshot entry API instead")]
        /// <summary>
        /// Read value stored in snapshot for <paramref name="sourceVar" />
        /// </summary>
        /// <param name="sourceVar">Variable which value will be read</param>
        /// <returns>Value stored for given variable</returns>
        MemoryEntry ReadValue(VariableName sourceVar);

        [Obsolete("Use snapshot entry API instead")]
        /// <summary>
        /// Tries to read value stored in current snapshot for <paramref name="sourceVar" />
        /// </summary>
        /// <param name="sourceVar">Variable which value will be attempted to read</param>
        /// <param name="entry">Value stored for given variable if exists, otherwise undefined value</param>
        /// <param name="forceGlobalContext">Determine that searching in global context has to be forced</param>
        /// <returns><c>true</c> if variable exists, <c>false</c> otherwise</returns>
        bool TryReadValue(VariableName sourceVar, out MemoryEntry entry, bool forceGlobalContext = false);

        [Obsolete("Use snapshot entry API instead")]
        /// <summary>
        /// Creates index for given identifier
        /// </summary>
        /// <param name="identifier">Identifier of index</param>
        /// <returns>Created index</returns>
        ContainerIndex CreateIndex(string identifier);

        [Obsolete("Use snapshot entry API instead")]
        /// <summary>
        /// Create alias for given variable
        /// </summary>
        /// <param name="sourceVar">Variable which alias will be created</param>
        /// <returns>Created alias</returns>
        AliasValue CreateAlias(VariableName sourceVar);

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

        IEnumerable<FunctionValue> ResolveStaticMethod(TypeValue value, QualifiedName methodName);

    }
}
