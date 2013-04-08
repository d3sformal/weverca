using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

using Weverca.ControlFlowGraph.Analysis;
using Weverca.ControlFlowGraph.Analysis.Expressions;

namespace Weverca.ControlFlowGraph.UnitTest
{
    class StringAnalysis2:ForwardAnalysis<ValueInfo>
    {
        public StringAnalysis2(ControlFlowGraph entryMethod)
            : base(entryMethod,new SimpleEvaluator())
        {
        }

        protected override bool canProveFalse(FlowInputSet<ValueInfo> inSet, ValueInfo conditionResult)
        {
            return !conditionResult.PossibleValues.Contains(true) && conditionResult.PossibleValues.Contains(false);
        }

        protected override bool canProveTrue(FlowInputSet<ValueInfo> inSet, ValueInfo conditionResult)
        {
            return !conditionResult.PossibleValues.Contains(false) && conditionResult.PossibleValues.Contains(true);
        }

        protected override void BlockMerge(FlowInputSet<ValueInfo> inSet1, FlowInputSet<ValueInfo> inSet2, FlowOutputSet<ValueInfo> outSet)
        {
            outSet.FillFrom(inSet2);
            foreach (var info in inSet1.CollectedInfo)
            {
                ValueInfo toMerge;
                if (inSet2.TryGetInfo(info.Name, out toMerge))
                {
                    var newInfo = new ValueInfo(info.Name);
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
    }

    class SimpleEvaluator:ExpressionEvaluator<ValueInfo>
    {

        public override ValueInfo Assign(ValueInfo p1, ValueInfo p2)
        {
            p1.PossibleValues = p2.PossibleValues;
            OutSet.SetInfo(p1.Name, p1);
            return p1;
        }

        public override ValueInfo Declare(DirectVarUse x)
        {
            var declared=new ValueInfo(x.VarName);
            OutSet.SetInfo(x.VarName,declared );
            return declared;
        }

        public override ValueInfo BinaryEx(ValueInfo op1, Operations operation, ValueInfo op2)
        {
            switch (operation)
            {
                case Operations.Equal:
                    var boolRes = new ValueInfo();

                    var pv1 = op1.PossibleValues;
                    var pv2 = op2.PossibleValues;

                    if (pv1.Count > 1 || pv2.Count > 1)
                    {
                        //they have choice to be not equal
                        boolRes.PossibleValues.Add(false);
                    }

                    if (pv1.Any((val) => pv2.Contains(val)))
                    {
                        //they have choice to be equal
                        boolRes.PossibleValues.Add(true);
                    }

                    
                    return boolRes;
                default:
                    throw new NotImplementedException();
            }
        }

        public override ValueInfo Copy(ValueInfo info)
        {
            return new ValueInfo(info);
        }

        public override ValueInfo StringLiteral(StringLiteral x)
        {
            var info = new ValueInfo();
            info.PossibleValues.Add(x.Value);
            return info;
        }
    }

    class ValueInfo
    {
        /// <summary>
        /// Name of value if is present
        /// </summary>
        public readonly VariableName Name;

        public HashSet<object> PossibleValues = new HashSet<object>();

        public ValueInfo()
        {
           
        }

        public ValueInfo(VariableName name)
        {
            Name = name;
        }

        public ValueInfo(ValueInfo info)
        {
            Name = info.Name;
            PossibleValues.UnionWith(info.PossibleValues);
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append(Name).Append(": ");
            foreach (var val in PossibleValues)
            {
                b.Append("'").Append(val).Append("', ");
            }
            return b.ToString();
        }
        
    }
}
