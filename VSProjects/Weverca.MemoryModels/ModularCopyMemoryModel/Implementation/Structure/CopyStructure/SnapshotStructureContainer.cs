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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using PHP.Core;

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

        private int localLevel = -1;
        private List<CopyStackContext> memoryStack;
        private Dictionary<AssociativeArray, IArrayDescriptor> arrayDescriptors;
        private Dictionary<ObjectValue, IObjectDescriptor> objectDescriptors;
        private Dictionary<MemoryIndex, IIndexDefinition> indexDefinitions;
        private Dictionary<AssociativeArray, CopySet<Snapshot>> callArrays;
        private CopyDeclarationContainer<FunctionValue> functionDecl;
        private CopyDeclarationContainer<TypeValue> classDecl;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotStructureContainer"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        private SnapshotStructureContainer(ModularMemoryModelFactories factories)
            : base(factories)
        {
        }

        /// <summary>
        /// Creates empty structure which contains no data in containers.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New empty structure which contains no data in containers.</returns>
        public static SnapshotStructureContainer CreateEmpty(ModularMemoryModelFactories factories)
        {
            SnapshotStructureContainer data = new SnapshotStructureContainer(factories);
            data.memoryStack = new List<CopyStackContext>();
            data.arrayDescriptors = new Dictionary<AssociativeArray, IArrayDescriptor>();
            data.objectDescriptors = new Dictionary<ObjectValue, IObjectDescriptor>();
            data.indexDefinitions = new Dictionary<MemoryIndex, IIndexDefinition>();
            data.functionDecl = new CopyDeclarationContainer<FunctionValue>();
            data.classDecl = new CopyDeclarationContainer<TypeValue>();
            data.callArrays = new Dictionary<AssociativeArray, CopySet<Snapshot>>();

            return data;
        }

        /// <summary>
        /// Creates the structure with memory stack with global level only.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New structure with memory stack with global level only.</returns>
        public static SnapshotStructureContainer CreateGlobal(ModularMemoryModelFactories factories)
        {
            SnapshotStructureContainer data = new SnapshotStructureContainer(factories);
            data.memoryStack = new List<CopyStackContext>();
            data.arrayDescriptors = new Dictionary<AssociativeArray, IArrayDescriptor>();
            data.objectDescriptors = new Dictionary<ObjectValue, IObjectDescriptor>();
            data.indexDefinitions = new Dictionary<MemoryIndex, IIndexDefinition>();
            data.functionDecl = new CopyDeclarationContainer<FunctionValue>();
            data.classDecl = new CopyDeclarationContainer<TypeValue>();
            data.callArrays = new Dictionary<AssociativeArray, CopySet<Snapshot>>();

            data.AddLocalLevel();

            return data;
        }

        /// <summary>
        /// Creates new structure object as copy of this structure which contains the same data as this instace.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New copy of this structure which contains the same data as this instace.</returns>
        public SnapshotStructureContainer Copy()
        {
            SnapshotStructureContainer data = new SnapshotStructureContainer(Factories);
            data.memoryStack = new List<CopyStackContext>();
            foreach (CopyStackContext context in this.memoryStack)
            {
                data.memoryStack.Add(new CopyStackContext(context));
                data.localLevel++;
            }

            data.arrayDescriptors = new Dictionary<AssociativeArray, IArrayDescriptor>(this.arrayDescriptors);
            data.objectDescriptors = new Dictionary<ObjectValue, IObjectDescriptor>(this.objectDescriptors);
            data.indexDefinitions = new Dictionary<MemoryIndex, IIndexDefinition>(this.indexDefinitions);
            data.functionDecl = new CopyDeclarationContainer<FunctionValue>(this.functionDecl);
            data.classDecl = new CopyDeclarationContainer<TypeValue>(this.classDecl);
            data.callArrays = new Dictionary<AssociativeArray, CopySet<Snapshot>>(this.callArrays);

            return data;
        }

        #region MemoryStack

        /// <inheritdoc />
        public override int CallLevel
        {
            get { return localLevel; }
        }

        /// <inheritdoc />
        public override bool ContainsStackWithLevel(int level)
        {
            return memoryStack.Count < level;
        }

        /// <inheritdoc />
        public override IReadonlyStackContext ReadonlyLocalContext
        {
            get
            {
                if (memoryStack.Count > 0)
                {
                    return memoryStack[localLevel];
                }
                else
                {
                    throw new IndexOutOfRangeException("Memory stack is empty");
                }
            }
        }

        /// <inheritdoc />
        public override IReadonlyStackContext ReadonlyGlobalContext
        {
            get
            {
                if (memoryStack.Count > 0)
                {
                    return memoryStack[Snapshot.GLOBAL_CALL_LEVEL];
                }
                else
                {
                    throw new IndexOutOfRangeException("Memory stack is empty");
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<IReadonlyStackContext> ReadonlyStackContexts
        {
            get { return memoryStack; }
        }

        /// <inheritdoc />
        public override IReadonlyStackContext GetReadonlyStackContext(int level)
        {
            if (memoryStack.Count > level)
            {
                return memoryStack[level];
            }
            else
            {
                throw new IndexOutOfRangeException("Given level of memory stack is out of memory stack size.");
            }
        }

        /// <inheritdoc />
        public override IWriteableStackContext WriteableLocalContext
        {
            get
            {
                if (memoryStack.Count > 0)
                {
                    return memoryStack[localLevel];
                }
                else
                {
                    throw new IndexOutOfRangeException("Memory stack is empty");
                }
            }
        }

        /// <inheritdoc />
        public override IWriteableStackContext WriteableGlobalContext
        {
            get
            {
                if (memoryStack.Count > 0)
                {
                    return memoryStack[Snapshot.GLOBAL_CALL_LEVEL];
                }
                else
                {
                    throw new IndexOutOfRangeException("Memory stack is empty");
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<IWriteableStackContext> WriteableStackContexts
        {
            get { return memoryStack; }
        }

        /// <inheritdoc />
        public override IWriteableStackContext GetWriteableStackContext(int level)
        {
            if (memoryStack.Count > level)
            {
                return memoryStack[level];
            }
            else
            {
                throw new IndexOutOfRangeException("Given level of memory stack is out of memory stack size.");
            }
        }

        /// <inheritdoc />
        public override void AddLocalLevel()
        {
            CopyStackContext context = new CopyStackContext(localLevel);
            context.WriteableVariables.SetUnknownIndex(VariableIndex.CreateUnknown(localLevel));
            context.WriteableControllVariables.SetUnknownIndex(ControlIndex.CreateUnknown(localLevel));

            memoryStack.Add(context);
            localLevel++;
        }

        #endregion

        #region Indexes

        /// <inheritdoc />
        public override IEnumerable<MemoryIndex> Indexes
        {
            get { return indexDefinitions.Keys; }
        }

        /// <inheritdoc />
        public override IEnumerable<KeyValuePair<MemoryIndex, IIndexDefinition>> IndexDefinitions
        {
            get { return indexDefinitions; }
        }

        /// <inheritdoc />
        public override bool IsDefined(MemoryIndex index)
        {
            return indexDefinitions.ContainsKey(index);
        }

        /// <inheritdoc />
        public override bool TryGetIndexDefinition(MemoryIndex index, out IIndexDefinition data)
        {
            return indexDefinitions.TryGetValue(index, out data);
        }

        /// <inheritdoc />
        public override IIndexDefinition GetIndexDefinition(MemoryIndex index)
        {
            IIndexDefinition data;
            if (indexDefinitions.TryGetValue(index, out data))
            {
                return data;
            }
            throw new Exception("Missing definition for " + index);
        }

        /// <inheritdoc />
        public override int GetNumberOfIndexes()
        {
            return indexDefinitions.Count();
        }

        /// <inheritdoc />
        public override void NewIndex(MemoryIndex index)
        {
            CopyIndexDefinition data = new CopyIndexDefinition();
            indexDefinitions.Add(index, data);
        }

        /// <inheritdoc />
        public override void RemoveIndex(MemoryIndex index)
        {
            indexDefinitions.Remove(index);
        }

        #endregion

        #region Objects

        /// <inheritdoc />
        public override IEnumerable<KeyValuePair<ObjectValue, IObjectDescriptor>> ObjectDescriptors
        {
            get { return objectDescriptors; }
        }

        /// <inheritdoc />
        public override IObjectDescriptor GetDescriptor(ObjectValue objectValue)
        {
            IObjectDescriptor descriptor;
            if (objectDescriptors.TryGetValue(objectValue, out descriptor))
            {
                return descriptor;
            }
            else
            {
                throw new Exception("Missing object descriptor");
            }
        }

        /// <inheritdoc />
        public override bool TryGetDescriptor(ObjectValue objectValue, out IObjectDescriptor descriptor)
        {
            return objectDescriptors.TryGetValue(objectValue, out descriptor);
        }

        /// <inheritdoc />
        public override bool HasObjects(MemoryIndex index)
        {
            IIndexDefinition data;
            if (indexDefinitions.TryGetValue(index, out data))
            {
                return data.Objects != null && data.Objects.Count > 0;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override IObjectValueContainer GetObjects(MemoryIndex index)
        {
            IIndexDefinition data;
            if (indexDefinitions.TryGetValue(index, out data))
            {
                if (data.Objects != null)
                {
                    return data.Objects;
                }
            }

            return new CopyObjectValueContainer();
        }

        /// <inheritdoc />
        public override void SetDescriptor(ObjectValue objectValue, IObjectDescriptor descriptor)
        {
            objectDescriptors[objectValue] = descriptor;
        }

        /// <inheritdoc />
        public override void SetObjects(MemoryIndex index, IObjectValueContainer objects)
        {
            IIndexDefinition data;
            if (!indexDefinitions.TryGetValue(index, out data))
            {
                data = new CopyIndexDefinition();
            }

            IIndexDefinitionBuilder builder = data.Builder(this);
            builder.SetObjects(objects);

            indexDefinitions[index] = builder.Build(this);
        }

        #endregion

        #region Arrays

        /// <inheritdoc />
        public override IEnumerable<KeyValuePair<AssociativeArray, IArrayDescriptor>> ArrayDescriptors
        {
            get { return arrayDescriptors; }
        }

        /// <inheritdoc />
        public override bool TryGetDescriptor(AssociativeArray arrayValue, out IArrayDescriptor descriptor)
        {
            return arrayDescriptors.TryGetValue(arrayValue, out descriptor);
        }

        /// <inheritdoc />
        public override IArrayDescriptor GetDescriptor(AssociativeArray arrayValue)
        {
            IArrayDescriptor descriptor;
            if (arrayDescriptors.TryGetValue(arrayValue, out descriptor))
            {
                return descriptor;
            }
            else
            {
                throw new Exception("Missing array descriptor " + arrayValue.ToString());
            }
        }

        /// <inheritdoc />
        public override AssociativeArray GetArray(MemoryIndex index)
        {
            IIndexDefinition data;
            if (indexDefinitions.TryGetValue(index, out data))
            {
                if (data.Array != null)
                {
                    return data.Array;
                }
            }

            throw new Exception("Missing array for index " + index);
        }

        /// <inheritdoc />
        public override bool HasArray(MemoryIndex index)
        {
            IIndexDefinition data;
            if (indexDefinitions.TryGetValue(index, out data))
            {
                if (data.Array != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public override bool TryGetArray(MemoryIndex index, out AssociativeArray arrayValue)
        {
            IIndexDefinition data;
            if (indexDefinitions.TryGetValue(index, out data))
            {
                if (data.Array != null)
                {
                    arrayValue = data.Array;
                    return true;
                }
            }

            arrayValue = null;
            return false;
        }

        /// <inheritdoc />
        public override bool TryGetCallArraySnapshot(AssociativeArray array, out IEnumerable<Snapshot> snapshots)
        {
            CopySet<Snapshot> snapshotSet = null;
            if (callArrays.TryGetValue(array, out snapshotSet))
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

        /// <inheritdoc />
        public override void SetDescriptor(AssociativeArray arrayvalue, IArrayDescriptor descriptor)
        {
            arrayDescriptors[arrayvalue] = descriptor;
        }

        /// <inheritdoc />
        public override void AddCallArray(AssociativeArray array, Snapshot snapshot)
        {
            CopySet<Snapshot> snapshots;
            if (!callArrays.TryGetValue(array, out snapshots))
            {
                snapshots = new CopySet<Snapshot>();
                callArrays[array] = snapshots;
            }

            snapshots.Add(snapshot);
        }

        /// <inheritdoc />
        public override void SetArray(MemoryIndex index, AssociativeArray arrayValue)
        {
            IIndexDefinition data;
            if (!indexDefinitions.TryGetValue(index, out data))
            {
                data = new CopyIndexDefinition();
            }

            IIndexDefinitionBuilder builder = data.Builder(this);
            builder.SetArray(arrayValue);

            indexDefinitions[index] = builder.Build(this);

            IArrayDescriptor descriptor;
            if (TryGetDescriptor(arrayValue, out descriptor))
            {
                if (descriptor.ParentIndex != null)
                {
                    GetWriteableStackContext(descriptor.ParentIndex.CallLevel).WriteableArrays.Remove(arrayValue);
                }
            }
            GetWriteableStackContext(index.CallLevel).WriteableArrays.Add(arrayValue);
        }

        /// <inheritdoc />
        public override void RemoveArray(MemoryIndex index, AssociativeArray arrayValue)
        {
            arrayDescriptors.Remove(arrayValue);

            IIndexDefinition data;
            if (!indexDefinitions.TryGetValue(index, out data))
            {
                data = new CopyIndexDefinition();
            }

            IIndexDefinitionBuilder builder = data.Builder(this);
            builder.SetArray(null);

            indexDefinitions[index] = builder.Build(this);
            GetWriteableStackContext(index.CallLevel).WriteableArrays.Remove(arrayValue);
        }

        #endregion

        #region Functions

        /// <inheritdoc />
        public override IEnumerable<QualifiedName> GetFunctions()
        {
            return functionDecl.GetNames();
        }

        /// <inheritdoc />
        public override bool IsFunctionDefined(QualifiedName functionName)
        {
            return functionDecl.Contains(functionName);
        }

        /// <inheritdoc />
        public override bool TryGetFunction(QualifiedName functionName, out IEnumerable<FunctionValue> functionValues)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override IEnumerable<FunctionValue> GetFunction(QualifiedName functionName)
        {
            return functionDecl.GetValue(functionName);
        }

        /// <inheritdoc />
        public override void AddFunctiondeclaration(QualifiedName name, FunctionValue declaration)
        {
            functionDecl.Add(name, declaration);
        }

        /// <inheritdoc />
        public override void SetFunctionDeclarations(QualifiedName name, IEnumerable<FunctionValue> declarations)
        {
            functionDecl.SetAll(name, declarations);
        }

        #endregion

        #region Classes

        /// <inheritdoc />
        public override IEnumerable<QualifiedName> GetClasses()
        {
            return classDecl.GetNames();
        }

        /// <inheritdoc />
        public override bool IsClassDefined(PHP.Core.QualifiedName className)
        {
            return classDecl.Contains(className);
        }

        /// <inheritdoc />
        public override bool TryGetClass(QualifiedName className, out IEnumerable<TypeValue> classValues)
        {
            return classDecl.TryGetValue(className, out classValues);
        }

        /// <inheritdoc />
        public override IEnumerable<TypeValue> GetClass(PHP.Core.QualifiedName className)
        {
            return classDecl.GetValue(className);
        }

        /// <inheritdoc />
        public override void AddClassDeclaration(PHP.Core.QualifiedName name, TypeValue declaration)
        {
            classDecl.Add(name, declaration);
        }

        /// <inheritdoc />
        public override void SetClassDeclarations(QualifiedName name, IEnumerable<TypeValue> declarations)
        {
            classDecl.SetAll(name, declarations);
        }

        #endregion

        #region Aliasses

        /// <inheritdoc />
        public override bool TryGetAliases(MemoryIndex index, out IMemoryAlias aliases)
        {
            IIndexDefinition data;
            if (indexDefinitions.TryGetValue(index, out data))
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

        /// <inheritdoc />
        public override IMemoryAlias GetAliases(MemoryIndex index)
        {
            IIndexDefinition data;
            if (indexDefinitions.TryGetValue(index, out data))
            {
                if (data.Aliases != null)
                {
                    return data.Aliases;
                }
            }
            throw new Exception("Missing alias value for " + index);
        }

        /// <inheritdoc />
        public override void SetAlias(MemoryIndex index, IMemoryAlias alias)
        {
            IIndexDefinition data;
            if (!indexDefinitions.TryGetValue(index, out data))
            {
                data = new CopyIndexDefinition();
            }

            IIndexDefinitionBuilder builder = data.Builder(this);
            builder.SetAliases(alias);

            indexDefinitions[index] = builder.Build(this);
        }

        /// <inheritdoc />
        public override void RemoveAlias(MemoryIndex index)
        {
            SetAlias(index, null);
        }

        #endregion

        public override void AddStackLevel(int level)
        {
            throw new NotImplementedException();
        }

        public override void SetLocalStackLevelNumber(int level)
        {
            throw new NotImplementedException();
        }
    }
}