using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Parsers;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis
{
    /// <summary>
    /// Delegate for native methods called during analysis in context of ProgramPointGraph
    /// </summary>
    /// <param name="flow">Flow controller available for method</param>
    public delegate void NativeAnalyzerMethod(FlowController flow);


    /// <summary>
    /// Lang element used for storing native analyzers in ProgramPointGraphs
    /// </summary>
    public class NativeAnalyzer:LangElement
    {
        /// <summary>
        /// Singleton for empty position - this LangElement is virtual
        /// </summary>
        static private Position virtualPosition = new Position();

        /// <summary>
        /// Stored native analyzer
        /// </summary>
        internal readonly NativeAnalyzerMethod Method;

        /// <summary>
        /// Create NativeAnalyzer with specified method
        /// </summary>
        /// <param name="method"></param>
        public NativeAnalyzer(NativeAnalyzerMethod method)
            :base(virtualPosition)
        {
            Method = method;
        }

        /// <summary>
        /// Override for VisitNative in PartialWalker
        /// </summary>
        /// <param name="visitor">Visitor</param>
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
