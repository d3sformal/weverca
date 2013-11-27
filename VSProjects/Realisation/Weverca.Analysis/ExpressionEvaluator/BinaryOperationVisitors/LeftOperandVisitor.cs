using System;
using System.Collections.Generic;
using System.Globalization;

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
    /// </list>
    /// The <see cref="Operations.Concat" /> is provided by <see cref="StringConverter" />
    /// </remarks>
    public abstract class LeftOperandVisitor : PartialExpressionEvaluator
    {
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
            : base(flowController) { }

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

        #region AbstractValueVisitor Members

        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            throw new InvalidOperationException("Resolving of non-binary operation");
        }

        #endregion AbstractValueVisitor Members

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
            switch (operation)
            {
                case Operations.Equal:
                    result = OutSet.CreateBool(leftOperand == rightOperand);
                    return true;
                case Operations.NotEqual:
                    result = OutSet.CreateBool(leftOperand != rightOperand);
                    return true;
                case Operations.LessThan:
                    result = OutSet.CreateBool((!leftOperand) && rightOperand);
                    return true;
                case Operations.LessThanOrEqual:
                    result = OutSet.CreateBool((!leftOperand) || rightOperand);
                    return true;
                case Operations.GreaterThan:
                    result = OutSet.CreateBool(leftOperand && (!rightOperand));
                    return true;
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(leftOperand || (!rightOperand));
                    return true;
                default:
                    return false;
            }
        }

        protected bool ComparisonOperation(int leftOperand, int rightOperand)
        {
            switch (operation)
            {
                case Operations.Equal:
                    result = OutSet.CreateBool(leftOperand == rightOperand);
                    return true;
                case Operations.NotEqual:
                    result = OutSet.CreateBool(leftOperand != rightOperand);
                    return true;
                case Operations.LessThan:
                    result = OutSet.CreateBool(leftOperand < rightOperand);
                    return true;
                case Operations.LessThanOrEqual:
                    result = OutSet.CreateBool(leftOperand <= rightOperand);
                    return true;
                case Operations.GreaterThan:
                    result = OutSet.CreateBool(leftOperand > rightOperand);
                    return true;
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(leftOperand >= rightOperand);
                    return true;
                default:
                    return false;
            }
        }

        protected bool ComparisonOperation(IntegerIntervalValue leftOperand, int rightOperand)
        {
            switch (operation)
            {
                case Operations.Equal:
                    if ((leftOperand.Start <= rightOperand) && (leftOperand.End >= rightOperand))
                    {
                        if ((leftOperand.Start == rightOperand) && (leftOperand.End == rightOperand))
                        {
                            result = OutSet.CreateBool(true);
                        }
                        else
                        {
                            result = OutSet.AnyBooleanValue;
                        }
                    }
                    else
                    {
                        result = OutSet.CreateBool(false);
                    }
                    return true;
                case Operations.NotEqual:
                    if ((leftOperand.Start > rightOperand) || (leftOperand.End < rightOperand))
                    {
                        result = OutSet.CreateBool(true);
                    }
                    else
                    {
                        if ((leftOperand.Start == rightOperand) && (leftOperand.End == rightOperand))
                        {
                            result = OutSet.CreateBool(false);
                        }
                        else
                        {
                            result = OutSet.AnyBooleanValue;
                        }
                    }
                    return true;
                case Operations.LessThan:
                    if (leftOperand.End < rightOperand)
                    {
                        result = OutSet.CreateBool(true);
                    }
                    else
                    {
                        if (leftOperand.Start >= rightOperand)
                        {
                            result = OutSet.CreateBool(false);
                        }
                        else
                        {
                            result = OutSet.AnyBooleanValue;
                        }
                    }
                    return true;
                case Operations.LessThanOrEqual:
                    if (leftOperand.End <= rightOperand)
                    {
                        result = OutSet.CreateBool(true);
                    }
                    else
                    {
                        if (leftOperand.Start > rightOperand)
                        {
                            result = OutSet.CreateBool(false);
                        }
                        else
                        {
                            result = OutSet.AnyBooleanValue;
                        }
                    }
                    return true;
                case Operations.GreaterThan:
                    if (leftOperand.Start > rightOperand)
                    {
                        result = OutSet.CreateBool(true);
                    }
                    else
                    {
                        if (leftOperand.End <= rightOperand)
                        {
                            result = OutSet.CreateBool(false);
                        }
                        else
                        {
                            result = OutSet.AnyBooleanValue;
                        }
                    }
                    return true;
                case Operations.GreaterThanOrEqual:
                    if (leftOperand.Start >= rightOperand)
                    {
                        result = OutSet.CreateBool(true);
                    }
                    else
                    {
                        if (leftOperand.End < rightOperand)
                        {
                            result = OutSet.CreateBool(false);
                        }
                        else
                        {
                            result = OutSet.AnyBooleanValue;
                        }
                    }
                    return true;
                default:
                    return false;
            }
        }

        protected bool ComparisonOperation(double leftOperand, double rightOperand)
        {
            switch (operation)
            {
                case Operations.Equal:
                    SetWarning("Comparing floating-point numbers directly for equality");
                    result = OutSet.CreateBool(leftOperand == rightOperand);
                    return true;
                case Operations.NotEqual:
                    SetWarning("Comparing floating-point numbers directly for non-equality");
                    result = OutSet.CreateBool(leftOperand != rightOperand);
                    return true;
                case Operations.LessThan:
                    result = OutSet.CreateBool(leftOperand < rightOperand);
                    return true;
                case Operations.LessThanOrEqual:
                    result = OutSet.CreateBool(leftOperand <= rightOperand);
                    return true;
                case Operations.GreaterThan:
                    result = OutSet.CreateBool(leftOperand > rightOperand);
                    return true;
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(leftOperand >= rightOperand);
                    return true;
                default:
                    return false;
            }
        }

        protected bool ComparisonOperation(string leftOperand, string rightOperand)
        {
            switch (operation)
            {
                case Operations.Equal:
                    result = OutSet.CreateBool(leftOperand == rightOperand);
                    return true;
                case Operations.NotEqual:
                    result = OutSet.CreateBool(leftOperand != rightOperand);
                    return true;
                case Operations.LessThan:
                    result = OutSet.CreateBool(string.Compare(leftOperand, rightOperand) < 0);
                    return true;
                case Operations.LessThanOrEqual:
                    result = OutSet.CreateBool(string.Compare(leftOperand, rightOperand) <= 0);
                    return true;
                case Operations.GreaterThan:
                    result = OutSet.CreateBool(string.Compare(leftOperand, rightOperand) > 0);
                    return true;
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(string.Compare(leftOperand, rightOperand) >= 0);
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
                    var product = System.Convert.ToInt64(leftOperand,
                        CultureInfo.InvariantCulture) * rightOperand;
                    if ((product >= int.MinValue) && (product <= int.MaxValue))
                    {
                        result = OutSet.CreateInt(System.Convert.ToInt32(product,
                            CultureInfo.InvariantCulture));
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

        protected void ModuloOperation(int leftOperand, int rightOperand)
        {
            if (rightOperand != 0)
            {
                // Value has the same sign as dividend
                result = OutSet.CreateInt(leftOperand % rightOperand);
            }
            else
            {
                SetWarning("Division (modulo) by zero",
                    AnalysisWarningCause.DIVISION_BY_ZERO);
                // Division (modulo) by zero returns false boolean value
                result = OutSet.CreateBool(false);
            }
        }

        protected bool BitwiseOperation(int leftOperand, int rightOperand)
        {
            switch (operation)
            {
                case Operations.BitAnd:
                    result = OutSet.CreateInt(leftOperand & rightOperand);
                    return true;
                case Operations.BitOr:
                    result = OutSet.CreateInt(leftOperand | rightOperand);
                    return true;
                case Operations.BitXor:
                    result = OutSet.CreateInt(leftOperand ^ rightOperand);
                    return true;
                case Operations.ShiftLeft:
                    result = OutSet.CreateInt(leftOperand << rightOperand);
                    return true;
                case Operations.ShiftRight:
                    result = OutSet.CreateInt(leftOperand >> rightOperand);
                    return true;
                default:
                    return false;
            }
        }

        protected bool LogicalOperation(bool leftOperand, bool rightOperand)
        {
            switch (operation)
            {
                case Operations.And:
                    result = OutSet.CreateBool(leftOperand && rightOperand);
                    return true;
                case Operations.Or:
                    result = OutSet.CreateBool(leftOperand || rightOperand);
                    return true;
                case Operations.Xor:
                    result = OutSet.CreateBool(leftOperand != rightOperand);
                    return true;
                default:
                    return false;
            }
        }

        protected bool LogicalOperation<T>(bool leftOperand, IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;

            switch (operation)
            {
                case Operations.And:
                    if (TypeConversion.TryConvertToBoolean<T>(rightOperand, out convertedValue))
                    {
                        result = OutSet.CreateBool(leftOperand && convertedValue);
                    }
                    else
                    {
                        result = OutSet.AnyBooleanValue;
                    }
                    return true;
                case Operations.Or:
                    if (TypeConversion.TryConvertToBoolean<T>(rightOperand, out convertedValue))
                    {
                        result = OutSet.CreateBool(leftOperand || convertedValue);
                    }
                    else
                    {
                        result = OutSet.AnyBooleanValue;
                    }
                    return true;
                case Operations.Xor:
                    if (TypeConversion.TryConvertToBoolean<T>(rightOperand, out convertedValue))
                    {
                        result = OutSet.CreateBool(leftOperand != convertedValue);
                    }
                    else
                    {
                        result = OutSet.AnyBooleanValue;
                    }
                    return true;
                default:
                    return false;
            }
        }

        #endregion Helper methods
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
        /// Initializes a new instance of the <see cref="GenericLeftOperandVisitor{T}" /> class.
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
