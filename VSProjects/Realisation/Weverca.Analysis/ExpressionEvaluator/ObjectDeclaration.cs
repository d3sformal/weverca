using System.Collections.Generic;
using System.Linq;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Reflection;

using Weverca.Analysis.NativeAnalyzers;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Expression evaluation is resolved here
    /// </summary>
    public partial class ExpressionEvaluator : ExpressionEvaluatorBase
    {
        #region Object Model

        /// <inheritdoc />
        public override void DeclareGlobal(TypeDecl declaration)
        {
            var objectAnalyzer = NativeObjectAnalyzer.GetInstance(Flow.OutSet);
            ClassDeclBuilder type = convertToClassDecl(declaration);
            if (type == null)
            {
                fatalError(true);
                return;
            }
            foreach (var method in type.SourceCodeMethods)
            {
                FunctionResolver.methodToClass[method.Value.DeclaringElement]= type.QualifiedName;
            }
            if (objectAnalyzer.ExistClass(declaration.Type.QualifiedName))
            {
                SetWarning("Cannot redeclare class/interface " + declaration.Type.QualifiedName, AnalysisWarningCause.CLASS_ALLREADY_EXISTS);
                fatalError(true);
            }
            else if (OutSet.ResolveType(declaration.Type.QualifiedName).Count() != 0)
            {
                SetWarning("Cannot redeclare class/interface " + declaration.Type.QualifiedName, AnalysisWarningCause.CLASS_ALLREADY_EXISTS);
                fatalError(true);
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
                                SetWarning("Class " + declaration.BaseClassName.Value.QualifiedName + " not found", AnalysisWarningCause.CLASS_DOESNT_EXIST);
                                fatalError(true);
                            }
                            else if (baseClass.IsFinal)
                            {
                                SetWarning("Cannot extend final class " + declaration.Type.QualifiedName, AnalysisWarningCause.FINAL_CLASS_CANNOT_BE_EXTENDED);
                                fatalError(true);
                            }
                            else
                            {
                                ClassDeclBuilder newType = CopyInfoFromBaseClass(baseClass, type);
                                if (newType != null)
                                {
                                    ClassDecl finalNewType = checkClassAndCopyConstantsFromInterfaces(newType, declaration);
                                    if (finalNewType != null)
                                    {
                                        insetStaticVariablesIntoMM(finalNewType);
                                        OutSet.DeclareGlobal(OutSet.CreateType(finalNewType));
                                    }
                                    else 
                                    {
                                        fatalError(true);
                                    }
                                }
                                else
                                {
                                    fatalError(true);
                                }
                            }
                        }
                        else
                        {
                            IEnumerable<TypeValue> types = OutSet.ResolveType(declaration.BaseClassName.Value.QualifiedName);
                            if (types.Count() == 0)
                            {
                                SetWarning("Class " + declaration.BaseClassName.Value.QualifiedName + " not found", AnalysisWarningCause.CLASS_DOESNT_EXIST);
                                fatalError(true);
                            }
                            else
                            {
                                int numberOfWarnings=0;
                                foreach (var value in types)
                                {
                                    if (value is TypeValue)
                                    {
                                        if ((value as TypeValue).Declaration.IsInterface)
                                        {
                                            SetWarning("Class " + (value as TypeValue).Declaration.QualifiedName.Name + " not found", AnalysisWarningCause.CLASS_DOESNT_EXIST);
                                            numberOfWarnings++;
                                        }
                                        else if ((value as TypeValue).Declaration.IsFinal)
                                        {
                                            SetWarning("Cannot extend final class " + declaration.Type.QualifiedName, AnalysisWarningCause.FINAL_CLASS_CANNOT_BE_EXTENDED);
                                            numberOfWarnings++;
                                        }
                                        else
                                        {
                                            ClassDeclBuilder newType = CopyInfoFromBaseClass((value as TypeValue).Declaration, type);
                                            if (newType != null)
                                            {
                                                ClassDecl finalNewType = checkClassAndCopyConstantsFromInterfaces(newType, declaration);
                                                if (finalNewType != null)
                                                {
                                                    insetStaticVariablesIntoMM(finalNewType);
                                                    OutSet.DeclareGlobal(OutSet.CreateType(finalNewType));
                                                }
                                                else 
                                                {
                                                    numberOfWarnings++;
                                                }
                                            }
                                            else
                                            {
                                                numberOfWarnings++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        SetWarning("Class " + declaration.BaseClassName.Value.QualifiedName + " not found", AnalysisWarningCause.CLASS_DOESNT_EXIST);
                                    }
                                }
                                if (numberOfWarnings > 0)
                                {
                                    if (numberOfWarnings == types.Count())
                                    {
                                        fatalError(true);
                                    }
                                    else
                                    {
                                        fatalError(false);
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        ClassDecl finalType = checkClassAndCopyConstantsFromInterfaces(type, declaration);
                        if (finalType == null)
                        {
                            fatalError(true);
                        }
                        else 
                        { 
                            insetStaticVariablesIntoMM(finalType);
                            OutSet.DeclareGlobal(OutSet.CreateType(finalType));
                        }
                    }
                }
            }
        }


        private ClassDecl checkClassAndCopyConstantsFromInterfaces(ClassDeclBuilder type, TypeDecl element)
        {
            bool success = true;
            foreach (var entry in type.SourceCodeMethods)
            {
                if (entry.Key.ClassName.Equals(type.QualifiedName))
                {
                    MethodDecl method = entry.Value.MethodDecl;
                    if (method.Modifiers.HasFlag(PhpMemberAttributes.Abstract))
                    {
                        if (method.Body != null)
                        {
                            SetWarning("Abstract method cannot have body", element, AnalysisWarningCause.ABSTRACT_METHOD_CANNOT_HAVE_BODY);
                            success = false;
                        }
                    }
                    else
                    {
                        if (method.Body == null)
                        {
                            SetWarning("Non abstract method must have body", element, AnalysisWarningCause.NON_ABSTRACT_METHOD_MUST_HAVE_BODY);
                            success = false;
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
                        SetWarning("Non abstract class cannot contain abstract method " + entry.Key, element, AnalysisWarningCause.NON_ABSTRACT_CLASS_CONTAINS_ABSTRACT_METHOD);
                        success = false;
                    }
                }
            }

            List<ClassDecl> interfaces = getImplementedInterfaces(element, ref success);
            foreach (var Interface in interfaces)
            {
                foreach (var constant in Interface.Constants)
                {
                    var query = type.Constants.Where(a => a.Key.Name == constant.Key.Name);
                    if (query.Count() > 0)
                    {
                        SetWarning("Cannot override interface constant " + constant.Key.Name, element, AnalysisWarningCause.CANNOT_OVERRIDE_INTERFACE_CONSTANT);
                        success = false;
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
                        SetWarning("Class " + type.QualifiedName + " doesn't implement method " + method.Key.Name, element, AnalysisWarningCause.CLASS_DOENST_IMPLEMENT_ALL_INTERFACE_METHODS);
                        success = false;
                    }
                    else
                    {
                        var classMethod = type.SourceCodeMethods[new MethodIdentifier(type.QualifiedName, method.Key.Name)];
                        success&=checkIfStaticMatch(method.Value, classMethod.MethodDecl, element);
                        if (!AreMethodsCompatible(classMethod.MethodDecl, method.Value))
                        {
                            SetWarning("Can't inherit abstract function " + classMethod.MethodDecl.Name + " beacuse arguments doesn't match", classMethod.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                            success = false;
                        }
                    }
                }

                foreach (var method in Interface.SourceCodeMethods)
                {
                    if (!type.SourceCodeMethods.ContainsKey(new MethodIdentifier(type.QualifiedName, method.Key.Name)))
                    {
                        SetWarning("Class " + type.QualifiedName + " doesn't implement method " + method.Key.Name, element, AnalysisWarningCause.CLASS_DOENST_IMPLEMENT_ALL_INTERFACE_METHODS);
                        success = false;
                    }
                    else
                    {
                        var classMethod = type.SourceCodeMethods[new MethodIdentifier(type.QualifiedName, method.Key.Name)];

                        success&=checkIfStaticMatch(method.Value.MethodDecl, classMethod.
                            MethodDecl, element);
                        if (!AreMethodsCompatible(classMethod.MethodDecl, method.Value.MethodDecl))
                        {
                            SetWarning("Can't inherit abstract function " + classMethod.MethodDecl.Name + " beacuse arguments doesn't match", classMethod.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                            success = false;
                        }
                    }
                }
            }
            if (success)
            {
                return type.Build();
            }
            else
            {
                return null;
            }
        }

        private void DeclareInterface(TypeDecl declaration, NativeObjectAnalyzer objectAnalyzer, ClassDeclBuilder type)
        {
            bool success = true;
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
                SetWarning("Interface cannot contain fields", AnalysisWarningCause.INTERFACE_CANNOT_CONTAIN_FIELDS);
                success = false;
            }

            foreach (var method in type.SourceCodeMethods.Values)
            {
                if (method.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Private) || method.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Protected))
                {
                    SetWarning("Interface method must be public", method.MethodDecl, AnalysisWarningCause.INTERFACE_METHOD_MUST_BE_PUBLIC);
                    success = false;
                }
                if (method.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Final))
                {
                    SetWarning("Interface method cannot be final", method.MethodDecl, AnalysisWarningCause.INTERFACE_METHOD_CANNOT_BE_FINAL);
                    success = false;
                }
                if (method.MethodDecl.Body != null)
                {
                    SetWarning("Interface method cannot have body", method.MethodDecl, AnalysisWarningCause.INTERFACE_METHOD_CANNOT_HAVE_IMPLEMENTATION);
                    success = false;
                }
            }

            List<ClassDecl> interfaces = getImplementedInterfaces(declaration, ref success);

            if (interfaces.Count != 0)
            {
                foreach (var value in interfaces)
                {
                    if (value.IsInterface == false)
                    {
                        SetWarning("Interface " + value.QualifiedName + " not found", AnalysisWarningCause.INTERFACE_DOESNT_EXIST);
                        success = false;
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
                                        SetWarning("Can't inherit abstract function " + method.MethodDecl.Name + " beacuse arguments doesn't match", method.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                                        success = false;
                                    }
                                    success &= checkIfStaticMatch(method.MethodDecl, match, declaration);
                                }
                                else if (result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                                {
                                    var match = result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).First();
                                    if (!AreMethodsCompatible(match.MethodDecl, method.MethodDecl))
                                    {
                                        SetWarning("Can't inherit abstract function " + method.MethodDecl.Name + " beacuse arguments doesn't match", method.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                                        success = false;
                                    }
                                    success &= checkIfStaticMatch(method.MethodDecl, match.MethodDecl, declaration);
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
                                        SetWarning("Can't inherit abstract function " + method.Name + " beacuse arguments doesn't match", declaration, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                                        success = false;
                                    }
                                    success &= checkIfStaticMatch(method, match, declaration);
                                }
                                else if (result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                                {
                                    var match = result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).First();
                                    if (!AreMethodsCompatible(match.MethodDecl, method))
                                    {
                                        SetWarning("Can't inherit abstract function " + method.Name + " beacuse arguments doesn't match", match.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                                        success = false;
                                    }
                                    success &= checkIfStaticMatch(method, match.MethodDecl, declaration);
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
                                SetWarning("Cannot override interface constant " + constant.Name, declaration, AnalysisWarningCause.CANNOT_OVERRIDE_INTERFACE_CONSTANT);
                                success = false;
                            }
                            else
                            {
                                type.Constants.Add(new FieldIdentifier(value.QualifiedName, constant.Name), constant);
                            }
                        }
                    }
                }
            }
            if (success == true)
            {
                var finalResult = result.Build();
                insetStaticVariablesIntoMM(finalResult);
                OutSet.DeclareGlobal(OutSet.CreateType(finalResult));
            }
            else 
            {
                fatalError(true);
            }
        }

        private List<ClassDecl> getImplementedInterfaces(TypeDecl declaration, ref bool success)
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
                    SetWarning("Interface " + Interface.QualifiedName + " not found", AnalysisWarningCause.INTERFACE_DOESNT_EXIST);
                    success = false;
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
                            SetWarning("Interface " + Interface.QualifiedName + " not found", AnalysisWarningCause.INTERFACE_DOESNT_EXIST);
                            success = false;
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
                    SetWarning("Cannot redeclare static method with non static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_STATIC_METHOD_WITH_NON_STATIC);
                }
                else
                {
                    SetWarning("Cannot redeclare non static method with static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC);
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
                    SetWarning("Cannot redeclare static method with non static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_STATIC_METHOD_WITH_NON_STATIC);
                }
                else
                {
                    SetWarning("Cannot redeclare non static method with static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC);
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
                    SetWarning("Cannot redeclare static method with non static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_STATIC_METHOD_WITH_NON_STATIC);
                }
                else
                {
                    SetWarning("Cannot redeclare non static method with static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC);
                } return false;
            }
            return true;
        }

        private bool checkIfStaticMatch(MethodDecl method, MethodInfo overridenMethod, LangElement element)
        {
            if (overridenMethod.IsStatic != method.Modifiers.HasFlag(PhpMemberAttributes.Static))
            {
                if (method.Modifiers.HasFlag(PhpMemberAttributes.Static))
                {
                    SetWarning("Cannot redeclare static method with non static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_STATIC_METHOD_WITH_NON_STATIC);
                }
                else
                {
                    SetWarning("Cannot redeclare non static method with static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC);
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

            bool success = true;

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
                            SetWarning("Cannot redeclare non static " + fieldName + " with static " + fieldName, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_FIELD_WITH_STATIC);
                            success = false;
                        }
                        else
                        {
                            SetWarning("Cannot redeclare static " + fieldName + " with non static " + fieldName, AnalysisWarningCause.CANNOT_REDECLARE_STATIC_FIELD_WITH_NON_STATIC);
                            success = false;
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
                            SetWarning("Cannot redeclare final method " + method.MethodDecl.Name, method.MethodDecl, AnalysisWarningCause.CANNOT_REDECLARE_FINAL_METHOD);
                            containsErrors = true;
                        }
                        if (!checkIfStaticMatch(method.MethodDecl, overridenMethod, method.MethodDecl))
                        {
                            containsErrors = true;
                        }

                        if (!AreMethodsCompatible(overridenMethod, method.MethodDecl))
                        {
                            SetWarning("Can't inherit function " + method.MethodDecl.Name + ", beacuse arguments doesn't match", method.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                            containsErrors = true;
                        }
                        if (method.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Abstract))
                        {
                            SetWarning("Can't override function " + method.MethodDecl.Name + ", with abstract function", method.MethodDecl, AnalysisWarningCause.CANNOT_OVERRIDE_FUNCTION_WITH_ABSTRACT);
                            containsErrors = true;
                        }
                        if (containsErrors == false)
                        {
                            result.SourceCodeMethods[new MethodIdentifier(currentClass.QualifiedName, method.Name)] = method;
                            result.SourceCodeMethods.Remove(new MethodIdentifier(overridenMethod.ClassName, overridenMethod.Name));
                        }
                        else
                        {
                            success = false;
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
                        SetWarning("Cannot redeclare final method " + method.MethodDecl.Name, method.MethodDecl, AnalysisWarningCause.CANNOT_REDECLARE_FINAL_METHOD);
                        containsErrors = true;
                    }

                    if (!checkIfStaticMatch(method.MethodDecl, overridenMethod.MethodDecl, method.MethodDecl))
                    {
                        containsErrors = true;
                    }

                    if (!AreMethodsCompatible(overridenMethod.MethodDecl, method.MethodDecl))
                    {
                        SetWarning("Can't inherit function " + method.MethodDecl.Name + ", beacuse arguments doesn't match", method.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                        containsErrors = true;
                    }
                    if (method.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Abstract))
                    {
                        SetWarning("Can't override function " + method.MethodDecl.Name + ", with abstract function", method.MethodDecl, AnalysisWarningCause.CANNOT_OVERRIDE_FUNCTION_WITH_ABSTRACT);
                        containsErrors = true;
                    }
                    if (containsErrors == false)
                    {
                        result.SourceCodeMethods.Remove(key);
                        result.SourceCodeMethods[new MethodIdentifier(currentClass.QualifiedName, method.Name)] = method;
                    }
                    else
                    {
                        success = false;
                    }
                }
            }
            if (success == true)
            {
                return result;
            }
            else 
            {
                return null;
            }
        }

        private ClassDeclBuilder convertToClassDecl(TypeDecl declaration)
        {
            bool success = true;
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
                            SetWarning("Cannot redeclare field " + field.Name, member, AnalysisWarningCause.CLASS_MULTIPLE_FIELD_DECLARATION);
                            success = false;
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
                            SetWarning("Cannot redeclare constant " + constant.Name, member, AnalysisWarningCause.CLASS_MULTIPLE_CONST_DECLARATION);
                            success = false;
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
                        result.SourceCodeMethods.Add(methosIdentifier, OutSet.CreateFunction(member as MethodDecl, Flow.CurrentScript));
                    }
                    else
                    {
                        SetWarning("Cannot redeclare method " + (member as MethodDecl).Name, member, AnalysisWarningCause.CLASS_MULTIPLE_FUNCTION_DECLARATION);
                        success = false;
                    }
                }
                else
                {
                    //ignore traits are not supported by AST, only by parser
                }
            }
            var contructIdentifier = new MethodIdentifier(result.QualifiedName, new Name("__construct"));
            if (!result.SourceCodeMethods.ContainsKey(contructIdentifier))
            {
                var id = new MethodIdentifier(result.QualifiedName, result.QualifiedName.Name);
                if (result.SourceCodeMethods.ContainsKey(id))
                {
                    var methodValue = result.SourceCodeMethods[id];
                    var element = methodValue.DeclaringElement as MethodDecl;
                    var newElement = new MethodDecl(element.Position, element.EntireDeclarationPosition, element.HeadingEndPosition, element.DeclarationBodyPosition, "__construct",
                        element.Signature.AliasReturn, element.Signature.FormalParams, new List<FormalTypeParam>(), element.Body, element.Modifiers, element.BaseCtorParams, element.Attributes.Attributes);
                    result.SourceCodeMethods.Add(contructIdentifier, OutSet.CreateFunction(newElement, methodValue.DeclaringScript));
                }
            }
            if (success == true)
            {
                return result;
            }
            else 
            {
                return null;
            }
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

            var initiliazer = new ObjectInitializer(this);
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
                    constant.Initializer.VisitMe(initiliazer);
                    variable.WriteMemory(OutSet.Snapshot, initiliazer.initializationValue);
                }
            }
            if (result.IsInterface == false)
            {
                var staticVariables = OutSet.GetControlVariable(FunctionResolver.staticVariables).ReadIndex(OutSet.Snapshot, new MemberIdentifier(result.QualifiedName.Name.LowercaseValue));
                staticVariables.WriteMemory(OutSet.Snapshot, new MemoryEntry(OutSet.CreateArray()));


                if (result.BaseClasses.Count > 0)
                {
                    for (int i = 0; i < result.BaseClasses.Count; i++)
                    {
                        if (Flow.OutSet.ResolveType(result.BaseClasses[i]).Count() == 0)
                        {
                            InsertNativeObjectStaticVariablesIntoMM(result.BaseClasses[i]);

                        }
                    }
                }

                foreach (var field in result.Fields.Values.Where(a => (a.IsStatic == true && a.ClassName != result.QualifiedName)))
                {
                    var baseClassVariable = OutSet.GetControlVariable(FunctionResolver.staticVariables).ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.ClassName.Name.LowercaseValue)).ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.Name.Value));

                    var currentClassVariable = OutSet.GetControlVariable(FunctionResolver.staticVariables).ReadIndex(OutSet.Snapshot, new MemberIdentifier(result.QualifiedName.Name.LowercaseValue)).ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.Name.Value));
                    if (result.Fields.Values.Where(a => (a.IsStatic == true && a.ClassName == result.QualifiedName && a.Name == field.Name)).Count() == 0)
                    {
                        currentClassVariable.SetAliases(OutSet.Snapshot, baseClassVariable);
                    }
                }
                HashSet<VariableName> usedFields = new HashSet<VariableName>();
                foreach (var field in result.Fields.Values.Where(a => (a.IsStatic == true && a.ClassName == result.QualifiedName)))
                {
                    usedFields.Add(field.Name);
                    if (field.Initializer != null)
                    {
                        field.Initializer.VisitMe(initiliazer);
                        staticVariables.ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.Name.Value)).WriteMemory(OutSet.Snapshot, initiliazer.initializationValue);
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

                        staticVariables.ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.Name.Value)).WriteMemory(OutSet.Snapshot, fieldValue);
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
        }

        private void InsertNativeObjectStaticVariablesIntoMM(QualifiedName qualifiedName)
        {
            NativeObjectAnalyzer objectAnalyzer = NativeObjectAnalyzer.GetInstance(OutSet);
            Debug.Assert(objectAnalyzer.ExistClass(qualifiedName));
            ClassDecl classs = objectAnalyzer.GetClass(qualifiedName);
            List<QualifiedName> classHierarchy = new List<QualifiedName>(classs.BaseClasses);
            classHierarchy.Add(qualifiedName);
            foreach (var name in classHierarchy)
            {
                ClassDecl currentClass = objectAnalyzer.GetClass(name);
                var staticVariables = OutSet.GetControlVariable(FunctionResolver.staticVariables).ReadIndex(OutSet.Snapshot, new MemberIdentifier(name.Name.LowercaseValue));
                if (!staticVariables.IsDefined(OutSet.Snapshot))
                {
                    staticVariables.WriteMemory(OutSet.Snapshot, new MemoryEntry(OutSet.CreateArray()));

                    foreach (var field in currentClass.Fields.Values.Where(a => (a.ClassName != currentClass.QualifiedName && a.IsStatic == true)))
                    {
                        var baseClassVariable = OutSet.GetControlVariable(FunctionResolver.staticVariables).ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.ClassName.Name.LowercaseValue)).ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.Name.Value));
                        if (currentClass.Fields.Values.Where(a => (a.IsStatic == true && a.ClassName == name && a.Name == field.Name)).Count() == 0)
                        {
                            var currentClassVariable = OutSet.GetControlVariable(FunctionResolver.staticVariables).ReadIndex(OutSet.Snapshot, new MemberIdentifier(name.Name.LowercaseValue)).ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.Name.Value));
                            currentClassVariable.SetAliases(OutSet.Snapshot, baseClassVariable);
                        }
                    }

                    foreach (var field in currentClass.Fields.Values.Where(a => (a.ClassName == currentClass.QualifiedName && a.IsStatic == true)))
                    {
                        var fieldIndex = staticVariables.ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.Name.Value));
                        fieldIndex.WriteMemory(OutSet.Snapshot, field.InitValue);
                    }
                }
            }
        }

        #endregion

    }
}
