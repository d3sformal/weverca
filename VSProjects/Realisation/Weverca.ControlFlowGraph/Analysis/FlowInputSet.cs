using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.Analysis
{
    /// <summary>
    /// Set of FlowInfo used as input for statement analysis.
    /// </summary>
    /// <typeparam name="FlowInfo">Type of object which hold information collected during statement analysis.</typeparam>
    public class FlowInputSet<FlowInfo>
    {
        public IEnumerable<FlowInfo> CollectedInfo { get { return collectedInfo.Values; } }

        protected Dictionary<object,FlowInfo> collectedInfo=new Dictionary<object,FlowInfo>();

        public override bool Equals(object obj)
        {
            var o = obj as FlowInputSet<FlowInfo>;
            if (o == null)
                return false;

            var sameCount=collectedInfo.Count == o.collectedInfo.Count;
            var sameEls = !collectedInfo.Except(o.collectedInfo).Any();
            return sameCount && sameEls;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
