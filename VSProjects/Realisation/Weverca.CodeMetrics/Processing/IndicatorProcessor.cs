using System;
using System.Collections.Generic;
using System.Linq;

using PHP.Core.AST;
using Weverca.CodeMetrics.Processing.ASTVisitors;
using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing
{

    abstract class IndicatorProcessor : MetricProcessor<ConstructIndicator, bool>
    {
        #region MetricProcessor abstract method implementations
        /// <summary>
        /// Merging of almost all indicators should be easy. Others can override this beahviour
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        protected override bool merge(bool r1, bool r2)
        {
            return r1 || r2;
        }

        /// <summary>
        /// Merging of almost all indicators should be easy. Others can override this beahviour
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        protected override IEnumerable<AstNode> merge(IEnumerable<AstNode> o1, IEnumerable<AstNode> o2)
        {
            var merged = new List<AstNode>(o1);
            merged.AddRange(o2);

            return merged;
        }
        #endregion

        #region Utility methods for child classes
        /// <summary>
        /// Determine that source in given parser contains any method from calls or not
        /// </summary>
        /// <param name="calls"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        protected IEnumerable<AstNode> findCalls(SyntaxParser parser, IEnumerable<string> calls)
        {
            if (calls.Count() == 0)
            {
                return new FunctionCall[0];
            }

            var visitor = new CallVisitor(calls);

            parser.Ast.VisitMe(visitor);
            return visitor.GetCalls();
        }        

        protected IEnumerable<FunctionCall> findMethods(SyntaxParser parser, IEnumerable<string> methods)
        {
            throw new NotImplementedException();
        }
        #endregion

    }

}
