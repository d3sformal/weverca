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
    /// <summary>
    /// Processor for analyzing maximum inheritance depth.
    /// </summary>
    [Metric(Quantity.MaxInheritanceDepth)]
    class MaxInheritanceDepthProcessor : QuantityProcessor
    {
        protected override Result process(bool resolveOccurances, Quantity category, SyntaxParser parser)
        {            
            var occurances = new List<AstNode>();
            var maxInheritanceDepth = findMaxInheritanceDepth(parser, occurances);

            return new Result(maxInheritanceDepth, occurances);
        }

        /// <summary>
        /// Find max inheritance depth and fill occurences with type declarations where this depth appeared.        
        /// </summary>
        /// <param name="parser">Parser which contains analyzed AST</param>
        /// <param name="occurances">Type declarations where max depth appeared</param>
        /// <returns>Maximum inheritance depth that has been found</returns>
        private int findMaxInheritanceDepth(SyntaxParser parser, List<AstNode> occurances)
        {            
            if (parser.Types == null)
                //no types were found - there is no inheritance
                return 0;

            int maxInheritanceDepth = 0;
            //find max inheritance depth in any types
            foreach (var qualifiedName in parser.Types.Keys)
            {
                var inheritanceDepth = getInheritanceDepth(qualifiedName, parser);
                if (inheritanceDepth > maxInheritanceDepth)
                {
                    //we have found greater inheritance chain - clear old entries
                    occurances.Clear();
                    maxInheritanceDepth = inheritanceDepth;
                }
                else if (inheritanceDepth < maxInheritanceDepth)
                {
                    //inheritance depth is lower skip it
                    continue;
                }
                else
                {
                    //inheritance depth is equal to maximum - no preprocesing is needed
                }

                var node = parser.Types[qualifiedName].GetNode() as AstNode;

                //Type declarations node shouldnt been null
                Debug.Assert(node != null);
                occurances.Add(node);
            }

            return maxInheritanceDepth;
        }

        /// <summary>
        /// Get inheritance depth for for given typeName.
        /// </summary>
        /// <param name="typeName">Name of analyzed type.</param>
        /// <param name="parser">Parser that has analyzed type.</param>
        /// <returns>Depth of inheritance of given typeName</returns>
        private int getInheritanceDepth(QualifiedName typeName, SyntaxParser parser)
        {
            Declaration type;
            if (!parser.Types.TryGetValue(typeName,out type))
            {
                //TODO: there is possible an error in parsed source
                return 1;
            }            
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

        /// <summary>
        /// Get inheritance depth for for given qualifiedName.
        /// </summary>
        /// <param name="qualifiedName">Name of analyzed type.</param>
        /// <param name="parser">Parser that has analyzed type.</param>
        /// <returns>Depth of inheritance of given qualifiedName or null if qualifiedName is null.</returns>
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
