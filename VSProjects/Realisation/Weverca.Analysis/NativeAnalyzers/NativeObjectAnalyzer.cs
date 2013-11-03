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

        public bool IsAbstract=false;

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
        public List<MethodArgument> ConvertArguments()
        {
            List<MethodArgument> res = new List<MethodArgument>();
            foreach (var arg in Arguments)
            { 
                res.Add(new MethodArgument(new VariableName(),arg.ByReference,arg.Optional));
            }

            return res;
        }

    }

    public class NativeObjectAnalyzer
    {
        private static NativeObjectAnalyzer instance = null;

        private Dictionary<QualifiedName, ClassDecl> nativeObjects;

        private Dictionary<string, MethodInfo> WevercaImplementedMethods
            = new Dictionary<string, MethodInfo>();

        private HashSet<string> fieldTypes = new HashSet<string>();
        private HashSet<string> methodTypes = new HashSet<string>();
        private HashSet<string> returnTypes = new HashSet<string>();

        #region parsing xml

        private NativeObjectAnalyzer(FlowController flow)
        {
            var reader = XmlReader.Create(new StreamReader("php_classes.xml"));
            nativeObjects = new Dictionary<QualifiedName, ClassDecl>();
            NativeObjectsAnalyzerHelper.mutableNativeObjects = new Dictionary<QualifiedName, ClassDeclBuilder>();

            var outSet = flow.OutSet;
            ClassDeclBuilder currentClass = null;
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
                                currentClass = new ClassDeclBuilder();
                                var classFinal = reader.GetAttribute("isFinal");
                                if (classFinal == "true")
                                {
                                    currentClass.IsFinal = true;
                                }
                                else
                                {
                                    currentClass.IsFinal = false;
                                }
                                currentClass.TypeName = new QualifiedName(new Name(reader.GetAttribute("name")));

                                if (reader.GetAttribute("baseClass") != null)
                                {
                                    currentClass.BaseClassName = new QualifiedName(new Name(reader.GetAttribute("baseClass")));
                                }

                                NativeObjectsAnalyzerHelper.mutableNativeObjects[currentClass.TypeName] = currentClass;
                                if (reader.Name == "class")
                                {
                                    currentClass.IsInterface = false;
                                }
                                else
                                {
                                    currentClass.IsInterface = true;
                                }
                                break;
                            case "field":
                                string fieldName = reader.GetAttribute("name");
                                string fieldVisibility = reader.GetAttribute("visibility");
                                string fieldIsStatic = reader.GetAttribute("isStatic");
                                string fieldIsConst = reader.GetAttribute("isConst");
                                string fieldType = reader.GetAttribute("type");
                                fieldTypes.Add(fieldType);
                                Visibility visibility;
                                switch (fieldVisibility)
                                {
                                    case "public":
                                        visibility = Visibility.PUBLIC;
                                        break;
                                    case "protectd":
                                        visibility = Visibility.PROTECTED;
                                        break;
                                    case "private":
                                        visibility = Visibility.PRIVATE;
                                        break;
                                    default:
                                        visibility = Visibility.PUBLIC;
                                        break;
                                }
                                
                                if (fieldIsConst == "false")
                                {
                                    
                                    Value initValue = outSet.UndefinedValue;
                                    string stringValue = reader.GetAttribute("value");
                                    int intValue;
                                    bool boolValue;
                                    long longValue;
                                    double doubleValue;

                                    if (stringValue == null)
                                    {
                                        // TODO: stringValue cannot be null, this is hotfix of error
                                        initValue = outSet.CreateString(string.Empty);
                                    }
                                    else if (bool.TryParse(stringValue, out boolValue))
                                    {
                                        initValue = outSet.CreateBool(boolValue);
                                    }
                                    else if (int.TryParse(stringValue, out intValue))
                                    {
                                        initValue = outSet.CreateInt(intValue);
                                    }
                                    else if (long.TryParse(stringValue, out longValue))
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

                                    currentClass.Fields[new FieldIdentifier(currentClass.TypeName, new VariableName(fieldName))] = new FieldInfo(new VariableName(fieldName), currentClass.TypeName, fieldType, visibility, new MemoryEntry(initValue), bool.Parse(fieldIsStatic));
                                }
                                else
                                {
                                    string value = reader.GetAttribute("value");
                                    //resolve constant
                                    VariableName constantName = new VariableName(fieldName);

                                    var constIdentifier = new FieldIdentifier(currentClass.TypeName, constantName);
                                    switch (fieldType)
                                    {
                                        case "int":
                                        case "integer":
                                            try
                                            {
                                                currentClass.Constants[constIdentifier] = new ConstantInfo(constantName, currentClass.TypeName, visibility, new MemoryEntry(outSet.CreateInt(int.Parse(value))));
                                            }
                                            catch (Exception)
                                            {
                                                currentClass.Constants[constIdentifier] = new ConstantInfo(constantName,currentClass.TypeName, visibility, new MemoryEntry(outSet.CreateDouble(double.Parse(value))));
                                            }
                                            break;
                                        case "string":
                                            currentClass.Constants[constIdentifier] = new ConstantInfo(constantName,currentClass.TypeName, visibility, new MemoryEntry(outSet.CreateString(value)));
                                            break;
                                        case "boolean":
                                            currentClass.Constants[constIdentifier] = new ConstantInfo(constantName,currentClass.TypeName, visibility, new MemoryEntry(outSet.CreateBool(bool.Parse(value))));
                                            break;
                                        case "float":
                                            currentClass.Constants[constIdentifier] = new ConstantInfo(constantName,currentClass.TypeName, visibility, new MemoryEntry(outSet.CreateDouble(double.Parse(value))));
                                            break;
                                        case "NULL":
                                            currentClass.Constants[constIdentifier] = new ConstantInfo(constantName,currentClass.TypeName, visibility, new MemoryEntry(outSet.UndefinedValue));
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
                                Visibility methodVisibility=Visibility.PUBLIC;
                                if (reader.GetAttribute("visibility") == "private")
                                {
                                    methodVisibility = Visibility.PRIVATE;
                                }
                                else if (reader.GetAttribute("visibility") == "protected")
                                {
                                    methodVisibility = Visibility.PROTECTED;
                                }
                                else 
                                {
                                    methodVisibility = Visibility.PUBLIC;
                                }


                                NativeAnalyzerMethod analyzer;
                                if (currentMethod.Name.Name.Equals(new Name("__construct")))
                                {
                                    analyzer = new NativeObjectsAnalyzerHelper(currentMethod, currentClass.TypeName).Construct;
                                }
                                else 
                                {
                                    analyzer = new NativeObjectsAnalyzerHelper(currentMethod, currentClass.TypeName).Analyze;
                                }
                                MethodInfo methodInfo = new MethodInfo(currentMethod.Name.Name, currentClass.TypeName, methodVisibility, analyzer, currentMethod.ConvertArguments(), currentMethod.IsFinal, currentMethod.IsStatic, currentMethod.IsAbstract);
                                currentClass.ModeledMethods[new MethodIdentifier(currentClass.TypeName, methodInfo.Name)]=methodInfo;
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
                if (nativeObject.TypeName != null)
                {
                    if (!NativeObjectsAnalyzerHelper.mutableNativeObjects.ContainsKey(nativeObject.TypeName))
                    {
                        throw new Exception();
                    }
                }
            }

            //generate result
            foreach (var nativeObject in NativeObjectsAnalyzerHelper.mutableNativeObjects.Values)
            {
                Nullable<QualifiedName> baseClassName=nativeObject.BaseClassName;
                if(baseClassName!=null)
                {
                    var baseClass = NativeObjectsAnalyzerHelper.mutableNativeObjects[baseClassName.Value];
                    foreach(var constant in baseClass.Constants)
                    {
                        nativeObject.Constants.Add(constant.Key, constant.Value);
                    }
                    foreach (var field in baseClass.Fields)
                    {
                        nativeObject.Fields.Add(field.Key, field.Value);
                    }
                    foreach (var method in baseClass.ModeledMethods)
                    {
                        nativeObject.ModeledMethods.Add(method.Key, method.Value);
                    }
                    baseClassName = baseClass.BaseClassName;

                }
                nativeObjects[nativeObject.TypeName] = nativeObject.Build();
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

        public ClassDecl GetClass(QualifiedName className)
        {
            return nativeObjects[className];
        }

        public bool TryGetClass(QualifiedName className, out ClassDecl declaration)
        {
            return nativeObjects.TryGetValue(className, out declaration);
        }
    }

    internal class NativeObjectsAnalyzerHelper
    {
        private NativeMethod Method;

        private QualifiedName ObjectName;

        public static Dictionary<QualifiedName, ClassDeclBuilder> mutableNativeObjects;

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

            var thisVariable=flow.OutSet.GetVariable(new VariableIdentifier("this"));
            var fieldsEntries = new List<MemoryEntry>();
            foreach (FieldInfo field in fields.Values)
            {
                var fieldEntry = thisVariable.ReadField(flow.OutSet.Snapshot, new VariableIdentifier(field.Name.Value));
                allFieldsEntries.Add(fieldEntry.ReadMemory(flow.OutSet.Snapshot));
                fieldsEntries.Add(fieldEntry.ReadMemory(flow.OutSet.Snapshot));
            }

            fieldsEntries.AddRange(arguments);
            foreach (FieldInfo field in fields.Values)
            {
                var fieldEntry = thisVariable.ReadField(flow.OutSet.Snapshot, new VariableIdentifier(field.Name.Value));
                MemoryEntry newfieldValues = NativeAnalyzerUtils.ResolveReturnValue(field.Type, flow);
                ValueInfoHandler.CopyFlags(flow.OutSet, fieldsEntries, newfieldValues);
                List<Value> addedValues;
                thisVariable.ReadField(flow.OutSet.Snapshot, new VariableIdentifier(field.Name.Value)).WriteMemory(flow.OutSet.Snapshot,addToEntry(fieldEntry.ReadMemory(flow.OutSet.Snapshot), newfieldValues.PossibleValues, out addedValues));
                
                if (addedValues.Count != 0)
                {
                    ValueInfoHandler.CopyFlags(flow.OutSet, fieldsEntries, new MemoryEntry(addedValues));
                }
            }


            allFieldsEntries.AddRange(arguments);
            ValueInfoHandler.CopyFlags(flow.OutSet, allFieldsEntries, functionResult);
            flow.OutSet.GetVariable(new VariableIdentifier(".return")).WriteMemory(flow.OutSet.Snapshot, functionResult);
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
            var thisVariable = flow.OutSet.GetVariable(new VariableIdentifier("this"));
                        
            foreach (FieldInfo field in fields.Values)
            {
                if (field.IsStatic == false)
                {
                    var fieldValues = NativeAnalyzerUtils.ResolveReturnValue(field.Type, flow);
                    createdFields.AddRange(fieldValues.PossibleValues);
                    thisVariable.ReadField(flow.OutSet.Snapshot, new VariableIdentifier(field.Name.Value));
                    thisVariable.WriteMemory(flow.OutSet.Snapshot, fieldValues);
                }
            }

            ValueInfoHandler.CopyFlags(flow.OutSet, getArguments(flow), new MemoryEntry(createdFields));
        }

        private List<MemoryEntry> getArguments(FlowController flow)
        {
            MemoryEntry argc = flow.InSet.ReadVariable(new VariableIdentifier(".argument_count")).ReadMemory(flow.OutSet.Snapshot);
            int argumentCount = ((IntegerValue)argc.PossibleValues.ElementAt(0)).Value;
            var arguments = new List<MemoryEntry>();

            for (int i = 0; i < argumentCount; i++)
            {
                arguments.Add(flow.OutSet.ReadVariable(NativeAnalyzerUtils.Argument(i)).ReadMemory(flow.OutSet.Snapshot));
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
