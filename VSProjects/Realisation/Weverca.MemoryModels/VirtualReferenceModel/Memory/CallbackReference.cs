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
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework.Memory;

using Weverca.MemoryModels.VirtualReferenceModel.Containers;

namespace Weverca.MemoryModels.VirtualReferenceModel.Memory
{
    /// <summary>
    /// Getter delegate for <see cref="CallbackReference"/>
    /// </summary>
    /// <param name="snapshot">Snapshot where getter is called</param>
    /// <returns>Got entry</returns>
    delegate MemoryEntry GetEntry(Snapshot snapshot);

    /// <summary>
    /// Setter delegate for <see cref="CallbackReference"/>
    /// </summary>
    /// <param name="snapshot">Snapshot where setter is called</param>
    /// <param name="entry">Set entry</param>
    delegate void SetEntry(Snapshot snapshot, MemoryEntry entry);

    /// <summary>
    /// Represents reference which causes callback invokation when attempted to set or get value
    /// </summary>
    class CallbackReference : VirtualReference
    {
        private readonly GetEntry _getter;

        private readonly SetEntry _setter;

        internal CallbackReference(VariableName originatedVar, GetEntry getter, SetEntry setter)
            : base(originatedVar, VariableKind.Meta, -1)
        {
            _getter = getter;
            _setter = setter;
        }

        internal override MemoryEntry GetEntry(Snapshot snapshot, DataContainer data)
        {
            return _getter(snapshot);
        }

        internal override void SetEntry(Snapshot snapshot, DataContainer data, MemoryEntry entry)
        {
            _setter(snapshot, entry);
        }

    }
}