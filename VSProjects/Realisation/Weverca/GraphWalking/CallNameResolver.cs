/*
Copyright (c) 2012-2014 David Hauzar, Miroslav Vodolan, Marcel Kikta, Pavel Bastecky, David Skorvaga, and Matyas Brenner

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;
using Weverca.AnalysisFramework;

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
                var invokingElement = (element as NativeAnalyzer).InvokingElement;
                if (invokingElement is StringLiteral)
                {
                    visitor._name = (invokingElement as StringLiteral).Value.ToString();
                }
                else if (invokingElement is DirectFcnCall)
                {
                    visitor._name = (invokingElement as DirectFcnCall).QualifiedName.Name.ToString();
                }
                else 
                {
                    visitor._name = (element as NativeAnalyzer).InvokingElement.ToString();
                }
                
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