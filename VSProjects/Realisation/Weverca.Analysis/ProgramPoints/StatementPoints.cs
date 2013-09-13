using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis.ProgramPoints
{

    /// <summary>
    /// Global statement representation
    /// </summary>
    public class GlobalStmtPoint : ProgramPointBase
    {
        private readonly VariableBased[] _variables;

        public readonly GlobalStmt Global;

        /// <summary>
        /// Variables to be fetched from global scope
        /// </summary>
        public IEnumerable<VariableBased> Variables { get { return _variables; } }

        public override LangElement Partial { get { return Global; } }


        internal GlobalStmtPoint(GlobalStmt global, VariableBased[] variables)
        {
            NeedsExpressionEvaluator = true;
            Global = global;
            _variables = variables;
        }

        protected override void flowThrough()
        {
            var variables = new VariableEntry[_variables.Length];
            for (int i = 0; i < _variables.Length; ++i)
            {
                variables[i] = _variables[i].VariableEntry;
            }
            Services.Evaluator.GlobalStatement(variables);
        }
    }

    /// <summary>
    /// Echo statement representation
    /// </summary>
    public class EchoStmtPoint : ProgramPointBase
    {
        private readonly RValuePoint[] _parameters;

        public readonly EchoStmt Echo;

        public override LangElement Partial { get { return Echo; } }

        /// <summary>
        /// Parameters pasted to echo statement
        /// </summary>
        public IEnumerable<RValuePoint> Parameters { get { return _parameters; } }

        internal EchoStmtPoint(EchoStmt echoStmt, RValuePoint[] parameters)
        {
            NeedsExpressionEvaluator = true;
            Echo = echoStmt;
            _parameters = parameters;
        }

        protected override void flowThrough()
        {
            var values = new MemoryEntry[_parameters.Length];

            for (int i = 0; i < values.Length; ++i)
            {
                values[i] = _parameters[i].Value;
            }
            Services.Evaluator.Echo(Echo, values);
        }
    }

    /// <summary>
    /// Foreach statement representation
    /// </summary>
    public class ForeachStmtPoint : ProgramPointBase
    {
        public readonly ForeachStmt Foreach;

        /// <summary>
        /// Enumerated object
        /// </summary>
        public readonly RValuePoint Enumeree;

        /// <summary>
        /// Key variable of foreach statement
        /// </summary>
        public readonly VariableBased KeyVar;

        /// <summary>
        /// Value variable of foreach statement
        /// </summary>
        public readonly VariableBased ValVar;

        public override LangElement Partial { get { return Foreach; } }

        internal ForeachStmtPoint(ForeachStmt foreachStmt, RValuePoint enumeree, VariableBased keyVar, VariableBased valVar)
        {
            NeedsExpressionEvaluator = true;

            Foreach = foreachStmt;
            KeyVar = keyVar;
            ValVar = valVar;
            Enumeree = enumeree;
        }

        protected override void flowThrough()
        {
            var keyVar = KeyVar == null ? null : KeyVar.VariableEntry;
            var valVar = ValVar == null ? null : ValVar.VariableEntry;

            Services.Evaluator.Foreach(Enumeree.Value, keyVar, valVar);
        }
    }

    /// <summary>
    /// Jump statement representation
    /// </summary>
    public class JumpStmtPoint : RValuePoint
    {
        public readonly JumpStmt Jump;

        /// <summary>
        /// Jump argument
        /// </summary>
        public readonly RValuePoint Expression;

        public override LangElement Partial { get { return Jump; } }

        internal JumpStmtPoint(RValuePoint expression, JumpStmt jmp)
        {
            NeedsFunctionResolver = true;
            Jump = jmp;
            Expression = expression;
        }

        protected override void flowThrough()
        {
            switch (Jump.Type)
            {
                case JumpStmt.Types.Return:
                    Value = Services.FunctionResolver.Return(Expression.Value);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }



}
