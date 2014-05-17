using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.CopyStructure
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
    /// Data of memory locations are stored in snapshot data container which contains associative container
    /// to map memory indexes to <see cref="Weverca.AnalysisFramework.Memory.MemoryEntry"/> instance. Separation
    /// of structure and data allows to have multiple data models with the same data structure to handle both
    /// levels of analysis. Using Data property or public interface can be accesed current version of data.
    /// 
    /// This class should be modified only during the snapshot transaction and then when commit is made there should be
    /// no structural changes at all.
    /// </summary>
    public class SnapshotStructureContainer : AbstractSnapshotStructure
    {
        #region Structure Data

        private List<CopyStackContext> memoryStack;
        private Dictionary<AssociativeArray, IArrayDescriptor> arrayDescriptors;
        private Dictionary<ObjectValue, IObjectDescriptor> objectDescriptors;
        private Dictionary<MemoryIndex, IIndexDefinition> indexDefinitions;
        private Dictionary<AssociativeArray, CopySet<Snapshot>> callArrays;
        private List<IMemoryAlias> createdAliases;
        private CopySet<FunctionValue> functionDecl;
        private CopySet<TypeValue> classDecl;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotStructureContainer"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        private SnapshotStructureContainer(Snapshot snapshot)
            : base(snapshot)
        {
            createdAliases = new List<IMemoryAlias>();
        }

        /// <summary>
        /// Creates empty structure which contains no data in containers.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New empty structure which contains no data in containers.</returns>
        public static SnapshotStructureContainer CreateEmpty(Snapshot snapshot)
        {
            SnapshotStructureContainer data = new SnapshotStructureContainer(snapshot);
            data.memoryStack = new List<CopyStackContext>();
            data.arrayDescriptors = new Dictionary<AssociativeArray, IArrayDescriptor>();
            data.objectDescriptors = new Dictionary<ObjectValue, IObjectDescriptor>();
            data.indexDefinitions = new Dictionary<MemoryIndex, IIndexDefinition>();
            data.functionDecl = new CopySet<FunctionValue>();
            data.classDecl = new CopySet<TypeValue>();
            data.callArrays = new Dictionary<AssociativeArray, CopySet<Snapshot>>();

            return data;
        }

        /// <summary>
        /// Creates the structure with memory stack with global level only.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New structure with memory stack with global level only.</returns>
        public static SnapshotStructureContainer CreateGlobal(Snapshot snapshot)
        {
            SnapshotStructureContainer data = new SnapshotStructureContainer(snapshot);
            data.memoryStack = new List<CopyStackContext>();
            data.arrayDescriptors = new Dictionary<AssociativeArray, IArrayDescriptor>();
            data.objectDescriptors = new Dictionary<ObjectValue, IObjectDescriptor>();
            data.indexDefinitions = new Dictionary<MemoryIndex, IIndexDefinition>();
            data.functionDecl = new CopySet<FunctionValue>();
            data.classDecl = new CopySet<TypeValue>();
            data.callArrays = new Dictionary<AssociativeArray, CopySet<Snapshot>>();

            data.AddLocalLevel();

            return data;
        }

        /// <summary>
        /// Creates new structure object as copy of this structure which contains the same data as this instace.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New copy of this structure which contains the same data as this instace.</returns>
        public SnapshotStructureContainer Copy(Snapshot snapshot)
        {
            SnapshotStructureContainer data = new SnapshotStructureContainer(snapshot);
            data.memoryStack = new List<CopyStackContext>();
            foreach (CopyStackContext context in this.memoryStack)
            {
                memoryStack.Add(new CopyStackContext(context));
            }

            data.arrayDescriptors = new Dictionary<AssociativeArray, IArrayDescriptor>(this.arrayDescriptors);
            data.objectDescriptors = new Dictionary<ObjectValue, IObjectDescriptor>(this.objectDescriptors);
            data.indexDefinitions = new Dictionary<MemoryIndex, IIndexDefinition>(this.indexDefinitions);
            data.functionDecl = new CopySet<FunctionValue>(this.functionDecl);
            data.classDecl = new CopySet<TypeValue>(this.classDecl);
            data.callArrays = new Dictionary<AssociativeArray, CopySet<Snapshot>>(this.callArrays);

            return data;
        }

        #region MemoryStack

        /// <inheritdoc />
        public override IReadonlyStackContext ReadonlyLocalContext
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        public override IReadonlyStackContext ReadonlyGlobalContext
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        public override IEnumerable<IReadonlyStackContext> ReadonlyStackContexts
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        public override IReadonlyStackContext GetReadonlyStackContext(int level)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override IWriteableStackContext WriteableLocalContext
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        public override IWriteableStackContext WriteableGlobalContext
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        public override IEnumerable<IWriteableStackContext> WriteableStackContexts
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        public override IWriteableStackContext GetWriteableStackContext(int level)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void AddLocalLevel()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Indexes

        /// <inheritdoc />
        public override IEnumerable<MemoryIndex> Indexes
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        public override IEnumerable<KeyValuePair<MemoryIndex, IIndexDefinition>> IndexDefinitions
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        public override bool IsDefined(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override bool TryGetIndexDefinition(MemoryIndex index, out IIndexDefinition data)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override IIndexDefinition GetIndexDefinition(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override int GetNumberOfIndexes()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void NewIndex(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void RemoveIndex(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Objects

        /// <inheritdoc />
        public override IEnumerable<KeyValuePair<ObjectValue, IObjectDescriptor>> ObjectDescriptors
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        public override IObjectDescriptor GetDescriptor(ObjectValue objectValue)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override bool TryGetDescriptor(ObjectValue objectValue, out IObjectDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override bool HasObjects(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override IObjectValueContainer GetObjects(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void SetDescriptor(ObjectValue objectValue, IObjectDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void SetObjects(MemoryIndex index, IObjectValueContainer objects)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Arrays

        /// <inheritdoc />
        public override IEnumerable<KeyValuePair<AssociativeArray, IArrayDescriptor>> ArrayDescriptors
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        public override bool TryGetDescriptor(AssociativeArray arrayValue, out IArrayDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override IArrayDescriptor GetDescriptor(AssociativeArray arrayValue)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override AssociativeArray GetArray(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override bool HasArray(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override bool TryGetArray(MemoryIndex index, out AssociativeArray arrayValue)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override bool TryGetCallArraySnapshot(AssociativeArray array, out IEnumerable<Snapshot> snapshots)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void SetDescriptor(AssociativeArray arrayvalue, IArrayDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void AddCallArray(AssociativeArray array, CopyMemoryModel.Snapshot snapshot)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void SetArray(MemoryIndex index, AssociativeArray arrayValue)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void RemoveArray(MemoryIndex index, AssociativeArray arrayValue)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Functions

        /// <inheritdoc />
        public override bool IsFunctionDefined(PHP.Core.QualifiedName functionName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override IEnumerable<FunctionValue> GetFunction(PHP.Core.QualifiedName functionName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void SetFunction(PHP.Core.QualifiedName name, FunctionValue declaration)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Classes
        
        /// <inheritdoc />
        public override bool IsClassDefined(PHP.Core.QualifiedName name)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override IEnumerable<TypeValue> GetClass(PHP.Core.QualifiedName className)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void SetClass(PHP.Core.QualifiedName name, TypeValue declaration)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Aliasses

        /// <inheritdoc />
        public override IEnumerable<IMemoryAlias> CreatedAliases
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        public override bool TryGetAliases(MemoryIndex index, out IMemoryAlias aliases)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override IMemoryAlias GetAliases(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void AddCreatedAlias(IMemoryAlias aliasData)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void SetAlias(MemoryIndex index, IMemoryAlias alias)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void RemoveAlias(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
