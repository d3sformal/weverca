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
    /// String concatenation representation
    /// </summary>
    public class IncDecExPoint : ValuePoint
    {
        public readonly IncDecEx IncDecEx;

        public override LangElement Partial { get { return IncDecEx; } }

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

        internal IncDecExPoint(IncDecEx incDecEx, ValuePoint incrementedValue)
        {            
            NeedsExpressionEvaluator = true;
            IncDecEx = incDecEx;
            IncrementedValue = incrementedValue;
            IncrementTarget = incrementedValue as LValuePoint;

            if (IncrementTarget == null)
            {
                throw new NotSupportedException("Given incrementedValue doesn't support incrementation");
            }
        }

        protected override void flowThrough()
        {
            var beforeIncrementValue= IncrementedValue.Value.ReadMemory(InSet.Snapshot);
            var afterIncrementValue = Services.Evaluator.IncDecEx(IncDecEx,beforeIncrementValue);
            Services.Evaluator.Assign(IncrementTarget.LValue, afterIncrementValue);

            if (IncDecEx.Post)
            {
                //return value before incrementation
                Value = OutSet.CreateSnapshotEntry(beforeIncrementValue);
            }
            else
            {
                //return value after incrementation
                Value=OutSet.CreateSnapshotEntry(afterIncrementValue);
            }
        }
    }

    /// <summary>
    /// String concatenation representation
    /// </summary>
    public class ConcatExPoint : ValuePoint
    {
        public readonly ConcatEx Concat;

        public override LangElement Partial { get { return Concat; } }

        /// <summary>
        /// Parts of concatenated string
        /// </summary>
        public readonly IEnumerable<ValuePoint> Parts;

        internal ConcatExPoint(ConcatEx concat, IEnumerable<ValuePoint> parts)
        {
            NeedsExpressionEvaluator = true;
            Parts = parts;
        }

        protected override void flowThrough()
        {
            var partValues = from part in Parts select part.Value.ReadMemory(InSnapshot);
            var concatedValue=Services.Evaluator.Concat(partValues);
            Value = OutSet.CreateSnapshotEntry(concatedValue);
        }
    }

    /// <summary>
    /// Unary expression representation
    /// </summary>
    public class UnaryExPoint : ValuePoint
    {
        public readonly UnaryEx Expression;

        /// <summary>
        /// Operand of unary expression
        /// </summary>
        public readonly ValuePoint Operand;

        public override LangElement Partial { get { return Expression; } }

        internal UnaryExPoint(UnaryEx expression, ValuePoint operand)
        {
            NeedsExpressionEvaluator = true;

            Expression = expression;
            Operand = operand;
        }

        protected override void flowThrough()
        {
            var value=Services.Evaluator.UnaryEx(Expression.PublicOperation, Operand.Value.ReadMemory(InSnapshot));
            Value = OutSet.CreateSnapshotEntry(value);
        }
    }

    /// <summary>
    /// Binary expression representation
    /// </summary>
    public class BinaryExPoint : ValuePoint
    {
        public readonly BinaryEx Expression;

        /// <summary>
        /// Left operand of expression
        /// </summary>
        public readonly ValuePoint LeftOperand;

        /// <summary>
        /// Right operand of expression
        /// </summary>
        public readonly ValuePoint RightOperand;

        public override LangElement Partial { get { return Expression; } }

        internal BinaryExPoint(BinaryEx expression, ValuePoint lOperand, ValuePoint rOperand)
        {
            NeedsExpressionEvaluator = true;

            Expression = expression;
            LeftOperand = lOperand;
            RightOperand = rOperand;
        }

        protected override void flowThrough()
        {
            var value=Services.Evaluator.BinaryEx(LeftOperand.Value.ReadMemory(InSnapshot), Expression.PublicOperation, RightOperand.Value.ReadMemory(InSnapshot));
            Value = OutSet.CreateSnapshotEntry(value);
        }
    }

    /// <summary>
    /// Include expression representation
    /// </summary>
    public class IncludingExPoint : RCallPoint
    {
        public readonly IncludingEx Include;

        /// <summary>
        /// Path specified for including expression
        /// </summary>
        public readonly ValuePoint IncludePath;

        public override LangElement Partial { get { return Include; } }


        internal IncludingExPoint(IncludingEx include, ValuePoint includePath)
            : base(null, null, new ValuePoint[] { includePath })
        {
            NeedsFlowResolver = true;

            Include = include;
            IncludePath = includePath;
        }

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
        public readonly ArrayEx Array;

        public override LangElement Partial { get { return Array; } }

        /// <summary>
        /// Initializer values specified for created array
        /// </summary>
        private LinkedList<KeyValuePair<ValuePoint, ValuePoint>> _initializedValues;

        public ArrayExPoint(ArrayEx array, LinkedList<KeyValuePair<ValuePoint, ValuePoint>> initializedValues)
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
                //resolve initializing values to memory entries

                var index = pair.Key == null ? null : pair.Key.Value.ReadMemory(InSnapshot);
                var value = pair.Value.Value.ReadMemory(InSnapshot);
                initializer.Add(new KeyValuePair<MemoryEntry, MemoryEntry>(index, value));
            }

            var arrayValue=Services.Evaluator.ArrayEx(initializer);
            Value = OutSet.CreateSnapshotEntry(arrayValue);
        }
    }

    /// <summary>
    /// New object expression representation
    /// </summary>
    public class NewExPoint : RCallPoint
    {
        public readonly NewEx NewEx;

        public readonly ValuePoint Name;

        public override LangElement Partial { get { return NewEx; } }

        public override ReadSnapshotEntryBase Value { get; protected set; }

        internal NewExPoint(NewEx newEx, ValuePoint name, ValuePoint[] arguments)
            : base(null, newEx.CallSignature, arguments)
        {
            NeedsFunctionResolver = true;
            NeedsExpressionEvaluator = true;
            Name = name;

            NewEx = newEx;
        }

        protected override void flowThrough()
        {
            PrepareArguments();


            MemoryEntry value;
            //Create object according to class name
            if (Name == null)
            {
                value = Services.Evaluator.CreateObject(NewEx.ClassNameRef.GenericQualifiedName.QualifiedName);
            }
            else
            {
                value= Services.Evaluator.IndirectCreateObject(Name.Value.ReadMemory(InSnapshot));
            }

            //initialize created object
            var initializedObject = Services.FunctionResolver.InitializeObject(value, Flow.Arguments);

            Value = OutSet.CreateSnapshotEntry(initializedObject);
        }
    }

}
