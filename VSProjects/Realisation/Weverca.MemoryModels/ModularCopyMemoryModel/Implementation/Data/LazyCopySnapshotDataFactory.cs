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
