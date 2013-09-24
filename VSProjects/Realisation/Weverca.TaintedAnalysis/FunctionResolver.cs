using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

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
        private NativeFunctionAnalyzer nativeFunctionAnalyzer = NativeFunctionAnalyzer.CreateInstance();
  
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionResolver" /> class.
        /// </summary>
        public FunctionResolver()
        {
        }

        #region FunctionResolverBase overrides

        public override void MethodCall(MemoryEntry calledObject, QualifiedName name, MemoryEntry[] arguments)
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

        public override void IndirectMethodCall(MemoryEntry calledObject, MemoryEntry name, MemoryEntry[] arguments)
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
        /// <param name="callInput"></param>
        /// <param name="declaration"></param>
        /// <param name="arguments"></param>
        public override void InitializeCall(FlowOutputSet callInput, ProgramPointGraph extensionGraph, MemoryEntry[] arguments)
        {
            var declaration = extensionGraph.SourceObject;
            var signature = getSignature(declaration);
            var hasNamedSignature = signature.HasValue;

            if (hasNamedSignature)
            {
                //we have names for passed arguments
                setNamedArguments(callInput, arguments, signature.Value);
            }
            else
            {
                //there are no names - use numbered arguments
                setOrderedArguments(callInput, arguments);
            }
            if (declaration is FunctionDecl)
            {
                callInput.Assign(new VariableName("$current_function"), new MemoryEntry(callInput.CreateFunction(declaration as FunctionDecl)));
            }

            if (declaration is MethodDecl)
            {
                callInput.Assign(new VariableName("$current_function"), new MemoryEntry(callInput.CreateFunction(declaration as MethodDecl)));
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
        /// Resolve return value from all possible calls. It also aplies user hints for flags removal 
        /// </summary>
        /// <param name="calls">All calls on dispatch level, which return value is resolved</param>
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

                var entries = new List<MemoryEntry>(calls.Length);
                foreach (var call in calls)
                {
                    var outSet = call.End.OutSet;
                    applyHints(outSet);
                    entries.Add(outSet.ReadValue(outSet.ReturnValue));
                }

                return MemoryEntry.Merge(entries);
            }
        }

        #endregion

        #region Private helpers

        private void applyHints(FlowOutputSet outSet)
        {
            if (outSet.ReadValue(new VariableName("$current_function")).Count == 1 && outSet.ReadValue(new VariableName("$current_function")).PossibleValues.First() is FunctionValue)
            {
                FunctionValue function = outSet.ReadValue(new VariableName("$current_function")).PossibleValues.First() as FunctionValue;
                if (function.DeclaringElement is FunctionDecl)
                { 
                    FunctionDecl funcDecl = (function.DeclaringElement as FunctionDecl);

                    new FunctionHints(funcDecl.PHPDoc,funcDecl).applyHints(outSet);
                }
                else if (function.DeclaringElement is MethodDecl)
                {
                    MethodDecl methodDecl = (function.DeclaringElement as MethodDecl);
                    new FunctionHints(methodDecl.PHPDoc,methodDecl).applyHints(outSet);
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
            //TODO resolving via visitor might be better
            if (declaration is MethodDecl)
            {
                return (declaration as MethodDecl).Signature;
            }
            
            if (declaration is FunctionDecl)
            {
                return (declaration as FunctionDecl).Signature;
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

        private Dictionary<object, FunctionValue> resolveFunction(QualifiedName name, MemoryEntry[] arguments)
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

                foreach (var function in functions)
                {
                    // TODO: Check whether the number of arguments match.
                    result[function.DeclaringElement] = function;
                }
            }

            return result;
        }

        private Dictionary<object, FunctionValue> resolveMethod(IEnumerable<ObjectValue> objects, QualifiedName name, MemoryEntry[] arguments)
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

        private static List<ObjectValue> resolveObjectsForMember(MemoryEntry entry, out bool isPossibleNonObject)
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

                //determine that argument value is based on variable, so we can get it's alias
                var aliasProvider = arg as AliasProvider;
                if (aliasProvider == null)
                {
                    //assign value for parameter
                    callInput.Assign(parVar, arguments[index]);
                }
                else
                {
                    //join parameter with alias (for testing we join all possible arguments)
                    //be carefull here - Flow.OutSet belongs to call context already - so we has to read variable from InSet
                    callInput.AssignAliases(parVar, aliasProvider.CreateAlias(Flow));
                }
                ++index;
            }

        }

        #endregion
    }


    //TODO testy treba pockat na priznaky
    internal class FunctionHints
    {
        HashSet<DirtyType> returnHints;
        Dictionary<int, HashSet<DirtyType>> argumentHints;
        LangElement declaration;
        internal FunctionHints(PHPDocBlock doc, LangElement _declaration)
        {
            declaration = _declaration;
            argumentHints = new Dictionary<int, HashSet<DirtyType>>();
            returnHints = new HashSet<DirtyType>();

            string comment;
            if(doc==null)
            {
                comment="";
            }
            else
            {
                comment=doc.ToString();
            }
            string endOfRegexp="";
            Array values = DirtyType.GetValues(typeof(DirtyType));
            foreach (DirtyType val in values)
            {
                   endOfRegexp+=val+"|";
            }
            endOfRegexp+="all)";
            string returnPatern = "(^[ \t]*\\*?[ \t]*@wev-hint[ \t]*returnvalue[ \t]*remove[ \t]*" + endOfRegexp;
            string argumentPatern = "(^[ \t]*\\*?[ \t]*@wev-hint[ \t]*outargument[ \t]*[0-9]+[ \t]*remove" + endOfRegexp;
            Regex retRegEx = new Regex(returnPatern, RegexOptions.IgnoreCase);
            Regex argRegEx = new Regex(argumentPatern, RegexOptions.IgnoreCase);
            foreach (var line in comment.Split('\n'))
            {
               
                if (retRegEx.IsMatch(line))
                {
                    Console.WriteLine("OK ppp" + line + "ppp");
                    Console.WriteLine();
                }

                if (argRegEx.IsMatch(line))
                {
                    Console.WriteLine("KO ppp" + line + "ppp");
                    Console.WriteLine();

                }

            }

        }

        private void addReturnHint(DirtyType type)
        {
            returnHints.Add(type);
        }

        private void addArgumentint(int i, DirtyType type)
        {
            if (argumentHints[i] == null)
            {
                argumentHints[i] = new HashSet<DirtyType>();
            }
            argumentHints[i].Add(type);
        }

        internal void applyHints(FlowOutputSet outset)
        {
            foreach (var type in returnHints)
            {
                MemoryEntry result = outset.ReadValue(outset.ReturnValue);
                foreach (var value in result.PossibleValues)
                {
                    ValueInfoHandler.setClean(outset, value, type);
                }
            }
           /* MemoryEntry argc = outset.ReadValue(new VariableName(".argument_count"));
            int argumentCount = ((IntegerValue)argc.PossibleValues.ElementAt(0)).Value;
            foreach (int arg in argumentHints.Keys)
            {
                if (arg < argumentCount)
                {
                    foreach (var type in argumentHints[arg])
                    {
                        MemoryEntry result = outset.ReadValue(argument(arg));
                        foreach (var value in result.PossibleValues)
                        {
                            ValueInfoHandler.setClean(outset, value, type);
                        }
                    }
                }
            }*/
        }
        private VariableName argument(int index)
        {
            if (index < 0)
            {
                throw new NotSupportedException("Cannot get argument variable for negative index");
            }
            return new VariableName(".arg" + index);
        }
    }
}
