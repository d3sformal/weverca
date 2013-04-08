﻿using System;
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


    
        public FlowOutputSet()
        {
        }

        /// <summary>
        /// Private copy constructor.
        /// </summary>
        /// <param name="output">Copied set</param>
        private FlowOutputSet(FlowOutputSet<FlowInfo> output)
        {
            collectedInfo = new Dictionary<object, FlowInfo>(output.collectedInfo);
        }


        /// <summary>
        /// Set info for given key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="info"></param>
        public void SetInfo(object key, FlowInfo info)
        {
            collectedInfo[key] = info;
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

        public void FillFrom(FlowInputSet<FlowInfo> inSet)
        {
            foreach (var key in inSet.CollectedKeys)
            {
                FlowInfo info;
                System.Diagnostics.Debug.Assert(inSet.TryGetInfo(key, out info));
                SetInfo(key, info);
            }
        }
    }
}