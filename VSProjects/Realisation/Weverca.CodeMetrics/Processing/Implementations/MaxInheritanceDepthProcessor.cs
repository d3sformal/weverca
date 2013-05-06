using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Reflection;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Processor for analyzing maximum inheritance depth
    /// </summary>
    [Metric(Quantity.MaxInheritanceDepth)]
    class MaxInheritanceDepthProcessor : QuantityProcessor
    {
        #region MetricProcessor overrides

        protected override Result process(bool resolveOccurances, Quantity category,
            SyntaxParser parser)
        {
            Debug.Assert(category == Quantity.MaxInheritanceDepth);

            var occurrences = new Stack<AstNode>();
            var maxInheritanceDepth = FindMaxInheritanceDepth(parser, occurrences);

            return new Result(maxInheritanceDepth, occurrences.ToArray());
        }

        #endregion

        /// <summary>
        /// Find max inheritance depth and fill occurrences with type declarations where this depth appeared.
        /// </summary>
        /// <param name="parser">Parser which contains analyzed AST</param>
        /// <param name="occurances">Type declarations where max depth appeared</param>
        /// <returns>Maximum inheritance depth that has been found</returns>
        private int FindMaxInheritanceDepth(SyntaxParser parser, Stack<AstNode> occurrences)
        {
            System.Diagnostics.Debug.Assert(parser.IsParsed);
            System.Diagnostics.Debug.Assert(!parser.Errors.AnyError);

            if (parser.Types == null)
            {
                // No type is declared, there is no inheritance
                return 0;
            }

            int maxInheritanceDepth = 0;
            // Find max inheritance depth in any types
            foreach (var qualifiedName in parser.Types.Keys)
            {
                var inheritanceDepth = GetInheritanceDepth(qualifiedName, parser);
                if (inheritanceDepth > maxInheritanceDepth)
                {
                    // We have found greater inheritance chain - clear old entries
                    occurrences.Clear();
                    maxInheritanceDepth = inheritanceDepth;
                }
                else if (inheritanceDepth < maxInheritanceDepth)
                {
                    // Inheritance depth is lower skip it
                    continue;
                }
                else
                {
                    // Inheritance depth is equal to maximum - no preprocesing is needed
                }

                var node = parser.Types[qualifiedName].Declaration.GetNode();
                // Node of type declarations should not been null
                System.Diagnostics.Debug.Assert(node != null);
                System.Diagnostics.Debug.Assert(node is AstNode);

                var astNode = node as AstNode;
                occurrences.Push(astNode);
            }

            return maxInheritanceDepth;
        }

        /// <summary>
        /// Get inheritance depth for for given typeName.
        /// </summary>
        /// <param name="typeName">Name of analyzed type</param>
        /// <param name="parser">Parser that has analyzed type</param>
        /// <returns>Depth of inheritance of given typeName</returns>
        private int GetInheritanceDepth(QualifiedName typeName, SyntaxParser parser)
        {
            PhpType type;
            if (!parser.Types.TryGetValue(typeName, out type))
            {
                // TODO: There is possible an error in parsed source
                return 1;
            }

            var node = type.Declaration.GetNode() as TypeDecl;

            var maxInheritanceDepth = GetInheritanceDepth(node.BaseClassName, parser);
            foreach (var implemented in node.ImplementsList)
            {
                var implementedName = implemented.QualifiedName;
                var inheritanceDepth = GetInheritanceDepth(implementedName, parser);

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
        private int GetInheritanceDepth(GenericQualifiedName? qualifiedName, SyntaxParser parser)
        {
            if (qualifiedName == null)
            {
                return 0;
            }

            return GetInheritanceDepth(qualifiedName.Value.QualifiedName, parser);
        }

    }
}
