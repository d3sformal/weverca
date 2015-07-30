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
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common
{
    /// <summary>
    /// Implementation of a change tracker to track changes within given collection.
    /// </summary>
    /// <typeparam name="C">Type of the container to track changes in</typeparam>
    public class ChangeTracker<C> : IReadonlyChangeTracker<C>, IWriteableChangeTracker<C>
        where C : class
    {
        HashSet<MemoryIndex> indexChanges;
        HashSet<QualifiedName> functionChanges;
        HashSet<QualifiedName> classChanges;
        Dictionary<Snapshot, IReadonlyChangeTracker<C>> callRouting;

        /// <inheritdoc />
        public TrackerConnectionType ConnectionType { get; private set; }

        /// <inheritdoc />
        public int CallLevel { get; private set; }

        /// <inheritdoc />
        public int TrackerId { get; private set; }

        /// <inheritdoc />
        public C Container { get; private set; }

        /// <inheritdoc />
        public IReadonlyChangeTracker<C> PreviousTracker { get; private set; }

        /// <inheritdoc />
        public IEnumerable<MemoryIndex> IndexChanges
        {
            get { return indexChanges; }
        }

        /// <inheritdoc />
        public IEnumerable<QualifiedName> FunctionChanges
        {
            get { return functionChanges; }
        }

        /// <inheritdoc />
        public IEnumerable<QualifiedName> ClassChanges
        {
            get { return classChanges; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeTracker{C}"/> class.
        /// </summary>
        /// <param name="trackerId">The tracker identifier.</param>
        /// <param name="container">The container.</param>
        /// <param name="previousTracker">The previous tracker.</param>
        public ChangeTracker(int trackerId, C container, IReadonlyChangeTracker<C> previousTracker)
        {
            ConnectionType = TrackerConnectionType.EXTEND;
            CallLevel = previousTracker != null ? previousTracker.CallLevel : Snapshot.GLOBAL_CALL_LEVEL;

            TrackerId = trackerId;
            Container = container;
            PreviousTracker = previousTracker;

            indexChanges = new HashSet<MemoryIndex>();
        }

        /// <inheritdoc />
        public void InsertedIndex(MemoryIndex index)
        {
            if (!(index is TemporaryIndex))
                indexChanges.Add(index);
        }

        /// <inheritdoc />
        public void DeletedIndex(MemoryIndex index)
        {
            if (!(index is TemporaryIndex))
                indexChanges.Add(index);
        }

        /// <inheritdoc />
        public void ModifiedIndex(MemoryIndex index)
        {
            if (!(index is TemporaryIndex))
                indexChanges.Add(index);
        }

        /// <inheritdoc />
        public void RemoveIndexChange(MemoryIndex index)
        {
            indexChanges.Remove(index);
        }

        /// <inheritdoc />
        public void ModifiedFunction(QualifiedName functionName)
        {
            if (functionChanges == null)
            {
                functionChanges = new HashSet<QualifiedName>();
            }
            functionChanges.Add(functionName);
        }

        /// <inheritdoc />
        public void ModifiedClass(QualifiedName className)
        {
            if (classChanges == null)
            {
                classChanges = new HashSet<QualifiedName>();
            }
            classChanges.Add(className);
        }

        /// <inheritdoc />
        public void RemoveFunctionChange(QualifiedName functionName)
        {
            if (functionChanges != null)
            {
                functionChanges.Remove(functionName);
            }
        }

        /// <inheritdoc />
        public void RemoveClassChange(QualifiedName className)
        {
            if (classChanges != null)
            {
                classChanges.Remove(className);
            }
        }

        /// <inheritdoc />
        public void SetCallLevel(int callLevel)
        {
            CallLevel = callLevel;
        }

        /// <inheritdoc />
        public void SetConnectionType(TrackerConnectionType connectionType)
        {
            ConnectionType = connectionType;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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