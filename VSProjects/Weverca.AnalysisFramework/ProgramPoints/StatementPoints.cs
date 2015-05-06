/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

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


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework.ProgramPoints
{
    /// <summary>
    /// Throw statement representation
    /// </summary>
    public class ThrowStmtPoint : ProgramPointBase
    {
        /// <summary>
        /// Throw element represented by current point
        /// </summary>
        public readonly ThrowStmt Throw;

        /// <summary>
        /// Point representint value throwed by current statement
        /// </summary>
        public readonly ValuePoint ThrowedValue;

        /// <inheritdoc />
        public override LangElement Partial { get { return Throw; } }

        /// <summary>
        /// Branches of throw computations currently available for current point
        /// </summary>
        public IEnumerable<ThrowInfo> ThrowBranches { get; private set; }

        internal ThrowStmtPoint(ThrowStmt throwStmt, ValuePoint throwedValue)
        {
            ThrowedValue = throwedValue;
            Throw = throwStmt;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            if (ThrowBranches == null)
            {
                //initialize - throw statement cannot have flow childrens
                RemoveFlowChildren();
            }

            ThrowBranches = Services.FlowResolver.Throw(Flow, OutSet, Throw, ThrowedValue.Value.ReadMemory(OutSnapshot));
            Flow.SetThrowBranching(ThrowBranches);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitThrow(this);
        }
    }

    /// <summary>
    /// Declaration of static variable.
    /// </summary>
    public class StaticVariablePoint : ProgramPointBase 
    {
        /// <summary>
        /// Variable that is  marked as static with statement represented by current point
        /// </summary>
        private readonly LValuePoint _variable;

        private readonly VariableName _variableName;

        /// <summary>
        /// Value that should be used to initialize the static variable represented by current point
        /// </summary>
        private readonly ValuePoint _initializer;

        /// <summary>
        /// Element represented by current point
        /// </summary>
        public readonly StaticVarDecl StaticVarDecl;

        /// <summary>
        /// Variable to be fetched from static scope
        /// </summary>
        public LValuePoint Variable { get { return _variable; } }

        /// <inheritdoc />
        public override LangElement Partial { get { return StaticVarDecl; } }

        internal StaticVariablePoint(StaticVarDecl staticVarDecl, LValuePoint variable, VariableName variableName, ValuePoint initializer)
        {
            StaticVarDecl = staticVarDecl;
            _variable = variable;
            _initializer = initializer;
            _variableName = variableName;

        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            // Create the name of the variable in global controls
            // Should be unique for each function
            var varNameInGlobalStore = new VariableName ("static_" + _variableName.Value + OwningPPGraph.FunctionName + OwningScript);

            // Get the variable from global store
            var variableInGlobalStore = OutSet.GetControlVariable (varNameInGlobalStore);

            // Initialize the variable with _initializer if it may be uninitialized and has an initializer
            var values = variableInGlobalStore.ReadMemory (OutSnapshot);
            if (_initializer != null && values.PossibleValues.Any (a => a is UndefinedValue))
            {
                var newValues = new List<Value> (values.PossibleValues.Where (a => !(a is UndefinedValue)));
                newValues.AddRange (_initializer.Values.PossibleValues);
                variableInGlobalStore.WriteMemory (OutSnapshot, new MemoryEntry (newValues), true);
            } else if (_initializer == null) {
                variableInGlobalStore.WriteMemory (OutSnapshot, new MemoryEntry (OutSnapshot.UndefinedValue), true);
            }

            // Static variable is an alias of the variable from global store (this respects the implementation in official PHP interpreter)
            _variable.LValue.SetAliases (OutSnapshot, variableInGlobalStore);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitStaticVariable(this);
        }
       
    }

    /// <summary>
    /// Global statement representation
    /// </summary>
    public class GlobalStmtPoint : ProgramPointBase
    {
        /// <summary>
        /// Variables that are marked as global with statement represented by current point
        /// </summary>
        private readonly LValuePoint[] _variables;

        /// <summary>
        /// Element represented by current point
        /// </summary>
        public readonly GlobalStmt Global;

        /// <summary>
        /// Variables to be fetched from global scope
        /// </summary>
        public IEnumerable<LValuePoint> Variables { get { return _variables; } }

        /// <inheritdoc />
        public override LangElement Partial { get { return Global; } }


        internal GlobalStmtPoint(GlobalStmt global, LValuePoint[] variables)
        {
            Global = global;
            _variables = variables;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            var variables = new VariableIdentifier[_variables.Length];
            for (int i = 0; i < _variables.Length; ++i)
            {
                variables[i] = _variables[i].LValue.GetVariableIdentifier(OutSnapshot);
            }
            Services.Evaluator.GlobalStatement(variables);
        }

        /// <inheritdoc />
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
        /// <summary>
        /// Parameters available for echo statement represented by current point
        /// </summary>
        private readonly ValuePoint[] _parameters;

        /// <summary>
        /// Echo statement represented by current point
        /// </summary>
        public readonly EchoStmt Echo;
        
        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override void flowThrough()
        {
            var values = new MemoryEntry[_parameters.Length];
            var pars = new string[_parameters.Length];

            for (int i = 0; i < values.Length; ++i)
            {
                values[i] = _parameters[i].Value.ReadMemory(OutSnapshot);
                pars[i] = _parameters[i].Value.ToString();
            }
            Services.Evaluator.Echo(Echo, values, pars);
        }

        /// <inheritdoc />
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
        /// <summary>
        /// Foreach statement represented by current point
        /// </summary>
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

        /// <inheritdoc />
        public override LangElement Partial { get { return Foreach; } }

        internal ForeachStmtPoint(ForeachStmt foreachStmt, ValuePoint enumeree, LValuePoint keyVar, LValuePoint valVar)
        {
            Foreach = foreachStmt;
            KeyVar = keyVar;
            ValVar = valVar;
            Enumeree = enumeree;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            var keyVar = KeyVar == null ? null : KeyVar.LValue;
            var valVar = ValVar == null ? null : ValVar.LValue;

            Services.Evaluator.Foreach(Enumeree.Value.ReadMemory(OutSnapshot), keyVar, valVar);
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
        /// <summary>
        /// Jump statement represented by current point
        /// </summary>
        public readonly JumpStmt Jump;

        /// <summary>
        /// Jump argument
        /// </summary>
        public readonly ValuePoint Expression;

        /// <inheritdoc />
        public override LangElement Partial { get { return Jump; } }

        internal JumpStmtPoint(ValuePoint expression, JumpStmt jmp)
        {
            Jump = jmp;
            Expression = expression;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            MemoryEntry value;
            switch (Jump.Type)
            {
                case JumpStmt.Types.Return:
                    if (Expression == null)
                    {
                        //php code: return ;
                        value = new MemoryEntry(OutSet.UndefinedValue);
                    }
                    else
                    {
                        value = Services.FunctionResolver.Return(Expression.Value.ReadMemory(OutSnapshot));
                    }
                    break;
                default:
                    throw new NotSupportedException(Jump.Type+" is not supported jump type");
            }

            Value = OutSet.CreateSnapshotEntry(value);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitJump(this);
        }
    }
}