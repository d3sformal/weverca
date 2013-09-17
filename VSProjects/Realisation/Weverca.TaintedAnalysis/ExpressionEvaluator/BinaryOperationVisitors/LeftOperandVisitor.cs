using System;

using PHP.Core.AST;

using Weverca.Analysis;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.ExpressionEvaluator
{
    internal abstract class LeftOperandVisitor : AbstractValueVisitor
    {
        protected ExpressionEvaluator evaluator;

        protected LeftOperandVisitor(ExpressionEvaluator expressionEvaluator)
        {
            evaluator = expressionEvaluator;
        }

        protected FlowOutputSet OutSet
        {
            get { return evaluator.OutSet; }
        }

        internal Operations Operation { get; set; }

        public Value Result { get; protected internal set; }

        protected bool IsOperationComparison()
        {
            return (Operation == Operations.Equal)
                || (Operation == Operations.NotEqual)
                || (Operation == Operations.LessThan)
                || (Operation == Operations.LessThanOrEqual)
                || (Operation == Operations.GreaterThan)
                || (Operation == Operations.GreaterThanOrEqual);
        }

        protected bool IsOperationBitwise()
        {
            return (Operation == Operations.BitAnd)
                || (Operation == Operations.BitOr)
                || (Operation == Operations.BitXor)
                || (Operation == Operations.ShiftLeft)
                || (Operation == Operations.ShiftRight);
        }

        protected bool IsLogicalBitwise()
        {
            return (Operation == Operations.And)
                || (Operation == Operations.Or)
                || (Operation == Operations.Xor);
        }

        protected bool ComparisonOperation(bool leftOperand, bool rightOperand)
        {
            var outSet = OutSet;

            switch (Operation)
            {
                case Operations.Equal:
                    Result = outSet.CreateBool(leftOperand == rightOperand);
                    return true;
                case Operations.NotEqual:
                    Result = outSet.CreateBool(leftOperand != rightOperand);
                    return true;
                case Operations.LessThan:
                    Result = outSet.CreateBool((!leftOperand) && rightOperand);
                    return true;
                case Operations.LessThanOrEqual:
                    Result = outSet.CreateBool((!leftOperand) || rightOperand);
                    return true;
                case Operations.GreaterThan:
                    Result = outSet.CreateBool(leftOperand && (!rightOperand));
                    return true;
                case Operations.GreaterThanOrEqual:
                    Result = outSet.CreateBool(leftOperand || (!rightOperand));
                    return true;
                default:
                    return false;
            }
        }

        protected bool ComparisonOperation(double leftOperand, double rightOperand)
        {
            var outSet = OutSet;

            switch (Operation)
            {
                case Operations.Equal:
                    evaluator.SetWarning("Comparing floating-point numbers directly for equality");
                    Result = outSet.CreateBool(leftOperand == rightOperand);
                    return true;
                case Operations.NotEqual:
                    evaluator.SetWarning("Comparing floating-point numbers directly for non-equality");
                    Result = outSet.CreateBool(leftOperand != rightOperand);
                    return true;
                case Operations.LessThan:
                    Result = outSet.CreateBool(leftOperand < rightOperand);
                    return true;
                case Operations.LessThanOrEqual:
                    Result = outSet.CreateBool(leftOperand <= rightOperand);
                    return true;
                case Operations.GreaterThan:
                    Result = outSet.CreateBool(leftOperand > rightOperand);
                    return true;
                case Operations.GreaterThanOrEqual:
                    Result = outSet.CreateBool(leftOperand >= rightOperand);
                    return true;
                default:
                    return false;
            }
        }

        protected bool BitwiseOperation(int leftOperand, int rightOperand)
        {
            var outSet = OutSet;

            switch (Operation)
            {
                case Operations.BitAnd:
                    Result = outSet.CreateInt(leftOperand & rightOperand);
                    return true;
                case Operations.BitOr:
                    Result = outSet.CreateInt(leftOperand | rightOperand);
                    return true;
                case Operations.BitXor:
                    Result = outSet.CreateInt(leftOperand ^ rightOperand);
                    return true;
                case Operations.ShiftLeft:
                    Result = outSet.CreateInt(leftOperand << rightOperand);
                    return true;
                case Operations.ShiftRight:
                    Result = outSet.CreateInt(leftOperand >> rightOperand);
                    return true;
                default:
                    return false;
            }
        }

        protected bool LogicalOperation(bool leftOperand, bool rightOperand)
        {
            var outSet = OutSet;

            switch (Operation)
            {
                case Operations.And:
                    Result = outSet.CreateBool(leftOperand && rightOperand);
                    return true;
                case Operations.Or:
                    Result = outSet.CreateBool(leftOperand || rightOperand);
                    return true;
                case Operations.Xor:
                    Result = outSet.CreateBool(leftOperand != rightOperand);
                    return true;
                default:
                    return false;
            }
        }

        #region IValueVisitor Members

        public override void VisitValue(Value value)
        {
            throw new InvalidOperationException("Resolving of non-binary operation");
        }

        #endregion
    }
}
