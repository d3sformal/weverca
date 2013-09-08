﻿using System;
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
using PHP.Core.AST;
using Weverca.TaintedAnalysis.ExpressionEvaluator;

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
                            arguments = new List<NativeFunctionArgument>();
                            function = reader.GetAttribute("name");
                            returnType = reader.GetAttribute("returnType");
                            functionAlias = reader.GetAttribute("alias");
                            QualifiedName functionName = new QualifiedName(new Name(function));
                            if (functionAlias != null)
                            {
                                allNativeFunctions[functionName] = allNativeFunctions[new QualifiedName(new Name(functionAlias))];
                            }
                            returnTypes.Add(reader.GetAttribute("returnType"));
                        }
                        else if (reader.Name == "arg")
                        {
                            types.Add(reader.GetAttribute("type"));
                            bool optional = false;
                            bool byReference = false;
                            bool dots = false;
                            if (reader.GetAttribute("optional") == "true")
                            {
                                optional = true;
                            }
                            if (reader.GetAttribute("byReference") == "true")
                            {
                                byReference = true;
                            }
                            if (reader.GetAttribute("dots") == "true")
                            {
                                dots = true;
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
                
            }*/


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
            

            QualifiedName defineName = new QualifiedName(new Name("define"));
            AnalyzerClass analyzer = new AnalyzerClass(allNativeFunctions[defineName]);
            wevercaImplementedFunctions.Add(defineName, new NativeAnalyzerMethod(analyzer._define));

            QualifiedName constantName = new QualifiedName(new Name("constant"));
            AnalyzerClass constantAnalyzer = new AnalyzerClass(allNativeFunctions[constantName]);
            wevercaImplementedFunctions.Add(constantName, new NativeAnalyzerMethod(constantAnalyzer._constant));
        }

        static public NativeFunctionAnalyzer CreateInstance()
        {
            if (instance != null)
            {
                return instance;
            }
            instance = new NativeFunctionAnalyzer();
            return instance;
        }
        
        /// <summary>
        /// Tells if the two intervals intersects.
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

        static public bool checkArgumentsCount(List<NativeFunction> nativeFunctions,FlowController flow)
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
                AnalysisWarningHandler.SetWarning(flow.OutSet, new AnalysisWarning("Function " + nativeFunctions.ElementAt(0).Name.ToString() + " expects" + numberOfArgumentMessage + ", " + argumentCount + " parameter" + s + " given.", flow.CurrentPartial, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
                return false;
            }
            else 
            {
                return true;
            }
       }

        public static void checkArgumentTypes(List<NativeFunction> nativeFunctions, FlowController flow)
        {
            List<List<AnalysisWarning>> warningsList = new List<List<AnalysisWarning>>();
            MemoryEntry argc = flow.InSet.ReadValue(new VariableName(".argument_count"));
            int argumentCount = ((IntegerValue)argc.PossibleValues.ElementAt(0)).Value;
            foreach (var nativeFunction in nativeFunctions)
            {

                if (nativeFunction.MinArgumentCount <= argumentCount && nativeFunction.MaxArgumentCount >= argumentCount)
                {
                    warningsList.Add(new List<AnalysisWarning>());
                    int functionArgumentNumber = 0;
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
                if (value is AnyValue || value is UndefinedValue)
                {
                    continue;
                }

                switch (argument.Type)
                {
                    case "mixed":                  
                        break;
                    case "int":
                    case "integer":
                        if (!(ValueTypeResolver.isInt(value) || ValueTypeResolver.isLong(value)))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "float":
                    case "number":
                        if (!(ValueTypeResolver.isInt(value) || ValueTypeResolver.isLong(value) || ValueTypeResolver.isFloat(value)))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "string":
                    case "char":
                        if (!ValueTypeResolver.isString(value))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "array":
                        if(!ValueTypeResolver.isArray(value))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "object":
                        if(!ValueTypeResolver.isObject(value))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "bool":
                    case "boolean":
                        if(!ValueTypeResolver.isBool(value))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "resource":
                    case "resouce":
                        if (!(value is AnyResourceValue))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "callable":
                    case "callback":
                        if(!ValueTypeResolver.isString(value) && !(value is FunctionValue))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "void":
                        throw new Exception("Void is not a type of argument");
                    default:
                        if (!ValueTypeResolver.isObject(value))
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
            if (NativeFunctionAnalyzer.checkArgumentsCount(nativeFunctions, flow))
            {
                NativeFunctionAnalyzer.checkArgumentTypes(nativeFunctions, flow);
            }
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

            List<MemoryEntry> arguments = new List<MemoryEntry>();
            for(int i=0;i<argumentCount;i++)
            {
                arguments.Add(flow.OutSet.ReadValue(NativeFunctionAnalyzer.argument(i)));
            }

            MemoryEntry functionResult = new MemoryEntry(possibleValues.ToArray());
            flow.OutSet.Assign(flow.OutSet.ReturnValue, functionResult);
            foreach (var value in functionResult.PossibleValues)
            {
                if (ValueTypeResolver.CanBeDirty(value))
                {
                    ValueInfoHandler.CopyFlags(flow.OutSet, arguments, value);
                }
            }
        }


        //todo unknown string - vytvori unknown costant pockat na podporu memory modelu
        public void _define(FlowController flow)
        {
            
            NativeFunctionAnalyzer.checkArgumentsCount(nativeFunctions, flow);
            MemoryEntry argc = flow.InSet.ReadValue(new VariableName(".argument_count"));
            int argumentCount = ((IntegerValue)argc.PossibleValues.ElementAt(0)).Value;

            var nativeFunction = nativeFunctions.ElementAt(0);
            List<Value> possibleValues = new List<Value>();
            bool canBeTrue = false;
            bool canBeFalse = false;
            if (nativeFunction.MinArgumentCount <= argumentCount && nativeFunction.MaxArgumentCount >= argumentCount)
            {
                bool canBeCaseSensitive=false, canBeCaseInsensitive=false;
                if (argumentCount == 2)
                {
                    canBeCaseSensitive = true;
                }
                else 
                {
                    foreach (var arg2 in flow.OutSet.ReadValue(NativeFunctionAnalyzer.argument(2)).PossibleValues)
                    {
                        UnaryOperationVisitor unaryVisitor = new UnaryOperationVisitor(new ExpressionEvaluator.ExpressionEvaluator());
                        Value result=unaryVisitor.Evaluate(Operations.BoolCast, arg2);
                        if (result is UndefinedValue)
                        {
                            canBeCaseSensitive = true;
                            canBeCaseInsensitive = true;
                        }
                        else if (result is BooleanValue)
                        {
                            if ((result as BooleanValue).Value == true)
                            {
                                canBeCaseInsensitive = true;
                            }
                            else 
                            {
                                canBeCaseSensitive = true;
                            }

                        }
                        else 
                        {
                            canBeCaseSensitive = true;
                            canBeCaseInsensitive = true;
                        }

                    }
                }
                foreach (var arg0 in flow.OutSet.ReadValue(NativeFunctionAnalyzer.argument(0)).PossibleValues)
                {
                    
                    UnaryOperationVisitor unaryVisitor = new UnaryOperationVisitor(new ExpressionEvaluator.ExpressionEvaluator());
                    Value arg0Retyped = unaryVisitor.Evaluate(Operations.StringCast, arg0);
                    string constantName = "";
                    if (arg0Retyped is UndefinedValue)
                    {
                        canBeFalse = true;
                        continue;
                    }
                    else 
                    {
                        constantName = (arg0Retyped as StringValue).Value;
                    }

                    QualifiedName qConstantName=new QualifiedName(new Name(constantName));
                    List<Value> result=new List<Value>();
                    foreach (var arg1 in flow.OutSet.ReadValue(NativeFunctionAnalyzer.argument(1)).PossibleValues)
                    {
                        if (ValueTypeResolver.isArray(arg1) || ValueTypeResolver.isObject(arg1))
                        {
                            canBeFalse = true;
                        }
                        else 
                        {
                            result.Add(arg1);
                            canBeTrue = true;    
                        }
                    }
                    if (canBeCaseSensitive)
                    {
                        UserDefinedConstantHandler.insertConstant(flow.OutSet, qConstantName, new MemoryEntry(result.ToArray()), false);
                    }
                    if (canBeCaseInsensitive)
                    {
                        UserDefinedConstantHandler.insertConstant(flow.OutSet, qConstantName, new MemoryEntry(result.ToArray()), true);
                    }
                }
                if (canBeTrue)
                {
                    possibleValues.Add(flow.OutSet.CreateBool(true));
                }
                if (canBeFalse)
                {
                    possibleValues.Add(flow.OutSet.CreateBool(false));
                }
            }
            else 
            {
                possibleValues.Add(flow.OutSet.CreateBool(false));
            }
            flow.OutSet.Assign(flow.OutSet.ReturnValue, new MemoryEntry(possibleValues));
        }

        //todo unknown string - vytvori unknown costant pockat na podporu memory modelu
        public void _constant(FlowController flow)
        {
            if (NativeFunctionAnalyzer.checkArgumentsCount(nativeFunctions, flow))
            {
                foreach (var arg0 in flow.OutSet.ReadValue(NativeFunctionAnalyzer.argument(0)).PossibleValues)
                {
                    UnaryOperationVisitor unaryVisitor = new UnaryOperationVisitor(new ExpressionEvaluator.ExpressionEvaluator());
                    Value arg0Retyped = unaryVisitor.Evaluate(Operations.StringCast, arg0);
                    List<Value> values = new List<Value>();
                    NativeConstantAnalyzer constantAnalyzer = NativeConstantAnalyzer.Create(flow.OutSet);
                    QualifiedName name = new QualifiedName(new Name((arg0Retyped as StringValue).Value));

                    if (constantAnalyzer.ExistContant(name))
                    {
                        values.Add(constantAnalyzer.GetConstantValue(name));
                    }
                    else
                    {
                        values = UserDefinedConstantHandler.getConstant(flow.OutSet, name);
                    }

                    flow.OutSet.Assign(flow.OutSet.ReturnValue, new MemoryEntry(values));
                }
            }
            else 
            {
                flow.OutSet.Assign(flow.OutSet.ReturnValue, new MemoryEntry(flow.OutSet.UndefinedValue));
            }
        }

    }
}
