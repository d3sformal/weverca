using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;


using Weverca.Analysis;
using PHP.Core;
using Weverca.Analysis.Memory;

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
            this.Analyzer = null;
        }
    }
    
    //TODO informacie o objektoch kvoli implementacii is_subclass, ktora je potreba pri exceptions

    public class NativeFunctionAnalyzer
    {
        private Dictionary<QualifiedName, List<NativeFunction>> allNativeFunctions = new Dictionary<QualifiedName, List<NativeFunction>>();
        private Dictionary<QualifiedName, NativeAnalyzerMethod> phalangerImplementedFunctions = new Dictionary<QualifiedName, NativeAnalyzerMethod>();
        private Dictionary<QualifiedName, NativeAnalyzerMethod> wevercaImplementedFunctions = new Dictionary<QualifiedName, NativeAnalyzerMethod>();
        private HashSet<string> types = new HashSet<string>();

        private static NativeFunctionAnalyzer instance=null;
        
        private NativeFunctionAnalyzer()
        {
        }

        static public NativeFunctionAnalyzer CreateInstance()
        {
            if (instance != null)
            {
                return instance;
            }
            instance = new NativeFunctionAnalyzer();
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
                                instance.allNativeFunctions[functionName] = instance.allNativeFunctions[new QualifiedName(new Name(functionAlias))];
                            }
                            instance.types.Add(reader.GetAttribute("returnType"));
                        }
                        else if (reader.Name == "arg")
                        {
                            instance.types.Add(reader.GetAttribute("type"));
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
                            if (!instance.allNativeFunctions.ContainsKey(functionName))
                            {
                                instance.allNativeFunctions[functionName] = new List<NativeFunction>();
                            }
                            instance.allNativeFunctions[functionName].Add(new NativeFunction(functionName, returnType, arguments));
                            
                        }
                        break;
                }
            }
            var it = instance.types.GetEnumerator();
            while (it.MoveNext())
            {
                Console.WriteLine(it.Current);
               
            }
            return instance;
        }


        public bool existNativeFunction(QualifiedName name)
        {
            return allNativeFunctions.Keys.Contains(name);
        }
        public QualifiedName[] getNativeFunctions()
        {
            return allNativeFunctions.Keys.ToArray();
        }
        public NativeAnalyzerMethod getNativeAnalyzer(QualifiedName name)
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
            else if (existNativeFunction(name))
            {
                if (allNativeFunctions[name][0].Analyzer == null)
                {
                    AnalyzerClass analyzer = new AnalyzerClass(allNativeFunctions[name]);
                    allNativeFunctions[name][0].Analyzer = new NativeAnalyzerMethod(analyzer.analyze);
                }
                return allNativeFunctions[name][0].Analyzer;
                
            }
            //doesnt exist
            return null;
        }

        public static void Main(string[] args)
        {
            NativeFunctionAnalyzer.CreateInstance();
        }
    }

    class AnalyzerClass
    {
        List<NativeFunction> nativeFunctions;
        public AnalyzerClass(List<NativeFunction> nativeFunctions)
        {
            this.nativeFunctions = nativeFunctions;
        }

        public void analyze(FlowController flow)
        {
            //check number of arduments
            
            //int argumentNumber pocet argumentov
            //check types
            //return value
            var possibleValues = new List<Value>();
            possibleValues.Add(flow.OutSet.AnyValue);
            flow.OutSet.Assign(flow.OutSet.ReturnValue, new MemoryEntry(possibleValues.ToArray()));
        }
    }
}
