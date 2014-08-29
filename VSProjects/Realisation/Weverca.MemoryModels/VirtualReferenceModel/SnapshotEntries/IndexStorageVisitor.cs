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

using Weverca.AnalysisFramework.Memory;

using Weverca.MemoryModels.VirtualReferenceModel.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel.SnapshotEntries
{
    /// <summary>
    /// Visitor used for resolving indexes on <see cref="MemoryEntry"/>
    /// </summary>
    class IndexStorageVisitor : AbstractValueVisitor
    {
        /// <summary>
        /// Determine that index with active reference is needed
        /// </summary>
        private bool _needsTemporaryIndex = false;

        /// <summary>
        /// Determine that memory entry has only AssociativeArray values
        /// </summary>
        private bool _hasOnlyArrays = true;

        /// <summary>
        /// Context of visited snapshot
        /// </summary>
        private readonly Snapshot _context;

        /// <summary>
        /// Storages resolved by walking possible values
        /// </summary>
        private readonly List<VariableKeyBase> _indexStorages = new List<VariableKeyBase>();

        /// <summary>
        /// Index identifier
        /// </summary>
        private readonly MemberIdentifier _index;

        /// <summary>
        /// Created implicit array (if needed)
        /// </summary>
        private AssociativeArray implicitArray;

        /// <summary>
        /// Result of indexing
        /// </summary>
        internal readonly ReadWriteSnapshotEntryBase IndexedValue;


        internal IndexStorageVisitor(SnapshotStorageEntry indexedEntry, Snapshot context, MemberIdentifier index)
        {
            _context = context;
            _index = index;
            var indexedValues = indexedEntry.ReadMemory(context);
            VisitMemoryEntry(indexedValues);

            if (implicitArray != null)
                //TODO replace only undefined values
                indexedEntry.WriteMemoryWithoutCopy(context, new MemoryEntry(implicitArray));

            var forceStrong = indexedEntry.ForceStrong;
            if (_hasOnlyArrays && indexedEntry.HasDirectIdentifier && index.IsDirect)
                //optimization
                forceStrong = true;

            if (_needsTemporaryIndex)
            {
                foreach (var key in indexedEntry.Storages)
                {
                    _indexStorages.Add(new VariableIndexKey(key, _index));
                }
            }

            IndexedValue = new SnapshotStorageEntry(null, forceStrong, _indexStorages.ToArray());
        }

        #region Visitor overrides

        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            _hasOnlyArrays = false;
            _needsTemporaryIndex = true;
        }

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            _hasOnlyArrays = false;

            var array = getImplicitArray();

            applyIndex(array);
        }

        /// <inheritdoc />
        public override void VisitAssociativeArray(AssociativeArray value)
        {
            applyIndex(value);
        }

        /// <inheritdoc />
        public override void VisitAnyValue(AnyValue value)
        {
            //read indexed value through memory assistant
            var indexed = _context.MemoryAssistant.ReadAnyValueIndex(value, _index);

            _indexStorages.Add(new TemporaryVariableKey(indexed));
        }

        #endregion

        #region Private helpers

        private void applyIndex(AssociativeArray array)
        {
            _indexStorages.AddRange(_context.IndexStorages(array, _index));
        }

        private AssociativeArray getImplicitArray()
        {
            if (implicitArray == null)
                implicitArray = _context.CreateArray();

            return implicitArray;
        }

        #endregion
    }
}