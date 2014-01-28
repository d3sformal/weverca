using System;
using System.Collections.Generic;
using System.Linq;

using PHP.Core.AST;
using Weverca.Analysis.ExpressionEvaluator;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.FlowResolver
{
    /// <summary>
    /// Class which holds a context of a part of a condition.
    /// </summary>
    class ConditionPart
    {
        #region Enums

        /// <summary>
        /// Possible result of the condition
        /// </summary>
        public enum PossibleValues
        {
            /// <summary>
            /// The only possible result is <c>true</c>.
            /// </summary>
            OnlyTrue,

            /// <summary>
            /// The only possible result is <c>false</c>.
            /// </summary>
            OnlyFalse,

            /// <summary>
            /// The result is uncertain.
            /// </summary>
            Unknown
        }

        #endregion

        #region Members

        LangElement conditionPart;
        MemoryEntry evaluatedPart;

        EvaluationLog log;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the condition result.
        /// </summary>
        /// <seealso cref="PossibleValues"/>
        public PossibleValues ConditionResult { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionPart" /> class.
        /// </summary>
        /// <param name="conditionPart">The definition of the part of the condition.</param>
        /// <param name="log">The log of evaluation of the conditions' parts.</param>
        public ConditionPart(LangElement conditionPart, EvaluationLog log, FlowOutputSet flowOutputSet)
        {
            this.conditionPart = conditionPart;
            this.evaluatedPart = log.ReadSnapshotEntry(conditionPart).ReadMemory(flowOutputSet.Snapshot);
            this.log = log;

            ConditionResult = GetConditionResult(flowOutputSet);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Assumes the condition.
        /// According to the possible results of the condition the state of the inner block will be set up.
        /// </summary>
        /// <param name="memoryContext">The flow output set.</param>
        public void AssumeCondition(ConditionForm conditionForm, MemoryContext memoryContext, FlowOutputSet flowOutputSet)
        {
            var variables = GetVariables();
            if (variables.Count() == 0)
            {
                //There is nothing to assume because there is no variable used in the expression.
                return;
            }

            if (ConditionResult == PossibleValues.OnlyTrue)
            {
                AssumeTrue(conditionPart, memoryContext, flowOutputSet);
            }
            else if (ConditionResult == PossibleValues.OnlyFalse)
            {
                AssumeFalse(conditionPart, memoryContext, flowOutputSet);
            }
            else if (ConditionResult == PossibleValues.Unknown)
            {
                if (conditionForm == ConditionForm.All)
                {
                    AssumeTrue(conditionPart, memoryContext, flowOutputSet);
                }
                else if (conditionForm == ConditionForm.None)
                {
                    AssumeFalse(conditionPart, memoryContext, flowOutputSet);
                }
                else
                {
                    //run both assumptions and merge results
                    MemoryContext memoryContextTrue = new MemoryContext(log, flowOutputSet.Snapshot);
                    AssumeTrue(conditionPart, memoryContextTrue, flowOutputSet);

                    AssumeFalse(conditionPart, memoryContext, flowOutputSet);
                    memoryContext.UnionMerge(memoryContextTrue);
                }
            }
            else
            {
                throw new NotSupportedException(string.Format("Condition result \"{0}\" is not supported.", ConditionResult));
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the variables used in the condition.
        /// </summary>
        /// <returns></returns>
        IEnumerable<VariableUse> GetVariables()
        {
            VariableVisitor visitor = new VariableVisitor();
            conditionPart.VisitMe(visitor);

            return visitor.Variables;
        }

        /// <summary>
        /// Gets the condition result.
        /// </summary>
        /// <returns>see <see cref="PossibleValues"/> for details of possible result.</returns>
        PossibleValues GetConditionResult(FlowOutputSet flowOutputSet)
        {
            bool onlyTrue = true;
            bool onlyFalse = true;

            foreach (var value in evaluatedPart.PossibleValues)
            {
                var boolean = value as BooleanValue;
                if (boolean == null)
                {
                    boolean = TypeConversion.ToBoolean(flowOutputSet, value);
                }

                if (boolean != null)
                {
                    if (!boolean.Value)
                    {
                        onlyTrue = false;
                    }
                    else
                    {
                        onlyFalse = false;
                    }
                }
                else
                {
                    onlyFalse = false;
                    onlyTrue = false;
                }
            }

            if (onlyTrue)
            {
                return PossibleValues.OnlyTrue;
            }
            else if (onlyFalse)
            {
                return PossibleValues.OnlyFalse;
            }
            else
            {
                return PossibleValues.Unknown;
            }
        }

        /// <summary>
        /// Makes the assumption in case of <c>true</c> as a condition result.
        /// </summary>
        /// <param name="langElement">The language element to assume.</param>
        /// </exception>
        void AssumeTrue(LangElement langElement, MemoryContext memoryContext, FlowOutputSet flowOutputSet)
        {
            if (langElement is BinaryEx)
            {
                BinaryEx binaryExpression = (BinaryEx)langElement;
                if (binaryExpression.PublicOperation == Operations.Equal)
                {
                    AssumeEquals(binaryExpression.LeftExpr, binaryExpression.RightExpr, memoryContext);
                }
                else if (binaryExpression.PublicOperation == Operations.NotEqual)
                {
                    AssumeNotEquals(binaryExpression.LeftExpr, binaryExpression.RightExpr, memoryContext);
                }
                else if (binaryExpression.PublicOperation == Operations.GreaterThan)
                {
                    AssumeGreaterThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, false, memoryContext);
                }
                else if (binaryExpression.PublicOperation == Operations.GreaterThanOrEqual)
                {
                    AssumeGreaterThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, true, memoryContext);
                }
                else if (binaryExpression.PublicOperation == Operations.LessThan)
                {
                    AssumeLesserThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, false, memoryContext);
                }
                else if (binaryExpression.PublicOperation == Operations.LessThanOrEqual)
                {
                    AssumeLesserThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, true, memoryContext);
                }
                else if (binaryExpression.PublicOperation == Operations.And ||
                    binaryExpression.PublicOperation == Operations.Or ||
                    binaryExpression.PublicOperation == Operations.Xor)
                {
                    ConditionForm conditionForm = ConditionForm.All;
                    if (binaryExpression.PublicOperation == Operations.Or)
                    {
                        conditionForm = ConditionForm.Some;
                    }
                    else if (binaryExpression.PublicOperation == Operations.Xor)
                    {
                        conditionForm = ConditionForm.ExactlyOne;
                    }

                    MemoryContext currentMemoryContext = new MemoryContext(log, flowOutputSet.Snapshot);
                    ConditionParts condition = new ConditionParts(conditionForm, flowOutputSet, log, binaryExpression.LeftExpr, binaryExpression.RightExpr);
                    condition.MakeAssumption(currentMemoryContext);
                    memoryContext.UnionMerge(currentMemoryContext);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Operation \"{0}\" is not supported for expression type \"{1}\"", binaryExpression.PublicOperation, langElement.GetType().Name));
                }
            }
            else if (langElement is UnaryEx)
            {
                UnaryEx unaryExpression = (UnaryEx)langElement;
                if (unaryExpression.PublicOperation == Operations.LogicNegation)
                {
                    AssumeFalse(unaryExpression.Expr, memoryContext, flowOutputSet);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Operation \"{0}\" is not supported for expression type \"{1}\"", unaryExpression.PublicOperation, langElement.GetType().Name));
                }
            }
            else if (langElement is DirectVarUse)
            {
                DirectVarUse directVarUse = (DirectVarUse)langElement;
                AssumeTrueDirectVarUse(directVarUse, memoryContext, flowOutputSet.Snapshot);
            }
            else
            {
                throw new NotSupportedException(string.Format("Expression type \"{0}\" is not supported", langElement.GetType().Name));
                //TODO: deal with IssetEx
            }
        }

        /// <summary>
        /// Makes the assumption in case of <c>false</c> as a condition result.
        /// </summary>
        /// <param name="langElement">The language element to assume.</param>
        /// </exception>
        void AssumeFalse(LangElement langElement, MemoryContext memoryContext, FlowOutputSet flowOutputSet)
        {
            if (langElement is BinaryEx)
            {
                BinaryEx binaryExpression = (BinaryEx)langElement;
                if (binaryExpression.PublicOperation == Operations.Equal)
                {
                    AssumeNotEquals(binaryExpression.LeftExpr, binaryExpression.RightExpr, memoryContext);
                }
                else if (binaryExpression.PublicOperation == Operations.NotEqual)
                {
                    AssumeEquals(binaryExpression.LeftExpr, binaryExpression.RightExpr, memoryContext);
                }
                else if (binaryExpression.PublicOperation == Operations.GreaterThan)
                {
                    AssumeLesserThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, true, memoryContext);
                }
                else if (binaryExpression.PublicOperation == Operations.GreaterThanOrEqual)
                {
                    AssumeLesserThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, false, memoryContext);
                }
                else if (binaryExpression.PublicOperation == Operations.LessThan)
                {
                    AssumeGreaterThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, true, memoryContext);
                }
                else if (binaryExpression.PublicOperation == Operations.LessThanOrEqual)
                {
                    AssumeGreaterThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, false, memoryContext);
                }
                else if (binaryExpression.PublicOperation == Operations.And ||
                    binaryExpression.PublicOperation == Operations.Or ||
                    binaryExpression.PublicOperation == Operations.Xor)
                {
                    ConditionForm conditionForm = ConditionForm.SomeNot; // !(a AND b) --> !a OR !b
                    if (binaryExpression.PublicOperation == Operations.Or)
                    {
                        conditionForm = ConditionForm.None; // !(a OR b) --> !a AND !b
                    }
                    else if (binaryExpression.PublicOperation == Operations.Xor)
                    {
                        conditionForm = ConditionForm.NotExactlyOne; //!(a XOR b) --> !((a OR b) AND !(a AND b)) --> (!a AND !b) OR (a AND b)
                    }

                    MemoryContext currentMemoryContext = new MemoryContext(log, flowOutputSet.Snapshot);
                    ConditionParts condition = new ConditionParts(conditionForm, flowOutputSet, log, binaryExpression.LeftExpr, binaryExpression.RightExpr);
                    condition.MakeAssumption(currentMemoryContext);
                    memoryContext.UnionMerge(currentMemoryContext);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Operation \"{0}\" is not supported for expression type \"{1}\"", binaryExpression.PublicOperation, langElement.GetType().Name));
                }
            }
            else if (langElement is UnaryEx)
            {
                UnaryEx unaryExpression = (UnaryEx)langElement;
                if (unaryExpression.PublicOperation == Operations.LogicNegation)
                {
                    AssumeTrue(unaryExpression.Expr, memoryContext, flowOutputSet);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Operation \"{0}\" is not supported for expression type \"{1}\"", unaryExpression.PublicOperation, langElement.GetType().Name));
                }
            }
            else if (langElement is DirectVarUse)
            {
                DirectVarUse directVarUse = (DirectVarUse)langElement;
                AssumeFalseDirectVarUse(directVarUse, memoryContext, flowOutputSet.Snapshot);
            }
            else
            {
                throw new NotSupportedException(string.Format("Expression type \"{0}\" is not supported", langElement.GetType().Name));
            }
        }

        /// <summary>
        /// Makes the assumption for case like <value>a != b</value>.
        /// </summary>
        /// <param name="left">The left side of the expression.</param>
        /// <param name="right">The right side of the expression.</param>
        void AssumeNotEquals(LangElement left, LangElement right, MemoryContext memoryContext)
        {
            if (right is DirectVarUse && !(left is DirectVarUse))
            {
                AssumeNotEquals(right, left, memoryContext);
            }

            //there is nothoing to do otherwise

            //else if (left is DirectVarUse)
            //{
            //    var leftVar = (DirectVarUse)left;
            //    //TODO: this can be done more accurate with negative set.
            //    //flowOutputSet.Assign(leftVar.VarName, flowOutputSet.AnyValue);
            //    //leave current set in the evaluation of the variable. There can be current values - {value}
            //}
            //else
            //{
            //    throw new NotSupportedException(string.Format("Element \"{0}\" is not supprted on the left side", left.GetType().Name));
            //}
        }

        /// <summary>
        /// Makes the assumption for case like <value>a == b</value>.
        /// </summary>
        /// <param name="left">The left side of the expression.</param>
        /// <param name="right">The right side of the expression.</param>
        void AssumeEquals(LangElement left, LangElement right, MemoryContext memoryContext)
        {
            if (right is DirectVarUse && !(left is DirectVarUse))
            {
                AssumeEquals(right, left, memoryContext);
            }
            else if (left is DirectVarUse)
            {
                var leftVar = (DirectVarUse)left;
                if (right is StringLiteral)
                {
                    var rigthValue = (StringLiteral)right;
                    memoryContext.IntersectionAssign(leftVar.VarName, leftVar, memoryContext.CreateString((string)rigthValue.Value));
                }
                else if (right is BoolLiteral)
                {
                    var rigthValue = (BoolLiteral)right;
                    memoryContext.IntersectionAssign(leftVar.VarName, leftVar, memoryContext.CreateBool((bool)rigthValue.Value));
                }
                else if (right is DoubleLiteral)
                {
                    var rigthValue = (DoubleLiteral)right;
                    memoryContext.IntersectionAssign(leftVar.VarName, leftVar, memoryContext.CreateDouble((double)rigthValue.Value));
                }
                else if (right is IntLiteral)
                {
                    var rigthValue = (IntLiteral)right;
                    memoryContext.IntersectionAssign(leftVar.VarName, leftVar, memoryContext.CreateInt((int)rigthValue.Value));
                }
                else if (right is LongIntLiteral)
                {
                    var rigthValue = (LongIntLiteral)right;
                    memoryContext.IntersectionAssign(leftVar.VarName, leftVar, memoryContext.CreateLong((long)rigthValue.Value));
                }
                else if (right is NullLiteral)
                {
                    //TODO: Is that proper null?
                    memoryContext.IntersectionAssign(leftVar.VarName, leftVar, memoryContext.UndefinedValue);
                }
                else
                {
                    throw new NotSupportedException(string.Format("right type \"{0}\" is not supported for \"{1}\"", right.GetType().Name, left.GetType().Name));
                    //TODO: deal with DirectVarUse on the right
                }
            }
            else
            {
                throw new NotSupportedException(string.Format("Element \"{0}\" is not supprted on the left side", left.GetType().Name));
            }
        }

        /// <summary>
        /// Makes the assumption for case like <value>a &gt; b</value>.
        /// </summary>
        /// <param name="left">The left side of the expression.</param>
        /// <param name="right">The right side of the expression.</param>
        /// <param name="equal">if set to <c>true</c> greater or equals is assumed.</param>
        void AssumeGreaterThan(LangElement left, LangElement right, bool equal, MemoryContext memoryContext)
        {
            if (right is DirectVarUse && !(left is DirectVarUse))
            {
                AssumeGreaterThan(right, left, equal, memoryContext);
            }
            else if (left is DirectVarUse)
            {
                var leftVar = (DirectVarUse)left;
                if (right is StringLiteral)
                {
                    memoryContext.IntersectionAssign(leftVar.VarName, leftVar, memoryContext.AnyStringValue);
                }
                else if (right is DoubleLiteral)
                {
                    var rigthValue = (DoubleLiteral)right;
                    double bound = (double)rigthValue.Value;
                    if (!equal)
                    {
                        bound += double.Epsilon;
                    }
                    memoryContext.IntersectionAssign(leftVar.VarName, leftVar, memoryContext.CreateFloatInterval(bound, double.MaxValue));
                }
                else if (right is IntLiteral)
                {
                    var rigthValue = (IntLiteral)right;
                    int bound = (int)rigthValue.Value;
                    if (!equal)
                    {
                        bound++;
                    }
                    memoryContext.IntersectionAssign(leftVar.VarName, leftVar, memoryContext.CreateIntegerInterval(bound, int.MaxValue));
                }
                else if (right is LongIntLiteral)
                {
                    var rigthValue = (LongIntLiteral)right;
                    long bound = (long)rigthValue.Value;
                    if (!equal)
                    {
                        bound++;
                    }
                    memoryContext.IntersectionAssign(leftVar.VarName, leftVar, memoryContext.CreateLongintInterval(bound, long.MaxValue));
                }
                else
                {
                    throw new NotSupportedException(string.Format("right type \"{0}\" is not supported for \"{1}\"", right.GetType().Name, left.GetType().Name));
                }
            }
            else
            {
                throw new NotSupportedException(string.Format("Element \"{0}\" is not supprted on the left side", left.GetType().Name));
            }
        }

        /// <summary>
        /// Makes the assumption for case like <value>a &lt; b</value>.
        /// </summary>
        /// <param name="left">The left side of the expression.</param>
        /// <param name="right">The right side of the expression.</param>
        /// <param name="equal">if set to <c>true</c> lesser or equals is assumed.</param>
        void AssumeLesserThan(LangElement left, LangElement right, bool equal, MemoryContext memoryContext)
        {
            if (right is DirectVarUse && !(left is DirectVarUse))
            {
                AssumeLesserThan(right, left, equal, memoryContext);
            }
            else if (left is DirectVarUse)
            {
                var leftVar = (DirectVarUse)left;
                if (right is StringLiteral)
                {
                    memoryContext.IntersectionAssign(leftVar.VarName, leftVar, memoryContext.AnyStringValue);
                }
                else if (right is DoubleLiteral)
                {
                    var rigthValue = (DoubleLiteral)right;
                    double bound = (double)rigthValue.Value;
                    if (!equal)
                    {
                        bound -= double.Epsilon;
                    }
                    memoryContext.IntersectionAssign(leftVar.VarName, leftVar, memoryContext.CreateFloatInterval(double.MinValue, bound));
                }
                else if (right is IntLiteral)
                {
                    var rigthValue = (IntLiteral)right;
                    int bound = (int)rigthValue.Value;
                    if (!equal)
                    {
                        bound--;
                    }
                    memoryContext.IntersectionAssign(leftVar.VarName, leftVar, memoryContext.CreateIntegerInterval(int.MinValue, bound));
                }
                else if (right is LongIntLiteral)
                {
                    var rigthValue = (LongIntLiteral)right;
                    long bound = (long)rigthValue.Value;
                    if (!equal)
                    {
                        bound--;
                    }
                    memoryContext.IntersectionAssign(leftVar.VarName, leftVar, memoryContext.CreateLongintInterval(long.MinValue, bound));
                }
                else
                {
                    throw new NotSupportedException(string.Format("right type \"{0}\" is not supported for \"{1}\"", right.GetType().Name, left.GetType().Name));
                }
            }
            else
            {
                throw new NotSupportedException(string.Format("Element \"{0}\" is not supprted on the left side", left.GetType().Name));
            }
        }

        void AssumeTrueDirectVarUse(DirectVarUse directVarUse, MemoryContext memoryContext, SnapshotBase flowOutputSet)
        {
            MemoryEntry memoryEntry = log.ReadSnapshotEntry(directVarUse).ReadMemory(flowOutputSet);
            if (memoryEntry.PossibleValues.Any(a => a is AnyBooleanValue))
            {
                memoryContext.IntersectionAssign(directVarUse.VarName, directVarUse, memoryContext.CreateBool(true));
            }
        }

        void AssumeFalseDirectVarUse(DirectVarUse directVarUse, MemoryContext memoryContext, SnapshotBase flowOutputSet)
        {
            MemoryEntry memoryEntry = log.ReadSnapshotEntry(directVarUse).ReadMemory(flowOutputSet);
            if (memoryEntry.PossibleValues.Any(a => a is AnyBooleanValue))
            {
                memoryContext.IntersectionAssign(directVarUse.VarName, directVarUse, memoryContext.CreateBool(false));
            }
            else if (memoryEntry.PossibleValues.Any(a => a is AnyIntegerValue))
            {
                memoryContext.IntersectionAssign(directVarUse.VarName, directVarUse, memoryContext.CreateInt(0));
            }
            else if (memoryEntry.PossibleValues.Any(a => a is IntegerIntervalValue))
            {
                //there should be 0 in the interval
                memoryContext.IntersectionAssign(directVarUse.VarName, directVarUse, memoryContext.CreateInt(0));
            }
        }

        #endregion
    }
}
