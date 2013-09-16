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
    /// Unary expression representation
    /// </summary>
    public class UnaryExPoint : RValuePoint
    {
        public readonly UnaryEx Expression;

        /// <summary>
        /// Operand of unary expression
        /// </summary>
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

    /// <summary>
    /// Binary expression representation
    /// </summary>
    public class BinaryExPoint : RValuePoint
    {
        public readonly BinaryEx Expression;

        /// <summary>
        /// Left operand of expression
        /// </summary>
        public readonly RValuePoint LeftOperand;

        /// <summary>
        /// Right operand of expression
        /// </summary>
        public readonly RValuePoint RightOperand;

        public override LangElement Partial { get { return Expression; } }

        internal BinaryExPoint(BinaryEx expression, RValuePoint lOperand, RValuePoint rOperand)
        {
            NeedsExpressionEvaluator = true;

            Expression = expression;
            LeftOperand = lOperand;
            RightOperand = rOperand;
        }

        protected override void flowThrough()
        {
            Value = Services.Evaluator.BinaryEx(LeftOperand.Value, Expression.PublicOperation, RightOperand.Value);
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
        public readonly RValuePoint IncludePath;

        public override LangElement Partial { get { return Include; } }


        internal IncludingExPoint(IncludingEx include, RValuePoint includePath)
            : base(null, new RValuePoint[] { includePath })
        {
            NeedsFlowResolver = true;

            Include = include;
            IncludePath = includePath;
        }

        protected override void flowThrough()
        {
            PrepareArguments();
            Flow.FlowResolver.Include(Flow, IncludePath.Value);
        }
    }

    /// <summary>
    /// Array expression representation
    /// </summary>
    public class ArrayExPoint : RValuePoint
    {
        public readonly ArrayEx Array;

        public override LangElement Partial { get { return Array; } }

        /// <summary>
        /// Initializer values specified for created array
        /// </summary>
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
                //resolve initializing values to memory entries

                var index = pair.Key == null ? null : pair.Key.Value;
                var value = pair.Value.Value;
                initializer.Add(new KeyValuePair<MemoryEntry, MemoryEntry>(index, value));
            }

            Value = Services.Evaluator.ArrayEx(initializer);
        }
    }

    /// <summary>
    /// New object expression representation
    /// </summary>
    public class NewExPoint : RCallPoint
    {
        public readonly NewEx NewEx;

        public readonly RValuePoint Name;

        public override LangElement Partial { get { return NewEx; } }

        public override MemoryEntry Value { get; protected set; }

        internal NewExPoint(NewEx newEx,RValuePoint name, RValuePoint[] arguments)
            : base(null, arguments)
        {
            NeedsFunctionResolver = true;
            NeedsExpressionEvaluator = true;
            Name = name;

            NewEx = newEx;
        }

        protected override void flowThrough()
        {
            PrepareArguments();


            //Create object according to class name
            if (Name == null)
            {
                Value = Services.Evaluator.CreateObject(NewEx.ClassNameRef.GenericQualifiedName.QualifiedName);
            }
            else
            {
                Value = Services.Evaluator.IndirectCreateObject(Name.Value);
            }

            //initialize created object
            Value = Services.FunctionResolver.InitializeObject(Value, Flow.Arguments);
        }
    }

}
