using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel
{
    class IndexStorageVisitor : AbstractValueVisitor
    {
        private readonly Snapshot _context;
        private readonly List<VariableInfo> _indexStorages = new List<VariableInfo>();
        private readonly MemberIdentifier _index;

        private AssociativeArray implicitArray;

        internal readonly VariableInfo[] Storages;


        internal IndexStorageVisitor(ReadWriteSnapshotEntryBase indexedEntry,Snapshot context,MemberIdentifier index)
        {
            _context = context;
            _index = index;
            var indexedValues = indexedEntry.ReadMemory(context);
            foreach (var indexedValue in indexedValues.PossibleValues)
            {
                indexedValue.Accept(this);
            }

            if (implicitArray != null)
                //TODO replace only undefined values
                indexedEntry.WriteMemory(context, new MemoryEntry(implicitArray));
            Storages = _indexStorages.ToArray();
        }

        public override void VisitValue(Value value)
        {
            throw new NotImplementedException("Reading index of given value type is not implemented yet");
        }

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            var array = getImplicitArray();

            applyIndex(array);
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            applyIndex(value);
        }

        public override void VisitAnyArrayValue(AnyArrayValue value)
        {


            throw new NotImplementedException("How can be any array value indexing implemented in memory model? ");
        }

        private void applyIndex(AssociativeArray array)
        {
            _indexStorages.AddRange(_context.IndexStorages(array,_index));
        }
        
        private AssociativeArray getImplicitArray()
        {
            if (implicitArray == null)
                implicitArray= _context.CreateArray();

            return implicitArray;
        }
    }
}
