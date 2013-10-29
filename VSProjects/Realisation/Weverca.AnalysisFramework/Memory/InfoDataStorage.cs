using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Storage for info data objects
    /// </summary>
    public class InfoDataStorage : Dictionary<Type, InfoDataBase>
    {

        internal InfoDataStorage() { }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="storage">Copied storage</param>
        private InfoDataStorage(InfoDataStorage storage)
            :base(storage)
        {
        }

        /// <summary>
        /// Create clone of current info storage
        /// </summary>
        /// <returns>Created clone</returns>
        internal InfoDataStorage Clone()
        {
            return new InfoDataStorage(this);
        }


        #region Standard method overrides

        public override int GetHashCode()
        {
            var hashcode = 0;

            foreach (var value in Values)
            {
                hashcode += hashcode;
            }

            return hashcode;
        }

        public override bool Equals(object obj)
        {
            var o = obj as InfoDataStorage;
            if (o == null)
                return false;

            if (o.Count != Count)
                return false;

            foreach (var pair in this)
            {
                InfoDataBase oData;
                if (!o.TryGetValue(pair.Key, out oData))
                    //missing key
                    return false;

                if (!pair.Value.Equals(oData))
                    //different info is stored
                    return false;
            }

            return true;
        }

        #endregion

    }
}
