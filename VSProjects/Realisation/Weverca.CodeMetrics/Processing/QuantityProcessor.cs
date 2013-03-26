using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing
{
    abstract class QuantityProcessor: MetricProcessor<Quantity, int>
    {
        /// <summary>
        /// Merging of almost all quantities should be easy. Others can override this beahviour
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        protected override int merge(int r1, int r2)
        {
            return r1+r2;
        }

        /// <summary>
        /// Merging of almost all quantitius should be easy. Others can override this beahviour
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
