using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;


using Weverca.AnalysisFramework;
using PHP.Core;
using Weverca.AnalysisFramework.Memory;
using Weverca.Parsers;

using PHP.Core.Parsers;
using PHP.Core.AST;
using Weverca.Analysis.ExpressionEvaluator;

namespace Weverca.Analysis
{
    //TODO - funkcie co nesiria priznaky - pridat najst


    public class NativeFunctionArgument
    {
        public string Type { get; private set; }
        public bool ByReference { get; private set; }
        public bool Optional { get; private set; }
        public bool Dots { get; private set; }
        public NativeFunctionArgument(string type, bool optional, bool byReference, bool dots)
        {
            this.Type = type;
            this.ByReference = byReference;
            this.Optional = optional;
            this.Dots = dots;
        }
    }

    public class NativeFunction
    {
        public NativeAnalyzerMethod Analyzer { get; set; }
        public QualifiedName Name { get; protected set; }
        public List<NativeFunctionArgument> Arguments { get; protected set; }
        public string ReturnType { get; protected set; }
        public int MinArgumentCount = -1;
        public int MaxArgumentCount = -1;
        public NativeFunction(QualifiedName name, string returnType, List<NativeFunctionArgument> arguments)
        {
            this.Name = name;
            this.Arguments = arguments;
            this.ReturnType = returnType;
            this.Analyzer = null;
        }
        public NativeFunction()
        { }
    }

    //TODO informacie o objektoch kvoli implementacii is_subclass, ktora je potreba pri exceptions

    public class NativeFunctionAnalyzer
    {
        private Dictionary<QualifiedName, List<NativeFunction>> allNativeFunctions = new Dictionary<QualifiedName, List<NativeFunction>>();
        private Dictionary<QualifiedName, NativeAnalyzerMethod> phalangerImplementedFunctions = new Dictionary<QualifiedName, NativeAnalyzerMethod>();
        private Dictionary<QualifiedName, NativeAnalyzerMethod> wevercaImplementedFunctions = new Dictionary<QualifiedName, NativeAnalyzerMethod>();
        private HashSet<string> types = new HashSet<string>();
        private HashSet<string> returnTypes = new HashSet<string>();
        private static NativeFunctionAnalyzer instance = null;

        #region consructor, xml parser

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
            FunctionAnalyzerHelper analyzer = new FunctionAnalyzerHelper(allNativeFunctions[defineName]);
            wevercaImplementedFunctions.Add(defineName, new NativeAnalyzerMethod(analyzer._define));

            QualifiedName constantName = new QualifiedName(new Name("constant"));
            FunctionAnalyzerHelper constantAnalyzer = new FunctionAnalyzerHelper(allNativeFunctions[constantName]);
            wevercaImplementedFunctions.Add(constantName, new NativeAnalyzerMethod(constantAnalyzer._constant));

            QualifiedName is_arrayName = new QualifiedName(new Name("is_array"));
            FunctionAnalyzerHelper is_arrayAnalyzer = new FunctionAnalyzerHelper(allNativeFunctions[is_arrayName]);
            wevercaImplementedFunctions.Add(is_arrayName, new NativeAnalyzerMethod(is_arrayAnalyzer._is_array));

            QualifiedName is_boolName = new QualifiedName(new Name("is_bool"));
            FunctionAnalyzerHelper is_boolAnalyzer = new FunctionAnalyzerHelper(allNativeFunctions[is_boolName]);
            wevercaImplementedFunctions.Add(is_boolName, new NativeAnalyzerMethod(is_boolAnalyzer._is_bool));

            QualifiedName is_doubleName = new QualifiedName(new Name("is_double"));
            FunctionAnalyzerHelper is_doubleAnalyzer = new FunctionAnalyzerHelper(allNativeFunctions[is_doubleName]);
            wevercaImplementedFunctions.Add(is_doubleName, new NativeAnalyzerMethod(is_doubleAnalyzer._is_double));

