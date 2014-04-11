using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;
using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.ProgramPoints;
using Weverca.Analysis.NativeAnalyzers;
using Weverca.Analysis;

namespace Weverca.Taint
{
    /// <summary>
    /// Class for analyzing a created program point graph for taint flows
    /// </summary>
    class TaintAnalyzer : NextPhaseAnalyzer
    {
        private ProgramPointBase _currentPoint;
        private NativeFunctionAnalyzer functAnalyzer;

        public List<AnalysisTaintWarning> analysisTaintWarnings = new List<AnalysisTaintWarning>();

        public override void VisitPoint(ProgramPointBase p)
        {
            //nothing to do
        }

        /// <summary>
        /// Visits a native analyzer program point. If function is a sanitizer, the output is sanitized,
        /// if it is a reporting function, a warning is created.
        /// </summary>
        /// <param name="p">program point to visit</param>
        public override void VisitNativeAnalyzer(NativeAnalyzerPoint p)
        {
            _currentPoint = p;
            string functionName = p.OwningPPGraph.FunctionName;         

            // 1. Get values of arguments of the function
            // TODO: code duplication: the following code, code in SimpleFunctionResolver, and NativeFunctionAnalyzer. Move the code to some API (? FlowInputSet)
            Input.SetMode(SnapshotMode.MemoryLevel);
            MemoryEntry argc = InputSet.ReadVariable(new VariableIdentifier(".argument_count")).ReadMemory(Input);
            Input.SetMode(SnapshotMode.InfoLevel);
            int argumentCount = ((IntegerValue)argc.PossibleValues.ElementAt(0)).Value;
            List<MemoryEntry> arguments = new List<MemoryEntry>();
            List<Value> argumentValues = new List<Value>();
            bool nullValue = false;
            for (int i = 0; i < argumentCount; i++)
            {
                arguments.Add(OutputSet.ReadVariable(Argument(i)).ReadMemory(OutputSet.Snapshot));
                argumentValues.AddRange(arguments.Last().PossibleValues);
                if (hasPossibleNullValue(OutputSet.ReadVariable(Argument(i)))) nullValue = true;
            }

            TaintInfo outputTaint = mergeTaint(argumentValues, nullValue);
            // try to sanitize the taint info
            sanitize(p, ref outputTaint);
            createWarnings(p, outputTaint);

            // 2. Propagate arguments to the return value.
            FunctionResolverBase.SetReturn(OutputSet, new MemoryEntry(Output.CreateInfo(outputTaint)));
        }

        /// <summary>
        /// Visits echo statement
        /// </summary>
        /// <param name="p">program point to visit</param>
        public override void VisitEcho(EchoStmtPoint p)
        {
            _currentPoint = p;
            var count = p.Echo.Parameters.Count;
            List<ValuePoint> valpoints = new List<ValuePoint>(p.Parameters);
            List<Value> argumentValues = new List<Value>();
            bool nullValue = false;

            foreach (ValuePoint val in valpoints)
            {
                argumentValues.AddRange(val.Value.ReadMemory(Output).PossibleValues);
                nullValue |= hasPossibleNullValue(val.Value);
            }

            TaintInfo outputTaint = mergeTaint(argumentValues, nullValue);
            createWarnings(p, outputTaint, new List<FlagType>() { FlagType.HTMLDirty });
        }

        /// <summary>
        /// Visits eval point
        /// </summary>
        /// <param name="p">program point to visit</param>
        public override void VisitRCall(RCallPoint p)
        {
            _currentPoint = p;
            if (p is EvalExPoint)
            {
                EvalExPoint pEval = p as EvalExPoint;
                List<Value> argumentValues = new List<Value>(pEval.EvalCode.Value.ReadMemory(Output).PossibleValues);
                bool nullValue = hasPossibleNullValue(pEval.EvalCode.Value);

                TaintInfo outputTaint = mergeTaint(argumentValues, nullValue);
                createWarnings(p, outputTaint, new List<FlagType>() { FlagType.HTMLDirty }, "Eval shoudn't contain anything from user input");
            }
        }

        /// <summary>
        /// Visits including point
        /// </summary>
        /// <param name="p">program point to visit</param>
        public override void VisitInclude(IncludingExPoint p)
        {
            _currentPoint = p;
            List<Value> argumentValues = new List<Value>(p.IncludePath.Value.ReadMemory(Output).PossibleValues);
            bool nullValue = hasPossibleNullValue(p.IncludePath.Value);

            TaintInfo outputTaint = mergeTaint(argumentValues, nullValue);
            createWarnings(p, outputTaint, new List<FlagType>() { FlagType.FilePathDirty });           

        }

