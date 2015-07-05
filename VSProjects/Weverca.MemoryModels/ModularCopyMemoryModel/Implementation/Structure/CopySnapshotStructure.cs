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
        public ISnapshotStructureProxy CreateEmptyInstance(ModularMemoryModelFactories factories, Snapshot snapshot)
        {
            return CopySnapshotStructureProxy.CreateEmpty(factories, snapshot);
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy CopyInstance(ModularMemoryModelFactories factories, Snapshot snapshot, ISnapshotStructureProxy oldData)
        {
            CopySnapshotStructureProxy proxy = oldData as CopySnapshotStructureProxy;
            if (proxy != null)
            {
                return proxy.Copy(factories, snapshot);
            }
            else
            {
                throw new InvalidCastException("Argument is not of type CopySnapshotStructureProxy");
            }
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy CreateNewInstanceWithData(ModularMemoryModelFactories factories, Snapshot snapshot, IReadOnlySnapshotStructure oldData)
        {
            SnapshotStructureContainer data = oldData as SnapshotStructureContainer;
            if (data != null)
            {
                return CopySnapshotStructureProxy.CreateWithData(factories, snapshot, data);
            }
            else
            {
                throw new InvalidCastException("Argument is not of type SnapshotStructureContainer");
            }
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy CreateGlobalContextInstance(ModularMemoryModelFactories factories, Snapshot snapshot)
        {
            return CopySnapshotStructureProxy.CreateGlobal(factories, snapshot);
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
        public static CopySnapshotStructureProxy CreateGlobal(ModularMemoryModelFactories factories, Snapshot snapshot)
        {
            CopySnapshotStructureProxy proxy = new CopySnapshotStructureProxy();
            proxy.snapshotStructure = SnapshotStructureContainer.CreateGlobal(factories, snapshot, proxy);
            return proxy;
        }

        /// <summary>
        /// Creates empty structure which contains no data in containers.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New empty structure which contains no data in containers.</returns>
        public static CopySnapshotStructureProxy CreateEmpty(ModularMemoryModelFactories factories, Snapshot snapshot)
        {
            CopySnapshotStructureProxy proxy = new CopySnapshotStructureProxy();
            proxy.snapshotStructure = SnapshotStructureContainer.CreateEmpty(factories, snapshot, proxy);
            return proxy;
        }

        /// <summary>
        /// Creates new structure object as copy of this structure which contains the same data as this instace.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New copy of this structure which contains the same data as this instace.</returns>
        public CopySnapshotStructureProxy Copy(ModularMemoryModelFactories factories, Snapshot snapshot)
        {
            CopySnapshotStructureProxy proxy = new CopySnapshotStructureProxy();
            proxy.snapshotStructure = this.snapshotStructure.Copy(factories, snapshot, proxy);
            return proxy;
        }

        /// <summary>
        /// Creates new structure object with copy of diven data object.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="data">The old data.</param>
        /// <returns>New structure object with copy of diven data object.</returns>
        public static ISnapshotStructureProxy CreateWithData(ModularMemoryModelFactories factories, Snapshot snapshot, SnapshotStructureContainer data)
        {
            CopySnapshotStructureProxy proxy = new CopySnapshotStructureProxy();
            proxy.snapshotStructure = data.Copy(factories, snapshot, proxy);
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
        public bool IsReadonly
        {
            get { return false; }
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


        public IWriteableStackContext CreateWriteableStackContext(int level)
        {
            return new CopyStackContext(level);
        }

        public IObjectValueContainer CreateObjectValueContainer()
        {
            return new CopyObjectValueContainer();
        }
    }
}