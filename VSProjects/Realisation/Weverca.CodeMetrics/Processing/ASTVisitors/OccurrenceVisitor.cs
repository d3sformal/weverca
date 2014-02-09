using System.Collections.Generic;

using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.AstVisitors
{
    /// <summary>
    /// Enriched tree with a place to store AST nodes which represent some wanted language construct.
    /// </summary>
    internal class OccurrenceVisitor : TreeVisitor
    {
        /// <summary>
        /// All occurrences of searched language constructs.
        /// </summary>
        protected Queue<AstNode> occurrenceNodes = new Queue<AstNode>();

        /// <summary>
        /// Return all nodes with occurrences of searched language constructs.
        /// </summary>
        /// <returns>Occurrences of appropriate nodes.</returns>
        internal IEnumerable<AstNode> GetOccurrences()
        {
            // Copy result to an array to make it immutable
            return occurrenceNodes.ToArray();
        }

        /// <summary>
        /// Remove all stored occurrences.
        /// </summary>
        internal void ResetContent()
        {
            occurrenceNodes.Clear();
        }
    }
}
