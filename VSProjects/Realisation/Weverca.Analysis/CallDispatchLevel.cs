using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.Analysis
{
    /// <summary>
    /// Handle multiple call dispatches on single stack level
    /// </summary>
    class CallDispatchLevel
    {
        #region Private members
        /// <summary>
        /// Available analysis services
        /// </summary>
        private readonly AnalysisServices _services;
        /// <summary>
        /// All dispatches on level
        /// </summary>
        private readonly CallInfo[] _dispatches;
        /// <summary>
        /// Index of currently processed dispatch
        /// </summary>
        private int _dispatchIndex;
        /// <summary>
        /// Results of every dispatch in level
        /// </summary>        
        private List<AnalysisCallContext> _callResults = new List<AnalysisCallContext>();

        #endregion

        /// <summary>
        /// Current analysis call context 
        /// </summary>
        public AnalysisCallContext CurrentContext { get; private set; }


        /// <summary>
        /// Create call dispatch level from given dispatches 
        /// </summary>
        /// <param name="dispatches">Dispaches at same stack level</param>
        /// <param name="services">Available services</param>
        public CallDispatchLevel(IEnumerable<CallInfo> dispatches, AnalysisServices services)
        {
            _dispatches = dispatches.ToArray();
            _services = services;

            setCurrentDispatch(_dispatchIndex);
        }

        /// <summary>
        /// Create call dispatch level from given dispatch
        /// </summary>
        /// <param name="dispatch">Single dispatch in level</param>
        /// <param name="services">Available services</param>
        public CallDispatchLevel(CallInfo dispatch, AnalysisServices services)
            : this(new CallInfo[] { dispatch }, services)
        {
        }

        /// <summary>
        /// Shift to next dispatch in level
        /// </summary>
        /// <returns>False if there is no next dispatch, true otherwise</returns>
        public bool ShiftToNextDispatch()
        {
            _callResults.Add(CurrentContext);
            ++_dispatchIndex;
            if (_dispatchIndex >= _dispatches.Length)
            {
                return false;
            }

            setCurrentDispatch(_dispatchIndex);
            return true;
        }

        /// <summary>
        /// Get results from dispatches
        /// </summary>
        /// <returns>Dispatches results</returns>
        public AnalysisCallContext[] GetResult()
        {
            if (_callResults.Count != _dispatches.Length)
                throw new InvalidOperationException("Cannot get result in given dispatch level state");

            return _callResults.ToArray();
        }

        /// <summary>
        /// Set current dispatch according to dispatchIndex
        /// </summary>
        /// <param name="dispatchIndex">Index of dispatch to set</param>
        private void setCurrentDispatch(int dispatchIndex)
        {
            CurrentContext = createContext(_dispatches[dispatchIndex]);

        }

        /// <summary>
        /// Create call context for given dispatch
        /// </summary>
        /// <param name="dispatch">Call dispatch</param>
        /// <returns>Created context</returns>
        private AnalysisCallContext createContext(CallInfo dispatch)
        {
            var context = new AnalysisCallContext(dispatch.InSet, dispatch.MethodGraph, _services);
            return context;
        }
    }
}
