﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Reflection;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.Analysis
{
    /// <summary>
    /// Resolving function names and function initializing
    /// </summary>
    public class FunctionResolver : FunctionResolverBase
    {
        private static readonly VariableName currentFunctionName = new VariableName("$current_function");
        private VariableName retrunVariable = new VariableName(".return");
        private NativeFunctionAnalyzer nativeFunctionAnalyzer = NativeFunctionAnalyzer.CreateInstance();
        private Dictionary<MethodDecl, FunctionHints> methods = new Dictionary<MethodDecl, FunctionHints>();
        private Dictionary<FunctionDecl, FunctionHints> functions
            = new Dictionary<FunctionDecl, FunctionHints>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionResolver" /> class.
        /// </summary>
        public FunctionResolver()
        {
        }

        #region FunctionResolverBase overrides

        public override void MethodCall(MemoryEntry calledObject, QualifiedName name,
            MemoryEntry[] arguments)
        {
            var objectValues = resolveObjectsForMember(calledObject);
            var methods = resolveMethod(objectValues, name, arguments);
            setCallBranching(methods);
        }

        public override void Call(QualifiedName name, MemoryEntry[] arguments)
        {
            var functions = resolveFunction(name, arguments);
            setCallBranching(functions);
        }

        public override void IndirectMethodCall(MemoryEntry calledObject, MemoryEntry name,
            MemoryEntry[] arguments)
        {
            var methods = new Dictionary<object, FunctionValue>();
            var methodNames = getSubroutineNames(name);
            var objectValues = resolveObjectsForMember(calledObject);

            foreach (var functionName in methodNames)
            {
                var resolvedMethods = resolveMethod(objectValues, functionName, arguments);
                foreach (var resolvedMethod in resolvedMethods)
                {
                    methods[resolvedMethod.Key] = resolvedMethod.Value;
                }
            }

            setCallBranching(methods);
        }

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

        public override MemoryEntry InitializeObject(MemoryEntry newObject, MemoryEntry[] arguments)
        {
            Flow.CalledObject = newObject;
            Flow.Arguments = arguments;

            var constructorName = new QualifiedName(new Name("__construct"));
            var objectValues = new List<ObjectValue>();

            foreach (var value in newObject.PossibleValues)
            {
                Debug.Assert(value is ObjectValue, "All objects are creating now");
                objectValues.Add(value as ObjectValue);
            }

            var constructors = resolveMethod(objectValues, constructorName, arguments);
            if (constructors.Count > 0)
            {
                setCallBranching(constructors);
            }

            return newObject;
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
                return outSet.GetLocalControlVariable(retrunVariable).ReadMemory(outSet.Snapshot);
            }
            else
            {
                Debug.Assert(calls.Length > 0, "There must be at least one call");

                var values = new HashSet<Value>();
                foreach (var call in calls)
                {
                    var outSet = call.Graph.End.OutSet;
                    applyHints(outSet);
                    var returnValue = outSet.GetLocalControlVariable(retrunVariable).ReadMemory(outSet.Snapshot);
                    values.UnionWith(returnValue.PossibleValues);
                }

                return new MemoryEntry(values);
            }
        }

        //TODO abstract stuff
        //TODO if implements interface
        public override void DeclareGlobal(TypeDecl declaration)
        {
            var objectAnalyzer = NativeObjectAnalyzer.GetInstance(Flow);
            ClassDecl type = convertToClassDecl(declaration);
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
                    //TODO dokoncit
                    //check imterface which are implement
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
                                ClassDecl newType = CopyInfoFromBaseClass(baseClass, type);
                                checkClass(newType);
                                OutSet.DeclareGlobal(OutSet.CreateType(newType));
                            }
                        }
                        else
                        {
                            IEnumerable<TypeValueBase> types = OutSet.ResolveType(declaration.BaseClassName.Value.QualifiedName);
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
                                        ClassDecl newType = CopyInfoFromBaseClass((value as TypeValue).Declaration, type);
                                        checkClass(newType);
                                        OutSet.DeclareGlobal(OutSet.CreateType(newType));
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
                        checkClass(type);
                        OutSet.DeclareGlobal(OutSet.CreateType(type));
                    }
                }
            }
        }

        public override MemoryEntry Return(MemoryEntry value)
        {
            OutSet.GetLocalControlVariable(retrunVariable).WriteMemory(OutSet.Snapshot, value);
            return value;
        }

        #endregion

        #region Private helpers

        private void checkClass(ClassDecl type)
        {
            //TODO
            //if one method is abstract whole class has to be abstract
            //check implemented interfaces
            //abstract method cannot have body
        }

        private void DeclareInterface(TypeDecl declaration, NativeObjectAnalyzer objectAnalyzer, ClassDecl type)
        {
            ClassDeclBuilder result = new ClassDeclBuilder();
            result.TypeName = type.QualifiedName;
            result.IsInterface = true;
            result.IsFinal = false;
            result.IsAbstract = false;
            result.SourceCodeMethods = new Dictionary<MethodIdentifier, MethodDecl>(type.SourceCodeMethods);
            result.ModeledMethods = new Dictionary<MethodIdentifier, MethodInfo>(type.ModeledMethods);


            if (type.Fields.Count != 0)
            {
                setWarning("Interface cannot contain fields", AnalysisWarningCause.INTERFACE_CANNOT_CONTAIN_FIELDS);
            }

            foreach (var method in type.SourceCodeMethods.Values)
            {
                if (method.Modifiers.HasFlag(PhpMemberAttributes.Private) || method.Modifiers.HasFlag(PhpMemberAttributes.Protected))
                {
                    setWarning("Interface method must be public", method, AnalysisWarningCause.INTERFACE_METHOD_MUST_BE_PUBLIC);
                }
                if (method.Modifiers.HasFlag(PhpMemberAttributes.Final))
                {
                    setWarning("Interface method cannot be final", method, AnalysisWarningCause.INTERFACE_METHOD_CANNOT_BE_FINAL);
                }
                if (method.Body != null)
                {
                    setWarning("Interface method cannot have body", method, AnalysisWarningCause.INTERFACE_METHOD_CANNOT_HAVE_IMPLEMENTATION);
                }
            }

            foreach (GenericQualifiedName Interface in declaration.ImplementsList)
            {
                List<ClassDecl> interfaces = new List<ClassDecl>();
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
                                if (result.ModeledMethods.Values.Where(a => a.Name == method.Name).Count() > 0 || result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                                {
                                    //if arguments doesnt match
                                    if (result.ModeledMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                                    {
                                        var match = result.ModeledMethods.Values.Where(a => a.Name == method.Name).First();
                                        if (!AreMethodsCompatible(match, method))
                                        {
                                            setWarning("Can't inherit abstract function " + method.Name, method, AnalysisWarningCause.INTERFACE_CANNOT_OVER_WRITE_FUNCTION);
                                        }
                                    }
                                    else if (result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                                    {
                                        var match = result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).First();
                                        if (!AreMethodsCompatible(match, method))
                                        {
                                            setWarning("Can't inherit abstract function " + method.Name, method, AnalysisWarningCause.INTERFACE_CANNOT_OVER_WRITE_FUNCTION);
                                        }
                                    }

                                }
                                else
                                {
                                    result.SourceCodeMethods.Add(new MethodIdentifier(result.TypeName, method.Name), method);
                                }
                            }
                            foreach (var method in value.ModeledMethods.Values)
                            {
                                if (result.ModeledMethods.Values.Where(a => a.Name == method.Name).Count() > 0 || result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                                {
                                    //if arguments doesnt match
                                    if (result.ModeledMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                                    {
                                        var match = result.ModeledMethods.Values.Where(a => a.Name == method.Name).First();
                                        if (!AreMethodsCompatible(match, method))
                                        {
                                            setWarning("Can't inherit abstract function " + method.Name, declaration, AnalysisWarningCause.INTERFACE_CANNOT_OVER_WRITE_FUNCTION);
                                        }
                                    }
                                    else if (result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                                    {
                                        var match = result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).First();
                                        if (!AreMethodsCompatible(match, method))
                                        {
                                            setWarning("Can't inherit abstract function " + method.Name, match, AnalysisWarningCause.INTERFACE_CANNOT_OVER_WRITE_FUNCTION);
                                        }
                                    }

                                }
                                else
                                {
                                    result.ModeledMethods.Add(new MethodIdentifier(result.TypeName, method.Name), method);
                                }
                            }
                        }
                    }
                }
            }

            OutSet.DeclareGlobal(OutSet.CreateType(result.Build()));
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

        private ClassDecl CopyInfoFromBaseClass(ClassDecl baseClass, ClassDecl currentClass)
        {

            ClassDeclBuilder result = new ClassDeclBuilder();
            result.Fields = new Dictionary<FieldIdentifier, FieldInfo>(baseClass.Fields);
            result.Constants = new Dictionary<FieldIdentifier, ConstantInfo>(baseClass.Constants);
            result.SourceCodeMethods = new Dictionary<MethodIdentifier, MethodDecl>(baseClass.SourceCodeMethods);
            result.ModeledMethods = new Dictionary<MethodIdentifier, MethodInfo>(baseClass.ModeledMethods);
            result.TypeName = currentClass.QualifiedName;
            result.BaseClassName = baseClass.QualifiedName;
            result.IsFinal = currentClass.IsFinal;
            result.IsInterface = currentClass.IsInterface;
            result.IsAbstract = currentClass.IsAbstract;

            foreach (var field in currentClass.Fields)
            {
                FieldIdentifier fieldIdentifier = new FieldIdentifier(currentClass.QualifiedName, field.Key.Name);
                if (!result.Fields.ContainsKey(fieldIdentifier))
                {
                    result.Fields.Add(fieldIdentifier, field.Value);
                }
                else
                {
                    if (result.Fields[fieldIdentifier].IsStatic != field.Value.IsStatic)
                    {
                        var fieldName = result.Fields[fieldIdentifier].Name;
                        if (field.Value.IsStatic)
                        {
                            setWarning("Cannot redeclare static " + fieldName + " with non static " + fieldName, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_FIELD_WITH_STATIC);
                        }
                        else
                        {
                            setWarning("Cannot redeclare non static " + fieldName + " with static " + fieldName, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_FIELD_WITH_STATIC);

                        }
                    }
                }
            }

            foreach (var constant in currentClass.Constants)
            {
                result.Constants.Add(constant.Key, constant.Value);
            }

            //TODO extending methods warnings
            foreach (var method in currentClass.SourceCodeMethods.Values)
            {
                MethodIdentifier methodIdentifier = new MethodIdentifier(result.TypeName, method.Name);
                if (result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).Count() == 0)
                {
                    if (result.ModeledMethods.Values.Where(a => a.Name == method.Name).Count() == 0)
                    {
                        result.SourceCodeMethods.Add(methodIdentifier, method);
                    }
                    else
                    {
                        if (result.ModeledMethods.Values.Where(a => a.Name == method.Name).First().IsFinal)
                        {
                            setWarning("Cannot redeclare final method " + method.Name, method, AnalysisWarningCause.CANNOT_REDECLARE_FINAL_METHOD);
                        }
                        //TODO
                        //static non static
                        //arguments has to match
                        //cannot be abstract
                    }
                }
                else
                {
                    if (result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).First().Modifiers.HasFlag(PhpMemberAttributes.Final))
                    {
                        setWarning("Cannot redeclare final method " + method.Name, method, AnalysisWarningCause.CANNOT_REDECLARE_FINAL_METHOD);
                    }
                    //TODO
                    //static non static
                    //arguments has to match
                    //cannot be abstract
                }
            }

            return result.Build();
        }

        private ClassDecl convertToClassDecl(TypeDecl declaration)
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
                            setWarning("Cannot redeclare field " + field.Name, member, AnalysisWarningCause.CLASS_MULTIPLE_FIELD_DECLARATION);
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
                            setWarning("Cannot redeclare constant " + constant.Name, member, AnalysisWarningCause.CLASS_MULTIPLE_CONST_DECLARATION);
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
                        setWarning("Cannot redeclare constant " + (member as MethodDecl).Name, member, AnalysisWarningCause.CLASS_MULTIPLE_CONST_DECLARATION);
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
                // Create graph for every function - NOTE: We can share pp graphs
                var ppGraph = ProgramPointGraph.From(function);
                Flow.AddExtension(function.DeclaringElement, ppGraph, ExtensionType.ParallelCall);
            }
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

        private Dictionary<object, FunctionValue> resolveMethod(IEnumerable<ObjectValue> objects,
            QualifiedName name, MemoryEntry[] arguments)
        {
            var result = new Dictionary<object, FunctionValue>();

            foreach (var objectValue in objects)
            {
                var functions = OutSet.ResolveMethod(objectValue, name);
                foreach (var function in functions)
                {
                    // TODO: Check whether the number of arguments match.
                    result[function.DeclaringElement] = function;
                }
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
            var callSignature = callPoint.CallSignature;
            var enumerator = callPoint.Arguments.GetEnumerator();
            for (int i = 0; i < signature.FormalParams.Count; ++i)
            {
                enumerator.MoveNext();

                var param = signature.FormalParams[i];
                var callParam = callSignature.Value.Parameters[i];

                var argumentVar = callInput.GetVariable(new VariableIdentifier(param.Name));

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

    // TODO: testy treba pockat na priznaky
    // TODO pravdepodobne sa zmeni praca s priznakmi
    internal class FunctionHints
    {
        private HashSet<DirtyType> returnHints;
        private Dictionary<VariableName, HashSet<DirtyType>> argumentHints;
        private LangElement declaration;

        internal FunctionHints(PHPDocBlock doc, LangElement langElement)
        {
            declaration = langElement;
            argumentHints = new Dictionary<VariableName, HashSet<DirtyType>>();
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
                                    addArgumentHint(new VariableName(argName), val);
                                }

                                if (res == "all")
                                {
                                    addArgumentHint(new VariableName(argName), val);
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

        private void addArgumentHint(VariableName name, DirtyType type)
        {
            if (!argumentHints.ContainsKey(name))
            {
                argumentHints[name] = new HashSet<DirtyType>();
            }

            argumentHints[name].Add(type);
        }

        internal void applyHints(FlowOutputSet outset)
        {
            foreach (var type in returnHints)
            {
                throw new NotImplementedException("Return value has been removed from API");
                /*   var result = outset.ReadValue(outset.ReturnValue);
                
                foreach (var value in result.PossibleValues)
                {
                    ValueInfoHandler.setClean(outset, value, type);
                }*/
            }

            foreach (var variable in argumentHints.Keys)
            {
                foreach (var flag in argumentHints[variable])
                {
                    var result = outset.ReadValue(variable);
                    foreach (var value in result.PossibleValues)
                    {
                        ValueInfoHandler.setClean(outset, value, flag);
                    }
                }
            }
        }
    }
}
