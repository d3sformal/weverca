using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    /// <summary>
    /// Contains structural data about memory indexes. Every memory index used in snapshot is mapped
    /// to one instance of IIndexDefinition interface. This interface allows to set structural data like aliases,
    /// array associated with index and associated object which is used to traverse the memory tree.
    /// This class is extension of memory index so when there is structural change the instane of
    /// iIndexData is changed and memory index can stay the same which prevents cascade of changes
    /// across whole snapshot.
    /// 
    /// Imutable class.
    /// </summary>
    public interface IIndexDefinition
    {
        /// <summary>
        /// Gets the object with informations about alias structure for associated memory index.
        /// </summary>
        /// <value>
        /// The aliases.
        /// </value>
        IMemoryAlias Aliases { get; }

        /// <summary>
        /// Gets the array which is assocoated with memory index.
        /// </summary>
        /// <value>
        /// The array.
        /// </value>
        IObjectValueContainer Objects { get; }

        /// <summary>
        /// Gets the list of all objects which are assocoated with memory index.
        /// </summary>
        /// <value>
        /// The objects.
        /// </value>
        AssociativeArray Array { get; }
    }
}