            QualifiedName is_floatName = new QualifiedName(new Name("is_float"));
            FunctionAnalyzerHelper is_floatAnalyzer = new FunctionAnalyzerHelper(allNativeFunctions[is_floatName]);
            wevercaImplementedFunctions.Add(is_floatName, new NativeAnalyzerMethod(is_floatAnalyzer._is_double));

            QualifiedName is_intName = new QualifiedName(new Name("is_int"));
            FunctionAnalyzerHelper is_intAnalyzer = new FunctionAnalyzerHelper(allNativeFunctions[is_intName]);
            wevercaImplementedFunctions.Add(is_intName, new NativeAnalyzerMethod(is_intAnalyzer._is_int));

            QualifiedName is_integerName = new QualifiedName(new Name("is_integer"));
            FunctionAnalyzerHelper is_integerAnalyzer = new FunctionAnalyzerHelper(allNativeFunctions[is_integerName]);
            wevercaImplementedFunctions.Add(is_integerName, new NativeAnalyzerMethod(is_integerAnalyzer._is_int));

            QualifiedName is_longName = new QualifiedName(new Name("is_long"));
            FunctionAnalyzerHelper is_longAnalyzer = new FunctionAnalyzerHelper(allNativeFunctions[is_longName]);
            wevercaImplementedFunctions.Add(is_longName, new NativeAnalyzerMethod(is_longAnalyzer._is_int));

            QualifiedName is_nullName = new QualifiedName(new Name("is_null"));
            FunctionAnalyzerHelper is_nullAnalyzer = new FunctionAnalyzerHelper(allNativeFunctions[is_nullName]);
            wevercaImplementedFunctions.Add(is_nullName, new NativeAnalyzerMethod(is_nullAnalyzer._is_null));

            QualifiedName is_numericName = new QualifiedName(new Name("is_numeric"));
            FunctionAnalyzerHelper is_numericAnalyzer = new FunctionAnalyzerHelper(allNativeFunctions[is_numericName]);
            wevercaImplementedFunctions.Add(is_numericName, new NativeAnalyzerMethod(is_numericAnalyzer._is_numeric));

            QualifiedName is_objectName = new QualifiedName(new Name("is_object"));
            FunctionAnalyzerHelper is_objectAnalyzer = new FunctionAnalyzerHelper(allNativeFunctions[is_objectName]);
            wevercaImplementedFunctions.Add(is_objectName, new NativeAnalyzerMethod(is_objectAnalyzer._is_object));

            QualifiedName is_realName = new QualifiedName(new Name("is_real"));
            FunctionAnalyzerHelper is_realAnalyzer = new FunctionAnalyzerHelper(allNativeFunctions[is_realName]);
            wevercaImplementedFunctions.Add(is_realName, new NativeAnalyzerMethod(is_realAnalyzer._is_double));

            QualifiedName is_resourceName = new QualifiedName(new Name("is_resource"));
            FunctionAnalyzerHelper is_resourceAnalyzer = new FunctionAnalyzerHelper(allNativeFunctions[is_resourceName]);
            wevercaImplementedFunctions.Add(is_resourceName, new NativeAnalyzerMethod(is_resourceAnalyzer._is_resource)); 

            QualifiedName is_scalarName = new QualifiedName(new Name("is_scalar"));
            FunctionAnalyzerHelper is_scalarAnalyzer = new FunctionAnalyzerHelper(allNativeFunctions[is_scalarName]);
            wevercaImplementedFunctions.Add(is_scalarName, new NativeAnalyzerMethod(is_scalarAnalyzer._is_scalar));

            QualifiedName is_stringName = new QualifiedName(new Name("is_string"));
            FunctionAnalyzerHelper is_stringAnalyzer = new FunctionAnalyzerHelper(allNativeFunctions[is_stringName]);
            wevercaImplementedFunctions.Add(is_stringName, new NativeAnalyzerMethod(is_stringAnalyzer._is_string));
        
        }

        #endregion


