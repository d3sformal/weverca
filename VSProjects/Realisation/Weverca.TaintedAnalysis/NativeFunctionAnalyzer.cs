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
        private HashSet<string> returnTypes = new HashSet<string>();
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
                            instance.returnTypes.Add(reader.GetAttribute("returnType"));
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
            /*
            var it = instance.types.GetEnumerator();
            while (it.MoveNext())
            {
                Console.WriteLine(it.Current);
               
            }
            Console.WriteLine();
            it = instance.returnTypes.GetEnumerator();
            while (it.MoveNext())
            {
                Console.WriteLine(it.Current);

            }
            */
            
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
            return allNativeFunctions.ContainsKey(name);
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
                AnalysisWarningHandler.SetWarning(flow.OutSet, new AnalysisWarning("Function "+nativeFunctions.ElementAt(0).Name.ToString() + " expects" + numberOfArgumentMessage + ", " + argumentCount + " parameter"+s+" given.", flow.CurrentPartial,AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
                return;
            }

            List<List<AnalysisWarning>> warningsList = new List<List<AnalysisWarning>>();


            foreach (var nativeFunction in nativeFunctions)
            {
                
                if (nativeFunction.MinArgumentCount <= argumentCount && nativeFunction.MaxArgumentCount >= argumentCount)
                {
                    warningsList.Add(new List<AnalysisWarning>());
                    int functionArgumentNumber=0;
                    for (int i = 0; i < argumentCount; i++)
                    {
                        MemoryEntry arg = flow.InSet.ReadValue(argument(i));

                        NativeFunctionArgument functionArgument = nativeFunction.Arguments.ElementAt(functionArgumentNumber);

                        checkArgument(flow, arg, functionArgument, i + 1, nativeFunctions.ElementAt(0).Name.ToString(), warningsList.Last());

                        //incremeneting functionArgumentNumber
                        if (nativeFunction.Arguments.ElementAt(functionArgumentNumber).Dots == false)
                        {
                            functionArgumentNumber++;               
                        }
                    }
                }

            }
            int index_min = -1;
            int value_min = int.MaxValue;
            for (int i = 0; i < warningsList.Count; i++)
            {
                if (warningsList[i].Count < value_min)
                {
                    value_min = warningsList[i].Count;
                    index_min = i;
                }
            }
            foreach (AnalysisWarning warning in warningsList[index_min])
            {
                AnalysisWarningHandler.SetWarning(flow.OutSet, warning);
            }
       }


        private static void checkArgument(FlowController flow,MemoryEntry memoryEntry, NativeFunctionArgument argument,int argumentNumber,string functionName, List<AnalysisWarning> warnings)
        {
            bool argumentMatches = true;
            foreach (Value value in memoryEntry.PossibleValues)
            {
                if (value.GetType() == typeof(AnyValue) || value.GetType() == typeof(UndefinedValue))
                {
                    continue;
                }

                switch (argument.Type)
                {
                    case "mixed":                  
                        break;
                    case "int":
                    case "integer":
                        if (value.GetType() != typeof(IntegerIntervalValue) && value.GetType() != typeof(IntegerValue) && value.GetType() != typeof(AnyIntegerValue) && value.GetType() != typeof(LongintValue) && value.GetType() != typeof(AnyLongintValue) && value.GetType() != typeof(LongintIntervalValue))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "float":
                        if (value.GetType() != typeof(FloatIntervalValue) && value.GetType() != typeof(FloatValue) && value.GetType() != typeof(AnyFloatValue) && value.GetType() != typeof(IntegerIntervalValue) && value.GetType() != typeof(IntegerValue) && value.GetType() != typeof(AnyIntegerValue) && value.GetType() != typeof(LongintValue) && value.GetType() != typeof(AnyLongintValue) && value.GetType() != typeof(LongintIntervalValue))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "number":
                        if (value.GetType() != typeof(FloatIntervalValue) && value.GetType() != typeof(FloatValue) && value.GetType() != typeof(AnyFloatValue) && value.GetType() != typeof(IntegerIntervalValue) && value.GetType() != typeof(IntegerValue) && value.GetType() != typeof(AnyIntegerValue) && value.GetType() != typeof(LongintValue) && value.GetType() != typeof(AnyLongintValue) && value.GetType() != typeof(LongintIntervalValue))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "string":
                    case "char":
                        if(value.GetType()!=typeof(StringValue) && value.GetType()!=typeof(AnyStringValue))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "array":
                        if(value.GetType()!=typeof(AssociativeArray) && value.GetType()!=typeof(AnyArrayValue))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "object":
                        if(value.GetType()!=typeof(ObjectValue) && value.GetType()!=typeof(AnyObjectValue))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "bool":
                    case "boolean":
                        if(value.GetType()!=typeof(BooleanValue) && value.GetType()!=typeof(AnyBooleanValue))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "resource":
                    case "resouce":
                        if (value.GetType() != typeof(AnyResourceValue))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "callable":
                    case "callback":
                        if(value.GetType()!=typeof(StringValue) && value.GetType()!=typeof(AnyStringValue) && value.GetType()!=typeof(FunctionValue))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "void":
                        throw new Exception("Void is not a type of argument");
                    default:
                        if (value.GetType() != typeof(ObjectValue) && value.GetType() != typeof(AnyObjectValue))
                        {
                            argumentMatches = false;
                        }
                        break;
                }
            }
            if (argumentMatches == false)
            {
                warnings.Add(new AnalysisWarning("Wrong type in argument No. " + argumentNumber + " in function " + functionName + ", expecting " + argument.Type, flow.CurrentPartial, AnalysisWarningCause.WRONG_ARGUMENTS_TYPE));
            }
        }

        public static Value getReturnValue(NativeFunction function,FlowController flow)
        {
            var outset = flow.OutSet;
            switch(function.ReturnType)
            {
                case "number":
                    return outset.AnyIntegerValue;
                case "float":
                    return outset.AnyFloatValue;
                case "int":
                case "integer":
                    return outset.AnyIntegerValue;
                case "string":
                    return outset.AnyStringValue;
                case "array":
                    return outset.AnyArrayValue;
                case "void":
                    return outset.UndefinedValue;
                case "boolean":
                case "bool":
                    return outset.AnyBooleanValue;
                case "object":
                    return outset.AnyObjectValue;
                case "mixed":
                    return outset.AnyValue;
                case "resource":
                    return outset.AnyResourceValue;
                case "callable":
                    return outset.AnyStringValue;
                default:
                    return outset.AnyObjectValue;
            }
        }

        internal static VariableName argument(int index)
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
/*
                $c=max(1,2,3,4);
                $e=strstr('a',4,8);
                $f=max(2,'aaa',$e);
                $g=htmlspecialchars('a');*/
                $a[$i]=5;
                
                ";
                var fileName = "./cfg_test.php";
                var sourceFile = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)), new FullPath(fileName));
                code = "<?php \n" + code + "?>";

                var parser = new SyntaxParser(sourceFile, code);
                parser.Parse();
                var cfg = new ControlFlowGraph.ControlFlowGraph(parser.Ast);

                var analysis = new ForwardAnalysis(cfg);
                analysis.Analyse();


                foreach (var warning in AnalysisWarningHandler.ReadWarnings(analysis.ProgramPointGraph.End.OutSet))
                {
                    Console.WriteLine(warning);
                }

                Console.WriteLine(analysis.ProgramPointGraph.End.OutSet.ReadInfo(new VariableName("a")));
                Console.WriteLine(analysis.ProgramPointGraph.End.OutSet.ReadValue(new VariableName("a")));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
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

            MemoryEntry argc = flow.InSet.ReadValue(new VariableName(".argument_count"));
            int argumentCount = ((IntegerValue)argc.PossibleValues.ElementAt(0)).Value;
            
            var possibleValues = new List<Value>();
            foreach (var nativeFunction in nativeFunctions)
            {
                if (nativeFunction.MinArgumentCount <= argumentCount && nativeFunction.MaxArgumentCount >= argumentCount)
                {
                    possibleValues.Add(NativeFunctionAnalyzer.getReturnValue(nativeFunction, flow));
                }
            }

            if (possibleValues.Count == 0)
            {
                foreach (var nativeFunction in nativeFunctions)
                {
                    possibleValues.Add(NativeFunctionAnalyzer.getReturnValue(nativeFunction, flow));
                }
            }

            flow.OutSet.Assign(flow.OutSet.ReturnValue, new MemoryEntry(possibleValues.ToArray()));
        }
    }
}
