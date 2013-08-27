using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.Analysis
{
    /// <summary>
    /// Specifies type of dispatch (In analyzis there are multiple types of dispatch e.g. include is handled as special type of call)
    /// </summary>
    public enum DispatchType
    {
        /// <summary>
        /// There can be multiple calls processed at one level         
        /// </summary>
        ParallelCall,
        /// <summary>
        /// There can be processed multiple includes processed at one level
        /// NOTE:
        ///     This dispatch type doesn't increase call stack depth
        /// </summary>
        ParallelInclude,
    }

    /// <summary>
    /// Handle multiple call dispatches on single stack level
    /// </summary>
    class DispatchLevel
    {
        #region Private members
        /// <summary>
        /// Available analysis services
        /// </summary>
        private readonly AnalysisServices _services;
        /// <summary>
        /// All dispatches on level
        /// </summary>
        private readonly DispatchInfo[] _dispatches;
        /// <summary>
        /// Index of currently processed dispatch
        /// </summary>
        private int _dispatchIndex;
        /// <summary>
        /// Results of every dispatch in level
        /// </summary>        
        private List<AnalysisDispatchContext> _callResults = new List<AnalysisDispatchContext>();

        #endregion

        /// <summary>
        /// Type of dispatch level
        /// </summary>
        public readonly DispatchType DispatchType;
        

        /// <summary>
        /// Current analysis call context 
        /// </summary>
        public AnalysisDispatchContext CurrentContext { get; private set; }

        /// <summary>
        /// Create call dispatch level from given dispatches 
        /// </summary>
        /// <param name="dispatches">Dispaches at same stack level</param>
        /// <param name="services">Available services</param>
        public DispatchLevel(IEnumerable<DispatchInfo> dispatches, AnalysisServices services, DispatchType dispatchType)
        {            
            _dispatches = dispatches.ToArray();
            _services = services;
            DispatchType = dispatchType;

            setCurrentDispatch(_dispatchIndex);
        }

        /// <summary>
        /// Create call dispatch level from given dispatch
        /// </summary>
        /// <param name="dispatch">Single dispatch in level</param>
        /// <param name="services">Available services</param>
        public DispatchLevel(DispatchInfo dispatch, AnalysisServices services, DispatchType dispatchType)
            : this(new DispatchInfo[] { dispatch }, services, dispatchType)
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
        public AnalysisDispatchContext[] GetResult()
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
        private AnalysisDispatchContext createContext(DispatchInfo dispatch)
        {
            var context = new AnalysisDispatchContext(dispatch.MethodGraph, _services, DispatchType, dispatch.InSet);
            return context;
        }
    }
}
