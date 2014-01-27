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
        public GlobalCode globalCode { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionResolver" /> class.
        /// </summary>
        public FunctionResolver()
        {
            nativeFunctionAnalyzer = NativeFunctionAnalyzer.CreateInstance();
        }

        #region FunctionResolverBase overrides

        /// <inheritdoc />
        public override void Call(QualifiedName name, MemoryEntry[] arguments)
        {
            var functions = resolveFunction(name, arguments);
            setCallBranching(functions);
        }

        /// <inheritdoc />
        public override void MethodCall(ReadSnapshotEntryBase calledObject, QualifiedName name,
            MemoryEntry[] arguments)
        {
            var methods = resolveMethod(calledObject, name, arguments);
            setCallBranching(methods);
        }

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
                        StaticMethodCall(visitor.className, name, arguments);
                        break;
                    case StaticObjectVisitorResult.MULTIPLE_RESULTS:
                        break;
                }
            }
        }

        /// <inheritdoc />
        public override void StaticMethodCall(QualifiedName typeName, QualifiedName name, MemoryEntry[] arguments)
        {
            IEnumerable<TypeValue> types=ExpressionEvaluator.ExpressionEvaluator.ResolveSourceOrNativeType(typeName, OutSet, Element);
            foreach (var type in types)
            {
                var methods = resolveStaticMethod(type, name, arguments);
                setCallBranching(methods);
            }

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

        /// <inheritdoc />
        public override void IndirectStaticMethodCall(QualifiedName typeName,
            MemoryEntry name, MemoryEntry[] arguments)
        {
            var functions = new Dictionary<object, FunctionValue>();
            var functionNames = getSubroutineNames(name);
            IEnumerable<TypeValue> types = ExpressionEvaluator.ExpressionEvaluator.ResolveSourceOrNativeType(typeName, OutSet, Element);
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
                        IndirectStaticMethodCall(visitor.className, name, arguments);
                        break;
                    case StaticObjectVisitorResult.MULTIPLE_RESULTS:
                        break;
                }
            }
        }

        /// <summary>
        /// Initialize call into callInput.
        /// NOTE:
        ///     arguments has to be initialized
        ///     sharing program point graphs is possible
        /// </summary>
        /// <param name="callInput">Input of initialized call</param>
        /// <param name="extensionGraph">Graph representing initialized call</param>
        /// <param name="arguments">Call arguments</param>
        public override void InitializeCall(ProgramPointGraph extensionGraph,
            MemoryEntry[] arguments)
        {
            var declaration = extensionGraph.SourceObject;
            var signature = getSignature(declaration);
            var hasNamedSignature = signature.HasValue;

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
                    new MemoryEntry(OutSet.CreateFunction(functionDeclaration)));
            }
            else
            {
                var methodDeclaration = declaration as MethodDecl;
                if (methodDeclaration != null)
                {
                    OutSet.GetLocalControlVariable(currentFunctionName).WriteMemory(OutSnapshot,
                        new MemoryEntry(OutSet.CreateFunction(methodDeclaration)));
                }
            }
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
        public override void DeclareGlobal(TypeDecl declaration)
        {
            var objectAnalyzer = NativeObjectAnalyzer.GetInstance(Flow.OutSet);
            ClassDeclBuilder type = convertToClassDecl(declaration);
            if (objectAnalyzer.ExistClass(declaration.Type.QualifiedName))
            {
                setWarning("Cannot redeclare class/interface " + declaration.Type.QualifiedName, AnalysisWarningCause.CLASS_ALLREADY_EXISTS);
            }
            else if (OutSet.ResolveType(declaration.Type.QualifiedName).Count() != 0)
            {
                setWarning("Cannot redeclare class/interface " + declaration.Type.QualifiedName, AnalysisWarningCause.CLASS_ALLREADY_EXISTS);
            }
            else
            {
                if (type.IsInterface)
                {
                    DeclareInterface(declaration, objectAnalyzer, type);
                }
                else
                {
                    if (declaration.BaseClassName != null)
                    {
                        if (objectAnalyzer.ExistClass(declaration.BaseClassName.Value.QualifiedName))
                        {
                            ClassDecl baseClass = objectAnalyzer.GetClass(declaration.BaseClassName.Value.QualifiedName);
                            if (baseClass.IsInterface == true)
                            {
                                setWarning("Class " + declaration.BaseClassName.Value.QualifiedName + " not found", AnalysisWarningCause.CLASS_DOESNT_EXIST);
                            }
                            else if (baseClass.IsFinal)
                            {
                                setWarning("Cannot extend final class " + declaration.Type.QualifiedName, AnalysisWarningCause.FINAL_CLASS_CANNOT_BE_EXTENDED);
                            }
                            else
                            {
                                ClassDeclBuilder newType = CopyInfoFromBaseClass(baseClass, type);
                                ClassDecl finalNewType = checkClassAndCopyConstantsFromInterfaces(newType, declaration);
                                insetStaticVariablesIntoMM(finalNewType);
                                OutSet.DeclareGlobal(OutSet.CreateType(finalNewType));
                            }
                        }
                        else
                        {
                            IEnumerable<TypeValue> types = OutSet.ResolveType(declaration.BaseClassName.Value.QualifiedName);
                            if (types.Count() == 0)
                            {
                                setWarning("Class " + declaration.BaseClassName.Value.QualifiedName + " not found", AnalysisWarningCause.CLASS_DOESNT_EXIST);
                            }
                            else
                            {
                                foreach (var value in types)
                                {
                                    if (value is TypeValue)
                                    {
                                        if ((value as TypeValue).Declaration.IsInterface)
                                        {
                                            setWarning("Class " + (value as TypeValue).Declaration.QualifiedName.Name + " not found", AnalysisWarningCause.CLASS_DOESNT_EXIST);
                                        }
                                        else if ((value as TypeValue).Declaration.IsFinal)
                                        {
                                            setWarning("Cannot extend final class " + declaration.Type.QualifiedName, AnalysisWarningCause.FINAL_CLASS_CANNOT_BE_EXTENDED);
                                        }
                                        ClassDeclBuilder newType = CopyInfoFromBaseClass((value as TypeValue).Declaration, type);
                                        ClassDecl finalNewType = checkClassAndCopyConstantsFromInterfaces(newType, declaration);
                                        insetStaticVariablesIntoMM(finalNewType);
                                        OutSet.DeclareGlobal(OutSet.CreateType(finalNewType));
                                    }
                                    else
                                    {
                                        setWarning("Class " + declaration.BaseClassName.Value.QualifiedName + " not found", AnalysisWarningCause.CLASS_DOESNT_EXIST);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        ClassDecl finalType = checkClassAndCopyConstantsFromInterfaces(type, declaration);
                        insetStaticVariablesIntoMM(finalType);
                        OutSet.DeclareGlobal(OutSet.CreateType(finalType));
                    }
                }
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

        #region Object Model


        private ClassDecl checkClassAndCopyConstantsFromInterfaces(ClassDeclBuilder type, TypeDecl element)
        {
            foreach (var entry in type.SourceCodeMethods)
            {
                if (entry.Key.ClassName.Equals(type.QualifiedName))
                {
                    MethodDecl method = entry.Value.MethodDecl;
                    if (method.Modifiers.HasFlag(PhpMemberAttributes.Abstract))
                    {
                        if (method.Body != null)
                        {
                            setWarning("Abstract method cannot have body", element, AnalysisWarningCause.ABSTRACT_METHOD_CANNOT_HAVE_BODY);
                        }
                    }
                    else
                    {
                        if (method.Body == null)
                        {
                            setWarning("Non abstract method must have body", element, AnalysisWarningCause.NON_ABSTRACT_METHOD_MUST_HAVE_BODY);
                        }
                    }
                }
            }
            //cannot contain abstract method
            if (type.IsAbstract == false)
            {
                Dictionary<Name, bool> methods = new Dictionary<Name, bool>();

                foreach (var entry in type.SourceCodeMethods)
                {
                    if (methods.ContainsKey(entry.Key.Name))
                    {
                        methods[entry.Key.Name] &= entry.Value.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Abstract);
                    }
                    else
                    {
                        methods.Add(entry.Key.Name, entry.Value.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Abstract));
                    }
                }

                foreach (var entry in type.ModeledMethods)
                {
                    if (methods.ContainsKey(entry.Key.Name))
                    {
                        methods[entry.Key.Name] &= entry.Value.IsAbstract;
                    }
                    else
                    {
                        methods.Add(entry.Key.Name, entry.Value.IsAbstract);
                    }
                }

                foreach (var entry in methods)
                {
                    if (entry.Value == true)
                    {
                        setWarning("Non abstract class cannot contain abstract method " + entry.Key, element, AnalysisWarningCause.NON_ABSTRACT_CLASS_CONTAINS_ABSTRACT_METHOD);
                    }
                }
            }

            List<ClassDecl> interfaces = getImplementedInterfaces(element);
            foreach (var Interface in interfaces)
            {
                foreach (var constant in Interface.Constants)
                {
                    var query = type.Constants.Where(a => a.Key.Name == constant.Key.Name);
                    if (query.Count() > 0)
                    {
                        setWarning("Cannot override interface constant " + constant.Key.Name, element, AnalysisWarningCause.CANNOT_OVERRIDE_INTERFACE_CONSTANT);
                    }
                    else
                    {
                        type.Constants.Add(new FieldIdentifier(type.QualifiedName, constant.Key.Name), constant.Value.CloneWithNewQualifiedName(type.QualifiedName));
                    }
                }

                foreach (var method in Interface.ModeledMethods)
                {
                    if (!type.SourceCodeMethods.ContainsKey(new MethodIdentifier(type.QualifiedName, method.Key.Name)))
                    {
                        setWarning("Class " + type.QualifiedName + " doesn't implement method " + method.Key.Name, element, AnalysisWarningCause.CLASS_DOENST_IMPLEMENT_ALL_INTERFACE_METHODS);
                    }
                    else
                    {
                        var classMethod = type.SourceCodeMethods[new MethodIdentifier(type.QualifiedName, method.Key.Name)];
                        checkIfStaticMatch(method.Value, classMethod.MethodDecl, element);
                        if (!AreMethodsCompatible(classMethod.MethodDecl, method.Value))
                        {
                            setWarning("Can't inherit abstract function " + classMethod.MethodDecl.Name + " beacuse arguments doesn't match", classMethod.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                        }
                    }
                }

                foreach (var method in Interface.SourceCodeMethods)
                {
                    if (!type.SourceCodeMethods.ContainsKey(new MethodIdentifier(type.QualifiedName, method.Key.Name)))
                    {
                        setWarning("Class " + type.QualifiedName + " doesn't implement method " + method.Key.Name, element, AnalysisWarningCause.CLASS_DOENST_IMPLEMENT_ALL_INTERFACE_METHODS);
                    }
                    else
                    {
                        var classMethod = type.SourceCodeMethods[new MethodIdentifier(type.QualifiedName, method.Key.Name)];

                        checkIfStaticMatch(method.Value.MethodDecl, classMethod.
                            MethodDecl, element);
                        if (!AreMethodsCompatible(classMethod.MethodDecl, method.Value.MethodDecl))
                        {
                            setWarning("Can't inherit abstract function " + classMethod.MethodDecl.Name + " beacuse arguments doesn't match", classMethod.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                        }
                    }
                }
            }

            return type.Build();
        }

        private void DeclareInterface(TypeDecl declaration, NativeObjectAnalyzer objectAnalyzer, ClassDeclBuilder type)
        {
            ClassDeclBuilder result = new ClassDeclBuilder();
            result.QualifiedName = type.QualifiedName;
            result.IsInterface = true;
            result.IsFinal = false;
            result.IsAbstract = false;
            result.Constants = type.Constants;
            result.SourceCodeMethods = new Dictionary<MethodIdentifier, FunctionValue>(type.SourceCodeMethods);
            result.ModeledMethods = new Dictionary<MethodIdentifier, MethodInfo>(type.ModeledMethods);


            if (type.Fields.Count != 0)
            {
                setWarning("Interface cannot contain fields", AnalysisWarningCause.INTERFACE_CANNOT_CONTAIN_FIELDS);
            }

            foreach (var method in type.SourceCodeMethods.Values)
            {
                if (method.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Private) || method.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Protected))
                {
                    setWarning("Interface method must be public", method.MethodDecl, AnalysisWarningCause.INTERFACE_METHOD_MUST_BE_PUBLIC);
                }
                if (method.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Final))
                {
                    setWarning("Interface method cannot be final", method.MethodDecl, AnalysisWarningCause.INTERFACE_METHOD_CANNOT_BE_FINAL);
                }
                if (method.MethodDecl.Body != null)
                {
                    setWarning("Interface method cannot have body", method.MethodDecl, AnalysisWarningCause.INTERFACE_METHOD_CANNOT_HAVE_IMPLEMENTATION);
                }
            }

            List<ClassDecl> interfaces = getImplementedInterfaces(declaration);

            if (interfaces.Count != 0)
            {
                foreach (var value in interfaces)
                {
                    if (value.IsInterface == false)
                    {
                        setWarning("Interface " + value.QualifiedName + " not found", AnalysisWarningCause.INTERFACE_DOESNT_EXIST);
                    }
                    else
                    {
                        //interface cannot have implement
                        foreach (var method in value.SourceCodeMethods.Values)
                        {
                            if (result.ModeledMethods.Values.Where(a => a.Name == method.MethodDecl.Name).Count() > 0 || result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                            {
                                if (result.ModeledMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                                {
                                    var match = result.ModeledMethods.Values.Where(a => a.Name == method.Name).First();
                                    if (!AreMethodsCompatible(match, method.MethodDecl))
                                    {
                                        setWarning("Can't inherit abstract function " + method.MethodDecl.Name + " beacuse arguments doesn't match", method.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                                    }
                                    checkIfStaticMatch(method.MethodDecl, match, declaration);
                                }
                                else if (result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                                {
                                    var match = result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).First();
                                    if (!AreMethodsCompatible(match.MethodDecl, method.MethodDecl))
                                    {
                                        setWarning("Can't inherit abstract function " + method.MethodDecl.Name + " beacuse arguments doesn't match", method.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                                    }
                                    checkIfStaticMatch(method.MethodDecl, match.MethodDecl, declaration);
                                }

                            }
                            else
                            {
                                result.SourceCodeMethods.Add(new MethodIdentifier(result.QualifiedName, method.Name), method);
                            }
                        }
                        foreach (var method in value.ModeledMethods.Values)
                        {
                            if (result.ModeledMethods.Values.Where(a => a.Name == method.Name).Count() > 0 || result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                            {

                                if (result.ModeledMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                                {
                                    var match = result.ModeledMethods.Values.Where(a => a.Name == method.Name).First();
                                    if (!AreMethodsCompatible(match, method))
                                    {
                                        setWarning("Can't inherit abstract function " + method.Name + " beacuse arguments doesn't match", declaration, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                                    }
                                    checkIfStaticMatch(method, match, declaration);
                                }
                                else if (result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                                {
                                    var match = result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).First();
                                    if (!AreMethodsCompatible(match.MethodDecl, method))
                                    {
                                        setWarning("Can't inherit abstract function " + method.Name + " beacuse arguments doesn't match", match.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                                    }
                                    checkIfStaticMatch(method, match.MethodDecl, declaration);
                                }

                            }
                            else
                            {
                                result.ModeledMethods.Add(new MethodIdentifier(result.QualifiedName, method.Name), method);
                            }
                        }

                        foreach (var constant in value.Constants.Values)
                        {
                            var query = type.Constants.Values.Where(a => a.Name.Equals(constant.Name));
                            if (query.Count() > 0)
                            {
                                setWarning("Cannot override interface constant " + constant.Name, declaration, AnalysisWarningCause.CANNOT_OVERRIDE_INTERFACE_CONSTANT);
                            }
                            else
                            {
                                type.Constants.Add(new FieldIdentifier(value.QualifiedName, constant.Name), constant);
                            }
                        }
                    }
                }
            }

            var finalResult = result.Build();
            insetStaticVariablesIntoMM(finalResult);
            OutSet.DeclareGlobal(OutSet.CreateType(finalResult));
        }

        private List<ClassDecl> getImplementedInterfaces(TypeDecl declaration)
        {
            NativeObjectAnalyzer objectAnalyzer = NativeObjectAnalyzer.GetInstance(Flow.OutSet);
            List<ClassDecl> interfaces = new List<ClassDecl>();
            foreach (GenericQualifiedName Interface in declaration.ImplementsList)
            {
                if (objectAnalyzer.ExistClass(Interface.QualifiedName))
                {
                    var interfaceType = objectAnalyzer.GetClass(Interface.QualifiedName);
                    interfaces.Add(interfaceType);
                }
                else if (OutSet.ResolveType(Interface.QualifiedName).Count() == 0)
                {
                    setWarning("Interface " + Interface.QualifiedName + " not found", AnalysisWarningCause.INTERFACE_DOESNT_EXIST);
                }
                else
                {
                    foreach (var interfaceValue in OutSet.ResolveType(Interface.QualifiedName))
                    {
                        if (interfaceValue is TypeValue)
                        {
                            interfaces.Add((interfaceValue as TypeValue).Declaration);
                        }
                        else
                        {
                            setWarning("Interface " + Interface.QualifiedName + " not found", AnalysisWarningCause.INTERFACE_DOESNT_EXIST);
                        }
                    }
                }
            }
            return interfaces;
        }

        private bool AreMethodsCompatible(MethodDecl a, MethodDecl b)
        {
            if (a.Signature.FormalParams.Count == b.Signature.FormalParams.Count)
            {
                for (int i = 0; i < a.Signature.FormalParams.Count; i++)
                {
                    if (a.Signature.FormalParams[i].PassedByRef != b.Signature.FormalParams[i].PassedByRef)
                    {
                        return false;
                    }
                    if (a.Signature.FormalParams[i].InitValue == null ^ a.Signature.FormalParams[i].InitValue == null)
                    {
                        return false;

                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool AreMethodsCompatible(MethodDecl a, MethodInfo b)
        {
            return AreMethodsCompatible(b, a);
        }

        private bool AreMethodsCompatible(MethodInfo a, MethodDecl b)
        {
            if (a.Arguments.Count == b.Signature.FormalParams.Count)
            {
                for (int i = 0; i < a.Arguments.Count; i++)
                {
                    if (a.Arguments[i].ByReference != b.Signature.FormalParams[i].PassedByRef)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool AreMethodsCompatible(MethodInfo a, MethodInfo b)
        {
            if (a.Arguments.Count == b.Arguments.Count)
            {
                for (int i = 0; i < a.Arguments.Count; i++)
                {
                    if (a.Arguments[i].ByReference != b.Arguments[i].ByReference)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool checkIfStaticMatch(MethodInfo method, MethodDecl overridenMethod, LangElement element)
        {
            if (overridenMethod.Modifiers.HasFlag(PhpMemberAttributes.Static) != method.IsStatic)
            {
                if (method.IsStatic)
                {
                    setWarning("Cannot redeclare static method with non static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_STATIC_METHOD_WITH_NON_STATIC);
                }
                else
                {
                    setWarning("Cannot redeclare non static method with static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC);
                }
                return false;
            }
            return true;
        }

        private bool checkIfStaticMatch(MethodInfo method, MethodInfo overridenMethod, LangElement element)
        {
            if (overridenMethod.IsStatic != method.IsStatic)
            {
                if (method.IsStatic)
                {
                    setWarning("Cannot redeclare static method with non static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_STATIC_METHOD_WITH_NON_STATIC);
                }
                else
                {
                    setWarning("Cannot redeclare non static method with static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC);
                }
                return false;
            }
            return true;
        }

        private bool checkIfStaticMatch(MethodDecl method, MethodDecl overridenMethod, LangElement element)
        {
            if (overridenMethod.Modifiers.HasFlag(PhpMemberAttributes.Static) != method.Modifiers.HasFlag(PhpMemberAttributes.Static))
            {
                if (method.Modifiers.HasFlag(PhpMemberAttributes.Static))
                {
                    setWarning("Cannot redeclare static method with non static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_STATIC_METHOD_WITH_NON_STATIC);
                }
                else
                {
                    setWarning("Cannot redeclare non static method with static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC);
                }return false;
            }
            return true;
        }

        private bool checkIfStaticMatch(MethodDecl method, MethodInfo overridenMethod, LangElement element)
        {
            if (overridenMethod.IsStatic != method.Modifiers.HasFlag(PhpMemberAttributes.Static))
            {
                if (method.Modifiers.HasFlag(PhpMemberAttributes.Static))
                {
                    setWarning("Cannot redeclare static method with non static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_STATIC_METHOD_WITH_NON_STATIC);
                }
                else
                {
                    setWarning("Cannot redeclare non static method with static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC);
                }
                return false;
            }
            return true;
        }

        private ClassDeclBuilder CopyInfoFromBaseClass(ClassDecl baseClass, ClassDeclBuilder currentClass)
        {

            ClassDeclBuilder result = new ClassDeclBuilder();
            result.Fields = new Dictionary<FieldIdentifier, FieldInfo>(baseClass.Fields);
            result.Constants = new Dictionary<FieldIdentifier, ConstantInfo>(baseClass.Constants);
            result.SourceCodeMethods = new Dictionary<MethodIdentifier, FunctionValue>(baseClass.SourceCodeMethods);
            result.ModeledMethods = new Dictionary<MethodIdentifier, MethodInfo>(baseClass.ModeledMethods);
            result.QualifiedName = currentClass.QualifiedName;
            result.BaseClasses = new List<QualifiedName>(baseClass.BaseClasses);
            result.BaseClasses.Add(baseClass.QualifiedName);
            result.IsFinal = currentClass.IsFinal;
            result.IsInterface = currentClass.IsInterface;
            result.IsAbstract = currentClass.IsAbstract;

            foreach (var field in currentClass.Fields)
            {
                var query = result.Fields.Keys.Where(a => a.Name == field.Key.Name);
                FieldIdentifier newFieldIdentifier = new FieldIdentifier(result.QualifiedName, field.Key.Name);
                if (query.Count() == 0)
                {
                    result.Fields.Add(newFieldIdentifier, field.Value);
                }
                else
                {
                    FieldIdentifier fieldIdentifier = query.First();
                    if (result.Fields[fieldIdentifier].IsStatic != field.Value.IsStatic)
                    {
                        var fieldName = result.Fields[fieldIdentifier].Name;
                        if (field.Value.IsStatic)
                        {
                            setWarning("Cannot redeclare non static " + fieldName + " with static " + fieldName, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_FIELD_WITH_STATIC);
                        }
                        else
                        {
                            setWarning("Cannot redeclare static " + fieldName + " with non static " + fieldName, AnalysisWarningCause.CANNOT_REDECLARE_STATIC_FIELD_WITH_NON_STATIC);

                        }
                    }
                    else
                    {
                        result.Fields.Add(newFieldIdentifier, field.Value);
                    }
                }
            }

            foreach (var constant in currentClass.Constants)
            {
                result.Constants.Add(constant.Key, constant.Value);
            }
            //todo test method overriding
            foreach (var method in currentClass.SourceCodeMethods.Values)
            {
                MethodIdentifier methodIdentifier = new MethodIdentifier(result.QualifiedName, method.Name);
                if (result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).Count() == 0)
                {
                    if (result.ModeledMethods.Values.Where(a => a.Name == method.Name).Count() == 0)
                    {
                        result.SourceCodeMethods.Add(methodIdentifier, method);
                    }
                    else
                    {
                        var overridenMethod = result.ModeledMethods.Values.Where(a => a.Name == method.Name).First();
                        bool containsErrors = false;
                        if (overridenMethod.IsFinal)
                        {
                            setWarning("Cannot redeclare final method " + method.MethodDecl.Name, method.MethodDecl, AnalysisWarningCause.CANNOT_REDECLARE_FINAL_METHOD);
                            containsErrors = true;
                        }
                        if (!checkIfStaticMatch(method.MethodDecl, overridenMethod, method.MethodDecl))
                        {
                            containsErrors = true;
                        }

                        if (!AreMethodsCompatible(overridenMethod, method.MethodDecl))
                        {
                            setWarning("Can't inherit function " + method.MethodDecl.Name + ", beacuse arguments doesn't match", method.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                            containsErrors = true;
                        }
                        if (method.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Abstract))
                        {
                            setWarning("Can't override function " + method.MethodDecl.Name + ", with abstract function", method.MethodDecl, AnalysisWarningCause.CANNOT_OVERRIDE_FUNCTION_WITH_ABSTRACT);
                            containsErrors = true;
                        }
                        if (containsErrors == false)
                        {
                            result.SourceCodeMethods[new MethodIdentifier(currentClass.QualifiedName, method.Name)] = method;
                            result.SourceCodeMethods.Remove(new MethodIdentifier(overridenMethod.ClassName, overridenMethod.Name));
                        }
                    }
                }
                else
                {
                    var key = result.SourceCodeMethods.Keys.Where(a => a.Name == method.Name).First();
                    var overridenMethod = result.SourceCodeMethods[key];
                    bool containsErrors = false;
                    if (overridenMethod.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Final))
                    {
                        setWarning("Cannot redeclare final method " + method.MethodDecl.Name, method.MethodDecl, AnalysisWarningCause.CANNOT_REDECLARE_FINAL_METHOD);
                        containsErrors = true;
                    }

                    if (!checkIfStaticMatch(method.MethodDecl, overridenMethod.MethodDecl, method.MethodDecl))
                    {
                        containsErrors = true;
                    }

                    if (!AreMethodsCompatible(overridenMethod.MethodDecl, method.MethodDecl))
                    {
                        setWarning("Can't inherit function " + method.MethodDecl.Name + ", beacuse arguments doesn't match", method.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                        containsErrors = true;
                    }
                    if (method.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Abstract))
                    {
                        setWarning("Can't override function " + method.MethodDecl.Name + ", with abstract function", method.MethodDecl, AnalysisWarningCause.CANNOT_OVERRIDE_FUNCTION_WITH_ABSTRACT);
                        containsErrors = true;
                    }
                    if (containsErrors == false)
                    {
                        result.SourceCodeMethods.Remove(key);
                        result.SourceCodeMethods[new MethodIdentifier(currentClass.QualifiedName, method.Name)] = method;
                    }
                }
            }

            return result;
        }

        private ClassDeclBuilder convertToClassDecl(TypeDecl declaration)
        {
            ClassDeclBuilder result = new ClassDeclBuilder();
            result.BaseClasses = new List<QualifiedName>();
            if (declaration.BaseClassName.HasValue)
            {
                result.BaseClasses.Add(declaration.BaseClassName.Value.QualifiedName);
            }
            result.IsFinal = declaration.Type.IsFinal;
            result.IsInterface = declaration.Type.IsInterface;
            result.IsAbstract = declaration.Type.IsAbstract;
            result.QualifiedName = new QualifiedName(declaration.Name);

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
                        if (result.Fields.ContainsKey(new FieldIdentifier(result.QualifiedName, field.Name)))
                        {
                            setWarning("Cannot redeclare field " + field.Name, member, AnalysisWarningCause.CLASS_MULTIPLE_FIELD_DECLARATION);
                        }
                        else
                        {
                            result.Fields.Add(new FieldIdentifier(result.QualifiedName, field.Name), new FieldInfo(field.Name, result.QualifiedName, "any", visibility, field.Initializer, isStatic));
                        }
                    }

                }
                else if (member is ConstDeclList)
                {
                    foreach (var constant in (member as ConstDeclList).Constants)
                    {
                        if (result.Constants.ContainsKey(new FieldIdentifier(result.QualifiedName, constant.Name)))
                        {
                            setWarning("Cannot redeclare constant " + constant.Name, member, AnalysisWarningCause.CLASS_MULTIPLE_CONST_DECLARATION);
                        }
                        else
                        {
                            //in php all object constatns are public
                            Visibility visbility = Visibility.PUBLIC;
                            result.Constants.Add(new FieldIdentifier(result.QualifiedName, constant.Name), new ConstantInfo(constant.Name, result.QualifiedName, visbility, constant.Initializer));
                        }
                    }
                }
                else if (member is MethodDecl)
                {
                    var methosIdentifier = new MethodIdentifier(result.QualifiedName, (member as MethodDecl).Name);
                    if (!result.SourceCodeMethods.ContainsKey(methosIdentifier))
                    {
                        result.SourceCodeMethods.Add(methosIdentifier, OutSet.CreateFunction(member as MethodDecl));
                    }
                    else
                    {
                        setWarning("Cannot redeclare method " + (member as MethodDecl).Name, member, AnalysisWarningCause.CLASS_MULTIPLE_FUNCTION_DECLARATION);
                    }
                }
                else
                {
                    //ignore traits are not supported by AST, only by parser
                }
            }

            return result;
        }

        private void insetStaticVariablesIntoMM(ClassDecl result)
        {
            Dictionary<VariableName, ConstantInfo> constants = new Dictionary<VariableName, ConstantInfo>();
            List<QualifiedName> classes = new List<QualifiedName>(result.BaseClasses);
            classes.Add(result.QualifiedName);

            NativeObjectAnalyzer objectAnalyzer = NativeObjectAnalyzer.GetInstance(Flow.OutSet);

            foreach (var currentClass in classes)
            {
                foreach (var constant in result.Constants.Values.Where(a => a.ClassName == currentClass))
                {
                    constants[constant.Name] = constant;
                }
            }
            string code = "function _static_intialization_of_" + result.QualifiedName + "(){$res=array();";
            foreach (var constant in constants.Values)
            {

                var variable = OutSet.GetControlVariable(new VariableName(".class(" + result.QualifiedName.Name.LowercaseValue + ")->constant(" + constant.Name + ")"));
                List<Value> constantValues = new List<Value>();
                if (variable.IsDefined(OutSet.Snapshot))
                {
                    constantValues.AddRange(variable.ReadMemory(OutSet.Snapshot).PossibleValues);
                }
                if (constant.Value != null)
                {
                    constantValues.AddRange(constant.Value.PossibleValues);
                    variable.WriteMemory(OutSet.Snapshot, new MemoryEntry(constantValues));
                }
                else
                {
                    string index = ".class(" + result.QualifiedName.Name.LowercaseValue + ")->constant(" + constant.Name + ")";
                    code += "$res[\"" + index + "\"]=" + globalCode.SourceUnit.GetSourceCode(constant.Initializer.Position) + ";\n";
                }
            }
            if (result.IsInterface == false)
            {
                var staticVariables = OutSet.GetControlVariable(new VariableName(".staticVariables")).ReadIndex(OutSet.Snapshot, new MemberIdentifier(result.QualifiedName.Name.LowercaseValue));
                staticVariables.WriteMemory(OutSet.Snapshot, new MemoryEntry(OutSet.CreateArray()));
                ReadWriteSnapshotEntryBase publicContainer = staticVariables.ReadIndex(OutSet.Snapshot, new MemberIdentifier("public"));
                publicContainer.WriteMemory(OutSet.Snapshot, new MemoryEntry(OutSet.CreateArray()));
                ReadWriteSnapshotEntryBase nonPublicContainer = staticVariables.ReadIndex(OutSet.Snapshot, new MemberIdentifier("non-public"));
                nonPublicContainer.WriteMemory(OutSet.Snapshot, new MemoryEntry(OutSet.CreateArray()));

                if (result.BaseClasses.Count > 0)
                {
                    for (int i = result.BaseClasses.Count - 1; i >= 0; i--)
                    {
                        if (Flow.OutSet.ResolveType(result.BaseClasses[i]).Count() == 0)
                        {
                            InsertNativeObjectStaticVariablesIntoMM(result.BaseClasses[i]);

                        }
                    }
                }
                HashSet<VariableName> usedFields = new HashSet<VariableName>();
                foreach (var field in result.Fields.Values.Where(a => (a.IsStatic == true && a.ClassName == result.QualifiedName)))
                {
                    usedFields.Add(field.Name);
                    if (field.Initializer != null)
                    {
                        string index = "";
                        if (field.Visibility == Visibility.PUBLIC)
                        {
                            index = "@class@" + result.QualifiedName.Name.LowercaseValue + "@->publicfield@" + field.Name + "@";
                        }
                        else
                        {
                            index = "@class@" + result.QualifiedName.Name.LowercaseValue + "@->nonpublicfield@(" + field.Name + "@";
                        }
                        code += "$res[\"" + index + "\"]=" + globalCode.SourceUnit.GetSourceCode(field.Initializer.Position) + ";\n";
                    }
                    else
                    {
                        MemoryEntry fieldValue;
                        if (field.InitValue != null)
                        {
                            fieldValue = field.InitValue;
                        }
                        else
                        {
                            fieldValue = new MemoryEntry(OutSet.UndefinedValue);
                        }

                        ReadWriteSnapshotEntryBase container = null;
                        if (field.Visibility == Visibility.PUBLIC)
                        {
                            container = publicContainer;
                        }
                        else
                        {
                            container = nonPublicContainer;
                        }
                        container.ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.Name.Value)).WriteMemory(OutSet.Snapshot, fieldValue);
                    }
                }

                //insert parent values
                for (int i = result.BaseClasses.Count - 1; i >= 0; i--)
                {
                    foreach (var field in result.Fields.Values.Where(a => (a.IsStatic == true && a.ClassName == result.BaseClasses[i])))
                    {
                        if (usedFields.Contains(field.Name))
                        {
                            //add
                            usedFields.Add(field.Name);
                        }
                    }
                }
            }
            string key = "staticInit" + result.QualifiedName.Name.LowercaseValue;
            if (!Flow.ExtensionKeys.Contains(key))
            {
                var fileName = "./cfg_test.php";
                var fullPath = new FullPath(Path.GetDirectoryName(fileName));
                var sourceFile = new PhpSourceFile(fullPath, new FullPath(fileName));
                code = @"<?php " + code + " } ?>";

                var parser = new SyntaxParser(sourceFile, code);
                parser.Parse();

                var function = (parser.Ast.Statements[0] as FunctionDecl);
                var parameters = new List<ActualParam>();
                parameters.Add(new ActualParam(Position.Invalid, new DirectVarUse(Position.Invalid, "res"), false));
                function.Body.Add(new ExpressionStmt(Position.Invalid,
                new DirectFcnCall(Position.Invalid, new QualifiedName(new Name(".initStaticProperties")), null, Position.Invalid, parameters, new List<TypeRef>())));

                var ppGraph = ProgramPointGraph.From(OutSet.CreateFunction(function));

                Flow.AddExtension(key, ppGraph, ExtensionType.ParallelCall);
            }
        }

        private void InsertNativeObjectStaticVariablesIntoMM(QualifiedName qualifiedName)
        {
            NativeObjectAnalyzer objectAnalyzer = NativeObjectAnalyzer.GetInstance(OutSet);
            Debug.Assert(objectAnalyzer.ExistClass(qualifiedName));
            ClassDecl classs = objectAnalyzer.GetClass(qualifiedName);

            for (int i = 0; i < classs.BaseClasses.Count; i++)
            {
                var staticVariables = OutSet.GetControlVariable(new VariableName(".staticVariables")).ReadIndex(OutSet.Snapshot, new MemberIdentifier(classs.BaseClasses[i].Name.LowercaseValue));
                if (!staticVariables.IsDefined(OutSet.Snapshot))
                {
                    staticVariables.WriteMemory(OutSet.Snapshot, new MemoryEntry(OutSet.CreateArray()));
                    ReadWriteSnapshotEntryBase publicContainer = staticVariables.ReadIndex(OutSet.Snapshot, new MemberIdentifier("public"));
                    publicContainer.WriteMemory(OutSet.Snapshot, new MemoryEntry(OutSet.CreateArray()));
                    ReadWriteSnapshotEntryBase nonPublicContainer = staticVariables.ReadIndex(OutSet.Snapshot, new MemberIdentifier("non-public"));
                    nonPublicContainer.WriteMemory(OutSet.Snapshot, new MemoryEntry(OutSet.CreateArray()));
                    ClassDecl currentClass = objectAnalyzer.GetClass(classs.BaseClasses[i]);
                    ReadWriteSnapshotEntryBase currentContainer = null;
                    foreach (var field in currentClass.Fields.Values.Where(a => (a.ClassName == currentClass.QualifiedName && a.IsStatic == true)))
                    {
                        if (field.Visibility == Visibility.PUBLIC)
                        {
                            currentContainer = publicContainer;
                        }
                        else
                        {
                            currentContainer = nonPublicContainer;
                        }
                        currentContainer.ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.Name.Value)).WriteMemory(OutSet.Snapshot, field.InitValue);
                    }
                }
            }
        }

        #endregion

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
            AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(message, Element));
        }

        private void setWarning(string message, AnalysisWarningCause cause)
        {
            AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(message, Element, cause));
        }

        private void setWarning(string message, LangElement element, AnalysisWarningCause cause)
        {
            AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(message, element, cause));
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
            var ppGraph = ProgramPointGraph.From(function);
            Flow.AddExtension(function.DeclaringElement, ppGraph, ExtensionType.ParallelCall);
        }

        /// <summary>
        /// Resolving names according to given memory entry
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        private List<QualifiedName> getSubroutineNames(MemoryEntry functionName)
        {
            var names = new HashSet<string>();
            foreach (var possibleValue in functionName.PossibleValues)
            {
                var stringValue = possibleValue as StringValue;
                // TODO: Other values convert to string
                if (stringValue == null)
                {
                    continue;
                }

                names.Add(stringValue.Value);
            }

            var qualifiedNames = new List<QualifiedName>(names.Count);
            foreach (var name in names)
            {
                qualifiedNames.Add(new QualifiedName(new Name(name)));
            }

            return qualifiedNames;
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
                // TODO: Check whether the number of arguments match.
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
                // TODO: Check whether the number of arguments match.
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
            for (int i = 0; i < signature.FormalParams.Count; ++i)
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

        #endregion


    }

    #region function hints
    // TODO: testy treba pockat na priznaky
    // TODO pravdepodobne sa zmeni praca s priznakmi
    internal class FunctionHints
    {
        private HashSet<DirtyType> returnHints;
        private Dictionary<VariableIdentifier, HashSet<DirtyType>> argumentHints;
        private LangElement declaration;

        internal FunctionHints(PHPDocBlock doc, LangElement langElement)
        {
            declaration = langElement;
            argumentHints = new Dictionary<VariableIdentifier, HashSet<DirtyType>>();
            returnHints = new HashSet<DirtyType>();

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
            var values = DirtyType.GetValues(typeof(DirtyType));
            foreach (DirtyType val in values)
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
                    foreach (DirtyType val in values)
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
                            foreach (DirtyType val in values)
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

        private void addReturnHint(DirtyType type)
        {
            returnHints.Add(type);
        }

        private void addArgumentHint(VariableIdentifier name, DirtyType type)
        {
            if (!argumentHints.ContainsKey(name))
            {
                argumentHints[name] = new HashSet<DirtyType>();
            }

            argumentHints[name].Add(type);
        }

        internal void applyHints(FlowOutputSet outSet)
        {
              
            foreach (var type in returnHints)
            {
                var result=outSet.GetLocalControlVariable(SnapshotBase.ReturnValue).ReadMemory(outSet.Snapshot);
                outSet.GetLocalControlVariable(SnapshotBase.ReturnValue).WriteMemory(outSet.Snapshot,new MemoryEntry(FlagsHandler.Clean(result.PossibleValues,type)));
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
}
