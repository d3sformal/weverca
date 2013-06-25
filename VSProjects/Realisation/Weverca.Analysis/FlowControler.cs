using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.Analysis
{
    public struct FlowControler
    {
        public readonly FlowInputSet InSet;
        public readonly FlowOutputSet OutSet;
        public bool HasCallDispatch { get { return _dispatches.Count > 0; } }
        public IEnumerable<CallInfo> CallDispatches { get { return _dispatches; } }

        private List<CallInfo> _dispatches;

        public FlowControler(FlowInputSet inSet,FlowOutputSet outSet)
        {
            _dispatches= new List<CallInfo>();
            InSet = inSet;
            OutSet = outSet;
        }


        public void AddDispatch(CallInfo dispatch)
        {
            _dispatches.Add(dispatch);
        }

        public void AddDispatch(IEnumerable<CallInfo> dispatches)
        {
            _dispatches.AddRange(dispatches);
        }


        public void FillOutputFromInSet()
        {
            throw new NotImplementedException();
        }
    }
}
