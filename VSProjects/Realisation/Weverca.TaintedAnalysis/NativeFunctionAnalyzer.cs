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
using Weverca.Parsers;

using PHP.Core.Parsers;

namespace Weverca.TaintedAnalysis
{



    public class NativeFunctionArgument
    {
        public string Type { get; private set; }
        public bool ByReference { get; private set; }
        public bool Optional { get; private set; }
        public bool Dots { get; private set; }
        public NativeFunctionArgument(string type,  bool optional,bool byReference,bool dots)
        {
            this.Type=type;
            this.ByReference=byReference;
            this.Optional=optional;
            this.Dots = dots;
        }
    }

    public class NativeFunction 
    {
        public NativeAnalyzerMethod Analyzer { get; set; }
        public QualifiedName Name { get; private set; }
        public List<NativeFunctionArgument> Arguments { get; private set; }
        public string ReturnType { get; private set; }
        public int MinArgumentCount = -1;
        public int MaxArgumentCount = -1;
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
            //    Console.WriteLine(it.Current);
               
            }
            
            
            
            /*foreach(var fnc in instance.allNativeFunctions)
            {
                checkFunctionsArguments(fnc.Value, null);
                for (int i = 0; i < fnc.Value.Count; i++)
                {
                    for (int j = i + 1; j < fnc.Value.Count; j++)
                    {
                        if (false==areIntervalsDisjuct(fnc.Value.ElementAt(i).MinArgumentCount, fnc.Value.ElementAt(i).MaxArgumentCount,fnc.Value.ElementAt(j).MinArgumentCount, fnc.Value.ElementAt(j).MaxArgumentCount))
                        {
                        Console.WriteLine("function: {0}", fnc.Value.ElementAt(0).Name);
                        Console.WriteLine("{0} {1} {2} {3}",fnc.Value.ElementAt(i).MinArgumentCount, fnc.Value.ElementAt(i).MaxArgumentCount,fnc.Value.ElementAt(j).MinArgumentCount, fnc.Value.ElementAt(j).MaxArgumentCount);
                        }
                    }
                }
            }*/
            return instance;
        }
        /// <summary>
        /// Tells us if the two intervals intersects.
        /// </summary>
        /// <param name="a">start of first interval</param>
        /// <param name="b">end of first interval</param>
        /// <param name="c">start of second interval</param>
        /// <param name="d">end of second interval</param>
        /// <returns>Return true when interval a,b intersects c,d</returns>
        private static bool areIntervalsDisjuct(int a, int b, int c, int d)
        {
            if (b >= c && a < d)
            {
                return false;
            }
            else
            {
                return true;
            }
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



        static public void checkFunctionsArguments(List<NativeFunction> nativeFunctions,FlowController flow)
        {
            //check number of arduments
            MemoryEntry argc = flow.InSet.ReadValue(new VariableName(".argument_count"));
            int argumentCount = ((IntegerValue)argc.PossibleValues.ElementAt(0)).Value;

            //argument count hassnt been comupted yet
            if (nativeFunctions.ElementAt(0).MinArgumentCount == -1)
            {
                foreach (var nativeFuntion in nativeFunctions)
                {
                    nativeFuntion.MinArgumentCount = 0;
                    nativeFuntion.MaxArgumentCount = 0;
                    foreach (var nativeFunctionArgument in nativeFuntion.Arguments)
                    {
                        if (nativeFunctionArgument.Dots)
                        {
                            nativeFuntion.MaxArgumentCount = 1000000000;
                        }
                        else if (nativeFunctionArgument.Optional)
                        {
                            nativeFuntion.MaxArgumentCount++;
                        }
                        else
                        {
                            nativeFuntion.MinArgumentCount++;
                            nativeFuntion.MaxArgumentCount++;
                        }
                    }
                    //Console.WriteLine("Name: {0},Min: {1}, Max: {2}", nativeFuntion.Name,nativeFuntion.MinArgumentCount, nativeFuntion.MaxArgumentCount);
                }
                
            }
            

            string numberOfArgumentMessage="";

            bool argumentCountMatches = false;
            foreach (var nativeFunction in nativeFunctions)
            {
                if (nativeFunction.MinArgumentCount <= argumentCount && nativeFunction.MaxArgumentCount >= argumentCount) 
                {
                    argumentCountMatches = true;
                }
                
                if (numberOfArgumentMessage != "")
                {
                    numberOfArgumentMessage += " or";
                }

                if (nativeFunction.MaxArgumentCount >= 1000000000)
                {
                    if (nativeFunction.MinArgumentCount == 1)
                    {
                        numberOfArgumentMessage += " at least " + nativeFunction.MinArgumentCount + " parameter";
                    }
                    else 
                    {
                        numberOfArgumentMessage += " at least " + nativeFunction.MinArgumentCount + " parameters";
                    }
                }
                else
                {
                    if (nativeFunction.MaxArgumentCount == nativeFunction.MinArgumentCount)
                    {
                        if (nativeFunction.MinArgumentCount == 1)
                        {
                            numberOfArgumentMessage += " " + nativeFunction.MinArgumentCount + " parameter";
                        }
                        else
                        {
                            numberOfArgumentMessage += " " + nativeFunction.MinArgumentCount + " parameters";
                        }
                    }
                    else 
                    {
                        numberOfArgumentMessage += " " + nativeFunction.MinArgumentCount + "-" + nativeFunction.MaxArgumentCount + " parameters";                  
                    }
                }
            }

            if (argumentCountMatches == false)
            {
                string s = "";
                if (argumentCount != 1)
                {
                    s = "s";
                }
                AnalysisWarningHandler.SetWarning(flow.OutSet, new AnalysisWarning("Function "+nativeFunctions.ElementAt(0).Name.ToString() + " expects" + numberOfArgumentMessage + ", " + argumentCount + " parameter"+s+" given.", flow.CurrentPartial));
                return;
            }

            foreach (var nativeFunction in nativeFunctions)
            {
                List<AnalysisWarning> warnings = new List<AnalysisWarning>();
                if (nativeFunction.MinArgumentCount <= argumentCount && nativeFunction.MaxArgumentCount >= argumentCount)
                {
                    for (int i = 0; i < argumentCount; i++)
                    {
                        MemoryEntry arg = flow.InSet.ReadValue(argument(0));
                        checkArgument(arg, nativeFunction.Arguments.ElementAt(i), warnings);
                    }
                }
            }
       }


        private static void checkArgument(MemoryEntry possibleValues, NativeFunctionArgument argument, List<AnalysisWarning> warnings)
        {
                
        }

        private static Value getReturnValue(NativeFunction function)
        {
            return null;
        }

        private static VariableName argument(int index)
        {
            if (index < 0)
            {
                throw new NotSupportedException("Cannot get argument variable for negative index");
            }
            return new VariableName(".arg" + index);
        }


        public static void Main(string[] args)
        {
            try
            {
                string code = @"
                $a=mysql_query();
                $b=min();
                $c=max($array,$array,$array,$array);
                $d=htmlspecialchars(0,1,2,3,4,5,6);
                $e=strstr($a);
                ";
                var fileName = "./cfg_test.php";
                var sourceFile = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)), new FullPath(fileName));
                code = "<?php \n" + code + "?>";
                
                var parser = new SyntaxParser(sourceFile, code);
                parser.Parse();
                var cfg = new ControlFlowGraph.ControlFlowGraph(parser.Ast);

                var analysis = new AdvancedForwardAnalysis(cfg);
                analysis.Analyse();

                Console.WriteLine(analysis.ProgramPointGraph.End.OutSet.ReadValue(new VariableName("a")));
                Console.WriteLine();
                foreach (var warning in AnalysisWarningHandler.ReadWarnings(analysis.ProgramPointGraph.End.OutSet))
                {
                    Console.WriteLine(warning);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
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
            NativeFunctionAnalyzer.checkFunctionsArguments(nativeFunctions, flow);
            //return value
            var possibleValues = new List<Value>();
            possibleValues.Add(flow.OutSet.AnyValue);
            flow.OutSet.Assign(flow.OutSet.ReturnValue, new MemoryEntry(possibleValues.ToArray()));
        }
    }
}
