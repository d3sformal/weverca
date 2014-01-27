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
    /// Throw statement representation
    /// </summary>
    public class ThrowStmtPoint : ProgramPointBase
    {
        public readonly ThrowStmt Throw;

        public readonly ValuePoint ThrowedValue;

        public override LangElement Partial { get { return Throw; } }

        internal ThrowStmtPoint(ThrowStmt throwStmt, ValuePoint throwedValue)
        {
            ThrowedValue = throwedValue;
            Throw = throwStmt;
        }

        protected override void flowThrough()
        {
            var catchBlocks = Services.FlowResolver.Throw(Flow, OutSet, Throw, ThrowedValue.Value.ReadMemory(InSet.Snapshot));

            RemoveFlowChildren();

            foreach (var catchBlock in catchBlocks)
            {
                AddFlowChild(catchBlock);
            }
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitThrow(this);
        }
    }

    /// <summary>
    /// Global statement representation
    /// </summary>
    public class GlobalStmtPoint : ProgramPointBase
    {
        private readonly LValuePoint[] _variables;

        public readonly GlobalStmt Global;

        /// <summary>
        /// Variables to be fetched from global scope
        /// </summary>
        public IEnumerable<LValuePoint> Variables { get { return _variables; } }

        public override LangElement Partial { get { return Global; } }


        internal GlobalStmtPoint(GlobalStmt global, LValuePoint[] variables)
        {
            Global = global;
            _variables = variables;
        }

        protected override void flowThrough()
        {
            var variables = new VariableIdentifier[_variables.Length];
            for (int i = 0; i < _variables.Length; ++i)
            {
                variables[i] = _variables[i].LValue.GetVariableIdentifier(InSet.Snapshot);
            }
            Services.Evaluator.GlobalStatement(variables);
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitGlobal(this);
        }
    }

    /// <summary>
    /// Echo statement representation
    /// </summary>
    public class EchoStmtPoint : ProgramPointBase
    {
        private readonly ValuePoint[] _parameters;

        public readonly EchoStmt Echo;

        public override LangElement Partial { get { return Echo; } }

        /// <summary>
        /// Parameters pasted to echo statement
        /// </summary>
        public IEnumerable<ValuePoint> Parameters { get { return _parameters; } }

        internal EchoStmtPoint(EchoStmt echoStmt, ValuePoint[] parameters)
        {
            Echo = echoStmt;
            _parameters = parameters;
        }

        protected override void flowThrough()
        {
            var values = new MemoryEntry[_parameters.Length];

            for (int i = 0; i < values.Length; ++i)
            {
                values[i] = _parameters[i].Value.ReadMemory(InSnapshot);
            }
            Services.Evaluator.Echo(Echo, values);
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitEcho(this);
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
        public readonly ValuePoint Enumeree;

        /// <summary>
        /// Key variable of foreach statement
        /// </summary>
        public readonly LValuePoint KeyVar;

        /// <summary>
        /// Value variable of foreach statement
        /// </summary>
        public readonly LValuePoint ValVar;

        public override LangElement Partial { get { return Foreach; } }

        internal ForeachStmtPoint(ForeachStmt foreachStmt, ValuePoint enumeree, LValuePoint keyVar, LValuePoint valVar)
        {
            Foreach = foreachStmt;
            KeyVar = keyVar;
            ValVar = valVar;
            Enumeree = enumeree;
        }

        protected override void flowThrough()
        {
            var keyVar = KeyVar == null ? null : KeyVar.LValue;
            var valVar = ValVar == null ? null : ValVar.LValue;

            Services.Evaluator.Foreach(Enumeree.Value.ReadMemory(InSet.Snapshot), keyVar, valVar);
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitForeach(this);
        }
    }

    /// <summary>
    /// Jump statement representation
    /// </summary>
    public class JumpStmtPoint : ValuePoint
    {
        public readonly JumpStmt Jump;

        /// <summary>
        /// Jump argument
        /// </summary>
        public readonly ValuePoint Expression;

        public override LangElement Partial { get { return Jump; } }

        internal JumpStmtPoint(ValuePoint expression, JumpStmt jmp)
        {
            Jump = jmp;
            Expression = expression;
        }

        protected override void flowThrough()
        {
            MemoryEntry value;
            switch (Jump.Type)
            {
                case JumpStmt.Types.Return:
                    if (Expression == null)
                    {
                        //php code: return ;
                        value =new MemoryEntry(OutSet.UndefinedValue);
                    }
                    else
                    {
                        value = Services.FunctionResolver.Return(Expression.Value.ReadMemory(InSnapshot));
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            Value = OutSet.CreateSnapshotEntry(value);
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitJump(this);
        }
    }



}
