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
    /// Represents single level of memory stack with collections of variables and defined arrays.
    /// This level is part of memory stack an allows to separate global and local memory contexts.
    /// 
    /// This interface contains readonly items.
    /// </summary>
    public interface IReadonlyStackContext
    {
        /// <summary>
        /// Gets the readonly collection with associative container of definition of variables.
        /// </summary>
        /// <value>
        /// The readonly collection of variables variables.
        /// </value>
        IReadonlyIndexContainer ReadonlyVariables { get; }

        /// <summary>
        /// Gets the readonly collection with associative container of definition of controll variables.
        /// </summary>
        /// <value>
        /// The readonly collection of controll variables.
        /// </value>
        IReadonlyIndexContainer ReadonlyControllVariables { get; }

        /// <summary>
        /// Gets the readonly set of temporary variables.
        /// </summary>
        /// <value>
        /// The readoly set of temporary variables.
        /// </value>
        IReadonlySet<MemoryIndex> ReadonlyTemporaryVariables { get; }

        /// <summary>
        /// Gets the readonly set of definitions of arrays.
        /// </summary>
        /// <value>
        /// The readonly set of definitions of arrays.
        /// </value>
        IReadonlySet<AssociativeArray> ReadonlyArrays { get; }
    }

    /// <summary>
    /// Represents single level of memory stack with collections of variables and defined arrays.
    /// This level is part of memory stack an allows to separate global and local memory contexts.
    /// 
    /// This interface allows modification of inner structure.
    /// </summary>
    public interface IWriteableStackContext
    {
        /// <summary>
        /// Gets the writeable collection with associative container of definition of variables.
        /// </summary>
        /// <value>
        /// The writeable collection of variables variables.
        /// </value>
        IWriteableIndexContainer WriteableVariables { get; }

        /// <summary>
        /// Gets the writeable collection with associative container of definition of controll variables.
        /// </summary>
        /// <value>
        /// The writeable collection of controll variables.
        /// </value>
        IWriteableIndexContainer WriteableControllVariables { get; }

        /// <summary>
        /// Gets the readoly set of temporary variables.
        /// </summary>
        /// <value>
        /// The readoly set of temporary variables.
        /// </value>
        IWriteableSet<MemoryIndex> WriteableTemporaryVariables { get; }

        /// <summary>
        /// Gets the writeable set of definitions of arrays.
        /// </summary>
        /// <value>
        /// The writeable set of definitions of arrays.
        /// </value>
        IWriteableSet<AssociativeArray> WriteableArrays { get; }
    }
}
