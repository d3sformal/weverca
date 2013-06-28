using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.Analysis
{
    /// <summary>
    /// Controller used for accessing flow sets and dispatching
    /// </summary>
    public class FlowController
    {
        /// <summary>
        /// Input set
        /// </summary>
        public readonly FlowInputSet InSet;
        /// <summary>
        /// Output set
        /// </summary>
        public readonly FlowOutputSet OutSet;
        /// <summary>
        /// Determine that controller has call dispatch
        /// </summary>
        public bool HasCallDispatch { get { return _dispatches.Count > 0; } }
        /// <summary>
        /// Available call dispatches
        /// </summary>
        public IEnumerable<CallInfo> CallDispatches { get { return _dispatches; } }

        /// <summary>
        /// Stored dispatches
        /// </summary>
        private List<CallInfo> _dispatches;

        /// <summary>
        /// Create flow controller for given input and output set
        /// </summary>
        /// <param name="inSet">Input set</param>
        /// <param name="outSet">Output set</param>
        internal FlowController(FlowInputSet inSet,FlowOutputSet outSet)
        {
            _dispatches= new List<CallInfo>();
            InSet = inSet;
            OutSet = outSet;
        }

        /// <summary>
        /// Add call dispatch into controller
        /// </summary>
        /// <param name="dispatch">Call dispatch</param>
        public void AddDispatch(CallInfo dispatch)
        {
            _dispatches.Add(dispatch);
        }

        /// <summary>
        /// Add multiple dispatches into controller
        /// </summary>
        /// <param name="dispatches">Call dispatches</param>
        public void AddDispatch(IEnumerable<CallInfo> dispatches)
        {
            _dispatches.AddRange(dispatches);
        }

    }
}
