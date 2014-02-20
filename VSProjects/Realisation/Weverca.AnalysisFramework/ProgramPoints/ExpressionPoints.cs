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
    /// Post/pre increment or decrement representation
    /// </summary>
    public class IncDecExPoint : ValuePoint
    {
        /// <summary>
        /// Post/pre increment or decrement expression
        /// </summary>
        public readonly IncDecEx IncDecEx;

        /// <summary>
        /// Value that is incremented
        /// </summary>
        public readonly ValuePoint IncrementedValue;

        /// <summary>
        /// Here is stored incremented value
        /// </summary>
        public readonly LValuePoint IncrementTarget;

        /// <summary>
        /// Parts of concatenated string
        /// </summary>
        public readonly IEnumerable<ValuePoint> Parts;

        /// <inheritdoc />
        public override LangElement Partial { get { return IncDecEx; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncDecExPoint" /> class.
        /// </summary>
        /// <param name="incDecEx">Post/pre increment or decrement expression</param>
        /// <param name="incrementedValue">Program point with increment or decrement expression</param>
        internal IncDecExPoint(IncDecEx incDecEx, ValuePoint incrementedValue)
        {
            IncDecEx = incDecEx;
            IncrementedValue = incrementedValue;
            IncrementTarget = incrementedValue as LValuePoint;

            if (IncrementTarget == null)
            {
                throw new NotSupportedException("Given incrementedValue doesn't support incrementation");
            }
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            var beforeIncrementValue = IncrementedValue.Value.ReadMemory(OutSnapshot);
            var afterIncrementValue = Services.Evaluator.IncDecEx(IncDecEx, beforeIncrementValue);
            IncrementTarget.LValue.WriteMemoryWithoutCopy(OutSnapshot, afterIncrementValue);

            if (IncDecEx.Post)
            {
                //return value before incrementation
                Value = OutSet.CreateSnapshotEntry(beforeIncrementValue);
            }
            else
            {
                //return value after incrementation
                Value = OutSet.CreateSnapshotEntry(afterIncrementValue);
            }
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitIncDec(this);
        }
    }

    /// <summary>
    /// String concatenation representation
    /// </summary>
    public class ConcatExPoint : ValuePoint
    {
        /// <summary>
        /// Concatenation expression of multiple expressions
        /// </summary>
        public readonly ConcatEx Concat;

        /// <summary>
        /// Parts of concatenated string
        /// </summary>
        public readonly IEnumerable<ValuePoint> Parts;

        /// <inheritdoc />
        public override LangElement Partial { get { return Concat; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcatExPoint" /> class.
        /// </summary>
        /// <param name="concat">Concatenation expression of multiple expressions</param>
        /// <param name="parts">Program points with expressions to concatenate</param>
        internal ConcatExPoint(ConcatEx concat, IEnumerable<ValuePoint> parts)
        {
            Parts = parts;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            var partValues = from part in Parts select part.Value.ReadMemory(OutSnapshot);
            var concatedValue = Services.Evaluator.Concat(partValues);

            Value = OutSet.CreateSnapshotEntry(concatedValue);
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitConcat(this);
        }
    }

    /// <summary>
    /// Unary expression representation
    /// </summary>
    public class UnaryExPoint : ValuePoint
    {
        /// <summary>
        /// Unary expression
        /// </summary>
        public readonly UnaryEx Expression;

        /// <summary>
        /// Operand of unary expression
        /// </summary>
        public readonly ValuePoint Operand;

        /// <inheritdoc />
        public override LangElement Partial { get { return Expression; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnaryExPoint" /> class.
        /// </summary>
        /// <param name="expression">Unary expression</param>
        /// <param name="operand">Program point with unary operand</param>
        internal UnaryExPoint(UnaryEx expression, ValuePoint operand)
        {
            Expression = expression;
            Operand = operand;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            var value = Services.Evaluator.UnaryEx(
                Expression.PublicOperation, Operand.Value.ReadMemory(OutSnapshot));

            Value = OutSet.CreateSnapshotEntry(value);
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitUnary(this);
        }
    }

    /// <summary>
    /// Binary expression representation
    /// </summary>
    public class BinaryExPoint : ValuePoint
    {
        /// <summary>
        /// Binary expression
        /// </summary>
        public readonly BinaryEx Expression;

        /// <summary>
        /// Left operand of expression
        /// </summary>
        public readonly ValuePoint LeftOperand;

        /// <summary>
        /// Right operand of expression
        /// </summary>
        public readonly ValuePoint RightOperand;

        /// <inheritdoc />
        public override LangElement Partial { get { return Expression; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryExPoint" /> class.
        /// </summary>
        /// <param name="expression">Binary expression</param>
        /// <param name="lOperand">Program point with left binary operand</param>
        /// <param name="rOperand">Program point with right binary operand</param>
        internal BinaryExPoint(BinaryEx expression, ValuePoint lOperand, ValuePoint rOperand)
        {
            Expression = expression;
            LeftOperand = lOperand;
            RightOperand = rOperand;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            var leftPoint = LeftOperand;
            var rightPoint = RightOperand;


            var leftValue = leftPoint.Value == null ? new MemoryEntry(OutSnapshot.UndefinedValue) : leftPoint.Value.ReadMemory(OutSnapshot);
            var rightValue = rightPoint.Value == null ? new MemoryEntry(OutSnapshot.UndefinedValue) : rightPoint.Value.ReadMemory(OutSnapshot);

            var value = Services.Evaluator.BinaryEx(leftValue,
                Expression.PublicOperation, rightValue);

            SetValueContent(value);
        }

        /// <summary>
        /// Set content of Value - result of binary operation
        /// </summary>
        /// <param name="valueContent">Content of value</param>
        public void SetValueContent(MemoryEntry valueContent)
        {
            Value = OutSet.CreateSnapshotEntry(valueContent);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitBinary(this);
        }
    }

    /// <summary>
    /// Conditional (ternary) expression representation
    /// </summary>
    public class ConditionalExPoint : ValuePoint
    {
        /// <summary>
        /// Conditional expression
        /// </summary>
        public readonly ConditionalEx Expression;

        /// <summary>
        /// Operand with value for true condition
        /// </summary>
        public readonly ValuePoint TrueOperand;

        /// <summary>
        /// Operand with value for false condition
        /// </summary>
        public readonly ValuePoint FalseOperand;

        /// <summary>
        /// Condition determining wheter true, false or merge should be used
        /// </summary>
        public readonly ValuePoint Condition;

        /// <summary>
        /// Assume point for true operand
        /// </summary>
        private readonly AssumePoint _trueAssume;

        /// <summary>
        /// Assume point for false operand
        /// </summary>
        private readonly AssumePoint _falseAssume;

        /// <inheritdoc />
        public override LangElement Partial { get { return Expression; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryExPoint" /> class.
        /// </summary>
        /// <param name="expression">Conditional expression</param>
        /// <param name="condition">Condition determining whether true or false, or merge will be used</param>
        /// <param name="trueAssume">Assume point with true binary operand (has to be connected with operand)</param>
        /// <param name="falseAssume">Assume point with false binary operand (has to be connected with operand)</param>
        internal ConditionalExPoint(ConditionalEx expression, ValuePoint condition, AssumePoint trueAssume, AssumePoint falseAssume)
        {
            Expression = expression;
            Condition = condition;
            _trueAssume = trueAssume;
            _falseAssume = falseAssume;

            TrueOperand = (ValuePoint)_trueAssume.FlowChildren.First();
            FalseOperand = (ValuePoint)_falseAssume.FlowChildren.First();
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            if (_trueAssume.Assumed && _falseAssume.Assumed)
            {
                //merge result from both branches
                var trueVal = TrueOperand.Value.ReadMemory(OutSnapshot);
                var falseVal = FalseOperand.Value.ReadMemory(OutSnapshot);

                var merged = MemoryEntry.Merge(trueVal, falseVal);
                Value = OutSnapshot.CreateSnapshotEntry(merged);
            }
            else if (_trueAssume.Assumed)
            {
                //only true value is used
                Value = TrueOperand.Value;
            }
            else
            {
                //only false value is used
                Value = FalseOperand.Value;
            }
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitConditional(this);
        }
    }

    /// <summary>
    /// Include expression representation
    /// </summary>
    public class IncludingExPoint : RCallPoint
    {
        /// <summary>
        /// Inclusion expression
        /// </summary>
        public readonly IncludingEx Include;

        /// <summary>
        /// Path specified for including expression
        /// </summary>
        public readonly ValuePoint IncludePath;

        /// <inheritdoc />
        public override LangElement Partial { get { return Include; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncludingExPoint" /> class.
        /// </summary>
        /// <param name="include">Inclusion expression</param>
        /// <param name="includePath">Program point with path of remote file</param>
        internal IncludingExPoint(IncludingEx include, ValuePoint includePath)
            : base(null, null, new ValuePoint[] { includePath })
        {
            Include = include;
            IncludePath = includePath;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            PrepareArguments();
            Flow.FlowResolver.Include(Flow, IncludePath.Value.ReadMemory(OutSnapshot));
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitInclude(this);
        }
    }

    /// <summary>
    /// Include expression representation
    /// </summary>
    public class EvalExPoint : RCallPoint
    {
        /// <summary>
        /// Inclusion expression
        /// </summary>
        public readonly EvalEx Eval;

        /// <summary>
        /// Source code specified for eval expression
        /// </summary>
        public readonly ValuePoint EvalCode;

        /// <inheritdoc />
        public override LangElement Partial { get { return Eval; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="EvalExPoint" /> class.
        /// </summary>
        /// <param name="eval">Eval expression</param>
        /// <param name="evalCode">Program point with source code for evaluation</param>
        internal EvalExPoint(EvalEx eval, ValuePoint evalCode)
            : base(null, null, new ValuePoint[] { evalCode })
        {
            Eval = eval;
            EvalCode = evalCode;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            PrepareArguments();
            Flow.FlowResolver.Eval(Flow, EvalCode.Value.ReadMemory(OutSnapshot));
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitEval(this);
        }
    }

    /// <summary> 
    /// Array expression representation
    /// </summary>
    public class ArrayExPoint : ValuePoint
    {
        /// <summary>
        /// Initializer values specified for created array
        /// </summary>
        private LinkedList<KeyValuePair<ValuePoint, ValuePoint>> _initializedValues;

        /// <summary>
        /// Representation of <c>array</c> construction
        /// </summary>
        public readonly ArrayEx Array;

        /// <summary>
        /// Gets initializer values specified for new array
        /// </summary>
        public IEnumerable<KeyValuePair<ValuePoint, ValuePoint>> InitializedValues
        {
            get { return _initializedValues; }
        }

        /// <inheritdoc />
        public override LangElement Partial { get { return Array; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayExPoint" /> class.
        /// </summary>
        /// <param name="array">Representation of <c>array</c> construction</param>
        /// <param name="initializedValues">Program points with items of new array</param>
        internal ArrayExPoint(ArrayEx array,
            LinkedList<KeyValuePair<ValuePoint, ValuePoint>> initializedValues)
        {
            _initializedValues = initializedValues;
            Array = array;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            var initializer = new List<KeyValuePair<MemoryEntry, MemoryEntry>>(_initializedValues.Count);

            foreach (var pair in _initializedValues)
            {
                //resolve initializing values to memory entries

                var index = pair.Key == null ? null : pair.Key.Value.ReadMemory(OutSnapshot);
                var value = pair.Value.Value.ReadMemory(OutSnapshot);
                initializer.Add(new KeyValuePair<MemoryEntry, MemoryEntry>(index, value));
            }

            var arrayValue = Services.Evaluator.ArrayEx(initializer);
            Value = OutSet.CreateSnapshotEntry(arrayValue);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitArray(this);
        }
    }

    /// <summary>
    /// New object expression representation
    /// </summary>
    public class NewExPoint : RCallPoint
    {
        /// <summary>
        /// <c>new</c> expression
        /// </summary>
        public readonly NewEx NewEx;

        /// <summary>
        /// Expression representing type name if object is created indirectly
        /// </summary>
        public readonly ValuePoint Name;

        /// <inheritdoc />
        public override LangElement Partial { get { return NewEx; } }

        /// <inheritdoc />
        /// <remarks>
        /// Explicitly hides the property inherited from the base class. It is absolutely correct to set it.
        /// </remarks>
        public override ReadSnapshotEntryBase Value { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewExPoint" /> class.
        /// </summary>
        /// <param name="newEx"><c>new</c> expression</param>
        /// <param name="name">Program point with expression that represents type name</param>
        /// <param name="arguments">Program points with arguments of the object constructor</param>
        internal NewExPoint(NewEx newEx, ValuePoint name, ValuePoint[] arguments)
            : base(null, newEx.CallSignature, arguments)
        {
            Name = name;
            NewEx = newEx;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            PrepareArguments();

            MemoryEntry value;
            //Create object according to class name
            if (Name == null)
            {
                value = Services.Evaluator.CreateObject(
                    NewEx.ClassNameRef.GenericQualifiedName.QualifiedName);
            }
            else
            {
                value = Services.Evaluator.IndirectCreateObject(Name.Value.ReadMemory(OutSnapshot));
            }

            //initialize created object
            var objectEntry = OutSet.CreateSnapshotEntry(value);
            var initializedObject = Services.FunctionResolver.InitializeObject(objectEntry, Flow.Arguments);

            Value = OutSet.CreateSnapshotEntry(initializedObject);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitNew(this);
        }
    }

    /// <summary>
    /// Representation of <c>instanceof</c> construct
    /// </summary>
    public class InstanceOfExPoint : ValuePoint
    {
        /// <summary>
        /// <c>instanceof</c> expression
        /// </summary>
        public readonly InstanceOfEx InstanceOfEx;

        /// <summary>
        /// Expression to determine whether it is instance of a specific class or interface
        /// </summary>
        public readonly ValuePoint Expression;

        /// <summary>
        /// Expression representing type name if object is created indirectly
        /// </summary>
        public readonly ValuePoint Name;

        /// <inheritdoc />
        public override LangElement Partial { get { return InstanceOfEx; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceOfExPoint" /> class.
        /// </summary>
        /// <param name="instanceOfEx"><c>instanceof</c> expression</param>
        /// <param name="expression">Program points with expression to be determined of inheritance</param>
        /// <param name="name">Program point with expression that represents type name</param>
        internal InstanceOfExPoint(InstanceOfEx instanceOfEx, ValuePoint expression, ValuePoint name)
        {
            Expression = expression;
            Name = name;
            InstanceOfEx = instanceOfEx;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            var expression = Expression.Value.ReadMemory(OutSnapshot);
            MemoryEntry value;

            if (Name == null)
            {
                value = Services.Evaluator.InstanceOfEx(expression,
                    InstanceOfEx.ClassNameRef.GenericQualifiedName.QualifiedName);
            }
            else
            {
                value = Services.Evaluator.IndirectInstanceOfEx(expression,
                    Name.Value.ReadMemory(OutSnapshot));
            }

            Value = OutSet.CreateSnapshotEntry(value);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitInstanceOf(this);
        }
    }

    /// <summary>
    /// <c>isset</c> construct representation
    /// </summary>
    public class IssetPoint : ValuePoint
    {
        /// <summary>
        /// Variables to be checked
        /// </summary>
        private readonly LValuePoint[] _variables;

        /// <summary>
        /// <c>isset</c> construct
        /// </summary>
        public readonly IssetEx Isset;

        /// <summary>
        /// Gets variables checked whether they are not set and not NULL
        /// </summary>
        public IEnumerable<LValuePoint> Variables { get { return _variables; } }

        /// <inheritdoc />
        public override LangElement Partial { get { return Isset; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="IssetPoint" /> class.
        /// </summary>
        /// <param name="isset"><c>isset</c> construct</param>
        /// <param name="variables">Variables to be checked whether they are not set and not NULL</param>
        internal IssetPoint(IssetEx isset, LValuePoint[] variables)
        {
            Isset = isset;
            _variables = variables;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            var entries = new ReadSnapshotEntryBase[_variables.Length];
            for (var i = 0; i < _variables.Length; ++i)
            {
                entries[i] = _variables[i].LValue;
            }

            var values = Services.Evaluator.IssetEx(entries);
            var value = new MemoryEntry(values);
            Value = OutSet.CreateSnapshotEntry(value);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitIsSet(this);
        }
    }

    /// <summary>
    /// Empty construct representation
    /// </summary>
    public class EmptyExPoint : ValuePoint
    {
        /// <summary>
        /// <c>empty</c> construct
        /// </summary>
        public readonly EmptyEx EmptyEx;

        /// <summary>
        /// Variable to be checked whether it is empty
        /// </summary>
        public readonly LValuePoint Variable;

        /// <inheritdoc />
        public override LangElement Partial { get { return EmptyEx; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyExPoint" /> class.
        /// </summary>
        /// <param name="emptyEx"><c>empty</c> construct</param>
        /// <param name="variable">Variable to be checked whether it is empty</param>
        internal EmptyExPoint(EmptyEx emptyEx, LValuePoint variable)
        {
            EmptyEx = emptyEx;
            Variable = variable;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            var value = Services.Evaluator.EmptyEx(Variable.LValue);

            Value = OutSet.CreateSnapshotEntry(value);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitEmptyEx(this);
        }
    }

    /// <summary>
    /// Exit call expression representation
    /// </summary>
    public class ExitExPoint : ValuePoint
    {
        /// <summary>
        /// <c>exit</c> expression
        /// </summary>
        public readonly ExitEx Exit;

        /// <summary>
        /// Exit status that is printed if it is string or returned if it is integer
        /// </summary>
        public readonly ValuePoint ResultExpression;

        /// <inheritdoc />
        public override LangElement Partial { get { return Exit; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExitExPoint" /> class.
        /// </summary>
        /// <param name="exitEx"><c>exit</c> expression</param>
        /// <param name="resultExpression">Program point with exit status</param>
        internal ExitExPoint(ExitEx exitEx, ValuePoint resultExpression)
        {
            Exit = exitEx;
            ResultExpression = resultExpression;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            MemoryEntry result;
            if (ResultExpression == null)
            {
                result = new MemoryEntry(OutSet.UndefinedValue);
            }
            else
            {
                result = ResultExpression.Value.ReadMemory(OutSnapshot);
            }
            var value = Services.Evaluator.Exit(Exit, result);

            Value = OutSet.CreateSnapshotEntry(value);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitExit(this);
        }
    }
}
