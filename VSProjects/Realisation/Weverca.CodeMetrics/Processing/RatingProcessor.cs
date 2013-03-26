using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing
{
    abstract class RatingProcessor : MetricProcessor<Rating, double>
    {
        /// <summary>
        /// Merging of almost all ratings should be easy. Others can override this beahviour
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        protected override double merge(double r1, double r2)
        {
            return Math.Max(r1, r2);
        }

        /// <summary>
        /// Merging of almost all ratings should be easy. Others can override this beahviour
        /// </summary>
        /// <param name="o1"></param>
        /// <param name="o2"></param>
        /// <returns></returns>
        protected override IEnumerable<AstNode> merge(IEnumerable<AstNode> o1, IEnumerable<AstNode> o2)
        {
            var mergedResult = new List<AstNode>(o1);
            mergedResult.AddRange(o2);
            return mergedResult;
        }
    }
}
