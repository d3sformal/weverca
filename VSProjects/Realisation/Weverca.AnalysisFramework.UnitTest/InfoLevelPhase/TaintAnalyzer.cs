/*
Copyright (c) 2012-2014 Natalia Tyrpakova, David Hauzar

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.AnalysisFramework.UnitTest.InfoLevelPhase
{
    class TaintAnalyzer : NextPhaseAnalyzer
    {

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
            string functionName = p.OwningPPGraph.FunctionName;
            if (nativeSanitizers.Contains(p.OwningPPGraph.FunctionName))
            {
                FunctionResolverBase.SetReturn(OutputSet, new MemoryEntry(Output.CreateInfo(false)));
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
            List<Value> argumentValues = new List<Value>();
            argumentValues.AddRange(p.LeftOperand.Value.ReadMemory(Output).PossibleValues);
            argumentValues.AddRange(p.RightOperand.Value.ReadMemory(Output).PossibleValues);

            p.SetValueContent(new MemoryEntry(Output.CreateInfo(mergeTaint(argumentValues))));
        }

        public override void VisitExtensionSink(ExtensionSinkPoint p)
        {
            var ends = p.OwningExtension.Branches.Select(c => c.Graph.End.OutSet).ToArray();
            OutputSet.MergeWithCallLevel(ends);

            p.ResolveReturnValue();
        }

        public override void VisitExtension(ExtensionPoint p)
        {
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

        private bool getTaint(ReadSnapshotEntryBase lValue)
        {
            var info = lValue.ReadMemory(Output);

            return mergeTaint(info.PossibleValues);
        }

        private bool mergeTaint(IEnumerable<Value> values)
        {
            foreach (var infoValue in values)
            {
                if (infoValue is UndefinedValue) continue;
                if (((InfoValue<bool>)infoValue).Data) return true;

            }
            return false;
        }

        private void setTaint(ReadWriteSnapshotEntryBase variable, bool taint)
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