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
    class StringAnalysis:ForwardAnalysis<StringVarInfo>
    {
        public StringAnalysis(ControlFlowGraph cfg):base(cfg)
        {
        }

     
        protected override FlowOutputSet<StringVarInfo> NewEmptySet()
        {
            return new FlowOutputSet<StringVarInfo>();
        }

        protected override void FlowThrough(FlowInputSet<StringVarInfo> inSet, LangElement statement, FlowOutputSet<StringVarInfo> outSet)
        {
            if (statement is ValueAssignEx)
            {
                var assign = statement as ValueAssignEx;
                var assigned=assign.LValue;
                var assignValue=assign.RValue;

                var varName = resolveVarName(assigned);
                var info = new StringVarInfo(varName);


                var str=resolveString(assignValue.Value);
                info.PossibleValues.Add(str);

                outSet.SetInfo(info.Name, info);
            }
        }

        private string resolveVarName(object lvalue)
        {
            var dirUse = (DirectVarUse)lvalue;
            return dirUse.VarName.Value;
        }

        private string resolveString(object expression)
        {
            return expression.ToString();
        }

        protected override IEnumerable<BlockDispatch> BlockDispatch(FlowInputSet<StringVarInfo> inSet, IEnumerable<ConditionalEdge> nextBlocks)
        {
            throw new NotImplementedException();
        }

        protected override void BlockMerge(FlowInputSet<StringVarInfo> inSet1, FlowInputSet<StringVarInfo> inSet2, FlowOutputSet<StringVarInfo> outSet)
        {
            foreach (var info in inSet1.CollectedInfo)
            {
                StringVarInfo toMerge;
                if (inSet2.TryGetInfo(info.Name, out toMerge))
                {
                    var newInfo=new StringVarInfo(info.Name);
                    newInfo.PossibleValues.UnionWith(info.PossibleValues);
                    newInfo.PossibleValues.UnionWith(toMerge.PossibleValues);
                    outSet.SetInfo(newInfo.Name, newInfo);
                }
                else
                {
                    outSet.SetInfo(info.Name,info);
                }
            }
        }

        protected override void IncludeMerge(IEnumerable<FlowInputSet<StringVarInfo>> inSets, FlowOutputSet<StringVarInfo> outSet)
        {
            throw new NotImplementedException();
        }

        protected override void CallMerge(IEnumerable<FlowInputSet<StringVarInfo>> inSets, FlowOutputSet<StringVarInfo> outSet)
        {
            throw new NotImplementedException();
        }

        protected override bool ConfirmAssumption(FlowInputSet<StringVarInfo> inSet, AssumptionCondition condition, FlowOutputSet<StringVarInfo> outSet)
        {
            //TODO resolve conditions
            outSet.FillFrom(inSet);
            return true;
        }
    }
}
