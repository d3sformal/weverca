using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.CopyStructure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure
{
    /// <summary>
    /// Factory for <see cref="CopySnapshotStructureProxy"/> class.
    /// 
    /// This instance of SnapshotStructure container uses the associative arrays and copy semantics. Inner structure
    /// is always deeply copied no mather that structure will be modified or not.
    /// </summary>
    public class CopySnapshotStructureFactory : ISnapshotStructureFactory
    {
        /// <inheritdoc />
        public ISnapshotStructureProxy CreateEmptyInstance(Snapshot snapshot)
        {
            return CopySnapshotStructureProxy.CreateEmpty(snapshot);
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy CopyInstance(Snapshot snapshot, ISnapshotStructureProxy oldData)
        {
            CopySnapshotStructureProxy proxy = oldData as CopySnapshotStructureProxy;
            if (proxy != null)
            {
                return proxy.Copy(snapshot);
            }
            else
            {
                throw new InvalidCastException("Argument is not of type CopySnapshotStructureProxy");
            }
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy CreateGlobalContextInstance(Snapshot snapshot)
        {
            return CopySnapshotStructureProxy.CreateGlobal(snapshot);
        }
    }

    /// <summary>
    /// This instance of SnapshotStructure container uses the associative arrays and copy semantics. Inner structure
    /// is always deeply copied no mather that structure will be modified or not.
    /// </summary>
    public class CopySnapshotStructureProxy : ISnapshotStructureProxy
    {
        private SnapshotStructureContainer snapshotStructure;

        /// <summary>
        /// Creates the structure with memory stack with global level only.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New structure with memory stack with global level only.</returns>
        public static CopySnapshotStructureProxy CreateGlobal(Snapshot snapshot)
        {
            CopySnapshotStructureProxy proxy = new CopySnapshotStructureProxy();
            proxy.snapshotStructure = SnapshotStructureContainer.CreateGlobal(snapshot);
            return proxy;
        }

        /// <summary>
        /// Creates empty structure which contains no data in containers.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New empty structure which contains no data in containers.</returns>
        public static CopySnapshotStructureProxy CreateEmpty(Snapshot snapshot)
        {
            CopySnapshotStructureProxy proxy = new CopySnapshotStructureProxy();
            proxy.snapshotStructure = SnapshotStructureContainer.CreateEmpty(snapshot);
            return proxy;
        }

        /// <summary>
        /// Creates new structure object as copy of this structure which contains the same data as this instace.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New copy of this structure which contains the same data as this instace.</returns>
        public CopySnapshotStructureProxy Copy(Snapshot snapshot)
        {
            CopySnapshotStructureProxy proxy = new CopySnapshotStructureProxy();
            proxy.snapshotStructure = this.snapshotStructure.Copy(snapshot);
            return proxy;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="CopySnapshotStructureProxy"/> class from being created.
        /// </summary>
        private CopySnapshotStructureProxy()
        {

        }

        /// <inheritdoc />
        public bool Locked { get; set; }

        /// <inheritdoc />
        public IReadOnlySnapshotStructure Readonly
        {
            get { return snapshotStructure; }
        }

        /// <inheritdoc />
        public IWriteableSnapshotStructure Writeable
        {
            get 
            {
                if (!Locked)
                {
                    return snapshotStructure;
                }
                else
                {
                    throw new InvalidOperationException("Snapshod structure is locked in this mode");
                }
            }
        }

        /// <inheritdoc />
        public IObjectDescriptor CreateObjectDescriptor(ObjectValue createdObject, TypeValue type, MemoryIndex memoryIndex)
        {
            CopyObjectDescriptor descriptor = new CopyObjectDescriptor();
            descriptor.SetObjectValue(createdObject);
            descriptor.SetType(type);
            descriptor.SetUnknownIndex(memoryIndex);
            return descriptor;
        }

        /// <inheritdoc />
        public IArrayDescriptor CreateArrayDescriptor(AssociativeArray createdArray, MemoryIndex memoryIndex)
        {
            CopyArrayDescriptor descriptor = new CopyArrayDescriptor();
            descriptor.SetArrayValue(createdArray);
            descriptor.SetParentIndex(memoryIndex);
            descriptor.SetUnknownIndex(memoryIndex.CreateUnknownIndex());
            return descriptor;
        }

        /// <inheritdoc />
        public IMemoryAlias CreateMemoryAlias(Memory.MemoryIndex index)
        {
            CopyMemoryAlias aliases = new CopyMemoryAlias();
            aliases.SetSourceIndex(index);
            return aliases;
        }

        /// <inheritdoc />
        public IObjectValueContainer CreateObjectValueContainer(IEnumerable<ObjectValue> objects)
        {
            CopyObjectValueContainer container = new CopyObjectValueContainer();
            container.AddAll(objects);
            return container;
        }

        /// <inheritdoc />
        public IIndexDefinition CreateIndexDefinition()
        {
            return new CopyIndexDefinition();
        }
    }
}
