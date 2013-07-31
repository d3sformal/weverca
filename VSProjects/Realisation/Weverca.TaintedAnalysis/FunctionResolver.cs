using System;
using System.Collections.Generic;
using System.Linq;

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
    class AdvancedFunctionResolver : FunctionResolver
    {
        private NativeFunctionAnalyzer nativeFunctionAnalyzer = NativeFunctionAnalyzer.CreateInstance(); 
        
        
        /// <summary>
        /// Table of native analyzers
        /// </summary>
        private readonly Dictionary<string, NativeAnalyzer> _nativeAnalyzers = new Dictionary<string, NativeAnalyzer>()
        {
            {"strtolower",new NativeAnalyzer(_strtolower)},
            {"strtoupper",new NativeAnalyzer(_strtoupper)},
            {"concat",new NativeAnalyzer(_concat)},
            {"__constructor",new NativeAnalyzer(_constructor)},
        };

        internal AdvancedFunctionResolver()
        {
        }

        #region Call processing

        public override void MethodCall(MemoryEntry calledObject, QualifiedName name, MemoryEntry[] arguments)
        {
            var methods = resolveMethod(calledObject, name);
            setCallBranching(methods);
        }

        public override void Call(QualifiedName name, MemoryEntry[] arguments)
        {
            var functions = resolveFunction(name);
            setCallBranching(functions);
        }

        public override void IndirectMethodCall(MemoryEntry calledObject, MemoryEntry name, MemoryEntry[] arguments)
        {
            throw new NotImplementedException();
        }

        public override void IndirectCall(MemoryEntry name, MemoryEntry[] arguments)
        {
            var functionNames = getFunctionNames(name);
            var functions = new HashSet<LangElement>();

            foreach (var functionName in functionNames)
            {
                functions.UnionWith(resolveFunction(functionName));
            }

            setCallBranching(functions);
        }

        /// <summary>
        /// Initialize call into callInput. 
        /// 
        /// NOTE: 
        ///     arguments has to be initialized
        ///     sharing program point graphs is possible
        /// </summary>
        /// <returns></returns>
        public override void InitializeCall(FlowOutputSet callInput, LangElement declaration, MemoryEntry[] arguments)
        {
            var method = declaration as MethodDecl;
            var hasNamedSignature = method != null;
            if (hasNamedSignature)
            {
                //we have names for passed arguments
                setNamedArguments(callInput, arguments, method.Signature);
            }
            else
            {
                //there are no names - use numbered arguments
                setOrderedArguments(callInput, arguments);
            }
        }

        /// <summary>
        /// Resolve return value from all possible calls
        /// </summary>
        /// <param name="calls">All calls on dispatch level, which return value is resolved</param>
        /// <returns>Resolved return value</returns>
        public override MemoryEntry ResolveReturnValue(ProgramPointGraph[] calls)
        {
            var possibleMemoryEntries = from call in calls select call.End.OutSet.ReadValue(call.End.OutSet.ReturnValue).PossibleValues;
            var flattenValues = possibleMemoryEntries.SelectMany((i) => i);

            return new MemoryEntry(flattenValues.ToArray());
        }

        #endregion

        #region Native analyzers


        /// <summary>
        /// Analyzer method for strtolower php function
        /// </summary>
        /// <param name="flow"></param>
        private static void _strtolower(FlowController flow)
        {
            var arg = flow.InSet.ReadValue(argument(0));
            var possibleValues = new List<Value>();

            foreach (var possible in arg.PossibleValues)
            {
                if (possible is StringValue)
                {
                    var lower = flow.OutSet.CreateString(((StringValue)possible).Value.ToLower());
                    possibleValues.Add(lower);
                }
                else
                {
                    possibleValues.Add(flow.OutSet.AnyValue);
                }
            }

            var output = new MemoryEntry(possibleValues.ToArray());

            flow.OutSet.Assign(flow.OutSet.ReturnValue, output);
        }

        /// <summary>
        /// Analyzer method for strtolower php function
        /// </summary>
        /// <param name="flow"></param>
        private static void _strtoupper(FlowController flow)
        {
            var arg = flow.InSet.ReadValue(argument(0));

            var possibleValues = new List<StringValue>();

            foreach (StringValue possible in arg.PossibleValues)
            {
                var lower = flow.OutSet.CreateString(possible.Value.ToUpper());
                possibleValues.Add(lower);
            }

            var output = new MemoryEntry(possibleValues.ToArray());

            flow.OutSet.Assign(flow.OutSet.ReturnValue, output);
        }

        private static void _concat(FlowController flow)
        {
            var arg0 = flow.InSet.ReadValue(argument(0));
            var arg1 = flow.InSet.ReadValue(argument(1));

            var possibleValues = new List<StringValue>();

            foreach (StringValue possible0 in arg0.PossibleValues)
            {
                foreach (StringValue possible1 in arg1.PossibleValues)
                {
                    possibleValues.Add(flow.OutSet.CreateString(possible0.Value + possible1.Value));
                }
            }

            flow.OutSet.Assign(flow.OutSet.ReturnValue, new MemoryEntry(possibleValues.ToArray()));
        }

        private static void _constructor(FlowController flow)
        {
            //  flow.OutSet.Assign(flow.OutSet.ReturnValue, flow.OutSet.ThisObject);
        }

        #endregion

        #region Private helpers

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

        private void setCallBranching(HashSet<LangElement> functions)
        {
            foreach (var branchKey in Flow.CallBranchingKeys)
            {
                if (!functions.Remove(branchKey))
                {
                    //this call is now not resolved as possible call branch
                    Flow.RemoveCallBranch(branchKey);
                }
            }

            foreach (var function in functions)
            {
                //Create graph for every function - NOTE: we can share pp graphs
                var ppGraph = ProgramPointGraph.From(function);
                Flow.AddCallBranch(function, ppGraph);
            }
        }

        /// <summary>
        /// Resolving names according to given memory entry
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        private QualifiedName[] getFunctionNames(MemoryEntry functionName)
        {
            var result = new List<QualifiedName>();
            foreach (StringValue stringName in functionName.PossibleValues)
            {
                result.Add(new QualifiedName(new Name(stringName.Value)));
            }

            return result.ToArray();
        }

        private HashSet<LangElement> resolveFunction(QualifiedName name)
        {
            NativeAnalyzer analyzer;
            var result = new HashSet<LangElement>();

            /*if (_nativeAnalyzers.TryGetValue(name.Name.Value, out analyzer))
            {
                //we have native analyzer - create it's program point 
                result.Add(analyzer);
            }*/
            if (nativeFunctionAnalyzer.existNativeFunction(name))
            {
                result.Add(new NativeAnalyzer(nativeFunctionAnalyzer.getNativeAnalyzer(name)));
            }
            else
            {
                var functions = OutSet.ResolveFunction(name);

                var declarations = from FunctionValue function in functions select function.Declaration;
                result.UnionWith(declarations);
            }

            return result;
        }

        private HashSet<LangElement> resolveMethod(MemoryEntry thisObject, QualifiedName methodName)
        {
            NativeAnalyzer analyzer;

            var result = new HashSet<LangElement>();

            if (_nativeAnalyzers.TryGetValue(methodName.Name.Value, out analyzer))
            {
                //we have native analyzer - create it's program point 
                result.Add(analyzer);
            }
            else
            {
                throw new NotImplementedException();
            }

            return result;
        }

        private void setNamedArguments(FlowOutputSet callInput, MemoryEntry[] arguments, Signature signature)
        {
            for (int i = 0; i < signature.FormalParams.Count; ++i)
            {
                var param = signature.FormalParams[i];

                callInput.Assign(param.Name, arguments[i]);
            }
        }

        private void setOrderedArguments(FlowOutputSet callInput, MemoryEntry[] arguments)
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
