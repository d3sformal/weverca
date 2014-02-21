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
