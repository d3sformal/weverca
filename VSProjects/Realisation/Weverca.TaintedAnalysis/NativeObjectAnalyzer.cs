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



        public bool IsFinal;

        public NativeTypeDecl ConvertToMuttable(NativeObjectAnalyzer analyzer, Dictionary<string, NativeMethodInfo> WevercaImplementedMethods)
        {
            List<NativeMethodInfo> nativeMethodsInfo = new List<NativeMethodInfo>();
            bool containsConstructor = false;
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
                    }
                }
            }
            /* if (containsConstructor == false)
                 Console.WriteLine(QualifiedName);
            */
            return new NativeTypeDecl(QualifiedName, nativeMethodsInfo, Constants, Fields, BaseClassName, IsFinal);
        }
    }

    public class NativeObjectAnalyzer
    {
        private static NativeObjectAnalyzer instance = null;

        private Dictionary<QualifiedName, NativeTypeDecl> nativeObjects;

        private Dictionary<QualifiedName, MutableNativeTypeDecl> mutableNativeObjects;

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
                                if (reader.GetAttribute("extends") != null)
                                {
                                    currentClass.BaseClassName = new QualifiedName(new Name(reader.GetAttribute("extends")));
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
                        switch (reader.Name)
                        {
                            case "class":
                                mutableNativeObjects[currentClass.QualifiedName] = currentClass;
                                break;
                        }
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
            if (NativeFunctionAnalyzer.checkArgumentsCount((new NativeFunction[1] { Method }).ToList(), flow))
            {
                NativeFunctionAnalyzer.checkArgumentTypes((new NativeFunction[1] { Method }).ToList(), flow);
            }

            //return result
            MemoryEntry functionResult = NativeFunctionAnalyzer.getReturnValue(Method.ReturnType,flow);
            flow.OutSet.Assign(flow.OutSet.ReturnValue, functionResult);
        }

        public void Construct(FlowController flow)
        {
            if (NativeFunctionAnalyzer.checkArgumentsCount((new NativeFunction[1] { Method }).ToList(), flow))
            {
                NativeFunctionAnalyzer.checkArgumentTypes((new NativeFunction[1] { Method }).ToList(), flow);
            }

            initObject(flow);

        }

        public void initObject(FlowController flow)
        {
            var  nativeClass=NativeObjectAnalyzer.GetInstance(flow).GetClass(ObjectName);
            var fields = nativeClass.Fields;
            foreach (var value in flow.OutSet.ReadValue(new VariableName("this")).PossibleValues)
            {
                if (value is ObjectValue)
                {
                    var obj = (value as ObjectValue);
                    foreach (NativeFieldInfo field in fields.Values)
                    {
                        if (field.isStatic == false)
                        {
                            flow.OutSet.SetField(obj, flow.OutSet.CreateIndex(field.Name.Value), (NativeFunctionAnalyzer.getReturnValue(field.Type, flow)));
                        }
                    }
                }
            }
        }
    }
}
