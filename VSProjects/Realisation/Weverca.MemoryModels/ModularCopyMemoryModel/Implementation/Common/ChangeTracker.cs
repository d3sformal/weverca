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


using PHP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common
{
    public enum ChangeType 
    {
        Inserted, Deleted, Modified
    }

    public interface IReadonlyChangeTracker<C>
    {
        int TrackerId { get; }

        C Container { get; }

        IReadonlyChangeTracker<C> PreviousTracker { get; }

        IEnumerable<MemoryIndex> IndexChanges { get; }

        IEnumerable<QualifiedName> FunctionChanges { get; }

        IEnumerable<QualifiedName> ClassChanges { get; }
    }

    public interface IWriteableChangeTracker<C> : IReadonlyChangeTracker<C>
    {
        void InsertedIndex(MemoryIndex index);
        void DeletedIndex(MemoryIndex index);
        void ModifiedIndex(MemoryIndex index);
        void RemoveIndexChange(MemoryIndex index);

        void ModifiedFunction(QualifiedName function);
        void ModifiedClass(QualifiedName function);

        void RemoveFunctionChange(QualifiedName functionName);
        void RemoveClassChange(QualifiedName className);
    }

    public class ChangeTracker<C> : IReadonlyChangeTracker<C>, IWriteableChangeTracker<C>
    {
        HashSet<MemoryIndex> indexChanges;
        HashSet<QualifiedName> functionChanges;
        HashSet<QualifiedName> classChanges;

        public int TrackerId { get; private set; }
        public C Container { get; private set; }

        public IEnumerable<MemoryIndex> IndexChanges
        {
            get { return indexChanges; }
        }

        public IEnumerable<QualifiedName> FunctionChanges
        {
            get { return functionChanges; }
        }

        public IEnumerable<QualifiedName> ClassChanges
        {
            get { return classChanges; }
        }

        public IReadonlyChangeTracker<C> PreviousTracker { get; private set; }



        public ChangeTracker(int trackerId, C container, IReadonlyChangeTracker<C> previousTracker)
        {
            TrackerId = trackerId;
            Container = container;
            PreviousTracker = previousTracker;

            indexChanges = new HashSet<MemoryIndex>();
        }

        public void InsertedIndex(MemoryIndex index)
        {
            indexChanges.Add(index);
        }

        public void DeletedIndex(MemoryIndex index)
        {
            indexChanges.Add(index);
        }

        public void ModifiedIndex(MemoryIndex index)
        {
            indexChanges.Add(index);
        }

        public void RemoveIndexChange(MemoryIndex index)
        {
            indexChanges.Remove(index);
        }

        public void ModifiedFunction(QualifiedName functionName)
        {
            if (functionChanges == null)
            {
                functionChanges = new HashSet<QualifiedName>();
            }
            functionChanges.Add(functionName);
        }

        public void ModifiedClass(QualifiedName className)
        {
            if (classChanges == null)
            {
                classChanges = new HashSet<QualifiedName>();
            }
            classChanges.Add(className);
        }

        public void RemoveFunctionChange(QualifiedName functionName)
        {
            if (functionChanges != null)
            {
                functionChanges.Remove(functionName);
            }
        }

        public void RemoveClassChange(QualifiedName className)
        {
            if (classChanges != null)
            {
                classChanges.Remove(className);
            }
        }
    }
}