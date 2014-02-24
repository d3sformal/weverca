using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Generic version of cloneable interface.
    /// </summary>
    /// <typeparam name="T">Typpe of class which shoul be cloned</typeparam>
    public interface IGenericCloneable<T>
    {
        /// <summary>
        /// Creates deep copy of this instance.
        /// </summary>
        /// <returns></returns>
        T Clone();
    }
}
