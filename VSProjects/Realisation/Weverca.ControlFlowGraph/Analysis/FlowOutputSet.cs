using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.Analysis
{
    /// <summary>
    /// Set of FlowInfo used as output from statement analysis.
    /// NOTE: Provides API for call dispatching, type resolving and include dispatching.
    /// </summary>
    /// <typeparam name="FlowInfo">Type of object which hold information collected during statement analysis.</typeparam>
    public class FlowOutputSet<FlowInfo> : FlowInputSet<FlowInfo>
    {
        /// <summary>
        /// Call dispatches added into output set
        /// </summary>
        public IEnumerable<CallDispatch> CallDispatches { get; private set; }


        private FlowOutputSet(FlowOutputSet<FlowInfo> output)
        {
            //TODO copy all info elements
            collectedInfo = new Dictionary<object,FlowInfo>(output.collectedInfo);
        }

        public FlowOutputSet()
        {
        }

        public void SetInfo(object key, FlowInfo info)
        {
            collectedInfo[key] = info;
        }

        public bool TryGetInfo(object key,out FlowInfo info)
        {
            return collectedInfo.TryGetValue(key, out info);            
        }
  
        /// <summary>
        /// Set flow dispatch to given calls
        /// </summary>
        /// <param name="calls">Calls to dispatch</param>
        public void Dispatch(params CallDispatch[] calls)
        {
            throw new NotImplementedException();
        }        

        internal FlowOutputSet<FlowInfo> Copy()
        {
            return new FlowOutputSet<FlowInfo>(this);
        }
    }
}
