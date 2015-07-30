using PHP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common
{
    /// <summary>
    /// Specifies the type of the connection between tracked container and its flow parent.
    /// </summary>
    public enum TrackerConnectionType
    {
        /// <summary>
        /// Data from parent snapshot was given from the single source - extend operation.
        /// </summary>
        EXTEND, 

        /// <summary>
        /// Data from parent snapshot was given from the single source at the beginning of 
        /// an function - extend as call operation.
        /// </summary>
        CALL_EXTEND,
 
        /// <summary>
        /// Data from parent snapshot was given from the multiple sources - merge operation.
        /// </summary>
        MERGE,

        /// <summary>
        /// Data from parent snapshot was given from the merge at the entry point of 
        /// an function - merge at subprogram entry
        /// </summary>
        SUBPROGRAM_MERGE,


        /// <summary>
        /// Data from parent snapshot was given from the merge at the end of 
        /// an function - merge with call
        /// </summary>
        CALL_MERGE
    }

    /// <summary>
    /// Specifies readonly operations for a change tracker implementation.
    /// 
    /// Change stracker is an component of memory model which allows to hold collection
    /// of changes which were made during the transaction. Tracker instance is connected
    /// with an structure or an data contaier. The connected container is able to store all
    /// changed memory indexes and declarations.
    /// </summary>
    /// <typeparam name="C">Type of the container to track changes in</typeparam>
    public interface IReadonlyChangeTracker<C>
        where C : class
    {
        /// <summary>
        /// Gets the type of the connection between tracked container and its flow parent.
        /// </summary>
        /// <value>
        /// The type of the connection.
        /// </value>
        TrackerConnectionType ConnectionType { get; }

        /// <summary>
        /// Gets the tracker identifier.
        /// </summary>
        /// <value>
        /// The tracker identifier.
        /// </value>
        int TrackerId { get; }

        /// <summary>
        /// Gets the call level.
        /// </summary>
        /// <value>
        /// The call level.
        /// </value>
        int CallLevel { get; }

        /// <summary>
        /// Gets the container connected with this tracker instance.
        /// </summary>
        /// <value>
        /// The container.
        /// </value>
        C Container { get; }

        /// <summary>
        /// Gets the link to the previous tracker. All changes within this tracker are 
        /// due to the previous one.
        /// </summary>
        /// <value>
        /// The previous tracker.
        /// </value>
        IReadonlyChangeTracker<C> PreviousTracker { get; }

        /// <summary>
        /// Gets the list of changed indexes.
        /// </summary>
        /// <value>
        /// The list of changed indexes.
        /// </value>
        IEnumerable<MemoryIndex> IndexChanges { get; }

        /// <summary>
        /// Gets list of changed functions.
        /// </summary>
        /// <value>
        /// The list of changed functions.
        /// </value>
        IEnumerable<QualifiedName> FunctionChanges { get; }

        /// <summary>
        /// Gets the list of changed classes.
        /// </summary>
        /// <value>
        /// The list of changed classes.
        /// </value>
        IEnumerable<QualifiedName> ClassChanges { get; }

        /// <summary>
        /// Gets the previous tracker registered for given call snapshot. This function is to find 
        /// correct tracker at the beginning of shared function.
        /// </summary>
        /// <param name="callSnapshot">The call snapshot.</param>
        /// <param name="callTracker">The call tracker.</param>
        /// <returns>True if tracker contains call tracker for given snapshot; otherwise false</returns>
        bool TryGetCallTracker(Snapshot callSnapshot, out IReadonlyChangeTracker<C> callTracker);
    }

    /// <summary>
    /// Specifies writeable operations for a change tracker implementation.
    /// </summary>
    /// <typeparam name="C">Type of the container to track changes in</typeparam>
    public interface IWriteableChangeTracker<C> : IReadonlyChangeTracker<C>
        where C : class
    {
        /// <summary>
        /// Sets the call level.
        /// </summary>
        /// <param name="callLevel">The call level.</param>
        void SetCallLevel(int callLevel);

        /// <summary>
        /// Sets the type of the connection.
        /// </summary>
        /// <param name="connectionType">Type of the connection.</param>
        void SetConnectionType(TrackerConnectionType connectionType);

        /// <summary>
        /// Stores an information that give index was inserted.
        /// </summary>
        /// <param name="index">The index.</param>
        void InsertedIndex(MemoryIndex index);

        /// <summary>
        /// Stores an information that give index was deleted.
        /// </summary>
        /// <param name="index">The index.</param>
        void DeletedIndex(MemoryIndex index);

        /// <summary>
        /// Stores an information that give index was modified.
        /// </summary>
        /// <param name="index">The index.</param>
        void ModifiedIndex(MemoryIndex index);

        /// <summary>
        /// Removes the information that the index was changed.
        /// </summary>
        /// <param name="index">The index.</param>
        void RemoveIndexChange(MemoryIndex index);

        /// <summary>
        /// Stores an information that give function was modified.
        /// </summary>
        /// <param name="function">The function.</param>
        void ModifiedFunction(QualifiedName function);

        /// <summary>
        /// Stores an information that give class was modified.
        /// </summary>
        /// <param name="function">The function.</param>
        void ModifiedClass(QualifiedName function);

        /// <summary>
        /// Removes the information that the function was changed.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        void RemoveFunctionChange(QualifiedName functionName);

        /// <summary>
        /// Removes the information that the class was changed.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        void RemoveClassChange(QualifiedName className);

        /// <summary>
        /// Registers given tracker as call tracker for given snapshot.
        /// </summary>
        /// <param name="callSnapshot">The call snapshot.</param>
        /// <param name="callTracker">The call tracker.</param>
        void AddCallTracker(Snapshot callSnapshot, IReadonlyChangeTracker<C> callTracker);
    }
}
