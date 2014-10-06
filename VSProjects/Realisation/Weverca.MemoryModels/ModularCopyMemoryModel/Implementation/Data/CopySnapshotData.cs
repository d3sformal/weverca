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
    public class CopySnapshotDataFactory : ISnapshotDataFactory
    {
        /// <inheritdoc />
        public ISnapshotDataProxy CreateEmptyInstance(Snapshot snapshot)
        {
            return new CopySnapshotDataProxy(snapshot);
        }

        /// <inheritdoc />
        public ISnapshotDataProxy CopyInstance(Snapshot snapshot, ISnapshotDataProxy oldData)
        {
            CopySnapshotDataProxy proxy = CopySnapshotDataProxy.Convert(oldData);
            return new CopySnapshotDataProxy(snapshot, proxy);
        }
    }

    /// <summary>
    /// This instance of SnapshotData container uses the associative array to store whole inner data 
    /// structure. The container is always deeply copied when new instance is created.
    /// </summary>
    public class CopySnapshotDataProxy : ISnapshotDataProxy
    {
        private SnapshotDataAssociativeContainer snapshotData;

        /// <summary>
        /// Converts the specified proxy to CopySnapshotDataProxy type.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        /// <returns>Casted object to CopySnapshotDataProxy type.</returns>
        /// <exception cref="System.InvalidCastException">Argument is not of type CopySnapshotDataProxy</exception>
        public static CopySnapshotDataProxy Convert(ISnapshotDataProxy proxy)
        {
            CopySnapshotDataProxy converted = proxy as CopySnapshotDataProxy;
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
        public CopySnapshotDataProxy(Snapshot snapshot)
        {
            snapshotData = new SnapshotDataAssociativeContainer(snapshot);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopySnapshotDataProxy"/> class.
        /// Deeply copies given proxy instance.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="proxy">The proxy.</param>
        public CopySnapshotDataProxy(Snapshot snapshot, CopySnapshotDataProxy proxy)
        {
            snapshotData = proxy.snapshotData.Copy(snapshot);
        }

        /// <inheritdoc />
        public IReadOnlySnapshotData Readonly
        {
            get { return snapshotData; }
        }

        /// <inheritdoc />
        public IWriteableSnapshotData Writeable
        {
            get { return snapshotData; }
        }

        /// <inheritdoc />
        public bool IsReadonly
        {
            get { return false; }
        }
    }
}