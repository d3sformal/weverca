using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;


namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// All implementations of value has to be immutable
    /// </summary>
    public abstract class Value
    {
        /// <summary>
        /// Last unique ID that has been provided to some value
        /// </summary>
        private static int _lastValueUID = 0;

        /// <summary>
        /// Storage for info data objects of value
        /// </summary>
        private InfoDataStorage _storage;

        /// <summary>
        /// Part of hash that can be precomputed in constructor
        /// </summary>
        private int _precomputedHash;

        /// <summary>
        /// Unique id for every instance of value
        /// </summary>
        public readonly int UID;

        /// <summary>
        /// Get hashcode of inheriting part of value 
        /// (hashcode of info data storages, etc. is resolved elsewhere)
        /// </summary>
        /// <returns>Hashcode of inheriting part of value</returns>
        protected abstract int getHashCode();

        /// <summary>
        /// Determine that inheriting part of value is
        /// same as inheriting part of other
        /// </summary>
        /// <param name="other">Other value to be compared</param>
        /// <returns>True if other value has same inheriting part of value, false otherwise</returns>
        protected abstract bool equals(Value other);

        /// <summary>
        /// Create clone of current value. Is used for cloning values
        /// for passing info storage.
        /// </summary>
        /// <returns>Created clone</returns>
        protected abstract Value cloneValue();

        internal Value()
        {
            setStorage(new InfoDataStorage());

            UID = ++_lastValueUID;
        }

        /// <summary>
        /// Set info of given Type to returned value
        /// </summary>
        /// <typeparam name="DataKey">Type used as key for stored info</typeparam>
        /// <param name="infoData"></param>
        /// <returns></returns>
        public Value SetInfo<DataKey>(DataKey infoData)
            where DataKey : InfoDataBase
        {
            var storage = _storage.Clone();

            var key = typeof(DataKey);
            storage[key] = infoData;

            var clone = cloneValue();
            clone.setStorage(storage);

            return clone;
        }

        /// <summary>
        /// Get stored according to given DataKey
        /// </summary>
        /// <typeparam name="DataKey">Type used as key for stored info</typeparam>
        /// <returns>InfoData object for given key if present, null otherwise</returns>
        public DataKey GetInfo<DataKey>()
            where DataKey : InfoDataBase
        {
            var key = typeof(DataKey);
            InfoDataBase info;
            _storage.TryGetValue(key, out info);

            return info as DataKey;
        }

        public virtual void Accept(IValueVisitor visitor)
        {
            visitor.VisitValue(this);
        }

        private void setStorage(InfoDataStorage storage)
        {
            _storage = storage;
            _precomputedHash = _storage.GetHashCode();
        }

        #region Standard method overrides

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            var o = obj as Value;
            if (o == null)
                return false;

            if (!_storage.Equals(o._storage))
                //differs in stored info data
                return false;

            return equals(o);
        }

        public override int GetHashCode()
        {
            return _precomputedHash + getHashCode();
        }

        #endregion
    }
}
