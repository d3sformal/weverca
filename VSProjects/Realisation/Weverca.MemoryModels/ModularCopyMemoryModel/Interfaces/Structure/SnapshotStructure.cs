using PHP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    /// <summary>
    /// Factory object of snapshot structure container. This is the only way memory model will
    /// create instances of snapshot structure container.
    /// 
    /// Supports creating new empty instance or copiing existing one.
    /// </summary>
    public interface ISnapshotStructureFactory
    {
        /// <summary>
        /// Creates the empty instance of snapshot structure object.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New object with empty inner structure.</returns>
        ISnapshotStructureProxy CreateEmptyInstance(Snapshot snapshot);

        /// <summary>
        /// Creates new snapshot structure container as copy of the given one.
        /// Copied object mustn't interfere with source.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="oldData">The old data.</param>
        /// <returns>New object with inner scructure as copy of given object.</returns>
        ISnapshotStructureProxy CopyInstance(Snapshot snapshot, ISnapshotStructureProxy oldData);

        /// <summary>
        /// Creates new snapshot structure container with empty global context.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New object with empty global context.</returns>
        ISnapshotStructureProxy CreateGlobalContextInstance(Snapshot snapshot);
    }

    /// <summary>
    /// Proxy object for snapshot structure container. This object is used to distinguish readonly or
    /// writeable access to structure container.
    /// </summary>
    public interface ISnapshotStructureProxy
    {
        /// <summary>
        /// Gets or sets a value indicating whether structural changes are allowed or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if structural changes are forbiden; otherwise, <c>false</c>.
        /// </value>
        bool Locked { get; set; }

        /// <summary>
        /// Gets the snasphot structure container for read only access.
        /// </summary>
        /// <value>
        /// The read only snapshot structure.
        /// </value>
        IReadOnlySnapshotStructure Readonly { get; }

        /// <summary>
        /// Gets the snapshot structure container for access which allows modifications.
        /// </summary>
        /// <value>
        /// The writeable snapshot structure.
        /// </value>
        IWriteableSnapshotStructure Writeable { get; }

        /// <summary>
        /// Creates the new instance of object descriptor to store object definition in structure.
        /// </summary>
        /// <param name="createdObject">The created object.</param>
        /// <param name="type">The type of object.</param>
        /// <param name="memoryIndex">The memory location of object.</param>
        /// <returns>Created object descriptor instance.</returns>
        IObjectDescriptor CreateObjectDescriptor(ObjectValue createdObject, TypeValue type, MemoryIndex memoryIndex);

        /// <summary>
        /// Creates the new instance of array descriptor to store array definition in structure.
        /// </summary>
        /// <param name="createdArray">The created array.</param>
        /// <param name="memoryIndex">The memory location of array.</param>
        /// <returns>Created array descriptor instance.</returns>
        IArrayDescriptor CreateArrayDescriptor(AssociativeArray createdArray, MemoryIndex memoryIndex);

        /// <summary>
        /// Creates the new instance of memory alias object to store alias definition in this structure.
        /// </summary>
        /// <param name="index">The memory index collection is created for.</param>
        /// <returns>Created alias collection.</returns>
        IMemoryAlias CreateMemoryAlias(MemoryIndex index);

        /// <summary>
        /// Creates the new instance of object container to store object values for memory location in this structure.
        /// </summary>
        /// <param name="objects">The objects to store in collection.</param>
        /// <returns>Created object container.</returns>
        IObjectValueContainer CreateObjectValueContainer(IEnumerable<ObjectValue> objects);

        /// <summary>
        /// Creates the new instance of object container to store alias, array and object data for memory indexes.
        /// </summary>
        /// <returns>New instance of index definition object.</returns>
        IIndexDefinition CreateIndexDefinition();
    }

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

        #region MemoryStack

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
        /// Gets the collection of created aliases in this snapshot.
        /// </summary>
        /// <value>
        /// The created aliases.
        /// </value>
        IEnumerable<IMemoryAlias> CreatedAliases { get; }

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
        /// Sets the function.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="declaration">The declaration.</param>
        void SetFunction(QualifiedName name, FunctionValue declaration);

        #endregion

        #region Classes

        /// <summary>
        /// Sets the class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="declaration">The declaration.</param>
        void SetClass(QualifiedName name, TypeValue declaration);

        #endregion

        #region Aliasses

        /// <summary>
        /// Adds the created alias.
        /// </summary>
        /// <param name="aliasData">The alias data.</param>
        void AddCreatedAlias(IMemoryAlias aliasData);

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
        /// Gets the snapshot.
        /// </summary>
        /// <value>
        /// The snapshot.
        /// </value>
        protected Snapshot Snapshot { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractSnapshotStructure" /> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        public AbstractSnapshotStructure(Snapshot snapshot)
        {
            StructureId = STRUCTURE_ID++;
            Snapshot = snapshot;
        }

        /// <inheritdoc />
        public int StructureId
        {
            get;
            private set;
        }

        #region MemoryStack

        /// <summary>
        /// Gets the readonly local context of memory stack.
        /// </summary>
        /// <value>
        /// The readonly local context of memory stack.
        /// </value>
        public abstract IReadonlyStackContext ReadonlyLocalContext { get; }

        /// <summary>
        /// Gets the readonly global context of memory stack.
        /// </summary>
        /// <value>
        /// The readonly global context of memory stack.
        /// </value>
        public abstract IReadonlyStackContext ReadonlyGlobalContext { get; }

        /// <summary>
        /// Gets the list of all readonly stack contexts.
        /// </summary>
        /// <value>
        /// The list of all readonly stack contexts.
        /// </value>
        public abstract IEnumerable<IReadonlyStackContext> ReadonlyStackContexts { get; }

        /// <summary>
        /// Gets the readonly stack context on specified level of memory stack.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>The readonly stack context on specified level of memory stack</returns>
        public abstract IReadonlyStackContext GetReadonlyStackContext(int level);

        /// <summary>
        /// Gets the writeable local context of memory stack.
        /// </summary>
        /// <value>
        /// The writeable local context of memory stack.
        /// </value>
        public abstract IWriteableStackContext WriteableLocalContext { get; }

        /// <summary>
        /// Gets the writeable global context of memory stack.
        /// </summary>
        /// <value>
        /// The writeable global context of memory stack.
        /// </value>
        public abstract IWriteableStackContext WriteableGlobalContext { get; }

        /// <summary>
        /// Gets the list of all writeable stack contexts.
        /// </summary>
        /// <value>
        /// The list of all writeable stack contexts.
        /// </value>
        public abstract IEnumerable<IWriteableStackContext> WriteableStackContexts { get; }

        /// <summary>
        /// Gets the writeable stack context on specified level of memory stack.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>The writeable stack context on specified level of memory stack</returns>
        public abstract IWriteableStackContext GetWriteableStackContext(int level);

        /// <summary>
        /// Adds the local level to memory stack.
        /// </summary>
        public abstract void AddLocalLevel();

        #endregion

        #region Indexes

        /// <summary>
        /// Gets the defined indexes in structure indexes.
        /// </summary>
        /// <returns>Stucture indexes.</returns>
        public abstract IEnumerable<MemoryIndex> Indexes { get; }

        /// <summary>
        /// Gets set of the index definitions.
        /// </summary>
        /// <value>
        /// The index definitions.
        /// </value>
        public abstract IEnumerable<KeyValuePair<MemoryIndex, IIndexDefinition>> IndexDefinitions { get; }

        /// <summary>
        /// Determines whether the specified index is defined.
        /// </summary>
        /// <param name="index">The index.</param>
        public abstract bool IsDefined(MemoryIndex index);

        /// <summary>
        /// Tries to get definition of given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="data">The data.</param>
        /// <returns><c>true</c> if the index is defined; otherwise, <c>false</c>.</returns>
        public abstract bool TryGetIndexDefinition(MemoryIndex index, out IIndexDefinition data);

        /// <summary>
        /// Gets the definition of specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Definition of specified index.</returns>
        /// <exception cref="System.Exception">Missing alias value for given index</exception>
        public abstract IIndexDefinition GetIndexDefinition(MemoryIndex index);

        /// <summary>
        /// Gets the number of indexes in structure.
        /// </summary>
        /// <returns>The number of indexes in structure.</returns>
        public abstract int GetNumberOfIndexes();

        /// <summary>
        /// Insert newly created index into structure and data collection.
        /// </summary>
        /// <param name="index">The index.</param>
        public abstract void NewIndex(MemoryIndex index);

        /// <summary>
        /// Removes the index from structure and data.
        /// </summary>
        /// <param name="index">The index.</param>
        public abstract void RemoveIndex(MemoryIndex index);

        #endregion

        #region Objects

        /// <summary>
        /// Gets the set of object descriptors.
        /// </summary>
        /// <value>
        /// The object descriptors.
        /// </value>
        public abstract IEnumerable<KeyValuePair<ObjectValue, IObjectDescriptor>> ObjectDescriptors { get; }

        /// <summary>
        /// Gets the PHP object descriptor which contains defined fields and informations about object.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        /// <returns>PHP object descriptor which contains defined fields and informations about object.</returns>
        /// <exception cref="System.Exception">Missing object descriptor</exception>
        public abstract IObjectDescriptor GetDescriptor(ObjectValue objectValue);

        /// <summary>
        /// Tries to get the PHP object descriptor which contains defined fields and informations about object.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns>True whether structure contains PHP object descriptor which contains defined fields and informations about object.</returns>
        public abstract bool TryGetDescriptor(ObjectValue objectValue, out IObjectDescriptor descriptor);

        /// <summary>
        /// Determines whether the specified index has some PHP objects.
        /// </summary>
        /// <param name="index">The index.</param>
        ///   <c>true</c> if specified index has some PHP objects; otherwise, <c>false</c>.
        public abstract bool HasObjects(MemoryIndex index);

        /// <summary>
        /// Gets the objects for given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Collection of objects for given index.</returns>
        public abstract IObjectValueContainer GetObjects(MemoryIndex index);

        /// <summary>
        /// Sets the PHP object descriptor which contains defined fields and informations about object.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        /// <param name="descriptor">The descriptor.</param>
        public abstract void SetDescriptor(ObjectValue objectValue, IObjectDescriptor descriptor);

        /// <summary>
        /// Sets objects for given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="objects">The objects.</param>
        public abstract void SetObjects(MemoryIndex index, IObjectValueContainer objects);

        #endregion

        #region Arrays

        /// <summary>
        /// Gets the set of array descriptors.
        /// </summary>
        /// <value>
        /// The array descriptors.
        /// </value>
        public abstract IEnumerable<KeyValuePair<AssociativeArray, IArrayDescriptor>> ArrayDescriptors { get; }

        /// <summary>
        /// Tries to get array descriptor which contains information about defined indexes in the specified array.
        /// </summary>
        /// <param name="arrayValue">The array value.</param>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns>Array descriptor which contains information about defined indexes in the specified array.</returns>
        public abstract bool TryGetDescriptor(AssociativeArray arrayValue, out IArrayDescriptor descriptor);

        /// <summary>
        /// Gets the array descriptor which contains information about defined indexes in the specified array.
        /// </summary>
        /// <param name="arrayValue">The array value.</param>
        /// <returns>True whether structure contains array descriptor which contains information about defined indexes in the specified array.</returns>
        /// <exception cref="System.Exception">Missing array descriptor</exception>
        public abstract IArrayDescriptor GetDescriptor(AssociativeArray arrayValue);

        /// <summary>
        /// Gets the array descriptor which contains information about defined indexes in the specified array.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Array descriptor which contains information about defined indexes in the specified array.</returns>
        /// <exception cref="System.Exception">Missing array for index  + index</exception>
        public abstract AssociativeArray GetArray(MemoryIndex index);

        /// <summary>
        /// Determines whether the specified index has array.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>True whether the specified index has array.</returns>
        public abstract bool HasArray(MemoryIndex index);

        /// <summary>
        /// Tries to get array for specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="arrayValue">The array value.</param>
        /// <returns>True whether the specified index has array.</returns>
        public abstract bool TryGetArray(MemoryIndex index, out AssociativeArray arrayValue);

        /// <summary>
        /// Tries to get list of spashots which contains specified array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="snapshots">The snapshots.</param>
        /// <returns>List of spashots which contains specified array.</returns>
        public abstract bool TryGetCallArraySnapshot(AssociativeArray array, out IEnumerable<Snapshot> snapshots);

        /// <summary>
        /// Sets the array descriptor which contains information about defined indexes in the specified array.
        /// </summary>
        /// <param name="arrayvalue">The arrayvalue.</param>
        /// <param name="descriptor">The descriptor.</param>
        public abstract void SetDescriptor(AssociativeArray arrayvalue, IArrayDescriptor descriptor);

        /// <summary>
        /// Adds the combination of array and snapshot into call arrays set.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="snapshot">The snapshot.</param>
        public abstract void AddCallArray(AssociativeArray array, Snapshot snapshot);

        /// <summary>
        /// Sets the array for specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="arrayValue">The array value.</param>
        public abstract void SetArray(MemoryIndex index, AssociativeArray arrayValue);

        /// <summary>
        /// Removes the array from specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="arrayValue">The array value.</param>
        public abstract void RemoveArray(MemoryIndex index, AssociativeArray arrayValue);

        #endregion

        #region Functions

        /// <summary>
        /// Gets the collection of defined functions.
        /// </summary>
        /// <returns>
        /// The collection of defined functions.
        /// </returns>
        public abstract IEnumerable<QualifiedName> GetFunctions();

        /// <summary>
        /// Determines whether function with given name is defined.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <returns>True whether function with given name is defined..</returns>
        public abstract bool IsFunctionDefined(PHP.Core.QualifiedName functionName);

        /// <summary>
        /// Tries the get functions with specified class name.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="functionValues">The function values.</param>
        /// <returns>
        /// True whether specified function is defined.
        /// </returns>
        public abstract bool TryGetFunction(QualifiedName functionName, out IEnumerable<FunctionValue> functionValues);

        /// <summary>
        /// Gets the function.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <returns>List of functions with given name.</returns>
        public abstract IEnumerable<FunctionValue> GetFunction(QualifiedName functionName);

        /// <summary>
        /// Sets the function.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="declaration">The declaration.</param>
        public abstract void SetFunction(QualifiedName name, FunctionValue declaration);

        #endregion

        #region Classes

        /// <summary>
        /// Gets the collection of defined classes.
        /// </summary>
        /// <returns>
        /// The collection of defined classes.
        /// </returns>
        public abstract IEnumerable<QualifiedName> GetClasses();

        /// <summary>
        /// Determines whether class with specified name is defined.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>True whether class with specified name is defined.</returns>
        public abstract bool IsClassDefined(PHP.Core.QualifiedName name);

        /// <summary>
        /// Tries the get classes with specified class name.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="classValues">The class values.</param>
        /// <returns>
        /// True whether specified class is defined.
        /// </returns>
        public abstract bool TryGetClass(QualifiedName className, out IEnumerable<TypeValue> classValues);

        /// <summary>
        /// Gets the class.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <returns>Class with the specified name.</returns>
        public abstract IEnumerable<TypeValue> GetClass(QualifiedName className);

        /// <summary>
        /// Sets the class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="declaration">The declaration.</param>
        public abstract void SetClass(QualifiedName name, TypeValue declaration);

        #endregion

        #region Aliasses

        /// <summary>
        /// Gets the collection of created aliases in this snapshot.
        /// </summary>
        /// <value>
        /// The created aliases.
        /// </value>
        public abstract IEnumerable<IMemoryAlias> CreatedAliases { get; }

        /// <summary>
        /// Tries the get aliases for specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="aliases">The aliases.</param>
        /// <returns>True whether specified index has aliases.</returns>
        public abstract bool TryGetAliases(MemoryIndex index, out IMemoryAlias aliases);

        /// <summary>
        /// Gets the aliases for specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Aliases for the specified index.</returns>
        /// <exception cref="System.Exception">Missing alias value for  + index</exception>
        public abstract IMemoryAlias GetAliases(MemoryIndex index);

        /// <summary>
        /// Adds the created alias.
        /// </summary>
        /// <param name="aliasData">The alias data.</param>
        public abstract void AddCreatedAlias(IMemoryAlias aliasData);

        /// <summary>
        /// Sets the alias to specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="alias">The alias.</param>
        public abstract void SetAlias(MemoryIndex index, IMemoryAlias alias);

        /// <summary>
        /// Removes the alias from specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        public abstract void RemoveAlias(MemoryIndex index);

        #endregion


    }
}
