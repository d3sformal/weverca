using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Memory;
using Weverca.Analysis.Expressions;
using Weverca.ControlFlowGraph;

namespace Weverca.Analysis.UnitTest
{
    class SimpleAnalysis:ForwardAnalysis
    {
        public SimpleAnalysis(ControlFlowGraph.ControlFlowGraph entryCFG)
            : base(entryCFG)           
        {
        }

        #region Resolvers that are used during analysis
        protected override ExpressionEvaluator createExpressionEvaluator()
        {
            return new SimpleExpressionEvaluator();
        }

        protected override FlowResolver createFlowResolver()
        {
            return new SimpleFlowResolver();
        }

        protected override FunctionResolver createFunctionResolver()
        {
            return new SimpleFunctionResolver();
        }

        protected override AbstractSnapshot createSnapshot()
        {
            return new VirtualReferenceModel.Snapshot();
        }
        #endregion
    }


 
    /// <summary>
    /// Expression evaluation is resovled here
    /// </summary>
    class SimpleExpressionEvaluator : ExpressionEvaluator
    {
        public override void Assign(VariableName target, MemoryEntry value)
        {
            Flow.OutSet.Assign(target, value);
        }

        public override void AliasAssign(VariableName target, AliasValue alias)
        {
            Flow.OutSet.Assign(target, alias);
        }

        public override MemoryEntry BinaryEx(MemoryEntry leftOperand, Operations operation, MemoryEntry rightOperand)
        {
            switch (operation)
            {
                case Operations.Equal:
                    return areEqual(leftOperand, rightOperand);            
                default:
                    throw new NotImplementedException();
            }
        }

        #region Expression evaluation helpers

        private MemoryEntry areEqual(MemoryEntry left, MemoryEntry right)
        {
            
            var result = new List<BooleanValue>();
            if (canBeDifferent(left,right))
            {
                result.Add(Flow.OutSet.CreateBool(false));
            }

            if (canBeSame(left,right))
            {
                result.Add(Flow.OutSet.CreateBool(true));
            }

            return new MemoryEntry(result.ToArray());
        }

        private bool canBeSame(MemoryEntry left, MemoryEntry right)
        {
            if (containsAnyValue(left)||containsAnyValue(right)) 
                return true;

            foreach (var possibleValue in left.PossibleValues)
            {
                if (right.PossibleValues.Contains(possibleValue))
                {
                    return true;
                }
            }
            return false;
        }

        private bool canBeDifferent(MemoryEntry left, MemoryEntry right)
        {
            if (containsAnyValue(left) || containsAnyValue(right))
                return true;

            if (left.PossibleValues.Count() > 1 || left.PossibleValues.Count() > 1)
            {
                return true;
            }

            return !left.Equals(right);
        }

        private bool containsAnyValue(MemoryEntry entry)
        {
            //TODO Undefined value maybe is not correct to be treated as any value
            return entry.PossibleValues.Contains(Flow.OutSet.AnyValue) || entry.PossibleValues.Contains(Flow.OutSet.UndefinedValue);
        }

        #endregion
    }
    
    /// <summary>
    /// Resolving function names and function initializing
    /// </summary>
    class SimpleFunctionResolver : FunctionResolver
    {
        /// <summary>
        /// Table of native analyzers
        /// </summary>
        Dictionary<string, NativeAnalyzer> _nativeAnalyzers = new Dictionary<string, NativeAnalyzer>()
        {
            {"strtolower",new NativeAnalyzer(_strtolower)},
            {"strtoupper",new NativeAnalyzer(_strtoupper)},
            {"concat",new NativeAnalyzer(_concat)}
        };

        /// <summary>
        /// Resolving names according to given memory entry
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public override QualifiedName[] GetFunctionNames(MemoryEntry functionName)
        {
            var result = new List<QualifiedName>();
            foreach (StringValue stringName in functionName.PossibleValues)
            {
                result.Add(new QualifiedName(new Name(stringName.Value)));
            }

            return result.ToArray();
        }

        /// <summary>
        /// Resolve return value from all possible calls
        /// </summary>
        /// <param name="calls"></param>
        /// <returns></returns>
        public override MemoryEntry ResolveReturnValue(ProgramPointGraph[] calls)
        {
            var possibleMemoryEntries = from call in calls select call.End.OutSet.ReadValue(call.End.OutSet.ReturnValue).PossibleValues;
            var flattenValues = possibleMemoryEntries.SelectMany((i) => i);


            return new MemoryEntry(flattenValues.ToArray());
        }

