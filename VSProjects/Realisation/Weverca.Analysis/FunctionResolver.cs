﻿using System;
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
        private static readonly VariableName currentFunctionName = new VariableName("$current_function");
        private NativeFunctionAnalyzer nativeFunctionAnalyzer;
        private Dictionary<MethodDecl, FunctionHints> methods = new Dictionary<MethodDecl, FunctionHints>();
        private Dictionary<FunctionDecl, FunctionHints> functions
            = new Dictionary<FunctionDecl, FunctionHints>();

        private readonly Dictionary<FunctionValue, ProgramPointGraph> sharedProgramPoints = new Dictionary<FunctionValue, ProgramPointGraph>();

        private readonly HashSet<FunctionValue> sharedFunctions = new HashSet<FunctionValue>();


        /// <summary>
        /// Readonly variable name for storing depth of recursion
        /// </summary>
        public static readonly VariableName callDepthName = new VariableName(".callDepth");

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
                        staticMethodCall(visitor.className, name, arguments);
                        break;
                    case StaticObjectVisitorResult.MULTIPLE_RESULTS:
                        break;
                }
            }
        }

        /// <inheritdoc />
        public override void StaticMethodCall(QualifiedName typeName, QualifiedName name, MemoryEntry[] arguments)
        {
            var resolvedTypes = ResolveType(typeName, Flow,OutSet, Element);
            foreach (var resolvedType in resolvedTypes)
            {
                staticMethodCall(resolvedType, name, arguments);
            }
        }

        private void staticMethodCall(QualifiedName typeName, QualifiedName name, MemoryEntry[] arguments)
        {
            IEnumerable<TypeValue> types = ExpressionEvaluator.ExpressionEvaluator.ResolveSourceOrNativeType(typeName,Flow, OutSet, Element);
            foreach (var type in types)
            {
                var methods = resolveStaticMethod(type, name, arguments);
                setCallBranching(methods);
            }
        }

        /// <inheritdoc />
        public override void IndirectStaticMethodCall(ReadSnapshotEntryBase calledObject, MemoryEntry name, MemoryEntry[] arguments)
        {
            var calledObjectValue = calledObject.ReadMemory(InSnapshot);

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
                        indirectStaticMethodCall(visitor.className, name, arguments);
                        break;
                    case StaticObjectVisitorResult.MULTIPLE_RESULTS:
                        break;
                }
            }
        }

        /// <inheritdoc />
        public override void IndirectStaticMethodCall(QualifiedName typeName, MemoryEntry name, MemoryEntry[] arguments)
        {
            foreach (var resolvedType in ResolveType(typeName, Flow, OutSet, Element))
            {
                indirectStaticMethodCall(resolvedType, name, arguments);
            }
        }

        private void indirectStaticMethodCall(QualifiedName typeName, MemoryEntry name, MemoryEntry[] arguments)
        {
            var functions = new Dictionary<object, FunctionValue>();

            var functionNames = getSubroutineNames(name);
            IEnumerable<TypeValue> types = ExpressionEvaluator.ExpressionEvaluator.ResolveSourceOrNativeType(typeName, Flow, OutSet, Element);
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
                string thisFile = Flow.CurrentScript.FullName;
                var includedFiles = OutSet.GetControlVariable(new VariableName(".includedFiles"));
                IEnumerable<Value> files = includedFiles.ReadMemory(OutSnapshot).PossibleValues;
                List<Value> result = IncreaseCalledInfo(thisFile, files);
                includedFiles.WriteMemory(OutSnapshot, new MemoryEntry(result));
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
                List<Value> newCalledFunctions = IncreaseCalledInfo(thisFunction, caller.OutSet.GetLocalControlVariable(new VariableName(".calledFunctions")).ReadMemory(caller.OutSnapshot).PossibleValues);
                OutSet.GetLocalControlVariable(new VariableName(".calledFunctions")).WriteMemory(OutSet.Snapshot, new MemoryEntry(newCalledFunctions));

                OutSet.GetLocalControlVariable(new VariableName("$currentScript")).WriteMemory(OutSet.Snapshot, new MemoryEntry(OutSet.CreateString(caller.OwningPPGraph.OwningScript.FullName)));
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
                applyHints(outSet);
                return outSet.GetLocalControlVariable(SnapshotBase.ReturnValue).ReadMemory(outSet.Snapshot);
            }
            else
            {
                Debug.Assert(calls.Length > 0, "There must be at least one call");

                var values = new HashSet<Value>();
                foreach (var call in calls)
                {
                    var outSet = call.Graph.End.OutSet;
                    applyHints(outSet);
                    var returnValue = outSet.GetLocalControlVariable(SnapshotBase.ReturnValue).ReadMemory(outSet.Snapshot);
                    values.UnionWith(returnValue.PossibleValues);
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
                setWarning("Cannot resolve function call. Function doesn't exist or analysis has not enough information to resolve function call", AnalysisWarningCause.CANNOT_RESOLVE_FUNCTION_CALL);
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


        protected virtual void addCallBranch(FunctionValue function)
        {
            // Create graph for every function - NOTE: We can share pp graphs
            var functionName = function.Name;
            ProgramPointGraph functionGraph;

            bool useSharedFunctions = false;
            if (OutSet.GetLocalControlVariable(callDepthName).IsDefined(OutSet.Snapshot))
            {
                MemoryEntry callDepthEntry = OutSet.GetLocalControlVariable(callDepthName).ReadMemory(OutSet.Snapshot);
                foreach (Value callDepth in callDepthEntry.PossibleValues)
                {
                    if (callDepth is IntegerValue)
                    {
                        if ((callDepth as IntegerValue).Value > 10)
                        {
                            useSharedFunctions = true;
                        }
                    }
                    else
                    {
                        useSharedFunctions = true;
                    }
                }
            }
            int max = 0;
            foreach (var value in OutSet.GetLocalControlVariable(new VariableName(".calledFunctions")).ReadMemory(OutSet.Snapshot).PossibleValues)
            {
                InfoValue<NumberOfCalledFunctions<FunctionValue>> info = value as InfoValue<NumberOfCalledFunctions<FunctionValue>>;
                if (info != null && info.Data.Function.Equals(function))
                {
                    max = Math.Max(max, info.Data.TimesCalled);
                }
            }
            if (max >= 2)
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
            var names = GetFunctionNames(functionName, Flow);
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
        /// <returns>List of string with possible function names</returns>
        public static List<string> GetFunctionNames(MemoryEntry functionName, FlowController flow)
        {
            var stringConverter = new StringConverter(flow);
            bool isAlwaysConcrete;
            // TODO: What happen if isAlwaysConcrete is true?
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
                // TODO: Check whether the number of arguments match.
                result[function.DeclaringElement] = function;
            }
            else
            {
                var functions = OutSet.ResolveFunction(name);
                // TODO: Test if functions.Count > 0

                foreach (var function in functions)
                {
                    // TODO: Check whether the number of arguments match.
                    result[function.DeclaringElement] = function;
                }
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

            return result;

        }

        private List<ObjectValue> resolveObjectsForMember(MemoryEntry entry)
        {
            var isPossibleNonObject = false;
            var objectValues = resolveObjectsForMember(entry, out isPossibleNonObject);

            if (isPossibleNonObject)
            {
                if (objectValues.Count >= 1)
                {
                    // TODO: This must be fatal error
                    setWarning("Possible call to a member function on a non-object",
                        AnalysisWarningCause.METHOD_CALL_ON_NON_OBJECT_VARIABLE);
                }
                else
                {
                    // TODO: This must be fatal error
                    setWarning("Call to a member function on a non-object",
                        AnalysisWarningCause.METHOD_CALL_ON_NON_OBJECT_VARIABLE);
                }
            }

            return objectValues;
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
                MemoryEntry stackSize = calledOutSet.GetLocalControlVariable(callDepthName).ReadMemory(calledOutSet.Snapshot);
                foreach (var value in stackSize.PossibleValues)
                {
                    if (value is AnyFloatValue)
                    {
                        newStackSize.Add(value);
                    }
                    else
                    {
                        newStackSize.Add(OutSet.CreateInt((value as IntegerValue).Value + 1));
                    }

                }
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

    internal class FunctionHints
    {
        private HashSet<FlagType> returnHints;
        private Dictionary<VariableIdentifier, HashSet<FlagType>> argumentHints;
        private LangElement declaration;

        internal FunctionHints(PHPDocBlock doc, LangElement langElement)
        {
            declaration = langElement;
            argumentHints = new Dictionary<VariableIdentifier, HashSet<FlagType>>();
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
            var returnPatern = "^[ \t]*\\*?[ \t]*@wev-hint[ \t]+returnvalue[ \t]+remove[ \t]+" + endOfRegexp;
            var argumentPatern = "^[ \t]*\\*?[ \t]*@wev-hint[ \t]+outargument[ \t]+([a-zA-Z_\x7f-\xff][a-zA-Z0-9_\x7f-\xff]*)[ \t]+remove[ \t]+" + endOfRegexp;
            var retRegEx = new Regex(returnPatern, RegexOptions.IgnoreCase);
            var argRegEx = new Regex(argumentPatern, RegexOptions.IgnoreCase);

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

                var argMatch = argRegEx.Match(line);
                if (argMatch.Success)
                {
                    var argName = argMatch.Groups[1].Value;
                    var res = argMatch.Groups[2].Value.ToString();
                    foreach (var parameter in parameters)
                    {
                        if (parameter.Name.Equals(argName))
                        {
                            foreach (FlagType val in values)
                            {
                                if (val.ToString().ToLower() == res.ToString().ToLower())
                                {
                                    addArgumentHint(new VariableIdentifier(argName), val);
                                }

                                if (res == "all")
                                {
                                    addArgumentHint(new VariableIdentifier(argName), val);
                                }
                            }

                            break;
                        }
                    }
                }
            }
        }

        private void addReturnHint(FlagType type)
        {
            returnHints.Add(type);
        }

        private void addArgumentHint(VariableIdentifier name, FlagType type)
        {
            if (!argumentHints.ContainsKey(name))
            {
                argumentHints[name] = new HashSet<FlagType>();
            }

            argumentHints[name].Add(type);
        }

        internal void applyHints(FlowOutputSet outSet)
        {

            foreach (var type in returnHints)
            {
                var result = outSet.GetLocalControlVariable(SnapshotBase.ReturnValue).ReadMemory(outSet.Snapshot);
                outSet.GetLocalControlVariable(SnapshotBase.ReturnValue).WriteMemory(outSet.Snapshot, new MemoryEntry(FlagsHandler.Clean(result.PossibleValues, type)));
            }

            foreach (var variable in argumentHints.Keys)
            {
                foreach (var flag in argumentHints[variable])
                {
                    var result = outSet.ReadVariable(variable).ReadMemory(outSet.Snapshot);
                    outSet.GetVariable(variable).WriteMemory(outSet.Snapshot, new MemoryEntry(FlagsHandler.Clean(result.PossibleValues, flag)));
                }
            }
        }
    }
    #endregion

    class NumberOfCalledFunctions<T>
    {
        public readonly T Function;
        public readonly int TimesCalled;

        public NumberOfCalledFunctions(T function, int timesCalled)
        {
            Function = function;
            TimesCalled = timesCalled;
        }

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

    }

}
