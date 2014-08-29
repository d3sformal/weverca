/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

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


/*
Copyright (c) 2012-2014 David Hauzar and Mirek Vodolan.

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


﻿using System;
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
        /// <param name="infoData">The information data.</param>
        /// <returns>set up value</returns>
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


        /// <summary>
        /// Method for calling IValueVisitor
        /// </summary>
        /// <param name="visitor">IValueVisitor</param>
        public virtual void Accept(IValueVisitor visitor)
        {
            visitor.VisitValue(this);
        }

        /// <summary>
        /// Set new InfoDataStorage and computes hash
        /// </summary>
        /// <param name="storage">new InfoDataStorage</param>
        protected void setStorage(InfoDataStorage storage)
        {
            _storage = storage;
            _precomputedHash = _storage.GetHashCode();
        }

        /// <summary>
        /// Returns Storage of aditional value information
        /// </summary>
        /// <returns>Storage of aditional value information</returns>
        protected InfoDataStorage getStorage()
        {
            return _storage;
        }

        #region Standard method overrides

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _precomputedHash + getHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.GetType().Name;
        }

		public virtual string ToString(ISnapshotReadonly snapshot)
		{
			return ToString();
		}
      
        #endregion
    }
}