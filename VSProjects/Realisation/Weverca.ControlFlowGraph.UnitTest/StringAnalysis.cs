using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.ControlFlowGraph.Analysis;

namespace Weverca.ControlFlowGraph.UnitTest
{
    /// <summary>
    /// Test implementation of simple string analysis
    /// </summary>
    class StringAnalysis : ForwardAnalysisAbstract<StringVarInfo>
    {
        public StringAnalysis(ControlFlowGraph cfg)
            : base(cfg)
        {
        }


        protected override void FlowThrough(FlowInputSet<StringVarInfo> inSet, LangElement statement, FlowOutputSet<StringVarInfo> outSet)
        {
            outSet.FillFrom(inSet);

            if (statement is ValueAssignEx)
            {
                var assign = statement as ValueAssignEx;
                var assigned = assign.LValue;
                var assignValue = assign.RValue;

                var varName = resolveVarName(assigned);
                var info = new StringVarInfo(varName);


                var str = resolveString(assignValue.Value);
                info.PossibleValues.Add(str);

                outSet.SetInfo(info.Name, info);
            }
        }



        protected override void BlockMerge(FlowInputSet<StringVarInfo> inSet1, FlowInputSet<StringVarInfo> inSet2, FlowOutputSet<StringVarInfo> outSet)
        {
            outSet.FillFrom(inSet2);
            foreach (var info in inSet1.CollectedInfo)
            {
                StringVarInfo toMerge;
                if (inSet2.TryGetInfo(info.Name, out toMerge))
                {
                    var newInfo = new StringVarInfo(info.Name);
                    newInfo.PossibleValues.UnionWith(info.PossibleValues);
                    newInfo.PossibleValues.UnionWith(toMerge.PossibleValues);
                    outSet.SetInfo(newInfo.Name, newInfo);
                }
                else
                {
                    outSet.SetInfo(info.Name, info);
                }
            }
        }



        protected override bool ConfirmAssumption(FlowInputSet<StringVarInfo> inSet, AssumptionCondition condition, FlowOutputSet<StringVarInfo> outSet)
        {
            outSet.FillFrom(inSet);

            if (!condition.Parts.Any())
            {
                //we can't disprove assumption
                return true;
            }

            var result = initialFor(condition.Form);
            foreach (var part in condition.Parts)
            {
                bool partResult;
                if (!canProve(inSet, part, condition.Form, out partResult))
                {
                    return true;
                }

                result = combine(result, partResult, condition.Form);

            }

            return result;
        }

        protected override void IncludeMerge(IEnumerable<FlowInputSet<StringVarInfo>> inSets, FlowOutputSet<StringVarInfo> outSet)
        {
            throw new NotImplementedException();
        }

        protected override void CallMerge(FlowInputSet<StringVarInfo> inSet1, FlowInputSet<StringVarInfo> inSet2, FlowOutputSet<StringVarInfo> outSet)
        {
            throw new NotImplementedException();
        }

        #region AST utils
        private string resolveVarName(object lvalue)
        {
            var dirUse = (DirectVarUse)lvalue;
            return dirUse.VarName.Value;
        }

        private string resolveString(object expression)
        {
            return expression.ToString();
        }
        #endregion

        #region Condition resolving

        private bool initialFor(ConditionForm form)
        {
            switch (form)
            {
                case ConditionForm.SomeNot:
                case ConditionForm.Some:
                    return false;
                case ConditionForm.All:
                case ConditionForm.None:
                    return true;
                default:
                    throw new NotSupportedException("Unsupported condition form");
            }
        }

        private bool combine(bool partResult, bool addedPart, ConditionForm form)
        {
            switch (form)
            {
                case ConditionForm.All:
                    return partResult && addedPart;
                case ConditionForm.Some:
                    return partResult || addedPart;
                case ConditionForm.SomeNot:
                    return partResult || !addedPart;
                case ConditionForm.None:
                    return partResult && !addedPart;
                default:
                    throw new NotSupportedException("Unsupported condition form");
            }
        }

        private bool canProve(FlowInputSet<StringVarInfo> inSet, Expression condition, ConditionForm form, out bool result)
        {
            //default result is true - if we can't disprove assumption we has to accept it
            var binEx = condition as BinaryEx;

            if (binEx != null && binEx.PublicOperation == Operations.Equal)
            {
                var varUsg = binEx.LeftExpr as DirectVarUse;
                var dirVal = binEx.RightExpr as StringLiteral;

                if (varUsg != null && dirVal != null)
                {
                    var varName = varUsg.VarName.Value;
                    var value = dirVal.Value.ToString();

                    return proveCanBeEqual(inSet, varName, value, out result);
                }
            }

            result = initialFor(form);
            return false;
        }


        private bool proveCanBeEqual(FlowInputSet<StringVarInfo> inSet, string varName, string comparedVal, out bool result)
        {
            result = false;
            StringVarInfo info;
            if (!inSet.TryGetInfo(varName, out info))
                return false;

            result = info.PossibleValues.Contains(comparedVal);
            return true;
        }

        #endregion

        protected override void ReturnedFromCall(FlowInputSet<StringVarInfo> callerInSet, FlowInputSet<StringVarInfo> callOutput, FlowOutputSet<StringVarInfo> outSet)
        {
            throw new NotImplementedException();
        }
    }
}