        /// <summary>
        /// Initialize call into callInput. 
        /// 
        /// NOTE: 
        ///     arguments are already initialized
        ///     sharing program point graphs is possible
        /// </summary>
        /// <param name="callInput"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public override ProgramPointGraph InitializeCall(FlowOutputSet callInput, QualifiedName name)
        {
            NativeAnalyzer analyzer;
        
            if (_nativeAnalyzers.TryGetValue(name.Name.Value,out analyzer))
            {
                //we have native analyzer - create it's program point 
                return ProgramPointGraph.ForNative(analyzer);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        #region Native analyzers
        /// <summary>
        /// Analyzer method for strtolower php function
        /// </summary>
        /// <param name="flow"></param>
        private static void _strtolower(FlowController flow)
        {
            var arg = flow.InSet.ReadValue(flow.InSet.Argument(0));

            var possibleValues = new List<StringValue>();

            foreach (StringValue possible in arg.PossibleValues)
            {
                var lower = flow.OutSet.CreateString(possible.Value.ToLower());
                possibleValues.Add(lower);
            }


            flow.OutSet.Assign(flow.OutSet.ReturnValue,new MemoryEntry(possibleValues.ToArray()));
        }

        /// <summary>
        /// Analyzer method for strtolower php function
        /// </summary>
        /// <param name="flow"></param>
        private static void _strtoupper(FlowController flow)
        {
            var arg = flow.InSet.ReadValue(flow.InSet.Argument(0));

            var possibleValues = new List<StringValue>();

            foreach (StringValue possible in arg.PossibleValues)
            {
                var lower = flow.OutSet.CreateString(possible.Value.ToUpper());
                possibleValues.Add(lower);
            }


            flow.OutSet.Assign(flow.OutSet.ReturnValue, new MemoryEntry(possibleValues.ToArray()));
        }



        private static void _concat(FlowController flow)
        {
            var arg0 = flow.InSet.ReadValue(flow.InSet.Argument(0));
            var arg1 = flow.InSet.ReadValue(flow.InSet.Argument(1));

            var possibleValues = new List<StringValue>();

            foreach (StringValue possible0 in arg0.PossibleValues)
            {
                foreach (StringValue possible1 in arg1.PossibleValues)
                {
                    possibleValues.Add(flow.OutSet.CreateString(possible0.Value + possible1.Value));
                }
               
            }


            flow.OutSet.Assign(flow.OutSet.ReturnValue, new MemoryEntry(possibleValues.ToArray()));
        }
        #endregion
    }


    /// <summary>
    /// Controlling flow actions during analysis
    /// </summary>
    class SimpleFlowResolver : FlowResolver
    {
        private FlowOutputSet _outSet;

        /// <summary>
        /// Represents method which is used for confirming assumption condition. Assumption can be declined - it means that we can prove, that condition CANNOT be ever satisfied.
        /// </summary>  
        /// <returns>False if you can prove that condition cannot be ever satisfied, true otherwise.</returns>
        public override bool ConfirmAssumption(FlowOutputSet outSet, AssumptionCondition condition, MemoryEntry[] expressionParts)
        {
            _outSet = outSet;

            bool willAssume;
            switch (condition.Form)
            {
                case ConditionForm.All:
                    willAssume = needsAll(condition.Parts, expressionParts);
                    break;

                default:
                    //we has to assume, because we can't disprove assumption
                    willAssume = true;
                    break;
            }

            if (willAssume)
            {
                processAssumption(condition, expressionParts);
            }

            return willAssume;
        }


        public override void CallDispatchMerge(FlowOutputSet callerOutput, ProgramPointGraph[] dispatchedProgramPointGraphs)
        {
            //TODO
        }

        #region Assumption helpers
        private bool needsAll(IEnumerable<Expressions.Postfix> conditionParts, MemoryEntry[] evaluatedParts)
        {
            //we are searching for one part, that can be evaluated only as false

            foreach (var evaluatedPart in evaluatedParts)
            {
                if (evalOnlyFalse(evaluatedPart))
                {
                    return false;
                }
            }

            //can disprove any part
            return true;
        }

        private bool evalOnlyFalse(MemoryEntry evaluatedPart)
        {
            foreach (var value in evaluatedPart.PossibleValues)
            {
                var boolean = value as BooleanValue;
                if (boolean != null)
                {
                    if (!boolean.Value)
                    {
                        //false cannot be evaluted as true
                        continue;
                    }
                }

                /* //Because of tests we use UndefinedValue as non deterministic     
                 * if (value == Flow.OutSet.UndefinedValue)
                      {
                          //undefined value ise evaluated as false
                          continue;
                      }*/

                //Thid part can be evaluated as true
                return false;
            }

            //no of possible values can be evaluted as true
            return true;
        }

        /// <summary>
        /// Assume valid condition into output set
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="expressionParts"></param>
        private void processAssumption(AssumptionCondition condition, MemoryEntry[] expressionParts)
        {
            if (condition.Form == ConditionForm.All)
            {
                if (condition.Parts.Count() == 1)
                {
                    assumeBinary(condition.Parts.First().SourceElement as BinaryEx, expressionParts[0]);
                }
            }
        }

        private void assumeBinary(BinaryEx exp, MemoryEntry expResult)
        {
            if (exp == null)
            {
                return;
            }
            switch (exp.PublicOperation)
            {
                case Operations.Equal:
                    assumeEqual(exp.LeftExpr, exp.RightExpr, expResult);
                    break;
            }
        }

        private void assumeEqual(LangElement left, LangElement right, MemoryEntry result)
        {
            var leftVar = left as DirectVarUse;
            var rightVal = right as StringLiteral;

            if (leftVar == null)
            {
                //for simplicity resolve only $var==stringliteral statements
                return;
            }

            _outSet.Assign(leftVar.VarName, _outSet.CreateString(rightVal.Value as string));
        }
        #endregion
    }

}
