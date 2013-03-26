using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Reflection;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    [Metric(Quantity.MaxInheritanceDepth)]
    class MaxInheritanceDepthProcessor : QuantityProcessor
    {
        protected override Result process(bool resolveOccurances, Quantity category, SyntaxParser parser)
        {
            var maxInheritanceDepth = 0;
            var collected = new List<AstNode>();


            foreach (var qualifiedName in parser.Types.Keys)
            {
                var inheritanceDepth = getInheritanceDepth(qualifiedName, parser);
                if (inheritanceDepth > maxInheritanceDepth)
                {
                    //we have found greater inheritance chain - clear old entries
                    collected.Clear();
                    maxInheritanceDepth = inheritanceDepth;
                }
                else if (inheritanceDepth < maxInheritanceDepth)
                {
                    continue;
                }
                
                var node = parser.Types[qualifiedName].GetNode() as AstNode;
                Debug.Assert(node != null);
                collected.Add(node);
            }

            return new Result(maxInheritanceDepth, collected);
        }

        private int getInheritanceDepth(QualifiedName typeName, SyntaxParser parser)
        {
            var type = parser.Types[typeName];
            var node = type.GetNode() as TypeDecl;


            var maxInheritanceDepth = getInheritanceDepth(node.BaseClassName, parser);
            foreach (var implemented in node.ImplementsList)
            {
                var implementedName = implemented.QualifiedName;
                var inheritanceDepth = getInheritanceDepth(implementedName, parser);

                if (inheritanceDepth > maxInheritanceDepth)
                {
                    maxInheritanceDepth = inheritanceDepth;
                }
            }

            return maxInheritanceDepth + 1;
        }

        private int getInheritanceDepth(GenericQualifiedName? qualifiedName, SyntaxParser parser)
        {
            if (qualifiedName == null)
            {
                return 0;
            }

            return getInheritanceDepth(qualifiedName.Value.QualifiedName, parser);
        }

    }
}