        /// <summary>
        /// Visits unary expression point - if operation is print, warning might be created
        /// </summary>
        /// <param name="p"></param>
        public override void VisitUnary(UnaryExPoint p)
        {
            _currentPoint = p;
            if (p.Expression.PublicOperation == Operations.Print)
            {
                List<Value> argumentValues = new List<Value>(p.Operand.Value.ReadMemory(Output).PossibleValues);

                bool nullValue = hasPossibleNullValue(p.Operand.Value);

                TaintInfo outputTaint = mergeTaint(argumentValues, nullValue);
                createWarnings(p, outputTaint, new List<FlagType>(){FlagType.HTMLDirty});           
            }
        }

        /// <summary>
        /// If the function is a sanitizer, the sanitized taint flows are removed
        /// </summary>
        /// <param name="p">program point with a function</param>
        /// <param name="taintInfo">TaintInfo that is being sanitized</param>
        private void sanitize(NativeAnalyzerPoint p,ref TaintInfo taintInfo)
        {
            NativeAnalyzerMethod method = p.Analyzer.Method;
            QualifiedName functName = getMethodName(p);
            functAnalyzer = NativeFunctionAnalyzer.CreateInstance();

            List<FlagType> flags;

            if (functAnalyzer.SanitizingFunctions.TryGetValue(functName, out flags))
            {
                taintInfo.setSanitized(flags);  
            }
        }

        /// <summary>
        /// If function is a reporting function, a warning might be created.
        /// </summary>
        /// <param name="p">program point with a function</param>
        /// <param name="taintInfo">TaintInfo that is being sanitized</param>
        private void createWarnings(NativeAnalyzerPoint p,TaintInfo taintInfo)
        {
            NativeAnalyzerMethod method = p.Analyzer.Method;
            QualifiedName functName = getMethodName(p);
            functAnalyzer = NativeFunctionAnalyzer.CreateInstance();

             List<FlagType> flags;

            if (functAnalyzer.ReportingFunctions.TryGetValue(functName, out flags))
            {
                createWarnings(p, taintInfo, flags);           
            }
        }

        private void createWarnings(ProgramPointBase p, TaintInfo taintInfo, List<FlagType> flags, String message = null )
        {
            foreach (FlagType flag in flags)
            {
                if (!taintInfo.taint.get(flag)) continue;
                String taint = taintInfo.print(flag);
                String currentScript = "";
                if (p.OwningPPGraph.OwningScript != null) currentScript = p.OwningPPGraph.OwningScript.FullName;
                AnalysisTaintWarning warning;
                if (message == null ) warning = new AnalysisTaintWarning(currentScript, taint,
                        p.Partial,p, flag);
                else warning = new AnalysisTaintWarning(currentScript, message ,taint,
                        p.Partial, p, flag);
                int index = analysisTaintWarnings.IndexOf(warning);
                if (index != -1)
                {
                    analysisTaintWarnings.RemoveAt(index);
                }
                analysisTaintWarnings.Add(warning);
            }  
        }

        /// <summary>
        /// Gets the method name from NativeAnalyzerPoint
        /// </summary>
        /// <param name="p">point to get the method from</param>
        /// <returns>a method name as a QualifiedName</returns>
        private QualifiedName getMethodName(NativeAnalyzerPoint p)
        {
            String functName = p.OwningPPGraph.FunctionName;
            return new QualifiedName(new Name(functName));
        }

        /// <summary>
        /// Visits a binary expression point and propagates the taint from both the operands.
        /// </summary>
        /// <param name="p">point to visit</param>
        public override void VisitBinary(BinaryExPoint p)
        {
            _currentPoint = p;
            List<Value> argumentValues = new List<Value>();
            argumentValues.AddRange(p.LeftOperand.Value.ReadMemory(Output).PossibleValues);
            argumentValues.AddRange(p.RightOperand.Value.ReadMemory(Output).PossibleValues);

            bool nullValue = hasPossibleNullValue(p.LeftOperand.Value) || hasPossibleNullValue(p.RightOperand.Value);

            p.SetValueContent(new MemoryEntry(Output.CreateInfo(mergeTaint(argumentValues,nullValue))));
        }

