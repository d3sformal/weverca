using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

namespace Weverca.AnalysisFramework.ProgramPoints
{
    /// <summary>
    /// Represents alias for direct variable use
    /// TODO: needs this object support
    /// </summary>
    public class AliasVariablePoint : AliasPoint
    {
        public readonly DirectVarUse Variable;

        public readonly VariableEntry VariableEntry;

        public override LangElement Partial { get { return Variable; } }

        internal AliasVariablePoint(DirectVarUse variable)
        {
            NeedsExpressionEvaluator = true;

            VariableEntry = new VariableEntry(variable.VarName);
            Variable = variable;
        }

        protected override void flowThrough()
        {
            Aliases = Services.Evaluator.ResolveAlias(VariableEntry);
        }
    }
}
