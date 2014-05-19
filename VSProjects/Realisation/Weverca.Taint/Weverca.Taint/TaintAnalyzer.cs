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
using Weverca.MemoryModels.CopyMemoryModel;

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
            List<ValueInfo> values = new List<ValueInfo>();
            bool nullValue = false;

            for (int i = 0; i < argumentCount; i++)
            {
                arguments.Add(OutputSet.ReadVariable(Argument(i)).ReadMemory(OutputSet.Snapshot));
                List<Value> argumentValues = new List<Value>(arguments.Last().PossibleValues);
                if (hasPossibleNullValue(OutputSet.ReadVariable(Argument(i)))) nullValue = true;
                VariableIdentifier varID = null;
                Value toRemove = null;
                foreach (Value val in argumentValues)
                {
                    if (val is InfoValue<VariableIdentifier>)
                    {
                        varID = (val as InfoValue<VariableIdentifier>).Data;
                        toRemove = val;
                    }
                }
                if (toRemove != null) argumentValues.Remove(toRemove);
                values.Add(new ValueInfo(argumentValues, varID));
            }

            TaintInfo outputTaint = mergeTaint(values, nullValue);
            // try to sanitize the taint info
            if (outputTaint != null)
            {
                sanitize(p, ref outputTaint);
                warningsReportingFunct(p, outputTaint);
            }

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
            List<ValueInfo> values = new List<ValueInfo>();
            bool nullValue = false;

            foreach (ValuePoint val in valpoints)
            {
                var varID = getVariableIdentifier(val.Value);
                List<Value> argumentValues = new List<Value>(val.Value.ReadMemory(Output).PossibleValues);
                values.Add(new ValueInfo(argumentValues, varID));
;                nullValue |= hasPossibleNullValue(val.Value);
            }

            TaintInfo outputTaint = mergeTaint(values, nullValue);
            createWarnings(p, outputTaint, new List<FlagType>() { FlagType.HTMLDirty });
        }

        /// <summary>
        /// Visits eval point
        /// </summary>
        /// <param name="p">program point to visit</param>
        public override void VisitRCall(RCallPoint p)
        {
            _currentPoint = p;
            OutputSet.Snapshot.SetMode(SnapshotMode.InfoLevel);
            if (p is EvalExPoint)
            {
                EvalExPoint pEval = p as EvalExPoint;

                var varID = getVariableIdentifier(pEval.EvalCode.Value);
                List<Value> argumentValues = new List<Value>(pEval.EvalCode.Value.ReadMemory(Output).PossibleValues);
                List<ValueInfo> values = new List<ValueInfo>();
                values.Add(new ValueInfo(argumentValues, varID));
                bool nullValue = hasPossibleNullValue(pEval.EvalCode.Value);

                TaintInfo outputTaint = mergeTaint(values, nullValue);
                createWarnings(p, outputTaint, null, "Eval shoudn't contain anything from user input");
            }
           /* if (p is FunctionCallPoint)
            {
                FunctionCallPoint pCall = p as FunctionCallPoint;
               
                List<ValueInfo> values = new List<ValueInfo>();

                List<ValuePoint> args = new List<ValuePoint>(pCall.Arguments);
                bool nullValue = false;

                foreach (ValuePoint arg in args)
                {
                    var varID = getVariableIdentifier(arg.Value);
                    List<Value> argumentValues = new List<Value>(arg.Value.ReadMemory(Output).PossibleValues);
                    values.Add(new ValueInfo(argumentValues, varID));
                    nullValue |= hasPossibleNullValue(arg.Value);
                } 

                TaintInfo outputTaint = mergeTaint(values, nullValue);

                FunctionResolverBase.SetReturn(OutputSet, new MemoryEntry(Output.CreateInfo(outputTaint)));
            }*/
        }

        /// <summary>
        /// Visits including point
        /// </summary>
        /// <param name="p">program point to visit</param>
        public override void VisitInclude(IncludingExPoint p)
        {
            _currentPoint = p;

            var varID = getVariableIdentifier(p.Value);
            List<Value> argumentValues = new List<Value>(p.IncludePath.Value.ReadMemory(Output).PossibleValues);
            List<ValueInfo> values = new List<ValueInfo>();
            values.Add(new ValueInfo(argumentValues, varID));
            bool nullValue = hasPossibleNullValue(p.IncludePath.Value);

            TaintInfo outputTaint = mergeTaint(values, nullValue);
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
                var varID = getVariableIdentifier(p.Operand.Value);
                List<Value> argumentValues = new List<Value>(p.Operand.Value.ReadMemory(Output).PossibleValues);
                List<ValueInfo> values = new List<ValueInfo>();
                values.Add(new ValueInfo(argumentValues, varID));
                bool nullValue = hasPossibleNullValue(p.Operand.Value);

                TaintInfo outputTaint = mergeTaint(values, nullValue);
                createWarnings(p, outputTaint, new List<FlagType>() { FlagType.HTMLDirty });           
            }
        }

        /// <summary>
        /// Visits a concatenation expression point and propagates the taint from all operands.
        /// </summary>
        /// <param name="p">point to visit</param>
        public override void VisitConcat (ConcatExPoint p)
        {
            _currentPoint = p;
            List<ValueInfo> values = new List<ValueInfo>();
            bool nullValue = false;
            foreach (var operand in p.Parts) 
            {
                nullValue = addOperandValues(values, operand, nullValue);
            }

            TaintInfo outputTaint = mergeTaint(values, nullValue);
            p.SetValueContent(new MemoryEntry(Output.CreateInfo(outputTaint)));
        }

        private bool addOperandValues(List<ValueInfo> values, ValuePoint operand, bool nullValue) 
        {
            if (operand.Value != null) 
            {
                var operandID = getVariableIdentifier(operand.Value);
                List<Value> operandValues = new List<Value>(operand.Value.ReadMemory(Output).PossibleValues);
                values.Add(new ValueInfo(operandValues, operandID));
                nullValue |= hasPossibleNullValue(operand.Value);
            }
            return nullValue;

        }

        /// <summary>
        /// Visits a binary expression point and propagates the taint from both the operands.
        /// </summary>
        /// <param name="p">point to visit</param>
        public override void VisitBinary(BinaryExPoint p)
        {
            _currentPoint = p;
            List<ValueInfo> values = new List<ValueInfo>();
            bool nullValue = false;

            nullValue = addOperandValues(values, p.LeftOperand, nullValue);
            nullValue = addOperandValues(values, p.RightOperand, nullValue);

            TaintInfo outputTaint = mergeTaint(values, nullValue);

            outputTaint.setSanitized(new List<FlagType>() { FlagType.FilePathDirty, FlagType.HTMLDirty, FlagType.SQLDirty });

            p.SetValueContent(new MemoryEntry(Output.CreateInfo(outputTaint)));
        }

        /// <summary>
        /// Visits a ConditionalExPoint point and propagates the taint
        /// </summary>
        /// <param name="p">point to visit</param>
        public override void VisitValue(ValuePoint p)
        {
            _currentPoint = p;
            if (p is ConditionalExPoint)
            {
                
                var pEx = p as ConditionalExPoint;
                var possibleValues = pEx.Condition.Value.ReadMemory(Output).PossibleValues;

                var truevarID = getVariableIdentifier(pEx.TrueOperand.Value);
                List<Value> trueValues = new List<Value>(pEx.TrueOperand.Value.ReadMemory(Output).PossibleValues);
                    

                var falsevarID = getVariableIdentifier(pEx.FalseOperand.Value);
                List<Value> falseValues = new List<Value>(pEx.FalseOperand.Value.ReadMemory(Output).PossibleValues);
                    
                if (pEx.TrueAssume.Assumed && pEx.FalseAssume.Assumed)
                {
                    //merge taint info from both branches
                    List<ValueInfo> values = new List<ValueInfo>();  
                    values.Add(new ValueInfo(trueValues, truevarID));
                    values.Add(new ValueInfo(falseValues, falsevarID));
                    bool nullValue = hasPossibleNullValue(pEx.TrueOperand.Value) || hasPossibleNullValue(pEx.FalseOperand.Value);

                    TaintInfo outputTaint = mergeTaint(values, nullValue);
                    pEx.SetValueContent(new MemoryEntry(Output.CreateInfo(outputTaint)));
                }
                else if (pEx.TrueAssume.Assumed)
                {
                    //only true value is used
                    List<ValueInfo> values = new List<ValueInfo>();  
                    values.Add(new ValueInfo(trueValues, truevarID));
                    bool nullValue = hasPossibleNullValue(pEx.TrueOperand.Value);

                    TaintInfo outputTaint = mergeTaint(values, nullValue);
                    pEx.SetValueContent(new MemoryEntry(Output.CreateInfo(outputTaint)));
                }
                else
                {
                    //only false value is used
                    List<ValueInfo> values = new List<ValueInfo>();  
                    values.Add(new ValueInfo(falseValues, falsevarID));
                    bool nullValue = hasPossibleNullValue(pEx.FalseOperand.Value);

                    TaintInfo outputTaint = mergeTaint(values, nullValue);
                    pEx.SetValueContent(new MemoryEntry(Output.CreateInfo(outputTaint)));
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

            var sourceVal = p.ROperand.Value.ReadMemory(Output);

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
                    /*var returnValue = p.Expression.Value.ReadMemory(Input);
                    returnVar.WriteMemory(Output, returnValue);*/

                    var varID = getVariableIdentifier(p.Expression.Value);
                    List<Value> possibleValues = new List<Value>(p.Expression.Value.ReadMemory(Output).PossibleValues);
                    List<ValueInfo> values = new List<ValueInfo>();
                    values.Add(new ValueInfo(possibleValues, varID));

                    bool nullValue = hasPossibleNullValue(p.Expression.Value) || hasPossibleNullValue(p.Expression.Value);

                    TaintInfo outputTaint = mergeTaint(values, nullValue);
                    returnVar.WriteMemory(Output, new MemoryEntry(Output.CreateInfo(outputTaint)));


                    break;
                default:
                    throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Visits an extension point and sets the parameters
        /// </summary>
        /// <param name="p">point to visit</param>
        public override void VisitExtension(ExtensionPoint p)
        {
            _currentPoint = p;

            if (p.Graph.FunctionName == null)
            {
                return;
            }

            var declaration = p.Graph.SourceObject;
            var signature = getSignature(declaration);
            var callPoint = p.Caller as RCallPoint;
            if (callPoint != null)
            {
                if (signature.HasValue)
                {
                    // We have names for passed arguments
                    setNamedArguments(OutputSet, callPoint.CallSignature, signature.Value, p.Arguments);
                }
                else
                {
                    // There are no names - use numbered arguments
                    setOrderedArguments(OutputSet, p.Arguments, declaration);
                }
            }

        }


        /// <summary>
        /// If the function is a sanitizer, the sanitized taint flows are removed
        /// </summary>
        /// <param name="p">program point with a function</param>
        /// <param name="taintInfo">TaintInfo that is being sanitized</param>
        private void sanitize(NativeAnalyzerPoint p, ref TaintInfo taintInfo)
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
        private void warningsReportingFunct(NativeAnalyzerPoint p, TaintInfo taintInfo)
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

        private void createWarnings(ProgramPointBase p, TaintInfo taintInfo, List<FlagType> flags, String message = null)
        {
            if (taintInfo.nullValue)
            {
                String taint = taintInfo.printNullFlows();
                String nullMessage = message;
                if (message == "Eval shoudn't contain anything from user input")
                    nullMessage = "Eval shoudn't contain null";
                if (flags == null)
                {
                    createWarning(p, FlagType.HTMLDirty, nullMessage, taint, true, false);
                }
                else foreach (FlagType flag in flags)
                    {
                        createWarning(p, flag, nullMessage, taint, true, false);
                    }
            }
            if (flags == null)
            {
                if (!taintInfo.tainted) return;
                String taint = taintInfo.print();
                Boolean priority = !taintInfo.priority.allFalse();
                createWarning(p, FlagType.HTMLDirty, message, taint, false, priority);
            }
            else foreach (FlagType flag in flags)
                {
                    if (!(taintInfo.taint.get(flag))) continue;
                    String taint = taintInfo.print(flag);
                    createWarning(p, flag, message, taint, false, taintInfo.priority.get(flag));
                }
        }
              

        private void createWarning(ProgramPointBase p, FlagType flag, String message, String taint, Boolean nullFlow, Boolean priority)
        {
            var currentScript = p.OwningScriptFullName;
            AnalysisTaintWarning warning;
            if (message == null) warning = new AnalysisTaintWarning(currentScript, taint, priority,
                   p.Partial, p, flag, nullFlow);
            else warning = new AnalysisTaintWarning(currentScript, message, taint, priority,
                    p.Partial, p, flag);
            int index = analysisTaintWarnings.IndexOf(warning);
            if (index != -1)
            {
                analysisTaintWarnings.RemoveAt(index);
            }
            analysisTaintWarnings.Add(warning);
        }





        /// <summary>
        /// Merges multiple taint information into one.
        /// </summary>
        /// <param name="values">info values with taint information</param>
        /// <param name="nullValue">indicator of null flow</param>
        /// <returns>merged taint information</returns>
        private TaintInfo mergeTaint(List<ValueInfo> values, bool nullValue)
        {
            TaintInfo info = new TaintInfo();
            info.point = _currentPoint;
            TaintPriority priority = new TaintPriority(true);
            List<TaintInfo> processedTaintInfos = new List<TaintInfo>();
            //if _currentPoint is a ConcatExPoint, its priority is high whenever one of the values has high priority
            if (_currentPoint is ConcatExPoint) priority.setAll(false);
            Taint taint = new Taint(false);
            bool existsNullFlow = false;
            bool existsFlow = false;
            bool tainted = false;

            foreach (var pair in values)
            {
                existsFlow |= (pair.values.Count > 0);
                foreach (var infoValue in pair.values)
                {
                    if (infoValue is UndefinedValue)
                    {
                        continue;
                    }
                    if (!(infoValue is InfoValue<TaintInfo>)) continue;
                    TaintInfo varInfo = (((InfoValue<TaintInfo>)infoValue).Data);
                    if (processedTaintInfos.Contains(varInfo)) continue;
                    processedTaintInfos.Add(varInfo);
                    existsNullFlow |= varInfo.nullValue;
                    tainted |= varInfo.tainted;

                    /* If _currentPoint is not ConcatExPoint, the priority is low whenever one of the values
                    has a low priority. 
                    If _currentPoint is ConcatExPoint, the priority is high whenever one of the values has
                    a high priority */
                    if (!(_currentPoint is ConcatExPoint)) priority.copyTaint(false, varInfo.priority);
                    if (_currentPoint is ConcatExPoint) priority.copyTaint(true, varInfo.priority);

                    taint.copyTaint(true, varInfo.taint);

                    info.possibleTaintFlows.Add(new TaintFlow(varInfo, pair.variable));
                }
            }

            info.nullValue = existsNullFlow;
            info.tainted = tainted;

            if (!existsFlow) priority.setAll(false);

            if (nullValue && !existsNullFlow)
            {
                if (!existsFlow) priority.setAll(true);
                info.nullValue = true;
                info.tainted = true;
            }

            info.priority = priority;
            info.taint = taint;
            return info;
        }

        /// <summary>
        /// Gets the complete taint information
        /// </summary>
        /// <param name="lValue">entry to get the taint for</param>
        /// <returns>the taint information of given entry</returns>
        private TaintInfo getTaint(ReadSnapshotEntryBase lValue)
        {
            var varID = getVariableIdentifier(lValue);
            List<Value> info = new List<Value>(lValue.ReadMemory(Output).PossibleValues);

            List<ValueInfo> values = new List<ValueInfo>();
            values.Add(new ValueInfo(info, varID));

            return mergeTaint(values, hasPossibleNullValue(lValue));
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

        private static VariableName argumentString(int index)
        {
            if (index < 0)
            {
                throw new NotSupportedException("Cannot get argument variable for negative index");
            }

            return new VariableName(".arg" + index);
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

        /// <summary>
        /// Returns the VariableIdentifier of given snapshot entry or null if it does not exist
        /// </summary>
        /// <param name="b">variable identifier or null</param>
        /// <returns></returns>
        private VariableIdentifier getVariableIdentifier(ReadSnapshotEntryBase b)
        {
            try 
            {
                return b.GetVariableIdentifier(Output);
            }
            catch (System.Exception e)
            {
                return null;
            }

        }

        private Signature? getSignature(LangElement declaration)
        {
            var methodDeclaration = declaration as MethodDecl;
            if (methodDeclaration != null)
            {
                return methodDeclaration.Signature;
            }
            else
            {
                var functionDeclaration = declaration as FunctionDecl;
                if (functionDeclaration != null)
                {
                    return functionDeclaration.Signature;
                }
            }

            return null;
        }

        private void setNamedArguments(FlowOutputSet callInput, CallSignature? callSignature, Signature signature, IEnumerable<ValuePoint> arguments)
        {
            int i = 0;
            foreach (var arg in arguments)
            {
                if (i >= signature.FormalParams.Count)
                    break;
                var param = signature.FormalParams[i];
                var argumentVar = callInput.GetVariable(new VariableIdentifier(param.Name));

                var argumentValue = arg.Value.ReadMemory(Output);
                argumentVar.WriteMemory(callInput.Snapshot, argumentValue);

                ++i;
            }
            // TODO: if (arguments.Count() < signature.FormalParams.Count) and exists i > arguments.Count() signature.FormalParams[i].InitValue != null

        }

        private void setOrderedArguments(FlowOutputSet callInput, IEnumerable<ValuePoint> arguments, LangElement declaration)
        {
            var index = 0;
            foreach (var arg in arguments)
            {
                var argID = getVariableIdentifier(arg.Value);
                var argVar = argumentString(index);
                var argumentEntry = callInput.GetVariable(new VariableIdentifier(argVar));

                var argumentValue = arg.Value.ReadMemory(Output);
                List<Value> values = new List<Value>(argumentValue.PossibleValues);
                if (argID != null) values.Add(Output.CreateInfo(argID));

                MemoryEntry mem = new MemoryEntry(values);
                argumentEntry.WriteMemory(callInput.Snapshot, mem);

                ++index;
            }
        }

    }

    /// <summary>
    /// Class for storing pairs of VariableIdentifiers and corresponding values
    /// </summary>
    class ValueInfo
    {
        public List<Value> values;
        public VariableIdentifier variable;

        public ValueInfo(List<Value> values, VariableIdentifier var)
        {
            this.values = values;
            this.variable = var;
        }
    }
}

