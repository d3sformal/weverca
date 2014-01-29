using System;
using System.Collections.Generic;

using Weverca.AnalysisFramework;
using Weverca.Output.Output;

namespace Weverca.Output.GraphWalking
{
    /// <summary>
    /// Base class for implementing output builders based on walking program point graphs
    /// </summary>
    public abstract class GraphWalkerBase
    {
        #region Private fields

        /// <summary>
        /// Entry program point graph, where walking starts
        /// </summary>
        readonly ProgramPointGraph _entryGraph;

        /// <summary>
        /// Callstack holding info about current stack trace
        /// </summary>
        readonly CallStack _callStack = new CallStack();

        #endregion

        #region API for implementors

        /// <summary>
        /// Callstack holding info about current stack trace
        /// </summary>
        protected ReadonlyCallStack CallStack { get { return _callStack; } }

        /// <summary>
        /// Output for printing info from walking
        /// </summary>
        protected OutputBase Output { get; private set; }

        /// <summary>
        /// Method handler called after call is pushed onto stack
        /// </summary>
        protected abstract void afterPushCall();

        /// <summary>
        /// Method handler called after program point is processed
        /// <remarks>Every program point is processed only once at on call level</remarks>
        /// </summary>
        /// <param name="point">Processed program point</param>
        protected abstract void onWalkPoint(ProgramPointBase point);

        /// <summary>
        /// Method handler called before call is poped from stack
        /// </summary>
        protected abstract void beforePopCall();

        #endregion

        /// <summary>
        /// Create graph walker
        /// </summary>
        /// <param name="entryGraph">Program point graph, where walking starts</param>
        protected GraphWalkerBase(ProgramPointGraph entryGraph)
        {
            if (entryGraph == null)
            {
                throw new ArgumentNullException("entryGraph");
            }

            _entryGraph = entryGraph;
        }

        /// <summary>
        /// Rung graph walking with printing info into given output
        /// </summary>
        /// <param name="output">Output for info generated when walking graph</param>
        public void Run(OutputBase output)
        {
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }
            Output = output;
            var visitedPoints = new HashSet<ProgramPointBase>();
            walkCall(_entryGraph, visitedPoints);
        }

        #region Private walking routines

        /// <summary>
        /// Walk given program point graph of call
        /// </summary>
        /// <param name="callPpGraph">Program point of walked call</param>
        private void walkCall(ProgramPointGraph callPpGraph, HashSet<ProgramPointBase> visitedPoints)
        {
            pushCall(callPpGraph);

            
            var pointsToVisit = new Queue<ProgramPointBase>();
            visitPoint(callPpGraph.Start, pointsToVisit, visitedPoints);

            while (pointsToVisit.Count > 0)
            {
                var point = pointsToVisit.Dequeue();
                onWalkPoint(point);


                if (point.Extension.IsConnected)
                {
                    walkExtension(point.Extension, pointsToVisit, visitedPoints);
                }
                else
                {
                    //walk flow children
                    foreach (var child in point.FlowChildren)
                    {
                        visitPoint(child, pointsToVisit, visitedPoints);
                    }
                }
            }

            popCall();
        }

        private void walkExtension(FlowExtension extension, Queue<ProgramPointBase> pointsToVisit, HashSet<ProgramPointBase> visitedPoints)
        {
            foreach (var branch in extension.Branches)
            {
                switch (branch.Type)
                {
                    case ExtensionType.ParallelCall:
                        walkCall(branch.Graph, visitedPoints);
                        break;
                    case ExtensionType.ParallelInclude:
                        visitPoint(branch.Graph.Start, pointsToVisit, visitedPoints);
                        break;
                    default:
                        throw new NotImplementedException("Walking of this extension type is not implemented yet");
                }
            }
        }



        /// <summary>
        /// Enqueue point to pointsToVisit if it already hasn't been visited
        /// </summary>
        /// <param name="point">Point to be visited</param>
        /// <param name="pointsToVisit">Storage of points that will be visited</param>
        /// <param name="visitedPoints">Already visited points</param>
        private void visitPoint(ProgramPointBase point, Queue<ProgramPointBase> pointsToVisit, HashSet<ProgramPointBase> visitedPoints)
        {
            if (!visitedPoints.Add(point))
            {
                //point has already been visited
                return;
            }

            pointsToVisit.Enqueue(point);
        }

        /// <summary>
        /// Push call program point graph onto stack
        /// </summary>
        /// <param name="callPpGraph">Call program point graph</param>
        private void pushCall(ProgramPointGraph callPpGraph)
        {
            _callStack.Push(callPpGraph);
            afterPushCall();
        }

        /// <summary>
        /// Pop call program point graph from stack
        /// </summary>
        private void popCall()
        {
            beforePopCall();
            _callStack.Pop();
        }

        #endregion
    }
}
