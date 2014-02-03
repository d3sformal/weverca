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

        public override LangElement Partial { get { return Field; } }

        public StaticFieldPoint(DirectStFldUse field)
        {
            Field = field;
            FieldName = new VariableIdentifier(field.PropertyName);
        }

        protected override void flowThrough()
        {
            LValue = Services.Evaluator.ResolveStaticField(TypeName, FieldName);
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Representation of indirect static field use, that can be assigned.
    /// </summary>
    public class IndirectStaticFieldPoint : LValuePoint
    {
        public readonly DirectStFldUse Field;

        public readonly VariableIdentifier FieldName;

        public readonly ValuePoint TypeName;

        public IEnumerable<GenericQualifiedName> ResolvedTypeNames { get; private set; }

        public override LangElement Partial { get { return Field; } }

        public IndirectStaticFieldPoint(DirectStFldUse field, ValuePoint typeName)
        {
            Field = field;
            FieldName = new VariableIdentifier(field.PropertyName);

            TypeName = typeName;
        }

        protected override void flowThrough()
        {
            var typeValue = TypeName.Value.ReadMemory(OutSnapshot);
            ResolvedTypeNames = Services.Evaluator.TypeNames(typeValue);

            LValue = Services.Evaluator.ResolveIndirectStaticField(ResolvedTypeNames, FieldName);
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            throw new NotImplementedException();
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
            var varNames = Services.Evaluator.VariableNames(VariableName.Value.ReadMemory(InSnapshot));
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
