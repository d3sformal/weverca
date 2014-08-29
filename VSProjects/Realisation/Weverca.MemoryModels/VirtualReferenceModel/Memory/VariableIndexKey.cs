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
    /// Resolve indexing of non arrays
    /// </summary>
    class VariableIndexKey : VariableVirtualKeyBase
    {
        private readonly MemberIdentifier _index;


        internal VariableIndexKey(VariableKeyBase indexedVariable, MemberIdentifier index)
            : base(indexedVariable)
        {
            _index = index;
        }

        ///<inheritdoc />
        protected override string getStorageName()
        {
            return string.Format("{0}_index-{1}", ParentVariable, indexRepresentation(_index));
        }

        ///<inheritdoc />
        protected override MemoryEntry getter(Snapshot s, MemoryEntry storedValues)
        {
            var indexReader = new IndexReadExecutor(s.MemoryAssistant, _index);
            indexReader.VisitMemoryEntry(storedValues);

            return new MemoryEntry(indexReader.Result);
        }

        ///<inheritdoc />
        protected override MemoryEntry setter(Snapshot s, MemoryEntry storedValues, MemoryEntry writtenValue)
        {
            var indexWriter = new IndexWriteExecutor(s.MemoryAssistant, _index, writtenValue);
            indexWriter.VisitMemoryEntry(storedValues);

            var backWrite = new MemoryEntry(indexWriter.Result);
            return backWrite;
        }


        /// <summary>
        /// Create index representation for given index
        /// </summary>
        /// <param name="index">Index which representation is created</param>
        /// <returns>Created index representation</returns>
        private string indexRepresentation(MemberIdentifier index)
        {
            var name = new StringBuilder();
            foreach (var possibleName in index.PossibleNames)
            {
                name.Append(possibleName);
                name.Append(',');
            }

            return name.ToString();
        }
    }
}