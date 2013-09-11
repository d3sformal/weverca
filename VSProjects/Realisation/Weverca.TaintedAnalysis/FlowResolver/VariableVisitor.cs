using System.Linq;
using System.Collections.Generic;

using PHP.Core.AST;

namespace Weverca.TaintedAnalysis.FlowResolver
{
    /// <summary>
    /// This visitor will find all variable uses in an AST.
    /// </summary>
    class VariableVisitor : TreeVisitor
    {
        /// <summary>
        /// Gets the variable uses.
        /// </summary>
        public IEnumerable<VariableUse> Variables
        {
            get
            {
                return directlyUsed.Cast<VariableUse>().Concat(indirectlyUsed).Cast<VariableUse>();
            }
        }

        List<DirectVarUse> directlyUsed = new List<DirectVarUse>();
        List<IndirectVarUse> indirectlyUsed = new List<IndirectVarUse>();

        public override void VisitDirectVarUse(DirectVarUse x)
        {
            base.VisitDirectVarUse(x);
            if (directlyUsed.FirstOrDefault(a => a.VarName != x.VarName) == null)
            {
                directlyUsed.Add(x);
            }
        }

        public override void VisitIndirectVarUse(IndirectVarUse x)
        {
            base.VisitIndirectVarUse(x);
            
            //TODO: find a way to get only distinct uses.
            if (indirectlyUsed.FirstOrDefault(a => a.VarNameEx.Value != x.VarNameEx.Value) == null)
            {
                indirectlyUsed.Add(x);
            }
        }
    }
}
