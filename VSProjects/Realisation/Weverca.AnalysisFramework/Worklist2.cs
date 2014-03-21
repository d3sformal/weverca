#define IMPORVEDFIXPOINT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.AnalysisFramework
{
    class WorkQueue
    {
        public readonly Queue<ProgramPointBase> _queue = new Queue<ProgramPointBase>();
        public readonly WorkQueue _parent;

        public WorkQueue(WorkQueue parent) { _parent = parent; }
    }

    /// <summary>
    /// List of work items that has to be processed within analysis. Optimize processing
    /// order because of effectivity of analysis.
    /// 
    /// Work points are classified as open/close/normal according to number of inputs and outputs.
    /// Every point with more than one input is close point - is needed to have processed all possible open points before.
    /// Open points has parents with more than one output - at a time only single opened branch is processed within queue.
    /// </summary>
    class Worklist2
    {
        /// <summary>
        /// Set of all points that are cotained within work list
        /// </summary>
        private readonly HashSet<ProgramPointBase> _containedPoints = new HashSet<ProgramPointBase>();

        /// <summary>
        /// Points which parents has more than one output
        /// </summary>
        private readonly Stack<ProgramPointBase> _openStack = new Stack<ProgramPointBase>();

        /// <summary>
        /// Points which has more than one input
        /// </summary>
        private readonly Stack<ProgramPointBase> _closeStack = new Stack<ProgramPointBase>();

        /// <summary>
        /// Work queu of points that can be currently processed
        /// </summary>
        private Queue<ProgramPointBase> _workQueue = new Queue<ProgramPointBase>();

        private WorkQueue hQueue = new WorkQueue(null);

        private readonly Stack<Queue<ProgramPointBase>> _hStack = new Stack<Queue<ProgramPointBase>>();
        private ProgramPointBase prevParent = null;

        /// <summary>
        /// Determine that any work
        /// </summary>
        public bool HasWork { get { return _containedPoints.Count > 0; } }

        /// <summary>
        /// Add work into list
        /// </summary>
        /// <param name="work">Added work</param>
        public void AddWork(ProgramPointBase work)
        {
            if (!_containedPoints.Add(work))
                //don't need to add same work twice
                return;
            
            // open point
            if (work.FlowParents.Count() > 0 && work.FlowParents.First().FlowChildrenCount > 1 && work.FlowParents.First() != prevParent)
            {
                hQueue = new WorkQueue(hQueue);
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
            if (work.FlowParentsCount > 1)
            {
                hQueue._parent._queue.Enqueue(work);
            }
            else 
            { 
                //normal point
                hQueue._queue.Enqueue(work);
                //_workQueue.Enqueue(work);
            }

            if (work.FlowParents.Count() > 0)
                prevParent = work.FlowParents.First();
        }

        /// <summary>
        /// Get work from list
        /// </summary>
        /// <returns>Work that should be processed</returns>
        internal ProgramPointBase GetWork()
        {
            while (hQueue._queue.Count == 0 && hQueue._parent != null) 
            {
                hQueue = hQueue._parent;
            }
            var result = hQueue._queue.Dequeue();
                
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

            _containedPoints.Remove(result);

            return result;
        }

    }
}
