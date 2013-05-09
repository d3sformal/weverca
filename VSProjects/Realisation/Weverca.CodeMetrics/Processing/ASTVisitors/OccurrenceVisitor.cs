using System.Collections.Generic;

using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.ASTVisitors
{
    /// <summary>
    /// Enriched tree with a place to store AST nodes which represent some wanted language construct
    /// </summary>
    class OccurrenceVisitor : TreeVisitor
    {
        /// <summary>
        /// All occurrences of searched language constructs
        /// </summary>
        protected Stack<AstNode> occurrenceNodes = new Stack<AstNode>();

        /// <summary>
        /// Return all nodes with occurences of searched language constructs
        /// </summary>
        /// <returns>Occurrences of appropriate nodes</returns>
        internal IEnumerable<AstNode> GetOccurrences()
        {
            // Copy result to an array to make it immutable
            return occurrenceNodes.ToArray();
        }

        /// <summary>
        /// Remove all stored occurrences
        /// </summary>
        internal void ResetContent()
        {
            occurrenceNodes.Clear();
        }
    }
}
