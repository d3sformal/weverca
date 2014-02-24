using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHP.Core;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Contains structural data of current memory snapshot - defined variables, arrays, objects and structural
    /// information about memory locations. 
    /// 
    /// Whole structure is based on MemoryIndex instances as the linking objects which map each memory location
    /// to its data, children or aliasses. For more informations about memory indexes and their types
    /// <see cref="MemoryIndex" />.
    /// 
    /// Each structure contains mapping between names of variables and current memory indexes stored in call stack
    /// hierarchy to distinguish between call levels. Variables and ContolVariables stacks (and also list of
    /// temporary indexes)  are the root containers of the memory model where the tree traversing usually starts.
    /// 
    /// An order to traverse the lookup path by fields and indexes there are another containers ArrayDescriptors
    /// and ObjectDescriptors where names of endexes and fields are mapped to memory indexes.
    /// 
    /// Another structural information are stored in associative container IndexData which maps memory index to
    /// IndexData object. In this container can be found definition of alias connections, associated array for
    /// memory location and objecst which are stored there.
    /// 
    /// Data of memory locations are stored in <see cref="SnapshotData"/> container which contains associative container
    /// to map memory indexes to <see cref="Weverca.AnalysisFramework.Memory.MemoryEntry"/> instance. Separation
    /// of structure and data allows to have multiple data models with the same data structure to handle both
    /// levels of analysis. Using Data property or public interface can be accesed current version of data.
    /// 
    /// This class should be modified only during the snapshot transaction and then when commit is made there should be
    /// no structural changes at all.
    /// </summary>
    public class SnapshotStructure
    {

        /// <summary>
        /// Incremental counter for structure unique identifier.
        /// </summary>
        static int DATA_ID = 0;

        /// <summary>
        /// The unique identifier for the data structure of the memory model.
        /// </summary>
        int dataId = DATA_ID++;

        #region Properties

        /// <summary>
        /// Gets the data unique identifier.
        /// </summary>
        /// <value>
        /// The data unique identifier.
        /// </value>
        public int DataId { get { return dataId; } }

        /// <summary>
        /// Gets the snapshot where this structure belongs to.
        /// </summary>
        /// <value>
        /// The snapshot.
        /// </value>
        internal Snapshot Snapshot { get; private set; }

        /// <summary>
        /// Gets or sets the data which maps memory location to current memory entries.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public SnapshotData Data { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether structural changes are allowed or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if structural changes are forbiden; otherwise, <c>false</c>.
        /// </value>
        public bool Locked { get; set; }

        #endregion

        #region Structure Data

        /// <summary>
        /// Gets the collection with definition of temporary indexes for each level of memory stack.
        /// </summary>
        /// <value>
        /// The temporary indexes.
        /// </value>
        internal MemoryStack<IndexSet<TemporaryIndex>> Temporary { get; private set; }

        /// <summary>
        /// Gets the collection with definition of indexes of variables for each level of memory stack.
        /// </summary>
        /// <value>
        /// The variables stack.
        /// </value>
        internal VariableStack Variables { get; private set; }

        /// <summary>
        /// Gets the collection with definition of indexes of control variables for each level of memory stack.
        /// </summary>
        /// <value>
        /// The contol variables stack.
        /// </value>
        internal VariableStack ContolVariables { get; private set; }

        /// <summary>
        /// Gets the associative array which maps array identifier to object with decription of the array structure.
        /// </summary>
        /// <value>
        /// The array descriptors collection.
        /// </value>
        internal Dictionary<AssociativeArray, ArrayDescriptor> ArrayDescriptors { get; private set; }

        /// <summary>
        /// Gets the associative array which maps object identifier to the decription of the object structure.
        /// </summary>
        /// <value>
        /// The object descriptors.
        /// </value>
        internal Dictionary<ObjectValue, ObjectDescriptor> ObjectDescriptors { get; private set; }

        /// <summary>
        /// Gets the associative container with structural data connected to each memory index.
        /// Every memory index which is defined in the memory model is stored in this collection.
        /// </summary>
        /// <value>
        /// The index data.
        /// </value>
        internal Dictionary<MemoryIndex, IndexData> IndexData { get; private set; }

        /// <summary>
        /// Gets the set of all arrays which is defined on local level of memory stack in memory model.
        /// This list is used to fill CallArrays by local context when the called function is exitted. 
        /// </summary>
        /// <value>
        /// The arrays.
        /// </value>
        internal MemoryStack<IndexSet<AssociativeArray>> Arrays { get; private set; }

        /// <summary>
        /// Gets the collection of arrays from the local levels of returned call.
        /// This property allows parent snapshot to get the value of array which was returned from function
        /// - return value is stored in the local control vaiable and local valueas are cleared at the end of function.
        /// </summary>
        /// <value>
        /// The call arrays.
        /// </value>
        internal Dictionary<AssociativeArray, IndexSet<Snapshot>> CallArrays { get; private set; }

        /// <summary>
        /// Gets the list of alliases which was created in this instance of structure.
        /// This list is used in info phase of analisys to transfer info values between aliassed locations. 
        /// </summary>
        /// <value>
        /// The created aliases.
        /// </value>
        internal List<AliasData> CreatedAliases { get; private set; }

        /// <summary>
        /// Gets the list of function declarations.
        /// </summary>
        /// <value>
        /// The function decl.
        /// </value>
        internal DeclarationContainer<FunctionValue> FunctionDecl { get; private set; }

        /// <summary>
        /// Gets the list of class declarations.
        /// </summary>
        /// <value>
        /// The class decl.
        /// </value>
        internal DeclarationContainer<TypeValue> ClassDecl { get; private set; }

        #endregion

        #region Construct structure

        /// <summary>
        /// Prevents a default instance of the <see cref="SnapshotStructure"/> class from being created.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        private SnapshotStructure(Snapshot snapshot)
        {
            Snapshot = snapshot;

            CreatedAliases = new List<AliasData>();
        }

        /// <summary>
        /// Creates empty structure which contains no data in containers.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns></returns>
        public static SnapshotStructure CreateEmpty(Snapshot snapshot)
        {
            SnapshotStructure data = new SnapshotStructure(snapshot);

            data.ArrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>();
            data.ObjectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>();
            data.IndexData = new Dictionary<MemoryIndex, IndexData>();

            data.Variables = new VariableStack(snapshot.CallLevel);
            data.ContolVariables = new VariableStack(snapshot.CallLevel);
            data.FunctionDecl = new DeclarationContainer<FunctionValue>();
            data.ClassDecl = new DeclarationContainer<TypeValue>();

            data.Temporary = new MemoryStack<IndexSet<TemporaryIndex>>(snapshot.CallLevel);
            data.Arrays = new MemoryStack<IndexSet<AssociativeArray>>(snapshot.CallLevel);
            data.CallArrays = new Dictionary<AssociativeArray, IndexSet<Snapshot>>();

            return data;
        }

        /// <summary>
        /// Creates the structure with memory stack with global level only.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="snapshotData">The snapshot data.</param>
        /// <returns></returns>
        public static SnapshotStructure CreateGlobal(Snapshot snapshot, SnapshotData snapshotData)
        {
            SnapshotStructure data = new SnapshotStructure(snapshot);
            data.Data = snapshotData;

            data.ArrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>();
            data.ObjectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>();
            data.IndexData = new Dictionary<MemoryIndex, IndexData>();

            data.Variables = data.createMemoryStack(VariableIndex.CreateUnknown(Snapshot.GLOBAL_CALL_LEVEL));
            data.ContolVariables = data.createMemoryStack(ControlIndex.CreateUnknown(Snapshot.GLOBAL_CALL_LEVEL));
            data.FunctionDecl = new DeclarationContainer<FunctionValue>();
            data.ClassDecl = new DeclarationContainer<TypeValue>();

            data.Temporary = new MemoryStack<IndexSet<TemporaryIndex>>(new IndexSet<TemporaryIndex>());
            data.Arrays = new MemoryStack<IndexSet<AssociativeArray>>(new IndexSet<AssociativeArray>());
            data.CallArrays = new Dictionary<AssociativeArray, IndexSet<Snapshot>>();

            return data;
        }

        /// <summary>
        /// Creates new structure object as copy of this structure which contains the same data as this instace.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="snapshotData">The snapshot data.</param>
        /// <returns></returns>
        public SnapshotStructure Copy(Snapshot snapshot, SnapshotData snapshotData)
        {
            SnapshotStructure data = new SnapshotStructure(snapshot);
            data.Data = snapshotData;

            data.ArrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>(ArrayDescriptors);
            data.ObjectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>(ObjectDescriptors);
            data.IndexData = new Dictionary<MemoryIndex, IndexData>(IndexData);
            data.FunctionDecl = new DeclarationContainer<FunctionValue>(FunctionDecl);
            data.ClassDecl = new DeclarationContainer<TypeValue>(ClassDecl);

            data.Variables = new VariableStack(Variables);
            data.ContolVariables = new VariableStack(ContolVariables);

            data.Temporary = new MemoryStack<IndexSet<TemporaryIndex>>(Temporary);
            data.Arrays = new MemoryStack<IndexSet<AssociativeArray>>(Arrays);
            data.CallArrays = new Dictionary<AssociativeArray, IndexSet<Snapshot>>(CallArrays);

            return data;
        }

        /// <summary>
        ///  Creates new structure object as copy of this structure and adds new local level into memory stacks.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="snapshotData">The snapshot data.</param>
        /// <returns></returns>
        public SnapshotStructure CopyAndAddLocalLevel(Snapshot snapshot, SnapshotData snapshotData)
        {
            SnapshotStructure data = new SnapshotStructure(snapshot);
            data.Data = snapshotData;

            data.ArrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>(ArrayDescriptors);
            data.ObjectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>(ObjectDescriptors);
            data.IndexData = new Dictionary<MemoryIndex, IndexData>(IndexData);
            data.FunctionDecl = new DeclarationContainer<FunctionValue>(FunctionDecl);
            data.ClassDecl = new DeclarationContainer<TypeValue>(ClassDecl);

            data.Variables = new VariableStack(Variables, data.createIndexContainer(VariableIndex.CreateUnknown(Variables.Length)));
            data.ContolVariables = new VariableStack(ContolVariables, data.createIndexContainer(ControlIndex.CreateUnknown(ContolVariables.Length)));

            data.Temporary = new MemoryStack<IndexSet<TemporaryIndex>>(Temporary, new IndexSet<TemporaryIndex>());
            data.Arrays = new MemoryStack<IndexSet<AssociativeArray>>(Arrays, new IndexSet<AssociativeArray>());
            data.CallArrays = new Dictionary<AssociativeArray, IndexSet<Snapshot>>(CallArrays);

            return data;
        }

        /// <summary>
        /// Creates the memory stack object with given unknown index.
        /// </summary>
        /// <param name="unknownIndex">Index of the unknown.</param>
        /// <returns></returns>
        private VariableStack createMemoryStack(MemoryIndex unknownIndex)
        {
            return new VariableStack(createIndexContainer(unknownIndex));
        }

        /// <summary>
        /// Creates the index container object with given unknown object.
        /// </summary>
        /// <param name="unknownIndex">Index of the unknown.</param>
        /// <returns></returns>
        private IndexContainer createIndexContainer(MemoryIndex unknownIndex)
        {
            IndexContainer container = new IndexContainer(unknownIndex);
            NewIndex(unknownIndex);

            return container;
        }

        #endregion

        /// <summary>
        /// Tests whether snapshot structure is locked and if is throws exception.
        /// This method is called from every method which provides structural changes.
        /// </summary>
        /// <exception cref="System.Exception">Snapshot structure is locked in this mode. Mode:  + Snapshot.CurrentMode</exception>
        private void lockedTest()
        {
            if (Locked)
            {
                throw new Exception("Snapshot structure is locked in this mode. Mode: " + Snapshot.CurrentMode);
            }
        }

        #region Snapshot Transaction

        /// <summary>
        /// Compares this structure object with given copy of old data.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <returns><c>true</c> if the data are same; otherwise, <c>false</c>.</returns>
        public bool DataEquals(SnapshotStructure oldValue)
        {
            if (!compareData(oldValue))
            {
                return false;
            }

            return compareDeclarations(oldValue);
        }

        /// <summary>
        /// Compares this structure object with given copy of old data and applies simplify limit when new memory entry is bigger than given simplify limit.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="simplifyLimit">The simplify limit.</param>
        /// <returns><c>true</c> if the data are same; otherwise, <c>false</c>.</returns>
        public bool DataEqualsAndSimplify(SnapshotStructure oldValue, int simplifyLimit, MemoryAssistantBase assistant)
        {
            if (!compareDataAndSimplify(oldValue, simplifyLimit, assistant))
            {
                return false;
            }

            return compareDeclarations(oldValue);
        }

        /// <summary>
        /// Compares this structure object with given copy of old data and applies widening operation on memory entry which differs.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="simplifyLimit">The simplify limit.</param>
        /// <param name="assistant">The assistant.</param>
        /// <returns><c>true</c> if the data are same; otherwise, <c>false</c>.</returns>
        public bool WidenNotEqual(SnapshotStructure oldValue, int simplifyLimit, MemoryAssistantBase assistant)
        {
            lockedTest();

            bool funcCount = this.FunctionDecl.Count == oldValue.FunctionDecl.Count;
            bool classCount = this.ClassDecl.Count == oldValue.ClassDecl.Count;

            if (!widenNotEqualData(oldValue, simplifyLimit, assistant))
            {
                return false;
            }

            return compareDeclarations(oldValue);
        }

        /// <summary>
        /// Compares the declarations of functions and classes whether there is some structural changes between this and old structure object.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <returns><c>true</c> if the data are same; otherwise, <c>false</c>.</returns>
        private bool compareDeclarations(SnapshotStructure oldValue)
        {
            bool funcCount = this.FunctionDecl.Count == oldValue.FunctionDecl.Count;
            bool classCount = this.ClassDecl.Count == oldValue.ClassDecl.Count;

            if (classCount && funcCount)
            {
                if (!this.FunctionDecl.DataEquals(oldValue.FunctionDecl))
                {
                    return false;
                }

                if (!this.ClassDecl.DataEquals(oldValue.ClassDecl))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Compares this structure object with given copy of old data.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <returns><c>true</c> if the data are same; otherwise, <c>false</c>.</returns>
        private bool compareData(SnapshotStructure oldValue)
        {
            HashSet<MemoryIndex> usedIndexes = new HashSet<MemoryIndex>();
            HashSetTools.AddAll(usedIndexes, this.IndexData.Keys);
            HashSetTools.AddAll(usedIndexes, oldValue.IndexData.Keys);

            IndexData emptyStructure = new CopyMemoryModel.IndexData(null, null, null);

            foreach (MemoryIndex index in usedIndexes)
            {
                if (index is TemporaryIndex)
                {
                    continue;
                }
                IndexData newStructure = getDataOrUndefined(index, this, emptyStructure);
                IndexData oldStructure = getDataOrUndefined(index, oldValue, emptyStructure);

                if (!newStructure.DataEquals(oldStructure))
                {
                    return false;
                }

                if (!Data.DataEquals(oldValue.Data, index))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Compares this structure object with given copy of old data and applies simplify limit when new memory entry is bigger than given simplify limit.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="simplifyLimit">The simplify limit.</param>
        /// <param name="assistant">The assistant.</param>
        /// <returns><c>true</c> if the data are same; otherwise, <c>false</c>.</returns>
        private bool compareDataAndSimplify(SnapshotStructure oldValue, int simplifyLimit, MemoryAssistantBase assistant)
        {
            HashSet<MemoryIndex> usedIndexes = new HashSet<MemoryIndex>();
            HashSetTools.AddAll(usedIndexes, this.IndexData.Keys);
            HashSetTools.AddAll(usedIndexes, oldValue.IndexData.Keys);

            IndexData emptyStructure = new CopyMemoryModel.IndexData(null, null, null);

            bool areEqual = true;

            foreach (MemoryIndex index in usedIndexes)
            {
                if (index is TemporaryIndex)
                {
                    continue;
                }
                IndexData newStructure = getDataOrUndefined(index, this, emptyStructure);
                IndexData oldStructure = getDataOrUndefined(index, oldValue, emptyStructure);

                if (!newStructure.DataEquals(oldStructure))
                {
                    areEqual = false;
                }

                if (!Data.DataEqualsAndSimplify(oldValue.Data, index, simplifyLimit, assistant))
                {
                    areEqual = false;
                }
            }

            return areEqual;
        }

        /// <summary>
        /// Compares this structure object with given copy of old data and applies widening operation on memory entry which differs.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="simplifyLimit">The simplify limit.</param>
        /// <param name="assistant">The assistant.</param>
        /// <returns><c>true</c> if the data are same; otherwise, <c>false</c>.</returns>
        private bool widenNotEqualData(SnapshotStructure oldValue, int simplifyLimit, MemoryAssistantBase assistant)
        {
            HashSet<MemoryIndex> indexes = new HashSet<MemoryIndex>();
            HashSetTools.AddAll(indexes, this.IndexData.Keys);
            HashSetTools.AddAll(indexes, oldValue.IndexData.Keys);

            IndexData emptyData = new CopyMemoryModel.IndexData(null, null, null);

            bool areEqual = true;
            foreach (MemoryIndex index in indexes)
            {
                IndexData newData = getDataOrUndefined(index, this, emptyData);
                IndexData oldData = getDataOrUndefined(index, oldValue, emptyData);

                Data.DataWiden(oldValue.Data, index, assistant);

                if (!Data.DataEqualsAndSimplify(oldValue.Data, index, simplifyLimit, assistant))
                {
                    areEqual = false;
                }
            }

            return areEqual;
        }

        /// <summary>
        /// Gets the index which is not above the local level of this structure.
        /// This method alows to widen data in local levels of shared functions.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="comparedStructure">The compared structure.</param>
        /// <param name="thisStructure">The this structure.</param>
        /// <returns></returns>
        private MemoryIndex getIndexOrLocal(MemoryIndex index, SnapshotStructure comparedStructure, SnapshotStructure thisStructure)
        {
            /*if (index.CallLevel == comparedStructure.Snapshot.CallLevel && thisStructure.Snapshot.CallLevel < index.CallLevel)
            {
                return index.SetCallLevel(thisStructure.Snapshot.CallLevel);
            }
            else
            {
                return index;
            }*/
            return index;
        }

        /// <summary>
        /// Gets the data if is set in structure or returns given empty data.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="snapshotStructure">The snapshot structure.</param>
        /// <param name="emptyData">The empty data.</param>
        /// <returns></returns>
        private CopyMemoryModel.IndexData getDataOrUndefined(MemoryIndex index, SnapshotStructure snapshotStructure, IndexData emptyData)
        {
            IndexData data = null;
            if (!snapshotStructure.IndexData.TryGetValue(index, out data))
            {
                data = emptyData;
            }

            return data;
        }

        #endregion

        #region Indexes

        /// <summary>
        /// Insert newly created index into structure and data collection.
        /// </summary>
        /// <param name="index">The index.</param>
        internal void NewIndex(MemoryIndex index)
        {
            lockedTest();

            IndexData data = new IndexData(null, null, null);

            IndexData[index] = data;
            Data.SetMemoryEntry(index, new MemoryEntry(Snapshot.UndefinedValue));
        }

        /// <summary>
        /// Determines whether the specified index is defined.
        /// </summary>
        /// <param name="index">The index.</param>
        internal bool IsDefined(MemoryIndex index)
        {
            return IndexData.ContainsKey(index);
        }

        /// <summary>
        /// Removes the index from structure and data.
        /// </summary>
        /// <param name="index">The index.</param>
        internal void RemoveIndex(MemoryIndex index)
        {
            lockedTest();

            IndexData.Remove(index);
            Data.RemoveMemoryEntry(index);
        }

        /// <summary>
        /// Tries to get data of given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="data">The data.</param>
        /// <returns><c>true</c> if the data are same; otherwise, <c>false</c>.</returns>
        internal bool TryGetIndexData(MemoryIndex index, out IndexData data)
        {
            return IndexData.TryGetValue(index, out data);
        }

        /// <summary>
        /// Gets the data of specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Missing alias value for given index</exception>
        internal IndexData GetIndexData(MemoryIndex index)
        {
            IndexData data;
            if (IndexData.TryGetValue(index, out data))
            {
                return data;
            }
            throw new Exception("Missing alias value for " + index);
        }

        #endregion

        #region Objects

        /// <summary>
        /// Gets the PHP object descriptor which contains defined fields and informations about object.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Missing object descriptor</exception>
        internal ObjectDescriptor GetDescriptor(ObjectValue objectValue)
        {
            ObjectDescriptor descriptor;
            if (ObjectDescriptors.TryGetValue(objectValue, out descriptor))
            {
                return descriptor;
            }
            else
            {
                throw new Exception("Missing object descriptor");
            }
        }

        /// <summary>
        /// Tries to get the PHP object descriptor which contains defined fields and informations about object.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns></returns>
        internal bool TryGetDescriptor(ObjectValue objectValue, out ObjectDescriptor descriptor)
        {
            return ObjectDescriptors.TryGetValue(objectValue, out descriptor);
        }

        /// <summary>
        /// Sets the PHP object descriptor which contains defined fields and informations about object.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        /// <param name="descriptor">The descriptor.</param>
        internal void SetDescriptor(ObjectValue objectValue, ObjectDescriptor descriptor)
        {
            lockedTest();

            ObjectDescriptors[objectValue] = descriptor;
        }

        /// <summary>
        /// Determines whether the specified index has some PHP objects.
        /// </summary>
        /// <param name="index">The index.</param>
        ///   <c>true</c> if specified index has some PHP objects; otherwise, <c>false</c>.
        internal bool HasObjects(MemoryIndex index)
        {
            IndexData data;
            if (IndexData.TryGetValue(index, out data))
            {
                return data.Objects != null;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the objects for given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public ObjectValueContainer GetObjects(MemoryIndex index)
        {
            IndexData data;
            if (IndexData.TryGetValue(index, out data))
            {
                if (data.Objects != null)
                {
                    return data.Objects;
                }
            }

            return new ObjectValueContainer();
        }

        /// <summary>
        /// Sets objects for given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="objects">The objects.</param>
        internal void SetObjects(MemoryIndex index, ObjectValueContainer objects)
        {
            lockedTest();

            IndexData data;
            if (!IndexData.TryGetValue(index, out data))
            {
                data = new IndexData(null, null, null);
            }

            IndexDataBuilder builder = data.Builder();
            builder.Objects = objects;

            IndexData[index] = builder.Build();
        }

        #endregion

        #region Arrays

        /// <summary>
        /// Tries to get array descriptor which contains information about defined indexes in the specified array.
        /// </summary>
        /// <param name="arrayValue">The array value.</param>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns></returns>
        internal bool TryGetDescriptor(AssociativeArray arrayValue, out ArrayDescriptor descriptor)
        {
            return ArrayDescriptors.TryGetValue(arrayValue, out descriptor);
        }

        /// <summary>
        /// Gets the array descriptor which contains information about defined indexes in the specified array.
        /// </summary>
        /// <param name="arrayValue">The array value.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Missing array descriptor</exception>
        internal ArrayDescriptor GetDescriptor(AssociativeArray arrayValue)
        {
            ArrayDescriptor descriptor;
            if (ArrayDescriptors.TryGetValue(arrayValue, out descriptor))
            {
                return descriptor;
            }
            else
            {
                throw new Exception("Missing array descriptor");
            }
        }

        /// <summary>
        /// Sets the array descriptor which contains information about defined indexes in the specified array.
        /// </summary>
        /// <param name="arrayvalue">The arrayvalue.</param>
        /// <param name="descriptor">The descriptor.</param>
        internal void SetDescriptor(AssociativeArray arrayvalue, ArrayDescriptor descriptor)
        {
            lockedTest();

            ArrayDescriptors[arrayvalue] = descriptor;
        }

        /// <summary>
        /// Gets the array descriptor which contains information about defined indexes in the specified array.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Missing array for index  + index</exception>
        internal AssociativeArray GetArray(MemoryIndex index)
        {
            IndexData data;
            if (IndexData.TryGetValue(index, out data))
            {
                if (data.Array != null)
                {
                    return data.Array;
                }
            }

            throw new Exception("Missing array for index " + index);
        }

        /// <summary>
        /// Determines whether the specified index has array.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        internal bool HasArray(MemoryIndex index)
        {
            IndexData data;
            if (IndexData.TryGetValue(index, out data))
            {
                return data.Array != null;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to get array for specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="arrayValue">The array value.</param>
        /// <returns></returns>
        internal bool TryGetArray(MemoryIndex index, out AssociativeArray arrayValue)
        {
            IndexData data;
            if (IndexData.TryGetValue(index, out data))
            {
                arrayValue = data.Array;
                return data.Array != null;
            }
            else
            {
                arrayValue = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to get list of spashots which contains specified array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="snapshots">The snapshots.</param>
        /// <returns></returns>
        internal bool TryGetCallArraySnapshot(AssociativeArray array, out IEnumerable<Snapshot> snapshots)
        {
            IndexSet<Snapshot> snapshotSet = null;
            if (CallArrays.TryGetValue(array, out snapshotSet))
            {
                snapshots = snapshotSet;
                return true;
            }
            else
            {
                snapshots = null;
                return false;
            }
        }

        /// <summary>
        /// Adds the combination of array and snapshot into call arrays set.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="snapshot">The snapshot.</param>
        internal void AddCallArray(AssociativeArray array, CopyMemoryModel.Snapshot snapshot)
        {
            IndexSet<Snapshot> snapshots;
            if (!CallArrays.TryGetValue(array, out snapshots))
            {
                snapshots = new IndexSet<Snapshot>();
                CallArrays[array] = snapshots;
            }

            snapshots.Add(snapshot);
        }

        /// <summary>
        /// Sets the array for specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="arrayValue">The array value.</param>
        internal void SetArray(MemoryIndex index, AssociativeArray arrayValue)
        {
            lockedTest();

            IndexData data;
            if (!IndexData.TryGetValue(index, out data))
            {
                data = new IndexData(null, null, null);
            }

            IndexDataBuilder builder = data.Builder();
            builder.Array = arrayValue;

            IndexData[index] = builder.Build();

            ArrayDescriptor descriptor;
            if (TryGetDescriptor(arrayValue, out descriptor))
            {
                if (descriptor.ParentVariable != null)
                {
                    Arrays[descriptor.ParentVariable.CallLevel].Remove(arrayValue);
                }
            }
            Arrays[index.CallLevel].Add(arrayValue);
        }

        /// <summary>
        /// Removes the array from specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="arrayValue">The array value.</param>
        internal void RemoveArray(MemoryIndex index, AssociativeArray arrayValue)
        {
            lockedTest();

            ArrayDescriptors.Remove(arrayValue);

            IndexData data;
            if (!IndexData.TryGetValue(index, out data))
            {
                data = new IndexData(null, null, null);
            }

            IndexDataBuilder builder = data.Builder();
            builder.Array = null;

            IndexData[index] = builder.Build();
            Arrays[index.CallLevel].Remove(arrayValue);
        }

        #endregion

        #region Functions

        /// <summary>
        /// Determines whether function with given name is defined.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <returns></returns>
        internal bool IsFunctionDefined(PHP.Core.QualifiedName functionName)
        {
            return FunctionDecl.ContainsKey(functionName);
        }

        /// <summary>
        /// Gets the function.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <returns></returns>
        internal IEnumerable<FunctionValue> GetFunction(PHP.Core.QualifiedName functionName)
        {
            return FunctionDecl.GetValue(functionName);
        }

        /// <summary>
        /// Sets the function.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="declaration">The declaration.</param>
        internal void SetFunction(QualifiedName name, FunctionValue declaration)
        {
            lockedTest();

            FunctionDecl.Add(name, declaration);
        }

        #endregion

        #region Classes

        /// <summary>
        /// Determines whether class with specified name is defined.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        internal bool IsClassDefined(PHP.Core.QualifiedName name)
        {
            return ClassDecl.ContainsKey(name);
        }

        /// <summary>
        /// Sets the class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="declaration">The declaration.</param>
        internal void SetClass(PHP.Core.QualifiedName name, TypeValue declaration)
        {
            lockedTest();

            ClassDecl.Add(name, declaration);
        }

        /// <summary>
        /// Gets the class.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <returns></returns>
        internal IEnumerable<TypeValue> GetClass(PHP.Core.QualifiedName className)
        {
            return ClassDecl.GetValue(className);
        }

        #endregion

        #region Aliasses

        /// <summary>
        /// Adds the created alias.
        /// </summary>
        /// <param name="aliasData">The alias data.</param>
        internal void AddCreatedAlias(AliasData aliasData)
        {
            CreatedAliases.Add(aliasData);
        }

        /// <summary>
        /// Tries the get aliases for specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="aliases">The aliases.</param>
        /// <returns></returns>
        internal bool TryGetAliases(MemoryIndex index, out MemoryAlias aliases)
        {
            IndexData data;
            if (IndexData.TryGetValue(index, out data))
            {
                aliases = data.Aliases;
                return data.Aliases != null;
            }
            else
            {
                aliases = null;
                return false;
            }
        }

        /// <summary>
        /// Gets the aliases for specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Missing alias value for  + index</exception>
        internal MemoryAlias GetAliases(MemoryIndex index)
        {
            IndexData data;
            if (IndexData.TryGetValue(index, out data))
            {
                if (data.Aliases != null)
                {
                    return data.Aliases;
                }
            }
            throw new Exception("Missing alias value for " + index);
        }

        /// <summary>
        /// Sets the alias to specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="alias">The alias.</param>
        internal void SetAlias(MemoryIndex index, MemoryAlias alias)
        {
            lockedTest();

            IndexData data;
            if (!IndexData.TryGetValue(index, out data))
            {
                data = new IndexData(null, null, null);
            }

            IndexDataBuilder builder = data.Builder();
            builder.Aliases = alias;

            IndexData[index] = builder.Build();
        }

        /// <summary>
        /// Removes the alias from specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        internal void RemoveAlias(MemoryIndex index)
        {
            lockedTest();

            IndexData data;
            if (!IndexData.TryGetValue(index, out data))
            {
                data = new IndexData(null, null, null);
            }

            IndexDataBuilder builder = data.Builder();
            builder.Aliases = null;

            IndexData[index] = builder.Build();
        }

        #endregion

        #region MemoryEntries

        /// <summary>
        /// Gets the memory entry from actual index data collection.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        internal MemoryEntry GetMemoryEntry(MemoryIndex index)
        {
            return Data.GetMemoryEntry(index);
        }

        /// <summary>
        /// Tries to get memory entry from actual index data collection.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="entry">The entry.</param>
        /// <returns></returns>
        internal bool TryGetMemoryEntry(MemoryIndex index, out MemoryEntry entry)
        {
            return Data.TryGetMemoryEntry(index, out entry);
        }

        /// <summary>
        /// Sets the memory entry to actual index data collection.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="memoryEntry">The memory entry.</param>
        internal void SetMemoryEntry(MemoryIndex index, MemoryEntry memoryEntry)
        {
            IndexData data;
            if (IndexData.TryGetValue(index, out data))
            {
                Data.SetMemoryEntry(index, memoryEntry);
            }
            else if (!Locked)
            {
                IndexData[index] = new IndexData(null, null, null);
                Data.SetMemoryEntry(index, memoryEntry);
            }


        }
        #endregion

        #region String representation

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var index in IndexData)
            {
                builder.Append(index.ToString());
                builder.Append("\n");
                //builder.Append(index.Value.MemoryEntry.ToString());

                if (index.Value.Aliases != null)
                {
                    index.Value.Aliases.ToString(builder);
                }

                builder.Append("\n\n");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Gets string representation of all arrays in the memory model.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="infos">The infos.</param>
        /// <returns></returns>
        internal string GetArraysRepresentation(SnapshotData data, SnapshotData infos)
        {
            StringBuilder result = new StringBuilder();

            foreach (var item in ArrayDescriptors)
            {
                IndexContainer.GetRepresentation(item.Value, data, infos, result);
                result.AppendLine();
            }

            return result.ToString();
        }

        /// <summary>
        /// Gets string representation of all objects and fields in memory model.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="infos">The infos.</param>
        /// <returns></returns>
        internal string GetFieldsRepresentation(SnapshotData data, SnapshotData infos)
        {
            StringBuilder result = new StringBuilder();

            foreach (var item in ObjectDescriptors)
            {
                IndexContainer.GetRepresentation(item.Value, data, infos, result);
                result.AppendLine();
            }

            return result.ToString();
        }

        /// <summary>
        /// Gets string representation af aliases between memory locations in memory model.
        /// </summary>
        /// <returns></returns>
        internal string GetAliasesRepresentation()
        {
            StringBuilder result = new StringBuilder();

            foreach (var item in IndexData)
            {
                var aliases = item.Value.Aliases;
                if (aliases != null && (aliases.MayAliasses.Count > 0 || aliases.MustAliasses.Count > 0))
                {
                    MemoryIndex index = item.Key;
                    result.AppendFormat("{0}: {{ ", index);

                    result.Append(" MUST: ");
                    foreach (var alias in aliases.MustAliasses)
                    {
                        result.Append(alias);
                        result.Append(", ");
                    }
                    result.Length -= 2;

                    result.Append(" | MAY: ");
                    foreach (var alias in aliases.MayAliasses)
                    {
                        result.Append(alias);
                        result.Append(", ");
                    }
                    result.Length -= 2;
                    result.AppendLine(" }");
                }
            }

            return result.ToString();
        }

        #endregion
    }
}

