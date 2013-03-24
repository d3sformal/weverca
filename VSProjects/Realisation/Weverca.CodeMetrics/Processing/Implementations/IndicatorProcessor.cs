using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
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

    #region Tree Visitors
    class CallVisitor : TreeVisitor
    {
        HashSet<string> searchedCalls;
        List<AstNode> foundCalls = new List<AstNode>();

        public CallVisitor(IEnumerable<string> functions)
        {
            searchedCalls = new HashSet<string>(functions);
        }

        #region TreeVisitor overrides
        public override void VisitFunctionCall(FunctionCall x)
        {
            throw new NotImplementedException("There is no method for getting FunctionCall name - In Progress, this will work in future");
        /*    if (searchedCalls.Contains(x.Name))
            {
                foundCalls.Add(x);
            }*/
        }

        /// <summary>
        /// Phalanger resolves eval as special expression
        /// </summary>
        /// <param name="x"></param>
        public override void VisitEvalEx(EvalEx x)
        {
            if (searchedCalls.Contains("eval"))
            {
                foundCalls.Add(x);
            }
        }
        #endregion
        /// <summary>
        /// Returns calls which were founded during visiting tree
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<AstNode> GetCalls()
        {
            //Copy result because its immutable
            return foundCalls.ToArray();
        }
    }
    #endregion
}
