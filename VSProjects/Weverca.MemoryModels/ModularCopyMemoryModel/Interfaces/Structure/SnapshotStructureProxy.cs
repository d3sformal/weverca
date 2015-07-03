using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    /// <summary>
    /// Proxy object for snapshot structure container. This object is used to distinguish readonly or
    /// writeable access to structure container.
    /// </summary>
    public interface ISnapshotStructureProxy
    {
        /// <summary>
        /// Gets or sets a value indicating whether structural changes are allowed or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if structural changes are forbiden; otherwise, <c>false</c>.
        /// </value>
        bool Locked { get; set; }

        /// <summary>
        /// Gets the snasphot structure container for read only access.
        /// </summary>
        /// <value>
        /// The read only snapshot structure.
        /// </value>
        IReadOnlySnapshotStructure Readonly { get; }

        /// <summary>
        /// Gets the snapshot structure container for access which allows modifications.
        /// </summary>
        /// <value>
        /// The writeable snapshot structure.
        /// </value>
        IWriteableSnapshotStructure Writeable { get; }

        /// <summary>
        /// Gets a value indicating whether this instance was used only in readonly mode or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance was used only in readonly mode; otherwise, <c>false</c>.
        /// </value>
        bool IsReadonly { get; }

        /// <summary>
        /// Creates the new instance of object descriptor to store object definition in structure.
        /// </summary>
        /// <param name="createdObject">The created object.</param>
        /// <param name="type">The type of object.</param>
        /// <param name="memoryIndex">The memory location of object.</param>
        /// <returns>Created object descriptor instance.</returns>
        IObjectDescriptor CreateObjectDescriptor(ObjectValue createdObject, TypeValue type, MemoryIndex memoryIndex);

        /// <summary>
        /// Creates the new instance of array descriptor to store array definition in structure.
        /// </summary>
        /// <param name="createdArray">The created array.</param>
        /// <param name="memoryIndex">The memory location of array.</param>
        /// <returns>Created array descriptor instance.</returns>
        IArrayDescriptor CreateArrayDescriptor(AssociativeArray createdArray, MemoryIndex memoryIndex);

        /// <summary>
        /// Creates the new instance of memory alias object to store alias definition in this structure.
        /// </summary>
        /// <param name="index">The memory index collection is created for.</param>
        /// <returns>Created alias collection.</returns>
        IMemoryAlias CreateMemoryAlias(MemoryIndex index);

        /// <summary>
        /// Creates the new instance of object container to store object values for memory location in this structure.
        /// </summary>
        /// <param name="objects">The objects to store in collection.</param>
        /// <returns>Created object container.</returns>
        IObjectValueContainer CreateObjectValueContainer(IEnumerable<ObjectValue> objects);

        /// <summary>
        /// Creates the new instance of object container to store alias, array and object data for memory indexes.
        /// </summary>
        /// <returns>New instance of index definition object.</returns>
        IIndexDefinition CreateIndexDefinition();

        /// <summary>
        /// Creates the writeable stack context.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        IWriteableStackContext CreateWriteableStackContext(int level);

        /// <summary>
        /// Creates the object value container.
        /// </summary>
        /// <returns></returns>
        IObjectValueContainer CreateObjectValueContainer();
    }
}
