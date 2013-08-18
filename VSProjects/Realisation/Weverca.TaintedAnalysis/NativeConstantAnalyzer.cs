using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Analysis.Memory;
using PHP.Core.AST;
using PHP.Core;
using System.Xml;
using System.IO;
using Weverca.Analysis;
namespace Weverca.TaintedAnalysis
{
    class NativeConstant
    {
        public QualifiedName Name { private set; get; }
        public Value Value { private set; get; }

        public NativeConstant(QualifiedName name, Value value)
        {
            Name = name;
            Value = value;
        }
    }


    class NativeConstantAnalyzer
    {
        private static NativeConstantAnalyzer instance = null;
        private Dictionary<QualifiedName, NativeConstant> constants = new Dictionary<QualifiedName,NativeConstant>();
        private FlowOutputSet outset;
        private NativeConstantAnalyzer(FlowOutputSet outset)
        {
            this.outset = outset;
            XmlReader reader = XmlReader.Create(new StreamReader("php_constants.xml"));
            
            string value = "";
            string name = "";
            string type = "";
            HashSet<string> types = new HashSet<string>();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "constant") {
                            name = reader.GetAttribute("name");
                            value = reader.GetAttribute("value");
                            
                        }
                        else if (reader.Name == "type") {
                            type = reader.GetAttribute("name");
                            types.Add(type);
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
                        if (reader.Name == "constant")
                        {
                            QualifiedName qname=new QualifiedName(new Name(name));
                            Value constantValue=null;
                            switch(type){
                                case "boolean":
                                    if (isValueUnknown(value))
                                    {
                                        constantValue = outset.AnyBooleanValue;
                                    }
                                    else
                                    {
                                        if (value == "1")
                                        {
                                            constantValue = outset.CreateBool(true);
                                        }
                                        else
                                        {
                                            constantValue = outset.CreateBool(false);
                                        }
                                    }
                                    break;
                                case "integer":
                                    if (isValueUnknown(value))
                                    {
                                        constantValue = outset.AnyIntegerValue;
                                    }
                                    else
                                    {
                                        constantValue = outset.CreateInt(int.Parse(value));
                                    }
                                    break;
                                case "string":
                                    if (isValueUnknown(value))
                                    {
                                        constantValue = outset.AnyStringValue;
                                    }
                                    else
                                    {
                                        constantValue = outset.CreateString(value);
                                    }
                                    break;
                                case "NULL":
                                    constantValue = outset.UndefinedValue;
                                    break;
                                case "resource":
                                    constantValue = outset.AnyResourceValue;
                                    break;
                                case "double":
                                    if (isValueUnknown(value))
                                    {
                                        constantValue = outset.AnyFloatValue;
                                    }
                                    else
                                    {
                                        switch (value)
                                        {
                                            case "NAN":
                                                constantValue = outset.CreateDouble(double.NaN);
                                                break;
                                            case "INF":
                                                constantValue = outset.CreateDouble(double.PositiveInfinity);
                                                break;
                                            default:
                                                constantValue = outset.CreateDouble(double.Parse(value));
                                                break;
                                        }
                                    }
                                    break;
                                default:
                                    constantValue = outset.AnyValue;
                                    break;
                            }
                            constants.Add(qname,new NativeConstant(qname,constantValue));
                        }
                        break;
                }
            }
            /*
            var it = types.GetEnumerator();
            while (it.MoveNext())
            {
                Console.WriteLine(it.Current);

            }
            */
        }

        private static bool isValueUnknown(string value)
        {
            return (value == "unknown");
        }

        public static NativeConstantAnalyzer Create(FlowOutputSet outset)
        {
            
            if(instance==null)
            {   
                instance=new NativeConstantAnalyzer(outset);
            }
            return instance;
        }

        public bool ExistContant(QualifiedName constant)
        {
            return constants.ContainsKey(constant);
        }

        public Value GetConstantValue(QualifiedName constant)
        {
            return constants[constant].Value;
        }

    }
}
