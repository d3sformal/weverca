/*
Copyright (c) 2012-2014 Pavel Bastecky.

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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Logging;

namespace Weverca.App.Benchmarking
{
    /// <summary>
    /// Represents key to the dictionary to locate algorithm entry by the type of the 
    /// algorithm and an enclosing snapshot.
    /// </summary>
    class AlgorithmKey
    {
        Snapshot snapshot;
        AlgorithmType algorithmType;

        public AlgorithmKey(AlgorithmType algorithmType, Snapshot snapshot)
        {
            this.algorithmType = algorithmType;
            this.snapshot = snapshot;
        }

        public override bool Equals(object obj)
        {
            AlgorithmKey key = obj as AlgorithmKey;

            if (key == null)
            {
                return false;
            }

            if (algorithmType != key.algorithmType)
            {
                return false;
            }

            if (snapshot != null)
            {
                return snapshot.Equals(key.snapshot);
            }
            else
            {
                return key.snapshot == null;
            }
        }

        public override int GetHashCode()
        {
            int hash = algorithmType.GetHashCode();

            if (snapshot != null)
            {
                hash ^= snapshot.GetHashCode();
            }

            return hash;
        }
    }
}