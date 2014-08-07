#define IMPORVEDFIXPOINT
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
			//return new WorkListReordering2(Direction);
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
		/// if it is reachable from other program points in the worklist - if the output of these program points is
		/// changed, the program point must be recomputed.
		/// 
		/// To reorder program points, it uses the concept of worklist segments. Fixpoint of all program points in the segment
		/// must be reached before the program point after the segment is processed.
		/// Segments are created during creating of control-flow graph, e.g., for if, while, for, and foreach statemetns.
		/// 
        /// TODO: ordering for backward analysis!
        /// </summary>
		private class WorkListReorderingSegments : WorkList
		{
            private readonly bool nextPhase;
			private WorkQueue hQueue = new WorkQueue (null);
            private readonly HashSet<ProgramPointBase> unreachablePoints = new HashSet<ProgramPointBase>();
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



            private void AddPoint(ProgramPointBase work)
            {
				if (work == exitPoint)
					return;
                unreachablePoints.Remove(work);
                if ((!nextPhase || !unreachable(work)) && _containedPoints.Add(work))
                {
                    hQueue._queue.Enqueue(work);
                }
            }

            public override void AddChildren(ProgramPointBase work)
            {
				if (work is RCallPoint) 
				{
					AddPoint(work.Extension.Sink);
					unreachablePoints.Add(work.Extension.Sink);

					// create queue for the branching
					hQueue = new WorkQueue(hQueue);
				}

                if (work.AfterWorklistSegment != null) 
                { 
                    // add point after branching
                    AddPoint(work.AfterWorklistSegment);
                    unreachablePoints.Add(work.AfterWorklistSegment);

                    // create queue for the branching
                    hQueue = new WorkQueue(hQueue);
                }

                foreach (var child in GetOutputPoints(work))
                {
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

            public override bool HasWork { get { return _containedPoints.Except(unreachablePoints).Count() > 0; } }

            

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
                if (unreachablePoints.Contains(result))
                {
                    unreachablePoints.Remove(result);
                    return GetWork();
                }

				return result;
			}
		}
	}
}
