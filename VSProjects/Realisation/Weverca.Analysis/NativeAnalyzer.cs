using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;
using PHP.Core.Parsers;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis
{
    public delegate void NativeAnalyzerMethod(FlowControler flow);

    public class NativeAnalyzer:LangElement
    {
        static private Position virtualPosition = new Position();

        internal readonly NativeAnalyzerMethod Method;

        public NativeAnalyzer(NativeAnalyzerMethod method)
            :base(virtualPosition)
        {
            Method = method;
        }

        public override void VisitMe(TreeVisitor visitor)
        {
            var partialWorker = visitor as Expressions.PartialWalker;

            if (partialWorker == null)
            {
                return;
            }
            else
            {
                partialWorker.VisitNative(this);
            }
        }
    }
}
