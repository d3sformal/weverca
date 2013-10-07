using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis;
using Weverca.Analysis.Expressions;
using Weverca.Analysis.Memory;
using Weverca.Analysis.ProgramPoints;

namespace Weverca.TaintedAnalysis
{
    /// <summary>
    /// Resolving function names and function initializing
    /// </summary>
    public class FunctionResolver : FunctionResolverBase
    {
        private static readonly VariableName currentFunctionName = new VariableName("$current_function");

        private NativeFunctionAnalyzer nativeFunctionAnalyzer = NativeFunctionAnalyzer.CreateInstance();
        private Dictionary<MethodDecl, FunctionHints> methods = new Dictionary<MethodDecl, FunctionHints>();
        private Dictionary<FunctionDecl, FunctionHints> functions
            = new Dictionary<FunctionDecl, FunctionHints>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionResolver" /> class.
        /// </summary>
        public FunctionResolver()
        {
        }

        #region FunctionResolverBase overrides

        public override void MethodCall(MemoryEntry calledObject, QualifiedName name,
            MemoryEntry[] arguments)
        {
            var objectValues = resolveObjectsForMember(calledObject);
            var methods = resolveMethod(objectValues, name, arguments);
            setCallBranching(methods);
        }

        public override void Call(QualifiedName name, MemoryEntry[] arguments)
        {
            var functions = resolveFunction(name, arguments);
            setCallBranching(functions);
        }

        public override void IndirectMethodCall(MemoryEntry calledObject, MemoryEntry name,
            MemoryEntry[] arguments)
        {
            var methods = new Dictionary<object, FunctionValue>();
            var methodNames = getSubroutineNames(name);
            var objectValues = resolveObjectsForMember(calledObject);

            foreach (var functionName in methodNames)
            {
                var resolvedMethods = resolveMethod(objectValues, functionName, arguments);
                foreach (var resolvedMethod in resolvedMethods)
                {
                    methods[resolvedMethod.Key] = resolvedMethod.Value;
                }
            }

            setCallBranching(methods);
        }

        public override void IndirectCall(MemoryEntry name, MemoryEntry[] arguments)
        {
            var functions = new Dictionary<object, FunctionValue>();
            var functionNames = getSubroutineNames(name);

            foreach (var functionName in functionNames)
            {
                var resolvedFunctions = resolveFunction(functionName, arguments);
                foreach (var resolvedFunction in resolvedFunctions)
                {
                    functions[resolvedFunction.Key] = resolvedFunction.Value;
                }
            }

            setCallBranching(functions);
        }

        /// <summary>
        /// Initialize call into callInput.
        /// NOTE:
        ///     arguments has to be initialized
        ///     sharing program point graphs is possible
        /// </summary>
        /// <param name="callInput">Input of initialized call</param>
        /// <param name="extensionGraph">Graph representing initialized call</param>
        /// <param name="arguments">Call arguments</param>
        public override void InitializeCall(FlowOutputSet callInput, ProgramPointGraph extensionGraph,
            MemoryEntry[] arguments)
        {
            var declaration = extensionGraph.SourceObject;
            var signature = getSignature(declaration);
            var hasNamedSignature = signature.HasValue;

            if (hasNamedSignature)
            {
                // We have names for passed arguments
                setNamedArguments(callInput, arguments, signature.Value);
            }
            else
            {
                // There are no names - use numbered arguments
                setOrderedArguments(callInput, arguments);
            }

            var functionDeclaration = declaration as FunctionDecl;
            if (functionDeclaration != null)
            {
                callInput.Assign(currentFunctionName,
                    new MemoryEntry(callInput.CreateFunction(functionDeclaration)));
            }
            else
            {
                var methodDeclaration = declaration as MethodDecl;
                if (methodDeclaration != null)
                {
                    callInput.Assign(currentFunctionName,
                        new MemoryEntry(callInput.CreateFunction(methodDeclaration)));
                }
            }
        }

        public override MemoryEntry InitializeObject(MemoryEntry newObject, MemoryEntry[] arguments)
        {
            Flow.CalledObject = newObject;
            Flow.Arguments = arguments;

            var constructorName = new QualifiedName(new Name("__construct"));
            var objectValues = new List<ObjectValue>();

            foreach (var value in newObject.PossibleValues)
            {
                Debug.Assert(value is ObjectValue, "All objects are creating now");
                objectValues.Add(value as ObjectValue);
            }

            var constructors = resolveMethod(objectValues, constructorName, arguments);
            if (constructors.Count > 0)
            {
                setCallBranching(constructors);
            }

            return newObject;
        }

