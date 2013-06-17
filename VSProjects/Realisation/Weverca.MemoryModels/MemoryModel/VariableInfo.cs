using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Weverca.Analysis.Memory;
using Weverca.Analysis;

namespace Weverca.MemoryModel
{
    class VariableInfo
    {
        public MemoryEntry Values { get; private set; }

        TransactionCounter counter;

        ArrayValue arrayValue = null;
        ObjectValue objectValue = null;

        List<VariableInfo> MustAliases = null;
        List<VariableInfo> MayAliases = null;

        List<VariableInfo> MustObjectReferences = null;
        List<VariableInfo> MayObjectReferences = null;
        private MemoryEntry entry;
        private TransactionCounter counterObj;

        VariableInfo(VariableInfo old)
        {
               
        }

        public VariableInfo(MemoryEntry entry, TransactionCounter counterObj)
        {
            // TODO: Complete member initialization
            this.entry = entry;
            this.counterObj = counterObj;
        }

        public bool HasMustAliases
        {
            get { return MustAliases != null && MustAliases.Count > 0; }
        }

        public bool HasMayAliases
        {
            get { return MayAliases != null && MayAliases.Count > 0; }
        }

        public bool HasMustObjectReferences
        {
            get { return MustObjectReferences != null && MustObjectReferences.Count > 0; }
        }

        public bool HasMayObjectReferences
        {
            get { return MayObjectReferences != null && MayObjectReferences.Count > 0; }
        }


        
    }
}
