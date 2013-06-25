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
    /// Controlling flow actions during analysis
    /// </summary>
    class SimpleFlowResolver : FlowResolver
    {
        /// <summary>
        /// Represents method which is used for confirming assumption condition. Assumption can be declined - it means that we can prove, that condition CANNOT be ever satisfied.
        /// </summary>  
        /// <returns>False if you can prove that condition cannot be ever satisfied, true otherwise.</returns>
        public override bool ConfirmAssumption(AssumptionCondition condition, MemoryEntry[] expressionParts)
        {
            return true;
        }

        public override void CallDispatchMerge(FlowOutputSet callerOutput, FlowOutputSet[] callsOutputs)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Expression evaluation is resovled here
    /// </summary>
    class SimpleExpressionEvaluator : ExpressionEvaluator
    {
        public override void Assign(PHP.Core.VariableName target, MemoryEntry value)
        {
            Flow.OutSet.Assign(target, value);
        }

        public override void Declare(DirectVarUse x)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry BinaryEx(MemoryEntry leftOperand, Operations operation, MemoryEntry rightOperand)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry StringLiteral(StringLiteral x)
        {
            return new MemoryEntry(Flow.OutSet.CreateString(x.Value as String));
        }

        public override MemoryEntry ResolveVariable(VariableName variable)
        {
            return Flow.InSet.ReadValue(variable);
        }
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
            {"strtolower",new NativeAnalyzer(_strtolower)}
        };

        /// <summary>
        /// Resolving names according to given memory entry
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public override string[] GetFunctionNames(MemoryEntry functionName)
        {
            throw new NotImplementedException();
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
        /// NOTE: arguments are already initialized
        /// </summary>
        /// <param name="callInput"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public override ProgramPointGraph InitializeCall(FlowOutputSet callInput, QualifiedName name)
        {
            NativeAnalyzer analyzer;
        
            if (_nativeAnalyzers.TryGetValue(name.Name.Value,out analyzer))
            {
                //we have native analyzer - create it's program point (NOTE: sharing program points is possible)
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
        private static void _strtolower(FlowControler flow)
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
        #endregion
    }
}
