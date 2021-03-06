/*
Copyright (c) 2012-2014 Matyas Brenner and David Hauzar

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
    class AssumptionExecuter
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

        private readonly LangElement astElement;

        private readonly EvaluationLog log;

        private readonly FlowOutputSet flowOutputSet;

        private readonly ConditionForm conditionForm;

        #endregion

        #region Properties

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssumptionExecuter" /> class.
        /// </summary>
        /// <param name="conditionForm">The condition form. See <see cref="ConditionForm"/> for more details.</param>
        /// <param name="astElement">The AST element corresponding to the condition.</param>
        /// <param name="log">The log of evaluation of the condition.</param>
        /// <param name="flowOutputSet">The Output set of a program point.</param>
        public AssumptionExecuter(ConditionForm conditionForm, LangElement astElement, EvaluationLog log, FlowOutputSet flowOutputSet)
        {
            this.flowOutputSet = flowOutputSet;
            this.astElement = astElement;
            this.conditionForm = conditionForm;
            this.log = log;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the assumption can be satisfied.
        /// </summary>
        /// <returns></returns>
        public PossibleValues IsSatisfied()
        {
            var snapshotEntry = log.ReadSnapshotEntry(astElement);
            if (snapshotEntry != null)
            {
                MemoryEntry evaluatedPart = snapshotEntry.ReadMemory(flowOutputSet.Snapshot);
                return GetConditionResult(conditionForm, flowOutputSet, evaluatedPart);
            }
            else
            {
                return PossibleValues.Unknown;
            }
        }

        /// <summary>
        /// Refines the state according Assumes the condition according to the possible results of the condition. The state of the inner block will be set up.
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        public void RefineState()
        {
            if (conditionForm == ConditionForm.All)
            {
                AssumeTrue(astElement, new MemoryContext(log, flowOutputSet), flowOutputSet);
            }
            else if (conditionForm == ConditionForm.None)
            {
                AssumeFalse(astElement, new MemoryContext(log, flowOutputSet), flowOutputSet);
            }
            else
            {
                throw new NotSupportedException(string.Format("Condition form \"{0}\" is not supported.", conditionForm));
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the variables used in the condition.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<VariableUse> GetVariables()
        {
            VariableVisitor visitor = new VariableVisitor();
            astElement.VisitMe(visitor);

            return visitor.Variables;
        }

        /// <summary>
        /// Gets the condition result.
        /// </summary>
        /// <returns>see <see cref="PossibleValues"/> for details of possible result.</returns>
        private PossibleValues GetConditionResult(ConditionForm conditionForm, FlowOutputSet flowOutputSet, MemoryEntry evaluatedPart)
        {
            var converter = new BooleanConverter(flowOutputSet.Snapshot);
            var value = converter.EvaluateToBoolean(evaluatedPart);

            if (value == null)
            {
                return PossibleValues.Unknown;
            }

            var flipResult = (conditionForm == ConditionForm.None) ? true : false;

            if (value.Value ^ flipResult)
            {
                return PossibleValues.OnlyTrue;
            }
            else
            {
                return PossibleValues.OnlyFalse;
            }
        }

        /// <summary>
        /// Makes the assumption in case of <c>true</c> as a condition result.
        /// </summary>
        /// <param name="langElement">The language element to assume.</param>
        /// <param name="memoryContext">The memory context of the code block and it's variables.</param>
        /// <param name="flowOutputSet">The Output set of a program point.</param>
        private void AssumeTrue(LangElement langElement, MemoryContext memoryContext, FlowOutputSet flowOutputSet)
        {
            if (langElement is BinaryEx)
            {
                BinaryEx binaryExpression = (BinaryEx)langElement;
                if (binaryExpression.PublicOperation == Operations.Equal)
                {
                    AssumeEquals(binaryExpression.LeftExpr, binaryExpression.RightExpr, memoryContext, flowOutputSet.Snapshot);
                }
                else if (binaryExpression.PublicOperation == Operations.NotEqual)
                {
                    AssumeNotEquals(binaryExpression.LeftExpr, binaryExpression.RightExpr, memoryContext);
                }
                else if (binaryExpression.PublicOperation == Operations.GreaterThan)
                {
                    AssumeGreaterThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, false, memoryContext, flowOutputSet.Snapshot);
                }
                else if (binaryExpression.PublicOperation == Operations.GreaterThanOrEqual)
                {
                    AssumeGreaterThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, true, memoryContext, flowOutputSet.Snapshot);
                }
                else if (binaryExpression.PublicOperation == Operations.LessThan)
                {
                    AssumeLesserThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, false, memoryContext, flowOutputSet.Snapshot);
                }
                else if (binaryExpression.PublicOperation == Operations.LessThanOrEqual)
                {
                    AssumeLesserThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, true, memoryContext, flowOutputSet.Snapshot);
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

                    MemoryContext currentMemoryContext = new MemoryContext(log, flowOutputSet);
                    //AssumptionExecuter condition = new AssumptionExecuter(conditionForm, flowOutputSet, log, binaryExpression.LeftExpr, binaryExpression.RightExpr);
                    //condition.RefineState();
                    memoryContext.UnionMerge(currentMemoryContext);
                }
            }
            else if (langElement is UnaryEx)
            {
                UnaryEx unaryExpression = (UnaryEx)langElement;
                if (unaryExpression.PublicOperation == Operations.LogicNegation)
                {
                    AssumeFalse(unaryExpression.Expr, memoryContext, flowOutputSet);
                }
            }
            else if (langElement is DirectVarUse)
            {
                DirectVarUse directVarUse = (DirectVarUse)langElement;
                AssumeTrueDirectVarUse(directVarUse, memoryContext, flowOutputSet.Snapshot);
            } else if (langElement is IssetEx) 
            {
                IssetEx issetEx = (IssetEx)langElement;
                AssumeIsset(issetEx, memoryContext, flowOutputSet.Snapshot, true);
            }
        }

        /// <summary>
        /// Makes the assumption in case of <c>false</c> as a condition result.
        /// </summary>
        /// <param name="langElement">The language element to assume.</param>
        /// <param name="memoryContext">The memory context of the code block and it's variables.</param>
        /// <param name="flowOutputSet">The Output set of a program point.</param>
        private void AssumeFalse(LangElement langElement, MemoryContext memoryContext, FlowOutputSet flowOutputSet)
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
                    AssumeEquals(binaryExpression.LeftExpr, binaryExpression.RightExpr, memoryContext, flowOutputSet.Snapshot);
                }
                else if (binaryExpression.PublicOperation == Operations.GreaterThan)
                {
                    AssumeLesserThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, true, memoryContext, flowOutputSet.Snapshot);
                }
                else if (binaryExpression.PublicOperation == Operations.GreaterThanOrEqual)
                {
                    AssumeLesserThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, false, memoryContext, flowOutputSet.Snapshot);
                }
                else if (binaryExpression.PublicOperation == Operations.LessThan)
                {
                    AssumeGreaterThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, true, memoryContext, flowOutputSet.Snapshot);
                }
                else if (binaryExpression.PublicOperation == Operations.LessThanOrEqual)
                {
                    AssumeGreaterThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, false, memoryContext, flowOutputSet.Snapshot);
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

                    MemoryContext currentMemoryContext = new MemoryContext(log, flowOutputSet);
                    //AssumptionConditionExecuterDepr condition = new AssumptionConditionExecuterDepr(conditionForm, flowOutputSet, log, binaryExpression.LeftExpr, binaryExpression.RightExpr);
                    //condition.MakeAssumption(currentMemoryContext);
                    memoryContext.UnionMerge(currentMemoryContext);
                }
            }
            else if (langElement is UnaryEx)
            {
                UnaryEx unaryExpression = (UnaryEx)langElement;
                if (unaryExpression.PublicOperation == Operations.LogicNegation)
                {
                    AssumeTrue(unaryExpression.Expr, memoryContext, flowOutputSet);
                }
            }
            else if (langElement is DirectVarUse)
            {
                DirectVarUse directVarUse = (DirectVarUse)langElement;
                AssumeFalseDirectVarUse(directVarUse, memoryContext, flowOutputSet.Snapshot);
            } else if (langElement is IssetEx) 
            {
                IssetEx issetEx = (IssetEx)langElement;
                AssumeIsset(issetEx, memoryContext, flowOutputSet.Snapshot, false);
            }
        }

        /// <summary>
        /// Makes the assumption for case like <value>a != b</value>.
        /// </summary>
        /// <param name="left">The left side of the expression.</param>
        /// <param name="right">The right side of the expression.</param>
        /// <param name="memoryContext">The memory context of the code block and it's variables.</param>
        private void AssumeNotEquals(LangElement left, LangElement right, MemoryContext memoryContext)
        {
            //There is nothing to do.
            //if (right is DirectVarUse && !(left is DirectVarUse))
            //{
            //    AssumeNotEquals(right, left, memoryContext);
            //}

            //there is nothoing to do otherwise
        }

        /// <summary>
        /// Makes the assumption for case like <value>a == b</value>.
        /// </summary>
        /// <param name="left">The left side of the expression.</param>
        /// <param name="right">The right side of the expression.</param>
        /// <param name="memoryContext">The memory context of the code block and it's variables.</param>
        /// <param name="flowOutputSet">The Output set of a program point.</param>
        private void AssumeEquals(LangElement left, LangElement right, MemoryContext memoryContext, SnapshotBase flowOutputSet)
        {
            if (right is VarLikeConstructUse && !(left is VarLikeConstructUse))
            {
                AssumeEquals(right, left, memoryContext, flowOutputSet);
            }
            else if (left is VarLikeConstructUse)
            {
                var leftVar = (VarLikeConstructUse)left;
                // this is probably not neceserry {{
                if (right is StringLiteral)
                {
                    var rigthValue = (StringLiteral)right;
                    memoryContext.IntersectionAssign(leftVar, memoryContext.CreateString((string)rigthValue.Value));
                }
                else if (right is BoolLiteral)
                {
                    var rigthValue = (BoolLiteral)right;
                    memoryContext.IntersectionAssign(leftVar, memoryContext.CreateBool((bool)rigthValue.Value));
                }
                else if (right is DoubleLiteral)
                {
                    var rigthValue = (DoubleLiteral)right;
                    memoryContext.IntersectionAssign(leftVar, memoryContext.CreateDouble((double)rigthValue.Value));
                }
                else if (right is IntLiteral)
                {
                    var rigthValue = (IntLiteral)right;
                    memoryContext.IntersectionAssign(leftVar, memoryContext.CreateInt((int)rigthValue.Value));
                }
                else if (right is LongIntLiteral)
                {
                    var rigthValue = (LongIntLiteral)right;
                    memoryContext.IntersectionAssign(leftVar, memoryContext.CreateLong((long)rigthValue.Value));
                }
                else if (right is NullLiteral)
                {
                    //TODO: Is that proper null?
                    memoryContext.IntersectionAssign(leftVar, memoryContext.UndefinedValue);
                }
                //}}
                else
                {
                    var snapshotEntry = log.ReadSnapshotEntry(right);
                    if (snapshotEntry != null)
                    {
                        memoryContext.IntersectionAssign(leftVar, snapshotEntry.ReadMemory(flowOutputSet).PossibleValues);
                    }
                }
            }
        }

        /// <summary>
        /// Makes the assumption for case like <value>a &gt; b</value>.
        /// </summary>
        /// <param name="left">The left side of the expression.</param>
        /// <param name="right">The right side of the expression.</param>
        /// <param name="memoryContext">The memory context of the code block and it's variables.</param>
        /// <param name="flowOutputSet">The Output set of a program point.</param>
        /// <param name="equal">if set to <c>true</c> greater or equals is assumed.</param>
        private void AssumeGreaterThan(LangElement left, LangElement right, bool equal, MemoryContext memoryContext, SnapshotBase flowOutputSet)
        {
            if (right is VarLikeConstructUse && !(left is VarLikeConstructUse))
            {
                AssumeLesserThan(right, left, equal, memoryContext, flowOutputSet);
            }
            else if (left is VarLikeConstructUse)
            {
                var leftVar = (VarLikeConstructUse)left;
                //this is probably not necessary{
                if (right is StringLiteral)
                {
                    memoryContext.IntersectionAssign(leftVar, memoryContext.AnyStringValue);
                }
                else if (right is DoubleLiteral)
                {
                    var rigthValue = (DoubleLiteral)right;
                    double bound = (double)rigthValue.Value;
                    if (!equal)
                    {
                        bound += double.Epsilon;
                    }
                    memoryContext.IntersectionAssign(leftVar, memoryContext.CreateFloatInterval(bound, double.MaxValue));
                }
                else if (right is IntLiteral)
                {
                    var rigthValue = (IntLiteral)right;
                    int bound = (int)rigthValue.Value;
                    if (!equal)
                    {
                        bound++;
                    }
                    memoryContext.IntersectionAssign(leftVar, memoryContext.CreateIntegerInterval(bound, int.MaxValue));
                }
                else if (right is LongIntLiteral)
                {
                    var rigthValue = (LongIntLiteral)right;
                    long bound = (long)rigthValue.Value;
                    if (!equal)
                    {
                        bound++;
                    }
                    memoryContext.IntersectionAssign(leftVar, memoryContext.CreateLongintInterval(bound, long.MaxValue));
                }
                //}
                else
                {
                    var snapshotEntry = log.ReadSnapshotEntry(right);
                    if (snapshotEntry != null)
                    {
                        //get lower bound of right and intersect with left
                        int? minInt;
                        long? minLong;
                        double? minDouble;

                        ValueHelper.TryGetMinimumValue(snapshotEntry.ReadMemory(flowOutputSet).PossibleValues, out minInt, out minLong, out minDouble);

                        if (minInt.HasValue)
                        {
                            if (!equal)
                            {
                                minInt++;
                            }

                            memoryContext.IntersectionAssign(leftVar, memoryContext.CreateIntegerInterval(minInt.Value, int.MaxValue));
                        }
                        else if (minLong.HasValue)
                        {
                            if (!equal)
                            {
                                minLong++;
                            }

                            memoryContext.IntersectionAssign(leftVar, memoryContext.CreateLongintInterval(minLong.Value, long.MaxValue));
                        }
                        else if (minDouble.HasValue)
                        {
                            if (!equal)
                            {
                                minDouble += double.Epsilon;
                            }

                            memoryContext.IntersectionAssign(leftVar, memoryContext.CreateFloatInterval(minDouble.Value, double.MaxValue));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Makes the assumption for case like <value>a &lt; b</value>.
        /// </summary>
        /// <param name="left">The left side of the expression.</param>
        /// <param name="right">The right side of the expression.</param>
        /// <param name="equal">if set to <c>true</c> lesser or equals is assumed.</param>
        /// <param name="memoryContext">The memory context of the code block and it's variables.</param>
        /// <param name="flowOutputSet">The Output set of a program point.</param>
        private void AssumeLesserThan(LangElement left, LangElement right, bool equal, MemoryContext memoryContext, SnapshotBase flowOutputSet)
        {
            if (right is VarLikeConstructUse && !(left is VarLikeConstructUse))
            {
                AssumeGreaterThan(right, left, equal, memoryContext, flowOutputSet);
            }
            else if (left is VarLikeConstructUse)
            {
                var leftVar = (VarLikeConstructUse)left;
                //this is probably not necessary{
                if (right is StringLiteral)
                {
                    memoryContext.IntersectionAssign(leftVar, memoryContext.AnyStringValue);
                }
                else if (right is DoubleLiteral)
                {
                    var rigthValue = (DoubleLiteral)right;
                    double bound = (double)rigthValue.Value;
                    if (!equal)
                    {
                        bound -= double.Epsilon;
                    }
                    memoryContext.IntersectionAssign(leftVar, memoryContext.CreateFloatInterval(double.MinValue, bound));
                }
                else if (right is IntLiteral)
                {
                    var rigthValue = (IntLiteral)right;
                    int bound = (int)rigthValue.Value;
                    if (!equal)
                    {
                        bound--;
                    }
                    memoryContext.IntersectionAssign(leftVar, memoryContext.CreateIntegerInterval(int.MinValue, bound));
                }
                else if (right is LongIntLiteral)
                {
                    var rigthValue = (LongIntLiteral)right;
                    long bound = (long)rigthValue.Value;
                    if (!equal)
                    {
                        bound--;
                    }
                    memoryContext.IntersectionAssign(leftVar, memoryContext.CreateLongintInterval(long.MinValue, bound));
                }
                //}
                else
                {
                    var snapshotEntry = log.ReadSnapshotEntry(right);
                    if (snapshotEntry != null)
                    {
                        //get upper bound of right and intersect with left
                        int? maxInt;
                        long? maxLong;
                        double? maxDouble;
                        ValueHelper.TryGetMaximumValue(snapshotEntry.ReadMemory(flowOutputSet).PossibleValues, out maxInt, out maxLong, out maxDouble);

                        if (maxInt.HasValue)
                        {
                            if (!equal)
                            {
                                maxInt--;
                            }

                            memoryContext.IntersectionAssign(leftVar, memoryContext.CreateIntegerInterval(int.MinValue, maxInt.Value));
                        }
                        else if (maxLong.HasValue)
                        {
                            if (!equal)
                            {
                                maxLong--;
                            }

                            memoryContext.IntersectionAssign(leftVar, memoryContext.CreateLongintInterval(long.MinValue, maxLong.Value));
                        }
                        else if (maxDouble.HasValue)
                        {
                            if (!equal)
                            {
                                maxDouble -= double.Epsilon;
                            }

                            memoryContext.IntersectionAssign(leftVar, memoryContext.CreateFloatInterval(double.MinValue, maxDouble.Value));
                        }
                    }
                }
            }
        }

        private void AssumeIsset(IssetEx issetEx, MemoryContext memoryContext, SnapshotBase flowOutputSet, bool assumeTrue)
        {
            foreach (var variable in issetEx.VarList)
            {
                if (variable is DirectVarUse) 
                {
                    var dirVariable = (DirectVarUse) variable;
                    if (assumeTrue)
                        memoryContext.RemoveUndefinedValue(dirVariable.VarName, dirVariable);
                    else
                        memoryContext.AssignUndefinedValue(dirVariable.VarName);
                }
            }
        }

        private void AssumeTrueDirectVarUse(DirectVarUse directVarUse, MemoryContext memoryContext, SnapshotBase flowOutputSet)
        {
            memoryContext.AssignTrueEvaluable(directVarUse.VarName, directVarUse);
        }

        private void AssumeFalseDirectVarUse(DirectVarUse directVarUse, MemoryContext memoryContext, SnapshotBase flowOutputSet)
        {
            memoryContext.AssignFalseEvaluable(directVarUse.VarName, directVarUse);
        }

        #endregion
    }
}