        /// <summary>
        /// Resolve return value from all possible calls. It also applies user hints for flags removal
        /// </summary>
        /// <param name="callGraphs">All calls on dispatch level, which return value is resolved</param>
        /// <returns>Resolved return value</returns>
        public override MemoryEntry ResolveReturnValue(IEnumerable<ProgramPointGraph> callGraphs)
        {
            var calls = callGraphs.ToArray();

            if (calls.Length == 1)
            {
                var outSet = calls[0].End.OutSet;
                applyHints(outSet);
                return outSet.ReadValue(outSet.ReturnValue);
            }
            else
            {
                Debug.Assert(calls.Length > 0, "There must be at least one call");

                var values = new HashSet<Value>();
                foreach (var call in calls)
                {
                    var outSet = call.End.OutSet;
                    applyHints(outSet);
                    var returnValue = outSet.ReadValue(outSet.ReturnValue);
                    values.UnionWith(returnValue.PossibleValues);
                }

                return new MemoryEntry(values);
            }
        }

        public override void DeclareGlobal(TypeDecl declaration)
        {
            var objectAnalyzer = NativeObjectAnalyzer.GetInstance(Flow);
            if (objectAnalyzer.ExistClass(declaration.Type.QualifiedName))
            {
                // TODO: This must be fatal error
                setWarning("Cannot redeclare class");
            }
            else
            {
                //TODO copy stuf into
                if (declaration.BaseClassName != null)
                {
                    if (objectAnalyzer.ExistClass(declaration.BaseClassName.Value.QualifiedName))
                    {
                        NativeTypeDecl baseClass = objectAnalyzer.GetClass(declaration.BaseClassName.Value.QualifiedName);
                    }
                    else
                    {
                        IEnumerable<TypeValue> types = OutSet.ResolveType(declaration.BaseClassName.Value.QualifiedName);
                    }

                }
                else
                {
                    var type = OutSet.CreateType(declaration);
                    OutSet.DeclareGlobal(type);
                }
            }
        }

        #endregion

        #region Private helpers

        private void applyHints(FlowOutputSet outSet)
        {
            var currentFunctionEntry = outSet.ReadValue(currentFunctionName);
            if (currentFunctionEntry.Count != 1)
            {
                return;
            }

            var enumerator = currentFunctionEntry.PossibleValues.GetEnumerator();
            enumerator.MoveNext();
            var currentFunction = enumerator.Current as FunctionValue;

            if (currentFunction != null)
            {
                var functionDeclaration = currentFunction.DeclaringElement as FunctionDecl;
                if (functionDeclaration != null)
                {
                    if (!functions.ContainsKey(functionDeclaration))
                    {
                        functions.Add(functionDeclaration,
                            new FunctionHints(functionDeclaration.PHPDoc, functionDeclaration));
                    }

                    functions[functionDeclaration].applyHints(outSet);
                }
                else
                {
                    var methodDeclaration = currentFunction.DeclaringElement as MethodDecl;
                    if (methodDeclaration != null)
                    {
                        if (!methods.ContainsKey(methodDeclaration))
                        {
                            methods.Add(methodDeclaration,
                                new FunctionHints(methodDeclaration.PHPDoc, methodDeclaration));
                        }

                        methods[methodDeclaration].applyHints(outSet);
                    }
                }
            }
        }

        private void setWarning(string message)
        {
            AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(message, Element));
        }

