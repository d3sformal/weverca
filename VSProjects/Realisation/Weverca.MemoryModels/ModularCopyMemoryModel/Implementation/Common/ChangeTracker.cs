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

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common
{
    public enum ChangeType 
    {
        Inserted, Deleted, Modified
    }

    public interface IReadonlyChangeTracker<T, C>
    {
        int TrackerId { get; }

        C Container { get; }

        IReadonlyChangeTracker<T, C> PreviousTracker { get; }

        IEnumerable<KeyValuePair<T, ChangeType>> Changes { get; }

        IEnumerable<T> ChangedValues { get; }
    }

    public class ChangeTracker<T, C> : IReadonlyChangeTracker<T, C>
    {
        HashSet<T> changes = new HashSet<T>();

        public int TrackerId { get; private set; }
        public C Container { get; private set; }
        public IReadonlyChangeTracker<T, C> PreviousTracker { get; private set; }
        public IEnumerable<KeyValuePair<T, ChangeType>> Changes { get { return null; } }
        public IEnumerable<T> ChangedValues { get { return changes; } }

        public ChangeTracker(int trackerId, C container, IReadonlyChangeTracker<T, C> previousTracker)
        {
            TrackerId = trackerId;
            Container = container;
            PreviousTracker = previousTracker;
        }

        public void Inserted(T value)
        {
            changes.Add(value);
            //changes[value] = ChangeType.Inserted;
        }

        public void Deleted(T value)
        {
            changes.Add(value);
            //changes[value] = ChangeType.Deleted;
        }

        public void Modified(T value)
        {
            changes.Add(value);
            /*if (!changes.ContainsKey(value))
            {
                changes[value] = ChangeType.Modified;
            }*/
        }
    }
}