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
    /// structure. The container is deeply copied only when structure will be modified.
    /// </summary>
    public class LazyCopySnapshotDataFactory : ISnapshotDataFactory
    {
        /// <inheritdoc />
        public ISnapshotDataProxy CreateEmptyInstance(Snapshot snapshot)
        {
            return new LazyCopySnapshotDataProxy(snapshot);
        }

        /// <inheritdoc />
        public ISnapshotDataProxy CopyInstance(Snapshot snapshot, ISnapshotDataProxy oldData)
        {
            LazyCopySnapshotDataProxy proxy = LazyCopySnapshotDataProxy.Convert(oldData);
            return new LazyCopySnapshotDataProxy(snapshot, proxy);
        }

        /// <inheritdoc />
        public ISnapshotDataProxy CreateNewInstanceWithData(Snapshot snapshot, IReadOnlySnapshotData oldData)
        {
            SnapshotDataAssociativeContainer data = oldData as SnapshotDataAssociativeContainer;
            if (data != null)
            {
                return new LazyCopySnapshotDataProxy(snapshot, data);
            }
            else
            {
                throw new InvalidCastException("Argument is not of type SnapshotDataAssociativeContainer");
            }
        }
    }

    /// <summary>
    /// This instance of SnapshotData container uses the associative array to store whole inner data 
    /// structure. The container is deeply copied only when structure will be modified.
    /// </summary>
    public class LazyCopySnapshotDataProxy : ISnapshotDataProxy
    {
        private SnapshotDataAssociativeContainer snapshotData;
        private SnapshotDataAssociativeContainer readonlyInstance;
        private bool isReadonly = true;
        private Snapshot snapshot;

        /// <summary>
        /// Converts the specified proxy to LazyCopySnapshotDataProxy type.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        /// <returns>Casted object to LazyCopySnapshotDataProxy type.</returns>
        /// <exception cref="System.InvalidCastException">Argument is not of type CopySnapshotDataProxy</exception>
        public static LazyCopySnapshotDataProxy Convert(ISnapshotDataProxy proxy)
        {
            LazyCopySnapshotDataProxy converted = proxy as LazyCopySnapshotDataProxy;
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
        /// Initializes a new instance of the <see cref="LazyCopySnapshotDataProxy"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        public LazyCopySnapshotDataProxy(Snapshot snapshot)
        {
            snapshotData = new SnapshotDataAssociativeContainer(snapshot);
            this.snapshot = snapshot;
            isReadonly = false;
            readonlyInstance = snapshotData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyCopySnapshotDataProxy"/> class.
        /// Uses given data instance as readonly source of data.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="oldData">The old data.</param>
        public LazyCopySnapshotDataProxy(Snapshot snapshot, SnapshotDataAssociativeContainer oldData)
        {
            readonlyInstance = oldData;
            this.snapshot = snapshot;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyCopySnapshotDataProxy"/> class.
        /// Deeply copies given proxy instance.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="proxy">The proxy.</param>
        public LazyCopySnapshotDataProxy(Snapshot snapshot, LazyCopySnapshotDataProxy proxy)
        {
            readonlyInstance = proxy.readonlyInstance;
            this.snapshot = snapshot;
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
                    snapshotData = readonlyInstance.Copy(snapshot);
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