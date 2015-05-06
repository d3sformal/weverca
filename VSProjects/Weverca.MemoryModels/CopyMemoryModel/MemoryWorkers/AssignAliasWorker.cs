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
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Implementation of algorithm for creating aliases. Creates alias links between all memory
    /// locations determined by given source and target collectors.
    /// 
    /// Data of every must target index are removed and aliased data are copied into this location. 
    /// </summary>
    class AssignAliasWorker
    {
        private Snapshot snapshot;

        AssignWorker assignWorker;

        List<MemoryIndex> mustSource = new List<MemoryIndex>();
        List<MemoryIndex> maySource = new List<MemoryIndex>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignAliasWorker"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        public AssignAliasWorker(Snapshot snapshot)
        {
            this.snapshot = snapshot;

            assignWorker = new AssignWorker(snapshot);
        }

        /// <summary>
        /// Assigns the alias.
        /// </summary>
        /// <param name="collector">The collector.</param>
        /// <param name="data">The data.</param>
        internal void AssignAlias(IIndexCollector collector, AliasData data)
        {
            assignWorker.Assign(collector, data.SourceIndex);
            makeAliases(collector, data);
        }

        /// <summary>
        /// Makes the aliases.
        /// </summary>
        /// <param name="collector">The collector.</param>
        /// <param name="data">The data.</param>
        private void makeAliases(IIndexCollector collector, AliasData data)
        {
            //Must target
            foreach (MemoryIndex index in collector.MustIndexes)
            {
                snapshot.MustSetAliases(index, data.MustIndexes, data.MayIndexes);
            }

            //Must source
            foreach (MemoryIndex index in data.MustIndexes)
            {
                snapshot.AddAliases(index, collector.MustIndexes, collector.MayIndexes);
            }

            //May target
            HashSet<MemoryIndex> sourceAliases = new HashSet<MemoryIndex>(data.MustIndexes.Concat(data.MayIndexes));
            foreach (MemoryIndex index in collector.MayIndexes)
            {
                snapshot.MaySetAliases(index, sourceAliases);
            }

            //May source
            HashSet<MemoryIndex> targetAliases = new HashSet<MemoryIndex>(collector.MustIndexes.Concat(collector.MayIndexes));
            foreach (MemoryIndex index in data.MayIndexes)
            {
                snapshot.AddAliases(index, null, targetAliases);
            }
        }
    }
}