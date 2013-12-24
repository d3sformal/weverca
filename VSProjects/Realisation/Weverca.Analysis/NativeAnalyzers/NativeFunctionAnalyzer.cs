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
using System.Text.RegularExpressions;
using Weverca.Analysis.Properties;

namespace Weverca.Analysis
{
    //TODO - funkcie co nesiria priznaky - pridat najst


    public class NativeFunctionArgument
    {
        public string Type { get; private set; }
        public bool ByReference { get; private set; }
        public bool Optional { get; private set; }
        public bool Dots { get; private set; }
        public NativeFunctionArgument( string type, bool optional, bool byReference, bool dots)
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
        /// <summary>
        /// Type-modeling implementations of functions.
        /// All PHP native functions are modeled by type.
        /// </summary>
        private Dictionary<QualifiedName, List<NativeFunction>> typeModeledFunctions = new Dictionary<QualifiedName, List<NativeFunction>>();
        /// <summary>
        /// Concrete implementations of functions.
        /// </summary>
        private Dictionary<QualifiedName, NativeAnalyzerMethod> concreteFunctions = new Dictionary<QualifiedName, NativeAnalyzerMethod>();
        /// <summary>
        /// Special implementations of functions.
        /// If a special implementation of function exists, it should be called and any other implementation
        /// should not be called.
        /// </summary>
        private Dictionary<QualifiedName, NativeAnalyzerMethod> specialFunctions = new Dictionary<QualifiedName, NativeAnalyzerMethod>();
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
            XmlReader reader = XmlReader.Create(new StreamReader(Settings.Default.PhpFunctionsFile));

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
                                typeModeledFunctions[functionName] = typeModeledFunctions[new QualifiedName(new Name(functionAlias))];
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
                            if (!typeModeledFunctions.ContainsKey(functionName))
                            {
                                typeModeledFunctions[functionName] = new List<NativeFunction>();
                            }
                            typeModeledFunctions[functionName].Add(new NativeFunction(functionName, returnType, arguments));

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
            SpecialFunctionsImplementations analyzer = new SpecialFunctionsImplementations(typeModeledFunctions[defineName]);
            specialFunctions.Add(defineName, new NativeAnalyzerMethod(analyzer._define));

            QualifiedName constantName = new QualifiedName(new Name("constant"));
            SpecialFunctionsImplementations constantAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[constantName]);
            specialFunctions.Add(constantName, new NativeAnalyzerMethod(constantAnalyzer._constant));

