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
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyStructure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.TrackingStructure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure
{
    /// <summary>
    /// Factory for <see cref="TrackingSnapshotStructureProxy"/> class.
    /// 
    /// This instance of SnapshotStructure container uses the associative arrays and copy semantics. Inner structure
    /// is copied only when structure will be modified. Uses change tracker to keep information about changed 
    /// indexes and declarations.
    /// </summary>
    public class TrackingSnapshotStructureFactory : ISnapshotStructureFactory
    {
        /// <inheritdoc />
        public ISnapshotStructureProxy CreateEmptyInstance(ModularMemoryModelFactories factories)
        {
            return TrackingSnapshotStructureProxy.CreateEmpty(factories);
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy CopyInstance(ISnapshotStructureProxy oldData)
        {
            TrackingSnapshotStructureProxy proxy = oldData as TrackingSnapshotStructureProxy;
            if (proxy != null)
            {
                return proxy.Copy();
            }
            else
            {
                throw new InvalidCastException("Argument is not of type CopySnapshotStructureProxy");
            }
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy CreateGlobalContextInstance(ModularMemoryModelFactories factories)
        {
            return TrackingSnapshotStructureProxy.CreateGlobal(factories);
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy CreateNewInstanceWithData(IReadOnlySnapshotStructure oldData)
        {
            TrackingSnapshotStructureContainer data = oldData as TrackingSnapshotStructureContainer;
            if (data != null)
            {
                return TrackingSnapshotStructureProxy.CreateWithData(data);
            }
            else
            {
                throw new InvalidCastException("Argument is not of type TrackingSnapshotStructureContainer");
            }
        }
    }

    /// <summary>
    /// This instance of SnapshotStructure container uses the associative arrays and copy semantics. Inner structure
    /// is copied only when structure will be modified. Uses change tracker to keep information about changed 
    /// indexes and declarations.
    /// </summary>
    public class TrackingSnapshotStructureProxy : ISnapshotStructureProxy
    {
        private ModularMemoryModelFactories factories;

        private TrackingSnapshotStructureContainer readonlyInstance;
        private TrackingSnapshotStructureContainer snapshotStructure;
        private bool isReadonly = true;

        /// <summary>
        /// Creates the structure with memory stack with global level only.
        /// </summary>
        /// <param name="factories">The factories.</param>
        /// <returns>
        /// New structure with memory stack with global level only.
        /// </returns>
        public static TrackingSnapshotStructureProxy CreateGlobal(ModularMemoryModelFactories factories)
        {
            TrackingSnapshotStructureProxy proxy = new TrackingSnapshotStructureProxy(factories);
            proxy.snapshotStructure = TrackingSnapshotStructureContainer.CreateEmpty(factories);
            proxy.isReadonly = false;

            proxy.snapshotStructure.AddStackLevel(Snapshot.GLOBAL_CALL_LEVEL);
            proxy.snapshotStructure.SetLocalStackLevelNumber(Snapshot.GLOBAL_CALL_LEVEL);

            proxy.readonlyInstance = proxy.snapshotStructure;
            return proxy;
        }

        /// <summary>
        /// Creates empty structure which contains no data in containers.
        /// </summary>
        /// <param name="factories">The factories.</param>
        /// <returns>
        /// New empty structure which contains no data in containers.
        /// </returns>
        public static TrackingSnapshotStructureProxy CreateEmpty(ModularMemoryModelFactories factories)
        {
            TrackingSnapshotStructureProxy proxy = new TrackingSnapshotStructureProxy(factories);
            proxy.snapshotStructure = TrackingSnapshotStructureContainer.CreateEmpty(factories);
            proxy.readonlyInstance = proxy.snapshotStructure;
            proxy.isReadonly = false;
            return proxy;
        }

        /// <summary>
        /// Creates new structure object as copy of this structure which contains the same data as this instace.
        /// </summary>
        /// <returns>
        /// New copy of this structure which contains the same data as this instace.
        /// </returns>
        public TrackingSnapshotStructureProxy Copy()
        {
            TrackingSnapshotStructureProxy proxy = new TrackingSnapshotStructureProxy(factories);
            proxy.readonlyInstance = this.readonlyInstance;
            return proxy;
        }

        /// <summary>
        /// Creates new structure object with copy of diven data object.
        /// </summary>
        /// <param name="data">The old data.</param>
        /// <returns>
        /// New structure object with copy of diven data object.
        /// </returns>
        public static ISnapshotStructureProxy CreateWithData(TrackingSnapshotStructureContainer data)
        {
            TrackingSnapshotStructureProxy proxy = new TrackingSnapshotStructureProxy(data.Factories);
            proxy.readonlyInstance = data;
            return proxy;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="LazyCopySnapshotStructureProxy"/> class from being created.
        /// </summary>
        private TrackingSnapshotStructureProxy(ModularMemoryModelFactories factories)
        {
            this.factories = factories;
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
                        snapshotStructure = readonlyInstance.Copy();
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
    }
}