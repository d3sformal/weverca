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
        public NativeFunctionArgumentType type {get; private set;}
        public bool byReference { get; private set; }
        public bool optional { get; private set; }
        public NativeFunctionArgument(NativeFunctionArgumentType type,bool byReference,bool optional)
        {
            this.type=type;
            this.byReference=byReference;
            this.optional=optional;
        }
    }

    enum NativeFunctionArgumentType 
    { 
        boolean,numeric,str,resource,empty,array
    }


    class NativeFunction 
    {
        public NativeAnalyzerMethod analyzer { get; private set; }
        public QualifiedName name { get; private set; }
        public List<List<NativeFunctionArgument>> arguments { get; private set; }
        public NativeFunction(NativeAnalyzerMethod analyzer,QualifiedName name,List<List<NativeFunctionArgument>> arguments)
        {
            this.analyzer = analyzer;
            this.name=name;
            this.arguments=arguments;
        }
    }

    class NativeFunctionAnalyzer
    {
        Dictionary<QualifiedName, NativeFunction> allNativeFunctions = new Dictionary<QualifiedName, NativeFunction>();
        Dictionary<QualifiedName, NativeAnalyzerMethod> phalangerImplementedFunctions = new Dictionary<QualifiedName, NativeAnalyzerMethod>();
        Dictionary<QualifiedName, NativeAnalyzerMethod> wevercaImplementedFunctions = new Dictionary<QualifiedName, NativeAnalyzerMethod>();
        HashSet<string> types = new HashSet<string>();
        internal NativeFunctionAnalyzer()
        {
            XmlReader reader = XmlReader.Create(new StreamReader("../../php_functions.xml"));

            bool inFunction = false;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "function") 
                        {
                        //    Console.WriteLine( reader.GetAttribute(0));
                            inFunction = true;
                        }
                        if (inFunction && reader.Name == "type")
                        {
                            types.Add(reader.GetAttribute(0));
                            inFunction = true;
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
                            inFunction = false;
                        }
                        break;
                }
            }
            var it=types.GetEnumerator();
           do
            {
                Console.WriteLine(it.Current);
               
            }while( it.MoveNext());

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
                    return allNativeFunctions[name].analyzer;
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
