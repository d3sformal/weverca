using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing
{
    
    abstract class IndicatorProcessor:MetricProcessor<ConstructIndicator,bool>
    {
        protected override bool merge(bool r1, bool r2)
        {
            return r1 || r2;
        }

        protected override ICollection<AstNode> merge(ICollection<AstNode> o1, ICollection<AstNode> o2)
        {
            var merged = new List<AstNode>(o1);
            merged.AddRange(o2);

            return merged;
        }
    }
}
