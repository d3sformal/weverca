using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.Analysis.Memory;
using Weverca.Analysis.Expressions;
using Weverca.ControlFlowGraph;

namespace Weverca.Analysis.UnitTest
{
    class SimpleAnalysis:ForwardAnalysis
    {
        public SimpleAnalysis(ControlFlowGraph.ControlFlowGraph entryCFG)
            : base(
            entryCFG,
            new SimpleExpressionEval(),
            new SimpleDeclarationResolver(),
            ()=>new VirtualReferenceModel.Snapshot())
        {
        }

        protected override void FlowThrough(FlowControler flow, LangElement statement)
        {
            throw new NotImplementedException();
        }

        protected override bool ConfirmAssumption(AssumptionCondition condition,MemoryEntry[] expressionParts)
        {
            return true;   
        }

        protected override void BlockMerge(FlowInputSet inSet1, FlowInputSet inSet2, FlowOutputSet outSet)
        {
            throw new NotImplementedException();
        }

        protected override void IncludeMerge(IEnumerable<FlowInputSet> inSets, FlowOutputSet outSet)
        {
            throw new NotImplementedException();
        }

        protected override void CallMerge(FlowInputSet inSet1, FlowInputSet inSet2, FlowOutputSet outSet)
        {
            throw new NotImplementedException();
        }

        protected override Memory.MemoryEntry ResolveReturnValue(ProgramPointGraph[] calls)
        {
            throw new NotImplementedException();
        }

        protected override void ReturnedFromCall(FlowInputSet callerInSet, FlowInputSet callOutput, FlowOutputSet outSet)
        {
            throw new NotImplementedException();
        }
    }


    class SimpleExpressionEval : ExpressionEvaluator
    {
        public override void Assign(PHP.Core.VariableName target, MemoryEntry value)
        {
            Flow.OutSet.Output.Assign(target, value);
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
            return new MemoryEntry(Flow.OutSet.Output.CreateString(x.Value as String));
        }

        public override MemoryEntry ResolveVariable(PHP.Core.VariableName variable)
        {
            return Flow.InSet.Input.ReadValue(variable);
        }
    }

    class SimpleDeclarationResolver : DeclarationResolver
    {
        public override string[] GetFunctionNames(MemoryEntry functionName)
        {
            throw new NotImplementedException();
        }

        public override FlowInputSet PrepareCallInput(FunctionDecl function, MemoryEntry[] args)
        {
            throw new NotImplementedException();
        }

        public override BasicBlock GetEntryPoint(FunctionDecl function)
        {
            throw new NotImplementedException();
        }

        public override void CallDispatch(PHP.Core.QualifiedName name, MemoryEntry[] args)
        {
            throw new NotImplementedException();
        }

        public override void CallDispatch(MemoryEntry functionName, MemoryEntry[] args)
        {
            throw new NotImplementedException();
        }
    }
}
