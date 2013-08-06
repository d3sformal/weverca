using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the condition result.
        /// </summary>
        /// <returns>see <see cref="PossibleValues"/> for details of possible result.</returns>
        public PossibleValues GetConditionResult()
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

        #endregion
    }
}
