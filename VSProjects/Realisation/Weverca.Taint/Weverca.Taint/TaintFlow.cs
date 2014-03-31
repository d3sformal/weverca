using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework;

namespace Weverca.Taint
{
    class TaintFlow
    {
        public List<ProgramPointBase> flow = new List<ProgramPointBase>();

        public void addPointToTaintFlow(ProgramPointBase programPoint)
        {
            flow.Add(programPoint);
        }

        public TaintFlow(TaintFlow oldFlow)
        {
            flow = new List<ProgramPointBase>(oldFlow.flow);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var result = new StringBuilder();

            foreach (ProgramPointBase pPoint in flow)
            {
                result.Append("->");
                result.Append(pPoint.OwningPPGraph.OwningScript.FullName + " at position " + pPoint.Partial.Position);
            }

            return result.ToString();
        }

    }

    class TaintInfo
    {
        public bool highPriority = true;
        public List<TaintFlow> possibleTaintFlows = new List<TaintFlow>();

        /// <inheritdoc />
        public override string ToString()
        {
            var result = new StringBuilder();

            foreach (TaintFlow possibleFlow in possibleTaintFlows)
            {
                result.Append("Possible flow: ");
                result.AppendLine(possibleFlow.ToString());
            }

            return result.ToString();
        }
    }
}
