using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.Analysis.Properties;

namespace Weverca.Analysis.NativeAnalyzers
{

    /// <summary>
    /// Represents constant defined by Php
    /// </summary>
    class NativeConstant
    {
        /// <summary>
        /// Name of the constant, is case insensitive.
        /// </summary>
        public QualifiedName Name { private set; get; }
        /// <summary>
        /// Constant value
        /// </summary>
        public Value Value { private set; get; }

        /// <summary>
        /// Creates an instance of Native constant
        /// </summary>
        /// <param name="name">Name of the constant</param>
        /// <param name="value">Constant value</param>
        public NativeConstant(QualifiedName name, Value value)
        {
            Name = name;
            Value = value;
        }
    }


    /// <summary>
    /// Stores all php defined constatns and provides functionality do retrieve their values.
    /// This class is singleton.
    /// </summary>
    class NativeConstantAnalyzer
    {
        #region properties

        /// <summary>
        /// Stores singleton instance
        /// </summary>
        private static NativeConstantAnalyzer instance = null;
        /// <summary>
        /// Stores defined constants.
        /// </summary>
        private Dictionary<QualifiedName, NativeConstant> constants = new Dictionary<QualifiedName,NativeConstant>();
        
        #endregion

        #region methods
        /// <summary>
        /// Creates new instance of NativeConstantAnalyzer. Is private because this class is singleton.
        /// </summary>
        /// <param name="outset">FlowOutputSet</param>
        private NativeConstantAnalyzer(FlowOutputSet outset)
        {

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(Resources.php_constants)))
            using (XmlReader reader = XmlReader.Create(stream))
            {
                string value = "";
                string name = "";
                string type = "";
                HashSet<string> types = new HashSet<string>();
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name == "constant")
                            {
                                name = reader.GetAttribute("name");
                                value = reader.GetAttribute("value");

                            }
                            else if (reader.Name == "type")
                            {
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
                                QualifiedName qname = new QualifiedName(new Name(name));
                                Value constantValue = null;
                                switch (type)
                                {
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
                                constants.Add(qname, new NativeConstant(qname, constantValue));
                            }
                            break;
                    }
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

        /// <summary>
        /// Method used when parsing xml with constants.
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>true the value equals unknown</returns>
        private static bool isValueUnknown(string value)
        {
            return (value == "unknown");
        }

        /// <summary>
        /// Return the singleton instance of NativeConstantAnalyzer. If it doesn't exist, it will be created here.
        /// </summary>
        /// <param name="outset">FlowOutputSet used for creating constant values.</param>
        /// <returns>Instance</returns>
        public static NativeConstantAnalyzer Create(FlowOutputSet outset)
        {
            
            if(instance==null)
            {   
                instance=new NativeConstantAnalyzer(outset);
            }
            return instance;
        }

        /// <summary>
        /// Determing if the constant exist
        /// </summary>
        /// <param name="constant">Name of the constant.</param>
        /// <returns>True when the constant exists.</returns>
        public bool ExistContant(QualifiedName constant)
        {
            return constants.ContainsKey(constant);
        }

        /// <summary>
        /// Return the constant value.
        /// </summary>
        /// <param name="constant">Name of the constant.</param>
        /// <returns>the value of constant.</returns>
        public Value GetConstantValue(QualifiedName constant)
        {
            return constants[constant].Value;
        }
        #endregion
    }
}
