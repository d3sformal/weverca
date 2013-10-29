using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Base class for info data objects. Info data can be stored in Value.
    /// </summary>
    public abstract class InfoDataBase
    {
        /// <summary>
        /// Get hashcode used for hash containers. It is
        /// expected to has same hashcode even in different infodata 
        /// instances with same stored info.
        /// </summary>
        /// <returns>Hash code of info data</returns>
        protected abstract int getHashCode();

        /// <summary>
        /// Determine that given info has same stored data
        /// </summary>
        /// <param name="other">Compared info</param>
        /// <returns>True if other info has same data stored, false otherwise</returns>
        protected abstract bool equals(InfoDataBase other);
    }
}
