/*
Copyright (c) 2012-2014 Miroslav Vodolan.

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

using PHP.Core;

using Weverca.AnalysisFramework.Memory;

using Weverca.MemoryModels.VirtualReferenceModel.Containers;

namespace Weverca.MemoryModels.VirtualReferenceModel.Memory
{
    /// <summary>
    /// Hash and equals according to originatedVariable - needed for merging two branches with same variable names
    /// </summary>
    class VirtualReference
    {
        /// <summary>
        /// Variable that cause creating this reference
        /// </summary>
        internal readonly VariableName OriginatedVariable;

        /// <summary>
        /// Kind determining variable storage
        /// </summary>
        internal readonly VariableKind Kind;

        /// <summary>
        /// Stamp determining call context, where reference has been created
        /// </summary>
        internal readonly int ContextStamp;

        /// <summary>
        /// Create virtual reference according to originatedVariable
        /// </summary>
        /// <param name="originatedVariable">Variable determining reference target</param>
        /// <param name="kind">Kind of virtual reference</param>
        /// <param name="stamp">Context stamp of virtual reference</param>
        internal VirtualReference(VariableName originatedVariable, VariableKind kind, int stamp)
        {
            OriginatedVariable = originatedVariable;
            Kind = kind;
            ContextStamp = stamp;
        }

        internal VirtualReference(VariableInfo info, int stamp)
            : this(info.Name, info.Kind, stamp)
        {
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return OriginatedVariable.GetHashCode() + (int)Kind + ContextStamp;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            var o = obj as VirtualReference;
            if (o == null)
            {
                return false;
            }

            return o.OriginatedVariable == this.OriginatedVariable && o.Kind == this.Kind && o.ContextStamp == ContextStamp;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("Ref: {0}-{1}|{2}", OriginatedVariable, ContextStamp, Kind);
        }
        
        internal virtual MemoryEntry GetEntry(Snapshot snapshot, DataContainer data)
        {
            return data.GetEntry(this);
        }

        internal virtual void SetEntry(Snapshot snapshot, DataContainer data, MemoryEntry entry)
        {
            data.SetEntry(this, entry);
        }
    }
}