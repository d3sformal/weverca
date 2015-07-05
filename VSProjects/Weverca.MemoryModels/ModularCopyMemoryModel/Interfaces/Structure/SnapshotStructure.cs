/*
Copyright (c) 2012-2014 Pavel Bastecky.

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


using PHP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    /// <summary>
    /// Definition of basic read only operation for snapshot structure container.  
    /// </summary>
    public interface IReadOnlySnapshotStructure
    {
        /// <summary>
        /// Gets the identifier of snapshot data object.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        int StructureId { get; }

        /// <summary>
        /// Gets a value indicating whether structure was changed on commit.
        /// </summary>
        /// <value>
        ///   <c>true</c> if was changed on commit; otherwise, <c>false</c>.
        /// </value>
        bool DiffersOnCommit { get; }

        /// <summary>
        /// Gets a value indicating whether function or class definition was added into structure.
        /// </summary>
        /// <value>
        ///   <c>true</c> if function or class definition was added into structure; otherwise, <c>false</c>.
        /// </value>
        bool DefinitionAdded { get; }

        /// <summary>
        /// Gets the readonly change tracker with lists of modified indexes, functions and classes.
        /// </summary>
        /// <value>
        /// The readonly change tracker.
        /// </value>
        IReadonlyChangeTracker<IReadOnlySnapshotStructure> ReadonlyChangeTracker { get; }

        #region MemoryStack

        /// <summary>
        /// Gets the number of memory stack levels.
        /// </summary>
        /// <value>
        /// The call level.
        /// </value>
        int CallLevel { get; }

        /// <summary>
        /// Gets the readonly local context of memory stack.
        /// </summary>
        /// <value>
        /// The readonly local context of memory stack.
        /// </value>
        IReadonlyStackContext ReadonlyLocalContext { get; }

        /// <summary>
        /// Gets the readonly global context of memory stack.
        /// </summary>
        /// <value>
        /// The readonly global context of memory stack.
        /// </value>
        IReadonlyStackContext ReadonlyGlobalContext { get; }

        /// <summary>
        /// Gets the list of all readonly stack contexts.
        /// </summary>
        /// <value>
        /// The list of all readonly stack contexts.
        /// </value>
        IEnumerable<IReadonlyStackContext> ReadonlyStackContexts { get; }

        /// <summary>
        /// Gets the readonly stack context on specified level of memory stack.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>The readonly stack context on specified level of memory stack</returns>
        IReadonlyStackContext GetReadonlyStackContext(int level);

        /// <summary>
        /// Determines whether structure stack contains context the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>True if structure stack contains context the specified level, otherwise false</returns>
        bool ContainsStackWithLevel(int level);

        #endregion

        #region Indexes

        /// <summary>
        /// Gets the defined indexes in structure indexes.
        /// </summary>
        /// <returns>Stucture indexes.</returns>
        IEnumerable<MemoryIndex> Indexes { get; }

        /// <summary>
        /// Gets set of the index definitions.
        /// </summary>
        /// <value>
        /// The index definitions.
        /// </value>
        IEnumerable<KeyValuePair<MemoryIndex, IIndexDefinition>> IndexDefinitions { get; }

        /// <summary>
        /// Determines whether the specified index is defined.
        /// </summary>
        /// <param name="index">The index.</param>
        bool IsDefined(MemoryIndex index);

        /// <summary>
        /// Tries to get definition of given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="data">The data.</param>
        /// <returns><c>true</c> if the index is defined; otherwise, <c>false</c>.</returns>
        bool TryGetIndexDefinition(MemoryIndex index, out IIndexDefinition data);

        /// <summary>
        /// Gets the definition of specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Definition of specified index.</returns>
        /// <exception cref="System.Exception">Missing alias value for given index</exception>
        IIndexDefinition GetIndexDefinition(MemoryIndex index);

        /// <summary>
        /// Gets the number of indexes in structure.
        /// </summary>
        /// <returns>The number of indexes in structure.</returns>
        int GetNumberOfIndexes();

        #endregion

        #region Objects

        /// <summary>
        /// Gets the set of object descriptors.
        /// </summary>
        /// <value>
        /// The object descriptors.
        /// </value>
        IEnumerable<KeyValuePair<ObjectValue, IObjectDescriptor>> ObjectDescriptors { get; }

        /// <summary>
        /// Gets the PHP object descriptor which contains defined fields and informations about object.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        /// <returns>PHP object descriptor which contains defined fields and informations about object.</returns>
        /// <exception cref="System.Exception">Missing object descriptor</exception>
        IObjectDescriptor GetDescriptor(ObjectValue objectValue);

        /// <summary>
        /// Tries to get the PHP object descriptor which contains defined fields and informations about object.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns>True whether structure contains PHP object descriptor which contains defined fields and informations about object.</returns>
        bool TryGetDescriptor(ObjectValue objectValue, out IObjectDescriptor descriptor);

        /// <summary>
        /// Determines whether the specified index has some PHP objects.
        /// </summary>
        /// <param name="index">The index.</param>
        ///   <c>true</c> if specified index has some PHP objects; otherwise, <c>false</c>.
        bool HasObjects(MemoryIndex index);

        /// <summary>
        /// Gets the objects for given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Collection of objects for given index.</returns>
        IObjectValueContainer GetObjects(MemoryIndex index);

        #endregion

        #region Arrays

        /// <summary>
        /// Gets the set of array descriptors.
        /// </summary>
        /// <value>
        /// The array descriptors.
        /// </value>
        IEnumerable<KeyValuePair<AssociativeArray, IArrayDescriptor>> ArrayDescriptors { get; }

        /// <summary>
        /// Tries to get array descriptor which contains information about defined indexes in the specified array.
        /// </summary>
        /// <param name="arrayValue">The array value.</param>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns>Array descriptor which contains information about defined indexes in the specified array.</returns>
        bool TryGetDescriptor(AssociativeArray arrayValue, out IArrayDescriptor descriptor);

        /// <summary>
        /// Gets the array descriptor which contains information about defined indexes in the specified array.
        /// </summary>
        /// <param name="arrayValue">The array value.</param>
        /// <returns>True whether structure contains array descriptor which contains information about defined indexes in the specified array.</returns>
        /// <exception cref="System.Exception">Missing array descriptor</exception>
        IArrayDescriptor GetDescriptor(AssociativeArray arrayValue);

        /// <summary>
        /// Gets the array descriptor which contains information about defined indexes in the specified array.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Array descriptor which contains information about defined indexes in the specified array.</returns>
        /// <exception cref="System.Exception">Missing array for index  + index</exception>
        AssociativeArray GetArray(MemoryIndex index);

        /// <summary>
        /// Determines whether the specified index has array.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>True whether the specified index has array.</returns>
        bool HasArray(MemoryIndex index);

        /// <summary>
        /// Tries to get array for specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="arrayValue">The array value.</param>
        /// <returns>True whether the specified index has array.</returns>
        bool TryGetArray(MemoryIndex index, out AssociativeArray arrayValue);

        /// <summary>
        /// Tries to get list of spashots which contains specified array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="snapshots">The snapshots.</param>
        /// <returns>List of spashots which contains specified array.</returns>
        bool TryGetCallArraySnapshot(AssociativeArray array, out IEnumerable<Snapshot> snapshots);

        #endregion

        #region Functions

        /// <summary>
        /// Gets the collection of defined functions.
        /// </summary>
        /// <returns>The collection of defined functions.</returns>
        IEnumerable<QualifiedName> GetFunctions();

        /// <summary>
        /// Determines whether function with given name is defined.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <returns>True whether function with given name is defined..</returns>
        bool IsFunctionDefined(QualifiedName functionName);

        /// <summary>
        /// Tries the get functions with specified class name.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="functionValues">The function values.</param>
        /// <returns>
        /// True whether specified function is defined.
        /// </returns>
        bool TryGetFunction(QualifiedName functionName, out IEnumerable<FunctionValue> functionValues);

        /// <summary>
        /// Gets the function.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <returns>List of functions with given name.</returns>
        IEnumerable<FunctionValue> GetFunction(QualifiedName functionName);

        #endregion

        #region Classes

        /// <summary>
        /// Gets the collection of defined classes.
        /// </summary>
        /// <returns>The collection of defined classes.</returns>
        IEnumerable<QualifiedName> GetClasses();

        /// <summary>
        /// Determines whether class with specified name is defined.
        /// </summary>
        /// <param name="className">The name.</param>
        /// <returns>True whether class with specified name is defined.</returns>
        bool IsClassDefined(QualifiedName className);

        /// <summary>
        /// Tries the get classes with specified class name.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="classValues">The class values.</param>
        /// <returns>
        /// True whether specified class is defined.
        /// </returns>
        bool TryGetClass(QualifiedName className, out IEnumerable<TypeValue> classValues);

        /// <summary>
        /// Gets the class.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <returns>Class with the specified name.</returns>
        IEnumerable<TypeValue> GetClass(QualifiedName className);

        #endregion

        #region Aliasses

        /// <summary>
        /// Tries the get aliases for specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="aliases">The aliases.</param>
        /// <returns>True whether specified index has aliases.</returns>
        bool TryGetAliases(MemoryIndex index, out IMemoryAlias aliases);

        /// <summary>
        /// Gets the aliases for specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Aliases for the specified index.</returns>
        /// <exception cref="System.Exception">Missing alias value for  + index</exception>
        IMemoryAlias GetAliases(MemoryIndex index);

        #endregion
    }

    /// <summary>
    /// Definition of operations for snapshot structure object which modifies inner structure.
    /// </summary>
    public interface IWriteableSnapshotStructure : IReadOnlySnapshotStructure
    {
        /// <summary>
        /// Sets the value indicating whether structure was changed on commit.
        /// </summary>
        /// <param name="isDifferent">if set to <c>true</c> this structure was changed on commit.</param>
        void SetDiffersOnCommit(bool isDifferent);

        /// <summary>
        /// Reinitializes change tracker and sets the parent tracker to be given structure tracker.
        /// </summary>
        /// <param name="parentSnapshotStructure">The parent snapshot structure.</param>
        void ReinitializeTracker(IReadOnlySnapshotStructure parentSnapshotStructure);

        /// <summary>
        /// Gets the writeable change tracker with lists of modified indexes, functions and classes.
        /// </summary>
        /// <value>
        /// The writeable change tracker.
        /// </value>
        IWriteableChangeTracker<IReadOnlySnapshotStructure> WriteableChangeTracker { get; }

        #region MemoryStack

        /// <summary>
        /// Gets the writeable local context of memory stack.
        /// </summary>
        /// <value>
        /// The writeable local context of memory stack.
        /// </value>
        IWriteableStackContext WriteableLocalContext { get; }

        /// <summary>
        /// Gets the writeable global context of memory stack.
        /// </summary>
        /// <value>
        /// The writeable global context of memory stack.
        /// </value>
        IWriteableStackContext WriteableGlobalContext { get; }

        /// <summary>
        /// Gets the list of all writeable stack contexts.
        /// </summary>
        /// <value>
        /// The list of all writeable stack contexts.
        /// </value>
        IEnumerable<IWriteableStackContext> WriteableStackContexts { get; }

        /// <summary>
        /// Gets the writeable stack context on specified level of memory stack.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>The writeable stack context on specified level of memory stack</returns>
        IWriteableStackContext GetWriteableStackContext(int level);

        /// <summary>
        /// Adds the local level to memory stack.
        /// </summary>
        void AddLocalLevel();

        /// <summary>
        /// Adds the stack level with given number.
        /// </summary>
        /// <param name="level">The level.</param>
        void AddStackLevel(int level);

        /// <summary>
        /// Sets the number of current local stack level.
        /// </summary>
        /// <param name="level">The level.</param>
        void SetLocalStackLevelNumber(int level);

        #endregion
        
        #region Indexes

        /// <summary>
        /// Insert newly created index into structure and data collection.
        /// </summary>
        /// <param name="index">The index.</param>
        void NewIndex(MemoryIndex index);

        /// <summary>
        /// Removes the index from structure and data.
        /// </summary>
        /// <param name="index">The index.</param>
        void RemoveIndex(MemoryIndex index);

        #endregion

        #region Objects

        /// <summary>
        /// Sets the PHP object descriptor which contains defined fields and informations about object.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        /// <param name="descriptor">The descriptor.</param>
        void SetDescriptor(ObjectValue objectValue, IObjectDescriptor descriptor);

        /// <summary>
        /// Sets objects for given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="objects">The objects.</param>
        void SetObjects(MemoryIndex index, IObjectValueContainer objects);

        #endregion

        #region Arrays

        /// <summary>
        /// Sets the array descriptor which contains information about defined indexes in the specified array.
        /// </summary>
        /// <param name="arrayvalue">The arrayvalue.</param>
        /// <param name="descriptor">The descriptor.</param>
        void SetDescriptor(AssociativeArray arrayvalue, IArrayDescriptor descriptor);

        /// <summary>
        /// Adds the combination of array and snapshot into call arrays set.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="snapshot">The snapshot.</param>
        void AddCallArray(AssociativeArray array, Snapshot snapshot);

        /// <summary>
        /// Sets the array for specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="arrayValue">The array value.</param>
        void SetArray(MemoryIndex index, AssociativeArray arrayValue);

        /// <summary>
        /// Removes the array from specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="arrayValue">The array value.</param>
        void RemoveArray(MemoryIndex index, AssociativeArray arrayValue);

        #endregion

        #region Functions

        /// <summary>
        /// Add new declaration for function with given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="declaration">The declaration.</param>
        void AddFunctiondeclaration(QualifiedName name, FunctionValue declaration);

        /// <summary>
        /// Sets the function declarations.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="declarations">The declarations.</param>
        void SetFunctionDeclarations(QualifiedName name, IEnumerable<FunctionValue> declarations);
        
        #endregion

        #region Classes

        /// <summary>
        /// Add new declaration for class with given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="declaration">The declaration.</param>
        void AddClassDeclaration(QualifiedName name, TypeValue declaration);

        /// <summary>
        /// Sets the class declarations.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="declarations">The declarations.</param>
        void SetClassDeclarations(QualifiedName name, IEnumerable<TypeValue> declarations);

        #endregion

        #region Aliasses

        /// <summary>
        /// Sets the alias to specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="alias">The alias.</param>
        void SetAlias(MemoryIndex index, IMemoryAlias alias);

        /// <summary>
        /// Removes the alias from specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        void RemoveAlias(MemoryIndex index);

        #endregion
    }
    
    /// <summary>
    /// Basic abstract implementation of snapshot structure container.
    /// 
    /// Implements unique identifiers and snapshot storing.
    /// </summary>
    public abstract class AbstractSnapshotStructure : IReadOnlySnapshotStructure, IWriteableSnapshotStructure
    {
        /// <summary>
        /// Incremental counter for structure unique identifier.
        /// </summary>
        private static int STRUCTURE_ID = 0;

        /// <summary>
        /// Gets the structure proxy.
        /// </summary>
        /// <value>
        /// The structure proxy.
        /// </value>
        protected ISnapshotStructureProxy StructureProxy { get; private set; }

        /// <inheritdoc />
        public bool DiffersOnCommit
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public void SetDiffersOnCommit(bool isDifferent)
        {
            DiffersOnCommit = isDifferent;
        }

        /// <inheritdoc />
        public virtual bool DefinitionAdded
        {
            get { return true; }
        }

        /// <inheritdoc />
        public virtual IWriteableChangeTracker<IReadOnlySnapshotStructure> WriteableChangeTracker
        {
            get { return null; }
        }

        /// <inheritdoc />
        public virtual IReadonlyChangeTracker<IReadOnlySnapshotStructure> ReadonlyChangeTracker
        {
            get { return null; }
        }

        /// <inheritdoc />
        public virtual void ReinitializeTracker(IReadOnlySnapshotStructure parentSnapshotStructure)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractSnapshotStructure" /> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        public AbstractSnapshotStructure(ModularMemoryModelFactories factories)
        {
            StructureId = STRUCTURE_ID++;
            Factories = factories;
        }

        /// <inheritdoc />
        public int StructureId
        {
            get;
            private set;
        }

        #region MemoryStack

        /// <inheritdoc />
        public abstract int CallLevel { get; }

        /// <inheritdoc />
        public abstract bool ContainsStackWithLevel(int level);

        /// <inheritdoc />
        public abstract void AddStackLevel(int level);

        /// <inheritdoc />
        public abstract void SetLocalStackLevelNumber(int level);

        /// <inheritdoc />
        public abstract IReadonlyStackContext ReadonlyLocalContext { get; }

        /// <inheritdoc />
        public abstract IReadonlyStackContext ReadonlyGlobalContext { get; }

        /// <inheritdoc />
        public abstract IEnumerable<IReadonlyStackContext> ReadonlyStackContexts { get; }

        /// <inheritdoc />
        public abstract IReadonlyStackContext GetReadonlyStackContext(int level);

        /// <inheritdoc />
        public abstract IWriteableStackContext WriteableLocalContext { get; }

        /// <inheritdoc />
        public abstract IWriteableStackContext WriteableGlobalContext { get; }

        /// <inheritdoc />
        public abstract IEnumerable<IWriteableStackContext> WriteableStackContexts { get; }

        /// <inheritdoc />
        public abstract IWriteableStackContext GetWriteableStackContext(int level);

        /// <inheritdoc />
        public abstract void AddLocalLevel();

        #endregion

        #region Indexes

        /// <inheritdoc />
        public abstract IEnumerable<MemoryIndex> Indexes { get; }

        /// <inheritdoc />
        public abstract IEnumerable<KeyValuePair<MemoryIndex, IIndexDefinition>> IndexDefinitions { get; }

        /// <inheritdoc />
        public abstract bool IsDefined(MemoryIndex index);

        /// <inheritdoc />
        public abstract bool TryGetIndexDefinition(MemoryIndex index, out IIndexDefinition data);

        /// <inheritdoc />
        public abstract IIndexDefinition GetIndexDefinition(MemoryIndex index);

        /// <inheritdoc />
        public abstract int GetNumberOfIndexes();

        /// <inheritdoc />
        public abstract void NewIndex(MemoryIndex index);

        /// <inheritdoc />
        public abstract void RemoveIndex(MemoryIndex index);

        #endregion

        #region Objects

        /// <inheritdoc />
        public abstract IEnumerable<KeyValuePair<ObjectValue, IObjectDescriptor>> ObjectDescriptors { get; }

        /// <inheritdoc />
        public abstract IObjectDescriptor GetDescriptor(ObjectValue objectValue);

        /// <inheritdoc />
        public abstract bool TryGetDescriptor(ObjectValue objectValue, out IObjectDescriptor descriptor);

        /// <inheritdoc />
        public abstract bool HasObjects(MemoryIndex index);

        /// <inheritdoc />
        public abstract IObjectValueContainer GetObjects(MemoryIndex index);

        /// <inheritdoc />
        public abstract void SetDescriptor(ObjectValue objectValue, IObjectDescriptor descriptor);

        /// <inheritdoc />
        public abstract void SetObjects(MemoryIndex index, IObjectValueContainer objects);

        #endregion

        #region Arrays

        /// <inheritdoc />
        public abstract IEnumerable<KeyValuePair<AssociativeArray, IArrayDescriptor>> ArrayDescriptors { get; }
        /// <inheritdoc />
        public abstract bool TryGetDescriptor(AssociativeArray arrayValue, out IArrayDescriptor descriptor);

        /// <inheritdoc />
        public abstract IArrayDescriptor GetDescriptor(AssociativeArray arrayValue);

        /// <inheritdoc />
        public abstract AssociativeArray GetArray(MemoryIndex index);

        /// <inheritdoc />
        public abstract bool HasArray(MemoryIndex index);

        /// <inheritdoc />
        public abstract bool TryGetArray(MemoryIndex index, out AssociativeArray arrayValue);

        /// <inheritdoc />
        public abstract bool TryGetCallArraySnapshot(AssociativeArray array, out IEnumerable<Snapshot> snapshots);

        /// <inheritdoc />
        public abstract void SetDescriptor(AssociativeArray arrayvalue, IArrayDescriptor descriptor);

        /// <inheritdoc />
        public abstract void AddCallArray(AssociativeArray array, Snapshot snapshot);

        /// <inheritdoc />
        public abstract void SetArray(MemoryIndex index, AssociativeArray arrayValue);

        /// <inheritdoc />
        public abstract void RemoveArray(MemoryIndex index, AssociativeArray arrayValue);

        #endregion

        #region Functions

        /// <inheritdoc />
        public abstract IEnumerable<QualifiedName> GetFunctions();

        /// <inheritdoc />
        public abstract bool IsFunctionDefined(PHP.Core.QualifiedName functionName);

        /// <inheritdoc />
        public abstract bool TryGetFunction(QualifiedName functionName, out IEnumerable<FunctionValue> functionValues);

        /// <inheritdoc />
        public abstract IEnumerable<FunctionValue> GetFunction(QualifiedName functionName);

        /// <inheritdoc />
        public abstract void AddFunctiondeclaration(QualifiedName name, FunctionValue declaration);

        /// <inheritdoc />
        public abstract void SetFunctionDeclarations(QualifiedName name, IEnumerable<FunctionValue> declarations);

        #endregion

        #region Classes

        /// <inheritdoc />
        public abstract IEnumerable<QualifiedName> GetClasses();

        /// <inheritdoc />
        public abstract bool IsClassDefined(PHP.Core.QualifiedName name);

        /// <inheritdoc />
        public abstract bool TryGetClass(QualifiedName className, out IEnumerable<TypeValue> classValues);

        /// <inheritdoc />
        public abstract IEnumerable<TypeValue> GetClass(QualifiedName className);

        /// <inheritdoc />
        public abstract void AddClassDeclaration(QualifiedName name, TypeValue declaration);

        /// <inheritdoc />
        public abstract void SetClassDeclarations(QualifiedName name, IEnumerable<TypeValue> declarations);

        #endregion

        #region Aliasses

        /// <inheritdoc />
        public abstract bool TryGetAliases(MemoryIndex index, out IMemoryAlias aliases);

        /// <inheritdoc />
        public abstract IMemoryAlias GetAliases(MemoryIndex index);

        /// <inheritdoc />
        public abstract void SetAlias(MemoryIndex index, IMemoryAlias alias);

        /// <inheritdoc />
        public abstract void RemoveAlias(MemoryIndex index);

        #endregion


        public ModularMemoryModelFactories Factories { get; set; }
    }
}