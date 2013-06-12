using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.MemoryModel.Deprecated
{
    
    
    

    class MemoryModel
    {
        private Table table;
        public MemoryModel()
        {
            table = new Table();
        }

        public void Merge(MemoryModel m) 
        {
        }
        
        public MemoryModel Copy()
        {
            MemoryModel newMemd = new MemoryModel();

            CopyResolver resolver = new CopyResolver();
            
            newMemd.table.CopyStructureFrom(table, resolver);
            resolver.ResolveAliases();

            return newMemd;
        }

        public void MergeCopy(MemoryModel source)
        {
            CopyResolver resolver = new CopyResolver();
            table.MergeCopy(source.table, resolver);
        }
     }

    class CopyResolver
    {

        Dictionary<Variable, Variable> resolveTable = new Dictionary<Variable, Variable>();

        internal void ResolveAliases()
        {
            foreach (var pair in resolveTable)
            {
                Variable oldVar = pair.Key;
                Variable newVar = pair.Value;

                foreach (Variable oldAlias in oldVar.mayAliases)
                {
                    Variable newAlias = resolveTable[oldAlias];
                    newVar.mayAliases.Add(newAlias);
                }

                if (oldVar.HasMustAliases && oldVar.IsRepresentant)
                {
                    foreach (Variable oldAlias in oldVar.mustAliases)
                    {
                        Variable newAlias = resolveTable[oldAlias];
                        newAlias.setResolvedAlias(newVar);
                    }
                }
            }
        }

        internal void Add(Variable newVar, Variable oldVar)
        {
            resolveTable[oldVar] = newVar;
        }
    }
}