        private void setWarning(string message, AnalysisWarningCause cause)
        {
            AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(message, Element, cause));
        }

        /// <summary>
        /// Get storage for argument at given index
        /// NOTE:
        ///     Is used only for native analyzers in Simple analysis
        /// </summary>
        /// <param name="index">Index of argument at given storage</param>
        /// <returns>Storage for argument at given index</returns>
        private static VariableName argument(int index)
        {
            if (index < 0)
            {
                throw new NotSupportedException("Cannot get argument variable for negative index");
            }

            return new VariableName(".arg" + index);
        }

        private Signature? getSignature(LangElement declaration)
        {
            // TODO: Resolving via visitor might be better
            var methodDeclaration = declaration as MethodDecl;
            if (methodDeclaration != null)
            {
                return methodDeclaration.Signature;
            }
            else
            {
                var functionDeclaration = declaration as FunctionDecl;
                if (functionDeclaration != null)
                {
                    return functionDeclaration.Signature;
                }
            }

            return null;
        }

        private void setCallBranching(Dictionary<object, FunctionValue> functions)
        {
            foreach (var branchKey in Flow.ExtensionKeys)
            {
                if (!functions.Remove(branchKey))
                {
                    // Now this call is not resolved as possible call branch
                    Flow.RemoveExtension(branchKey);
                }
            }

            foreach (var function in functions.Values)
            {
                // Create graph for every function - NOTE: We can share pp graphs
                var ppGraph = ProgramPointGraph.From(function);
                Flow.AddExtension(function.DeclaringElement, ppGraph);
            }
        }

        /// <summary>
        /// Resolving names according to given memory entry
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        private List<QualifiedName> getSubroutineNames(MemoryEntry functionName)
        {
            var names = new HashSet<string>();
            foreach (var possibleValue in functionName.PossibleValues)
            {
                var stringValue = possibleValue as StringValue;
                // TODO: Other values convert to string
                if (stringValue == null)
                {
                    continue;
                }

                names.Add(stringValue.Value);
            }

            var qualifiedNames = new List<QualifiedName>(names.Count);
            foreach (var name in names)
            {
                qualifiedNames.Add(new QualifiedName(new Name(name)));
            }

            return qualifiedNames;
        }

        private Dictionary<object, FunctionValue> resolveFunction(QualifiedName name,
            MemoryEntry[] arguments)
        {
            var result = new Dictionary<object, FunctionValue>();

            if (nativeFunctionAnalyzer.existNativeFunction(name))
            {
                var function = OutSet.CreateFunction(name.Name,
                    new NativeAnalyzer(nativeFunctionAnalyzer.getNativeAnalyzer(name), Flow.CurrentPartial));
                // TODO: Check whether the number of arguments match.
                result[function.DeclaringElement] = function;
            }
            else
            {
                var functions = OutSet.ResolveFunction(name);
                // TODO: Test if functions.Count > 0

                foreach (var function in functions)
                {
                    // TODO: Check whether the number of arguments match.
                    result[function.DeclaringElement] = function;
                }
            }

            return result;
        }

        private Dictionary<object, FunctionValue> resolveMethod(IEnumerable<ObjectValue> objects,
            QualifiedName name, MemoryEntry[] arguments)
        {
            var result = new Dictionary<object, FunctionValue>();

            foreach (var objectValue in objects)
            {
                var functions = OutSet.ResolveMethod(objectValue, name);
                foreach (var function in functions)
                {
                    // TODO: Check whether the number of arguments match.
                    result[function.DeclaringElement] = function;
                }
            }

            return result;
        }

        private List<ObjectValue> resolveObjectsForMember(MemoryEntry entry)
        {
            var isPossibleNonObject = false;
            var objectValues = resolveObjectsForMember(entry, out isPossibleNonObject);

            if (isPossibleNonObject)
            {
                if (objectValues.Count >= 1)
                {
                    // TODO: This must be fatal error
                    setWarning("Possible call to a member function on a non-object",
                        AnalysisWarningCause.METHOD_CALL_ON_NON_OBJECT_VARIABLE);
                }
                else
                {
                    // TODO: This must be fatal error
                    setWarning("Call to a member function on a non-object",
                        AnalysisWarningCause.METHOD_CALL_ON_NON_OBJECT_VARIABLE);
                }
            }

            return objectValues;
        }

        private static List<ObjectValue> resolveObjectsForMember(MemoryEntry entry,
            out bool isPossibleNonObject)
        {
            var objectValues = new List<ObjectValue>();
            isPossibleNonObject = false;

            foreach (var variableValue in entry.PossibleValues)
            {
                // TODO: Inside method, $this variable is an object, otherwise a runtime error has occurred.
                // The problem is that we do not know the name of variable and we cannot detect it.

                var objectInstance = variableValue as ObjectValue;
                if (objectInstance != null)
                {
                    objectValues.Add(objectInstance);
                }
                else
                {
                    if (!isPossibleNonObject)
                    {
                        isPossibleNonObject = true;
                    }
                }
            }

            return objectValues;
        }

        private void setNamedArguments(FlowOutputSet callInput, MemoryEntry[] arguments, Signature signature)
        {
            var callPoint = Flow.ProgramPoint as RCallPoint;
            var callSignature = callPoint.CallSignature;
            var enumerator = callPoint.Arguments.GetEnumerator();

            for (int i = 0; i < signature.FormalParams.Count; ++i)
            {
                enumerator.MoveNext();

                var param = signature.FormalParams[i];
                var callParam = callSignature.Value.Parameters[i];

                if (callParam.PublicAmpersand)
                {
                    var aliasProvider = enumerator.Current as AliasProvider;
                    callInput.AssignAliases(param.Name, aliasProvider.CreateAlias(Flow));
                }
                else
                {
                    callInput.Assign(param.Name, arguments[i]);
                }
            }
        }

        private void setOrderedArguments(FlowOutputSet callInput, MemoryEntry[] arguments)
        {
            var argCount = callInput.CreateInt(arguments.Length);
            callInput.Assign(new VariableName(".argument_count"), argCount);

            var index = 0;
            var callPoint = Flow.ProgramPoint as RCallPoint;
            foreach (var arg in callPoint.Arguments)
            {
                var parVar = argument(index);

                // Determine that argument value is based on variable, so we can get it's alias
                var aliasProvider = arg as AliasProvider;
                if (aliasProvider == null)
                {
                    // Assign value for parameter
                    callInput.Assign(parVar, arguments[index]);
                }
                else
                {
                    // Join parameter with alias (for testing we join all possible arguments)
                    // Be carefull here - Flow.OutSet belongs to call context already - so we has to read variable from InSet
                    callInput.AssignAliases(parVar, aliasProvider.CreateAlias(Flow));
                }
                ++index;
            }
        }

        #endregion
    }

    // TODO: testy treba pockat na priznaky
    internal class FunctionHints
    {
        private HashSet<DirtyType> returnHints;
        private Dictionary<VariableName, HashSet<DirtyType>> argumentHints;
        private LangElement declaration;

        internal FunctionHints(PHPDocBlock doc, LangElement langElement)
        {
            declaration = langElement;
            argumentHints = new Dictionary<VariableName, HashSet<DirtyType>>();
            returnHints = new HashSet<DirtyType>();

            string comment;
            if (doc == null)
            {
                comment = string.Empty;
            }
            else
            {
                comment = doc.ToString();
            }

            List<FormalParam> parameters = null;
            if (declaration is MethodDecl)
            {
                parameters = (declaration as MethodDecl).Signature.FormalParams;
            }
            else if (declaration is FunctionDecl)
            {
                parameters = (declaration as FunctionDecl).Signature.FormalParams;
            }

            var endOfRegexp = "(";
            var values = DirtyType.GetValues(typeof(DirtyType));
            foreach (DirtyType val in values)
            {
                endOfRegexp += val + "|";
            }

            endOfRegexp += "all)";
            var returnPatern = "^[ \t]*\\*?[ \t]*@wev-hint[ \t]+returnvalue[ \t]+remove[ \t]+" + endOfRegexp;
            var argumentPatern = "^[ \t]*\\*?[ \t]*@wev-hint[ \t]+outargument[ \t]+([a-zA-Z_\x7f-\xff][a-zA-Z0-9_\x7f-\xff]*)[ \t]+remove[ \t]+" + endOfRegexp;
            var retRegEx = new Regex(returnPatern, RegexOptions.IgnoreCase);
            var argRegEx = new Regex(argumentPatern, RegexOptions.IgnoreCase);

            foreach (var line in comment.Split('\n'))
            {
                var match = retRegEx.Match(line);
                if (match.Success)
                {
                    var res = match.Groups[1].Value.ToString();
                    foreach (DirtyType val in values)
                    {
                        if (val.ToString().ToLower() == res.ToString().ToLower())
                        {
                            addReturnHint(val);
                        }

                        if (res == "all")
                        {
                            addReturnHint(val);
                        }
                    }
                }

                var argMatch = argRegEx.Match(line);
                if (argMatch.Success)
                {
                    var argName = argMatch.Groups[1].Value;
                    var res = argMatch.Groups[2].Value.ToString();
                    foreach (var parameter in parameters)
                    {
                        if (parameter.Name.Equals(argName))
                        {
                            foreach (DirtyType val in values)
                            {
                                if (val.ToString().ToLower() == res.ToString().ToLower())
                                {
                                    addArgumentHint(new VariableName(argName), val);
                                }

                                if (res == "all")
                                {
                                    addArgumentHint(new VariableName(argName), val);
                                }
                            }

                            break;
                        }
                    }
                }
            }
        }

        private void addReturnHint(DirtyType type)
        {
            returnHints.Add(type);
        }

        private void addArgumentHint(VariableName name, DirtyType type)
        {
            if (!argumentHints.ContainsKey(name))
            {
                argumentHints[name] = new HashSet<DirtyType>();
            }

            argumentHints[name].Add(type);
        }

        internal void applyHints(FlowOutputSet outset)
        {
            foreach (var type in returnHints)
            {
                var result = outset.ReadValue(outset.ReturnValue);
                foreach (var value in result.PossibleValues)
                {
                    ValueInfoHandler.setClean(outset, value, type);
                }
            }

            foreach (var variable in argumentHints.Keys)
            {
                foreach (var flag in argumentHints[variable])
                {
                    var result = outset.ReadValue(variable);
                    foreach (var value in result.PossibleValues)
                    {
                        ValueInfoHandler.setClean(outset, value, flag);
                    }
                }
            }
        }
    }
}
