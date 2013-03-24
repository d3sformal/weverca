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
        protected override bool merge(bool r1, bool r2)
        {
            return r1 || r2;
        }

        protected override IEnumerable<AstNode> merge(IEnumerable<AstNode> o1, IEnumerable<AstNode> o2)
        {
            var merged = new List<AstNode>(o1);
            merged.AddRange(o2);

            return merged;
        }

        /// <summary>
        /// Determine that source in given parser contains any method from calls or not
        /// </summary>
        /// <param name="calls"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        protected IEnumerable<FunctionCall> findCalls(SyntaxParser parser, IEnumerable<string> calls)
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

    }

    class CallVisitor : TreeVisitor
    {
        HashSet<string> searchedCalls;
        List<FunctionCall> foundCalls = new List<FunctionCall>();

        public CallVisitor(IEnumerable<string> functions)
        {
            searchedCalls = new HashSet<string>(functions);
        }

        #region TreeVisitor overrides
        public override void VisitFunctionCall(FunctionCall x)
        {
            //TODO find out how to get call name
            if (searchedCalls.Contains(x.ToString()))
            {
                foundCalls.Add(x);
            }
        }
        #endregion
        /// <summary>
        /// Returns calls which were founded during visiting tree
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<FunctionCall> GetCalls()
        {
            //Copy result because its immutable
            return foundCalls.ToArray();
        }
    }
}
