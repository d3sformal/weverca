using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    public static class BitwiseOperation
    {
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

        public static AnyIntegerValue Bitwise(FlowOutputSet outset, Operations operation)
        {
            return IsBitWise(operation) ? outset.AnyIntegerValue : null;
        }

        public static bool IsBitWise(Operations operation)
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
