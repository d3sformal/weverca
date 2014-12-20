/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


﻿#define IMPORVEDFIXPOINT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.AnalysisFramework
{
	/// <summary>
	/// List of work items that has to be processed within analysis. Optimize processing
	/// order because of effectivity of analysis.
	/// </summary>
	public abstract class WorkList
	{
		/// <summary>
		/// Add work into list
		/// </summary>
		/// <param name="work">Added work</param>
		public abstract void AddChildren (ProgramPointBase work);

		/// <summary>
		/// Get work from list
		/// </summary>
		/// <returns>Work that should be processed</returns>
		internal abstract ProgramPointBase GetWork ();

        /// <summary>
        /// Adds entry point to the worklist.
        /// </summary>
        /// <param name="entryPoint">entry point of the analysis.</param>
		public abstract void AddEntryPoint(ProgramPointBase entryPoint, ProgramPointBase exitPoint);

		/// <summary>
		/// Determine that any work
		/// </summary>
		public virtual bool HasWork { get { return _containedPoints.Count > 0; } }

		/// <summary>
		/// Set of all points that are cotained within work list
		/// </summary>
		protected readonly HashSet<ProgramPointBase> _containedPoints = new HashSet<ProgramPointBase> ();

        /// <summary>
        /// Direction of the analysis.
        /// </summary>
        protected readonly AnalysisDirection Direction;

        protected WorkList(AnalysisDirection Direction) { this.Direction = Direction; }

        protected IEnumerable<ProgramPointBase> GetOutputPoints(ProgramPointBase point)
        {
            switch (Direction)
            {
                case AnalysisDirection.Forward:
                    return point.FlowChildren;
                case AnalysisDirection.Backward:
                    return point.FlowParents;
                default:
                    throwUnknownDirection();
                    return null;
            }
        }

        private void throwUnknownDirection()
        {
            throw new NotSupportedException("Analysis doesn't support: " + Direction);
        }

		/// <summary>
		/// Gets the instance of Worklist that should be used for the analysis.
		/// </summary>
        /// <param name="nextPhase">Indicates whether the worklist will be used for the next phase.</param>
		/// <returns>The instance of the worklist.</returns>
		public static WorkList GetInstance (bool nextPhase = false, AnalysisDirection Direction = AnalysisDirection.Forward)
		{
			//return new WorklistNaive(Direction);
			//return new WorkListReordering1(Direction);
            //return new WorklistReordering2(nextPhase, Direction);
			return new WorkListReorderingSegments (nextPhase, Direction);
		}

        /// <summary>
		/// Implements naive ordering of program points in the worklist (no reordering).
        /// </summary>
        private class WorklistNaive : WorkList
        {
            /// <summary>
            /// Work queu of points that can be currently processed
            /// </summary>
            private readonly Queue<ProgramPointBase> _workQueue = new Queue<ProgramPointBase>();

            public WorklistNaive(AnalysisDirection Direction)
                : base(Direction) { }

			public override void AddEntryPoint(ProgramPointBase entryPoint, ProgramPointBase exitPoint)
            {
                AddWork(entryPoint);
            }

            /// <summary>
            /// Add work into list
            /// </summary>
            /// <param name="work">Added work</param>
            private void AddWork(ProgramPointBase work)
            {
                if (!_containedPoints.Add(work))
                    //don't need to add same work twice
                    return;

                _workQueue.Enqueue(work);
            }

            public override void AddChildren(ProgramPointBase work)
            {
                foreach (var child in GetOutputPoints(work))
                {
                    AddWork(child);
                }
            }

            /// <inheritdoc/>
            internal override ProgramPointBase GetWork()
            {
                var work = _workQueue.Dequeue();
                _containedPoints.Remove(work);

                return work;
            }
        }

		/// <summary>
		/// Worklist algorithm that tries to reorder program points in the worklist in order to not process program point
		/// if it is reachable from other program points in the worklist - if the output of these program points is
		/// changed, the program point must be recomputed.
		/// 
		/// Work points are classified as open/close/normal according to number of inputs and outputs.
		/// Every point with more than one input is close point - is needed to have processed all possible open points before.
		/// Open points has parents with more than one output - at a time only single opened branch is processed within queue.
		/// 
		/// Note that this worklist is in practise often not optimal. This happens especially
		/// in present of if branching.
		/// </summary>
		private class WorkListReordering1 : WorkList
		{
			/// <summary>
			/// Points which parents has more than one output
			/// </summary>
			private readonly Stack<ProgramPointBase> _openStack = new Stack<ProgramPointBase> ();
			/// <summary>
			/// Points which has more than one input
			/// </summary>
			private readonly Stack<ProgramPointBase> _closeStack = new Stack<ProgramPointBase> ();
			/// <summary>
			/// Work queu of points that can be currently processed
			/// </summary>
			private readonly Queue<ProgramPointBase> _workQueue = new Queue<ProgramPointBase> ();

            public WorkListReordering1(AnalysisDirection Direction)
                : base(Direction) {}

			public override void AddEntryPoint(ProgramPointBase entryPoint, ProgramPointBase exitPoint)
            {
                AddWork(entryPoint);
            }

            /// <summary>
            /// Add work into list
            /// </summary>
            /// <param name="work">Added work</param>
			private void AddWork (ProgramPointBase work)
			{
				if (!_containedPoints.Add (work))
					//don't need to add same work twice
					return;
				#if IMPORVEDFIXPOINT
				if (work.FlowParentsCount > 1) {
					//close point has been found
					_closeStack.Push (work);
				} else if (work.FlowParentsCount == 1 && work.FlowParents.First ().FlowChildrenCount > 1) {
					//open point has been found
					_openStack.Push (work);
				} else
					#endif
				{
					//normal point
					_workQueue.Enqueue (work);
				}
			}

            public override void AddChildren(ProgramPointBase work)
            {
                foreach (var child in GetOutputPoints(work))
                {
                    AddWork(child);
                }
            }

			/// <inheritdoc/>
			internal override ProgramPointBase GetWork ()
			{
				ProgramPointBase result;
				if (_workQueue.Count > 0) {
					//dequeue normal points befor other kinds of points
					result = _workQueue.Dequeue ();
				} else if (_openStack.Count > 0) {
					//all open points has to be processed before close points
					result = _openStack.Pop ();
				} else {
					//close points are processed at last
					result = _closeStack.Pop ();
				}

				_containedPoints.Remove (result);

				return result;
			}
		}

		class WorkQueue
		{
			public readonly Queue<ProgramPointBase> _queue = new Queue<ProgramPointBase>();
			public readonly WorkQueue _parent;

			public WorkQueue(WorkQueue parent) { _parent = parent; }
		}

		/// <summary>
		/// Worklist algorithm that tries to reorder program points in the worklist in order to not process program point
		/// if it is reachable from other program points in the worklist - if the output of these program points is
		/// changed, the program point must be recomputed.
		/// 
		/// Note that this worklist is in practise often not optimal. This happens especially
		/// in present of cycles.
		/// </summary>
		private class WorklistReordering2 : WorkList
		{
			private readonly bool nextPhase;
			private WorkQueue hQueue = new WorkQueue (null);
			private ProgramPointBase prevParent = null;

			internal WorklistReordering2(bool nextPhase, AnalysisDirection Direction) : base(Direction) { this.nextPhase = nextPhase; }

			public override void AddEntryPoint(ProgramPointBase entryPoint, ProgramPointBase exitPoint)
			{
				if (!_containedPoints.Add(entryPoint))
					//don't need to add same work twice
					return;
				hQueue._queue.Enqueue(entryPoint);
			}



			private void AddWork (ProgramPointBase work)
			{
				if (!_containedPoints.Add (work))
					//don't need to add same work twice
					return;

				// open point
				if (work.FlowParents.Count () > 0 && work.FlowParents.First ().FlowChildrenCount > 1 && work.FlowParents.First () != prevParent) {
					hQueue = new WorkQueue (hQueue);
				}

				if (work.FlowParentsCount > 1 && hQueue._parent != null) { // close point
					hQueue._parent._queue.Enqueue (work);
				} else { 
					//normal point
					hQueue._queue.Enqueue (work);
					//_workQueue.Enqueue(work);
				}

				if (work.FlowParents.Count () > 0)
					prevParent = work.FlowParents.First ();
			}

			public override void AddChildren(ProgramPointBase work)
			{
				foreach (var child in GetOutputPoints(work))
				{
					if (!nextPhase || !unreachable(child))
						AddWork(child);
				}
			}

		

			/// <summary>
			/// Determine whether the specified program point is unreachable in the first phase.
			/// </summary>
			private bool unreachable(ProgramPointBase point)
			{
				return (point.InSet == null) && (point.OutSet == null);
			}


			/// <inheritdoc/>
			internal override ProgramPointBase GetWork ()
			{
				while (hQueue._queue.Count == 0 && hQueue._parent != null) {
					hQueue = hQueue._parent;
				}
				var result = hQueue._queue.Dequeue ();

				_containedPoints.Remove (result);

				return result;
			}
		}

        /// <summary>
		/// Worklist algorithm that tries to reorder program points in the worklist in order to not process program point
		/// if it is reachable from other program points in the worklist. It is useless to process this program point - if the output of
        /// a program point that precedes the progrma point in the worklist will be changed, the program point will have to be again processed.
		/// 
		/// To reorder program points, it uses the concept of worklist segments. Fixpoint of all program points in the segment
		/// must be reached before the program point after the segment is processed.
		/// Segments are created during creating of control-flow graph, e.g., for if, while, for, and foreach statemetns.
        /// 
        /// It works in the following way: for each segment, there is a program point afterSegment. First, this program point is added
        /// to the current worklist queue and added to the set unreachableAfterSegmentPoints - they will be processed after fixpoint of the segment
        /// is reached and they are reachable from some program point in the segment. Then, it is created new queue for program points 
        /// in the segment and this queue is used for processing program points of this segment. If the queue becomes empty, it means 
        /// that the fixpoint of the segment is reached. The queue for this segment is removed and the processing continues with
        /// the previsous queue. 
        /// 
        /// Note that the previsous queue contains the program point afterSegment, but this program point will be processed only if
        /// it is reachable from some program point in the segment.
        /// 
        /// Note that if the program point already is in the worklist, it is not added again.
        /// 
        /// Example 1:
        /// (1) if () {
        /// (2)     $a = 1;
        /// (3) } else {
        /// (4)     while (true) {
        /// (5)        $i++;
        /// (6)     }
        /// (7)    $a = 2;
        /// (8) }
        /// (9) $b = 1;
        /// 
        /// 1. Adding children of program point (1): program point (9) ends the segment of the if statement. It is added to the queue queue1 and to the unreachableAfterSegmentPoints.
        /// New queue queue2 is created. Program points (2), (4) are added to queue2.
        /// 
        /// 2. Adding children of PP (2): child is PP (9). It already is in the worklist, it is not added. But it is removed from unreachableAfterSegmentPoints.
        /// 
        /// 3. Adding children of PP (4): program point (7) ends the segment of the while statement. It is added to the queue2 and to  unreachableAfterSegmentPoints.
        /// Queue3 is created for the new segment. Program point (5) is added.
        /// 
        /// 4. Removing queue3: when the fixpoint is reached, the queue3 is empty, it is removed and queue2 is used.
        /// Note that PP (7) is in queue2, but as it is not reachable the while, it is still in unreachableAfterSegmentPoints and it is not processed.
        /// 
        /// 5. Removing queue2: PP (7) is the only program point is queue2, however, because it is also in unreachableAfterSegmentPoints, queue2 is considered empty
        /// and it is removed and queue1 is used.
        /// 
        /// 6. PP (9) is in queue1 and it is not in unreachableAfterSegmentPoints - it is fetched by getWork.
        /// 
        /// End Example 1
        /// 
		/// 
        /// TODO: ordering for backward analysis!
        /// </summary>
		private class WorkListReorderingSegments : WorkList
		{
            private readonly bool nextPhase;
			private WorkQueue hQueue = new WorkQueue (null);
            /// <summary>
            /// Program points that were added to the worklist because they are ends of segments in the worklist 
            /// and which are not yet reachable.
            /// </summary>
            private readonly HashSet<ProgramPointBase> unreachableAfterSegmentPoints = new HashSet<ProgramPointBase>();
			private ProgramPointBase exitPoint;

            internal WorkListReorderingSegments(bool nextPhase, AnalysisDirection Direction) : base(Direction) { this.nextPhase = nextPhase; }

			public override void AddEntryPoint(ProgramPointBase entryPoint, ProgramPointBase exitPoint)
            {
				this.exitPoint = exitPoint;
				_containedPoints.Add (exitPoint);
                if (!_containedPoints.Add(entryPoint))
                    //don't need to add same work twice
                    return;
                hQueue._queue.Enqueue(entryPoint);
            }



            private bool AddPoint(ProgramPointBase work)
            {
				if (work == exitPoint)
					return false;
                
                // Do not add program point that is already in the worklist.
                if ((!nextPhase || !unreachable(work)) && _containedPoints.Add(work))
                {
                    hQueue._queue.Enqueue(work);
                    return true;
                }

                return false;
            }

            public override void AddChildren(ProgramPointBase work)
            {
				if (work is RCallPoint) 
				{
					if (AddPoint(work.Extension.Sink))
					    unreachableAfterSegmentPoints.Add(work.Extension.Sink);

					// create queue for the branching
					hQueue = new WorkQueue(hQueue);
				}

                if (work.AfterWorklistSegment != null) 
                { 
                    // add point after branching
                    if (AddPoint(work.AfterWorklistSegment))
                        unreachableAfterSegmentPoints.Add(work.AfterWorklistSegment);

                    // create queue for the branching
                    hQueue = new WorkQueue(hQueue);
                }

                foreach (var child in GetOutputPoints(work))
                {
                    unreachableAfterSegmentPoints.Remove(child);
                    AddPoint(child);
                }
            }

            /// <summary>
            /// Determine whether the specified program point is unreachable in the first phase.
            /// </summary>
            private bool unreachable(ProgramPointBase point)
            {
                return (point.InSet == null) && (point.OutSet == null);
            }

            public override bool HasWork { get { return _containedPoints.Except(unreachableAfterSegmentPoints).Count() > 0; } }

            

			/// <inheritdoc/>
			internal override ProgramPointBase GetWork ()
			{
				while (hQueue._queue.Count == 0 && hQueue._parent != null) {
					hQueue = hQueue._parent;
				}
					
				ProgramPointBase result;
				if (hQueue._queue.Count == 0)
					result = exitPoint;
				else
					result = hQueue._queue.Dequeue ();

				_containedPoints.Remove (result);
                if (unreachableAfterSegmentPoints.Contains(result))
                {
                    unreachableAfterSegmentPoints.Remove(result);
                    return GetWork();
                }

				return result;
			}
		}
	}
}