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
            var beforeIncrementValue = IncrementedValue.Value.ReadMemory(InSet.Snapshot);
            var afterIncrementValue = Services.Evaluator.IncDecEx(IncDecEx, beforeIncrementValue);
            Services.Evaluator.Assign(IncrementTarget.LValue, afterIncrementValue);

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
            var partValues = from part in Parts select part.Value.ReadMemory(InSnapshot);
            var concatedValue = Services.Evaluator.Concat(partValues);

            Value = OutSet.CreateSnapshotEntry(concatedValue);
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
                Expression.PublicOperation, Operand.Value.ReadMemory(InSnapshot));

            Value = OutSet.CreateSnapshotEntry(value);
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
            var value = Services.Evaluator.BinaryEx(LeftOperand.Value.ReadMemory(InSnapshot),
                Expression.PublicOperation, RightOperand.Value.ReadMemory(InSnapshot));

            Value = OutSet.CreateSnapshotEntry(value);
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
            Flow.FlowResolver.Include(Flow, IncludePath.Value.ReadMemory(InSnapshot));
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

                var index = pair.Key == null ? null : pair.Key.Value.ReadMemory(InSnapshot);
                var value = pair.Value.Value.ReadMemory(InSnapshot);
                initializer.Add(new KeyValuePair<MemoryEntry, MemoryEntry>(index, value));
            }

            var arrayValue = Services.Evaluator.ArrayEx(initializer);
            Value = OutSet.CreateSnapshotEntry(arrayValue);
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
                value = Services.Evaluator.IndirectCreateObject(Name.Value.ReadMemory(InSnapshot));
            }

            //initialize created object
            var initializedObject = Services.FunctionResolver.InitializeObject(value, Flow.Arguments);

            Value = OutSet.CreateSnapshotEntry(initializedObject);
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
            var expression = Expression.Value.ReadMemory(InSet.Snapshot);
            MemoryEntry value;

            if (Name == null)
            {
                value = Services.Evaluator.InstanceOfEx(expression,
                    InstanceOfEx.ClassNameRef.GenericQualifiedName.QualifiedName);
            }
            else
            {
                value = Services.Evaluator.IndirectInstanceOfEx(expression,
                    Name.Value.ReadMemory(InSnapshot));
            }

            Value = OutSet.CreateSnapshotEntry(value);
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
            var variables = new VariableIdentifier[_variables.Length];
            for (var i = 0; i < _variables.Length; ++i)
            {
                variables[i] = _variables[i].LValue.GetVariableIdentifier(InSet.Snapshot);
            }

            var value = Services.Evaluator.IssetEx(variables);
            Value = OutSet.CreateSnapshotEntry(value);
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
            var variable = Variable.LValue.GetVariableIdentifier(InSet.Snapshot);
            var value = Services.Evaluator.EmptyEx(variable);

            Value = OutSet.CreateSnapshotEntry(value);
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
            var result = ResultExpression.Value.ReadMemory(InSnapshot);
            var value = Services.Evaluator.Exit(Exit, result);

            Value = OutSet.CreateSnapshotEntry(value);
        }
    }
}
