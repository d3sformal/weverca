using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Determines whether there is class presence.
    /// </summary>
    [Metric(ConstructIndicator.Class)]
    class ClassPresenceProcessor : IndicatorProcessor
    {
        #region IndicatorProcessor overrides

        protected override Result process(bool resolveOccurances, ConstructIndicator category, Parsers.SyntaxParser parser)
        {
            Debug.Assert(category == ConstructIndicator.Class);
            Debug.Assert(parser.IsParsed);
            Debug.Assert(!parser.Errors.AnyError);

            var classes = parser.Types.Where(t => !t.Value.IsInterface);
            bool classPresent = classes.Count() > 0;

            var occurences = classes.Select(t => t.Value.Declaration.GetNode() as TypeDecl).Where(t => t != null).ToArray(); // they all should not be null

            return new Result(classPresent, occurences);
        }

        #endregion
    }
}
