using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PHP.Core;
using PHP.Core.Parsers;
using PHP.Core.AST;
using PHP.Core.Reflection;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.ProgramPoints;
using System.IO;
using Weverca.Parsers;
using Weverca.Analysis.ExpressionEvaluator;

namespace Weverca.Analysis
{
    /// <summary>
    /// Resolving function names and function initializing
    /// </summary>
    public class FunctionResolver : FunctionResolverBase
    {
        private static readonly VariableName currentFunctionName = new VariableName(".currentFunction");
        private NativeFunctionAnalyzer nativeFunctionAnalyzer;
        private Dictionary<MethodDecl, FunctionHints> methods = new Dictionary<MethodDecl, FunctionHints>();
        private Dictionary<FunctionDecl, FunctionHints> functions
            = new Dictionary<FunctionDecl, FunctionHints>();

        private readonly Dictionary<FunctionValue, ProgramPointGraph> sharedProgramPoints = new Dictionary<FunctionValue, ProgramPointGraph>();

        private readonly HashSet<FunctionValue> sharedFunctions = new HashSet<FunctionValue>();

        private static readonly VariableName calledFunctionsName = new VariableName(".calledFunctions");

        /// <summary>
        ///  Readonly variable name for storing depth of eval recursion
        /// </summary>
        public static readonly VariableName evalDepth = new VariableName(".evalDepth");

        /// <summary>
        /// Readonly variable name for storing depth of recursion
        /// </summary>
        public static readonly VariableName callDepthName = new VariableName(".callDepth");

        /// <summary>
        /// Readonly variable name for storing current script name
        /// </summary>
        public static readonly VariableName currentScript = new VariableName(".currentScript");
        
        /// <summary>
        /// Readonly variable name for storing type of called object
        /// </summary>
        public static readonly VariableName calledObjectTypeName = new VariableName(".calledObject");

        /// <summary>
        /// Readonly variable name for static variables
        /// </summary>
        public static readonly VariableName staticVariables = new VariableName(".staticVariables");

        /// <summary>
        /// Readonly variable name for sink of static variables
        /// </summary>
        public static readonly VariableName staticVariableSink = new VariableName(".staticVariableSink");

        /// <summary>
        /// Structure, that stores for every method, its coresponding class name
        /// </summary>
        public static Dictionary<LangElement, QualifiedName> methodToClass = new Dictionary<LangElement, QualifiedName>();
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionResolver" /> class.
        /// </summary>
        public FunctionResolver()
        {
            nativeFunctionAnalyzer = NativeFunctionAnalyzer.CreateInstance();
        }

        #region FunctionResolverBase overrides

        /// <inheritdoc />
        public override void DeclareGlobal(FunctionDecl declaration)
        {
            if (nativeFunctionAnalyzer.existNativeFunction(new QualifiedName(declaration.Name)))
            {
                setWarning("Fuction allready exists", AnalysisWarningCause.FUNCTION_ALLREADY_EXISTS);
            }
            else if (OutSet.ResolveFunction(new QualifiedName(declaration.Name)).Count() > 0)
            {
                setWarning("Fuction allready exists", AnalysisWarningCause.FUNCTION_ALLREADY_EXISTS);
            }
            else
            {
                OutSet.DeclareGlobal(declaration, Flow.CurrentScript);
            }
        }


        #region function calls

        /// <inheritdoc />
        public override void Call(QualifiedName name, MemoryEntry[] arguments)
        {
            var functions = resolveFunction(name, arguments);
            setCallBranching(functions);

        }

        /// <inheritdoc />
        public override void IndirectCall(MemoryEntry name, MemoryEntry[] arguments)
        {
            var functions = new Dictionary<object, FunctionValue>();
            var functionNames = getSubroutineNames(name);

            foreach (var functionName in functionNames)
            {
                var resolvedFunctions = resolveFunction(functionName, arguments);
                foreach (var resolvedFunction in resolvedFunctions)
                {
                    functions[resolvedFunction.Key] = resolvedFunction.Value;
                }
            }

            setCallBranching(functions);
        }

        #endregion

        #region method calls


        /// <inheritdoc />
        public override void MethodCall(ReadSnapshotEntryBase calledObject, QualifiedName name,
            MemoryEntry[] arguments)
        {
            var methods = resolveMethod(calledObject, name, arguments);
            setCallBranching(methods);
        }


        /// <inheritdoc />
        public override void IndirectMethodCall(ReadSnapshotEntryBase calledObject, MemoryEntry name,
            MemoryEntry[] arguments)
        {
            var methods = new Dictionary<object, FunctionValue>();
            var methodNames = getSubroutineNames(name);

            foreach (var methodName in methodNames)
            {
                var resolvedMethods = resolveMethod(calledObject, methodName, arguments);
                foreach (var resolvedMethod in resolvedMethods)
                {
                    methods[resolvedMethod.Key] = resolvedMethod.Value;
                }
            }

            setCallBranching(methods);
        }


        #endregion

        #region static calls

