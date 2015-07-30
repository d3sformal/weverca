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
    /// is copied only when structure will be modified.
    /// </summary>
    public class LazyCopySnapshotStructureFactory : ISnapshotStructureFactory
    {
        /// <inheritdoc />
        public ISnapshotStructureProxy CreateEmptyInstance(ModularMemoryModelFactories factories)
        {
            return LazyCopySnapshotStructureProxy.CreateEmpty(factories);
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy CopyInstance(ISnapshotStructureProxy oldData)
        {
            LazyCopySnapshotStructureProxy proxy = oldData as LazyCopySnapshotStructureProxy;
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
        public ISnapshotStructureProxy CreateNewInstanceWithData(IReadOnlySnapshotStructure oldData)
        {
            SnapshotStructureContainer data = oldData as SnapshotStructureContainer;
            if (data != null)
            {
                return LazyCopySnapshotStructureProxy.CreateWithData(data);
            }
            else
            {
                throw new InvalidCastException("Argument is not of type SnapshotStructureContainer");
            }
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy CreateGlobalContextInstance(ModularMemoryModelFactories factories)
        {
            return LazyCopySnapshotStructureProxy.CreateGlobal(factories);
        }
    }

    /// <summary>
    /// This instance of SnapshotStructure container uses the associative arrays and copy semantics. Inner structure
    /// is copied only when structure will be modified.
    /// </summary>
    public class LazyCopySnapshotStructureProxy : ISnapshotStructureProxy
    {
        private ModularMemoryModelFactories factories;

        private SnapshotStructureContainer readonlyInstance;
        private SnapshotStructureContainer snapshotStructure;
        private bool isReadonly = true;

        /// <summary>
        /// Creates the structure with memory stack with global level only.
        /// </summary>
        /// <param name="factories">The factories.</param>
        /// <returns>
        /// New structure with memory stack with global level only.
        /// </returns>
        public static LazyCopySnapshotStructureProxy CreateGlobal(ModularMemoryModelFactories factories)
        {
            LazyCopySnapshotStructureProxy proxy = new LazyCopySnapshotStructureProxy(factories);
            proxy.snapshotStructure = SnapshotStructureContainer.CreateGlobal(factories);
            proxy.readonlyInstance = proxy.snapshotStructure;
            proxy.isReadonly = false;
            return proxy;
        }

        /// <summary>
        /// Creates empty structure which contains no data in containers.
        /// </summary>
        /// <param name="factories">The factories.</param>
        /// <returns>
        /// New empty structure which contains no data in containers.
        /// </returns>
        public static LazyCopySnapshotStructureProxy CreateEmpty(ModularMemoryModelFactories factories)
        {
            LazyCopySnapshotStructureProxy proxy = new LazyCopySnapshotStructureProxy(factories);
            proxy.snapshotStructure = SnapshotStructureContainer.CreateEmpty(factories);
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
        public LazyCopySnapshotStructureProxy Copy()
        {
            LazyCopySnapshotStructureProxy proxy = new LazyCopySnapshotStructureProxy(factories);
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
        public static ISnapshotStructureProxy CreateWithData(SnapshotStructureContainer data)
        {
            LazyCopySnapshotStructureProxy proxy = new LazyCopySnapshotStructureProxy(data.Factories);
            proxy.readonlyInstance = data;
            return proxy;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="LazyCopySnapshotStructureProxy"/> class from being created.
        /// </summary>
        private LazyCopySnapshotStructureProxy(ModularMemoryModelFactories factories)
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