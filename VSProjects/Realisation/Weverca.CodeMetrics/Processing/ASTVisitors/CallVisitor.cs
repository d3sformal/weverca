using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.AstVisitors
{
    /// <summary>
    /// Represents the visitor that collects all occurrences the given function calls.
    /// </summary>
    internal class CallVisitor : OccurrenceVisitor
    {
        /// <summary>
        /// Functions whose calls are looked for.
        /// </summary>
        private HashSet<string> searchedCalls;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallVisitor" /> class.
        /// </summary>
        /// <param name="functions">List of functions to be detected.</param>
        public CallVisitor(IEnumerable<string> functions)
        {
            searchedCalls = new HashSet<string>(functions);
        }

        #region TreeVisitor overrides

        /// <inheritdoc />
        public override void VisitDirectFcnCall(DirectFcnCall x)
        {
            if (IsSearched(x.QualifiedName))
            {
                occurrenceNodes.Enqueue(x);
            }

            base.VisitDirectFcnCall(x);
        }

        /// <inheritdoc />
        /// <remarks>
        /// Phalanger resolves eval as special expression
        /// </remarks>
        public override void VisitEvalEx(EvalEx x)
        {
            if (searchedCalls.Contains("eval"))
            {
                occurrenceNodes.Enqueue(x);
            }
        }

        #endregion TreeVisitor overrides

        #region Private utilities for function matching

        /// <summary>
        /// Indicate whether the function is in the list of searched function calls.
        /// </summary>
        /// <param name="qualifiedName">Name of the function.</param>
        /// <returns><c>true</c> if it is searched, otherwise <c>false</c>.</returns>
        private bool IsSearched(QualifiedName qualifiedName)
        {
            var name = qualifiedName.Name.Value;
            return searchedCalls.Contains(name);
        }

        #endregion Private utilities for function matching
    }
}
