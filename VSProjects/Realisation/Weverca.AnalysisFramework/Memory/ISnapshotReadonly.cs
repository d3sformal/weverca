/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


/*
Copyright (c) 2012-2014 David Hauzar and Mirek Vodolan.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


﻿using System;
using System.Collections.Generic;

using PHP.Core;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Readonly operations exposed by <see cref="SnapshotBase"/>
    /// </summary>
    public interface ISnapshotReadonly
    {
        #region Value singletons
        
        /// <summary>
        /// Creates new AnyValue
        /// </summary>
        AnyValue AnyValue { get; }

        /// <summary>
        /// Gets an instance of class representing any float value.
        /// </summary>
        AnyFloatValue AnyFloatValue { get; }

        /// <summary>
        /// Gets an instance of class representing any resource value.
        /// </summary>
        AnyResourceValue AnyResourceValue { get; }
        
        /// <summary>
        /// Creates new UndefinedValue
        /// </summary>
        UndefinedValue UndefinedValue { get; }

        /// <summary>
        /// Creates new AnyStringValue
        /// </summary>
        AnyStringValue AnyStringValue { get; }

        /// <summary>
        /// Creates new AnyBooleanValue
        /// </summary>
        AnyBooleanValue AnyBooleanValue { get; }

        /// <summary>
        /// Creates new AnyIntegerValue
        /// </summary>
        AnyIntegerValue AnyIntegerValue { get; }

        /// <summary>
        /// Creates new AnyLongintValue
        /// </summary>
        AnyLongintValue AnyLongintValue { get; }

        /// <summary>
        /// Creates new AnyObjectValue
        /// </summary>
        AnyObjectValue AnyObjectValue { get; }

        /// <summary>
        /// Creates new AnyArrayValue
        /// </summary>
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
        /// <param name="variable">Identifier of variable that is read</param>
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

        /// <summary>
        /// Resolves all possible functions for given functionName
        /// NOTE:
        ///     Multiple declarations for single functionName can happen for example because of branch merging
        /// </summary>
        /// <param name="functionName">Name of resolved function</param>
        /// <returns>Resolved functions</returns>
        IEnumerable<FunctionValue> ResolveFunction(QualifiedName functionName);

        /// <summary>
        /// Resolves all possible functions for given static methodName on type 
        /// </summary>
        /// <param name="type">Type which method is resolved</param>
        /// <param name="methodName">Name of resolved method</param>
        /// <returns>Resolved functions</returns>
        IEnumerable<FunctionValue> ResolveStaticMethod(TypeValue type, QualifiedName methodName);

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