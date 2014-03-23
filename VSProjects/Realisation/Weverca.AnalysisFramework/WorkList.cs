#define IMPORVEDFIXPOINT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public abstract void AddWork (ProgramPointBase work);

		/// <summary>
		/// Get work from list
		/// </summary>
		/// <returns>Work that should be processed</returns>
		internal abstract ProgramPointBase GetWork ();

		/// <summary>
		/// Determine that any work
		/// </summary>
		public bool HasWork { get { return _containedPoints.Count > 0; } }

		/// <summary>
		/// Set of all points that are cotained within work list
		/// </summary>
		protected readonly HashSet<ProgramPointBase> _containedPoints = new HashSet<ProgramPointBase> ();

		/// <summary>
		/// Gets the instance of Worklist that should be used for the analysis.
		/// </summary>
		/// <returns>The instance of the worklist.</returns>
		public static WorkList GetInstance ()
		{
			return new WorkList1 ();
		}

		/// <summary>
		/// Work points are classified as open/close/normal according to number of inputs and outputs.
		/// Every point with more than one input is close point - is needed to have processed all possible open points before.
		/// Open points has parents with more than one output - at a time only single opened branch is processed within queue.
		/// </summary>
		private class WorkList1 : WorkList
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

			public WorkList1 ()
			{
			}

			/// <inheritdoc />
			public override void AddWork (ProgramPointBase work)
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

		private class Worklist2 : WorkList
		{
			private WorkQueue hQueue = new WorkQueue (null);
			private ProgramPointBase prevParent = null;

			/// <inheritdoc/>
			public override void AddWork (ProgramPointBase work)
			{
				if (!_containedPoints.Add (work))
                //don't need to add same work twice
					return;
            
				// open point
				if (work.FlowParents.Count () > 0 && work.FlowParents.First ().FlowChildrenCount > 1 && work.FlowParents.First () != prevParent) {
					hQueue = new WorkQueue (hQueue);
				}

				/*
            if (work.FlowParentsCount > 1)
            {
                //close point has been found
                _closeStack.Push(work);
            }
            else if (work.FlowParentsCount == 1 && work.FlowParents.First().FlowChildrenCount > 1)
            {
                //open point has been found
                _openStack.Push(work);
            }
            else
            {
                //normal point
                _workQueue.Enqueue(work);
            }
             * */

				// close point
				if (work.FlowParentsCount > 1) {
					hQueue._parent._queue.Enqueue (work);
				} else { 
					//normal point
					hQueue._queue.Enqueue (work);
					//_workQueue.Enqueue(work);
				}

				if (work.FlowParents.Count () > 0)
					prevParent = work.FlowParents.First ();
			}

			/// <inheritdoc/>
			internal override ProgramPointBase GetWork ()
			{
				while (hQueue._queue.Count == 0 && hQueue._parent != null) {
					hQueue = hQueue._parent;
				}
				var result = hQueue._queue.Dequeue ();
                
				/*if (_openStack.Count > 0)
            {
                //all open points has to be processed before close points
                result = _openStack.Pop();
            }
            else
            {
                //close points are processed at last
                result = _closeStack.Pop();
            }*/

				_containedPoints.Remove (result);

				return result;
			}
		}
	}
}
