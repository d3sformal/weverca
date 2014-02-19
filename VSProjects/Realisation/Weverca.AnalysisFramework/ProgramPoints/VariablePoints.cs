using System;
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
    /// Representation of direct static field use, that can be assigned.
    /// </summary>
    public class StaticFieldPoint : LValuePoint
    {
        /// <summary>
        /// Element represented by current point
        /// </summary>
        public readonly DirectStFldUse Field;

        /// <summary>
        /// Name of static field
        /// </summary>
        public readonly VariableIdentifier FieldName;

        /// <summary>
        /// Type which static field is used if known in parse time
        /// </summary>
        public GenericQualifiedName SelfType { get { return Field.TypeName; } }
        
        /// <summary>
        /// Type which static field is used if needs to be computed
        /// </summary>
        public ValuePoint Self;

        /// <inheritdoc />
        public override LangElement Partial { get { return Field; } }

        internal StaticFieldPoint(DirectStFldUse field)
        {
            Field = field;
            FieldName = new VariableIdentifier(field.PropertyName);
            Self = null;
        }

        internal StaticFieldPoint(DirectStFldUse field, ValuePoint typeName)
        {
            Field = field;
            FieldName = new VariableIdentifier(field.PropertyName);
            Self = typeName;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            if (Self==null)
            {
                LValue = Services.Evaluator.ResolveStaticField(SelfType, FieldName);
            }
            else
            {
                var typeValue = Self.Value.ReadMemory(OutSnapshot);
                var ResolvedTypeNames = Services.Evaluator.TypeNames(typeValue);

                LValue = Services.Evaluator.ResolveIndirectStaticField(ResolvedTypeNames, FieldName);
            }
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitStaticField(this);
        }
    }

    /// <summary>
    /// Representation of indirect static field use, that can be assigned.
    /// </summary>
    public class IndirectStaticFieldPoint : LValuePoint
    {
        /// <summary>
        /// Static field use represented by current point
        /// </summary>
        public readonly IndirectStFldUse Field;

        /// <summary>
        /// Computed name of static field
        /// </summary>
        public readonly ValuePoint FieldName;

        /// <summary>
        /// Type which static field is used if known in parse time
        /// </summary>
        public GenericQualifiedName SelfType { get { return Field.TypeName; } }

        /// <summary>
        /// Type which static field is used if needs to be computed
        /// </summary>
        public ValuePoint Self;

        /// <inheritdoc />
        public override LangElement Partial { get { return Field; } }

        internal IndirectStaticFieldPoint(IndirectStFldUse field, ValuePoint variable)
        {
            Field = field;
            FieldName = variable;
            Self = null;
        }

        internal IndirectStaticFieldPoint(IndirectStFldUse field, ValuePoint variable, ValuePoint typeName)
        {
            Field = field;
            FieldName = variable;
            Self = typeName;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            if (Self==null)
            {

                LValue = Services.Evaluator.ResolveStaticField(SelfType, FieldName.Value.ReadMemory(OutSnapshot));
            }
            else
            {
                var typeValue = Self.Value.ReadMemory(OutSnapshot);
                var ResolvedTypeNames = Services.Evaluator.TypeNames(typeValue);

                LValue = Services.Evaluator.ResolveIndirectStaticField(ResolvedTypeNames, FieldName.Value.ReadMemory(OutSnapshot));
            }
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitIndirectStaticField(this);
        }
    }

    /// <summary>
    /// Representation of direct variable use, that can be assigned
    /// </summary>
    public class VariablePoint : LValuePoint
    {
        /// <summary>
        /// Direct variable use represented by current point
        /// </summary>
        public readonly DirectVarUse Variable;

        /// <summary>
        /// This object which can specify object which variable is used
        /// </summary>
        public readonly ValuePoint ThisObj;

        /// <summary>
        /// Identifier of represented variable
        /// </summary>
        public readonly VariableIdentifier VariableName;

        /// <inheritdoc />
        public override LangElement Partial { get { return Variable; } }

        internal VariablePoint(DirectVarUse variable, ValuePoint thisObj)
        {
            Variable = variable;
            VariableName = new VariableIdentifier(Variable.VarName);
            ThisObj = thisObj;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            if (ThisObj == null)
            {
                LValue = Services.Evaluator.ResolveVariable(VariableName);
            }
            else
            {
                LValue = Services.Evaluator.ResolveField(ThisObj.Value, VariableName);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "$" + Variable.VarName;
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitVariable(this);
        }
    }

    /// <summary>
    /// Representation of indirect variable use that can be assigned
    /// </summary>
    public class IndirectVariablePoint : LValuePoint
    {
        /// <summary>
        /// Indirect variable use represented by current variable
        /// </summary>
        public readonly IndirectVarUse Variable;

        /// <summary>
        /// Indirect name of variable
        /// </summary>
        public readonly ValuePoint VariableName;

        /// <summary>
        /// This object which can specify object which variable is used
        /// </summary>
        public readonly ValuePoint ThisObj;

        /// <inheritdoc />
        public override LangElement Partial { get { return Variable; } }

        internal IndirectVariablePoint(IndirectVarUse variable, ValuePoint variableName, ValuePoint thisObj)
        {
            Variable = variable;
            VariableName = variableName;
            ThisObj = thisObj;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            var varNames = Services.Evaluator.VariableNames(VariableName.Value.ReadMemory(OutSnapshot));
            if (varNames == null)
            {
                varNames = new string[0];
            }

            var variable = new VariableIdentifier(varNames);

            if (ThisObj == null)
            {
                LValue = Services.Evaluator.ResolveVariable(variable);
            }
            else
            {
                LValue = Services.Evaluator.ResolveField(ThisObj.Value, variable);
            }
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitIndirectVariable(this);
        }
    }
}
