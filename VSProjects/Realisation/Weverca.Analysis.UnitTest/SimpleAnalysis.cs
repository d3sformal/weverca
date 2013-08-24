using System;
using System.Collections.Generic;
using System.Linq;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Expressions;
using Weverca.Analysis.Memory;

namespace Weverca.Analysis.UnitTest
{
    /// <summary>
    /// Initializer used for setting environment because of testing purposes
    /// </summary>
    /// <param name="outSet">Initialized output set</param>
    delegate void EnvironmentInitializer(FlowOutputSet outSet);

    class SimpleAnalysis : ForwardAnalysisBase
    {
        private readonly EnvironmentInitializer _initializer;

        private readonly SimpleFlowResolver _flowResolver = new SimpleFlowResolver();

        public SimpleAnalysis(ControlFlowGraph.ControlFlowGraph entryCFG, EnvironmentInitializer initializer)
            : base(entryCFG)
        {
            _initializer = initializer;
        }

        #region Resolvers that are used during analysis

        protected override ExpressionEvaluatorBase createExpressionEvaluator()
        {
            return new SimpleExpressionEvaluator();
        }

        protected override FlowResolverBase createFlowResolver()
        {
            return _flowResolver;
        }

        protected override FunctionResolverBase createFunctionResolver()
        {
            return new SimpleFunctionResolver(_initializer);
        }

        protected override SnapshotBase createSnapshot()
        {
            return new VirtualReferenceModel.Snapshot();
        }
        #endregion

        #region Analysis settings routines

        internal void SetInclude(string fileName, string fileCode)
        {
            _flowResolver.SetInclude(fileName, fileCode);
        }

        #endregion
    }

    class SimpleInfo
    {
        internal readonly bool XssSanitized;

        internal SimpleInfo(bool xssSanitized)
        {
            XssSanitized = xssSanitized;
        }
    }

