using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.Taint
{
    class TaintAnalyzer : NextPhaseAnalyzer
    {
        private ProgramPointBase _currentPoint;
        private static readonly List<string> nativeSanitizers = new List<string>() 
        {
            "strlen"
        };

        private static readonly List<string> taintedVariables = new List<string>() 
        {
            "$_POST"
        };

        public override void VisitPoint(ProgramPointBase p)
        {
            //nothing to do
        }

        public override void VisitNativeAnalyzer(NativeAnalyzerPoint p)
        {
            _currentPoint = p;
            string functionName = p.OwningPPGraph.FunctionName;
            if (nativeSanitizers.Contains(p.OwningPPGraph.FunctionName))
            {
                TaintInfo noTaint = new TaintInfo();
                noTaint.highPriority = false;
                FunctionResolverBase.SetReturn(OutputSet, new MemoryEntry(Output.CreateInfo(noTaint)));
                return;
            }

            // If a native function is not sanitizer, propagates taint status from arguments to return value

            // 1. Get values of arguments of the function
            // TODO: code duplication: the following code, code in SimpleFunctionResolver, and NativeFunctionAnalyzer. Move the code to some API (? FlowInputSet)
            Input.SetMode(SnapshotMode.MemoryLevel);
            MemoryEntry argc = InputSet.ReadVariable(new VariableIdentifier(".argument_count")).ReadMemory(Input);
            Input.SetMode(SnapshotMode.InfoLevel);
            int argumentCount = ((IntegerValue)argc.PossibleValues.ElementAt(0)).Value;
            List<MemoryEntry> arguments = new List<MemoryEntry>();
            List<Value> argumentValues = new List<Value>();
            for (int i = 0; i < argumentCount; i++)
            {
                arguments.Add(OutputSet.ReadVariable(Argument(i)).ReadMemory(OutputSet.Snapshot));
                argumentValues.AddRange(arguments.Last().PossibleValues);
            }

            // 2. Propagate arguments to the return value.
            FunctionResolverBase.SetReturn(OutputSet, new MemoryEntry(Output.CreateInfo(mergeTaint(argumentValues))));
        }

        public override void VisitBinary(BinaryExPoint p)
        {
            _currentPoint = p;
            List<Value> argumentValues = new List<Value>();
            argumentValues.AddRange(p.LeftOperand.Value.ReadMemory(Output).PossibleValues);
            argumentValues.AddRange(p.RightOperand.Value.ReadMemory(Output).PossibleValues);

            p.SetValueContent(new MemoryEntry(Output.CreateInfo(mergeTaint(argumentValues))));
        }

        public override void VisitExtensionSink(ExtensionSinkPoint p)
        {
            _currentPoint = p;
            var ends = p.OwningExtension.Branches.Select(c => c.Graph.End.OutSet).ToArray();
            OutputSet.MergeWithCallLevel(ends);

            p.ResolveReturnValue();
        }

        public override void VisitExtension(ExtensionPoint p)
        {
            _currentPoint = p;
            //TODO resolve caller properly
            var callPoint = p.Caller as FunctionCallPoint;
            var decl = p.Graph.SourceObject as FunctionDecl;
            if (decl == null)
                return;

            var signature = decl.Signature;
            var callSignature = callPoint.CallSignature;
            var enumerator = callPoint.Arguments.GetEnumerator();
            for (int i = 0; i < signature.FormalParams.Count; ++i)
            {
                enumerator.MoveNext();

                //determine that argument is passed by reference or not
                var param = signature.FormalParams[i];
                var callParam = callSignature.Value.Parameters[i];

                var argumentVar = Output.GetVariable(new VariableIdentifier(param.Name));
                var argument = enumerator.Current.Value;

                if (callParam.PublicAmpersand || param.PassedByRef)
                {
                    argumentVar.SetAliases(Output, argument);
                }
                else
                {
                    var argumentValue = argument.ReadMemory(Output);
                    argumentVar.WriteMemory(Output, argumentValue);
                }
            }
        }

        public override void VisitAssign(AssignPoint p)
        {
            _currentPoint = p;
            var source = p.ROperand.Value;
            var target = p.LOperand.LValue;

            if (target == null || source == null)
                //Variable has to be LValue
                return;

            var sourceTaint = getTaint(source);

            var finalPropagation = sourceTaint;

            setTaint(target, finalPropagation);
        }

        public override void VisitJump(JumpStmtPoint p)
        {
            _currentPoint = p;
            switch (p.Jump.Type)
            {
                case JumpStmt.Types.Return:
                    var returnVar = Output.GetLocalControlVariable(SnapshotBase.ReturnValue);
                    var returnValue = p.Expression.Value.ReadMemory(Input);
                    returnVar.WriteMemory(Output, returnValue);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private TaintInfo getTaint(ReadSnapshotEntryBase lValue)
        {
            var info = lValue.ReadMemory(Output);

            return mergeTaint(info.PossibleValues);
        }

        private TaintInfo mergeTaint(IEnumerable<Value> values)
        {
            TaintInfo info = new TaintInfo();
            bool highPriority = true;
            foreach (var infoValue in values)
            {
                if (infoValue is UndefinedValue) continue;
                TaintInfo varInfo = (((InfoValue<TaintInfo>)infoValue).Data);
                if (varInfo.possibleTaintFlows.Count == 0) highPriority = false;
                if (varInfo.highPriority == false) highPriority = false;
                foreach (TaintFlow flow in varInfo.possibleTaintFlows)
                {
                    TaintFlow newFlow = new TaintFlow(flow);
                    newFlow.addPointToTaintFlow(_currentPoint);
                    info.possibleTaintFlows.Add(newFlow);
                }
            }
            info.highPriority = highPriority;
            return info;
        }

        private void setTaint(ReadWriteSnapshotEntryBase variable, TaintInfo taint)
        {
            var infoValue = Output.CreateInfo(taint);
            variable.WriteMemory(Output, new MemoryEntry(infoValue));
        }

        private static VariableIdentifier Argument(int index)
        {
            if (index < 0)
            {
                throw new NotSupportedException("Cannot get argument variable for negative index");
            }
            return new VariableIdentifier(".arg" + index);
        }

    }
}

