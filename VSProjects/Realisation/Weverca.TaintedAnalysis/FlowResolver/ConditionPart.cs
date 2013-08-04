using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Analysis.Expressions;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.FlowResolver
{
    class ConditionPart
    {
        #region Enums

        public enum PossibleValues { OnlyTrue, OnlyFalse, Unknown }

        #endregion

        #region Members

        Postfix conditionPart;
        MemoryEntry evaluatedPart;

        #endregion

        #region Constructors

        public ConditionPart(Postfix conditionPart, MemoryEntry evaluatedPart)
        {
            this.conditionPart = conditionPart;
            this.evaluatedPart = evaluatedPart;
        }

        #endregion

        #region Methods

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
