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


﻿using System;
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

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashcode = 0;

            foreach (var value in Values)
            {
                hashcode += hashcode;
            }

            return hashcode;
        }

        /// <inheritdoc />
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