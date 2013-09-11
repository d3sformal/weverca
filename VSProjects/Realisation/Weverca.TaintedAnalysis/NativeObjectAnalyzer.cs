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
        
        public IEnumerable<NativeMethodInfo> Methods;

        public bool IsFinal;
    }

    class NativeObjectAnalyzer
    {
        private static NativeObjectAnalyzer instance=null;

        public Dictionary<QualifiedName,NativeTypeDecl> nativeObjects;

        private Dictionary<QualifiedName, MutableNativeTypeDecl> mutableNativeObjects;

        public static NativeObjectAnalyzer GetInstance()
        {
            if (instance == null)
            {
                instance = new NativeObjectAnalyzer();
            }
            return instance;
        }
        private NativeObjectAnalyzer()
        {
            XmlReader reader = XmlReader.Create(new StreamReader("php_classes.xml"));
            nativeObjects = new Dictionary<QualifiedName, NativeTypeDecl>();
            mutableNativeObjects = new Dictionary<QualifiedName, MutableNativeTypeDecl>();

            MutableNativeTypeDecl currentClass=null;

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
                                currentClass.Methods = new List<NativeMethodInfo>();
                                if (reader.GetAttribute("extends")!=null)
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
                                if (fieldIsConst == "false")
                                {
                                    bool isStatic = fieldIsStatic == "true" ? true : false;
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

                                    currentClass.Fields[fieldName] = new NativeFieldInfo(new Name(fieldName), fieldType, visiblity, isStatic);
                                }
                                else 
                                {
                                    //resolve constant
                                    currentClass.Constants[fieldName] = null;
                                }
                                break;
                            case "method":
                                break;
                            case "arg":
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
                            case "method":
                                break;
                        }
                        break;
                }
            }
        }
    }
}