    /// <summary>
    /// Expression evaluation is resovled here
    /// </summary>
    class SimpleExpressionEvaluator : ExpressionEvaluatorBase
    {
        public override void Assign(VariableEntry target, MemoryEntry value)
        {
            if (target.IsDirect)
            {
                OutSet.Assign(target.DirectName, value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void FieldAssign(MemoryEntry objectValue, VariableEntry targetField, MemoryEntry value)
        {
            if (!targetField.IsDirect)
            {
                throw new NotImplementedException();
            }

            var index = OutSet.CreateIndex(targetField.DirectName.Value);
            foreach (ObjectValue obj in objectValue.PossibleValues)
            {
                OutSet.SetField(obj, index, value);
            }
        }

        public override MemoryEntry ResolveField(MemoryEntry objectValue, VariableEntry field)
        {
            if (!field.IsDirect || objectValue.PossibleValues.Count() != 1)
            {
                throw new NotImplementedException();
            }

            var obj = objectValue.PossibleValues.First() as ObjectValue;
            var index = OutSet.CreateIndex(field.DirectName.Value);
            return OutSet.GetField(obj, index);
        }

        public override MemoryEntry ResolveIndex(MemoryEntry array, MemoryEntry index)
        {
            if (index.PossibleValues.Count() != 1)
            {
                throw new NotImplementedException();
            }

            var values = new HashSet<Value>();
            var indexValue = index.PossibleValues.First() as PrimitiveValue;
            var containerIndex = OutSet.CreateIndex(indexValue.RawValue.ToString());

            foreach (var arrayValue in array.PossibleValues)
            {
                if (arrayValue is AssociativeArray)
                {

                    var possibleIndexValues = OutSet.GetIndex((AssociativeArray)arrayValue, containerIndex).PossibleValues;
                    values.UnionWith(possibleIndexValues);
                }
                else
                {
                    values.Add(OutSet.AnyValue);
                }
            }

            var result = new MemoryEntry(values.ToArray());
            keepParentInfo(array, result);

            return result;
        }

        public override MemoryEntry ResolveIndexedVariable(VariableEntry entry)
        {
            if (!entry.IsDirect)
            {
                throw new NotImplementedException();
            }

            var array = OutSet.ReadValue(entry.DirectName);
            //NOTE there should be precise resolution of multiple values

            var arrayValue = array.PossibleValues.First();
            if (arrayValue is UndefinedValue)
            {
                //new array is implicitly created
                arrayValue = OutSet.CreateArray();
                array = new MemoryEntry(arrayValue);
                OutSet.Assign(entry.DirectName, array);
            }

            return array;
        }

        public override void IndexAssign(MemoryEntry array, MemoryEntry index, MemoryEntry assignedValue)
        {
            if (array.PossibleValues.Count() != 1 && index.PossibleValues.Count() != 1)
            {
                throw new NotImplementedException();
            }

            var arrayValue = array.PossibleValues.First();
            var indexValue = index.PossibleValues.First() as PrimitiveValue;



            var containerIndex = OutSet.CreateIndex(indexValue.RawValue.ToString());

            OutSet.SetIndex(arrayValue as AssociativeArray, containerIndex, assignedValue);
        }

        public override void AliasAssign(VariableEntry target, IEnumerable<AliasValue> alias)
        {
            if (alias.Count() != 1)
            {
                throw new NotImplementedException();
            }

            if (target.IsDirect)
            {
                OutSet.Assign(target.DirectName, alias.First());
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override MemoryEntry ResolveVariable(VariableEntry variable)
        {
            var values = new HashSet<Value>();
            foreach (var varName in variable.PossibleNames)
            {
                values.UnionWith(OutSet.ReadValue(varName).PossibleValues);
            }

            return new MemoryEntry(values); ;
        }

        public override IEnumerable<string> VariableNames(MemoryEntry value)
        {
            //TODO convert all value types
            return from StringValue possible in value.PossibleValues select possible.Value;
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

        public override MemoryEntry UnaryEx(Operations operation, MemoryEntry operand)
        {
            var result = new HashSet<IntegerValue>();
            switch (operation)
            {
                case Operations.Minus:
                    var negations = from IntegerValue number in operand.PossibleValues select Flow.OutSet.CreateInt(-number.Value);
                    result.UnionWith(negations);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return new MemoryEntry(result.ToArray());
        }

        public override MemoryEntry ArrayEx(IEnumerable<KeyValuePair<MemoryEntry, MemoryEntry>> keyValuePairs)
        {
            throw new NotImplementedException();
        }

        #region Expression evaluation helpers

        private void keepParentInfo(MemoryEntry parent, MemoryEntry child)
        {
            AnalysisTestUtils.CopyInfo(OutSet, parent, child);
        }

        private MemoryEntry areEqual(MemoryEntry left, MemoryEntry right)
        {
            var result = new List<BooleanValue>();
            if (canBeDifferent(left, right))
            {
                result.Add(OutSet.CreateBool(false));
            }

            if (canBeSame(left, right))
            {
                result.Add(OutSet.CreateBool(true));
            }

            return new MemoryEntry(result.ToArray());
        }

        private bool canBeSame(MemoryEntry left, MemoryEntry right)
        {
            if (containsAnyValue(left) || containsAnyValue(right))
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
            return entry.PossibleValues.Any((val)=>val is AnyValue);
        }

        #endregion

        public override IEnumerable<AliasValue> ResolveAliasedField(MemoryEntry objectValue, VariableEntry aliasedField)
        {
            throw new NotImplementedException();
        }

        public override void AliasedFieldAssign(MemoryEntry objectValue, VariableEntry fieldEntry, IEnumerable<AliasValue> possibleAliasses)
        {
            throw new NotImplementedException();
        }

        public override void Foreach(MemoryEntry enumeree, VariableEntry keyVariable, VariableEntry valueVariable)
        {
            if (valueVariable.IsDirect)
            {
                var values=new HashSet<Value>();
                
                var array=enumeree.PossibleValues.First() as AssociativeArray;
                var indexes=OutSet.IterateArray(array);

                foreach(var index in indexes){
                    values.UnionWith(OutSet.GetIndex(array,index).PossibleValues);
                }

                OutSet.Assign(valueVariable.DirectName, new MemoryEntry(values));
            }
        }

        public override MemoryEntry Constant(GlobalConstUse x)
        {
            Value result;
            switch (x.Name.Name.Value)
            {
                case "true":
                    result = OutSet.CreateBool(true);
                    break;
                case "false":
                    result = OutSet.CreateBool(false);
                    break;
                default:
                    var constantName=".constant_"+x.Name;
                    var constantVar=new VariableName(constantName);
                    OutSet.FetchFromGlobal(constantVar);
                    return OutSet.ReadValue(constantVar);
            }

            return new MemoryEntry(result);
        }

        public override void ConstantDeclaration(ConstantDecl x, MemoryEntry constantValue)
        {
            var constName=new VariableName(".constant_"+x.Name);
            OutSet.FetchFromGlobal(constName);
            OutSet.Assign(constName, constantValue);
        }

        public override MemoryEntry Concat(MemoryEntry leftOperand, MemoryEntry rightOperand)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Resolving function names and function initializing
    /// </summary>
    class SimpleFunctionResolver : FunctionResolverBase
    {
        private readonly EnvironmentInitializer _environmentInitializer;

        /// <summary>
        /// Table of native analyzers
        /// </summary>
        private readonly Dictionary<string, NativeAnalyzerMethod> _nativeAnalyzers = new Dictionary<string, NativeAnalyzerMethod>()
        {
            {"strtolower",_strtolower},
            {"strtoupper",_strtoupper},
            {"concat",_concat},
            {"define",_define},
            {"abs",_abs},
            {"__constructor",_constructor},
        };

        internal SimpleFunctionResolver(EnvironmentInitializer initializer)
        {
            _environmentInitializer = initializer;
        }

        #region Call processing

        public override void MethodCall(MemoryEntry calledObject, QualifiedName name, MemoryEntry[] arguments)
        {
            var methods = resolveMethod(calledObject, name);
            setCallBranching(methods);
        }

        public override void Call(QualifiedName name, MemoryEntry[] arguments)
        {
            var functions = resolveFunction(name);
            setCallBranching(functions);
        }

        public override void IndirectMethodCall(MemoryEntry calledObject, MemoryEntry name, MemoryEntry[] arguments)
        {
            throw new NotImplementedException();
        }

        public override void IndirectCall(MemoryEntry name, MemoryEntry[] arguments)
        {
            var functionNames = getFunctionNames(name);
            var functions = new Dictionary<LangElement,FunctionValue>();

            foreach (var functionName in functionNames)
            {
                foreach(var fn in resolveFunction(functionName)){
                    functions[fn.Key] = fn.Value;
                }
            }

            setCallBranching(functions);
        }

        /// <summary>
        /// Initialize call into callInput. 
        /// 
        /// NOTE: 
        ///     arguments has to be initialized
        ///     sharing program point graphs is possible
        /// </summary>
        /// <returns></returns>
        public override void InitializeCall(FlowOutputSet callInput, LangElement declaration, MemoryEntry[] arguments)
        {
            _environmentInitializer(callInput);
            var method = declaration as MethodDecl;
            var hasNamedSignature = method != null;
            if (hasNamedSignature)
            {
                //we have names for passed arguments
                setNamedArguments(callInput, arguments, method.Signature);
            }
            else
            {
                //there are no names - use numbered arguments
                setOrderedArguments(callInput, arguments);
            }
        }

        /// <summary>
        /// Resolve return value from all possible calls
        /// </summary>
        /// <param name="calls">All calls on dispatch level, which return value is resolved</param>
        /// <returns>Resolved return value</returns>
        public override MemoryEntry ResolveReturnValue(ProgramPointGraph[] calls)
        {
            var possibleMemoryEntries = from call in calls select call.End.OutSet.ReadValue(call.End.OutSet.ReturnValue).PossibleValues;
            var flattenValues = possibleMemoryEntries.SelectMany((i) => i);

            return new MemoryEntry(flattenValues.ToArray());
        }

        #endregion

        #region Native analyzers

        /// <summary>
        /// Analyzer method for strtolower php function
        /// </summary>
        /// <param name="flow"></param>
        private static void _strtolower(FlowController flow)
        {
            var arg = flow.InSet.ReadValue(argument(0));

            var possibleValues = new List<Value>();

            foreach (var possible in arg.PossibleValues)
            {
                if (possible is StringValue)
                {
                    var lower = flow.OutSet.CreateString(((StringValue)possible).Value.ToLower());
                    possibleValues.Add(lower);
                }
                else
                {
                    possibleValues.Add(flow.OutSet.AnyValue);
                }
            }

            var output = new MemoryEntry(possibleValues.ToArray());

            keepArgumentInfo(flow, arg, output);
            flow.OutSet.Assign(flow.OutSet.ReturnValue, output);
        }

        /// <summary>
        /// Analyzer method for strtolower php function
        /// </summary>
        /// <param name="flow"></param>
        private static void _strtoupper(FlowController flow)
        {
            var arg = flow.InSet.ReadValue(argument(0));

            var possibleValues = new List<StringValue>();

            foreach (StringValue possible in arg.PossibleValues)
            {
                var lower = flow.OutSet.CreateString(possible.Value.ToUpper());
                possibleValues.Add(lower);
            }

            var output = new MemoryEntry(possibleValues.ToArray());

            keepArgumentInfo(flow, arg, output);
            flow.OutSet.Assign(flow.OutSet.ReturnValue, output);
        }

        private static void _concat(FlowController flow)
        {
            var arg0 = flow.InSet.ReadValue(argument(0));
            var arg1 = flow.InSet.ReadValue(argument(1));

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

        private static void _define(FlowController flow)
        {
            var arg0 = flow.InSet.ReadValue(argument(0));
            var arg1 = flow.InSet.ReadValue(argument(1));

            foreach (StringValue constName in arg0.PossibleValues)
            {
                var constVar=new VariableName(".constant_"+constName.Value);
                flow.OutSet.FetchFromGlobal(constVar);
                flow.OutSet.Assign(constVar, new MemoryEntry(arg1.PossibleValues));               
            }
        }

        private static void _abs(FlowController flow)
        {
            var arg0 = flow.InSet.ReadValue(argument(0));

            flow.OutSet.Assign(flow.OutSet.ReturnValue, new MemoryEntry(flow.OutSet.AnyFloatValue));
        }

        private static void _constructor(FlowController flow)
        {
            //  flow.OutSet.Assign(flow.OutSet.ReturnValue, flow.OutSet.ThisObject);
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Get storage for argument at given index
        /// NOTE:
        ///     Is used only for native analyzers in Simple analysis
        /// </summary>
        /// <param name="index">Index of argument at given storage</param>
        /// <returns>Storage for argument at given index</returns>
        private static VariableName argument(int index)
        {
            if (index < 0)
            {
                throw new NotSupportedException("Cannot get argument variable for negative index");
            }

            return new VariableName(".arg" + index);
        }

        private void setCallBranching(Dictionary<LangElement,FunctionValue> functions)
        {
            foreach (var branchKey in Flow.CallBranchingKeys)
            {
                if (!functions.Remove(branchKey))
                {
                    //this call is now not resolved as possible call branch
                    Flow.RemoveCallBranch(branchKey);
                }
            }

            foreach (var function in functions.Values)
            {
                //Create graph for every function - NOTE: we can share pp graphs
                var ppGraph = ProgramPointGraph.From(function);
                Flow.AddCallBranch(function.DeclaringElement, ppGraph);
            }
        }

        /// <summary>
        /// Resolving names according to given memory entry
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        private QualifiedName[] getFunctionNames(MemoryEntry functionName)
        {
            var result = new List<QualifiedName>();
            foreach (StringValue stringName in functionName.PossibleValues)
            {
                result.Add(new QualifiedName(new Name(stringName.Value)));
            }

            return result.ToArray();
        }

        private Dictionary<LangElement,FunctionValue> resolveFunction(QualifiedName name)
        {
            NativeAnalyzerMethod analyzer;
            var result = new Dictionary<LangElement,FunctionValue>();

            if (_nativeAnalyzers.TryGetValue(name.Name.Value, out analyzer))
            {
                //we have native analyzer - create it's program point graph
                var function = OutSet.CreateFunction(name.Name,new NativeAnalyzer(analyzer, Flow.CurrentPartial));
                result[function.DeclaringElement]=function;
            }
            else
            {
                var functions = OutSet.ResolveFunction(name);
                foreach (var function in functions)
                {
                    result[function.DeclaringElement] = function;
                }
            }

            return result;
        }

        private Dictionary<LangElement,FunctionValue> resolveMethod(MemoryEntry thisObject, QualifiedName methodName)
        {
            NativeAnalyzerMethod analyzer;
            var result = new Dictionary<LangElement,FunctionValue>();

            if (_nativeAnalyzers.TryGetValue(methodName.Name.Value, out analyzer))
            {
                //we have native analyzer - create it's program point graph
                var function = OutSet.CreateFunction(methodName.Name,new NativeAnalyzer(analyzer, Flow.CurrentPartial));
                result[function.DeclaringElement]=function;
            }
            else
            {
                var thisObj = thisObject.GetSingle<ObjectValue>();

                var functions = Flow.OutSet.ResolveMethod(thisObj, methodName);
                foreach (var function in functions)
                {
                    result[function.DeclaringElement] = function;
                }
            }

            return result;
        }

        private void setNamedArguments(FlowOutputSet callInput, MemoryEntry[] arguments, Signature signature)
        {
            for (int i = 0; i < signature.FormalParams.Count; ++i)
            {
                var param = signature.FormalParams[i];

                callInput.Assign(param.Name, arguments[i]);
            }
        }

        private void setOrderedArguments(FlowOutputSet callInput, MemoryEntry[] arguments)
        {
            var argCount = callInput.CreateInt(arguments.Length);
            callInput.Assign(new VariableName(".argument_count"), argCount);

            for (int i = 0; i < arguments.Length; ++i)
            {
                var argVar = argument(i);

                callInput.Assign(argVar, arguments[i]);
            }
        }

        private static void keepArgumentInfo(FlowController flow, MemoryEntry argument, MemoryEntry resultValue)
        {
            AnalysisTestUtils.CopyInfo(flow.OutSet, argument, resultValue);
        }

        #endregion
    }

    /// <summary>
    /// Controlling flow actions during analysis
    /// </summary>
    class SimpleFlowResolver : FlowResolverBase
    {
        private readonly Dictionary<string, string> _includes = new Dictionary<string, string>();

        private FlowOutputSet _outSet;
        private EvaluationLog _log;
        
        /// <summary>
        /// Represents method which is used for confirming assumption condition. Assumption can be declined - it means that we can prove, that condition CANNOT be ever satisfied.
        /// </summary>
        /// <returns>False if you can prove that condition cannot be ever satisfied, true otherwise.</returns>
        public override bool ConfirmAssumption(FlowOutputSet outSet, AssumptionCondition condition, EvaluationLog log)
        {
            _outSet = outSet;
            _log = log;

            bool willAssume;
            switch (condition.Form)
            {
                case ConditionForm.All:
                    willAssume = needsAll(condition.Parts);
                    break;

                default:
                    //we has to assume, because we can't disprove assumption
                    willAssume = true;
                    break;
            }

            if (willAssume)
            {
                processAssumption(condition);
            }

            return willAssume;
        }

        public override void CallDispatchMerge(FlowOutputSet callerOutput, ProgramPointGraph[] dispatchedProgramPointGraphs, DispatchType callType)
        {
            var ends = (from callOutput in dispatchedProgramPointGraphs select callOutput.End.OutSet as ISnapshotReadonly).ToArray();
            
            if (callType == DispatchType.ParallelInclude)
            {
                callerOutput.Extend(ends);
            }
            else
            {
                callerOutput.MergeWithCallLevel(ends);
            }
        }

        public override void Include(FlowController flow, MemoryEntry includeFile)
        {
            var files = new HashSet<string>();
            foreach (StringValue possibleFile in includeFile.PossibleValues)
            {
                files.Add(possibleFile.Value);
            }

            foreach (var branchKey in flow.IncludeBranchingKeys)
            {
                if (!files.Remove(branchKey))
                {
                    //this include is now not resolved as possible include branch
                    flow.RemoveIncludeBranch(branchKey);
                }
            }

            foreach (var file in files)
            {
                //Create graph for every include - NOTE: we can share pp graphs
                var cfg = AnalysisTestUtils.CreateCFG(_includes[file]);
                var ppGraph = new ProgramPointGraph(cfg.start);
                flow.AddIncludeBranch(file, ppGraph);
            }
        }

        internal void SetInclude(string fileName, string fileCode)
        {
            _includes.Add(fileName, fileCode);
        }

        #region Assumption helpers

        private bool needsAll(IEnumerable<Expressions.Postfix> conditionParts)
        {
            //we are searching for one part, that can be evaluated only as false

            foreach (var evaluatedPart in conditionParts)
            {
                var value=_log.GetValue(evaluatedPart.SourceElement);          
                if (evalOnlyFalse(value))
                {
                    return false;
                }
            }

            //can disprove some part
            return true;
        }

        private bool evalOnlyFalse(MemoryEntry evaluatedPart)
        {
            if (evaluatedPart == null)
            {
                //Possible cause of this is that evaluatedPart doesn't contains expression
                //Syntax error
                throw new NotSupportedException("Can assume only expression - PHP syntax error");
            }
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

                if (value is UndefinedValue)
                {
                    //undefined value is evaluated as false
                    continue;
                }

                //This part can be evaluated as true
                return false;
            }

            //some of possible values cant be evaluted as true
            return true;
        }

        /// <summary>
        /// Assume valid condition into output set
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="expressionParts"></param>
        private void processAssumption(AssumptionCondition condition)
        {
            if (condition.Form == ConditionForm.All)
            {
                if (condition.Parts.Count() == 1)
                {
                    assumeBinary(condition.Parts.First().SourceElement as BinaryEx);
                }
            }
        }

        private void assumeBinary(BinaryEx exp)
        {
            if (exp == null)
            {
                return;
            }
            switch (exp.PublicOperation)
            {
                case Operations.Equal:
                    assumeEqual(exp.LeftExpr, exp.RightExpr);
                    break;
            }
        }

        private void assumeEqual(LangElement left, LangElement right)
        {
            VariableEntry leftVar;
            MemoryEntry value;

            var call=left as DirectFcnCall;
            if (call != null && call.QualifiedName.Name.Value=="abs")
            {
                var absParam = call.CallSignature.Parameters[0];
                leftVar = _log.GetVariable(absParam.Expression);
                value = getReverse_abs(_log.GetValue(right));
            }
            else
            {
                leftVar = _log.GetVariable(left);
                value = _log.GetValue(right);
            }
          
            if (leftVar != null && value!=null)
            {
                foreach (var possibleVar in leftVar.PossibleNames)
                {
                    _outSet.Assign(possibleVar,value);
                }
             
            }
        }

        private MemoryEntry getReverse_abs(MemoryEntry entry)
        {
            var values= new HashSet<Value>();
            foreach (IntegerValue value in entry.PossibleValues)
            {
                values.Add(_outSet.CreateInt(value.Value));
                values.Add(_outSet.CreateInt(-value.Value));
            }

            return new MemoryEntry(values);
        }

        #endregion
    }
}