        /// <inheritdoc />
        public override void StaticMethodCall(ReadSnapshotEntryBase calledObject, QualifiedName name, MemoryEntry[] arguments)
        {
            var calledObjectValue = calledObject.ReadMemory(InSnapshot);
            HashSet<QualifiedName> calledClasses = new HashSet<QualifiedName>();
            foreach (var value in calledObjectValue.PossibleValues)
            {
                var visitor = new StaticObjectVisitor(Flow);
                value.Accept(visitor);
                switch (visitor.Result)
                {
                    case StaticObjectVisitorResult.NO_RESULT:
                        setWarning("Cannot call method on non object ", AnalysisWarningCause.METHOD_CALL_ON_NON_OBJECT_VARIABLE);
                        break;
                    case StaticObjectVisitorResult.ONE_RESULT:
                        calledClasses.Add(visitor.className);
                        break;
                    case StaticObjectVisitorResult.MULTIPLE_RESULTS:
                        break;
                }
            }
            staticMethodCall(calledClasses, name, arguments);
        }

        /// <inheritdoc />
        public override void StaticMethodCall(QualifiedName typeName, QualifiedName name, MemoryEntry[] arguments)
        {
            var resolvedTypes = ResolveType(typeName, Flow,OutSet, Element);
            staticMethodCall(resolvedTypes, name, arguments);
        }

        private void staticMethodCall(IEnumerable<QualifiedName> typeName, QualifiedName name, MemoryEntry[] arguments)
        {
            List<TypeValue> types = new List<TypeValue>();
            foreach (var type in typeName)
            {
                types.AddRange(ExpressionEvaluator.ExpressionEvaluator.ResolveSourceOrNativeType(type, Flow, Element));
            }
            Dictionary<object, FunctionValue> calledMethods = new Dictionary<object, FunctionValue>();
            foreach (var type in types)
            {
                var methods = resolveStaticMethod(type, name, arguments);
                foreach (var entry in methods)
                {
                    calledMethods[entry.Key] = entry.Value;
                }
            }
            setCallBranching(calledMethods);
        }

        /// <inheritdoc />
        public override void IndirectStaticMethodCall(ReadSnapshotEntryBase calledObject, MemoryEntry name, MemoryEntry[] arguments)
        {
            var calledObjectValue = calledObject.ReadMemory(InSnapshot);
            HashSet<QualifiedName> typeNames = new HashSet<QualifiedName>();
            foreach (var value in calledObjectValue.PossibleValues)
            {
                var visitor = new StaticObjectVisitor(Flow);
                value.Accept(visitor);
                switch (visitor.Result)
                {
                    case StaticObjectVisitorResult.NO_RESULT:
                        setWarning("Cannot call method on non object ", AnalysisWarningCause.METHOD_CALL_ON_NON_OBJECT_VARIABLE);
                        break;
                    case StaticObjectVisitorResult.ONE_RESULT:
                        typeNames.Add(visitor.className);
                        break;
                    case StaticObjectVisitorResult.MULTIPLE_RESULTS:

                        break;
                }
            }
            indirectStaticMethodCall(typeNames, name, arguments);
        }

        /// <inheritdoc />
        public override void IndirectStaticMethodCall(QualifiedName typeName, MemoryEntry name, MemoryEntry[] arguments)
        {
            HashSet<QualifiedName> typeNames = new HashSet<QualifiedName>();
            foreach (var resolvedType in ResolveType(typeName, Flow, OutSet, Element))
            {
                typeNames.Add(resolvedType);
            }
            indirectStaticMethodCall(typeNames, name, arguments);
        }

        private void indirectStaticMethodCall(IEnumerable<QualifiedName> typeName, MemoryEntry name, MemoryEntry[] arguments)
        {
            var functions = new Dictionary<object, FunctionValue>();
            var functionNames = getSubroutineNames(name);
            List<TypeValue> types = new List<TypeValue>();
            foreach (var type in typeName)
            {
                types.AddRange(ExpressionEvaluator.ExpressionEvaluator.ResolveSourceOrNativeType(type, Flow, Element));
            }
            foreach (var type in types)
            {
                foreach (var functionName in functionNames)
                {
                    var resolvedFunctions = resolveStaticMethod(type, functionName, arguments);
                    foreach (var resolvedFunction in resolvedFunctions)
                    {
                        functions[resolvedFunction.Key] = resolvedFunction.Value;
                    }
                }
            }

            setCallBranching(functions);
        }

