using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;
using Weverca.Analysis;
using Weverca.Analysis.Expressions;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.FlowResolver
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

        Postfix conditionPart;
        MemoryEntry evaluatedPart;

        FlowOutputSet flowOutputSet;

        #endregion

        #region Members

        public PossibleValues ConditionResult { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionPart"/> class.
        /// </summary>
        /// <param name="conditionPart">The definition of the part of the condition.</param>
        /// <param name="evaluatedPart">The evaluated part of the condition.</param>
        public ConditionPart(Postfix conditionPart, MemoryEntry evaluatedPart)
        {
            this.conditionPart = conditionPart;
            this.evaluatedPart = evaluatedPart;

            ConditionResult = GetConditionResult();
        }

        #endregion

        #region Methods

        public void AssumeCondition(FlowOutputSet flowOutputSet)
        {
            this.flowOutputSet = flowOutputSet;

            if (ConditionResult == PossibleValues.OnlyTrue)
            {
                AssumeTrue();
            }
            else if (ConditionResult == PossibleValues.OnlyFalse)
            {
                AssumeFalse();
            }
            else if (ConditionResult == PossibleValues.Unknown)
            {
                AssumeUnknown(); //assumeTrue?
            }
            else
            {
                throw new NotSupportedException(string.Format("Condition result \"{0}\" is not supported.", ConditionResult));
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the condition result.
        /// </summary>
        /// <returns>see <see cref="PossibleValues"/> for details of possible result.</returns>
        PossibleValues GetConditionResult()
        {
            bool onlyTrue = true;
            bool onlyFalse = true;

            foreach (var value in evaluatedPart.PossibleValues)
            {
                var boolean = value as BooleanValue;
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
                    //TODO: what to do with non-bool values?
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

        void AssumeTrue()
        {
            if (conditionPart.SourceElement is BinaryEx)
            {
                BinaryEx binaryExpression = conditionPart.SourceElement as BinaryEx;
                //TODO: tady muze byt i AND, OR, ... a pak bude treba nastartovat cely proces od zacatku pro levou a pravou stranu zvlast... od new ConditionParts();
                if (binaryExpression.PublicOperation == Operations.Equal)
                {
                    AssumeEquals(binaryExpression.LeftExpr, binaryExpression.RightExpr);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Operation \"{0}\" is not supported for expression type \"{1}\"", binaryExpression.PublicOperation, conditionPart.GetType().Name));
                }
            }
            else
            {
                throw new NotSupportedException(string.Format("Expression type \"{0}\" is not supported", conditionPart.SourceElement.GetType().Name));
            }
        }

        void AssumeUnknown()
        {
            // only variables which are not set precisely not used in true parts will be evaluated.
        }

        void AssumeFalse()
        {
            // will be used only if the variable is not used in true or unknown parts. Possible values of the variable can be still infinit after few are eliminated.
            // invert operation --> assume true
        }

        void AssumeEquals(LangElement left, LangElement right)
        {
            if (right is DirectVarUse && !(left is DirectVarUse))
            {
                AssumeEquals(right, left);
            }
            else if (left is DirectVarUse)
            {
                var leftVar = (DirectVarUse)left;
                if (right is StringLiteral)
                {
                    var rigthValue = (StringLiteral)right;
                    flowOutputSet.Assign(leftVar.VarName, flowOutputSet.CreateString((string)rigthValue.Value));
                }
                else if (right is BoolLiteral)
                {
                    var rigthValue = (BoolLiteral)right;
                    flowOutputSet.Assign(leftVar.VarName, flowOutputSet.CreateBool((bool)rigthValue.Value));
                }
                else if (right is DoubleLiteral)
                {
                    var rigthValue = (DoubleLiteral)right;
                    flowOutputSet.Assign(leftVar.VarName, flowOutputSet.CreateDouble((double)rigthValue.Value));
                }
                else if (right is IntLiteral)
                {
                    var rigthValue = (IntLiteral)right;
                    flowOutputSet.Assign(leftVar.VarName, flowOutputSet.CreateInt((int)rigthValue.Value));
                }
                else if (right is LongIntLiteral)
                {
                    var rigthValue = (LongIntLiteral)right;
                    flowOutputSet.Assign(leftVar.VarName, flowOutputSet.CreateLong((long)rigthValue.Value));
                }
                else if (right is NullLiteral)
                {
                    //TODO: how to create null?
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


        #endregion
    }
}
