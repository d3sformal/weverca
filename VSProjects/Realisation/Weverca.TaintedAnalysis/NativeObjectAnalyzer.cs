using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using PHP.Core;
using Weverca.Analysis;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis
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
            this.Name = name;
            this.Arguments = arguments;
            this.ReturnType = returnType;
            this.Analyzer = null;
        }
    }


    public class MutableNativeTypeDecl
    {
        /// <summary>
        /// Name of native type
        /// </summary>
        public QualifiedName QualifiedName;

        /// <summary>
        /// Name of base class
        /// </summary>
        public QualifiedName? BaseClassName;

        public Dictionary<string, NativeFieldInfo> Fields;

        public Dictionary<string, Value> Constants;

        public List<NativeMethod> Methods;

        public bool IsInterFace;

        public bool IsFinal;

        public NativeTypeDecl ConvertToMuttable(NativeObjectAnalyzer analyzer, Dictionary<string, NativeMethodInfo> WevercaImplementedMethods)
        {
            List<NativeMethodInfo> nativeMethodsInfo = new List<NativeMethodInfo>();
            bool containsConstructor = false;
            HashSet<QualifiedName> allreadyDeclaredMethods = new HashSet<QualifiedName>();
            foreach (var method in Methods)
            {
                if (WevercaImplementedMethods.ContainsKey(QualifiedName + "." + method))
                {
                    nativeMethodsInfo.Add(WevercaImplementedMethods[QualifiedName + "." + method]);
                }
                else
                {
                    NativeObjectsAnalyzerHelper helper = new NativeObjectsAnalyzerHelper(method, QualifiedName);
                    if (method.Name.Name.Value.ToLower() == "__construct")
                    {
                        nativeMethodsInfo.Add(new NativeMethodInfo(method.Name.Name, helper.Construct, method.IsFinal, method.IsStatic));
                        containsConstructor = true;
                    }
                    else
                    {
                        nativeMethodsInfo.Add(new NativeMethodInfo(method.Name.Name, helper.Analyze, method.IsFinal, method.IsStatic));
                        allreadyDeclaredMethods.Add(method.Name);
                    }
                }
            }
            var baseclassName = BaseClassName;
            while (baseclassName != null)
            {
                MutableNativeTypeDecl baseClass = analyzer.mutableNativeObjects[baseclassName.Value];
                foreach (var method in baseClass.Methods)
                {
                    NativeObjectsAnalyzerHelper helper = new NativeObjectsAnalyzerHelper(method, QualifiedName);
                    if (method.Name.Name.Value.ToLower() == "__construct" && containsConstructor == false)
                    {
                        nativeMethodsInfo.Add(new NativeMethodInfo(method.Name.Name, helper.Construct, method.IsFinal, method.IsStatic));
                        containsConstructor = true;
                    }
                    else
                    {
                        var newMethod = new NativeMethodInfo(method.Name.Name, helper.Analyze, method.IsFinal, method.IsStatic);
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
            return new NativeTypeDecl(QualifiedName, nativeMethodsInfo, Constants, Fields, BaseClassName, IsFinal, IsInterFace);
        }
    }

    public class NativeObjectAnalyzer
    {
        private static NativeObjectAnalyzer instance = null;

        private Dictionary<QualifiedName, NativeTypeDecl> nativeObjects;

        public Dictionary<QualifiedName, MutableNativeTypeDecl> mutableNativeObjects;

        private Dictionary<string, NativeMethodInfo> WevercaImplementedMethods = new Dictionary<string, NativeMethodInfo>();

        private HashSet<string> fieldTypes = new HashSet<string>();
        private HashSet<string> methodTypes = new HashSet<string>();
        private HashSet<string> returnTypes = new HashSet<string>();

        /* private static bool once = false;
         private void checkTypes(FlowController flow)
         {
             once = true;
               Console.WriteLine("arg types");
                       Console.WriteLine();
                       var it = methodTypes.GetEnumerator();
                       while (it.MoveNext())
                       {
                           Console.WriteLine(it.Current);

                       }
                       Console.WriteLine();
              Console.WriteLine("field types");
             Console.WriteLine();
              var it = fieldTypes.GetEnumerator();
             while (it.MoveNext())
             {
                 Console.WriteLine(it.Current);

             }
             Console.WriteLine();
            

             Console.WriteLine("return types");
             Console.WriteLine();
             var it = returnTypes.GetEnumerator();
             while (it.MoveNext())
             {
                 //Console.WriteLine(it.Current);
                 if (NativeFunctionAnalyzer.getReturnValue(it.Current, flow) == null)
                 {
                     Console.WriteLine(it.Current.ToString());
                 }
             }
             Console.WriteLine("done");
         }*/


        public static NativeObjectAnalyzer GetInstance(FlowController flow)
        {
            if (instance == null)
            {
                instance = new NativeObjectAnalyzer(flow);
            }
            return instance;
        }
        private NativeObjectAnalyzer(FlowController flow)
        {

            XmlReader reader = XmlReader.Create(new StreamReader("php_classes.xml"));
            nativeObjects = new Dictionary<QualifiedName, NativeTypeDecl>();
            mutableNativeObjects = new Dictionary<QualifiedName, MutableNativeTypeDecl>();


            FlowOutputSet outSet = flow.OutSet;
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
                                string classFinal = reader.GetAttribute("isFinal");
                                if (classFinal == "true")
                                {
                                    currentClass.IsFinal = true;
                                }
                                else
                                {
                                    currentClass.IsFinal = false;
                                }
                                currentClass.QualifiedName = new QualifiedName(new Name(reader.GetAttribute("name")));
                                currentClass.Fields = new Dictionary<string, NativeFieldInfo>();
                                currentClass.Constants = new Dictionary<string, Value>();
                                currentClass.Methods = new List<NativeMethod>();
                                if (reader.GetAttribute("baseClass") != null)
                                {
                                    currentClass.BaseClassName = new QualifiedName(new Name(reader.GetAttribute("baseClass")));
                                }

                                mutableNativeObjects[currentClass.QualifiedName] = currentClass;
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
                                    currentClass.Fields[fieldName] = new NativeFieldInfo(new Name(fieldName), fieldType, visiblity, bool.Parse(fieldIsStatic));
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
                                                currentClass.Constants[fieldName] = outSet.CreateInt(int.Parse(value));
                                            }
                                            catch (Exception)
                                            {
                                                currentClass.Constants[fieldName] = outSet.CreateDouble(double.Parse(value));
                                            }
                                            break;
                                        case "string":
                                            currentClass.Constants[fieldName] = outSet.CreateString(value);
                                            break;
                                        case "boolean":
                                            currentClass.Constants[fieldName] = outSet.CreateBool(bool.Parse(value));
                                            break;
                                        case "float":
                                            currentClass.Constants[fieldName] = outSet.CreateDouble(double.Parse(value));
                                            break;
                                        case "NULL":
                                            currentClass.Constants[fieldName] = outSet.UndefinedValue;
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
            foreach (var nativeObject in mutableNativeObjects.Values)
            {
                if (nativeObject.QualifiedName != null)
                {
                    if (!mutableNativeObjects.ContainsKey(nativeObject.QualifiedName))
                    {
                        throw new Exception();
                    }
                }
            }

            //generate result
            foreach (var nativeObject in mutableNativeObjects.Values)
            {
                nativeObjects[nativeObject.QualifiedName] = nativeObject.ConvertToMuttable(this, WevercaImplementedMethods);
            }


        }

        public bool ExistClass(QualifiedName className)
        {
            return nativeObjects.ContainsKey(className);
        }

        public NativeTypeDecl GetClass(QualifiedName className)
        {
            return nativeObjects[className];
        }

    }

    class NativeObjectsAnalyzerHelper
    {
        private NativeMethod Method;

        private QualifiedName ObjectName;

        public NativeObjectsAnalyzerHelper(NativeMethod method, QualifiedName objectName)
        {
            Method = method;
            ObjectName = objectName;
        }

        public void Analyze(FlowController flow)
        {
            if (NativeFunctionAnalyzer.checkArgumentsCount(flow, Method))
            {
                NativeFunctionAnalyzer.checkArgumentTypes(flow,Method);
            }

            var nativeClass = NativeObjectAnalyzer.GetInstance(flow).GetClass(ObjectName);
            var fields = nativeClass.Fields;

            MemoryEntry functionResult = NativeFunctionAnalyzer.getReturnValue(Method.ReturnType, flow);
            List<MemoryEntry> arguments = getArguments(flow);
            List<MemoryEntry> allFieldsEntries = new List<MemoryEntry>();

            foreach (var value in flow.OutSet.ReadValue(new VariableName("this")).PossibleValues)
            {
                if (value is ObjectValue)
                {
                    List<MemoryEntry> fieldsEntries = new List<MemoryEntry>();
                    var obj = (value as ObjectValue);
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
                        MemoryEntry newfieldValues = NativeFunctionAnalyzer.getReturnValue(field.Type, flow);
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
            List<Value> assigned_aliases=NativeFunctionAnalyzer.ResolveAliasArguments(flow, (new NativeFunction[1] { Method }).ToList());
            ValueInfoHandler.CopyFlags(flow.OutSet, allFieldsEntries, new MemoryEntry(assigned_aliases));
        }

        public void Construct(FlowController flow)
        {
            if (NativeFunctionAnalyzer.checkArgumentsCount(flow,Method))
            {
                NativeFunctionAnalyzer.checkArgumentTypes(flow,Method );
            }

            initObject(flow);

        }


        public void initObject(FlowController flow)
        {
            var nativeClass = NativeObjectAnalyzer.GetInstance(flow).GetClass(ObjectName);
            var fields = nativeClass.Fields;

            List<Value> createdFields = new List<Value>();
            foreach (var value in flow.OutSet.ReadValue(new VariableName("this")).PossibleValues)
            {
                if (value is ObjectValue)
                {
                    var obj = (value as ObjectValue);
                    foreach (NativeFieldInfo field in fields.Values)
                    {
                        if (field.isStatic == false)
                        {
                            MemoryEntry fieldValues = NativeFunctionAnalyzer.getReturnValue(field.Type, flow);
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
            MemoryEntry argc = flow.InSet.ReadValue(new VariableName(".argument_count"));
            int argumentCount = ((IntegerValue)argc.PossibleValues.ElementAt(0)).Value;
            List<MemoryEntry> arguments = new List<MemoryEntry>();
            for (int i = 0; i < argumentCount; i++)
            {
                arguments.Add(flow.OutSet.ReadValue(NativeFunctionAnalyzer.argument(i)));
            }
            return arguments;
        }

        private MemoryEntry addToEntry(MemoryEntry entry, IEnumerable<Value> newValues, out List<Value> addedValues)
        {
            addedValues = new List<Value>();
            HashSet<Value> resList = new HashSet<Value>(entry.PossibleValues);
            foreach (var value in newValues)
            {
                if(resList.Contains(value))
                {
                    resList.Add(value);
                    addedValues.Add(value);
                }
            }
            return new MemoryEntry(resList);
        }
    }
}
