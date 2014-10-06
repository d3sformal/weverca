/*
Copyright (c) 2012-2014 David Skorvaga and David Hauzar

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation with fixed left operand during the analysis.
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
    /// <seealso cref="BinaryOperationEvaluator" />
    public abstract class LeftOperandVisitor : PartialExpressionEvaluator
    {
        /// <summary>
        /// Binary operation that determines the proper action with operands.
        /// </summary>
        protected Operations operation;

        /// <summary>
        /// Result of performing the binary operation of the left and right operand.
        /// </summary>
        protected Value result;

        /// <summary>
        /// Initializes a new instance of the <see cref="LeftOperandVisitor" /> class.
        /// </summary>
        protected LeftOperandVisitor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeftOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point.</param>
        protected LeftOperandVisitor(FlowController flowController)
            : base(flowController)
        {
        }

        /// <summary>
        /// Evaluates binary operation with left operand of this visitor and the given right operand.
        /// </summary>
        /// <param name="binaryOperation">Binary operation to be performed.</param>
        /// <param name="rightOperand">The right operand of binary operation.</param>
        /// <returns>Result of performing the binary operation on the operands.</returns>
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
        /// Evaluates binary operation with one left operand and all possible values of right operand.
        /// </summary>
        /// <param name="binaryOperation">Binary operation to be performed.</param>
        /// <param name="rightOperand">Entry with all possible right operands of binary operation.</param>
        /// <returns>Resulting entry of performing the binary operation on all possible operands.</returns>
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
        /// <exception cref="System.InvalidOperationException">Thrown always</exception>
        public override void VisitValue(Value value)
        {
            throw new InvalidOperationException("Resolving of non-binary operation");
        }

        #region Concrete values

        #region Scalar values

        #region Numeric values

        /// <inheritdoc />
        /// <exception cref="System.NotSupportedException">Thrown always</exception>
        public override void VisitLongintValue(LongintValue value)
        {
            throw new NotSupportedException("Long integer is not currently supported");
        }

        #endregion Numeric values

        #endregion Scalar values

        /// <inheritdoc />
        public override void VisitResourceValue(ResourceValue value)
        {
            // Every binary operation converts resource to another type, but conversion to another type
            // makes no sence expect boolean that is always true. Thou we do not need concrete resource.
            VisitAnyResourceValue(OutSet.AnyResourceValue);
        }

        #endregion Concrete values

        #region Interval values

        /// <inheritdoc />
        /// <exception cref="System.NotSupportedException">Thrown always</exception>
        public override void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            throw new NotSupportedException("Long integer is not currently supported");
        }

        #endregion Interval values

        #region Abstract values

        #region Abstract scalar values

        #region Abstract numeric values

        /// <inheritdoc />
        /// <exception cref="System.NotSupportedException">Thrown always</exception>
        public override void VisitAnyLongintValue(AnyLongintValue value)
        {
            throw new NotSupportedException("Long integer is not currently supported");
        }

        #endregion Abstract numeric values

        #endregion Abstract scalar values

        #endregion Abstract values

        #endregion AbstractValueVisitor Members
    }

    /// <summary>
    /// Evaluates one binary operation with typed fixed left operand during the analysis.
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />.
    /// </remarks>
    /// <typeparam name="T">Type of left operand.</typeparam>
    public abstract class GenericLeftOperandVisitor<T> : LeftOperandVisitor where T : Value
    {
        /// <summary>
        /// A value of specified type representing the left operand of binary operation.
        /// </summary>
        protected T leftOperand;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericLeftOperandVisitor{T}" /> class.
        /// </summary>
        protected GenericLeftOperandVisitor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericLeftOperandVisitor{T}" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point.</param>
        protected GenericLeftOperandVisitor(FlowController flowController)
            : base(flowController)
        {
        }

        /// <summary>
        /// Set a value of specified type as left operand of binary operation.
        /// </summary>
        /// <param name="value">A concrete integer value.</param>
        public void SetLeftOperand(T value)
        {
            leftOperand = value;
        }
    }
}