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
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyCopyStructure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.TrackingStructure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure
{
    /// <summary>
    /// Factory for <see cref="TrackingSnapshotStructureProxy"/> class.
    /// 
    /// This instance of SnapshotStructure container uses the associative arrays and copy semantics. Inner structure
    /// is copied only when structure will be modified.
    /// </summary>
    public class TrackingSnapshotStructureFactory : ISnapshotStructureFactory
    {
        private TrackingSnapshotStructureProxy emptyProxyInstance;

        public TrackingSnapshotStructureFactory()
        {
            emptyProxyInstance = TrackingSnapshotStructureProxy.CreateEmpty(null);
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy CreateEmptyInstance(Snapshot snapshot)
        {
            return emptyProxyInstance.Copy(snapshot);
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy CopyInstance(Snapshot snapshot, ISnapshotStructureProxy oldData)
        {
            TrackingSnapshotStructureProxy proxy = oldData as TrackingSnapshotStructureProxy;
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
            return TrackingSnapshotStructureProxy.CreateGlobal(snapshot);
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy CreateNewInstanceWithData(Snapshot snapshot, IReadOnlySnapshotStructure oldData)
        {
            TrackingSnapshotStructureContainer data = oldData as TrackingSnapshotStructureContainer;
            if (data != null)
            {
                return TrackingSnapshotStructureProxy.CreateWithData(snapshot, data);
            }
            else
            {
                throw new InvalidCastException("Argument is not of type TrackingSnapshotStructureContainer");
            }
        }
    }

    /// <summary>
    /// This instance of SnapshotStructure container uses the associative arrays and copy semantics. Inner structure
    /// is copied only when structure will be modified.
    /// </summary>
    public class TrackingSnapshotStructureProxy : ISnapshotStructureProxy
    {
        private TrackingSnapshotStructureContainer readonlyInstance;
        private TrackingSnapshotStructureContainer snapshotStructure;
        private bool isReadonly = true;
        private Snapshot snapshot;

        /// <summary>
        /// Creates the structure with memory stack with global level only.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New structure with memory stack with global level only.</returns>
        public static TrackingSnapshotStructureProxy CreateGlobal(Snapshot snapshot)
        {
            TrackingSnapshotStructureProxy proxy = new TrackingSnapshotStructureProxy();
            proxy.snapshotStructure = TrackingSnapshotStructureContainer.CreateEmpty(snapshot, proxy);
            proxy.isReadonly = false;
            proxy.snapshot = snapshot;

            proxy.snapshotStructure.AddStackLevel(Snapshot.GLOBAL_CALL_LEVEL);
            proxy.snapshotStructure.SetLocalStackLevelNumber(Snapshot.GLOBAL_CALL_LEVEL);

            proxy.readonlyInstance = proxy.snapshotStructure;
            return proxy;
        }

        /// <summary>
        /// Creates empty structure which contains no data in containers.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New empty structure which contains no data in containers.</returns>
        public static TrackingSnapshotStructureProxy CreateEmpty(Snapshot snapshot)
        {
            TrackingSnapshotStructureProxy proxy = new TrackingSnapshotStructureProxy();
            proxy.snapshotStructure = TrackingSnapshotStructureContainer.CreateEmpty(snapshot, proxy);
            proxy.readonlyInstance = proxy.snapshotStructure;
            proxy.isReadonly = false;
            proxy.snapshot = snapshot;
            return proxy;
        }

        /// <summary>
        /// Creates new structure object as copy of this structure which contains the same data as this instace.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New copy of this structure which contains the same data as this instace.</returns>
        public TrackingSnapshotStructureProxy Copy(Snapshot snapshot)
        {
            TrackingSnapshotStructureProxy proxy = new TrackingSnapshotStructureProxy();
            proxy.readonlyInstance = this.readonlyInstance;
            proxy.snapshot = snapshot;
            return proxy;
        }

        /// <summary>
        /// Creates new structure object with copy of diven data object.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="data">The old data.</param>
        /// <returns>New structure object with copy of diven data object.</returns>
        public static ISnapshotStructureProxy CreateWithData(Snapshot snapshot, TrackingSnapshotStructureContainer data)
        {
            TrackingSnapshotStructureProxy proxy = new TrackingSnapshotStructureProxy();
            proxy.readonlyInstance = data;
            proxy.snapshot = snapshot;
            return proxy;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="LazyCopySnapshotStructure"/> class from being created.
        /// </summary>
        private TrackingSnapshotStructureProxy()
        {

        }

        /// <inheritdoc />
        public bool Locked { get; set; }

        /// <inheritdoc />
        public IReadOnlySnapshotStructure Readonly
        {
            get { return readonlyInstance; }
        }

        /// <inheritdoc />
        public IWriteableSnapshotStructure Writeable
        {
            get
            {
                if (!Locked)
                {
                    if (isReadonly)
                    {
                        snapshotStructure = readonlyInstance.Copy(snapshot, this);
                        readonlyInstance = snapshotStructure;
                        isReadonly = false;
                    }
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
            get { return isReadonly; }
        }

        /// <inheritdoc />
        public IObjectDescriptor CreateObjectDescriptor(ObjectValue createdObject, TypeValue type, MemoryIndex memoryIndex)
        {
            LazyCopyObjectDescriptor descriptor = new LazyCopyObjectDescriptor(snapshotStructure);
            descriptor.SetObjectValue(createdObject);
            descriptor.SetType(type);
            descriptor.SetUnknownIndex(memoryIndex);
            return descriptor;
        }

        /// <inheritdoc />
        public IArrayDescriptor CreateArrayDescriptor(AssociativeArray createdArray, MemoryIndex memoryIndex)
        {
            LazyCopyArrayDescriptor descriptor = new LazyCopyArrayDescriptor(snapshotStructure);
            descriptor.SetArrayValue(createdArray);
            descriptor.SetParentIndex(memoryIndex);
            descriptor.SetUnknownIndex(memoryIndex.CreateUnknownIndex());
            return descriptor;
        }

        /// <inheritdoc />
        public IMemoryAlias CreateMemoryAlias(Memory.MemoryIndex index)
        {
            LazyCopyMemoryAlias aliases = new LazyCopyMemoryAlias(snapshotStructure);
            aliases.SetSourceIndex(index);
            return aliases;
        }

        /// <inheritdoc />
        public IObjectValueContainer CreateObjectValueContainer(IEnumerable<ObjectValue> objects)
        {
            LazyCopyObjectValueContainer container = new LazyCopyObjectValueContainer(snapshotStructure);
            container.AddAll(objects);
            return container;
        }

        /// <inheritdoc />
        public IIndexDefinition CreateIndexDefinition()
        {
            return new LazyCopyIndexDefinition(snapshotStructure);
        }


        public IWriteableStackContext CreateWriteableStackContext(int level)
        {
            return new LazyCopyStackContext(level);
        }

        public IObjectValueContainer CreateObjectValueContainer()
        {
            return new LazyCopyObjectValueContainer(snapshotStructure);
        }
    }
}