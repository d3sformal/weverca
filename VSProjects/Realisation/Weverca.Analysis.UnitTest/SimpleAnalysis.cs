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
    /// <summary>
    /// Initializer used for setting environment because of testing purposes
    /// </summary>
    /// <param name="outSet">Initialized output set</param>
    delegate void EnvironmentInitializer(FlowOutputSet outSet);

    class SimpleAnalysis : ForwardAnalysis
    {

        private readonly EnvironmentInitializer _initializer;

        public SimpleAnalysis(ControlFlowGraph.ControlFlowGraph entryCFG,EnvironmentInitializer initializer)
            : base(entryCFG)
        {
            _initializer = initializer;
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
            return new SimpleFunctionResolver(_initializer);
        }

        protected override AbstractSnapshot createSnapshot()
        {
            return new VirtualReferenceModel.Snapshot();
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
    class SimpleExpressionEvaluator : ExpressionEvaluator
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

        public override void Assign(MemoryEntry objectValue, VariableEntry targetField, MemoryEntry value)
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

        public override MemoryEntry ArrayRead(MemoryEntry array, MemoryEntry index)
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



        public override MemoryEntry ResolveArray(VariableEntry entry)
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

        public override void ArrayAssign(MemoryEntry array, MemoryEntry index, MemoryEntry assignedValue)
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
            if (!variable.IsDirect)
            {
                throw new NotImplementedException();
            }

            return OutSet.ReadValue(variable.DirectName);
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
            return entry.PossibleValues.Contains(OutSet.AnyValue);
        }

        #endregion

        public override IEnumerable<AliasValue> ResolveAlias(MemoryEntry objectValue, VariableEntry aliasedField)
        {
            throw new NotImplementedException();
        }

        public override void AliasAssign(MemoryEntry objectValue, VariableEntry fieldEntry, IEnumerable<AliasValue> possibleAliasses)
        {
            throw new NotImplementedException();
        }

    }



    /// <summary>
    /// Resolving function names and function initializing
    /// </summary>
    class SimpleFunctionResolver : FunctionResolver
    {
        private readonly EnvironmentInitializer _environmentInitializer;

        /// <summary>
        /// Table of native analyzers
        /// </summary>
        private readonly Dictionary<string, NativeAnalyzer> _nativeAnalyzers = new Dictionary<string, NativeAnalyzer>()
        {
            {"strtolower",new NativeAnalyzer(_strtolower)},
            {"strtoupper",new NativeAnalyzer(_strtoupper)},
            {"concat",new NativeAnalyzer(_concat)},
            {"__constructor",new NativeAnalyzer(_constructor)},
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
            var functions = new HashSet<LangElement>();

            foreach (var functionName in functionNames)
            {
                functions.UnionWith(resolveFunction(functionName));
            }

            setCallBranching(functions);
        }


        /// <summary>
        /// Initialize call into callInput. 
        /// 
        /// NOTE: 
        ///     arguments are already initialized
        ///     sharing program point graphs is possible
        /// </summary>
        /// <returns></returns>
        public override void InitializeCall(FlowOutputSet callInput, LangElement declaration)
        {
            _environmentInitializer(callInput);
            var method = declaration as MethodDecl;
            if (method != null)
            {
                joinArgumentAliases(callInput, method.Signature);
            }
        }

        #endregion

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

        #region Native analyzers

        

        /// <summary>
        /// Analyzer method for strtolower php function
        /// </summary>
        /// <param name="flow"></param>
        private static void _strtolower(FlowController flow)
        {
            var arg = flow.InSet.ReadValue(flow.InSet.Argument(0));
            
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
            var arg = flow.InSet.ReadValue(flow.InSet.Argument(0));
            


            var possibleValues = new List<StringValue>();

            foreach (StringValue possible in arg.PossibleValues)
            {
                var lower = flow.OutSet.CreateString(possible.Value.ToUpper());
                possibleValues.Add(lower);
            }

            var output=new MemoryEntry(possibleValues.ToArray());

            keepArgumentInfo(flow, arg, output);
            flow.OutSet.Assign(flow.OutSet.ReturnValue, output);
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

        private static void _constructor(FlowController flow)
        {
            //  flow.OutSet.Assign(flow.OutSet.ReturnValue, flow.OutSet.ThisObject);
        }
        #endregion

        #region Private helpers

        private void setCallBranching(HashSet<LangElement> functions)
        {
            foreach (var branchKey in Flow.CallBranchingKeys)
            {
                if (!functions.Remove(branchKey))
                {
                    //this call is now not resolved as possible call branch
                    Flow.RemoveCallBranch(branchKey);
                }
            }

            foreach (var function in functions)
            {
                //Create graph for every function - NOTE: we can share pp graphs
                var ppGraph = ProgramPointGraph.From(function);
                Flow.AddCallBranch(function, ppGraph);
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

        private HashSet<LangElement> resolveFunction(QualifiedName name)
        {
            NativeAnalyzer analyzer;
            var result = new HashSet<LangElement>();

            if (_nativeAnalyzers.TryGetValue(name.Name.Value, out analyzer))
            {
                //we have native analyzer - create it's program point 
                result.Add(analyzer);
            }
            else
            {
                var functions = OutSet.ResolveFunction(name);

                var declarations = from FunctionValue function in functions select function.Declaration;
                result.UnionWith(declarations);
            }

            return result;
        }

        private HashSet<LangElement> resolveMethod(MemoryEntry thisObject, QualifiedName methodName)
        {
            NativeAnalyzer analyzer;

            var result = new HashSet<LangElement>();

            if (_nativeAnalyzers.TryGetValue(methodName.Name.Value, out analyzer))
            {
                //we have native analyzer - create it's program point 
                result.Add(analyzer);
            }
            else
            {
                var thisObj = thisObject.GetSingle<ObjectValue>();

                var functions = Flow.OutSet.ResolveMethod(thisObj, methodName);
                result.UnionWith(functions);
            }

            return result;
        }

        private void joinArgumentAliases(FlowOutputSet callInput, Signature signature)
        {
            for (int i = 0; i < signature.FormalParams.Count; ++i)
            {
                var param = signature.FormalParams[i];
                var arg = callInput.Argument(i);

                var argAlias = callInput.CreateAlias(arg);
                callInput.Assign(param.Name, argAlias);
            }
        }

        private static void keepArgumentInfo(FlowController flow, MemoryEntry argument, MemoryEntry resultValue )
        {
            AnalysisTestUtils.CopyInfo(flow.OutSet, argument,resultValue);
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
            var ends = (from callOutput in dispatchedProgramPointGraphs select callOutput.End.OutSet as ISnapshotReadonly).ToArray();
            callerOutput.MergeWithCallLevel(ends);
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

            //can disprove some part
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


                if (value is UndefinedValue)
                {
                    //undefined value is evaluated as false
                    continue;
                }

                //This part can be evaluated as true
                return false;
            }

            //any of possible values cant be evaluted as true
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