            QualifiedName is_arrayName = new QualifiedName(new Name("is_array"));
            SpecialFunctionsImplementations is_arrayAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_arrayName]);
            specialFunctions.Add(is_arrayName, new NativeAnalyzerMethod(is_arrayAnalyzer._is_array));

            QualifiedName is_boolName = new QualifiedName(new Name("is_bool"));
            SpecialFunctionsImplementations is_boolAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_boolName]);
            specialFunctions.Add(is_boolName, new NativeAnalyzerMethod(is_boolAnalyzer._is_bool));

            QualifiedName is_doubleName = new QualifiedName(new Name("is_double"));
            SpecialFunctionsImplementations is_doubleAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_doubleName]);
            specialFunctions.Add(is_doubleName, new NativeAnalyzerMethod(is_doubleAnalyzer._is_double));

            QualifiedName is_floatName = new QualifiedName(new Name("is_float"));
            SpecialFunctionsImplementations is_floatAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_floatName]);
            specialFunctions.Add(is_floatName, new NativeAnalyzerMethod(is_floatAnalyzer._is_double));

            QualifiedName is_intName = new QualifiedName(new Name("is_int"));
            SpecialFunctionsImplementations is_intAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_intName]);
            specialFunctions.Add(is_intName, new NativeAnalyzerMethod(is_intAnalyzer._is_int));

            QualifiedName is_integerName = new QualifiedName(new Name("is_integer"));
            SpecialFunctionsImplementations is_integerAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_integerName]);
            specialFunctions.Add(is_integerName, new NativeAnalyzerMethod(is_integerAnalyzer._is_int));

            QualifiedName is_longName = new QualifiedName(new Name("is_long"));
            SpecialFunctionsImplementations is_longAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_longName]);
            specialFunctions.Add(is_longName, new NativeAnalyzerMethod(is_longAnalyzer._is_int));

            QualifiedName is_nullName = new QualifiedName(new Name("is_null"));
            SpecialFunctionsImplementations is_nullAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_nullName]);
            specialFunctions.Add(is_nullName, new NativeAnalyzerMethod(is_nullAnalyzer._is_null));

            QualifiedName is_numericName = new QualifiedName(new Name("is_numeric"));
            SpecialFunctionsImplementations is_numericAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_numericName]);
            specialFunctions.Add(is_numericName, new NativeAnalyzerMethod(is_numericAnalyzer._is_numeric));

            QualifiedName is_objectName = new QualifiedName(new Name("is_object"));
            SpecialFunctionsImplementations is_objectAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_objectName]);
            specialFunctions.Add(is_objectName, new NativeAnalyzerMethod(is_objectAnalyzer._is_object));

            QualifiedName is_realName = new QualifiedName(new Name("is_real"));
            SpecialFunctionsImplementations is_realAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_realName]);
            specialFunctions.Add(is_realName, new NativeAnalyzerMethod(is_realAnalyzer._is_double));

            QualifiedName is_resourceName = new QualifiedName(new Name("is_resource"));
            SpecialFunctionsImplementations is_resourceAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_resourceName]);
            specialFunctions.Add(is_resourceName, new NativeAnalyzerMethod(is_resourceAnalyzer._is_resource)); 

            QualifiedName is_scalarName = new QualifiedName(new Name("is_scalar"));
            SpecialFunctionsImplementations is_scalarAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_scalarName]);
            specialFunctions.Add(is_scalarName, new NativeAnalyzerMethod(is_scalarAnalyzer._is_scalar));

            QualifiedName is_stringName = new QualifiedName(new Name("is_string"));
            SpecialFunctionsImplementations is_stringAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_stringName]);
            specialFunctions.Add(is_stringName, new NativeAnalyzerMethod(is_stringAnalyzer._is_string));

            NativeFunctionsConcreteImplementations.AddConcreteFunctions(typeModeledFunctions, concreteFunctions);
        
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
            if(name==new QualifiedName(new Name(".initStaticProperties")))
            {
                return true;
            }
            return typeModeledFunctions.ContainsKey(name);
        }
        public QualifiedName[] getNativeFunctions()
        {
            return typeModeledFunctions.Keys.ToArray();
        }

        public NativeAnalyzerMethod GetInstance(QualifiedName name) 
        {
            if (!existNativeFunction(name))
            {
                return null;
            }

            if (name == new QualifiedName(new Name(".initStaticProperties")))
            {
                return new NativeAnalyzerMethod(InsetStaticPropertiesIntoMemoryModel);
            }

            if (specialFunctions.Keys.Contains(name))
            {
                return specialFunctions[name];
            }

            if (concreteFunctions.Keys.Contains(name))
            {
                return concreteFunctions[name];
            }

            // default: model function by type
            InitTypeModeledFunction(name);
            return typeModeledFunctions[name][0].Analyzer;




        }

        private void InitTypeModeledFunction(QualifiedName name)
        {
            if (typeModeledFunctions[name][0].Analyzer == null)
            {
                TypeModeledFunctionAnalyzerHelper analyzer = new TypeModeledFunctionAnalyzerHelper(typeModeledFunctions[name]);
                typeModeledFunctions[name][0].Analyzer = new NativeAnalyzerMethod(analyzer.analyze);
            }
        }

   
        public static List<string> indices;
        public void InsetStaticPropertiesIntoMemoryModel(FlowController flow)
        {
            //todo replace with iterate array
            var res=flow.OutSet.ReadVariable(NativeAnalyzerUtils.Argument(0));
            foreach (string index in indices)
            {
                if (index.StartsWith("."))
                {
                    var mmValue = res.ReadIndex(flow.OutSet.Snapshot, new MemberIdentifier(index));
                    List<Value> valueToWrite = new List<Value>();
                    valueToWrite.AddRange(mmValue.ReadMemory(flow.OutSet.Snapshot).PossibleValues);
                    if (flow.OutSet.GetControlVariable(new VariableName(index)).IsDefined(flow.OutSet.Snapshot))
                    {
                        valueToWrite.AddRange(flow.OutSet.GetControlVariable(new VariableName(index)).ReadMemory(flow.OutSet.Snapshot).PossibleValues);
                    }

                    flow.OutSet.GetControlVariable(new VariableName(index)).WriteMemory(flow.OutSet.Snapshot, new MemoryEntry(valueToWrite));
                }
                else 
                {
                    var publicFieldPattern = @"^@class@(.*)@->publicfield@(.*)@$";
                    var nonPublicFieldPattern = @"^@class@(.*)@->nonpublicfield@(.*)@$";
                    var publicRegularExpression = new Regex(publicFieldPattern, RegexOptions.None);
                    var nonPublicRegularExpression = new Regex(nonPublicFieldPattern, RegexOptions.None);
                    var match = publicRegularExpression.Match(index);
                    
                    string visibility="";
                    if (!match.Success)
                    {
                        match = nonPublicRegularExpression.Match(index);
                        visibility = "nonpublic";
                    }
                    else 
                    {
                        visibility = "public";
                    }

                    string className = match.Groups[1].Value;
                    string fieldname = match.Groups[2].Value;
                    SnapshotBase snapshot = flow.OutSet.Snapshot;
                    var staticField = flow.OutSet.GetControlVariable(new VariableName(".staticVariables")).ReadIndex(snapshot, new MemberIdentifier(className)).ReadIndex(snapshot, new MemberIdentifier(visibility)).ReadIndex(snapshot, new MemberIdentifier(fieldname));
                    List<Value> values = new List<Value>();
                    if (staticField.IsDefined(snapshot))
                    {
                        values.AddRange(staticField.ReadMemory(snapshot).PossibleValues);
                    }
                    var mmValue = res.ReadIndex(flow.OutSet.Snapshot, new MemberIdentifier(index));
                    values.AddRange(mmValue.ReadMemory(flow.OutSet.Snapshot).PossibleValues);
                    staticField.WriteMemory(snapshot, new MemoryEntry(values));
                    //store static variable
                }
            }
        }
    }

    abstract class NativeFunctionAnalyzerHelper
    {
        protected List<NativeFunction> nativeFunctions;
        protected static readonly VariableName returnVariable = new VariableName(".return");

        public NativeFunctionAnalyzerHelper(List<NativeFunction> nativeFunctions)
        {
            this.nativeFunctions = nativeFunctions;
        }

        protected abstract List<Value> ComputeResult(FlowController flow, List<MemoryEntry> arguments);

        public void analyze(FlowController flow)
        {
            // Check arguments
            if (NativeAnalyzerUtils.checkArgumentsCount(flow, nativeFunctions))
            {
                // TODO: what to do if some argument value does not match (it is not possible to call the function with value of such type)?
                // a) Remove this value?
                // b) If we keep the value there is a problem what to do if the functon is evaluated concretely using this value
                //      We can perform this check again before evaluationg the function and detect that the function should not be evaluated
                //          This is not good - dupolicates the check
                //      We can try to convert this value to the type supported by concrete function. If the conversion is not possible, do not call the function.
                //          Not very good - gives weird semantics.
                // Overall, a) is better choice, but it is not done yet.
                NativeAnalyzerUtils.checkArgumentTypes(flow, nativeFunctions);
            }

            // Get function arguments
            MemoryEntry argc = flow.InSet.ReadVariable(new VariableIdentifier(".argument_count")).ReadMemory(flow.OutSet.Snapshot);
            int argumentCount = ((IntegerValue)argc.PossibleValues.ElementAt(0)).Value;
            List<MemoryEntry> arguments = new List<MemoryEntry>();
            List<Value> argumentValues = new List<Value>();
            for (int i = 0; i < argumentCount; i++)
            {
                arguments.Add(flow.OutSet.ReadVariable(NativeAnalyzerUtils.Argument(i)).ReadMemory(flow.OutSet.Snapshot));
                argumentValues.AddRange(arguments.Last().PossibleValues);
            }

            // Compute result
            MemoryEntry functionResult = new MemoryEntry(ComputeResult(flow, arguments).ToArray());
            functionResult = new MemoryEntry(FlagsHandler.CopyFlags(argumentValues, functionResult.PossibleValues));
            flow.OutSet.GetLocalControlVariable(returnVariable).WriteMemory(flow.OutSet.Snapshot, functionResult);

            List<Value> assigned_aliases = NativeAnalyzerUtils.ResolveAliasArguments(flow, argumentValues, nativeFunctions);

        }

        protected List<Value> ComputeResultType(FlowController flow, List<MemoryEntry> arguments)
        {
            var argumentCount = arguments.Count;
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

            return possibleValues;
        }
    }

    delegate Value ConcreteFunctionDelegate(FlowController flow, Value[] arguments);

    class ConcreteFunctionAnalyzerHelper : NativeFunctionAnalyzerHelper
    {
        private bool containsAbstractValue;
        private ConcreteFunctionDelegate concreteFunction;

        public ConcreteFunctionAnalyzerHelper(List<NativeFunction> nativeFunctions, ConcreteFunctionDelegate concreteFunction)
            : base(nativeFunctions) 
        {
            this.concreteFunction = concreteFunction;
        }

        protected override List<Value> ComputeResult(FlowController flow, List<MemoryEntry> arguments)
        {
            var result = new List<Value>();
            containsAbstractValue = false;

            combination(flow, new List<Value>(), arguments, 0, result);

            if (containsAbstractValue)
            {
                result.AddRange(ComputeResultType(flow, arguments));
            }

            return result;
        }

        private void combination(FlowController flow, List<Value> argsValues, List<MemoryEntry> args, int pos, List<Value> result)
        {
            if (pos >= args.Count)
            {
                result.Add(concreteFunction(flow, argsValues.ToArray()));
                return;
            }

            foreach (var argValue in args.ElementAt(pos).PossibleValues)
            {
                if (argValue is AnyValue)
                {
                    containsAbstractValue = true;
                    continue;
                }

                var newArgsValues = new List<Value>(argsValues);
                newArgsValues.Add(argValue);
                combination(flow, newArgsValues, args, pos + 1, result);
            }
            return;
        }
    }

    class TypeModeledFunctionAnalyzerHelper : NativeFunctionAnalyzerHelper
    {
        public TypeModeledFunctionAnalyzerHelper(List<NativeFunction> nativeFunctions) : base(nativeFunctions) {}

        protected override List<Value> ComputeResult(FlowController flow, List<MemoryEntry> arguments)
        {
            return ComputeResultType(flow, arguments);
        }

        
    }

    class SpecialFunctionsImplementations
    {
        private List<NativeFunction> nativeFunctions;
        private static readonly VariableName returnVariable = new VariableName(".return");

        public SpecialFunctionsImplementations(List<NativeFunction> nativeFunctions)
        {
            this.nativeFunctions = nativeFunctions;
        }

        #region implementation of native php functions

        //todo unknown string - vytvori unknown costant pockat na podporu memory modelu
        public void _define(FlowController flow)
        {

            if (NativeAnalyzerUtils.checkArgumentsCount(flow, nativeFunctions))
            {
                NativeAnalyzerUtils.checkArgumentTypes(flow, nativeFunctions);
            }

            MemoryEntry argc = flow.InSet.ReadVariable(new VariableIdentifier(".argument_count")).ReadMemory(flow.OutSet.Snapshot);
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
                    foreach (var arg2 in flow.OutSet.ReadVariable(NativeAnalyzerUtils.Argument(2)).ReadMemory(flow.OutSet.Snapshot).PossibleValues)
                    {
                        var unaryVisitor = new UnaryOperationEvaluator(flow, new StringConverter(flow));
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
                foreach (var arg0 in flow.OutSet.ReadVariable(NativeAnalyzerUtils.Argument(0)).ReadMemory(flow.OutSet.Snapshot).PossibleValues)
                {
                    var stringConverter = new StringConverter(flow);
                    // TODO: arg0Retyped can be null if cannot be converted to StringValue
                    var arg0Retyped = stringConverter.EvaluateToString(arg0);
                    string constantName = "";

                    // TODO: It cannot never be null
                    /*
                    if (arg0Retyped is UndefinedValue)
                    {
                        canBeFalse = true;
                        continue;
                    }
                    else
                     */
                    {
                        constantName = arg0Retyped.Value;
                    }

                    QualifiedName qConstantName = new QualifiedName(new Name(constantName));
                    List<Value> result = new List<Value>();
                    foreach (var arg1 in flow.OutSet.ReadVariable(NativeAnalyzerUtils.Argument(1)).ReadMemory(flow.OutSet.Snapshot).PossibleValues)
                    {
                        if (ValueTypeResolver.IsArray(arg1) || ValueTypeResolver.IsObject(arg1))
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
            flow.OutSet.GetLocalControlVariable(returnVariable).WriteMemory(flow.OutSet.Snapshot, new MemoryEntry(possibleValues));
        }

        //todo unknown string - vytvori unknown costant pockat na podporu memory modelu
        public void _constant(FlowController flow)
        {
            if (NativeAnalyzerUtils.checkArgumentsCount(flow, nativeFunctions))
            {
                var stringConverter = new StringConverter(flow);

                foreach (var arg0 in flow.OutSet.ReadVariable(NativeAnalyzerUtils.Argument(0)).ReadMemory(flow.OutSet.Snapshot).PossibleValues)
                {
                    // TODO: arg0Retyped can be null if cannot be converted to StringValue
                    var arg0Retyped = stringConverter.EvaluateToString(arg0);
                    List<Value> values = new List<Value>();
                    NativeConstantAnalyzer constantAnalyzer = NativeConstantAnalyzer.Create(flow.OutSet);
                    QualifiedName name = new QualifiedName(new Name(arg0Retyped.Value));

                    if (constantAnalyzer.ExistContant(name))
                    {
                        values.Add(constantAnalyzer.GetConstantValue(name));
                    }
                    else
                    {
                        values = UserDefinedConstantHandler.GetConstant(flow.OutSet, name).PossibleValues.ToList();
                    }
                    flow.OutSet.GetLocalControlVariable(returnVariable).WriteMemory(flow.OutSet.Snapshot, new MemoryEntry(values));
                }
            }
            else
            {
                flow.OutSet.GetLocalControlVariable(returnVariable).WriteMemory(flow.OutSet.Snapshot, new MemoryEntry(flow.OutSet.UndefinedValue));
            }
        }


        delegate bool Typedelegate(Value value);

        public void _is_array(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.IsArray(value);
            }));
        }
        public void _is_bool(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.IsBool(value);
            }));
        }
        public void _is_double(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.IsFloat(value);
            }));
        }
        public void _is_int(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.IsInt(value) || ValueTypeResolver.IsLong(value);
            }));
        }
        public void _is_null(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return (value is UndefinedValue);
            }));
        }
        public void _is_numeric(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.IsInt(value) || ValueTypeResolver.IsLong(value) || ValueTypeResolver.IsFloat(value);
            }));
        }
        public void _is_object(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.IsObject(value);
            }));
        }
        public void _is_resource(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return (value is AnyResourceValue);
            }));
        }
        public void _is_scalar(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.IsInt(value) || ValueTypeResolver.IsLong(value) || ValueTypeResolver.IsFloat(value) || ValueTypeResolver.IsBool(value) || ValueTypeResolver.IsString(value); ;
            }));
        }
        public void _is_string(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.IsString(value);
            }));
        }


        private void processIsFunctions(FlowController flow, Typedelegate del)
        {
            if (NativeAnalyzerUtils.checkArgumentsCount(flow, nativeFunctions))
            {
                bool canBeTrue = false;
                bool canBeFalse = false;
                foreach (var arg0 in flow.OutSet.ReadVariable(NativeAnalyzerUtils.Argument(0)).ReadMemory(flow.OutSet.Snapshot).PossibleValues)
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

                flow.OutSet.GetLocalControlVariable(returnVariable).WriteMemory(flow.OutSet.Snapshot, new MemoryEntry(result));
            }
            else
            {
                flow.OutSet.GetLocalControlVariable(returnVariable).WriteMemory(flow.OutSet.Snapshot, new MemoryEntry(flow.OutSet.AnyBooleanValue));
            }
        }

        #endregion
    }

}
