using System;
using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis;
using Weverca.Analysis.Expressions;
using Weverca.Analysis.Memory;

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
            var methods = new Dictionary<LangElement, FunctionValue>();
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
            var functions = new Dictionary<LangElement, FunctionValue>();
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
        public override void InitializeCall(FlowOutputSet callInput, LangElement declaration, MemoryEntry[] arguments)
        {
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
                // Object is returned via return value of call extension
                return null;
            }

            // No constructor call
            return newObject;
        }

        /// <summary>
        /// Resolve return value from all possible calls
        /// </summary>
        /// <param name="calls">All calls on dispatch level, which return value is resolved</param>
        /// <returns>Resolved return value</returns>
        public override MemoryEntry ResolveReturnValue(ProgramPointGraph[] calls)
        {
            if (calls.Length == 1)
            {
                var outSet = calls[0].End.OutSet;
                return outSet.ReadValue(outSet.ReturnValue);
            }
            else
            {
                Debug.Assert(calls.Length > 0, "There must be at least one call");

                var entries = new List<MemoryEntry>(calls.Length);
                foreach (var call in calls)
                {
                    var outSet = call.End.OutSet;
                    entries.Add(outSet.ReadValue(outSet.ReturnValue));
                }

                return MemoryEntry.Merge(entries);
            }
        }

        #endregion

        #region Private helpers

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


        private void setCallBranching(Dictionary<LangElement, FunctionValue> functions)
        {
            foreach (var branchKey in Flow.CallBranchingKeys)
            {
                if (!functions.Remove(branchKey))
                {
                    // Now this call is not resolved as possible call branch
                    Flow.RemoveCallBranch(branchKey);
                }
            }

            foreach (var function in functions.Values)
            {
                // Create graph for every function - NOTE: We can share pp graphs
                var ppGraph = ProgramPointGraph.From(function);
                Flow.AddCallBranch(function.DeclaringElement, ppGraph);
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

        private Dictionary<LangElement, FunctionValue> resolveFunction(QualifiedName name, MemoryEntry[] arguments)
        {
            var result = new Dictionary<LangElement, FunctionValue>();

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

        private Dictionary<LangElement, FunctionValue> resolveMethod(IEnumerable<ObjectValue> objects, QualifiedName name, MemoryEntry[] arguments)
        {
            var result = new Dictionary<LangElement, FunctionValue>();

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

        private static void setNamedArguments(FlowOutputSet callInput, MemoryEntry[] arguments, Signature signature)
        {
            for (int i = 0; i < signature.FormalParams.Count; ++i)
            {
                var param = signature.FormalParams[i];

                callInput.Assign(param.Name, arguments[i]);
            }
        }

        private static void setOrderedArguments(FlowOutputSet callInput, MemoryEntry[] arguments)
        {
            var argCount = callInput.CreateInt(arguments.Length);
            callInput.Assign(new VariableName(".argument_count"), argCount);

            for (int i = 0; i < arguments.Length; ++i)
            {
                var argVar = argument(i);

                callInput.Assign(argVar, arguments[i]);
            }
        }

        #endregion
    }
}
