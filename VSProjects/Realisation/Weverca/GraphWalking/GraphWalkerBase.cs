﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Analysis;
using Weverca.Output;

namespace Weverca.GraphWalking
{
    /// <summary>
    /// Base class for implementing output builders based on walking program point graphs
    /// </summary>
    abstract class GraphWalkerBase
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
        protected abstract void onWalkPoint(ProgramPoint point);

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
        internal void Run(OutputBase output)
        {
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }
            Output = output;

            walkCall(_entryGraph);
        }

        #region Private walking routines

        /// <summary>
        /// Walk given program point graph of call
        /// </summary>
        /// <param name="callPpGraph">Program point of walked call</param>
        private void walkCall(ProgramPointGraph callPpGraph)
        {
            pushCall(callPpGraph);

            var visitedPoints = new HashSet<ProgramPoint>();
            var pointsToVisit = new Queue<ProgramPoint>();
            visitPoint(callPpGraph.Start, pointsToVisit, visitedPoints);

            while (pointsToVisit.Count > 0)
            {
                var point = pointsToVisit.Dequeue();
                onWalkPoint(point);

                //walk all call extensions
                foreach (var call in point.ContainedCallExtensions)
                {
                    foreach (var branch in call.Branches)
                    {
                        walkCall(branch);
                    }
                }
                
                foreach (var include in point.ContainedIncludeExtensions)
                {
                    foreach (var branch in include.Branches)
                    {
                        visitPoint(branch.Start, pointsToVisit, visitedPoints);
                    }
                }

                foreach (var child in point.Children)
                {
                    visitPoint(child, pointsToVisit, visitedPoints);
                }
            }

            popCall();
        }
        
        /// <summary>
        /// Enqueue point to pointsToVisit if it already hasn't been visited
        /// </summary>
        /// <param name="point">Point to be visited</param>
        /// <param name="pointsToVisit">Storage of points that will be visited</param>
        /// <param name="visitedPoints">Already visited points</param>
        private void visitPoint(ProgramPoint point, Queue<ProgramPoint> pointsToVisit, HashSet<ProgramPoint> visitedPoints)
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