        static public NativeFunctionAnalyzer CreateInstance()
        {
            if (instance != null)
            {
                return instance;
            }
            instance = new NativeFunctionAnalyzer();
            return instance;
        }

        
        public bool existNativeFunction(QualifiedName name)
        {
            return allNativeFunctions.ContainsKey(name);
        }
        public QualifiedName[] getNativeFunctions()
        {
            return allNativeFunctions.Keys.ToArray();
        }
        public NativeAnalyzerMethod GetInstance(QualifiedName name)
        {
            if (!existNativeFunction(name))
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
                    FunctionAnalyzerHelper analyzer = new FunctionAnalyzerHelper(allNativeFunctions[name]);
                    allNativeFunctions[name][0].Analyzer = new NativeAnalyzerMethod(analyzer.analyze);
                }
                return allNativeFunctions[name][0].Analyzer;

            }
            //doesnt exist
            return null;
        }
    }

    class FunctionAnalyzerHelper
    {
        private List<NativeFunction> nativeFunctions;

        #region functions

        public FunctionAnalyzerHelper(List<NativeFunction> nativeFunctions)
        {
            this.nativeFunctions = nativeFunctions;
        }

        public void analyze(FlowController flow)
        {
            if (NativeAnalyzerUtils.checkArgumentsCount(flow, nativeFunctions))
            {
                NativeAnalyzerUtils.checkArgumentTypes(flow, nativeFunctions);
            }
            //return value

            MemoryEntry argc = flow.InSet.ReadValue(new VariableName(".argument_count"));
            int argumentCount = ((IntegerValue)argc.PossibleValues.ElementAt(0)).Value;

            var possibleValues = new List<Value>();
            foreach (var nativeFunction in nativeFunctions)
            {
                if (nativeFunction.MinArgumentCount <= argumentCount && nativeFunction.MaxArgumentCount >= argumentCount)
                {
                    foreach (var value in NativeAnalyzerUtils.ResolveReturnValue(nativeFunction.ReturnType, flow).PossibleValues)
                    {
                        possibleValues.Add(value);
                    }
                }
            }

            if (possibleValues.Count == 0)
            {
                foreach (var nativeFunction in nativeFunctions)
                {
                    foreach (var value in NativeAnalyzerUtils.ResolveReturnValue(nativeFunction.ReturnType, flow).PossibleValues)
                    {
                        possibleValues.Add(value);
                    }
                }
            }

            List<MemoryEntry> arguments = new List<MemoryEntry>();
            for (int i = 0; i < argumentCount; i++)
            {
                arguments.Add(flow.OutSet.ReadValue(NativeAnalyzerUtils.Argument(i)));
            }

            MemoryEntry functionResult = new MemoryEntry(possibleValues.ToArray());
            flow.OutSet.Assign(flow.OutSet.ReturnValue, functionResult);
            ValueInfoHandler.CopyFlags(flow.OutSet, arguments, functionResult);
            List<Value> assigned_aliases = NativeAnalyzerUtils.ResolveAliasArguments(flow, nativeFunctions);
            ValueInfoHandler.CopyFlags(flow.OutSet, arguments, new MemoryEntry(assigned_aliases));
           

        }

        #endregion

        #region implementation of native php functions

        //todo unknown string - vytvori unknown costant pockat na podporu memory modelu
        public void _define(FlowController flow)
        {

            NativeAnalyzerUtils.checkArgumentsCount(flow, nativeFunctions);
            MemoryEntry argc = flow.InSet.ReadValue(new VariableName(".argument_count"));
            int argumentCount = ((IntegerValue)argc.PossibleValues.ElementAt(0)).Value;

            var nativeFunction = nativeFunctions.ElementAt(0);
            List<Value> possibleValues = new List<Value>();
            bool canBeTrue = false;
            bool canBeFalse = false;
            if (nativeFunction.MinArgumentCount <= argumentCount && nativeFunction.MaxArgumentCount >= argumentCount)
            {
                bool canBeCaseSensitive = false, canBeCaseInsensitive = false;
                if (argumentCount == 2)
                {
                    canBeCaseSensitive = true;
                }
                else
                {
                    foreach (var arg2 in flow.OutSet.ReadValue(NativeAnalyzerUtils.Argument(2)).PossibleValues)
                    {
                        UnaryOperationVisitor unaryVisitor = new UnaryOperationVisitor(new ExpressionEvaluator.ExpressionEvaluator());
                        Value result = unaryVisitor.Evaluate(Operations.BoolCast, arg2);
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
                foreach (var arg0 in flow.OutSet.ReadValue(NativeAnalyzerUtils.Argument(0)).PossibleValues)
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

                    QualifiedName qConstantName = new QualifiedName(new Name(constantName));
                    List<Value> result = new List<Value>();
                    foreach (var arg1 in flow.OutSet.ReadValue(NativeAnalyzerUtils.Argument(1)).PossibleValues)
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
            if (NativeAnalyzerUtils.checkArgumentsCount(flow, nativeFunctions))
            {
                foreach (var arg0 in flow.OutSet.ReadValue(NativeAnalyzerUtils.Argument(0)).PossibleValues)
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
                        values = UserDefinedConstantHandler.GetConstant(flow.OutSet, name).PossibleValues.ToList();
                    }

                    flow.OutSet.Assign(flow.OutSet.ReturnValue, new MemoryEntry(values));
                }
            }
            else
            {
                flow.OutSet.Assign(flow.OutSet.ReturnValue, new MemoryEntry(flow.OutSet.UndefinedValue));
            }
        }


        delegate bool Typedelegate(Value value);

        public void _is_array(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value => {
                return ValueTypeResolver.isArray(value);    
            }));
        }
        public void _is_bool(FlowController flow) {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.isBool(value);
            }));
        }
        public void _is_double(FlowController flow) {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.isFloat(value);
            }));
        }
        public void _is_int(FlowController flow) {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.isInt(value) || ValueTypeResolver.isLong(value);
            }));
        }
        public void _is_null(FlowController flow) {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return (value is UndefinedValue);
            }));
        }
        public void _is_numeric(FlowController flow) {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.isInt(value) || ValueTypeResolver.isLong(value) || ValueTypeResolver.isFloat(value);
            }));
        }
        public void _is_object(FlowController flow) {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.isObject(value);
            }));
        }
        public void _is_resource(FlowController flow) {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return (value is AnyResourceValue);
            }));
        }
        public void _is_scalar(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.isInt(value) || ValueTypeResolver.isLong(value) || ValueTypeResolver.isFloat(value) || ValueTypeResolver.isBool(value) || ValueTypeResolver.isString(value); ;
            }));
        }
        public void _is_string(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.isString(value);
            }));
        }


        private void processIsFunctions(FlowController flow,Typedelegate del)
        {
            if (NativeAnalyzerUtils.checkArgumentsCount(flow, nativeFunctions))
            {
                bool canBeTrue = false;
                bool canBeFalse = false;
                foreach (var arg0 in flow.OutSet.ReadValue(NativeAnalyzerUtils.Argument(0)).PossibleValues)
                {
                    if (del(arg0))
                    {
                        canBeTrue = true;
                    }
                    else if (arg0 is AnyValue)
                    {
                        canBeTrue = true;
                        canBeFalse = true;
                        break;
                    }
                    else
                    {
                        canBeFalse = true;
                    }
                }
                List<Value> result = new List<Value>();
                if (canBeTrue)
                {
                    result.Add(flow.OutSet.CreateBool(true));
                }
                if (canBeFalse)
                {
                    result.Add(flow.OutSet.CreateBool(false));
                }
                flow.OutSet.Assign(flow.OutSet.ReturnValue, new MemoryEntry(result));
            }
            else
            {
                flow.OutSet.Assign(flow.OutSet.ReturnValue, new MemoryEntry(flow.OutSet.AnyBooleanValue));
            }
        }

        #endregion
    }
}
