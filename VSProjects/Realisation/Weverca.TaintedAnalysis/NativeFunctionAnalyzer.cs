using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;


using Weverca.Analysis;
using PHP.Core;

namespace Weverca.TaintedAnalysis
{

    

    class NativeFunctionArgument
    {
        public string Type { get; private set; }
        public bool byReference { get; private set; }
        public bool Optional { get; private set; }
        public bool dots { get; private set; }
        public NativeFunctionArgument(string type, bool byReference, bool optional,bool dots)
        {
            this.Type=type;
            this.byReference=byReference;
            this.Optional=optional;
        }
    }

    class NativeFunction 
    {
        public NativeAnalyzerMethod Analyzer { get; set; }
        public QualifiedName Name { get; private set; }
        public List<NativeFunctionArgument> Arguments { get; private set; }
        public string ReturnType { get; private set; }
        public NativeFunction( QualifiedName name, string returnType, List<NativeFunctionArgument> arguments)
        {
            this.Name = name;
            this.Arguments = arguments;
            this.ReturnType = returnType;
        }
    }

    class NativeFunctionAnalyzer
    {
        private Dictionary<QualifiedName, List<NativeFunction>> allNativeFunctions = new Dictionary<QualifiedName, List<NativeFunction>>();
        private Dictionary<QualifiedName, NativeAnalyzerMethod> phalangerImplementedFunctions = new Dictionary<QualifiedName, NativeAnalyzerMethod>();
        private Dictionary<QualifiedName, NativeAnalyzerMethod> wevercaImplementedFunctions = new Dictionary<QualifiedName, NativeAnalyzerMethod>();
        private HashSet<string> types = new HashSet<string>();
        internal NativeFunctionAnalyzer()
        {
            string function = "";
            string returnType = "";
            string functionAlias = "";
            List<NativeFunctionArgument> arguments = new List<NativeFunctionArgument>();
            XmlReader reader = XmlReader.Create(new StreamReader("php_functions.xml"));

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "function") 
                        {
                            arguments=new List<NativeFunctionArgument>();
                            function = reader.GetAttribute("name") ;
                            returnType = reader.GetAttribute("returnType");
                            functionAlias = reader.GetAttribute("alias");
                            QualifiedName functionName = new QualifiedName(new Name(function));
                            if (functionAlias != null)
                            {
                                allNativeFunctions[functionName] = allNativeFunctions[new QualifiedName(new Name(functionAlias))];
                            }
                            types.Add(reader.GetAttribute("returnType"));
                        }
                        else if (reader.Name == "arg")
                        {
                            types.Add(reader.GetAttribute("type"));
                            bool optional=false;
                            bool byReference=false;
                            bool dots = false;
                            if(reader.GetAttribute("optional")=="true")
                            {
                                optional=true;
                            }
                            if(reader.GetAttribute("byReference")=="true")
                            {
                                byReference=true;
                            }
                            if(reader.GetAttribute("dots")=="true")
                            {
                                dots=true;
                            }
                            NativeFunctionArgument argument = new NativeFunctionArgument(reader.GetAttribute("type"), optional, byReference, dots);
                            arguments.Add(argument);
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
                        if (reader.Name == "function")
                        {
                            QualifiedName functionName = new QualifiedName(new Name(function));
                            if (!allNativeFunctions.ContainsKey(functionName))
                            {
                                allNativeFunctions[functionName] = new List<NativeFunction>();
                            }
                            allNativeFunctions[functionName].Add(new NativeFunction(functionName, returnType, arguments));
                            
                        }
                        break;
                }
            }
            var it=types.GetEnumerator();
            while (it.MoveNext())
            {
                Console.WriteLine(it.Current);
               
            }

        }


        bool existNativeFunction(QualifiedName name)
        {
            return allNativeFunctions.Keys.Contains(name);
        }
        QualifiedName[] getNativeFunctions()
        {
            return allNativeFunctions.Keys.ToArray();
        }
        NativeAnalyzerMethod getNativeAnalyzer(QualifiedName name)
        {
            if(!existNativeFunction(name))
            {
                return null;
            }
            
            if (wevercaImplementedFunctions.Keys.Contains(name))
            {
                return wevercaImplementedFunctions[name];
            }
            else if (phalangerImplementedFunctions.Keys.Contains(name))
            {
                return phalangerImplementedFunctions[name];
            }
            else{
                if (allNativeFunctions[name] == null)
                {
                    //create and return analyzer
                }
                else 
                {
                    return allNativeFunctions[name][0].Analyzer;
                }
            }
            return null;
        }
        public static void Main(string[] args)
        {
            new NativeFunctionAnalyzer();
        }
    }    
    
}
