using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using PHP.Core;
using PHP.Core.AST;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis
{
    public class NativeMethod : NativeFunction
    {
        public bool IsStatic;

        public bool IsFinal;

        public NativeMethod()
        {
        }

        public NativeMethod(QualifiedName name, string returnType, List<NativeFunctionArgument> arguments)
        {
            Name = name;
            Arguments = arguments;
            ReturnType = returnType;
            Analyzer = null;
        }
    }

    public class MutableNativeTypeDecl
    {
        #region properties
        /// <summary>
        /// Name of native type
        /// </summary>
        public QualifiedName QualifiedName;

        /// <summary>
        /// Name of base class
        /// </summary>
        public QualifiedName? BaseClassName;

        public Dictionary<VariableName, NativeFieldInfo> Fields;

        public Dictionary<VariableName, MemoryEntry> Constants;

        public List<NativeMethod> Methods;

        public bool IsInterFace;

        public bool IsFinal;

        #endregion

        #region convert to mutable

        public NativeTypeDecl ConvertToMuttable(NativeObjectAnalyzer analyzer,
            Dictionary<string, NativeMethodInfo> WevercaImplementedMethods)
        {
            var nativeMethodsInfo = new List<NativeMethodInfo>();
            bool containsConstructor = false;
            var allreadyDeclaredMethods = new HashSet<QualifiedName>();

            foreach (var method in Methods)
            {
                if (WevercaImplementedMethods.ContainsKey(QualifiedName + "." + method))
                {
                    nativeMethodsInfo.Add(WevercaImplementedMethods[QualifiedName + "." + method]);
                }
                else
                {
                    var helper = new NativeObjectsAnalyzerHelper(method, QualifiedName);
                    if (method.Name.Name.Value.ToLower() == "__construct")
                    {
                        nativeMethodsInfo.Add(new NativeMethodInfo(method.Name.Name,
                            helper.Construct, method.IsFinal, method.IsStatic));
                        containsConstructor = true;
                    }
                    else
                    {
                        nativeMethodsInfo.Add(new NativeMethodInfo(method.Name.Name,
                            helper.Analyze, method.IsFinal, method.IsStatic));
                        allreadyDeclaredMethods.Add(method.Name);
                    }
                }
            }

            var baseclassName = BaseClassName;
            while (baseclassName != null)
            {
                var baseClass = NativeObjectsAnalyzerHelper.mutableNativeObjects[baseclassName.Value];
                foreach (var method in baseClass.Methods)
                {
                    var helper = new NativeObjectsAnalyzerHelper(method, QualifiedName);
                    if (method.Name.Name.Value.ToLower() == "__construct" && containsConstructor == false)
                    {
                        var methodInfo = new NativeMethodInfo(method.Name.Name, helper.Construct,
                            method.IsFinal, method.IsStatic);
                        nativeMethodsInfo.Add(methodInfo);
                        containsConstructor = true;
                    }
                    else
                    {
                        var newMethod = new NativeMethodInfo(method.Name.Name,
                            helper.Analyze, method.IsFinal, method.IsStatic);
                        if (!allreadyDeclaredMethods.Contains(method.Name))
                        {
                            nativeMethodsInfo.Add(newMethod);
                            allreadyDeclaredMethods.Add(method.Name);
                        }
                    }
                }

                foreach (var field in baseClass.Fields)
                {
                    if (!Fields.ContainsKey(field.Key))
                    {
                        Fields.Add(field.Key, field.Value);
                    }
                }

                foreach (var constant in baseClass.Constants)
                {
                    if (!Constants.ContainsKey(constant.Key))
                    {
                        Constants.Add(constant.Key, constant.Value);
                    }
                }

                baseclassName = baseClass.BaseClassName;
            }

            /* if (containsConstructor == false)
                 Console.WriteLine(QualifiedName);
            */

            return new NativeTypeDecl(QualifiedName, nativeMethodsInfo,new List<MethodDecl>(), Constants,Fields, BaseClassName, IsFinal, IsInterFace);
        }

        #endregion
    }

    public class NativeObjectAnalyzer
    {
        private static NativeObjectAnalyzer instance = null;

        private Dictionary<QualifiedName, NativeTypeDecl> nativeObjects;

        private Dictionary<string, NativeMethodInfo> WevercaImplementedMethods
            = new Dictionary<string, NativeMethodInfo>();

        private HashSet<string> fieldTypes = new HashSet<string>();
        private HashSet<string> methodTypes = new HashSet<string>();
        private HashSet<string> returnTypes = new HashSet<string>();

        #region parsing xml

        private NativeObjectAnalyzer(FlowController flow)
        {
            var reader = XmlReader.Create(new StreamReader("php_classes.xml"));
            nativeObjects = new Dictionary<QualifiedName, NativeTypeDecl>();
            NativeObjectsAnalyzerHelper.mutableNativeObjects = new Dictionary<QualifiedName, MutableNativeTypeDecl>();

            var outSet = flow.OutSet;
            MutableNativeTypeDecl currentClass = null;
            NativeMethod currentMethod = null;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "class":
                            case "interface":
                                currentClass = new MutableNativeTypeDecl();
                                var classFinal = reader.GetAttribute("isFinal");
                                if (classFinal == "true")
                                {
                                    currentClass.IsFinal = true;
                                }
                                else
                                {
                                    currentClass.IsFinal = false;
                                }
                                currentClass.QualifiedName = new QualifiedName(new Name(reader.GetAttribute("name")));
                                currentClass.Fields = new Dictionary<VariableName, NativeFieldInfo>();
                                currentClass.Constants = new Dictionary<VariableName, MemoryEntry>();
                                currentClass.Methods = new List<NativeMethod>();
                                if (reader.GetAttribute("baseClass") != null)
                                {
                                    currentClass.BaseClassName = new QualifiedName(new Name(reader.GetAttribute("baseClass")));
                                }

                                NativeObjectsAnalyzerHelper.mutableNativeObjects[currentClass.QualifiedName] = currentClass;
                                if (reader.Name == "class")
                                {
                                    currentClass.IsInterFace = false;
                                }
                                else
                                {
                                    currentClass.IsInterFace = true;
                                }
                                break;
                            case "field":
                                string fieldName = reader.GetAttribute("name");
                                string fieldVisibility = reader.GetAttribute("visibility");
                                string fieldIsStatic = reader.GetAttribute("isStatic");
                                string fieldIsConst = reader.GetAttribute("isConst");
                                string fieldType = reader.GetAttribute("type");
                                fieldTypes.Add(fieldType);
                                if (fieldIsConst == "false")
                                {
                                    Visibility visiblity;
                                    switch (fieldVisibility)
                                    {
                                        case "public":
                                            visiblity = Visibility.PUBLIC;
                                            break;
                                        case "protectd":
                                            visiblity = Visibility.PROTECTED;
                                            break;
                                        case "private":
                                            visiblity = Visibility.PRIVATE;
                                            break;
                                        default:
                                            visiblity = Visibility.PUBLIC;
                                            break;
                                    }
                                    Value initValue=outSet.UndefinedValue;
                                    string stringValue = reader.GetAttribute("value");
                                    int intValue;
                                    bool boolValue;
                                    long longValue;
                                    double doubleValue;

                                    if (bool.TryParse(stringValue, out boolValue))
                                    {
                                        initValue=outSet.CreateBool(boolValue);
                                    }
                                    else if(int.TryParse(stringValue, out intValue))
                                    {
                                        initValue = outSet.CreateInt(intValue);
                                    }
                                    else if (long.TryParse(stringValue,out longValue))
                                    {
                                        initValue = outSet.CreateLong(longValue);
                                    }
                                    else if (double.TryParse(stringValue, out doubleValue))
                                    {
                                        initValue = outSet.CreateDouble(doubleValue);
                                    }
                                    else 
                                    {
                                        initValue = outSet.CreateString(stringValue);
                                    }
                                    
                                    currentClass.Fields[new VariableName(fieldName)] = new NativeFieldInfo(new VariableName(fieldName), fieldType, visiblity, new MemoryEntry(initValue), bool.Parse(fieldIsStatic));
                                }
                                else
                                {
                                    string value = reader.GetAttribute("value");
                                    //resolve constant
                                    switch (fieldType)
                                    {
                                        case "int":
                                        case "integer":
                                            try
                                            {
                                                currentClass.Constants[new VariableName(fieldName)] = new MemoryEntry(outSet.CreateInt(int.Parse(value)));
                                            }
                                            catch (Exception)
                                            {
                                                currentClass.Constants[new VariableName(fieldName)] = new MemoryEntry(outSet.CreateDouble(double.Parse(value)));
                                            }
                                            break;
                                        case "string":
                                            currentClass.Constants[new VariableName(fieldName)] = new MemoryEntry(outSet.CreateString(value));
                                            break;
                                        case "boolean":
                                            currentClass.Constants[new VariableName(fieldName)] = new MemoryEntry(outSet.CreateBool(bool.Parse(value)));
                                            break;
                                        case "float":
                                            currentClass.Constants[new VariableName(fieldName)] = new MemoryEntry(outSet.CreateDouble(double.Parse(value)));
                                            break;
                                        case "NULL":
                                            currentClass.Constants[new VariableName(fieldName)] = new MemoryEntry(outSet.UndefinedValue);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                break;
                            case "method":
                                currentMethod = new NativeMethod(new QualifiedName(new Name(reader.GetAttribute("name"))), reader.GetAttribute("returnType"), new List<NativeFunctionArgument>());
                                currentMethod.IsFinal = bool.Parse(reader.GetAttribute("final"));
                                currentMethod.IsStatic = bool.Parse(reader.GetAttribute("static"));
                                returnTypes.Add(reader.GetAttribute("returnType"));
                                currentClass.Methods.Add(currentMethod);
                                break;
                            case "arg":
                                currentMethod.Arguments.Add(new NativeFunctionArgument(reader.GetAttribute("type"), bool.Parse(reader.GetAttribute("optional")), bool.Parse(reader.GetAttribute("byReference")), bool.Parse(reader.GetAttribute("dots"))));
                                methodTypes.Add(reader.GetAttribute("type"));
                                break;
                        }

                        break;
                    case XmlNodeType.Text:
                        break;
                    case XmlNodeType.XmlDeclaration:
                    case XmlNodeType.ProcessingInstruction:
                        break;
                    case XmlNodeType.Comment:
                        break;
                    case XmlNodeType.EndElement:
                        break;
                }
            }
            /*
            check if every ancestor exist 
             */
            foreach (var nativeObject in NativeObjectsAnalyzerHelper.mutableNativeObjects.Values)
            {
                if (nativeObject.QualifiedName != null)
                {
                    if (!NativeObjectsAnalyzerHelper.mutableNativeObjects.ContainsKey(nativeObject.QualifiedName))
                    {
                        throw new Exception();
                    }
                }
            }

            //generate result
            foreach (var nativeObject in NativeObjectsAnalyzerHelper.mutableNativeObjects.Values)
            {
                nativeObjects[nativeObject.QualifiedName] = nativeObject.ConvertToMuttable(this, WevercaImplementedMethods);
            }
        }

        #endregion

        public static NativeObjectAnalyzer GetInstance(FlowController flow)
        {
            if (instance == null)
            {
                instance = new NativeObjectAnalyzer(flow);
            }

            return instance;
        }

        public bool ExistClass(QualifiedName className)
        {
            return nativeObjects.ContainsKey(className);
        }

        public NativeTypeDecl GetClass(QualifiedName className)
        {
            return nativeObjects[className];
        }

        public bool TryGetClass(QualifiedName className, out NativeTypeDecl declaration)
        {
            return nativeObjects.TryGetValue(className, out declaration);
        }
    }

    internal class NativeObjectsAnalyzerHelper
    {
        private NativeMethod Method;

        private QualifiedName ObjectName;

        public static Dictionary<QualifiedName, MutableNativeTypeDecl> mutableNativeObjects;

        public NativeObjectsAnalyzerHelper(NativeMethod method, QualifiedName objectName)
        {
            Method = method;
            ObjectName = objectName;
        }

        public void Analyze(FlowController flow)
        {
            if (NativeAnalyzerUtils.checkArgumentsCount(flow, Method))
            {
                NativeAnalyzerUtils.checkArgumentTypes(flow, Method);
            }

            var nativeClass = NativeObjectAnalyzer.GetInstance(flow).GetClass(ObjectName);
            var fields = nativeClass.Fields;

            var functionResult = NativeAnalyzerUtils.ResolveReturnValue(Method.ReturnType, flow);
            var arguments = getArguments(flow);
            var allFieldsEntries = new List<MemoryEntry>();

            foreach (var value in flow.OutSet.ReadValue(new VariableName("this")).PossibleValues)
            {
                if (value is ObjectValue)
                {
                    var fieldsEntries = new List<MemoryEntry>();
                    var obj = value as ObjectValue;
                    foreach (NativeFieldInfo field in fields.Values)
                    {
                        var fieldEntry = flow.OutSet.GetField(obj, flow.OutSet.CreateIndex(field.Name.Value));
                        allFieldsEntries.Add(fieldEntry);
                        fieldsEntries.Add(fieldEntry);
                    }

                    fieldsEntries.Add(new MemoryEntry(value));
                    fieldsEntries.AddRange(arguments);
                    foreach (NativeFieldInfo field in fields.Values)
                    {
                        var fieldEntry = flow.OutSet.GetField(obj, flow.OutSet.CreateIndex(field.Name.Value));
                        MemoryEntry newfieldValues = NativeAnalyzerUtils.ResolveReturnValue(field.Type, flow);
                        ValueInfoHandler.CopyFlags(flow.OutSet, fieldsEntries, newfieldValues);
                        List<Value> addedValues;
                        flow.OutSet.SetField(obj, flow.OutSet.CreateIndex(field.Name.Value), addToEntry(fieldEntry, newfieldValues.PossibleValues, out addedValues));
                        if (addedValues.Count != 0)
                        {
                            ValueInfoHandler.CopyFlags(flow.OutSet, fieldsEntries, new MemoryEntry(addedValues));
                        }
                    }

                    ValueInfoHandler.CopyFlags(flow.OutSet, fieldsEntries, value);
                }
                else if (value is AnyObjectValue)
                {
                    ValueInfoHandler.CopyFlags(flow.OutSet, arguments, value);
                }
            }

            allFieldsEntries.AddRange(arguments);
            ValueInfoHandler.CopyFlags(flow.OutSet, allFieldsEntries, functionResult);
            flow.OutSet.Assign(flow.OutSet.ReturnValue, functionResult);
            var assigned_aliases = NativeAnalyzerUtils.ResolveAliasArguments(flow, (new NativeFunction[1] { Method }).ToList());
            ValueInfoHandler.CopyFlags(flow.OutSet, allFieldsEntries, new MemoryEntry(assigned_aliases));
        }

        public void Construct(FlowController flow)
        {
            if (NativeAnalyzerUtils.checkArgumentsCount(flow, Method))
            {
                NativeAnalyzerUtils.checkArgumentTypes(flow, Method);
            }

            initObject(flow);
        }

        public void initObject(FlowController flow)
        {
            var nativeClass = NativeObjectAnalyzer.GetInstance(flow).GetClass(ObjectName);
            var fields = nativeClass.Fields;

            var createdFields = new List<Value>();
            foreach (var value in flow.OutSet.ReadValue(new VariableName("this")).PossibleValues)
            {
                if (value is ObjectValue)
                {
                    var obj = value as ObjectValue;
                    foreach (NativeFieldInfo field in fields.Values)
                    {
                        if (field.IsStatic == false)
                        {
                            var fieldValues = NativeAnalyzerUtils.ResolveReturnValue(field.Type, flow);
                            createdFields.AddRange(fieldValues.PossibleValues);
                            flow.OutSet.SetField(obj, flow.OutSet.CreateIndex(field.Name.Value), fieldValues);
                        }
                    }

                    createdFields.Add(value);
                }
                else if (value is AnyObjectValue)
                {
                    ValueInfoHandler.CopyFlags(flow.OutSet, getArguments(flow), value);
                }
            }

            ValueInfoHandler.CopyFlags(flow.OutSet, getArguments(flow), new MemoryEntry(createdFields));
        }

        private List<MemoryEntry> getArguments(FlowController flow)
        {
            var argc = flow.InSet.ReadValue(new VariableName(".argument_count"));
            var argumentCount = ((IntegerValue)argc.PossibleValues.ElementAt(0)).Value;
            var arguments = new List<MemoryEntry>();

            for (int i = 0; i < argumentCount; i++)
            {
                arguments.Add(flow.OutSet.ReadValue(NativeAnalyzerUtils.Argument(i)));
            }

            return arguments;
        }

        private MemoryEntry addToEntry(MemoryEntry entry, IEnumerable<Value> newValues, out List<Value> addedValues)
        {
            addedValues = new List<Value>();
            var resList = new HashSet<Value>(entry.PossibleValues);
            foreach (var value in newValues)
            {
                if (resList.Contains(value))
                {
                    resList.Add(value);
                    addedValues.Add(value);
                }
            }

            return new MemoryEntry(resList);
        }
    }
}
