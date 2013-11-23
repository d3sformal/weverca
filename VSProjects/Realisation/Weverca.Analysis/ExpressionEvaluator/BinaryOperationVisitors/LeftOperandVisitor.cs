using System;
using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation with fixed left operand during the analysis
    /// </summary>
    /// <remarks>
    /// The visitor must resolve only the right operand, left operand of a concrete type is set
    /// in a derived class. The class can evaluate the following binary operations:
    /// <list type="bullet">
    /// <item><term><see cref="Operations.Equal" /></term></item>
    /// <item><term><see cref="Operations.Identical" /></term></item>
    /// <item><term><see cref="Operations.NotEqual" /></term></item>
    /// <item><term><see cref="Operations.NotIdentical" /></term></item>
    /// <item><term><see cref="Operations.LessThan" /></term></item>
    /// <item><term><see cref="Operations.LessThanOrEqual" /></term></item>
    /// <item><term><see cref="Operations.GreaterThan" /></term></item>
    /// <item><term><see cref="Operations.GreaterThanOrEqual" /></term></item>
    /// <item><term><see cref="Operations.Add" /></term></item>
    /// <item><term><see cref="Operations.Sub" /></term></item>
    /// <item><term><see cref="Operations.Mul" /></term></item>
    /// <item><term><see cref="Operations.Div" /></term></item>
    /// <item><term><see cref="Operations.Mod" /></term></item>
    /// <item><term><see cref="Operations.And" /></term></item>
    /// <item><term><see cref="Operations.Or" /></term></item>
    /// <item><term><see cref="Operations.Xor" /></term></item>
    /// <item><term><see cref="Operations.BitAnd" /></term></item>
    /// <item><term><see cref="Operations.BitOr" /></term></item>
    /// <item><term><see cref="Operations.BitXor" /></term></item>
    /// <item><term><see cref="Operations.ShiftLeft" /></term></item>
    /// <item><term><see cref="Operations.ShiftRight" /></term></item>
    /// <item><term><see cref="Operations.Concat" /></term></item>
    /// </list>
    /// </remarks>
    public abstract class LeftOperandVisitor : AbstractValueVisitor
    {
        /// <summary>
        /// Flow controller of program point providing data for evaluation (output set, position etc.)
        /// </summary>
        protected FlowController flow;

        /// <summary>
        /// Binary operation that determines the proper action with operands
        /// </summary>
        protected Operations operation;

        /// <summary>
        /// Result of performing the binary operation of the left and right operand
        /// </summary>
        protected Value result;

        /// <summary>
        /// Initializes a new instance of the <see cref="LeftOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        public LeftOperandVisitor(FlowController flowController)
        {
            SetContext(flowController);
        }

        /// <summary>
        /// Gets output set of a program point
        /// </summary>
        public FlowOutputSet OutSet
        {
            get { return flow.OutSet; }
        }

        /// <summary>
        /// Evaluates binary operation with left operand of this visitor and the given right operand
        /// </summary>
        /// <param name="binaryOperation">Binary operation to be performed</param>
        /// <param name="rightOperand">The right operand of binary operation</param>
        /// <returns>Result of performing the binary operation on the operands</returns>
        public Value Evaluate(Operations binaryOperation, Value rightOperand)
        {
            // Sets current operation
            operation = binaryOperation;

            // Gets type of right operand and evaluate expression for given operation
            result = null;
            rightOperand.Accept(this);

            // Returns result of binary operation
            Debug.Assert(result != null, "The result must be assigned after visiting the value");
            return result;
        }

        /// <summary>
        /// Evaluates binary operation with one left operand and all possible values of right operand
        /// </summary>
        /// <param name="binaryOperation">Binary operation to be performed</param>
        /// <param name="rightOperand">Entry with all possible right operands of binary operation</param>
        /// <returns>Resulting entry after performing the binary operation on all possible operands</returns>
        public MemoryEntry Evaluate(Operations binaryOperation, MemoryEntry rightOperand)
        {
            // Sets current operation
            operation = binaryOperation;

            var values = new HashSet<Value>();
            foreach (var value in rightOperand.PossibleValues)
            {
                // Gets type of right operand and evaluate expression for given operation
                result = null;
                value.Accept(this);

                // Returns result of binary operation
                Debug.Assert(result != null, "The result must be assigned after visiting the value");
                values.Add(result);
            }

            return new MemoryEntry(values);
        }

        /// <summary>
        /// Set current evaluation context.
        /// </summary>
        /// <param name="flowController">Flow controller of program point available for evaluation</param>
        public void SetContext(FlowController flowController)
        {
            flow = flowController;
        }

        /// <summary>
        /// Report a warning for the position of current expression
        /// </summary>
        /// <param name="message">Message of the warning</param>
        protected void SetWarning(string message)
        {
            var warning = new AnalysisWarning(message, flow.CurrentPartial);
            AnalysisWarningHandler.SetWarning(OutSet, warning);
        }

        /// <summary>
        /// Report a warning for the position of current expression
        /// </summary>
        /// <param name="message">Message of the warning</param>
        /// <param name="cause">Cause of the warning</param>
        protected void SetWarning(string message, AnalysisWarningCause cause)
        {
            var warning = new AnalysisWarning(message, flow.CurrentPartial, cause);
            AnalysisWarningHandler.SetWarning(OutSet, warning);
        }

        #region IValueVisitor Members

        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            throw new InvalidOperationException("Resolving of non-binary operation");
        }

        #endregion

        #region Helper methods

        protected bool IsOperationComparison()
        {
            return (operation == Operations.Equal)
                || (operation == Operations.NotEqual)
                || (operation == Operations.LessThan)
                || (operation == Operations.LessThanOrEqual)
                || (operation == Operations.GreaterThan)
                || (operation == Operations.GreaterThanOrEqual);
        }

        protected bool IsOperationBitwise()
        {
            return (operation == Operations.BitAnd)
                || (operation == Operations.BitOr)
                || (operation == Operations.BitXor)
                || (operation == Operations.ShiftLeft)
                || (operation == Operations.ShiftRight);
        }

        protected bool IsLogicalBitwise()
        {
            return (operation == Operations.And)
                || (operation == Operations.Or)
                || (operation == Operations.Xor);
        }

        protected bool ComparisonOperation(bool leftOperand, bool rightOperand)
        {
            var outSet = OutSet;

            switch (operation)
            {
                case Operations.Equal:
                    result = outSet.CreateBool(leftOperand == rightOperand);
                    return true;
                case Operations.NotEqual:
                    result = outSet.CreateBool(leftOperand != rightOperand);
                    return true;
                case Operations.LessThan:
                    result = outSet.CreateBool((!leftOperand) && rightOperand);
                    return true;
                case Operations.LessThanOrEqual:
                    result = outSet.CreateBool((!leftOperand) || rightOperand);
                    return true;
                case Operations.GreaterThan:
                    result = outSet.CreateBool(leftOperand && (!rightOperand));
                    return true;
                case Operations.GreaterThanOrEqual:
                    result = outSet.CreateBool(leftOperand || (!rightOperand));
                    return true;
                default:
                    return false;
            }
        }

        protected bool ComparisonOperation(int leftOperand, int rightOperand)
        {
            var outSet = OutSet;

            switch (operation)
            {
                case Operations.Equal:
                    result = outSet.CreateBool(leftOperand == rightOperand);
                    return true;
                case Operations.NotEqual:
                    result = outSet.CreateBool(leftOperand != rightOperand);
                    return true;
                case Operations.LessThan:
                    result = outSet.CreateBool(leftOperand < rightOperand);
                    return true;
                case Operations.LessThanOrEqual:
                    result = outSet.CreateBool(leftOperand <= rightOperand);
                    return true;
                case Operations.GreaterThan:
                    result = outSet.CreateBool(leftOperand > rightOperand);
                    return true;
                case Operations.GreaterThanOrEqual:
                    result = outSet.CreateBool(leftOperand >= rightOperand);
                    return true;
                default:
                    return false;
            }
        }

        protected bool ComparisonOperation(double leftOperand, double rightOperand)
        {
            var outSet = OutSet;

            switch (operation)
            {
                case Operations.Equal:
                    SetWarning("Comparing floating-point numbers directly for equality");
                    result = outSet.CreateBool(leftOperand == rightOperand);
                    return true;
                case Operations.NotEqual:
                    SetWarning("Comparing floating-point numbers directly for non-equality");
                    result = outSet.CreateBool(leftOperand != rightOperand);
                    return true;
                case Operations.LessThan:
                    result = outSet.CreateBool(leftOperand < rightOperand);
                    return true;
                case Operations.LessThanOrEqual:
                    result = outSet.CreateBool(leftOperand <= rightOperand);
                    return true;
                case Operations.GreaterThan:
                    result = outSet.CreateBool(leftOperand > rightOperand);
                    return true;
                case Operations.GreaterThanOrEqual:
                    result = outSet.CreateBool(leftOperand >= rightOperand);
                    return true;
                default:
                    return false;
            }
        }

        protected bool ComparisonOperation(string leftOperand, string rightOperand)
        {
            var outSet = OutSet;

            switch (operation)
            {
                case Operations.Equal:
                    result = outSet.CreateBool(leftOperand == rightOperand);
                    return true;
                case Operations.NotEqual:
                    result = outSet.CreateBool(leftOperand != rightOperand);
                    return true;
                case Operations.LessThan:
                    result = outSet.CreateBool(String.Compare(leftOperand, rightOperand) < 0);
                    return true;
                case Operations.LessThanOrEqual:
                    result = outSet.CreateBool(String.Compare(leftOperand, rightOperand) <= 0);
                    return true;
                case Operations.GreaterThan:
                    result = outSet.CreateBool(String.Compare(leftOperand, rightOperand) > 0);
                    return true;
                case Operations.GreaterThanOrEqual:
                    result = outSet.CreateBool(String.Compare(leftOperand, rightOperand) >= 0);
                    return true;
                default:
                    return false;
            }
        }

        protected bool ArithmeticOperation(int leftOperand, int rightOperand)
        {
            switch (operation)
            {
                case Operations.Add:
                    // Result of addition can overflow or underflow
                    if (((rightOperand >= 0) && (leftOperand <= int.MaxValue - rightOperand))
                        || ((rightOperand < 0) && (leftOperand >= int.MinValue - rightOperand)))
                    {
                        result = OutSet.CreateInt(leftOperand + rightOperand);
                    }
                    else
                    {
                        // If aritmetic overflows or underflows, result is double
                        result = OutSet.CreateDouble(TypeConversion.ToFloat(leftOperand) + rightOperand);
                    }
                    return true;
                case Operations.Sub:
                    // Result of addition can underflow or underflow
                    if (((rightOperand >= 0) && (leftOperand >= int.MinValue + rightOperand))
                        || ((rightOperand < 0) && (leftOperand <= int.MaxValue + rightOperand)))
                    {
                        result = OutSet.CreateInt(leftOperand - rightOperand);
                    }
                    else
                    {
                        // If aritmetic overflows or underflows, result is double
                        result = OutSet.CreateDouble(TypeConversion.ToFloat(leftOperand) - rightOperand);
                    }
                    return true;
                case Operations.Mul:
                    // Result of addition can overflow or underflow
                    // TODO: Find more cleaner solution ((a * b <= c) <==> (a <= c / b))
                    var product = System.Convert.ToInt64(leftOperand) * rightOperand;
                    if ((product >= int.MinValue) && (product <= int.MaxValue))
                    {
                        result = OutSet.CreateInt(System.Convert.ToInt32(product));
                    }
                    else
                    {
                        // If aritmetic overflows or underflows, result is double
                        result = OutSet.CreateDouble(TypeConversion.ToFloat(product));
                    }
                    return true;
                case Operations.Div:
                    if (rightOperand != 0)
                    {
                        if ((leftOperand % rightOperand) == 0)
                        {
                            result = OutSet.CreateInt(leftOperand / rightOperand);
                        }
                        else
                        {
                            result = OutSet.CreateDouble(TypeConversion.ToFloat(leftOperand) / rightOperand);
                        }
                    }
                    else
                    {
                        SetWarning("Division by zero", AnalysisWarningCause.DIVISION_BY_ZERO);
                        // Division by zero returns false boolean value
                        result = OutSet.CreateBool(false);
                    }
                    return true;
                default:
                    return false;
            }
        }

        protected bool ArithmeticOperation(double leftOperand, double rightOperand)
        {
            switch (operation)
            {
                case Operations.Add:
                    result = OutSet.CreateDouble(leftOperand + rightOperand);
                    return true;
                case Operations.Sub:
                    result = OutSet.CreateDouble(leftOperand - rightOperand);
                    return true;
                case Operations.Mul:
                    result = OutSet.CreateDouble(leftOperand * rightOperand);
                    return true;
                case Operations.Div:
                    if (rightOperand != 0.0)
                    {
                        result = OutSet.CreateDouble(leftOperand / rightOperand);
                    }
                    else
                    {
                        SetWarning("Division by floating-point zero",
                            AnalysisWarningCause.DIVISION_BY_ZERO);
                        // Division by floating-point zero does not return NaN, but false boolean value
                        result = OutSet.CreateBool(false);
                    }
                    return true;
                default:
                    return false;
            }
        }

        protected bool BitwiseOperation(int leftOperand, int rightOperand)
        {
            var outSet = OutSet;

            switch (operation)
            {
                case Operations.BitAnd:
                    result = outSet.CreateInt(leftOperand & rightOperand);
                    return true;
                case Operations.BitOr:
                    result = outSet.CreateInt(leftOperand | rightOperand);
                    return true;
                case Operations.BitXor:
                    result = outSet.CreateInt(leftOperand ^ rightOperand);
                    return true;
                case Operations.ShiftLeft:
                    result = outSet.CreateInt(leftOperand << rightOperand);
                    return true;
                case Operations.ShiftRight:
                    result = outSet.CreateInt(leftOperand >> rightOperand);
                    return true;
                default:
                    return false;
            }
        }

        protected bool LogicalOperation(bool leftOperand, bool rightOperand)
        {
            var outSet = OutSet;

            switch (operation)
            {
                case Operations.And:
                    result = outSet.CreateBool(leftOperand && rightOperand);
                    return true;
                case Operations.Or:
                    result = outSet.CreateBool(leftOperand || rightOperand);
                    return true;
                case Operations.Xor:
                    result = outSet.CreateBool(leftOperand != rightOperand);
                    return true;
                default:
                    return false;
            }
        }

        #endregion
    }

    /// <summary>
    /// Evaluates one binary operation with typed fixed left operand during the analysis
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />
    /// </remarks>
    /// <typeparam name="T">Type of left operand</typeparam>
    public abstract class GenericLeftOperandVisitor<T> : LeftOperandVisitor where T : Value
    {
        /// <summary>
        /// A value of specified type representing the left operand of binary operation
        /// </summary>
        protected T leftOperand;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericLeftOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        public GenericLeftOperandVisitor(FlowController flowController)
            : base(flowController)
        {
        }

        /// <summary>
        /// Set a value of specified type as left operand of binary operation
        /// </summary>
        /// <param name="value">A concrete integer value</param>
        public void SetLeftOperand(T value)
        {
            leftOperand = value;
        }
    }
}
