using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework.ProgramPoints
{
    /// <summary>
    /// Representation of direct variable use, that can be assigned
    /// </summary>
    public class LVariablePoint : LVariableEntryPoint
    {
        public readonly DirectVarUse Variable;

        public override LangElement Partial { get { return Variable; } }

        internal LVariablePoint(DirectVarUse variable, RValuePoint thisObj)
            : base(thisObj)
        {
            Variable = variable;
            VariableEntry = new VariableIdentifier(Variable.VarName);
        }

        protected override void flowThrough()
        {

        }

        public override string ToString()
        {
            return "$" + Variable.VarName;
        }
    }

    /// <summary>
    /// Representation of indirect variable use that can be assigned
    /// </summary>
    public class LIndirectVariablePoint : LVariableEntryPoint
    {
        public readonly IndirectVarUse Variable;

        /// <summary>
        /// Indirect name of variable
        /// </summary>
        public readonly RValuePoint VariableName;

        public override LangElement Partial { get { return Variable; } }

        internal LIndirectVariablePoint(IndirectVarUse variable, RValuePoint variableName, RValuePoint thisObj)
            : base(thisObj)
        {
            NeedsExpressionEvaluator = true;

            Variable = variable;
            VariableName = variableName;
        }

        protected override void flowThrough()
        {
            var varNames = Services.Evaluator.VariableNames(VariableName.Value);
            if (varNames == null)
            {
                varNames = new string[0];
            }

            VariableEntry = new VariableIdentifier(varNames);
        }

    }

    /// <summary>
    /// Representation of direct variable use, which can be asked for variable value
    /// </summary>
    public class RVariablePoint : RVariableEntryPoint
    {
        public readonly DirectVarUse Variable;

        public override LangElement Partial { get { return Variable; } }

        internal RVariablePoint(DirectVarUse variable, RValuePoint thisObj)
            : base(thisObj)
        {

            Variable = variable;
            VariableEntry = new VariableIdentifier(Variable.VarName);
        }

        protected override void flowThrough()
        {
            //no preparations are needed
            resolveValue();
        }
    }

    /// <summary>
    /// Representation of indirect variable use, which can be asked for variable value
    /// </summary>
    public class RIndirectVariablePoint : RVariableEntryPoint
    {
        public readonly IndirectVarUse Variable;

        /// <summary>
        /// Indirect name of variable
        /// </summary>
        public readonly RValuePoint Name;

        public override LangElement Partial { get { return Variable; } }

        internal RIndirectVariablePoint(IndirectVarUse variable, RValuePoint name, RValuePoint thisObj)
            : base(thisObj)
        {
            NeedsExpressionEvaluator = true;
            Variable = variable;
            Name = name;
        }

        protected override void flowThrough()
        {
            var names = Services.Evaluator.VariableNames(Name.Value);
            VariableEntry = new VariableIdentifier(names);

            resolveValue();
        }
    }
}
