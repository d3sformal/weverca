using System.Collections.Generic;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.Analysis.ExpressionEvaluator;

namespace Weverca.Analysis.NativeAnalyzers
{
    /// <summary>
    /// Contains concrete implementations of native functions.
    /// 
    /// Note that all functions that have concrete implementation have to be also type-modeled.
    /// </summary>
    public static class NativeFunctionsConcreteImplementations
    {
        #region Utility methods and fields

        private static readonly StringConverter stringConverter = new StringConverter();

        /// <summary>
        /// Adds concrete functions implemented in this class to the the dictionary of concrete native functions.
        /// 
        /// Note that all functions that have concrete implementation have to be also type-modeled.
        /// </summary>
        /// <param name="typeModeledFunctions">the dictionary of type-modeled native functions</param>
        /// <param name="concreteFunctions">(output parameter) the dictionary of concrete native functions to which functions implemented in this class will be added</param>
        public static void AddConcreteFunctions(Dictionary<QualifiedName, List<NativeFunction>> typeModeledFunctions, Dictionary<QualifiedName, NativeAnalyzerMethod> concreteFunctions)
        {
            foreach (var methodName in functions.Keys)
            {
                var qualifiedName = new QualifiedName(new Name(methodName));
                concreteFunctions.Add(qualifiedName, new NativeAnalyzerMethod(new ConcreteFunctionAnalyzerHelper(typeModeledFunctions[qualifiedName], functions[methodName]).analyze));
            }
        }
        #endregion

        /// <summary>
        /// Contains all pairs "name of the function - implementation of the function" that can be used by the analysis.
        /// </summary>
        private static Dictionary<string, ConcreteFunctionDelegate> functions = new Dictionary<string, ConcreteFunctionDelegate>() 
        {
            { "strtolower", _strtolower },
            { "strtoupper", _strtoupper },
            { "concat", _concat }
        };

        #region Implementations of concrete native functions
        private static Value _strtolower(FlowController flow, Value[] arguments)
        {
            stringConverter.SetContext(flow);
            return flow.OutSet.CreateString(
                PHP.Library.PhpStrings.ToLower(
                    stringConverter.EvaluateToString(arguments[0]).Value)
                );
        }

        private static Value _strtoupper(FlowController flow, Value[] arguments)
        {
            stringConverter.SetContext(flow);
            return flow.OutSet.CreateString(
                PHP.Library.PhpStrings.ToUpper(
                    stringConverter.EvaluateToString(arguments[0]).Value)
                );
        }

        private static Value _concat(FlowController flow, Value[] arguments)
        {
            stringConverter.SetContext(flow);
            return stringConverter.EvaluateConcatenation(arguments[0], arguments[1]);
        }
        #endregion

    }
}