        /// <summary>
        /// Visits an extension sink point
        /// </summary>
        /// <param name="p">point to visit</param>
        public override void VisitExtensionSink(ExtensionSinkPoint p)
        {
            _currentPoint = p;
            var ends = p.OwningExtension.Branches.Select(c => c.Graph.End.OutSet).ToArray();
            OutputSet.MergeWithCallLevel(ends);

            p.ResolveReturnValue();
        }

        /// <summary>
        /// Visits an extension point and propagates the taint
        /// </summary>
        /// <param name="p">point to visit</param>
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

        /// <summary>
        /// Visits an assign point and propagates the taint to the assigned operand
        /// </summary>
        /// <param name="p">point to visit</param>
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

        /// <summary>
        /// Visits a jump statement point
        /// </summary>
        /// <param name="p">point to visit</param>
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

        /// <summary>
        /// Gets the complete taint information
        /// </summary>
        /// <param name="lValue">entry to get the taint for</param>
        /// <returns>the taint information of given entry</returns>
        private TaintInfo getTaint(ReadSnapshotEntryBase lValue)
        {
            var info = lValue.ReadMemory(Output);

            return mergeTaint(info.PossibleValues,hasPossibleNullValue(lValue));
        }

        /// <summary>
        /// Merges multiple taint information into one.
        /// </summary>
        /// <param name="values">info values with taint information</param>
        /// <param name="nullValue">indicator of null flow</param>
        /// <returns>merged taint information</returns>
        private TaintInfo mergeTaint(IEnumerable<Value> values, bool nullValue)
        {
            TaintInfo info = new TaintInfo();
            info.point = _currentPoint;
            TaintPriority priority = new TaintPriority(true);
            //if _currentPoint is a BinaryExPoint, its priority is high whenever one of the values has high priority
            if (values.Count() == 0 || _currentPoint is BinaryExPoint) priority.setAll(false);
            Taint taint = new Taint(false);
            bool existsNullFlow = false;
            
            foreach (var infoValue in values)
            {         
                if (infoValue is UndefinedValue)
                {
                    continue;
                }
                if (!(infoValue is InfoValue<TaintInfo>)) continue;
                TaintInfo varInfo = (((InfoValue<TaintInfo>)infoValue).Data);
                existsNullFlow |= varInfo.nullValue;
               
                /* If _currentPoint is not BinaryExPoint, the priority is low whenever one of the values
                has a low priority. 
                If _currentPoint is BinaryExPoint, the priority is high whenever one of the values has
                a high priority */
                if (!(_currentPoint is BinaryExPoint)) priority.copyTaint(false, varInfo.priority);
                if (_currentPoint is BinaryExPoint) priority.copyTaint(true, varInfo.priority);

                taint.copyTaint(true, varInfo.taint);

                if (!varInfo.taint.allFalse()) info.possibleTaintFlows.Add(varInfo);       
            }

            if (nullValue && !existsNullFlow)
            {
                taint.setAll(true);
                if (values.Count() == 0) priority.setAll(true);
                info.nullValue = true;
            }

            info.priority = priority;
            info.taint = taint;
            return info;
        }

        /// <summary>
        /// Sets the taint information to the given entry
        /// </summary>
        /// <param name="variable">entry to set the taint to</param>
        /// <param name="taint">taint to be set</param>
        private void setTaint(ReadWriteSnapshotEntryBase variable, TaintInfo taint)
        {
            var infoValue = Output.CreateInfo(taint);
            variable.WriteMemory(Output, new MemoryEntry(infoValue));
        }

        /// <summary>
        /// Converts the given integer to VariableIdentifier
        /// </summary>
        /// <param name="index">integer to convert</param>
        /// <returns>a function argument as a VariableIdentifier</returns>
        private static VariableIdentifier Argument(int index)
        {
            if (index < 0)
            {
                throw new NotSupportedException("Cannot get argument variable for negative index");
            }
            return new VariableIdentifier(".arg" + index);
        }

        /// <summary>
        /// Determines whether variable may be undefined or null
        /// </summary>
        /// <param name="variable">variable to check</param>
        /// <returns>true if variable may be undefined or null</returns>
        private bool hasPossibleNullValue(ReadSnapshotEntryBase variable)
        {
            bool nullValue = false;
            OutputSet.Snapshot.SetMode(SnapshotMode.MemoryLevel);
            var values = variable.ReadMemory(OutputSet.Snapshot).PossibleValues;

            foreach (Value value in values)
            {
                if (value is UndefinedValue)
                {
                    nullValue = true;
                    break;
                }
            }
            OutputSet.Snapshot.SetMode(SnapshotMode.InfoLevel);
            return nullValue;     
        }

    }
}

