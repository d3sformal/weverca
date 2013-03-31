using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
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
        public override void VisitDirectFcnCall(DirectFcnCall x)
        {
            if (isSearched(x.QualifiedName))
            {
                foundCalls.Add(x);
            }
            base.VisitDirectFcnCall(x);
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

        
        #region Private utilities for function matching

        private bool isSearched(QualifiedName qualifiedName)
        {
            var name = qualifiedName.Name.Value;
            return searchedCalls.Contains(name);
        }

        #endregion
    }


    /// <summary>
    /// Visitor which collect super global variable usage
    /// </summary>
    class SuperGlobalVarVisitor : TreeVisitor
    {
        List<AstNode> foundSuperGlobals = new List<AstNode>();

        /// <summary>
        /// Returns calls which were founded during visiting tree
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<AstNode> GetVariables()
        {
            //Copy result because of make it immutable
            return foundSuperGlobals.ToArray();
        }

        #region TreeVisitor overrides

        public override void VisitDirectVarUse(DirectVarUse x)
        {
            var name=x.VarName;
            if (name.IsAutoGlobal)
            {
                foundSuperGlobals.Add(x);
            }

            base.VisitDirectVarUse(x);
        }

        #endregion
    }
}
