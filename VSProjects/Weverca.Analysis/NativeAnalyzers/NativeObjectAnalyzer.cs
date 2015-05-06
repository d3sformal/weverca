/*
Copyright (c) 2012-2014 David Hauzar and Marcel Kikta

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using PHP.Core;

using Weverca.Analysis.Properties;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.NativeAnalyzers
{
    /// <summary>
    /// Sotred information about native method
    /// </summary>
    public class NativeMethod : NativeFunction
    {
        /// <summary>
        /// Indicates if method is static
        /// </summary>
        public bool IsStatic;

        /// <summary>
        /// Indicates if method is final
        /// </summary>
        public bool IsFinal;

        /// <summary>
        /// Indicates if method is abstract
        /// </summary>
        public bool IsAbstract = false;

        /// <summary>
        /// Default empty contructor
        /// </summary>
        public NativeMethod()
        {
        }

        /// <summary>
        /// Creates new instance of NativeMethod
        /// </summary>
        /// <param name="name">Method name</param>
        /// <param name="returnType">Return type</param>
        /// <param name="arguments">Method argument</param>
        public NativeMethod(QualifiedName name, string returnType, List<NativeFunctionArgument> arguments)
        {
            Name = name;
            Arguments = arguments;
            ReturnType = returnType;
            Analyzer = null;
        }

        /// <summary>
        /// Converts method arguments into imutable list of MethodArgument
        /// </summary>
        /// <returns>The list of converted arguments.</returns>
        public List<MethodArgument> ConvertArguments()
        {
            List<MethodArgument> res = new List<MethodArgument>();
            foreach (var arg in Arguments)
            {
                res.Add(new MethodArgument(new VariableName(), arg.ByReference, arg.Optional));
            }

            return res;
        }
    }


    /// <summary>
    /// Stores information about native object
    /// </summary>
    public class NativeObjectAnalyzer
    {

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static NativeObjectAnalyzer instance = null;

        /// <summary>
        /// Structure which stores all native objects
        /// </summary>
        private Dictionary<QualifiedName, ClassDecl> nativeObjects;

        /// <summary>
        /// Structure which stores all native objects, with concrete implementation
        /// </summary>
        private Dictionary<MethodIdentifier, FlagType> WevercaImplementedMethods
            = new Dictionary<MethodIdentifier, FlagType>();

        private HashSet<string> fieldTypes = new HashSet<string>();
        private HashSet<string> methodTypes = new HashSet<string>();
        private HashSet<string> returnTypes = new HashSet<string>();

        /// <summary>
        /// List of function which clean dirty floag in return value
        /// </summary>
        public static Dictionary<MethodIdentifier, List<FlagType>> CleaningFunctions = new Dictionary<MethodIdentifier, List<FlagType>>();

        /// <summary>
        /// List of function which reports security warning when it recieved dirty flag in input 
        /// </summary>
        public static Dictionary<MethodIdentifier, List<FlagType>> ReportingFunctions = new Dictionary<MethodIdentifier, List<FlagType>>();

        #region parsing xml

        /// <summary>
        /// Creates a new instance of NativeObjectAnalyzer. It parses the xml with information about the objects.
        /// </summary>
        /// <param name="outSet">FlowOutputSet</param>
        private NativeObjectAnalyzer(FlowOutputSet outSet)
        {
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(Resources.php_classes)))
            using (XmlReader reader = XmlReader.Create(stream))
            {
                nativeObjects = new Dictionary<QualifiedName, ClassDecl>();
                NativeObjectsAnalyzerHelper.mutableNativeObjects = new Dictionary<QualifiedName, ClassDeclBuilder>();

                Visibility methodVisibility = Visibility.PUBLIC;
                ClassDeclBuilder currentClass = null;
                NativeMethod currentMethod = null;
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            switch (reader.Name)
                            {
                                case "class":
                                case "interface":
                                    currentClass = new ClassDeclBuilder();
                                    var classFinal = reader.GetAttribute("isFinal");
                                    if (classFinal == "true")
                                    {
                                        currentClass.IsFinal = true;
                                    }
                                    else
                                    {
                                        currentClass.IsFinal = false;
                                    }
                                    currentClass.QualifiedName = new QualifiedName(new Name(reader.GetAttribute("name")));

                                    if (reader.GetAttribute("baseClass") != null)
                                    {
                                        currentClass.BaseClasses = new List<QualifiedName>();
                                        currentClass.BaseClasses.Add(new QualifiedName(new Name(reader.GetAttribute("baseClass"))));
                                    }
                                    else
                                    {
                                        currentClass.BaseClasses = null;
                                    }

                                    NativeObjectsAnalyzerHelper.mutableNativeObjects[currentClass.QualifiedName] = currentClass;
                                    if (reader.Name == "class")
                                    {
                                        currentClass.IsInterface = false;
                                    }
                                    else
                                    {
                                        currentClass.IsInterface = true;
                                    }

                                    break;
                                case "field":
                                    string fieldName = reader.GetAttribute("name");
                                    string fieldVisibility = reader.GetAttribute("visibility");
                                    string fieldIsStatic = reader.GetAttribute("isStatic");
                                    string fieldIsConst = reader.GetAttribute("isConst");
                                    string fieldType = reader.GetAttribute("type");
                                    fieldTypes.Add(fieldType);
                                    Visibility visibility;
                                    switch (fieldVisibility)
                                    {
                                        case "public":
                                            visibility = Visibility.PUBLIC;
                                            break;
                                        case "protectd":
                                            visibility = Visibility.PROTECTED;
                                            break;
                                        case "private":
                                            visibility = Visibility.PRIVATE;
                                            break;
                                        default:
                                            visibility = Visibility.PUBLIC;
                                            break;
                                    }

                                    if (fieldIsConst == "false")
                                    {

                                        Value initValue = outSet.UndefinedValue;
                                        string stringValue = reader.GetAttribute("value");
                                        int intValue;
                                        bool boolValue;
                                        long longValue;
                                        double doubleValue;

                                        if (stringValue == null)
                                        {
                                            initValue = outSet.UndefinedValue;
                                        }
                                        else if (bool.TryParse(stringValue, out boolValue))
                                        {
                                            initValue = outSet.CreateBool(boolValue);
                                        }
                                        else if (int.TryParse(stringValue, out intValue))
                                        {
                                            initValue = outSet.CreateInt(intValue);
                                        }
                                        else if (long.TryParse(stringValue, out longValue))
                                        {
                                            initValue = outSet.CreateLong(longValue);
                                        }
                                        else if (double.TryParse(stringValue, out doubleValue))
                                        {
                                            initValue = outSet.CreateDouble(doubleValue);
                                        }
                                        else
                                        {
                                            initValue = outSet.CreateString(stringValue);
                                        }

                                        currentClass.Fields[new FieldIdentifier(currentClass.QualifiedName, new VariableName(fieldName))] = new FieldInfo(new VariableName(fieldName), currentClass.QualifiedName, fieldType, visibility, new MemoryEntry(initValue), bool.Parse(fieldIsStatic));
                                    }
                                    else
                                    {
                                        string value = reader.GetAttribute("value");
                                        //resolve constant
                                        VariableName constantName = new VariableName(fieldName);

                                        var constIdentifier = new FieldIdentifier(currentClass.QualifiedName, constantName);
                                        switch (fieldType)
                                        {
                                            case "int":
                                            case "integer":
                                                try
                                                {
                                                    currentClass.Constants[constIdentifier] = new ConstantInfo(constantName, currentClass.QualifiedName, visibility, new MemoryEntry(outSet.CreateInt(int.Parse(value))));
                                                }
                                                catch (Exception)
                                                {
                                                    currentClass.Constants[constIdentifier] = new ConstantInfo(constantName, currentClass.QualifiedName, visibility, new MemoryEntry(outSet.CreateDouble(double.Parse(value))));
                                                }
                                                break;
                                            case "string":
                                                currentClass.Constants[constIdentifier] = new ConstantInfo(constantName, currentClass.QualifiedName, visibility, new MemoryEntry(outSet.CreateString(value)));
                                                break;
                                            case "boolean":
                                                currentClass.Constants[constIdentifier] = new ConstantInfo(constantName, currentClass.QualifiedName, visibility, new MemoryEntry(outSet.CreateBool(bool.Parse(value))));
                                                break;
                                            case "float":
                                                currentClass.Constants[constIdentifier] = new ConstantInfo(constantName, currentClass.QualifiedName, visibility, new MemoryEntry(outSet.CreateDouble(double.Parse(value))));
                                                break;
                                            case "NULL":
                                                currentClass.Constants[constIdentifier] = new ConstantInfo(constantName, currentClass.QualifiedName, visibility, new MemoryEntry(outSet.UndefinedValue));
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    break;
                                case "method":
                                    currentMethod = new NativeMethod(new QualifiedName(new Name(reader.GetAttribute("name"))), reader.GetAttribute("returnType"), new List<NativeFunctionArgument>());
                                    currentMethod.IsFinal = bool.Parse(reader.GetAttribute("final"));
                                    currentMethod.IsStatic = bool.Parse(reader.GetAttribute("static"));
                                    returnTypes.Add(reader.GetAttribute("returnType"));
                                    if (currentMethod.Name == new QualifiedName(new Name("getLastErrors")))
                                    {

                                    }
                                    methodVisibility = Visibility.PUBLIC;
                                    if (reader.GetAttribute("visibility") == "private")
                                    {
                                        methodVisibility = Visibility.PRIVATE;
                                    }
                                    else if (reader.GetAttribute("visibility") == "protected")
                                    {
                                        methodVisibility = Visibility.PROTECTED;
                                    }
                                    else
                                    {
                                        methodVisibility = Visibility.PUBLIC;
                                    }

                                    if (reader.IsEmptyElement)
                                    {
                                        NativeAnalyzerMethod analyzer;
                                        if (currentMethod.Name.Name.Equals(new Name("__construct")))
                                        {
                                            analyzer = new NativeObjectsAnalyzerHelper(currentMethod, currentClass.QualifiedName).Construct;
                                        }
                                        else
                                        {
                                            analyzer = new NativeObjectsAnalyzerHelper(currentMethod, currentClass.QualifiedName).Analyze;
                                        }
                                        MethodInfo methodInfo = new MethodInfo(currentMethod.Name.Name, currentClass.QualifiedName, methodVisibility, analyzer, currentMethod.ConvertArguments(), currentMethod.IsFinal, currentMethod.IsStatic, currentMethod.IsAbstract);
                                        currentClass.ModeledMethods[new MethodIdentifier(currentClass.QualifiedName, methodInfo.Name)] = methodInfo;
                                    }

                                    break;
                                case "arg":
                                    currentMethod.Arguments.Add(new NativeFunctionArgument(reader.GetAttribute("type"), bool.Parse(reader.GetAttribute("optional")), bool.Parse(reader.GetAttribute("byReference")), bool.Parse(reader.GetAttribute("dots"))));
                                    methodTypes.Add(reader.GetAttribute("type"));
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
                                case "method":

                                    NativeAnalyzerMethod analyzer;
                                    if (currentMethod.Name.Name.Equals(new Name("__construct")))
                                    {
                                        analyzer = new NativeObjectsAnalyzerHelper(currentMethod, currentClass.QualifiedName).Construct;
                                    }
                                    else
                                    {
                                        analyzer = new NativeObjectsAnalyzerHelper(currentMethod, currentClass.QualifiedName).Analyze;
                                    }
                                    MethodInfo methodInfo = new MethodInfo(currentMethod.Name.Name, currentClass.QualifiedName, methodVisibility, analyzer, currentMethod.ConvertArguments(), currentMethod.IsFinal, currentMethod.IsStatic, currentMethod.IsAbstract);
                                    currentClass.ModeledMethods[new MethodIdentifier(currentClass.QualifiedName, methodInfo.Name)] = methodInfo;


                                    break;
                            }

                            break;
                    }
                }
            }
            /*
            check if every ancestor exist 
             */
            foreach (var nativeObject in NativeObjectsAnalyzerHelper.mutableNativeObjects.Values)
            {
                if (nativeObject.QualifiedName != null)
                {
                    if (!NativeObjectsAnalyzerHelper.mutableNativeObjects.ContainsKey(nativeObject.QualifiedName))
                    {
                        throw new Exception();
                    }
                }
            }

            //generate result
            foreach (var nativeObject in NativeObjectsAnalyzerHelper.mutableNativeObjects.Values)
            {
                List<QualifiedName> newBaseClasses = new List<QualifiedName>();
                if (nativeObject.BaseClasses != null)
                {
                    QualifiedName baseClassName = nativeObject.BaseClasses.Last();
                    while (true)
                    {
                        newBaseClasses.Add(baseClassName);
                        var baseClass = NativeObjectsAnalyzerHelper.mutableNativeObjects[baseClassName];
                        foreach (var constant in baseClass.Constants)
                        {
                            nativeObject.Constants[constant.Key] = constant.Value;
                        }
                        foreach (var field in baseClass.Fields)
                        {
                            nativeObject.Fields[field.Key] = field.Value;
                        }
                        foreach (var method in baseClass.ModeledMethods)
                        {
                            nativeObject.ModeledMethods[method.Key] = method.Value;
                        }
                        if (baseClass.BaseClasses != null && baseClass.BaseClasses.Count > 0)
                        {
                            baseClassName = baseClass.BaseClasses.Last();
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                newBaseClasses.Reverse();
                nativeObject.BaseClasses = newBaseClasses;
                nativeObjects[nativeObject.QualifiedName] = nativeObject.Build();



            }
            initReportingFunctions();
            initCleaningFunctions();
        }

        private QualifiedName getQualifiedName(string s)
        {
            return new QualifiedName(new Name(s));
        }

        private List<FlagType> getList(params FlagType[] types)
        {
            var result = new List<FlagType>();
            foreach (var type in types)
            {
                result.Add(type);
            }
            return result;

        }

        private void initCleaningFunctions()
        {
            CleaningFunctions.Add(new MethodIdentifier(getQualifiedName("mysqli"), new Name("escape_string")), getList(FlagType.SQLDirty));

            CleaningFunctions.Add(new MethodIdentifier(getQualifiedName("MysqlndUhConnection"), new Name("escapeString")), getList(FlagType.SQLDirty));

            CleaningFunctions.Add(new MethodIdentifier(getQualifiedName("SQLite3"), new Name("escapeString")), getList(FlagType.SQLDirty));

        }

        private void initReportingFunctions()
        {
            ReportingFunctions.Add(new MethodIdentifier(getQualifiedName("mysqli"), new Name("query")), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(new MethodIdentifier(getQualifiedName("mysqli"), new Name("real_query")), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(new MethodIdentifier(getQualifiedName("mysqli"), new Name("send_query")), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(new MethodIdentifier(getQualifiedName("mysqli"), new Name("change_user")), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(new MethodIdentifier(getQualifiedName("mysqli"), new Name("select_db")), getList(FlagType.SQLDirty));

            ReportingFunctions.Add(new MethodIdentifier(getQualifiedName("mysqli_stmt"), new Name("execute")), getList(FlagType.SQLDirty));

            ReportingFunctions.Add(new MethodIdentifier(getQualifiedName("MysqlndUhConnection"), new Name("query")), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(new MethodIdentifier(getQualifiedName("MysqlndUhConnection"), new Name("sendQuery")), getList(FlagType.SQLDirty));

            ReportingFunctions.Add(new MethodIdentifier(getQualifiedName("MysqlndUhPreparedStatement"), new Name("execute")), getList(FlagType.SQLDirty));

            ReportingFunctions.Add(new MethodIdentifier(getQualifiedName("SQLite3"), new Name("exec")), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(new MethodIdentifier(getQualifiedName("SQLite3"), new Name("query")), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(new MethodIdentifier(getQualifiedName("SQLite3"), new Name("querySingle")), getList(FlagType.SQLDirty));

            ReportingFunctions.Add(new MethodIdentifier(getQualifiedName("SQLite3Stmt"), new Name("execute")), getList(FlagType.SQLDirty));

            ReportingFunctions.Add(new MethodIdentifier(getQualifiedName("mysqli"), new Name("__construct")), getList(FlagType.SQLDirty));
        }
        #endregion

        /// <summary>
        /// Return singleton instance of NativeObjectAnalyzer
        /// </summary>
        /// <param name="outSet">FlowOutputSet</param>
        /// <returns>singleton instance of NativeObjectAnalyzer</returns>
        public static NativeObjectAnalyzer GetInstance(FlowOutputSet outSet)
        {
            if (instance == null)
            {
                instance = new NativeObjectAnalyzer(outSet);
            }

            return instance;
        }

        /// <summary>
        /// Indicates if the class exist
        /// </summary>
        /// <param name="className">Class name</param>
        /// <returns>true if class exist, false otherwise</returns>
        public bool ExistClass(QualifiedName className)
        {
            return nativeObjects.ContainsKey(className);
        }

        /// <summary>
        /// Return native class
        /// </summary>
        /// <param name="className">Class name</param>
        /// <returns>native class</returns>
        public ClassDecl GetClass(QualifiedName className)
        {
            return nativeObjects[className];
        }

        /// <summary>
        /// Tries to get class and return in second parameter.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="declaration">out parameter with result</param>
        /// <returns>true if class exists</returns>
        public bool TryGetClass(QualifiedName className, out ClassDecl declaration)
        {
            return nativeObjects.TryGetValue(className, out declaration);
        }

        /// <summary>
        /// Returns all classes
        /// </summary>
        /// <returns>all classes</returns>
        public IEnumerable<ClassDecl> GetAllClasses()
        {
            return nativeObjects.Values;
        }
    }

    /// <summary>
    /// Helper for native metod analyzer
    /// </summary>
    internal class NativeObjectsAnalyzerHelper
    {
        /// <summary>
        /// Method information
        /// </summary>
        private NativeMethod Method;

        /// <summary>
        /// Class name
        /// </summary>
        private QualifiedName ObjectName;

        /// <summary>
        /// Structure with muttable native objects
        /// </summary>
        public static Dictionary<QualifiedName, ClassDeclBuilder> mutableNativeObjects;

        /// <summary>
        /// Creates instance of NativeObjectsAnalyzerHelper
        /// </summary>
        /// <param name="method">Method name</param>
        /// <param name="objectName">Class name</param>
        public NativeObjectsAnalyzerHelper(NativeMethod method, QualifiedName objectName)
        {
            Method = method;
            ObjectName = objectName;
        }

        /// <summary>
        /// Models a called method.
        /// </summary>
        /// <param name="flow"></param>
        public void Analyze(FlowController flow)
        {
            if (NativeAnalyzerUtils.checkArgumentsCount(flow, Method))
            {
                NativeAnalyzerUtils.checkArgumentTypes(flow, Method);
            }
            MethodIdentifier methodIdentifier = new MethodIdentifier(ObjectName, Method.Name.Name);
            var nativeClass = NativeObjectAnalyzer.GetInstance(flow.OutSet).GetClass(ObjectName);
            var fields = nativeClass.Fields;

            var functionResult = NativeAnalyzerUtils.ResolveReturnValue(Method.ReturnType, flow);
            var arguments = getArguments(flow);

            var thisVariable = flow.OutSet.GetVariable(new VariableIdentifier("this"));
            List<Value> inputValues = new List<Value>();
            bool isStaticCall = false;

            if (thisVariable.ReadMemory(flow.OutSet.Snapshot).PossibleValues.Count() == 1 && thisVariable.ReadMemory(flow.OutSet.Snapshot).PossibleValues.First() is UndefinedValue)
            {
                isStaticCall = true;
                var fieldsEntries = new List<MemoryEntry>();

                foreach (FieldInfo field in fields.Values)
                {
                    var fieldEntry = thisVariable.ReadField(flow.OutSet.Snapshot, new VariableIdentifier(field.Name.Value));
                    fieldsEntries.Add(fieldEntry.ReadMemory(flow.OutSet.Snapshot));
                    inputValues.AddRange(fieldEntry.ReadMemory(flow.OutSet.Snapshot).PossibleValues);
                }

                fieldsEntries.AddRange(arguments);

                foreach (FieldInfo field in fields.Values)
                {
                    var fieldEntry = thisVariable.ReadField(flow.OutSet.Snapshot, new VariableIdentifier(field.Name.Value));
                    MemoryEntry newfieldValues = NativeAnalyzerUtils.ResolveReturnValue(field.Type, flow);
                    newfieldValues = new MemoryEntry(FlagsHandler.CopyFlags(inputValues, newfieldValues.PossibleValues));
                    HashSet<Value> newValues = new HashSet<Value>((fieldEntry.ReadMemory(flow.OutSet.Snapshot)).PossibleValues);
                    foreach (var newValue in newfieldValues.PossibleValues)
                    {
                        newValues.Add(newValue);
                    }
                    thisVariable.ReadField(flow.OutSet.Snapshot, new VariableIdentifier(field.Name.Value)).WriteMemory(flow.OutSet.Snapshot, new MemoryEntry(newValues));
                }
            }

            foreach (var argument in arguments)
            {
                inputValues.AddRange(argument.PossibleValues);
            }

            if (NativeObjectAnalyzer.ReportingFunctions.ContainsKey(methodIdentifier))
            {
                foreach (var flag in NativeObjectAnalyzer.ReportingFunctions[methodIdentifier])
                {
                    if (FlagsHandler.IsDirty(inputValues, flag))
                    {
                        AnalysisWarningHandler.SetWarning(flow.OutSet, new AnalysisSecurityWarning(NativeAnalyzerUtils.GetCallerScript(flow.OutSet), flow.CurrentPartial, flow.CurrentProgramPoint, flag, methodIdentifier.Name.ToString()));
                        break;
                    }
                }
            }

            if (isStaticCall == true)
            {
                thisVariable.WriteMemory(flow.OutSet.Snapshot, new MemoryEntry(FlagsHandler.CopyFlags(inputValues, thisVariable.ReadMemory(flow.OutSet.Snapshot).PossibleValues)));
            }

            functionResult = new MemoryEntry(FlagsHandler.CopyFlags(inputValues, functionResult.PossibleValues));
            if (NativeObjectAnalyzer.CleaningFunctions.ContainsKey(methodIdentifier))
            {
                foreach (var flag in NativeObjectAnalyzer.CleaningFunctions[methodIdentifier])
                {
                    functionResult = new MemoryEntry(FlagsHandler.Clean(functionResult.PossibleValues, flag));
                }
            }
            flow.OutSet.GetLocalControlVariable(SnapshotBase.ReturnValue).WriteMemory(flow.OutSet.Snapshot, functionResult);

            var assigned_aliases = NativeAnalyzerUtils.ResolveAliasArguments(flow, inputValues, (new NativeFunction[1] { Method }).ToList());
        }

        /// <summary>
        /// Models constructor of native object
        /// </summary>
        /// <param name="flow">FlowController</param>
        public void Construct(FlowController flow)
        {
            if (NativeAnalyzerUtils.checkArgumentsCount(flow, Method))
            {
                NativeAnalyzerUtils.checkArgumentTypes(flow, Method);
            }

            initObject(flow);
        }

        /// <summary>
        /// Models constructor of native object
        /// </summary>
        /// <param name="flow">FlowController</param>
        public void initObject(FlowController flow)
        {
            var nativeClass = NativeObjectAnalyzer.GetInstance(flow.OutSet).GetClass(ObjectName);
            var fields = nativeClass.Fields;

            var thisVariable = flow.OutSet.GetVariable(new VariableIdentifier("this"));
            List<Value> inputValues = new List<Value>();

            foreach (var argument in getArguments(flow))
            {
                inputValues.AddRange(argument.PossibleValues);
            }
            MethodIdentifier methodIdentifier = new MethodIdentifier(ObjectName, Method.Name.Name);
            if (NativeObjectAnalyzer.ReportingFunctions.ContainsKey(methodIdentifier))
            {
                foreach (var flag in NativeObjectAnalyzer.ReportingFunctions[methodIdentifier])
                {
                    if (FlagsHandler.IsDirty(inputValues, flag))
                    {
                        AnalysisWarningHandler.SetWarning(flow.OutSet, new AnalysisSecurityWarning(NativeAnalyzerUtils.GetCallerScript(flow.OutSet), flow.CurrentPartial, flow.CurrentProgramPoint, flag, methodIdentifier.Name.ToString()));
                        break;
                    }
                }
            }
            
            foreach (FieldInfo field in fields.Values)
            {
                if (field.IsStatic == false)
                {
                    MemoryEntry fieldValues = NativeAnalyzerUtils.ResolveReturnValue(field.Type, flow);
                    fieldValues = new MemoryEntry(FlagsHandler.CopyFlags(inputValues, fieldValues.PossibleValues));
                    var fieldEntry = thisVariable.ReadField(flow.OutSet.Snapshot, new VariableIdentifier(field.Name.Value));
                    fieldEntry.WriteMemory(flow.OutSet.Snapshot, fieldValues);
                }
            }


        }

        /// <summary>
        /// Reads all arguments and returns them in List
        /// </summary>
        /// <param name="flow">FlowController</param>
        /// <returns>List of arguments</returns>
        private List<MemoryEntry> getArguments(FlowController flow)
        {
            MemoryEntry argc = flow.InSet.ReadVariable(new VariableIdentifier(".argument_count")).ReadMemory(flow.OutSet.Snapshot);
            int argumentCount = ((IntegerValue)argc.PossibleValues.ElementAt(0)).Value;
            var arguments = new List<MemoryEntry>();

            for (int i = 0; i < argumentCount; i++)
            {
                arguments.Add(flow.OutSet.ReadVariable(NativeAnalyzerUtils.Argument(i)).ReadMemory(flow.OutSet.Snapshot));
            }

            return arguments;
        }
    }
}