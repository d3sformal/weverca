using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHP.Core;
using Weverca.Analysis.ExpressionEvaluator;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;


namespace Weverca.Analysis
{
    class NativeAnalyzerUtils
    {
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


        static public bool checkArgumentsCount(FlowController flow, NativeFunction nativeFunction)
        {
            List<NativeFunction> nativeFunctions = new List<NativeFunction>();
            nativeFunctions.Add(nativeFunction);
            return checkArgumentsCount(flow, nativeFunctions);
        }

        static public bool checkArgumentsCount(FlowController flow, List<NativeFunction> nativeFunctions)
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


            string numberOfArgumentMessage = "";

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

        public static void checkArgumentTypes(FlowController flow, NativeFunction nativeFunction)
        {
            List<NativeFunction> nativeFunctions = new List<NativeFunction>();
            nativeFunctions.Add(nativeFunction);
            checkArgumentTypes(flow, nativeFunctions);
        }

        public static void checkArgumentTypes(FlowController flow, List<NativeFunction> nativeFunctions)
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

                        MemoryEntry arg = flow.InSet.ReadValue(Argument(i));

                        NativeFunctionArgument functionArgument = nativeFunction.Arguments.ElementAt(functionArgumentNumber);

                        CheckArgumentTypes(flow, arg, functionArgument, i + 1, nativeFunctions.ElementAt(0).Name.ToString(), warningsList.Last());

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

        public static List<Value> ResolveAliasArguments(FlowController flow, List<NativeFunction> nativeFunctions)
        {
            List<Value> result = new List<Value>();
            MemoryEntry argc = flow.InSet.ReadValue(new VariableName(".argument_count"));
            int argumentCount = ((IntegerValue)argc.PossibleValues.ElementAt(0)).Value;

            foreach (var nativeFunction in nativeFunctions)
            {
                if (nativeFunction.MinArgumentCount <= argumentCount && nativeFunction.MaxArgumentCount >= argumentCount)
                {

                    int functionArgumentNumber = 0;
                    for (int i = 0; i < argumentCount; i++)
                    {

                        MemoryEntry arg = flow.InSet.ReadValue(Argument(i));

                        NativeFunctionArgument functionArgument = nativeFunction.Arguments.ElementAt(functionArgumentNumber);
                        if (functionArgument.ByReference == true)
                        {
                            MemoryEntry res = NativeAnalyzerUtils.ResolveReturnValue(functionArgument.Type, flow);
                            flow.OutSet.Assign(Argument(i), res);
                            result.AddRange(res.PossibleValues);
                        }


                        //incremeneting functionArgumentNumber
                        if (nativeFunction.Arguments.ElementAt(functionArgumentNumber).Dots == false)
                        {
                            functionArgumentNumber++;
                        }
                    }
                }

            }
            return result;
        }

        private static void CheckArgumentTypes(FlowController flow, MemoryEntry memoryEntry, NativeFunctionArgument argument, int argumentNumber, string functionName, List<AnalysisWarning> warnings)
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
                        if (!(ValueTypeResolver.IsInt(value) || ValueTypeResolver.IsLong(value)))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "float":
                    case "number":
                        if (!(ValueTypeResolver.IsInt(value) || ValueTypeResolver.IsLong(value) || ValueTypeResolver.IsFloat(value)))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "string":
                    case "char":
                        if (!ValueTypeResolver.IsString(value))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "array":
                        if (!ValueTypeResolver.IsArray(value))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "object":
                        if (!ValueTypeResolver.IsObject(value))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "bool":
                    case "boolean":
                        if (!ValueTypeResolver.IsBool(value))
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
                        if (!ValueTypeResolver.IsString(value) && !(value is FunctionValue))
                        {
                            argumentMatches = false;
                        }
                        break;
                    case "void":
                        throw new Exception("Void is not a type of argument");
                    default:
                        if (!ValueTypeResolver.IsObject(value))
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

        public static MemoryEntry ResolveReturnValue(string type, FlowController flow)
        {
            var outset = flow.OutSet;
            List<Value> res = new List<Value>();
            switch (type)
            {
                case "number":
                    res.Add(outset.AnyIntegerValue);
                    break;
                case "float":
                case "double":
                    res.Add(outset.AnyFloatValue);
                    break;
                case "int":
                case "integer":
                    res.Add(outset.AnyIntegerValue);
                    break;
                case "long":
                    res.Add(outset.AnyLongintValue);
                    break;
                case "string":
                    res.Add(outset.AnyStringValue);
                    break;
                case "array":
                    res.Add(outset.AnyArrayValue);
                    break;
                case "void":
                case "none":
                    res.Add(outset.UndefinedValue);
                    break;
                case "boolean":
                case "bool":
                    res.Add(outset.AnyBooleanValue);
                    break;
                case "object":
                    res.Add(outset.AnyObjectValue);
                    break;
                case "mixed":
                case "any":
                case "ReturnType":
                    res.Add(outset.AnyValue);
                    break;
                case "NULL":
                    res.Add(outset.UndefinedValue);
                    break;
                case "resource":
                    res.Add(outset.AnyResourceValue);
                    break;
                case "callable":
                    res.Add(outset.AnyStringValue);
                    break;
                case "bool|string":
                    res.Add(outset.AnyStringValue);
                    res.Add(outset.AnyBooleanValue);
                    break;
                case "string|array":
                    res.Add(outset.AnyStringValue);
                    res.Add(outset.AnyArrayValue);
                    break;
                case "bool|array":
                    res.Add(outset.AnyStringValue);
                    res.Add(outset.AnyArrayValue);
                    break;
                default:
                    res.Add(CreateObject(flow, type));
                    break;
            }

            return new MemoryEntry(res);
        }

        private static Value CreateObject(FlowController flow, string type)
        {
            var objectAnalyzer = NativeObjectAnalyzer.GetInstance(flow);
            QualifiedName typeName = new QualifiedName(new Name(type));
            if (objectAnalyzer.ExistClass(typeName))
            {
                ObjectDecl decl = objectAnalyzer.GetClass(typeName);

                var fields = objectAnalyzer.GetClass(typeName).Fields;
                ObjectValue value = flow.OutSet.CreateObject(flow.OutSet.CreateType(decl));
                if (value is ObjectValue)
                {
                    var obj = (value as ObjectValue);
                    foreach (NativeFieldInfo field in fields.Values)
                    {
                        if (field.IsStatic == false)
                        {
                            flow.OutSet.SetField(obj, flow.OutSet.CreateIndex(field.Name.Value), (NativeAnalyzerUtils.ResolveReturnValue(field.Type, flow)));
                        }
                    }
                }
                return value;
            }
            else
            {
                return flow.OutSet.AnyObjectValue;
            }

        }

        public static VariableName Argument(int index)
        {
            if (index < 0)
            {
                throw new NotSupportedException("Cannot get argument variable for negative index");
            }
            return new VariableName(".arg" + index);
        }
    }
}
