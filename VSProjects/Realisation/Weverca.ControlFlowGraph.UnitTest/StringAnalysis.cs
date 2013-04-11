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
    class StringAnalysis : ForwardAnalysis<ValueInfo>
    {
        public StringAnalysis(ControlFlowGraph entryMethod)
            : base(entryMethod, new SimpleEvaluator(),new SimpleResolver(entryMethod))
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
                    var newInfo = new ValueInfo(info);
                    newInfo.MergeWith(toMerge);
                    outSet.SetInfo(newInfo.Name, newInfo);
                }
                else
                {
                    outSet.SetInfo(info.Name, info);
                }
            }
        }

        protected override void Assume(FlowControler<ValueInfo> flow, AssumptionCondition condition)
        {
            if (condition.Parts.Count() == 1)
            {
                var part = condition.Parts.First() as BinaryEx;
                if (part != null)
                {
                    switch (part.PublicOperation)
                    {
                        case Operations.Equal:
                            tryAssumeEqual(part, flow.OutSet);
                            break;
                    }
                }
            }
        }

        private void tryAssumeEqual(BinaryEx eq, FlowOutputSet<ValueInfo> outSet)
        {
            var varUse = eq.LeftExpr as DirectVarUse;
            var assigned = eq.RightExpr as StringLiteral;

            if (varUse != null && assigned != null)
            {
                var assumedValue = new ValueInfo(varUse.VarName);
                assumedValue.PossibleValues.Add(assigned.Value);
                assumedValue.BoundCondition = eq;
                outSet.SetInfo(assumedValue.Name, assumedValue);
            }
        }

        protected override void ReturnedFromCall(FlowInputSet<ValueInfo> callerInSet, FlowInputSet<ValueInfo> callOutput, FlowOutputSet<ValueInfo> outSet)
        {
            //TODO resolve local/global
            outSet.FillFrom(callerInSet);
            foreach (var valueInfo in callOutput.CollectedInfo)
            {
                outSet.SetInfo(valueInfo.Name, valueInfo);
            }        
        }

        protected override ValueInfo extractReturnValue(FlowInputSet<ValueInfo> callOutput)
        {
            return null;
        }
    }

    class SimpleResolver : DeclarationResolver<ValueInfo>
    {
        ControlFlowGraph _entryCFG;
        public SimpleResolver(ControlFlowGraph entryCFG)
        {
            _entryCFG = entryCFG;
        }

        public override string[] GetFunctionNames(ValueInfo functionName)
        {
            var result = new List<string>();

            foreach (var name in functionName.PossibleValues)
            {
                result.Add(name.ToString());
            }

            return result.ToArray();
        }

        public override FlowInputSet<ValueInfo> PrepareCallInput(FunctionDecl function, ValueInfo[] args)
        {
            if (args.Length > 0)
                throw new NotImplementedException();

            return Flow.InSet;
        }

        public override BasicBlock GetEntryPoint(FunctionDecl function)
        {
            return _entryCFG.GetBasicBlock(function);
        }
    }

    class SimpleEvaluator : ExpressionEvaluator<ValueInfo>
    {

        public override ValueInfo Assign(ValueInfo p1, ValueInfo p2)
        {
            p1.PossibleValues = p2.PossibleValues;
            Flow.OutSet.SetInfo(p1.Name, p1);            
            return p1;
        }

        public override ValueInfo Declare(DirectVarUse x)
        {
            var declared = new ValueInfo(x.VarName);
            Flow.OutSet.SetInfo(x.VarName, declared);
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
        public bool IsUnbounded { get; private set; }
        public BinaryEx BoundCondition;

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
            BoundCondition = info.BoundCondition;
            IsUnbounded = info.IsUnbounded;
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

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var o = obj as ValueInfo;
            if (o == null)
                return false;

            var sameAttribs = IsUnbounded == o.IsUnbounded && BoundCondition == o.BoundCondition;
            var sameCounts = PossibleValues.Count == o.PossibleValues.Count;
            var sameValues = PossibleValues.Union(o.PossibleValues).Count() == PossibleValues.Count;

            return Name == o.Name && sameCounts && sameValues && sameAttribs;
        }

        public void SetUnbounded()
        {
            IsUnbounded = true;
            PossibleValues.Clear();
        }

        public void MergeWith(ValueInfo other)
        {
      /*      if (BoundCondition != other.BoundCondition)
            {
                //TODO more precise resolve condition merging
                IsUnbounded = true;
            }*/

            IsUnbounded |= other.IsUnbounded;
            if (IsUnbounded)
                PossibleValues.Clear();
            else
                PossibleValues.UnionWith(other.PossibleValues);
        }
    }
}
