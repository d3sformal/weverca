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

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel.Memory
{
    /// <summary>
    /// Resolve fielding of non-objects
    /// </summary>
    class VariableFieldKey : VariableVirtualKeyBase
    {
        private readonly VariableIdentifier _field;

        internal VariableFieldKey(VariableKeyBase fieldedVariable, VariableIdentifier field)
            : base(fieldedVariable)
        {
            _field = field;
        }

        /// <inheritdoc />
        protected override string getStorageName()
        {
            //TODO what about multiple names ?
            return string.Format("{0}_field-{1}", ParentVariable, fieldRepresentation(_field));
        }

        /// <inheritdoc />
        protected override MemoryEntry getter(Snapshot s, MemoryEntry storedValues)
        {
            var subResults = new HashSet<Value>();

            foreach (var value in storedValues.PossibleValues)
            {
                if (value is ObjectValue)
                    continue;

                if (value is UndefinedValue)
                    continue;

                if (value is AnyValue)
                    continue;

                var subResult = s.MemoryAssistant.ReadValueField(value, _field);
                subResults.UnionWith(subResult);
            }

            return new MemoryEntry(subResults);
        }

        /// <inheritdoc />
        protected override MemoryEntry setter(Snapshot s, MemoryEntry storedValues, MemoryEntry writtenValue)
        {
            var subResults = new HashSet<Value>();

            foreach (var value in storedValues.PossibleValues)
            {
                if (value is ObjectValue)
                    continue;

                if (value is UndefinedValue)
                    continue;

                if (value is AnyValue)
                    continue;

                var subResult = s.MemoryAssistant.WriteValueField(value, _field, writtenValue);
                subResults.UnionWith(subResult);
            }

            return new MemoryEntry(subResults);
        }


        /// <summary>
        /// Create field representation for given field
        /// </summary>
        /// <param name="field">Field which representation is created</param>
        /// <returns>Created field representation</returns>
        private string fieldRepresentation(VariableIdentifier field)
        {
            var name = new StringBuilder();
            foreach (var possibleName in field.PossibleNames)
            {
                name.Append(possibleName);
                name.Append(',');
            }

            return name.ToString();
        }
    }
}