using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.Analysis
{
    /// <summary>
    /// Program point computed during fix point algorithm.
    /// </summary>
    /// <typeparam name="FlowInfo"></typeparam>
    public class ProgramPoint<FlowInfo>
    {
        public FlowInputSet<FlowInfo> InSet { get; internal set; }
        public FlowOutputSet<FlowInfo> OutSet { get; private set; }
                
        /// <summary>
        /// Determine that some updates has been requested.
        /// </summary>
        internal bool HasUpdate { get; private set; }

        /// <summary>
        /// Requested update for output set.
        /// </summary>
        private FlowOutputSet<FlowInfo> _outSetUpdate;

        /// <summary>
        /// Request update on output set.
        /// </summary>
        /// <param name="outSet"></param>
        internal void UpdateOutSet(FlowOutputSet<FlowInfo> outSet)
        {
            HasUpdate = true;
            _outSetUpdate = outSet;
        }

        /// <summary>
        /// Commit updates on program point. 
        /// </summary>
        /// <returns>True if any changes has been changed, false otherwise.</returns>
        internal bool CommitUpdate()
        {            
            if (!HasUpdate || _outSetUpdate == OutSet)
            {
                HasUpdate = false;
                return false;
            }

            HasUpdate = false;
            OutSet = _outSetUpdate;
            return true;
        }
    }

}
