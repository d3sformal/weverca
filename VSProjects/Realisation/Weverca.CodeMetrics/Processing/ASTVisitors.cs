using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing
{
    /// <summary>
    /// Visitor which collect function calls
    /// </summary>
    class CallVisitor : TreeVisitor
    {
        HashSet<string> searchedCalls;
        List<AstNode> foundCalls = new List<AstNode>();

        /// <summary>
        /// Create call visitor, which collect occurances of given functions
        /// </summary>
        /// <param name="functions"></param>
        public CallVisitor(IEnumerable<string> functions)
        {
            searchedCalls = new HashSet<string>(functions);
        }

        /// <summary>
        /// Returns calls which were founded during visiting tree
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<AstNode> GetCalls()
        {
            //Copy result because of make it immutable
            return foundCalls.ToArray();
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
    }
}
