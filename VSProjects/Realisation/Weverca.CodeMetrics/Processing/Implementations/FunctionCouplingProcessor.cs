using System.Collections.Generic;
using System.Diagnostics;

using PHP.Core.AST;
using PHP.Core.Reflection;

using Weverca.CodeMetrics.Processing.ASTVisitors;
using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Calculates how many user-defined functions a single function calls on average
    /// </summary>
    [Metric(Rating.PhpFunctionsCoupling)]
    class FunctionCouplingProcessor : RatingProcessor
    {
        #region MetricProcessor overrides

        protected override Result process(bool resolveOccurances, Rating category,
            SyntaxParser parser)
        {
            Debug.Assert(category == Rating.PhpFunctionsCoupling);
            Debug.Assert(parser.IsParsed);
            Debug.Assert(!parser.Errors.AnyError);

            if (parser.Functions == null)
            {
                // No function is declared
                if (resolveOccurances)
                {
                    return new Result(0.0, new DirectFcnCall[0]);
                }
                else
                {
                    return new Result(0.0);
                }
            }

            var functionReferences = new Stack<KeyValuePair<FunctionDecl, DirectFcnCall[]>>();
            foreach (var routine in parser.Functions)
            {
                Debug.Assert(routine.Value.Member is PhpFunction);
                var phpFunction = routine.Value.Member as PhpFunction;

                Debug.Assert(phpFunction.Declaration.GetNode() is FunctionDecl);
                var declaration = phpFunction.Declaration.GetNode() as FunctionDecl;

                var visitor = new FunctionCouplingVisitor(declaration.Function);
                declaration.VisitMe(visitor);
                var references = visitor.GetReferences();
                functionReferences.Push(new KeyValuePair<FunctionDecl, DirectFcnCall[]>(
                    declaration, references));
            }

            var result = CalculateRating(functionReferences.ToArray());

            if (resolveOccurances)
            {
                var allFunctionReferences = new Stack<DirectFcnCall>();
                foreach (var functionReference in functionReferences)
                {
                    foreach (var reference in functionReference.Value)
                    {
                        allFunctionReferences.Push(reference);
                    }
                }

                // Return all function calls (DirectFcnCall) used inside a function body
                return new Result(result, allFunctionReferences.ToArray());
            }
            else
            {
                return new Result(result);
            }
        }

        #endregion

        /// <summary>
        /// Calculate average number of unique function references that a function contains
        /// </summary>
        /// <returns>
        /// Measurement of average number of unique function reference inside a function implementation
        /// </returns>
        private double CalculateRating(KeyValuePair<FunctionDecl, DirectFcnCall[]>[]/*!*/ functionCouplings)
        {
            if (functionCouplings.Length <= 0)
            {
                return 0.0;
            }

            var numberOfReferences = 0;
            foreach (var functionDeclaration in functionCouplings)
            {
                Debug.Assert(functionDeclaration.Value != null);
                numberOfReferences += functionDeclaration.Value.Length;
            }
            return System.Convert.ToDouble(numberOfReferences) / functionCouplings.Length;
        }
    }
}
