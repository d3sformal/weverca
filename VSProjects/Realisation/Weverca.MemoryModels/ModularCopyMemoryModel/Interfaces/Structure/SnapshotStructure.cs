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
        IReadOnlySnapshotStructure ReadOnlySnapshotStructure { get; }

        /// <summary>
        /// Gets the snapshot structure container for access which allows modifications.
        /// </summary>
        /// <value>
        /// The writeable snapshot structure.
        /// </value>
        IWriteableSnapshotStructure WriteableSnapshotStructure { get; }

        IObjectDescriptor CreateObjectDescriptor(ObjectValue createdObject, TypeValue type, MemoryIndex memoryIndex);

        IArrayDescriptor CreateArrayDescriptor(AssociativeArray createdArray, TemporaryIndex arrayIndex);
        
        IMemoryAlias CreateMemoryAlias(MemoryIndex index);
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
        IEnumerable<KeyValuePair<MemoryIndex, IndexDefinition>> IndexDefinitions { get; }

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
        bool TryGetIndexDefinition(MemoryIndex index, out IndexDefinition data);

        /// <summary>
        /// Gets the definition of specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Definition of specified index.</returns>
        /// <exception cref="System.Exception">Missing alias value for given index</exception>
        IndexDefinition GetIndexDefinition(MemoryIndex index);

        #endregion

        #region Objects

        /// <summary>
        /// Gets the set of object descriptors.
        /// </summary>
        /// <value>
        /// The object descriptors.
        /// </value>
        IEnumerable<KeyValuePair<IObjectDescriptor, IndexDefinition>> ObjectDescriptors { get; }

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
        IEnumerable<KeyValuePair<IArrayDescriptor, IndexDefinition>> ArrayDescriptors { get; }

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
        /// Determines whether function with given name is defined.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <returns>True whether function with given name is defined..</returns>
        bool IsFunctionDefined(PHP.Core.QualifiedName functionName);

        /// <summary>
        /// Gets the function.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <returns>List of functions with given name.</returns>
        IEnumerable<FunctionValue> GetFunction(PHP.Core.QualifiedName functionName);

        #endregion

        #region Classes

        /// <summary>
        /// Determines whether class with specified name is defined.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>True whether class with specified name is defined.</returns>
        bool IsClassDefined(PHP.Core.QualifiedName name);

        /// <summary>
        /// Gets the class.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <returns>Class with the specified name.</returns>
        IEnumerable<TypeValue> GetClass(QualifiedName className);

        #endregion

        #region Aliasses

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

        int GetNumberOfIndexes();
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
        void AddCallArray(AssociativeArray array, CopyMemoryModel.Snapshot snapshot);

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

        void AddLocalLevel();
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

        public IWriteableStackContext WriteableLocalContext
        {
            get { throw new NotImplementedException(); }
        }

        public IWriteableStackContext WriteableGlobalContext
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<IWriteableStackContext> WriteableStackContexts
        {
            get { throw new NotImplementedException(); }
        }

        public IWriteableStackContext GetWriteableStackContext(int level)
        {
            throw new NotImplementedException();
        }

        public void NewIndex(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        public void RemoveIndex(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        public void SetDescriptor(ObjectValue objectValue, IObjectDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        public void SetObjects(MemoryIndex index, IObjectValueContainer objects)
        {
            throw new NotImplementedException();
        }

        public void SetDescriptor(AssociativeArray arrayvalue, IArrayDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        public void AddCallArray(AssociativeArray array, CopyMemoryModel.Snapshot snapshot)
        {
            throw new NotImplementedException();
        }

        public void SetArray(MemoryIndex index, AssociativeArray arrayValue)
        {
            throw new NotImplementedException();
        }

        public void RemoveArray(MemoryIndex index, AssociativeArray arrayValue)
        {
            throw new NotImplementedException();
        }

        public void SetFunction(QualifiedName name, FunctionValue declaration)
        {
            throw new NotImplementedException();
        }

        public void SetClass(QualifiedName name, TypeValue declaration)
        {
            throw new NotImplementedException();
        }

        public void AddCreatedAlias(AliasData aliasData)
        {
            throw new NotImplementedException();
        }

        public void SetAlias(MemoryIndex index, IMemoryAlias alias)
        {
            throw new NotImplementedException();
        }

        public void RemoveAlias(MemoryIndex index)
        {
            throw new NotImplementedException();
        }


        public IReadonlyStackContext ReadonlyLocalContext
        {
            get { throw new NotImplementedException(); }
        }

        public IReadonlyStackContext ReadonlyGlobalContext
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<IReadonlyStackContext> ReadonlyStackContexts
        {
            get { throw new NotImplementedException(); }
        }

        public IReadonlyStackContext GetReadonlyStackContext(int level)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MemoryIndex> Indexes
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<KeyValuePair<MemoryIndex, IndexDefinition>> IndexDefinitions
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsDefined(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        public bool TryGetIndexDefinition(MemoryIndex index, out IndexDefinition data)
        {
            throw new NotImplementedException();
        }

        public IndexDefinition GetIndexDefinition(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<KeyValuePair<IObjectDescriptor, IndexDefinition>> ObjectDescriptors
        {
            get { throw new NotImplementedException(); }
        }

        public IObjectDescriptor GetDescriptor(ObjectValue objectValue)
        {
            throw new NotImplementedException();
        }

        public bool TryGetDescriptor(ObjectValue objectValue, out IObjectDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        public bool HasObjects(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        public IObjectValueContainer GetObjects(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<KeyValuePair<IArrayDescriptor, IndexDefinition>> ArrayDescriptors
        {
            get { throw new NotImplementedException(); }
        }

        public bool TryGetDescriptor(AssociativeArray arrayValue, out IArrayDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        public IArrayDescriptor GetDescriptor(AssociativeArray arrayValue)
        {
            throw new NotImplementedException();
        }

        public AssociativeArray GetArray(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        public bool HasArray(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        public bool TryGetArray(MemoryIndex index, out AssociativeArray arrayValue)
        {
            throw new NotImplementedException();
        }

        public bool TryGetCallArraySnapshot(AssociativeArray array, out IEnumerable<Snapshot> snapshots)
        {
            throw new NotImplementedException();
        }

        public bool IsFunctionDefined(QualifiedName functionName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FunctionValue> GetFunction(QualifiedName functionName)
        {
            throw new NotImplementedException();
        }

        public bool IsClassDefined(QualifiedName name)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TypeValue> GetClass(QualifiedName className)
        {
            throw new NotImplementedException();
        }

        public bool TryGetAliases(MemoryIndex index, out IMemoryAlias aliases)
        {
            throw new NotImplementedException();
        }

        public IMemoryAlias GetAliases(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        public MemoryEntry GetMemoryEntry(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        public bool TryGetMemoryEntry(MemoryIndex index, out MemoryEntry entry)
        {
            throw new NotImplementedException();
        }


        public int GetNumberOfIndexes()
        {
            throw new NotImplementedException();
        }


        public void AddCreatedAlias(IMemoryAlias aliasData)
        {
            throw new NotImplementedException();
        }

        public void AddLocalLevel()
        {
            throw new NotImplementedException();
        }


        public IEnumerable<IMemoryAlias> CreatedAliases
        {
            get { throw new NotImplementedException(); }
        }
    }
}
