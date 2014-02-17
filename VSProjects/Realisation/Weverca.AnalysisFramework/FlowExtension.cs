using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.AnalysisFramework
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
        /// <summary>
        /// There can be evaluated multiple sources processed at one level
        /// NOTE:
        ///     This dispatch type doesn't increase call stack depth
        /// </summary>
        ParallelEval,
    }

    public class FlowExtension
    {
        public readonly ProgramPointBase Owner;

        public IEnumerable<object> Keys { get { return _extensions.Keys; } }

        public IEnumerable<ExtensionPoint> Branches { get { return _extensions.Values; } }

        public bool IsConnected { get { return _extensions.Count > 0; } }

        internal readonly ExtensionSinkPoint Sink;

        Dictionary<object, ExtensionPoint> _extensions = new Dictionary<object, ExtensionPoint>();

        internal FlowExtension(ProgramPointBase owner)
        {
            Owner = owner;

            if (Owner is ExtensionSinkPoint)
            {
                //Because of avoiding recursion - extension sink is also sink for self
                Sink = owner as ExtensionSinkPoint;
            }
            else
            {
                Sink = new ExtensionSinkPoint(this);
            }
        }

        internal ExtensionPoint Add(object key, ProgramPointGraph ppGraph, ExtensionType type)
        {
            if (!IsConnected)
            {
                connect();
            }

            Owner.Services.SetServices(ppGraph);
            var extension = new ExtensionPoint(Owner, ppGraph, type);
            Owner.Services.SetServices(extension);

            _extensions.Add(key, extension);

            //connect ppGraph into current graph
            Owner.AddFlowChild(extension);
            extension.Graph.End.AddFlowChild(Sink);

            return extension;
        }

        internal void Remove(object key)
        {
            if (!IsConnected)
            {
                //nothing to remove
                return;
            }

            ExtensionPoint extension;
            if (!_extensions.TryGetValue(key, out extension))
                //there is nothing with given key - don't need to remove branch
                return;

            _extensions.Remove(key);
            Owner.RemoveFlowChild(extension);
            extension.Graph.End.RemoveFlowChild(Sink);

            if (_extensions.Count == 0)
                disconnect();
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

        private void disconnect()
        {
            //copy children
            var children = Sink.FlowChildren.ToArray();

            foreach (var child in children)
            {
                //put children back 
                Owner.AddFlowChild(child);

                //disconnect from sink
                Sink.RemoveFlowChild(child);
            }
        }
    }
}
