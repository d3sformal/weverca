using System.Diagnostics;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    [Metric(ConstructIndicator.Eval, ConstructIndicator.Session, ConstructIndicator.MySql,
        ConstructIndicator.ClassAlias)]
    internal class FunctionPresenceProcessor : IndicatorProcessor
    {
        #region MetricProcessor overrides

        /// <inheritdoc />
        public override Result Process(bool resolveOccurances, ConstructIndicator category,
            SyntaxParser parser)
        {
            Debug.Assert(parser.IsParsed, "Source code must be parsed");
            Debug.Assert(!parser.Errors.AnyError, "Source code must not have any syntax error");

            var functions = MetricRelatedFunctions.Get(category);

            var calls = FindCalls(parser, functions);
            var hasCalls = calls.GetEnumerator().MoveNext();

            return new Result(hasCalls, calls);
        }

        #endregion MetricProcessor overrides
    }
}
