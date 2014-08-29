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

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

using Weverca.MemoryModels.VirtualReferenceModel.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel.SnapshotEntries
{
    /// <summary>
    /// Visitor used for resolving fields on <see cref="MemoryEntry"/>
    /// </summary>
    class FieldStorageVisitor : AbstractValueVisitor
    {
        /// <summary>
        /// Determine that index with active reference is needed
        /// </summary>
        private bool _needsTemporaryField = false;

        /// <summary>
        /// Context of visited snapshot
        /// </summary>
        private readonly Snapshot _context;

        /// <summary>
        /// Storages resolved by walking possible values
        /// </summary>
        private readonly List<VariableKeyBase> _fieldStorages = new List<VariableKeyBase>();

        /// <summary>
        /// Field identifier
        /// </summary>
        private readonly VariableIdentifier _field;

        /// <summary>
        /// Created implicit object (if needed)
        /// </summary>
        private ObjectValue _implicitObject = null;

        /// <summary>
        /// Result of field operation
        /// </summary>
        internal readonly VariableKeyBase[] Storages;

        internal FieldStorageVisitor(SnapshotStorageEntry fieldedEntry, Snapshot context, VariableIdentifier field)
        {
            _context = context;
            _field = field;

            var fieldedValues = fieldedEntry.ReadMemory(context);
            VisitMemoryEntry(fieldedValues);

            if (_implicitObject != null)
                //TODO replace only undefined values
                fieldedEntry.WriteMemory(context, new MemoryEntry(_implicitObject));

            if (_needsTemporaryField)
            {
                foreach (var key in fieldedEntry.Storages)
                {
                    _fieldStorages.Add(new VariableFieldKey(key, _field));
                }
            }

            Storages = _fieldStorages.ToArray();
        }

        #region Visitor overrides

        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            //Temporary field is needed for resolving given value
            _needsTemporaryField = true;
        }

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            var obj = getImplicitObject();
            applyField(obj);
        }

        /// <inheritdoc />
        public override void VisitObjectValue(ObjectValue value)
        {
            applyField(value);
        }

        /// <inheritdoc />
        public override void VisitAnyValue(AnyValue value)
        {
            //read fielded value through memory assistant
            var fielded = _context.MemoryAssistant.ReadAnyField(value, _field);

            _fieldStorages.Add(new TemporaryVariableKey(fielded));
        }
        #endregion

        #region Private helpers

        private void applyField(ObjectValue objectValue)
        {
            _fieldStorages.AddRange(_context.FieldStorages(objectValue, _field));
        }

        private ObjectValue getImplicitObject()
        {
            if (_implicitObject == null)
                _implicitObject = _context.MemoryAssistant.CreateImplicitObject();

            return _implicitObject;
        }

        #endregion
    }
}