        /// <summary>
        /// This method resolves keyword self and parent and return list of possible QualifiedName
        /// </summary>
        /// <param name="typeName">type name to resolve</param>
        /// <param name="flow">FlowController</param>
        /// <param name="OutSet">FlowOutputSet</param>
        /// <param name="element">element, where the call apears</param>
        /// <returns>resolved QualifiedNames</returns>
        public static IEnumerable<QualifiedName> ResolveType(QualifiedName typeName,FlowController flow, FlowOutputSet OutSet, LangElement element)
        {
            List<QualifiedName> result = new List<QualifiedName>();
            if (typeName.Name.Value == "self" || typeName.Name.Value == "parent")
            {
                if (OutSet.ReadLocalControlVariable(calledObjectTypeName).IsDefined(OutSet.Snapshot))
                {
                    MemoryEntry calledObjects =
                    OutSet.GetLocalControlVariable(calledObjectTypeName).ReadMemory(OutSet.Snapshot);

                    if (typeName.Name.Value == "self")
                    {
                        foreach (var calledObject in calledObjects.PossibleValues)
                        {
                            result.Add((calledObject as TypeValue).QualifiedName);
                        }
                    }
                    if (typeName.Name.Value == "parent")
                    {
                        foreach (var calledObject in calledObjects.PossibleValues)
                        {
                            ClassDecl classDeclaration = (calledObject as TypeValue).Declaration;
                            if (classDeclaration.BaseClasses.Count > 0)
                            {
                                result.Add((calledObject as TypeValue).Declaration.BaseClasses.Last());
                            }
                            else
                            {
                                AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(flow.CurrentScript.FullName,"Cannot acces parrent:: current class has no parrent", element, AnalysisWarningCause.CANNOT_ACCCES_PARENT_CURRENT_CLASS_HAS_NO_PARENT));
                            }
                        }
                    }
                }
                else
                {
                    if (typeName.Name.Value == "self")
                    {
                        AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(flow.CurrentScript.FullName, "Cannot acces self:: when not in class", element, AnalysisWarningCause.CANNOT_ACCCES_SELF_WHEN_NOT_IN_CLASS));
                    }
                    else
                    {
                        AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(flow.CurrentScript.FullName, "Cannot acces parent:: when not in class", element, AnalysisWarningCause.CANNOT_ACCCES_PARENT_WHEN_NOT_IN_CLASS));
                    }

                }
            }
            else
            {
                result.Add(typeName);
            }
            return result;

        }

        #endregion

        /// <inheritdoc />
        public override MemoryEntry InitializeCalledObject(ProgramPointBase caller, ProgramPointGraph extensionGraph, MemoryEntry calledObject)
        {

            if (caller is StaticMethodCallPoint || caller is IndirectStaticMethodCallPoint)
            {
                StaticMtdCall element = caller.Partial as StaticMtdCall;
                if (element != null)
                {
                    if (element.ClassName.QualifiedName.Name.Value == "self" || element.ClassName.QualifiedName.Name.Value == "parent")
                    {
                        return caller.OutSet.ReadVariable(new VariableIdentifier("this")).ReadMemory(caller.OutSnapshot);
                    }
                }

                return new MemoryEntry(OutSet.UndefinedValue);
            }
            else
            {
                var declaration = extensionGraph.SourceObject;
                if (declaration is MethodDecl)
                {
                    //normal call of static method  $this cannot be accesible
                    if ((declaration as MethodDecl).Modifiers.HasFlag(PhpMemberAttributes.Static))
                    {
                        return new MemoryEntry(OutSet.UndefinedValue);
                    }
                }
            }

            return calledObject;
        }

        /// <summary>
        /// Initialize call into callInput.
        /// NOTE:
        ///     arguments has to be initialized
        ///     sharing program point graphs is possible
        /// </summary>
        /// <param name="caller">Information abour called program point</param>
        /// <param name="extensionGraph">Graph representing initialized call</param>
        /// <param name="arguments">Call arguments</param>
        public override void InitializeCall(ProgramPointBase caller, ProgramPointGraph extensionGraph,
            MemoryEntry[] arguments)
        {
            //include            
            if (extensionGraph.FunctionName == null)
            {
                if (caller is IncludingExPoint)
                {
                    string thisFile = Flow.CurrentScript.FullName;
                    var includedFiles = OutSet.GetControlVariable(new VariableName(".includedFiles"));
                    IEnumerable<Value> files = includedFiles.ReadMemory(OutSnapshot).PossibleValues;
                    List<Value> result = IncreaseCalledInfo(thisFile, files);
                    includedFiles.WriteMemory(OutSnapshot, new MemoryEntry(result));
                }
                else
                { 
                    //eval
                    increaseEvalDepth(OutSet);
                }
            }
            else
            {
                //function
                IncreaseStackSize(caller.OutSet);

                var declaration = extensionGraph.SourceObject;
                var signature = getSignature(declaration);
                var hasNamedSignature = signature.HasValue;

                if (declaration != null && methodToClass.ContainsKey(declaration))
                {
                    QualifiedName calledClass = methodToClass[declaration];
                    MemoryEntry types = new MemoryEntry(OutSet.ResolveType(calledClass));
                    OutSet.GetLocalControlVariable(calledObjectTypeName).WriteMemory(OutSet.Snapshot, types);
                }

                if (hasNamedSignature)
                {
                    // We have names for passed arguments
                    setNamedArguments(OutSet, arguments, signature.Value);
                }
                else
                {
                    // There are no names - use numbered arguments
                    setOrderedArguments(OutSet, arguments);
                }

                var functionDeclaration = declaration as FunctionDecl;
                if (functionDeclaration != null)
                {
                    OutSet.GetLocalControlVariable(currentFunctionName).WriteMemory(OutSnapshot,
                        new MemoryEntry(OutSet.CreateFunction(functionDeclaration, Flow.CurrentScript)));
                }
                else
                {
                    var methodDeclaration = declaration as MethodDecl;
                    if (methodDeclaration != null)
                    {
                        OutSet.GetLocalControlVariable(currentFunctionName).WriteMemory(OutSnapshot,
                            new MemoryEntry(OutSet.CreateFunction(methodDeclaration, Flow.CurrentScript)));
                    }
                }

                //superglobal variables
                OutSet.FetchFromGlobal(new VariableName("GLOBALS"));
                OutSet.FetchFromGlobal(new VariableName("_SERVER"));
                OutSet.FetchFromGlobal(new VariableName("_GET"));
                OutSet.FetchFromGlobal(new VariableName("_POST"));
                OutSet.FetchFromGlobal(new VariableName("_FILES"));
                OutSet.FetchFromGlobal(new VariableName("_COOKIE"));
                OutSet.FetchFromGlobal(new VariableName("_SESSION"));
                OutSet.FetchFromGlobal(new VariableName("_REQUEST"));
                OutSet.FetchFromGlobal(new VariableName("_ENV"));

                FunctionValue thisFunction = OutSet.GetLocalControlVariable(currentFunctionName).ReadMemory(OutSet.Snapshot).PossibleValues.First() as FunctionValue;
                if (caller.OutSet.GetLocalControlVariable(calledFunctionsName).IsDefined(caller.OutSet.Snapshot))
                {
                    List<Value> newCalledFunctions = IncreaseCalledInfo(thisFunction, caller.OutSet.GetLocalControlVariable(calledFunctionsName).ReadMemory(caller.OutSnapshot).PossibleValues);
                    OutSet.GetLocalControlVariable(calledFunctionsName).WriteMemory(OutSet.Snapshot, new MemoryEntry(newCalledFunctions));
                }
                else 
                {
                    OutSet.GetLocalControlVariable(calledFunctionsName).WriteMemory(OutSet.Snapshot, new MemoryEntry(OutSet.CreateInfo(new NumberOfCalledFunctions<FunctionValue>(thisFunction, 1))));
                }
                OutSet.GetLocalControlVariable(currentScript).WriteMemory(OutSet.Snapshot, new MemoryEntry(OutSet.CreateString(caller.OwningPPGraph.OwningScript.FullName)));
            }
        }


        private List<Value> IncreaseCalledInfo<T>(T thisFile, IEnumerable<Value> files)
        {
            List<Value> result = new List<Value>();
            bool containsInclude = false;
            foreach (var value in files)
            {
                InfoValue<NumberOfCalledFunctions<T>> info = value as InfoValue<NumberOfCalledFunctions<T>>;
                if (info != null)
                {
                    if (info.Data.Function.Equals(thisFile))
                    {
                        containsInclude = true;
                        result.Add(OutSet.CreateInfo(new NumberOfCalledFunctions<T>(thisFile, info.Data.TimesCalled + 1)));
                    }
                    else
                    {
                        result.Add(value);
                    }
                }
                else
                {
                    result.Add(value);
                }
            }
            if (containsInclude == false)
            {
                result.Add(OutSet.CreateInfo(new NumberOfCalledFunctions<T>(thisFile, 1)));
            }
            return result;
        }

        private void decrease(FlowOutputSet outSet, string thisFile)
        {
            var includedFiles = outSet.GetControlVariable(new VariableName(".includedFiles"));
            IEnumerable<Value> files = includedFiles.ReadMemory(outSet.Snapshot).PossibleValues;
            List<Value> result = DecreaseCalledInfo(outSet, thisFile, files);
            includedFiles.WriteMemory(outSet.Snapshot, new MemoryEntry(result));
        }


        private List<Value> DecreaseCalledInfo<T>(FlowOutputSet outSet, T thisFile, IEnumerable<Value> files)
        {
            List<Value> result = new List<Value>();
            foreach (var value in files)
            {
                InfoValue<NumberOfCalledFunctions<T>> info = value as InfoValue<NumberOfCalledFunctions<T>>;
                if (info != null)
                {
                    if (info.Data.Function.Equals(thisFile))
                    {
                        result.Add(outSet.CreateInfo(new NumberOfCalledFunctions<T>(thisFile, info.Data.TimesCalled - 1)));
                    }
                    else
                    {
                        result.Add(value);
                    }
                }
                else
                {
                    result.Add(value);
                }
            }
            return result;
        }


        private void increaseEvalDepth(FlowOutputSet outSet)
        {
            List<Value> result = new List<Value>();
            result.AddRange(Flow.ExpressionEvaluator.BinaryEx(outSet.GetControlVariable(evalDepth).ReadMemory(outSet.Snapshot), Operations.Add, new MemoryEntry(outSet.CreateInt(1))).PossibleValues);            
            outSet.GetControlVariable(evalDepth).WriteMemory(outSet.Snapshot,new MemoryEntry(result));
        }


        private void decreaseEvalDepth(FlowOutputSet outSet)
        {
            List<Value> result=new List<Value>();
            result.AddRange(Flow.ExpressionEvaluator.BinaryEx(outSet.GetControlVariable(evalDepth).ReadMemory(outSet.Snapshot), Operations.Sub, new MemoryEntry(outSet.CreateInt(1))).PossibleValues);
            outSet.GetControlVariable(evalDepth).WriteMemory(outSet.Snapshot,new MemoryEntry(result));
        }


        /// <inheritdoc />
        public override MemoryEntry InitializeObject(ReadSnapshotEntryBase newObject, MemoryEntry[] arguments)
        {
            var newObjectValue = newObject.ReadMemory(InSnapshot);

            Flow.CalledObject = newObjectValue;
            Flow.Arguments = arguments;

            var constructorName = new QualifiedName(new Name("__construct"));

            var constructors = resolveMethod(newObject, constructorName, arguments);
            if (constructors.Count > 0)
            {
                setCallBranching(constructors);
            }

            return newObjectValue;
        }

        /// <summary>
        /// Resolve return value from all possible calls. It also applies user hints for flags removal
        /// </summary>
        /// <param name="dispatchedExtensions">All calls on dispatch level, which return value is resolved</param>
        /// <returns>Resolved return value</returns>
        public override MemoryEntry ResolveReturnValue(IEnumerable<ExtensionPoint> dispatchedExtensions)
        {

            var calls = dispatchedExtensions.ToArray();
            if (calls.Length == 1)
            {
                var outSet = calls[0].Graph.End.OutSet;
                if (outSet == null)
                {
                    return new MemoryEntry(OutSet.UndefinedValue);
                }
                applyHints(outSet);
                if (calls[0].Caller is IncludingExPoint)
                {
                    decrease(OutSet, calls[0].OwningPPGraph.OwningScript.FullName);
                }
                if (calls[0].Caller is EvalExPoint)
                {
                    decreaseEvalDepth(OutSet);
                }
                return outSet.GetLocalControlVariable(SnapshotBase.ReturnValue).ReadMemory(outSet.Snapshot);
            }
            else
            {
                Debug.Assert(calls.Length > 0, "There must be at least one call");
                HashSet<string> fileNames = new HashSet<string>();
                var values = new HashSet<Value>();
                bool decreaseEvalDepth = false;
                foreach (var call in calls)
                {
                    var outSet = call.Graph.End.OutSet;
                    if (outSet == null)
                    {
                        values.Add(OutSet.UndefinedValue);
                        continue;
                    }
                    applyHints(outSet);
                    if (call.Caller is IncludingExPoint)
                    {
                        fileNames.Add(call.OwningPPGraph.OwningScript.FullName);
                    }
                    if (calls[0].Caller is EvalExPoint)
                    {
                        decreaseEvalDepth = true;
                    }
                    var returnValue = outSet.GetLocalControlVariable(SnapshotBase.ReturnValue).ReadMemory(outSet.Snapshot);
                    values.UnionWith(returnValue.PossibleValues);
                }
                foreach (string file in fileNames)
                {
                    decrease(OutSet, file);
                }
                if (decreaseEvalDepth)
                {
                    this.decreaseEvalDepth(OutSet);
                }
                return new MemoryEntry(values);
            }
        }


        /// <inheritdoc />
        public override MemoryEntry Return(MemoryEntry value)
        {
            OutSet.GetLocalControlVariable(SnapshotBase.ReturnValue).WriteMemory(OutSet.Snapshot, value);
            return value;
        }

        #endregion

        #region Private helpers



        private void applyHints(FlowOutputSet outSet)
        {
            var currentFunctionEntry = outSet.GetLocalControlVariable(currentFunctionName).ReadMemory(outSet.Snapshot);
            if (currentFunctionEntry.Count != 1)
            {
                return;
            }

            var enumerator = currentFunctionEntry.PossibleValues.GetEnumerator();
            enumerator.MoveNext();
            var currentFunction = enumerator.Current as FunctionValue;

            if (currentFunction != null)
            {
                var functionDeclaration = currentFunction.DeclaringElement as FunctionDecl;
                if (functionDeclaration != null)
                {
                    if (!functions.ContainsKey(functionDeclaration))
                    {
                        functions.Add(functionDeclaration,
                            new FunctionHints(functionDeclaration.PHPDoc, functionDeclaration));
                    }

                    functions[functionDeclaration].applyHints(outSet);
                }
                else
                {
                    var methodDeclaration = currentFunction.DeclaringElement as MethodDecl;
                    if (methodDeclaration != null)
                    {
                        if (!methods.ContainsKey(methodDeclaration))
                        {
                            methods.Add(methodDeclaration,
                                new FunctionHints(methodDeclaration.PHPDoc, methodDeclaration));
                        }

                        methods[methodDeclaration].applyHints(outSet);
                    }
                }
            }
        }

        private void setWarning(string message)
        {
            AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(Flow.CurrentScript.FullName, message, Element));
        }

        private void setWarning(string message, AnalysisWarningCause cause)
        {
            AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(Flow.CurrentScript.FullName, message, Element, cause));
        }

        private void setWarning(string message, LangElement element, AnalysisWarningCause cause)
        {
            AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(Flow.CurrentScript.FullName, message, element, cause));
        }

        private void fatalError(bool removeFlowChildren)
        {
            fatalError(Flow, removeFlowChildren);
        }

        private void fatalError(FlowController flow, bool removeFlowChildren)
        {
            var catchedType = new GenericQualifiedName(new QualifiedName(new Name(string.Empty)));
            var catchVariable = new VariableIdentifier(string.Empty);
            var description = new CatchBlockDescription(flow.ProgramEnd, catchedType, catchVariable);
            var info = new ThrowInfo(description, new MemoryEntry());

            var throws = new ThrowInfo[] { info };
            flow.SetThrowBranching(throws, removeFlowChildren);
        }

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
            // TODO: Resolving via visitor might be better
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

        private void setCallBranching(Dictionary<object, FunctionValue> functions)
        {
            if (functions.Count == 0)
            {
                fatalError(true);
            }
            
            Dictionary<object, FunctionValue> newFunctions = new Dictionary<object, FunctionValue>();
            foreach (var entry in functions)
            {
                if (entry.Value.MethodDecl != null)
                {
                    if (entry.Value.MethodDecl.Body == null)
                    {
                        setWarning("Cannot call function without body", AnalysisWarningCause.CANNOT_CALL_METHOD_WITHOUT_BODY);
                        continue;
                    }
                }
                newFunctions.Add(entry.Key, entry.Value);
            }
            functions = newFunctions;


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
                addCallBranch(function);
            }
        }

        /// <summary>
        /// Add new branch into flow. Calls given function
        /// Handles sharing program points based of control variable .calledFunctions
        /// </summary>
        /// <param name="function">called function</param>
        protected virtual void addCallBranch(FunctionValue function)
        {
            // Create graph for every function - NOTE: We can share pp graphs
            var functionName = function.Name;
            ProgramPointGraph functionGraph;

            bool useSharedFunctions = false;
            if (OutSet.GetLocalControlVariable(callDepthName).IsDefined(OutSet.Snapshot))
            {
                MemoryEntry callDepthEntry = OutSet.GetLocalControlVariable(callDepthName).ReadMemory(OutSet.Snapshot);
                var maxValue = new MaxValueVisitor(OutSet);
                if (maxValue.Evaluate(callDepthEntry) > 10)
                {
                    useSharedFunctions = true;
                }
            }

            int max = 0;
            if (OutSet.GetLocalControlVariable(new VariableName(".calledFunctions")).IsDefined(OutSet.Snapshot))
            {
                foreach (var value in OutSet.GetLocalControlVariable(new VariableName(".calledFunctions")).ReadMemory(OutSet.Snapshot).PossibleValues)
                {
                    InfoValue<NumberOfCalledFunctions<FunctionValue>> info = value as InfoValue<NumberOfCalledFunctions<FunctionValue>>;
                    if (info != null && info.Data.Function.Equals(function))
                    {
                        max = Math.Max(max, info.Data.TimesCalled);
                    }
                }
            }
            if (max >= 3)
            {
                useSharedFunctions = true;
            }

            if (sharedFunctions.Contains(function) || useSharedFunctions)
            {
                if (sharedFunctions.Contains(function))
                {
                    //set graph sharing for this function
                    if (!sharedProgramPoints.ContainsKey(function))
                    {
                        //create single graph instance
                        sharedProgramPoints[function] = ProgramPointGraph.From(function);
                    }

                    //get shared instance of program point graph
                    functionGraph = sharedProgramPoints[function];
                }
                else
                {
                    functionGraph = ProgramPointGraph.From(function);
                    sharedFunctions.Add(function);
                }
            }
            else
            {
                functionGraph = ProgramPointGraph.From(function);

            }

            Flow.AddExtension(function.DeclaringElement, functionGraph, ExtensionType.ParallelCall);
        }

        /// <summary>
        /// Resolving names according to given memory entry
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        private List<QualifiedName> getSubroutineNames(MemoryEntry functionName)
        {
            bool isAlwaysConcrete=true;
            var names = GetFunctionNames(functionName, Flow, out isAlwaysConcrete);

            if (isAlwaysConcrete == false)
            {
                setWarning("Couldn't resolve all possible calls", AnalysisWarningCause.COULDNT_RESOLVE_ALL_CALLS);
            }

            var qualifiedNames = new List<QualifiedName>(names.Count);
            foreach (var name in names)
            {
                qualifiedNames.Add(new QualifiedName(new Name(name)));
            }

            return qualifiedNames;
        }

        /// <summary>
        /// Resolve function names from memory entry and resutrn list of possible called function names
        /// </summary>
        /// <param name="functionName">Input memory entry</param>
        /// <param name="flow">FlowController</param>
        /// <param name="isAlwaysConcrete">out parameter that indicates if function name contains some unresolvable value like anyvalue</param>
        /// <returns>List of string with possible function names</returns>
        public static List<string> GetFunctionNames(MemoryEntry functionName, FlowController flow, out bool isAlwaysConcrete)
        {
            var stringConverter = new StringConverter(flow);
            var stringEntry = stringConverter.Evaluate(functionName, out isAlwaysConcrete);
            var names = new List<string>();

            foreach (var stringValue in stringEntry)
            {
                names.Add(stringValue.Value);
            }

            return names;
        }



        private Dictionary<object, FunctionValue> resolveFunction(QualifiedName name,
            MemoryEntry[] arguments)
        {
            var result = new Dictionary<object, FunctionValue>();

            if (nativeFunctionAnalyzer.existNativeFunction(name))
            {
                var function = OutSet.CreateFunction(name.Name,
                    new NativeAnalyzer(nativeFunctionAnalyzer.GetInstance(name), Flow.CurrentPartial));
                
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

            if (result.Count == 0)
            {
                setWarning("Function " + name.Name.Value + " doesn't exists", AnalysisWarningCause.FUNCTION_DOESNT_EXISTS);
            }

            return result;
        }

        private Dictionary<object, FunctionValue> resolveMethod(ReadSnapshotEntryBase thisObject,
            QualifiedName name, MemoryEntry[] arguments)
        {
            var result = new Dictionary<object, FunctionValue>();

            var methods = thisObject.ResolveMethod(OutSnapshot, name);
            foreach (var method in methods)
            {
                result[method.DeclaringElement] = method;
            }
            
            if (result.Count == 0)
            {
                setWarning("Method " + name.Name.Value + " doesn't exists", AnalysisWarningCause.FUNCTION_DOESNT_EXISTS);
            }

            return result;
        }

        private Dictionary<object, FunctionValue> resolveStaticMethod(TypeValue value, QualifiedName name, MemoryEntry[] arguments)
        {
            var result = new Dictionary<object, FunctionValue>();
            var methods = OutSet.ResolveStaticMethod(value, name);
            foreach (var method in methods)
            {
                result[method.DeclaringElement] = method;
            }

            if (result.Count == 0)
            {
                setWarning("Method " + name.Name.Value + " doesn't exists", AnalysisWarningCause.FUNCTION_DOESNT_EXISTS);
            }

            return result;

        }

        private static List<ObjectValue> resolveObjectsForMember(MemoryEntry entry,
            out bool isPossibleNonObject)
        {
            var objectValues = new List<ObjectValue>();
            isPossibleNonObject = false;

            foreach (var variableValue in entry.PossibleValues)
            {
                // TODO: Inside method, $this variable is an object, otherwise a runtime error has occurred.
                // The problem is that we do not know the name of variable and we cannot detect it.

                var objectInstance = variableValue as ObjectValue;
                if (objectInstance != null)
                {
                    objectValues.Add(objectInstance);
                }
                else
                {
                    if (!isPossibleNonObject)
                    {
                        isPossibleNonObject = true;
                    }
                }
            }

            return objectValues;
        }

        private void setNamedArguments(FlowOutputSet callInput, MemoryEntry[] arguments, Signature signature)
        {

            var callPoint = (Flow.ProgramPoint as ExtensionPoint).Caller as RCallPoint;

            if (callPoint == null)
            {
                return;
            }
            var callSignature = callPoint.CallSignature;
            var enumerator = callPoint.Arguments.GetEnumerator();
            int argMin = signature.FormalParams.Count;
            int argMax = signature.FormalParams.Count;
            for (int i = signature.FormalParams.Count-1; i >= 0; i--)
            {
                if (signature.FormalParams[i].InitValue == null)
                {
                    break;
                }
                argMin--;
            }
            if (argMin > callPoint.Arguments.Count() || argMax < callPoint.Arguments.Count())
            {
                AnalysisWarningHandler.SetWarning(callInput,new AnalysisWarning(Flow.CurrentScript.FullName,"Wrong number of arguments",callPoint.Partial,AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
            }

            for (int i = 0; i < Math.Min(signature.FormalParams.Count, arguments.Count()); ++i)
            {
                enumerator.MoveNext();

                var param = signature.FormalParams[i];
                var callParam = callSignature.Value.Parameters[i];

                var argumentVar = callInput.GetVariable(new VariableIdentifier(param.Name));

                if (callParam.PublicAmpersand || param.PassedByRef)
                {
                    argumentVar.SetAliases(callInput.Snapshot, enumerator.Current.Value);
                }
                else
                {
                    argumentVar.WriteMemory(callInput.Snapshot, arguments[i]);
                }
            }
            if(arguments.Count()<signature.FormalParams.Count)
            for (int i = arguments.Count(); i < signature.FormalParams.Count; ++i)
            {
                var param = signature.FormalParams[i];

                var argumentVar = callInput.GetVariable(new VariableIdentifier(param.Name));

                if (param.PassedByRef)
                {
                    argumentVar.SetAliases(callInput.Snapshot, enumerator.Current.Value);
                }
                else
                {
                    if(param.InitValue!=null)
                    {
                        var initializer = new ObjectInitializer(Flow.ExpressionEvaluator);
                        param.InitValue.VisitMe(initializer);
                        argumentVar.WriteMemory(callInput.Snapshot, initializer.initializationValue);
                    }
                }
            
            }

        }

        private void setOrderedArguments(FlowOutputSet callInput, MemoryEntry[] arguments)
        {
            var argCount = new MemoryEntry(callInput.CreateInt(arguments.Length));
            var argCountEntry = callInput.GetVariable(new VariableIdentifier(".argument_count"));
            argCountEntry.WriteMemory(callInput.Snapshot, argCount);

            var index = 0;
            var callPoint = (Flow.ProgramPoint as ExtensionPoint).Caller as RCallPoint;
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

        private void IncreaseStackSize(FlowOutputSet calledOutSet)
        {
            List<Value> newStackSize = new List<Value>();
            if (calledOutSet.GetLocalControlVariable(callDepthName).IsDefined(calledOutSet.Snapshot))
            {
                var result=Flow.ExpressionEvaluator.BinaryEx(calledOutSet.GetLocalControlVariable(callDepthName).ReadMemory(calledOutSet.Snapshot),Operations.Add,new MemoryEntry(OutSet.CreateInt(1)));
                newStackSize.AddRange(result.PossibleValues);
            }
            else
            {
                newStackSize.Add(OutSet.CreateInt(1));
            }

            OutSet.GetLocalControlVariable(callDepthName).WriteMemory(OutSet.Snapshot, new MemoryEntry(newStackSize));
        }

        #endregion



    }

    #region function hints

    /// <summary>
    /// Represents user hints for analyzer.
    /// User can define hint in php doc of function and method.
    /// Exapmle @wev-hint returnvalue remove HTMLDirty
    /// analyzer will remove html dirty flag from return value of this function
    /// With this functionality, cas user create its own cleaning functions
    /// </summary>
    public class FunctionHints
    {
        private HashSet<FlagType> returnHints;
        private LangElement declaration;

        /// <summary>
        /// Creates new instance of FunctionHints
        /// </summary>
        /// <param name="doc">PHPDocument comment</param>
        /// <param name="langElement">function or method declaration</param>
        public FunctionHints(PHPDocBlock doc, LangElement langElement)
        {
            declaration = langElement;
            returnHints = new HashSet<FlagType>();

            string comment;
            if (doc == null)
            {
                comment = string.Empty;
            }
            else
            {
                comment = doc.ToString();
            }

            List<FormalParam> parameters = null;
            if (declaration is MethodDecl)
            {
                parameters = (declaration as MethodDecl).Signature.FormalParams;
            }
            else if (declaration is FunctionDecl)
            {
                parameters = (declaration as FunctionDecl).Signature.FormalParams;
            }

            var endOfRegexp = "(";
            var values = FlagType.GetValues(typeof(FlagType));
            foreach (FlagType val in values)
            {
                endOfRegexp += val + "|";
            }

            endOfRegexp += "all)";
            var returnPatern = "^[ \t]*\\*?[ \t]*@wev-hint[ \t]+sanitize[ \t]+" + endOfRegexp;
            var retRegEx = new Regex(returnPatern, RegexOptions.IgnoreCase);

            foreach (var line in comment.Split('\n'))
            {
                var match = retRegEx.Match(line);
                if (match.Success)
                {
                    var res = match.Groups[1].Value.ToString();
                    foreach (FlagType val in values)
                    {
                        if (val.ToString().ToLower() == res.ToString().ToLower())
                        {
                            addReturnHint(val);
                        }

                        if (res == "all")
                        {
                            addReturnHint(val);
                        }
                    }
                }
            }
        }

        private void addReturnHint(FlagType type)
        {
            returnHints.Add(type);
        }

    
        /// <summary>
        /// Apply current hints on analyzed branch
        /// </summary>
        /// <param name="outSet">FlowOutputSet</param>
        public void applyHints(FlowOutputSet outSet)
        {

            foreach (var type in returnHints)
            {
                var result = outSet.GetLocalControlVariable(SnapshotBase.ReturnValue).ReadMemory(outSet.Snapshot);
                outSet.GetLocalControlVariable(SnapshotBase.ReturnValue).WriteMemory(outSet.Snapshot, new MemoryEntry(FlagsHandler.Clean(result.PossibleValues, type)));
            }
        }
    }
    #endregion

    /// <summary>
    /// Imutable class stores iformation about number of calls of specified function or included file in one recursion
    /// </summary>
    /// <typeparam name="T">Function type</typeparam>
    public class NumberOfCalledFunctions<T>
    {
        /// <summary>
        /// function or include information
        /// </summary>
        public readonly T Function;
        /// <summary>
        /// number of calls or includes
        /// </summary>
        public readonly int TimesCalled;

        /// <summary>
        /// Creates bew instacne of NumberOfCalledFunctions
        /// </summary>
        /// <param name="function">Function infomation</param>
        /// <param name="timesCalled">number of calls or includes</param>
        public NumberOfCalledFunctions(T function, int timesCalled)
        {
            Function = function;
            TimesCalled = timesCalled;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            if (Function != null)
            {
                return Function.GetHashCode() + TimesCalled.GetHashCode();
            }
            else
            {
                return TimesCalled.GetHashCode();
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is NumberOfCalledFunctions<T>)
            {
                NumberOfCalledFunctions<T> other = obj as NumberOfCalledFunctions<T>;
                if (this.Function == null && other.Function == null)
                {
                    return this.TimesCalled == other.TimesCalled;

                }
                else if (this.Function != null && other.Function != null)
                {
                    return this.TimesCalled == other.TimesCalled && this.Function.Equals(other.Function);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Function.ToString() + ":" + TimesCalled;
        }
    }

}
