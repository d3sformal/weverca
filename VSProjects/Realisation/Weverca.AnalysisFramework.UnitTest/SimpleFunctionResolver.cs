using System;
using System.Collections.Generic;
using System.Linq;
using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Reflection;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.AnalysisFramework.UnitTest
{
    /// <summary>
    /// Resolving function names and function initializing
    /// </summary>
    internal class SimpleFunctionResolver : FunctionResolverBase
    {
        private readonly EnvironmentInitializer _environmentInitializer;

        /// <summary>
        /// Table of native analyzers
        /// </summary>
        private readonly Dictionary<string, NativeAnalyzerMethod> _nativeAnalyzers = new Dictionary<string, NativeAnalyzerMethod>()
        {
            { "strtolower", _strtolower },
            { "strtoupper", _strtoupper },
            { "concat", _concat },
            { "define", _define },
            { "abs", _abs },
            { "write_argument", _write_argument }
        };

        private readonly Dictionary<string, ProgramPointGraph> _sharedPpGraphs = new Dictionary<string, ProgramPointGraph>();

        private readonly HashSet<string> _sharedFunctionNames = new HashSet<string>();

        internal SimpleFunctionResolver(EnvironmentInitializer initializer)
        {
            _environmentInitializer = initializer;
        }

        internal void SetFunctionShare(string functionName)
        {
            _sharedFunctionNames.Add(functionName);
        }

        #region Call processing

        public override void MethodCall(MemoryEntry calledObject, QualifiedName name, MemoryEntry[] arguments)
        {
            var methods = resolveMethod(calledObject, name);
            setCallBranching(methods);
        }

        public override MemoryEntry InitializeObject(MemoryEntry newObject, MemoryEntry[] arguments)
        {
            Flow.Arguments = arguments;
            Flow.CalledObject = newObject;

            var ctorName = new QualifiedName(new Name("__construct"));
            var ctors = resolveMethod(newObject, ctorName);

            if (ctors.Count > 0)
            {
                setCallBranching(ctors);
            }

            return newObject;
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
            var functions = new Dictionary<object, FunctionValue>();

            foreach (var functionName in functionNames)
            {
                foreach (var fn in resolveFunction(functionName))
                {
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
        /// <param name="callInput"></param>
        /// <param name="extensionGraph"></param>
        /// <param name="arguments"></param>
        public override void InitializeCall(FlowOutputSet callInput, ProgramPointGraph extensionGraph, MemoryEntry[] arguments)
        {
            _environmentInitializer(callInput);

            var declaration = extensionGraph.SourceObject;
            var signature = getSignature(declaration);
            var hasNamedSignature = signature.HasValue;

            if (hasNamedSignature)
            {
                //we have names for passed arguments
                setNamedArguments(callInput, arguments, signature.Value);
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
        public override MemoryEntry ResolveReturnValue(IEnumerable<ProgramPointGraph> calls)
        {
            var possibleMemoryEntries = from call in calls select call.End.OutSet.ReadValue(call.End.OutSet.ReturnValue).PossibleValues;
            var flattenValues = possibleMemoryEntries.SelectMany((i) => i);

            return new MemoryEntry(flattenValues.ToArray());
        }

        public override void DeclareGlobal(TypeDecl declaration)
        {

            var type = OutSet.CreateType(convertToType(declaration));
            OutSet.DeclareGlobal(type);
        }

        private ClassDecl convertToType(TypeDecl declaration)
        {
            ClassDeclBuilder result = new ClassDeclBuilder();
            result.BaseClassName = declaration.BaseClassName.HasValue ? new Nullable<QualifiedName>(declaration.BaseClassName.Value.QualifiedName) : null;

            result.IsFinal = declaration.Type.IsFinal;
            result.IsInterface = declaration.Type.IsInterface;
            result.IsAbstract = declaration.Type.IsAbstract;
            result.TypeName = new QualifiedName(declaration.Name);



            foreach (var member in declaration.Members)
            {
                if (member is FieldDeclList)
                {
                    foreach (FieldDecl field in (member as FieldDeclList).Fields)
                    {
                        Visibility visibility;
                        if (member.Modifiers.HasFlag(PhpMemberAttributes.Private))
                        {
                            visibility = Visibility.PRIVATE;
                        }
                        else if (member.Modifiers.HasFlag(PhpMemberAttributes.Protected))
                        {
                            visibility = Visibility.PROTECTED;
                        }
                        else
                        {
                            visibility = Visibility.PUBLIC;
                        }
                        bool isStatic = member.Modifiers.HasFlag(PhpMemberAttributes.Static);
                        //multiple declaration of fields
                        if (result.Fields.ContainsKey(new FieldIdentifier(result.TypeName, field.Name)))
                        {
                            //dont need to set warning in simpleAnalysis
                           // setWarning("Cannot redeclare field " + field.Name, member, AnalysisWarningCause.CLASS_MULTIPLE_FIELD_DECLARATION);
                        }
                        else
                        {
                            result.Fields.Add(new FieldIdentifier(result.TypeName, field.Name), new FieldInfo(field.Name, result.TypeName, "any", visibility, field.Initializer, isStatic));
                        }
                    }

                }
                else if (member is ConstDeclList)
                {
                    foreach (var constant in (member as ConstDeclList).Constants)
                    {
                        if (result.Constants.ContainsKey(new FieldIdentifier(result.TypeName, constant.Name)))
                        {
                            //dont need to set warning in simpleAnalysis
                            // setWarning("Cannot redeclare constant " + constant.Name, member, AnalysisWarningCause.CLASS_MULTIPLE_CONST_DECLARATION);
                        }
                        else
                        {
                            //in php all object constatns are public
                            Visibility visbility = Visibility.PUBLIC;
                            result.Constants.Add(new FieldIdentifier(result.TypeName, constant.Name), new ConstantInfo(constant.Name, result.TypeName, visbility, constant.Initializer));
                        }
                    }
                }
                else if (member is MethodDecl)
                {
                    var methosIdentifier = new MethodIdentifier(result.TypeName, (member as MethodDecl).Name);
                    if (!result.SourceCodeMethods.ContainsKey(methosIdentifier))
                    {
                        result.SourceCodeMethods.Add(methosIdentifier, member as MethodDecl);
                    }
                    else
                    {
                        //dont need to set warning in simpleAnalysis
                        // setWarning("Cannot redeclare constant " + (member as MethodDecl).Name, member, AnalysisWarningCause.CLASS_MULTIPLE_CONST_DECLARATION);
                    }
                }
                else
                {
                    //ignore traits are not supported by AST, only by parser
                }
            }


            // NativeTypeDecl result=new NativeTypeDecl();

            return result.Build();
        }

        #endregion

        #region Native analyzers

        /// <summary>
        /// Analyzer method for <c>strtolower</c> php function
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
        /// Analyzer method for <c>strtoupper</c> php function
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
                var constVar = new VariableName(".constant_" + constName.Value);
                flow.OutSet.FetchFromGlobal(constVar);
                flow.OutSet.Assign(constVar, new MemoryEntry(arg1.PossibleValues));
            }
        }

        private static void _abs(FlowController flow)
        {
            var arg0 = flow.InSet.ReadValue(argument(0));

            flow.OutSet.Assign(flow.OutSet.ReturnValue, new MemoryEntry(flow.OutSet.AnyFloatValue));
        }

        private static void _write_argument(FlowController flow)
        {
            var arg0 = flow.InSet.ReadValue(argument(0)).PossibleValues.First() as StringValue;

            var value = new MemoryEntry(flow.OutSet.CreateString(arg0.Value + "_WrittenInArgument"));
            flow.OutSet.Assign(argument(0), value);
        }

        private static void _constructor(FlowController flow)
        {
            // flow.OutSet.Assign(flow.OutSet.ReturnValue, flow.OutSet.ThisObject);
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

        private Signature? getSignature(LangElement declaration)
        {
            //TODO resolving via visitor might be better
            if (declaration is MethodDecl)
            {
                return (declaration as MethodDecl).Signature;
            }
            else if (declaration is FunctionDecl)
            {
                return (declaration as FunctionDecl).Signature;
            }

            return null;
        }

        private void setCallBranching(Dictionary<object, FunctionValue> functions)
        {
            foreach (var branchKey in Flow.ExtensionKeys)
            {
                if (!functions.Remove(branchKey))
                {
                    // Now this call is not resolved as possible call branch
                    Flow.RemoveExtension(branchKey);
                }
            }

            foreach (var function in functions.Values)
            {
                // Create graph for every function - NOTE: We can share pp graphs
                addCallBranch(function);
            }
        }

        private void addCallBranch(FunctionValue function)
        {
            var functionName = function.Name.Value;
            ProgramPointGraph functionGraph;
            if (_sharedFunctionNames.Contains(functionName))
            {
                //set graph sharing for this function
                if (!_sharedPpGraphs.ContainsKey(functionName))
                {
                    //create single graph instance
                    _sharedPpGraphs[functionName] = ProgramPointGraph.From(function);
                }

                //get shared instance of program point graph
                functionGraph = _sharedPpGraphs[functionName];
            }
            else
            {
                functionGraph = ProgramPointGraph.From(function);
            }

            Flow.AddExtension(function.DeclaringElement, functionGraph);
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

        private Dictionary<object, FunctionValue> resolveFunction(QualifiedName name)
        {
            NativeAnalyzerMethod analyzer;
            var result = new Dictionary<object, FunctionValue>();

            if (_nativeAnalyzers.TryGetValue(name.Name.Value, out analyzer))
            {
                //we have native analyzer - create it's program point graph
                var function = OutSet.CreateFunction(name.Name, new NativeAnalyzer(analyzer, Flow.CurrentPartial));
                result[function.DeclaringElement] = function;
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

        private Dictionary<object, FunctionValue> resolveMethod(MemoryEntry thisObject, QualifiedName methodName)
        {
            NativeAnalyzerMethod analyzer;
            var result = new Dictionary<object, FunctionValue>();

            if (_nativeAnalyzers.TryGetValue(methodName.Name.Value, out analyzer))
            {
                //we have native analyzer - create it's program point graph
                var function = OutSet.CreateFunction(methodName.Name, new NativeAnalyzer(analyzer, Flow.CurrentPartial));
                result[function.DeclaringElement] = function;
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
            var callPoint = Flow.ProgramPoint as RCallPoint;
            var callSignature = callPoint.CallSignature;
            var enumerator = callPoint.Arguments.GetEnumerator();
            for (int i = 0; i < signature.FormalParams.Count; ++i)
            {
                enumerator.MoveNext();

                var param = signature.FormalParams[i];
                var callParam = callSignature.Value.Parameters[i];

                var argumentVar=callInput.GetVariable(new VariableIdentifier(param.Name));

                if (callParam.PublicAmpersand)
                {
                    argumentVar.SetAliases(callInput.Snapshot, enumerator.Current.Value);
                }
                else
                {
                    argumentVar.WriteMemory(callInput.Snapshot, arguments[i]);
                }
            }
        }

        private void setOrderedArguments(FlowOutputSet callInput, MemoryEntry[] arguments)
        {
            var argCount = new MemoryEntry(callInput.CreateInt(arguments.Length));
            var argCountEntry=callInput.GetVariable(new VariableIdentifier(".argument_count"));
            argCountEntry.WriteMemory(callInput.Snapshot, argCount);

            var index = 0;
            var callPoint = Flow.ProgramPoint as RCallPoint;
            foreach (var arg in callPoint.Arguments)
            {
                var argVar = argument(index);
                var argumentEntry = callInput.GetVariable(new VariableIdentifier(argVar));

                //determine that argument value is based on variable, so we can get it's alias
                var aliasProvider = arg as LValuePoint;
                if (aliasProvider == null)
                {
                    //assign value for parameter
                    argumentEntry.WriteMemory(callInput.Snapshot, arguments[index]);
                }
                else
                {
                    //join parameter with alias (for testing we join all possible arguments)
                    //be carefull here - Flow.OutSet belongs to call context already - so we has to read variable from InSet
                    argumentEntry.SetAliases(callInput.Snapshot, aliasProvider.LValue);
                }
                ++index;
            }

        }

        private static void keepArgumentInfo(FlowController flow, MemoryEntry argument, MemoryEntry resultValue)
        {
            AnalysisTestUtils.CopyInfo(flow.OutSet, argument, resultValue);
        }

        #endregion
    }
}
