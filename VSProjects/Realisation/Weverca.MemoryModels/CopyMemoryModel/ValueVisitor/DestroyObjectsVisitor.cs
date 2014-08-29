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
    /// Search memory entry for object values and releases them from given snapshot.
    /// </summary>
    class DestroyObjectsVisitor : AbstractValueVisitor
    {
        private MemoryIndex parentIndex;
        private Snapshot snapshot;

        /// <summary>
        /// Initializes a new instance of the <see cref="DestroyObjectsVisitor"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="parentIndex">Index of the parent.</param>
        public DestroyObjectsVisitor(Snapshot snapshot, MemoryIndex parentIndex)
        {
            this.snapshot = snapshot;
            this.parentIndex = parentIndex;
        }

        public override void VisitValue(Value value)
        {
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            snapshot.DestroyObject(parentIndex, value);
        }
    }
}