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
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyCopyStructure
{
    class LazyCopyList<T> : IEnumerable<T> where T : IGenericCloneable<T>
    {
        private List<T> list;
        private bool copied;

        public LazyCopyList()
        {
            list = new List<T>();
            copied = true;
        }

        public LazyCopyList(LazyCopyList<T> listToCopy)
        {
            list = listToCopy.list;
            copied = false;
        }

        public void Copy()
        {
            if (!copied)
            {
                List<T> oldList = list;
                list = new List<T>();

                foreach (T value in oldList)
                {
                    list.Add(value.Clone());
                }

                copied = true;
            }
        }

        public int Count { get { return list.Count; } }

        public void Add(T value)
        {
            Copy();
            list.Add(value);
        }

        public T this[int index]
        {
            get { return list[index]; }
            set { Copy(); list[index] = value; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}