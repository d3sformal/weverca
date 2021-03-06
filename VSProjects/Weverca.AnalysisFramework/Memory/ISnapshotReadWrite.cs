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


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Weverca.AnalysisFramework.ProgramPoints;

using System.IO;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Read/Write operations exposed by <see cref="SnapshotBase"/>
    /// </summary>
    public interface ISnapshotReadWrite : ISnapshotReadonly
    {
        #region Snapshot flow operations
        /// <summary>
        /// Snapshot has to contain merged info present in inputs (no matter what snapshots contains till now)
        /// This merged info can be than changed with snapshot updatable operations
        /// NOTE: Further changes of inputs can't change extended snapshot
        /// </summary>
        /// <param name="inputs">Input snapshots that should be merged</param>
        void Extend(params ISnapshotReadonly[] inputs);

        /// <summary>
        /// Merges information at the entry of the subprogram (function, method, or included file).
        /// 
        /// Note that if inputs.Length > 1 than the subprogram is shared between more extended points (e.g., callers).
        /// 
        /// Note that it holds that inputs.Lenght == extendedPoints.Length and for each inputs[i], extendedPoints[i]
        /// is extended point (e.g., caller) correspoinding to the input.
        /// After the subprogram is processed, method MergeWithCallLevel(extendedPoints[i]) will be called for every
        /// input. Snapshot can, e.g., keep inputs or their parts separated (not merge them) and identify these separated
        /// parts using extendedPoints[i] and than when the subprogram is processed and MergeWithCallLevel(extendedPoints[i])
        /// is called use extendedPoints[i] to identify these parts that belong to the extended point (caller) and drop others.
        /// See <see cref="MergeWithCallLevel"/> for more information.
        /// 
        /// Snapshot has to contain merged info present in inputs (no matter what snapshots contain till now)
        /// This merged info can be than changed with snapshot updatable operations
        /// NOTE: Further changes of inputs can't change extended snapshot
        /// </summary>
        /// <param name="inputs">Input snapshots that should be merged</param>
        /// <param name="extendedPoints">The points that are extended (e.g, callers).</param>
        /// <seealso cref="MergeWithCallLevel"/>
        void ExtendAtSubprogramEntry(ISnapshotReadonly[] inputs, ProgramPointBase[] extendedPoints);

        /// <summary>
        /// Merges the result of the extension (call or include) performed by extendedPoint into this snapshot.
        /// 
        /// There can be multiple callees for a single call (extension) - callOutputs.Length can be more than 1.
        /// Callees can be shared by multiple callers (extensions) - they can contain parts of the stack and global
        /// state that do not belong to this particular caller.
        /// See <see cref="ExtendAtSubprogramEntry"/> for more information.
        /// 
        /// WARNING: Call can change many objects via references (they don't have to be in global context)
        /// </summary>
        /// <param name="extendedPoint">The program point that was extended (e.g., caller)</param>    
        /// <param name="extensionsOutputs">Output snapshots of callees belonging to the call</param>  
        /// <seealso cref="ExtendAtSubprogramEntry"/>
        void MergeWithCallLevel(ProgramPointBase extendedPoint, params ISnapshotReadonly[] extensionsOutputs);

        /// <summary>
        /// Merges information at the entry of the catch block.
        /// 
        /// Snapshot has to contain merged info present in inputs (no matter what snapshots contain till now)
        /// This merged info can be than changed with snapshot updatable operations
        /// NOTE: Further changes of inputs can't change extended snapshot
        /// </summary>
        /// <param name="inputs">Input snapshots that should be merged (snapshots of program points where the catched exception is thrown).</param>
        /// <param name="catchDescription">Catch description.</param>
        void ExtendAtCatchEntry(ISnapshotReadonly[] inputs, CatchBlockDescription catchDescription);

        #endregion

        #region Creating values

        /// <summary>
        /// Create interval of integers
        /// </summary>
        /// <param name="start">Lower bound</param>
        /// <param name="end">Upper bound</param>
        /// <returns>Created interval</returns>
        IntegerIntervalValue CreateIntegerInterval(int start, int end);

        /// <summary>
        /// Create interval of long integers
        /// </summary>
        /// <param name="start">Lower bound</param>
        /// <param name="end">Upper bound</param>
        /// <returns>Created interval</returns>
        LongintIntervalValue CreateLongintInterval(long start, long end);


        /// <summary>
        /// Create interval of doubles
        /// </summary>
        /// <param name="start">Lower bound</param>
        /// <param name="end">Upper bound</param>
        /// <returns>Created interval</returns>
        FloatIntervalValue CreateFloatInterval(double start, double end);

        /// <summary>
        /// Create string value from given literal
        /// </summary>
        /// <param name="literal">String literal</param>
        /// <returns>Created value</returns>
        StringValue CreateString(string literal);

        /// <summary>
        /// Create integer value from given number
        /// </summary>
        /// <param name="number">Represented number</param>
        /// <returns>Created value</returns>
        IntegerValue CreateInt(int number);

        /// <summary>
        /// Create long integer value from given number
        /// </summary>
        /// <param name="number">Represented number</param>
        /// <returns>Created value</returns>
        LongintValue CreateLong(long number);

        /// <summary>
        /// Create float value from given number
        /// </summary>
        /// <param name="number">Represented number</param>
        /// <returns>Created value</returns>
        FloatValue CreateDouble(double number);

        /// <summary>
        /// Create boolean value from given value
        /// </summary>
        /// <param name="boolean">Boolean value</param>
        /// <returns>Created value</returns>
        BooleanValue CreateBool(bool boolean);

        /// <summary>
        /// Create info value from given data
        /// NOTE:
        ///     T should provide immutability that avoid wrong usage
        /// </summary>
        /// <typeparam name="T">Type of stored info</typeparam>
        /// <param name="data">Info data</param>
        /// <returns>Created info value</returns>
        InfoValue<T> CreateInfo<T>(T data);

        /// <summary>
        /// Create function value from given declaration
        /// </summary>
        /// <param name="declaration">Function declaration</param>
        /// <param name="declaringScript">information about owning script</param>
        /// <returns>Created value</returns>
        FunctionValue CreateFunction(FunctionDecl declaration, FileInfo declaringScript);

        /// <summary>
        /// Create function value from given declaration
        /// </summary>
        /// <param name="declaration">Method declaration</param>
        /// <param name="declaringScript">information about owning script</param>
        /// <returns>Created value</returns>
        FunctionValue CreateFunction(MethodDecl declaration, FileInfo declaringScript);

        /// <summary>
        /// Create function value from given declaration
        /// </summary>
        /// <param name="analyzer">Analyzer declaration</param>
        /// <param name="name">Name of created analyzer</param>
        /// <returns>Created value</returns>
        FunctionValue CreateFunction(Name name, NativeAnalyzer analyzer);

        /// <summary>
        /// Create type value from given declaration
        /// </summary>
        /// <param name="declaration">Native type declaration</param>
        /// <returns>Created value</returns>
        TypeValue CreateType(ClassDecl declaration);

        /// <summary>
        /// Create function value from given expression
        /// </summary>
        /// <param name="expression">Lambda function declaration</param>     
        /// <param name="declaringScript">information about owning script</param>
        /// <returns>Created value</returns>
        FunctionValue CreateFunction(LambdaFunctionExpr expression, FileInfo declaringScript);

        /// <summary>
        /// Create array empty array
        /// </summary>
        /// <returns>Created value</returns>
        AssociativeArray CreateArray();

        /// <summary>
        /// Create object of given type
        /// </summary>
        /// <param name="type">Desired type of created object</param>
        /// <returns>Instace of object of given type</returns>
        ObjectValue CreateObject(TypeValue type);

        #endregion

        #region Snapshot entry manipulation

        /// <summary>
        /// Get snapshot entry providing reading,... services for variable
        /// </summary>
        /// <remarks>
        /// If global context is not forced, searches in local context (there can be 
        /// fetched some variables from global context also),
        /// or in global context in snapshot belonging to global code
        /// </remarks>
        /// <param name="variable">Identifier of variable</param>
        /// <param name="forceGlobalContext">Determine that searching in global context has to be forced</param>
        /// <returns>Readable snapshot entry for variable identifier</returns>
        ReadWriteSnapshotEntryBase GetVariable(VariableIdentifier variable, bool forceGlobalContext = false);

        /// <summary>
        /// Get snapshot entry for variable, used for extra info controlling. Control entries may share names with other variables,
        /// indexes or fields. Control entries are not affected by unknown fields, also they cannot be aliased to non-control
        /// entries.
        /// </summary>
        /// <param name="variable">Variable determining control entry</param>
        /// <returns>Created control entry</returns>
        ReadWriteSnapshotEntryBase GetControlVariable(VariableName variable);

        /// <summary>
        /// Get snapshot entry for variable, used for extra info controlling in local context. Control entries may share names with other variables,
        /// indexes or fields. Control entries are not affected by unknown fields, also they cannot be aliased to non-control
        /// entries.
        /// </summary>
        /// <param name="variable">Variable determining control entry</param>
        /// <returns>Created control entry</returns>
        ReadWriteSnapshotEntryBase GetLocalControlVariable(VariableName variable);

        /// <summary>
        /// Creates snapshot entry containing given value. Created entry doesn't have
        /// explicit memory storage. But it still can be asked for saving indexes, fields, resolving aliases,... !!!
        /// </summary>
        /// <param name="value">Value wrapped in snapshot entry</param>
        /// <returns>Created value entry</returns>
        ReadSnapshotEntryBase CreateSnapshotEntry(MemoryEntry value);



        #endregion

        #region Value storing

        /// <summary>
        /// Set given info for value
        /// </summary>
        /// <param name="value">Value which info is stored</param>
        /// <param name="info">Info stored for value</param>
        void SetInfo(Value value, params InfoValue[] info);

        /// <summary>
        /// Set given info for variable
        /// </summary>
        /// <param name="variable">Variable which info is stored</param>
        /// <param name="info">Info stored for variable</param>
        void SetInfo(VariableName variable, params InfoValue[] info);

        #endregion

        #region Global context manipulation

        /// <summary>
        /// Declare function into global scope
        /// </summary>
        /// <param name="declaration">Function declaration</param>
        /// <param name="declaringScript">File info about script where function has been declared</param>
        void DeclareGlobal(FunctionDecl declaration, FileInfo declaringScript);

        /// <summary>
        /// Declare type into global scope
        /// </summary>
        /// <param name="type">Declared type</param>
        void DeclareGlobal(TypeValue type);

        /// <summary>
        /// Fetch variables from global context into current context
        /// </summary>
        /// <example>global x,y;</example>
        /// <param name="variables">Variables that will be fetched</param>
        void FetchFromGlobal(params VariableName[] variables);

        /// <summary>
        /// Fetch all variables defined in global context into current context
        /// </summary>
        void FetchFromGlobalAll();

        #endregion

    }
}