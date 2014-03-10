using System.Collections.Generic;

using PHP.Core;
using PHP.Library;

using Weverca.Analysis.ExpressionEvaluator;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

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

        private static readonly BooleanConverter booleanConverter = new BooleanConverter();

        private static readonly StringConverter stringConverter = new StringConverter();

        /// <summary>
        /// Adds concrete functions implemented in this class to the the dictionary of concrete native functions.
        /// 
        /// Note that all functions that have concrete implementation have to be also type-modeled.
        /// </summary>
        /// <param name="typeModeledFunctions">The dictionary of type-modeled native functions.</param>
        /// <param name="concreteFunctions">
        /// (output parameter) the dictionary of concrete native functions
        /// to which functions implemented in this class will be added.
        /// </param>
        public static void AddConcreteFunctions(
            Dictionary<QualifiedName, List<NativeFunction>> typeModeledFunctions,
            Dictionary<QualifiedName, NativeAnalyzerMethod> concreteFunctions)
        {
            foreach (var methodName in functions.Keys)
            {
                var qualifiedName = new QualifiedName(new Name(methodName));
                var analyzer = new ConcreteFunctionAnalyzerHelper(typeModeledFunctions[qualifiedName],
                    functions[methodName]);
                var method = new NativeAnalyzerMethod(analyzer.analyze);
                concreteFunctions.Add(qualifiedName, method);
            }
        }

        #endregion Utility methods and fields

        /// <summary>
        /// Contains all pairs "name of the function - implementation of the function" that can be used by the analysis.
        /// </summary>
        private static Dictionary<string, ConcreteFunctionDelegate> functions
            = new Dictionary<string, ConcreteFunctionDelegate>()
        {
            { "strtolower", _strtolower },
            { "strtoupper", _strtoupper },
            { "htmlentities", _htmlentities },
            { "md5", _md5 }
        };

        #region Implementations of concrete native functions

        private static Value _strtolower(FlowController flow, Value[] arguments)
        {
            Debug.Assert(arguments.Length == 1);

            stringConverter.SetContext(flow);
            var stringValue = stringConverter.EvaluateToString(arguments[0]);

            if (stringValue == null)
            {
                return flow.OutSet.AnyStringValue;
            }

            return flow.OutSet.CreateString(PhpStrings.ToLower(stringValue.Value));
        }

        private static Value _strtoupper(FlowController flow, Value[] arguments)
        {
            Debug.Assert(arguments.Length == 1);

            stringConverter.SetContext(flow);
            var stringValue = stringConverter.EvaluateToString(arguments[0]);

            if (stringValue == null)
            {
                return flow.OutSet.AnyStringValue;
            }

            return flow.OutSet.CreateString(PhpStrings.ToUpper(stringValue.Value));
        }

        private static Value _concat(FlowController flow, Value[] arguments)
        {
            stringConverter.SetContext(flow);
            return stringConverter.EvaluateConcatenation(arguments[0], arguments[1]);
        }

        private static Value _htmlentities(FlowController flow, Value[] arguments)
        {
            Debug.Assert(arguments.Length > 0);

            if (arguments.Length > 1)
            {
                // TODO: Implement precisely
                return flow.OutSet.AnyStringValue;
            }

            stringConverter.SetContext(flow);
            var stringValue = stringConverter.EvaluateToString(arguments[0]);

            if (stringValue == null)
            {
                return flow.OutSet.AnyStringValue;
            }

            return flow.OutSet.CreateString(PhpStrings.EncodeHtmlEntities(stringValue.Value));
        }

        private static Value _md5(FlowController flow, Value[] arguments)
        {
            stringConverter.SetContext(flow);
            var stringValue = stringConverter.EvaluateToString(arguments[0]);

            if (stringValue == null)
            {
                return flow.OutSet.AnyStringValue;
            }

            var phpBytes = new PhpBytes(stringValue.Value);

            Debug.Assert(arguments.Length > 0);

            if (arguments.Length > 1)
            {
                booleanConverter.SetContext(flow.OutSet.Snapshot);
                var isRawOutput = booleanConverter.EvaluateToBoolean(arguments[1]);

                if ((isRawOutput == null) || isRawOutput.Value)
                {
                    // TODO: Implement precisely
                    return flow.OutSet.AnyStringValue;
                }
            }

            return flow.OutSet.CreateString(PhpHash.MD5(phpBytes));
        }

        #endregion Implementations of concrete native functions
    }
}
