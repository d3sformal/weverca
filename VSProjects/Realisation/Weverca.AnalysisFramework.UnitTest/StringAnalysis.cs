using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Expressions;

namespace Weverca.AnalysisFramework.UnitTest
{
    /// <summary>
    /// Simple string analysis implementation which demonstrates usege of ForwardAnalysis of CFG.
    /// </summary>
    class StringAnalysis : ForwardAnalysis<ValueInfo>
    {
        
        /// <summary>
        /// Creates analysis object for given entryMethod. Uses SimpleEvaluator and SimpleResolver for computations.
        /// </summary>
        /// <param name="entryMethod"></param>
        public StringAnalysis(ControlFlowGraph.ControlFlowGraph entryMethod)
            : base(entryMethod, new SimpleEvaluator(), new SimpleResolver(entryMethod))
        {
        }

        #region Analysis API methods implementation

        /// <summary>
        /// Handle merging input set's on BasicBlock join points.
        /// </summary>
        /// <param name="inSet1">Input set of block1 flow.</param>
        /// <param name="inSet2">Input set of block2 flow.</param>
        /// <param name="outSet">Output of merge.</param>
        protected override void BlockMerge(FlowInputSet<ValueInfo> inSet1, FlowInputSet<ValueInfo> inSet2, FlowOutputSet<ValueInfo> outSet)
        {            
            outSet.FillFrom(inSet2); //everything in inSet2 will be in output

            //find ValueInfo objects that are in "collision" and merge them into outSet.
            foreach (var toMerge1 in inSet1.CollectedInfo)
            {
                ValueInfo toMerge2,outInfo;                
                if (inSet2.TryGetInfo(toMerge1.Name, out toMerge2))
                {
                    //collision found
                    outInfo = new ValueInfo(toMerge1);
                    outInfo.MergeWith(toMerge2);                    
                }
                else
                {
                    outInfo = toMerge1;
                }

                outSet.SetInfo(outInfo.Name, outInfo);
            }
        }

        /// <summary>
        /// Determine that analysis could prove that conditionResult is always false according to inSet.
        /// </summary>
        /// <param name="inSet">Current input set of flow.</param>
        /// <param name="conditionResult">Result of some condition that should be proved to be false.</param>
        /// <returns>True if conditionResult can be proved to be always false. Returns false if we cannot prove, or conditionResult can be true.</returns>
        protected override bool CanProveFalse(FlowInputSet<ValueInfo> inSet, ValueInfo conditionResult)
        {
            return !conditionResult.PossibleValues.Contains(true) && conditionResult.PossibleValues.Contains(false);
        }

        /// <summary>
        /// Determine that analysis could prove that conditionResult is always true according to inSet.
        /// </summary>
        /// <param name="inSet">Current input set of flow.</param>
        /// <param name="conditionResult">Result of some condition that should be proved to be true.</param>
        /// <returns>True if conditionResult can be proved to be always true. Returns false if we cannot prove, or conditionResult can be false.</returns>
        protected override bool CanProveTrue(FlowInputSet<ValueInfo> inSet, ValueInfo conditionResult)
        {
            return !conditionResult.PossibleValues.Contains(false) && conditionResult.PossibleValues.Contains(true);
        }

