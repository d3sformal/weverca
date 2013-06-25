using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.Analysis
{
    class CallDispatchLevel
    {
        public AnalysisCallContext CurrentContext { get; private set; }

        private int _position;
        private readonly CallInfo[] _dispatches;

        private List<AnalysisCallContext> _result = new List<AnalysisCallContext>();


        AnalysisServices _services;

        public CallDispatchLevel(IEnumerable<CallInfo> dispatches,AnalysisServices services)
        {
            _dispatches = dispatches.ToArray();
            _services = services;
            
            setCurrentDispatch(_position);
        }

        public CallDispatchLevel(CallInfo dispatch,AnalysisServices services)
            : this(new CallInfo[] { dispatch },services)
        {
        }

        public bool ShiftToNext()
        {
            _result.Add(CurrentContext);
            ++_position;
            if (_position >= _dispatches.Length)
            {
                return false;
            }

            setCurrentDispatch(_position);
            return true;
        }

        public AnalysisCallContext[] GetResult()
        {
            if (_result.Count != _dispatches.Length)
                throw new InvalidOperationException("Cannot get result in given dispatch level state");

            return _result.ToArray();
        }


        private void setCurrentDispatch(int position)
        {
            CurrentContext = createContext(_dispatches[position]);

        }

        private AnalysisCallContext createContext(CallInfo dispatch)
        {
            var context = new AnalysisCallContext(dispatch.InSet,dispatch.MethodGraph,_services);
            return context;
        }
    }
}
