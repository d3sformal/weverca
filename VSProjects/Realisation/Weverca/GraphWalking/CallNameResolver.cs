using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;
using Weverca.Analysis;

namespace Weverca.GraphWalking
{
    /// <summary>
    /// Resolve representative names for given lang elements
    /// </summary>
    class NameResolver:TreeVisitor
    {
        private string _name;
        private NameResolver()
        {}

        /// <summary>
        /// Resolve representative name for given element
        /// </summary>
        /// <param name="element">Element which name is resolved</param>
        /// <returns>Resolved name</returns>
        public static string Resolve(LangElement element)
        {
            var visitor = new NameResolver();
            element.VisitMe(visitor);

            if (element is NativeAnalyzer)
            {
                visitor._name = ((StringLiteral)((element as NativeAnalyzer).InvokingElement)).Value.ToString();
            }

            if (visitor._name == null)
            {
                throw new NotImplementedException("Method for resolving given element hasn't been implemented yet");
                
            }

            return visitor._name;
        }


        public override void VisitMethodDecl(MethodDecl x)
        {
            _name = x.Name.Value;
        }

        public override void VisitFunctionDecl(FunctionDecl x)
        {
            _name = x.Name.Value;
        }
    }
}
