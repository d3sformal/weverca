using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Weverca.Analysis.ProgramPoints;

namespace Weverca.Analysis
{
    /// <summary>
    /// Specifies type of dispatch (In analyzis there are multiple types of dispatch e.g. include is handled as special type of call)
    /// </summary>
    public enum ExtensionType
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

    public class FlowExtension
    {
        public readonly ProgramPointBase Owner;

        public ExtensionType Type { get; internal set; }

        public IEnumerable<object> Keys { get { return _extensions.Keys; } }

        public IEnumerable<ProgramPointGraph> Branches { get { return _extensions.Values; } }

        public bool IsConnected { get { return _extensions.Count > 0; } }

        internal readonly ExtensionSinkPoint Sink;

        Dictionary<object, ProgramPointGraph> _extensions = new Dictionary<object, ProgramPointGraph>();

        internal FlowExtension(ProgramPointBase owner)
        {
            Owner = owner;

            if (Owner is ExtensionSinkPoint)
            {
                //Because of avoiding recursion - extension sink is also sink for self
                Sink = owner as ExtensionSinkPoint;
            }
            else {
                Sink = new ExtensionSinkPoint(this);
            }
        }

        internal void Add(object key, ProgramPointGraph ppGraph)
        {
            if (!IsConnected)
            {
                connect();
            }

            Owner.Services.SetServices(ppGraph);

            _extensions.Add(key, ppGraph);

            //connect ppGraph into current graph
            Owner.AddFlowChild(ppGraph.Start);
            ppGraph.End.AddFlowChild(Sink);
        }

        internal void Remove(object key)
        {
            if (!IsConnected)
            {
                //nothing to remove
                return;
            }

            throw new NotImplementedException("Check for disconnection");
        }

        private void connect()
        {
            //copy children
            var children = Owner.FlowChildren.ToArray();

            foreach (var child in children)
            {
                //disconnect original children
                Owner.RemoveFlowChild(child);

                //reconnect behind sink
                Sink.AddFlowChild(child);
            }
        }

        
    }
}
