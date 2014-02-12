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
        public readonly DirectStFldUse Field;

        public readonly VariableIdentifier FieldName;

        public GenericQualifiedName TypeName { get { return Field.TypeName; } }

        public ValuePoint Obj;

        public override LangElement Partial { get { return Field; } }

        public StaticFieldPoint(DirectStFldUse field)
        {
            Field = field;
            FieldName = new VariableIdentifier(field.PropertyName);
            Obj = null;
        }

        public StaticFieldPoint(DirectStFldUse field, ValuePoint typeName)
        {
            Field = field;
            FieldName = new VariableIdentifier(field.PropertyName);
            Obj = typeName;
        }

        protected override void flowThrough()
        {
            if (Obj==null)
            {
                LValue = Services.Evaluator.ResolveStaticField(TypeName, FieldName);
            }
            else
            {
                var typeValue = Obj.Value.ReadMemory(OutSnapshot);
                var ResolvedTypeNames = Services.Evaluator.TypeNames(typeValue);

                LValue = Services.Evaluator.ResolveIndirectStaticField(ResolvedTypeNames, FieldName);
            }
        }

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
        public readonly IndirectStFldUse Field;

        public readonly ValuePoint FieldName;

        public GenericQualifiedName TypeName { get { return Field.TypeName; } }

        public ValuePoint Obj;

        public override LangElement Partial { get { return Field; } }

        public IndirectStaticFieldPoint(IndirectStFldUse field, ValuePoint variable)
        {
            Field = field;
            FieldName = variable;
            Obj = null;
        }

        public IndirectStaticFieldPoint(IndirectStFldUse field, ValuePoint variable, ValuePoint typeName)
        {
            Field = field;
            FieldName = variable;
            Obj = typeName;
        }

        protected override void flowThrough()
        {
            if (Obj==null)
            {

                LValue = Services.Evaluator.ResolveStaticField(TypeName, FieldName.Value.ReadMemory(OutSnapshot));
            }
            else
            {
                var typeValue = Obj.Value.ReadMemory(OutSnapshot);
                var ResolvedTypeNames = Services.Evaluator.TypeNames(typeValue);

                LValue = Services.Evaluator.ResolveIndirectStaticField(ResolvedTypeNames, FieldName.Value.ReadMemory(OutSnapshot));
            }
        }

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
        public readonly DirectVarUse Variable;

        public readonly ValuePoint ThisObj;

        public readonly VariableIdentifier VariableName;

        public override LangElement Partial { get { return Variable; } }

        internal VariablePoint(DirectVarUse variable, ValuePoint thisObj)
        {
            Variable = variable;
            VariableName = new VariableIdentifier(Variable.VarName);
            ThisObj = thisObj;
        }

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

        public override string ToString()
        {
            return "$" + Variable.VarName;
        }

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
        public readonly IndirectVarUse Variable;

        /// <summary>
        /// Indirect name of variable
        /// </summary>
        public readonly ValuePoint VariableName;

        public readonly ValuePoint ThisObj;

        public override LangElement Partial { get { return Variable; } }

        internal IndirectVariablePoint(IndirectVarUse variable, ValuePoint variableName, ValuePoint thisObj)
        {
            Variable = variable;
            VariableName = variableName;
        }

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

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitIndirectVariable(this);
        }
    }
}