        /// <summary>
        /// Is called for assuming given condition to be true.
        /// </summary>
        /// <param name="flow">Current input set of flow.</param>
        /// <param name="condition">Condition to be assumed.</param>
        protected override void Assume(FlowControler<ValueInfo> flow, AssumptionCondition_deprecated condition)
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
                        default:
                            throw new NotImplementedException("Operation: '" + part.PublicOperation + "', hasn't been implemented yet");
                    }
                }
            }
        }    

        /// <summary>
        /// Is called for applying call analysis result into caller flow.
        /// </summary>
        /// <param name="callerInSet">Input set from caller flow context.</param>
        /// <param name="callOutput">Output set acquired from call analysis.</param>
        /// <param name="outSet">Output of caller </param>
        protected override void ReturnedFromCall(FlowInputSet<ValueInfo> callerInSet, FlowInputSet<ValueInfo> callOutput, FlowOutputSet<ValueInfo> outSet)
        {            
            outSet.FillFrom(callerInSet);

            //TODO this in fact fills also data from local context of callOutput
            foreach (var valueInfo in callOutput.CollectedInfo)
            {
                outSet.SetInfo(valueInfo.Name, valueInfo);
            }
        }

        /// <summary>
        /// Extract info from callOutput about function's return value.
        /// </summary>
        /// <param name="callOutput">Output acquired from call analysis.</param>
        /// <returns>Object representing function's return value.</returns>
        protected override ValueInfo ExtractReturnValue(FlowInputSet<ValueInfo> callOutput)
        {
            //return values analysis is not implemented yet
            return null;
        }
        #endregion

        /// <summary>
        /// Try to assume information obtained from condition into outSet.
        /// </summary>
        /// <param name="condition">Conditin to be assumed as true.</param>
        /// <param name="outSet">Output set of flow after assumption.</param>
        private void tryAssumeEqual(BinaryEx condition, FlowOutputSet<ValueInfo> outSet)
        {
            var varUse = condition.LeftExpr as DirectVarUse;
            var assigned = condition.RightExpr as StringLiteral;

            if (varUse != null && assigned != null)
            {
                var assumedValue = new ValueInfo(varUse.VarName);
                assumedValue.PossibleValues.Add(assigned.Value);
                assumedValue.BoundCondition = condition;
                outSet.SetInfo(assumedValue.Name, assumedValue);
            }
        }

    }


    /// <summary>
    /// Simple resolver implementation for resolving declarations.
    /// </summary>
    class SimpleResolver : DeclarationResolver<ValueInfo>
    {
        ControlFlowGraph.ControlFlowGraph _entryCFG;
        public SimpleResolver(ControlFlowGraph.ControlFlowGraph entryCFG)
        {
            _entryCFG = entryCFG;
        }

        /// <summary>
        /// Get possible function names which can be represented by given functionName info.
        /// </summary>
        /// <param name="functionName">Info which represent's function names.</param>
        /// <returns>Possible function names.</returns>
        public override string[] GetFunctionNames(ValueInfo functionName)
        {
            var result = new List<string>();

            foreach (var name in functionName.PossibleValues)
            {
                result.Add(name.ToString());
            }

            return result.ToArray();
        }

        /// <summary>
        /// Prepares input for given function call.
        /// </summary>
        /// <param name="function">Function which call input is preparing.</param>
        /// <param name="args">Info about arguments specified for call.</param>
        /// <returns>Input set for function call.</returns>
        public override FlowInputSet<ValueInfo> PrepareCallInput(FunctionDecl function, ValueInfo[] args)
        {
            if (args.Length > 0)
                throw new NotImplementedException();

            return Flow.InSet;
        }

        /// <summary>
        /// Get entry point for specified function declaration.
        /// </summary>
        /// <param name="function">Function which entry point is needed.</param>
        /// <returns>BasicBlock which is entry point for given function.</returns>
        public override ControlFlowGraph.BasicBlock GetEntryPoint(FunctionDecl function)
        {
            var cfg= ControlFlowGraph.ControlFlowGraph.FromFunction(function);
            return cfg.start;
        }
    }

    /// <summary>
    /// Simple implementation of expression evaluator.
    /// </summary>
    class SimpleEvaluator : ExpressionEvaluator<ValueInfo>
    {
        /// <summary>
        /// Is called for assigning rValue into lValue.
        /// </summary>
        /// <param name="lValue"></param>
        /// <param name="rValue"></param>
        /// <returns>Result value of assign. For example it's used on assign chains (a = b = c).</returns>
        public override ValueInfo Assign(ValueInfo lValue, ValueInfo rValue)
        {
            lValue.PossibleValues = rValue.PossibleValues;
            Flow.OutSet.SetInfo(lValue.Name, lValue);
            return lValue;
        }

        /// <summary>
        /// Get declaration of given variable.
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public override ValueInfo Declare(DirectVarUse variable)
        {
            var declared = new ValueInfo(variable.VarName);
            Flow.OutSet.SetInfo(variable.VarName, declared);
            return declared;
        }

        /// <summary>
        /// Resolves binary expression for operation on given operands.
        /// </summary>
        /// <param name="op1"></param>
        /// <param name="operation"></param>
        /// <param name="op2"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates copy of given info.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public override ValueInfo Copy(ValueInfo info)
        {
            return new ValueInfo(info);
        }

        /// <summary>
        /// Creates representation of given string literal.
        /// </summary>
        /// <param name="literal"></param>
        /// <returns></returns>
        public override ValueInfo StringLiteral(StringLiteral literal)
        {
            var info = new ValueInfo();
            info.PossibleValues.Add(literal.Value);
            return info;
        }
    }

    /// <summary>
    /// Information that is handled for object during analysis.
    /// NOTE: FlowInfo should override Equals, GetHashCode - because of HashSet equality comparison
    /// </summary>
    class ValueInfo
    {
        /// <summary>
        /// Name of value if is present.
        /// </summary>
        public readonly VariableName Name;

        /// <summary>
        /// Determine that value is unbounded.
        /// </summary>
        public bool IsUnbounded { get; private set; }
        /// <summary>
        /// Condition that can bound's value.
        /// </summary>
        public BinaryEx BoundCondition;
        /// <summary>
        /// Values that are possible for represented object.
        /// </summary>
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
    }
}
