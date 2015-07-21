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
using Weverca.AnalysisFramework;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common
{
    public enum TrackerConnectionType
    {
        EXTEND, CALL_EXTEND, MERGE, SUBPROGRAM_MERGE, CALL_MERGE
    }

    public interface IReadonlyChangeTracker<C>
        where C : class
    {
        TrackerConnectionType ConnectionType { get; }

        int TrackerId { get; }

        int CallLevel { get; }

        C Container { get; }

        IReadonlyChangeTracker<C> PreviousTracker { get; }

        IEnumerable<MemoryIndex> IndexChanges { get; }

        IEnumerable<QualifiedName> FunctionChanges { get; }

        IEnumerable<QualifiedName> ClassChanges { get; }

        bool TryGetCallTracker(Snapshot callSnapshot, out IReadonlyChangeTracker<C> callTracker);
    }

    public interface IWriteableChangeTracker<C> : IReadonlyChangeTracker<C>
        where C : class
    {
        void SetCallLevel(int callLevel);
        void SetConnectionType(TrackerConnectionType connectionType);

        void InsertedIndex(MemoryIndex index);
        void DeletedIndex(MemoryIndex index);
        void ModifiedIndex(MemoryIndex index);
        void RemoveIndexChange(MemoryIndex index);

        void ModifiedFunction(QualifiedName function);
        void ModifiedClass(QualifiedName function);

        void RemoveFunctionChange(QualifiedName functionName);
        void RemoveClassChange(QualifiedName className);

        void AddCallTracker(Snapshot callSnapshot, IReadonlyChangeTracker<C> callTracker);
    }

    public class ChangeTracker<C> : IReadonlyChangeTracker<C>, IWriteableChangeTracker<C>
        where C : class
    {
        HashSet<MemoryIndex> indexChanges;
        HashSet<QualifiedName> functionChanges;
        HashSet<QualifiedName> classChanges;
        Dictionary<Snapshot, IReadonlyChangeTracker<C>> callRouting;


        public TrackerConnectionType ConnectionType { get; private set; }
        public int CallLevel { get; private set; }
        public int TrackerId { get; private set; }
        public C Container { get; private set; }

        public IReadonlyChangeTracker<C> PreviousTracker { get; private set; }

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

        public ChangeTracker(int trackerId, C container, IReadonlyChangeTracker<C> previousTracker)
        {
            ConnectionType = TrackerConnectionType.EXTEND;
            CallLevel = previousTracker != null ? previousTracker.CallLevel : Snapshot.GLOBAL_CALL_LEVEL;

            TrackerId = trackerId;
            Container = container;
            PreviousTracker = previousTracker;

            indexChanges = new HashSet<MemoryIndex>();
        }

        public void InsertedIndex(MemoryIndex index)
        {
            if (!(index is TemporaryIndex))
                indexChanges.Add(index);
        }

        public void DeletedIndex(MemoryIndex index)
        {
            if (!(index is TemporaryIndex))
                indexChanges.Add(index);
        }

        public void ModifiedIndex(MemoryIndex index)
        {
            if (!(index is TemporaryIndex))
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

        public void SetCallLevel(int callLevel)
        {
            CallLevel = callLevel;
        }

        public void SetConnectionType(TrackerConnectionType connectionType)
        {
            ConnectionType = connectionType;
        }

        public void AddCallTracker(Snapshot callSnapshot, IReadonlyChangeTracker<C> callTracker)
        {
            if (callRouting == null)
            {
                callRouting = new Dictionary<Snapshot, IReadonlyChangeTracker<C>>();
            }

            if (!callRouting.ContainsKey(callSnapshot))
            {
                callRouting.Add(callSnapshot, callTracker);
            }
            else
            {
                callRouting[callSnapshot] = null;
            }
        }


        public bool TryGetCallTracker(Snapshot callSnapshot, out IReadonlyChangeTracker<C> callTracker)
        {
            if (callRouting != null)
            {
                if (callRouting.TryGetValue(callSnapshot, out callTracker))
                {
                    return callTracker != null;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                callTracker = null;
                return false;
            }
        }
    }
}