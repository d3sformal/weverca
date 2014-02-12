using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// The class contains methods performing bitwise operations.
    /// </summary>
    /// <remarks>
    /// When PHP performs a bitwise operation, it always converts both operands into integers. Thou there
    /// is just one function that actually performs the operation, the rest of functions try to convert
    /// operands into integers. If either operand is not concrete, generally it is difficult to predicate
    /// the result of operation and abstract integer value should be returned.
    /// </remarks>
    public static class BitwiseOperation
    {
        /// <summary>
        /// Perform bitwise operation for given integer operands.
        /// </summary>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="operation">Operation to be performed, only the bitwise one gives a result.</param>
        /// <param name="leftOperand">Left integer operand of bitwise operation.</param>
        /// <param name="rightOperand">Right integer operand of bitwise operation.</param>
        /// <returns>If operation is bitwise, it returns integer result, otherwise <c>null</c>.</returns>
        public static IntegerValue Bitwise(FlowOutputSet outset, Operations operation,
            int leftOperand, int rightOperand)
        {
            switch (operation)
            {
                case Operations.BitAnd:
                    return outset.CreateInt(leftOperand & rightOperand);
                case Operations.BitOr:
                    return outset.CreateInt(leftOperand | rightOperand);
                case Operations.BitXor:
                    return outset.CreateInt(leftOperand ^ rightOperand);
                case Operations.ShiftLeft:
                    return outset.CreateInt(leftOperand << rightOperand);
                case Operations.ShiftRight:
                    return outset.CreateInt(leftOperand >> rightOperand);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Perform bitwise operation for given integer and string operands.
        /// </summary>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="operation">Operation to be performed, only the bitwise one gives a result.</param>
        /// <param name="leftOperand">Left integer operand of bitwise operation.</param>
        /// <param name="rightOperand">Right floating-point operand of bitwise operation.</param>
        /// <returns>If operation is bitwise, it returns integer result, otherwise <c>null</c>.</returns>
        public static Value Bitwise(FlowOutputSet outset, Operations operation,
            int leftOperand, double rightOperand)
        {
            int rightInteger;
            if (TypeConversion.TryConvertToInteger(rightOperand, out rightInteger))
            {
                return Bitwise(outset, operation, leftOperand, rightInteger);
            }
            else
            {
                // If the right operand can not be recognized, the number can be any integer value.
                return Bitwise(outset, operation);
            }
        }

        /// <summary>
        /// Perform bitwise operation for given integer and floating-point operands.
        /// </summary>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="operation">Operation to be performed, only the bitwise one gives a result.</param>
        /// <param name="leftOperand">Left integer operand of bitwise operation.</param>
        /// <param name="rightOperand">Right string operand of bitwise operation.</param>
        /// <returns>If operation is bitwise, it returns integer result, otherwise <c>null</c>.</returns>
        public static Value Bitwise(FlowOutputSet outset, Operations operation,
            int leftOperand, string rightOperand)
        {
            int rightInteger;
            if (TypeConversion.TryConvertToInteger(rightOperand, out rightInteger))
            {
                return Bitwise(outset, operation, leftOperand, rightInteger);
            }
            else
            {
                // If the right operand can not be recognized, the number can be any integer value.
                return Bitwise(outset, operation);
            }
        }

        /// <summary>
        /// Perform bitwise operation for given floating-point and integer operands.
        /// </summary>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="operation">Operation to be performed, only the bitwise one gives a result.</param>
        /// <param name="leftOperand">Left floating-point operand of bitwise operation.</param>
        /// <param name="rightOperand">Right integer operand of bitwise operation.</param>
        /// <returns>If operation is bitwise, it returns integer result, otherwise <c>null</c>.</returns>
        public static Value Bitwise(FlowOutputSet outset, Operations operation,
            double leftOperand, int rightOperand)
        {
            int leftInteger;
            if (TypeConversion.TryConvertToInteger(leftOperand, out leftInteger))
            {
                return Bitwise(outset, operation, leftInteger, rightOperand);
            }
            else
            {
                // If the left operand can not be recognized, the number can be any integer value.
                return Bitwise(outset, operation);
            }
        }

        /// <summary>
        /// Perform bitwise operation for given floating-point operands.
        /// </summary>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="operation">Operation to be performed, only the bitwise one gives a result.</param>
        /// <param name="leftOperand">Left floating-point operand of bitwise operation.</param>
        /// <param name="rightOperand">Right floating-point operand of bitwise operation.</param>
        /// <returns>If operation is bitwise, it returns integer result, otherwise <c>null</c>.</returns>
        public static Value Bitwise(FlowOutputSet outset, Operations operation,
            double leftOperand, double rightOperand)
        {
            int leftInteger;
            if (TypeConversion.TryConvertToInteger(leftOperand, out leftInteger))
            {
                return Bitwise(outset, operation, leftInteger, rightOperand);
            }
            else
            {
                // If the left or right operands can not be recognized, numbers can be any integer values.
                return Bitwise(outset, operation);
            }
        }

        /// <summary>
        /// Perform bitwise operation for given floating-point and string operands.
        /// </summary>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="operation">Operation to be performed, only the bitwise one gives a result.</param>
        /// <param name="leftOperand">Left floating-point operand of bitwise operation.</param>
        /// <param name="rightOperand">Right string operand of bitwise operation.</param>
        /// <returns>If operation is bitwise, it returns integer result, otherwise <c>null</c>.</returns>
        public static Value Bitwise(FlowOutputSet outset, Operations operation,
            double leftOperand, string rightOperand)
        {
            int leftInteger;
            if (TypeConversion.TryConvertToInteger(leftOperand, out leftInteger))
            {
                return Bitwise(outset, operation, leftInteger, rightOperand);
            }
            else
            {
                // If the left or right operands can not be recognized, numbers can be any integer values.
                return Bitwise(outset, operation);
            }
        }

        /// <summary>
        /// Perform bitwise operation for given string and integer operands.
        /// </summary>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="operation">Operation to be performed, only the bitwise one gives a result.</param>
        /// <param name="leftOperand">Left string operand of bitwise operation.</param>
        /// <param name="rightOperand">Right integer operand of bitwise operation.</param>
        /// <returns>If operation is bitwise, it returns integer result, otherwise <c>null</c>.</returns>
        public static Value Bitwise(FlowOutputSet outset, Operations operation,
            string leftOperand, int rightOperand)
        {
            int leftInteger;
            if (TypeConversion.TryConvertToInteger(leftOperand, out leftInteger))
            {
                return Bitwise(outset, operation, leftInteger, rightOperand);
            }
            else
            {
                // If the left operand can not be recognized, the number can be any integer value.
                return Bitwise(outset, operation);
            }
        }

        /// <summary>
        /// Perform bitwise operation for given string and floating-point operands.
        /// </summary>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="operation">Operation to be performed, only the bitwise one gives a result.</param>
        /// <param name="leftOperand">Left string operand of bitwise operation.</param>
        /// <param name="rightOperand">Right floating-point operand of bitwise operation.</param>
        /// <returns>If operation is bitwise, it returns integer result, otherwise <c>null</c>.</returns>
        public static Value Bitwise(FlowOutputSet outset, Operations operation,
            string leftOperand, double rightOperand)
        {
            int leftInteger;
            if (TypeConversion.TryConvertToInteger(leftOperand, out leftInteger))
            {
                return Bitwise(outset, operation, leftInteger, rightOperand);
            }
            else
            {
                // If the left or right operands can not be recognized, numbers can be any integer values.
                return Bitwise(outset, operation);
            }
        }

        /// <summary>
        /// Perform bitwise operation for given string operands.
        /// </summary>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="operation">Operation to be performed, only the bitwise one gives a result.</param>
        /// <param name="leftOperand">Left string operand of bitwise operation.</param>
        /// <param name="rightOperand">Right string operand of bitwise operation.</param>
        /// <returns>If operation is bitwise, it returns integer result, otherwise <c>null</c>.</returns>
        public static Value Bitwise(FlowOutputSet outset, Operations operation,
            string leftOperand, string rightOperand)
        {
            int leftInteger;
            if (TypeConversion.TryConvertToInteger(leftOperand, out leftInteger))
            {
                return Bitwise(outset, operation, leftInteger, rightOperand);
            }
            else
            {
                // If the left or right operands can not be recognized, numbers can be any integer values.
                return Bitwise(outset, operation);
            }
        }

        /// <summary>
        /// Return an abstract integer result of bitwise operation when operands are unknown.
        /// </summary>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="operation">Operation to be performed, only the bitwise one gives a result.</param>
        /// <returns>If operation is bitwise, it returns abstract integer, otherwise <c>null</c>.</returns>
        public static AnyIntegerValue Bitwise(FlowOutputSet outset, Operations operation)
        {
            return IsBitwise(operation) ? outset.AnyIntegerValue : null;
        }

        /// <summary>
        /// Indicate whether the given operation is bitwise.
        /// </summary>
        /// <param name="operation">Operation to be checked.</param>
        /// <returns><c>true</c> whether operation is bitwise, otherwise <c>false</c></returns>
        public static bool IsBitwise(Operations operation)
        {
            switch (operation)
            {
                case Operations.BitAnd:
                case Operations.BitOr:
                case Operations.BitXor:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    return true;
                default:
                    return false;
            }
        }
    }
}
