using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Memory;
using Weverca.Analysis.Expressions;

namespace Weverca.Analysis
{

    //==========READING OF THIS PIECE OF CODE MAY HURT - Please wait for code comments =======//


    public interface ItemUseable
    {
        MemoryEntry ItemUseValue(FlowController flow);
    }

    public interface VariableBased
    {
        VariableEntry VariableEntry { get; }
    }

    public class EmptyProgramPoint : ProgramPointBase
    {
        public override LangElement Partial { get { return null; } }

        protected override void flowThrough()
        {

        }
    }

    public class GlobalStmtPoint : ProgramPointBase
    {
        public readonly GlobalStmt Global;

        private readonly VariableBased[] _variables;

        public IEnumerable<VariableBased> Variables { get { return _variables; } }

        public override LangElement Partial{get { return Global; }}

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

    public abstract class RValuePoint : ProgramPointBase
    {
        public virtual MemoryEntry Value { get; protected set; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public abstract class AliasPoint : ProgramPointBase
    {
        public virtual IEnumerable<AliasValue> Aliases { get; protected set; }
    }


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

    public class ForeachPoint : ProgramPointBase
    {
        public readonly ForeachStmt Foreach;

        public readonly RValuePoint Enumeree;

        public readonly VariableBased KeyVar;

        public readonly VariableBased ValVar;

        public override LangElement Partial{get { return Foreach; }}

        internal ForeachPoint(ForeachStmt foreachStmt,RValuePoint enumeree, VariableBased keyVar, VariableBased valVar){
            NeedsExpressionEvaluator = true;

            Foreach=foreachStmt;
            KeyVar=keyVar;
            ValVar=valVar;
            Enumeree=enumeree;
        }

        protected override void flowThrough()
        {
            var keyVar = KeyVar == null ? null : KeyVar.VariableEntry;
            var valVar = ValVar == null ? null : ValVar.VariableEntry;

            Services.Evaluator.Foreach(Enumeree.Value, keyVar, valVar);
        }
    }

    public class UnaryExPoint : RValuePoint
    {
        public readonly UnaryEx Expression;

        public readonly RValuePoint Operand;

        public override LangElement Partial { get { return Expression; } }

        internal UnaryExPoint(UnaryEx expression, RValuePoint operand)
        {
            NeedsExpressionEvaluator = true;

            Expression = expression;
            Operand = operand;
        }

        protected override void flowThrough()
        {
            Value = Services.Evaluator.UnaryEx(Expression.PublicOperation, Operand.Value);
        }
    }

    public class BinaryExPoint : RValuePoint
    {
        public readonly BinaryEx Expression;

        public readonly RValuePoint LeftOperand;
        public readonly RValuePoint RightOperand;

        public override LangElement Partial { get { return Expression; } }

        internal BinaryExPoint(BinaryEx expression, RValuePoint lOperand, RValuePoint rOperand)
        {
            NeedsExpressionEvaluator = true;

            Expression = expression;
            LeftOperand = lOperand;
            RightOperand=rOperand;
        }

        protected override void flowThrough()
        {
            Value = Services.Evaluator.BinaryEx(LeftOperand.Value,Expression.PublicOperation,RightOperand.Value);
        }
    }

    public class AssumePoint : ProgramPointBase
    {
        public readonly AssumptionCondition Condition;

        public readonly IEnumerable<RValuePoint> ExpressionParts;

        public readonly EvaluationLog Log;

        public override LangElement Partial { get { return null; } }

        public bool Assumed { get; private set; }

        internal AssumePoint(AssumptionCondition condition, IEnumerable<RValuePoint> expressionParts)
        {
            NeedsFlowResolver = true;
            Condition = condition;
            Log = new EvaluationLog(expressionParts);
        }

        protected override void flowThrough()
        {
            Assumed = Services.FlowResolver.ConfirmAssumption(OutSet, Condition, Log);
        }

        protected override void enqueueChildren()
        {
            if (Assumed)
            {
                //only if assumption is made, process children
                base.enqueueChildren();
            }
        }
    }

    public abstract class VariableEntryPoint : LValuePoint,VariableBased
    {
        public VariableEntry VariableEntry { get; protected set; }

        public override void Assign(FlowController flow, MemoryEntry entry)
        {
            flow.Services.Evaluator.Assign(VariableEntry, entry);
        }

        public override void AssignAlias(FlowController flow, IEnumerable<AliasValue> alias)
        {
            flow.Services.Evaluator.AliasAssign(VariableEntry, alias);
        }
    }

    public class MemoryEntryPoint : RValuePoint
    {
        public readonly LangElement Element;
        public override LangElement Partial { get { return Element; } }

        internal MemoryEntryPoint(LangElement element, MemoryEntry entry)
        {
            Element = element;
            Value = entry;
        }

        protected override void flowThrough()
        {
            throw new NotSupportedException("This node is used only as workaround for testing");
        }
    }

    public class VariablePoint : VariableEntryPoint
    {
        public readonly LangElement Element;
        public override LangElement Partial { get { return Element; } }

        internal VariablePoint(LangElement element, VariableEntry entry)
        {
            Element = element;
            VariableEntry = entry;
        }

        protected override void flowThrough()
        {
            throw new NotSupportedException("This node is used only as workaround for testing");
        }
    }

    public class NativeAnalyzerPoint : RValuePoint
    {
        public readonly NativeAnalyzer Analyzer;

        public override LangElement Partial { get { return Analyzer; } }

        internal NativeAnalyzerPoint(NativeAnalyzer analyzer)
        {
            NeedsFunctionResolver = true;
            Analyzer = analyzer;
        }

        protected override void flowThrough()
        {
            Analyzer.Method(Flow);
        }
    }

    public class ArrayExPoint : RValuePoint
    {
        public readonly ArrayEx Array;

        public override LangElement Partial { get { return Array; } }

        private LinkedList<KeyValuePair<RValuePoint, RValuePoint>> _initializedValues;

        public ArrayExPoint(ArrayEx array, LinkedList<KeyValuePair<RValuePoint, RValuePoint>> initializedValues)
        {
            NeedsExpressionEvaluator = true;

            _initializedValues = initializedValues;
            Array = array;
        }

        protected override void flowThrough()
        {
            var initializer = new List<KeyValuePair<MemoryEntry, MemoryEntry>>(_initializedValues.Count);
            foreach (var pair in _initializedValues)
            {
                var index = pair.Key == null ? null : pair.Key.Value;
                var value = pair.Value.Value;
                initializer.Add(new KeyValuePair<MemoryEntry, MemoryEntry>(index, value));
            }

            Value = Services.Evaluator.ArrayEx(initializer);
        }
    }

    public class ConstantDeclPoint : RValuePoint
    {
        public readonly ConstantDecl Declaration;

        public readonly RValuePoint Initializer;

        public override LangElement Partial { get { return Declaration; } }


        internal ConstantDeclPoint(ConstantDecl declaration, RValuePoint initializer)
        {
            NeedsExpressionEvaluator = true;

            Declaration = declaration;
            Initializer = initializer;
        }

        protected override void flowThrough()
        {
            Services.Evaluator.ConstantDeclaration(Declaration, Initializer.Value);

            Value = Initializer.Value;
        }
    }

    public class JumpPoint : RValuePoint
    {
        public readonly JumpStmt Jump;

        public readonly RValuePoint Expression;

        public override LangElement Partial { get { return Jump; } }

        internal JumpPoint(RValuePoint expression, JumpStmt jmp)
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

    public class IncludePoint : RValuePoint
    {
        public readonly IncludingEx Include;

        public readonly RValuePoint IncludePath;

        public override LangElement Partial { get { return Include; } }

        public override MemoryEntry Value
        {
            get
            {
                return Extension.Sink.Value;
            }
            protected set
            {
                throw new NotSupportedException("Cannot set value of call node");
            }
        }

        internal IncludePoint(IncludingEx include, RValuePoint includePath)
        {
            NeedsFlowResolver = true;

            Include = include;
            IncludePath = includePath;
        }

        protected override void flowThrough()
        {
            Flow.FlowResolver.Include(Flow, IncludePath.Value);
        }
    }

    public class FunctionCallPoint : RValuePoint
    {
        public readonly DirectFcnCall FunctionCall;
        public IEnumerable<RValuePoint> Arguments { get { return _arguments; } }
        public override LangElement Partial { get { return FunctionCall; } }

        private readonly RValuePoint[] _arguments;

        public override MemoryEntry Value
        {
            get
            {
                return Extension.Sink.Value;
            }
            protected set
            {
                throw new NotSupportedException("Cannot set value of call node");
            }
        }

        internal FunctionCallPoint(DirectFcnCall functionCall, RValuePoint[] arguments)
        {
            NeedsFunctionResolver = true;

            FunctionCall = functionCall;
            _arguments = arguments;
        }

        protected override void flowThrough()
        {
            PrepareArguments(_arguments, Flow);
            Services.FunctionResolver.Call(FunctionCall.QualifiedName, Flow.Arguments);
        }

        internal static void PrepareArguments(RValuePoint[] arguments, FlowController flow)
        {
            //TODO better argument handling avoid copying values
            var argumentValues = new MemoryEntry[arguments.Length];
            for (int i = 0; i < arguments.Length; ++i)
            {
                argumentValues[i] = arguments[i].Value;
            }

            flow.Arguments = argumentValues;
        }
    }

    public class IndirectFunctionCallPoint : RValuePoint
    {
        public readonly IndirectFcnCall FunctionCall;
        public IEnumerable<RValuePoint> Arguments { get { return _arguments; } }
        public readonly RValuePoint Name;
        public override LangElement Partial { get { return FunctionCall; } }

        private readonly RValuePoint[] _arguments;


        public override MemoryEntry Value
        {
            get
            {
                return Extension.Sink.Value;
            }
            protected set
            {
                throw new NotSupportedException("Cannot set value of call node");
            }
        }

        internal IndirectFunctionCallPoint(IndirectFcnCall functionCall,RValuePoint name, RValuePoint[] arguments)
        {
            NeedsFunctionResolver = true;

            Name = name;
            FunctionCall = functionCall;
            _arguments = arguments;
        }

        protected override void flowThrough()
        {
            FunctionCallPoint.PrepareArguments(_arguments, Flow);
            Services.FunctionResolver.IndirectCall(Name.Value, Flow.Arguments);
        }
    }


    public class ExtensionSinkPoint : RValuePoint
    {
        public readonly FlowExtension OwningExtension;

        public override LangElement Partial { get { return null; } }

        internal ExtensionSinkPoint(FlowExtension owningExtension)
        {
            NeedsFlowResolver = true;
            NeedsFunctionResolver = true;

            OwningExtension = owningExtension;
        }

        protected override void flowThrough()
        {
            Services.FlowResolver.CallDispatchMerge(OutSet, OwningExtension.Branches, OwningExtension.Type);
            Value = Services.FunctionResolver.ResolveReturnValue(OwningExtension.Branches);
        }

        protected override void extendInput()
        {
            _inSet.StartTransaction();
            //skip outset because of it belongs into call context
            _inSet.Extend(OwningExtension.Owner.InSet);
            _inSet.CommitTransaction();
        }
    }

    public abstract class LValuePoint : ProgramPointBase
    {
        public abstract void Assign(FlowController flow, MemoryEntry entry);
        public abstract void AssignAlias(FlowController flow, IEnumerable<AliasValue> aliases);
    }

    public class LVariablePoint : VariableEntryPoint
    {
        public readonly DirectVarUse Variable;
        public override LangElement Partial { get { return Variable; } }

        internal LVariablePoint(DirectVarUse variable)
        {
            Variable = variable;
            VariableEntry = new VariableEntry(Variable.VarName);
        }

        protected override void flowThrough()
        {

        }

        public override string ToString()
        {
            return "$" + Variable.VarName;
        }
    }

    public class LIndirectVariablePoint : VariableEntryPoint
    {
        public readonly IndirectVarUse Variable;

        public readonly RValuePoint VariableName;

        public override LangElement Partial { get { return Variable; } }

        internal LIndirectVariablePoint(IndirectVarUse variable, RValuePoint variableName)
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

            VariableEntry = new VariableEntry(varNames);
        }

    }

    public class LItemUsePoint : LValuePoint
    {
        public readonly ItemUse ItemUse;

        public readonly RValuePoint UsedItem;

        public readonly RValuePoint Index;

        public MemoryEntry IndexedValue { get; private set; }

        public override LangElement Partial { get { return ItemUse; } }

        internal LItemUsePoint(ItemUse itemUse, RValuePoint usedItem, RValuePoint index)
        {
            NeedsExpressionEvaluator = true;

            ItemUse = itemUse;
            UsedItem = usedItem;
            Index = index;
        }

        public override void Assign(FlowController flow, MemoryEntry entry)
        {
            flow.Services.Evaluator.IndexAssign(IndexedValue, Index.Value, entry);
        }

        protected override void flowThrough()
        {
            var initializable = UsedItem as ItemUseable;
            if (initializable != null)
            {
                IndexedValue = initializable.ItemUseValue(Flow);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void AssignAlias(FlowController flow, IEnumerable<AliasValue> aliases)
        {
            throw new NotImplementedException();
        }
    }

    public class RItemUsePoint : RValuePoint
    {
        public readonly ItemUse ItemUse;
        public readonly RValuePoint UsedItem;
        public readonly RValuePoint Index;
        public override LangElement Partial { get { return ItemUse; } }

        internal RItemUsePoint(ItemUse itemUse, RValuePoint usedItem, RValuePoint index)
        {
            NeedsExpressionEvaluator = true;

            ItemUse = itemUse;
            UsedItem = usedItem;
            Index = index;
        }

        protected override void flowThrough()
        {
            Value = Services.Evaluator.ResolveIndex(UsedItem.Value, Index.Value);
        }

    }

    public class RVariablePoint : RValuePoint, ItemUseable,VariableBased
    {
        public readonly DirectVarUse Variable;
        public VariableEntry VariableEntry { get; private set; }

        public override LangElement Partial { get { return Variable; } }

        internal RVariablePoint(DirectVarUse variable)
        {
            NeedsExpressionEvaluator = true;
            Variable = variable;
            VariableEntry = new VariableEntry(Variable.VarName);
        }

        protected override void flowThrough()
        {
            Value = Services.Evaluator.ResolveVariable(VariableEntry);
        }

        public MemoryEntry ItemUseValue(FlowController flow)
        {
            return flow.Services.Evaluator.ResolveIndexedVariable(VariableEntry);
        }
    }

    public class RIndirectVariablePoint : RValuePoint, ItemUseable, VariableBased
    {
        public readonly IndirectVarUse Variable;
        public VariableEntry VariableEntry { get; private set; }
        public readonly RValuePoint Name;

        public override LangElement Partial { get { return Variable; } }

        internal RIndirectVariablePoint(IndirectVarUse variable,RValuePoint name)
        {
            NeedsExpressionEvaluator = true;
            Variable = variable;
            Name = name;
        }

        protected override void flowThrough()
        {
            var names = Services.Evaluator.VariableNames(Name.Value);
            VariableEntry = new VariableEntry(names);

            Value = Services.Evaluator.ResolveVariable(VariableEntry);
        }

        public MemoryEntry ItemUseValue(FlowController flow)
        {
            return flow.Services.Evaluator.ResolveIndexedVariable(VariableEntry);
        }
    }


    internal delegate MemoryEntry ConstantProvider(ExpressionEvaluatorBase evaluator);

    public class ConstantProgramPoint : RValuePoint
    {
        private readonly ConstantProvider _constantProvider;
        private readonly LangElement _partial;

        public override LangElement Partial { get { return _partial; } }

        internal ConstantProgramPoint(LangElement partial, ConstantProvider constantProvider)
        {
            NeedsExpressionEvaluator = true;
            _constantProvider = constantProvider;
            _partial = partial;
        }

        protected override void flowThrough()
        {
            Value = _constantProvider(Services.Evaluator);
        }
    }

    public class FunctionDeclPoint : ProgramPointBase
    {
        public readonly FunctionDecl Declaration;

        public override LangElement Partial { get { return Declaration; } }

        internal FunctionDeclPoint(FunctionDecl declaration)
        {
            NeedsFunctionResolver = true;
            Declaration = declaration;
        }

        protected override void flowThrough()
        {
            Services.FunctionResolver.DeclareGlobal(Declaration);
        }
    }

    
    public class AssignPoint : RValuePoint
    {
        public readonly ValueAssignEx Assign;

        public override LangElement Partial { get { return Assign; } }

        public readonly LValuePoint LOperand;

        public readonly RValuePoint ROperand;

        internal AssignPoint(ValueAssignEx assign, LValuePoint lOperand, RValuePoint rOperand)
        {
            NeedsExpressionEvaluator = true;
            LOperand = lOperand;
            ROperand = rOperand;
            Assign = assign;
        }

        protected override void flowThrough()
        {
            LOperand.Assign(Flow, ROperand.Value);
        }
    }

    public class RefAssignPoint : RValuePoint
    {
        public readonly RefAssignEx Assign;

        public override LangElement Partial { get { return Assign; } }

        public readonly LValuePoint LOperand;

        public readonly AliasPoint ROperand;

        internal RefAssignPoint(RefAssignEx assign, LValuePoint lOperand, AliasPoint rOperand)
        {
            NeedsExpressionEvaluator = true;
            LOperand = lOperand;
            ROperand = rOperand;
            Assign = assign;
        }

        protected override void flowThrough()
        {
            LOperand.AssignAlias(Flow, ROperand.Aliases);
        }
    }
}
