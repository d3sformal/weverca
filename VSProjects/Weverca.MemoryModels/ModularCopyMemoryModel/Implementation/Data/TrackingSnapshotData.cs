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
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Data
{
    /// <summary>
    /// Factory for <see cref="CopySnapshotDataProxy"/> class.
    /// 
    /// This instance of SnapshotData container uses the associative array to store whole inner data 
    /// structure. The container is always deeply copied when new instance is created.
    /// </summary>
    public class TrackingSnapshotDataFactory : ISnapshotDataFactory
    {
        /// <inheritdoc />
        public ISnapshotDataProxy CreateEmptyInstance(ModularMemoryModelFactories factories)
        {
            return new TrackingSnapshotDataProxy();
        }

        /// <inheritdoc />
        public ISnapshotDataProxy CopyInstance(ISnapshotDataProxy oldData)
        {
            TrackingSnapshotDataProxy proxy = TrackingSnapshotDataProxy.Convert(oldData);
            return new TrackingSnapshotDataProxy(proxy);
        }

        /// <inheritdoc />
        public ISnapshotDataProxy CreateNewInstanceWithData(IReadOnlySnapshotData oldData)
        {
            TrackingSnapshotDataAssociativeContainer data = oldData as TrackingSnapshotDataAssociativeContainer;
            if (data != null)
            {
                return new TrackingSnapshotDataProxy(data);
            }
            else
            {
                throw new InvalidCastException("Argument is not of type TrackingSnapshotDataAssociativeContainer");
            }
        }
    }

    /// <summary>
    /// This instance of SnapshotData container uses the associative array to store whole inner data 
    /// structure. The container is always deeply copied when new instance is created.
    /// </summary>
    public class TrackingSnapshotDataProxy : ISnapshotDataProxy
    {
        private TrackingSnapshotDataAssociativeContainer snapshotData;
        private TrackingSnapshotDataAssociativeContainer readonlyInstance;
        private bool isReadonly = true;

        /// <summary>
        /// Converts the specified proxy to CopySnapshotDataProxy type.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        /// <returns>Casted object to CopySnapshotDataProxy type.</returns>
        /// <exception cref="System.InvalidCastException">Argument is not of type CopySnapshotDataProxy</exception>
        public static TrackingSnapshotDataProxy Convert(ISnapshotDataProxy proxy)
        {
            TrackingSnapshotDataProxy converted = proxy as TrackingSnapshotDataProxy;
            if (converted != null)
            {
                return converted;
            }
            else
            {
                throw new InvalidCastException("Argument is not of type CopySnapshotDataProxy");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopySnapshotDataProxy"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        public TrackingSnapshotDataProxy()
        {
            snapshotData = new TrackingSnapshotDataAssociativeContainer();
            isReadonly = false;
            readonlyInstance = snapshotData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopySnapshotDataProxy"/> class.
        /// Uses given data instance as readonly source of data.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="oldData">The old data.</param>
        public TrackingSnapshotDataProxy(TrackingSnapshotDataAssociativeContainer oldData)
        {
            readonlyInstance = oldData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopySnapshotDataProxy"/> class.
        /// Deeply copies given proxy instance.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="proxy">The proxy.</param>
        public TrackingSnapshotDataProxy(TrackingSnapshotDataProxy proxy)
        {
            readonlyInstance = proxy.readonlyInstance;
        }

        /// <inheritdoc />
        public IReadOnlySnapshotData Readonly
        {
            get { return readonlyInstance; }
        }

        /// <inheritdoc />
        public IWriteableSnapshotData Writeable
        {
            get
            {
                if (isReadonly)
                {
                    snapshotData = readonlyInstance.Copy();
                    readonlyInstance = snapshotData;
                    isReadonly = false;
                }

                return snapshotData;
            }
        }

        /// <inheritdoc />
        public bool IsReadonly
        {
            get { return isReadonly; }
        }
    }
}