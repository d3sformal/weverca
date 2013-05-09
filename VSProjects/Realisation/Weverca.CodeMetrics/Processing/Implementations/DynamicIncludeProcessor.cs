using System.Collections.Generic;
using System.Diagnostics;

using PHP.Core.AST;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Identifies all inclusions which can be evaluated statically
    /// </summary>
    [Metric(ConstructIndicator.DynamicInclude)]
    class DynamicIncludeProcessor : IndicatorProcessor
    {
        #region MetricProcessor overrides

        protected override IndicatorProcessor.Result process(bool resolveOccurances,
            ConstructIndicator category, SyntaxParser parser)
        {
            Debug.Assert(category == ConstructIndicator.DynamicInclude);
            Debug.Assert(parser.IsParsed);
            Debug.Assert(!parser.Errors.AnyError);

            if (parser.Inclusions == null)
            {
                // No type is inclusions
                if (resolveOccurances)
                {
                    return new Result(false, new IncludingEx[0]);
                }
                else
                {
                    return new Result(false);
                }
            }

            var occurrences = new Stack<IncludingEx>();
            var stringFunctions = MetricRelatedFunctions.Get(category);
            Debug.Assert(stringFunctions.GetEnumerator().MoveNext());
            var functions = new HashSet<string>(stringFunctions);

            foreach (var inclusion in parser.Inclusions)
            {
                var expressions = new Queue<Expression>();
                expressions.Enqueue(inclusion.Target);
                var isDynamic = false;

                while (expressions.Count > 0)
                {
                    var expression = expressions.Dequeue();

                    // Note that the strings beginning with quotes are automatically broken down by variables
                    if (expression is ConcatEx)
                    {
                        var concatenation = expression as ConcatEx;
                        foreach (var operand in concatenation.Expressions)
                        {
                            expressions.Enqueue(operand);
                        }
                    }
                    else if (expression is DirectFcnCall)
                    {
                        var functionCall = expression as DirectFcnCall;

                        // The subroutine must be function, i.e. it must not be member of a class
                        if (functionCall.IsMemberOf == null
                            // The number of parameters must be exactly 1
                            && functionCall.CallSignature.Parameters.Count == 1
                            // Function names are case-insensitive
                            && functions.Contains(functionCall.QualifiedName.Name.LowercaseValue))
                        {
                            expressions.Enqueue(functionCall.CallSignature.Parameters[0].Expression);
                        }
                        else
                        {
                            isDynamic = true;
                            break;
                        }
                    }
                    else if (expression is StringLiteral)
                    {
                        // Correct terminal symbol of the expression
                    }
                    else
                    {
                        isDynamic = true;
                        break;
                    }
                }

                if (isDynamic)
                {
                    occurrences.Push(inclusion);
                }
            }

            var hasOccurrence = occurrences.GetEnumerator().MoveNext();
            // Return inclusions (IncludingEx) that cannot be evaluated statically
            return new Result(hasOccurrence, occurrences);
        }

        #endregion
    }
}
