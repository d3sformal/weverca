using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.Analysis
{
    public struct FlowControler<FlowInfo>
    {
        public readonly FlowInputSet<FlowInfo> InSet;
        public readonly FlowOutputSet<FlowInfo> OutSet;
        public bool HasCallDispatch { get { return _dispatches.Count > 0; } }
        public IEnumerable<CallDispatch<FlowInfo>> CallDispatches { get { return _dispatches; } }

        private List<CallDispatch<FlowInfo>> _dispatches;

        public FlowControler(FlowInputSet<FlowInfo> inSet,FlowOutputSet<FlowInfo> outSet)
        {
            _dispatches= new List<CallDispatch<FlowInfo>>();
            InSet = inSet;
            OutSet = outSet;
        }


        public void AddDispatch(CallDispatch<FlowInfo> dispatch)
        {
            _dispatches.Add(dispatch);
        }

        public void AddDispatch(IEnumerable<CallDispatch<FlowInfo>> dispatches)
        {
            _dispatches.AddRange(dispatches);
        }


        public void FillOutputFromInSet()
        {
            OutSet.FillFrom(InSet);
        }
    }
}
