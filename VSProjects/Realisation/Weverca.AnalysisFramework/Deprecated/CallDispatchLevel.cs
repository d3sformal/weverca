using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.AnalysisFramework
{
    class CallDispatchLevel<FlowInfo>
    {
        public AnalysisCallContext<FlowInfo> CurrentContext { get; private set; }

        private int _position;
        private readonly CallDispatch<FlowInfo>[] _dispatches;

        private List<AnalysisCallContext<FlowInfo>> _result = new List<AnalysisCallContext<FlowInfo>>();
        AnalysisServices<FlowInfo> _services;

        public CallDispatchLevel(IEnumerable<CallDispatch<FlowInfo>> dispatches, AnalysisServices<FlowInfo> services)
        {
            _dispatches = dispatches.ToArray();
            _services = services;
            setCurrentDispatch(_position);
        }

        public CallDispatchLevel(CallDispatch<FlowInfo> dispatch, AnalysisServices<FlowInfo> services)
            : this(new CallDispatch<FlowInfo>[] { dispatch }, services)
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

        public AnalysisCallContext<FlowInfo>[] GetResult()
        {
            if (_result.Count != _dispatches.Length)
                throw new InvalidOperationException("Cannot get result in given dispatch level state");

            return _result.ToArray();
        }


        private void setCurrentDispatch(int position)
        {
            CurrentContext = createContext(_dispatches[position]);

        }

        private AnalysisCallContext<FlowInfo> createContext(CallDispatch<FlowInfo> dispatch)
        {
            var context = new AnalysisCallContext<FlowInfo>(dispatch.EntryPoint, dispatch.InSet, _services);
            return context;
        }
    }
}
