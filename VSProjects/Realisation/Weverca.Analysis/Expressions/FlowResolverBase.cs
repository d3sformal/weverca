using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis.Expressions
{
    public abstract class FlowResolverBase
    {
        /// <summary>
        /// Represents method which is used for confirming assumption condition. 
        /// Assumption can be declined - it means that we can prove, that condition CANNOT be ever satisfied.
        /// </summary>  
        /// <param name="outSet">Output set where condition will be assumed</param>
        /// <param name="condition">Assumed condition</param>
        /// <param name="expressionParts">Evaluated values for condition parts</param>
        /// <returns>False if you can prove that condition cannot be ever satisfied, true otherwise.</returns>
        public abstract bool ConfirmAssumption(FlowOutputSet outSet, AssumptionCondition condition, MemoryEntry[] expressionParts);

        /// <summary>
        /// Is called after each invoked call - has to merge data from dispatched calls into callerOutput
        /// </summary>
        /// <param name="callerOutput">Output of caller, which dispatch calls</param>
        /// <param name="dispatchedProgramPointGraphs">Program point graphs obtained during analysis</param>
        public abstract void CallDispatchMerge(FlowOutputSet callerOutput, ProgramPointGraph[] dispatchedProgramPointGraphs);

        /// <summary>
        /// Handler called before programPoint analysis starts 
        /// </summary>
        /// <param name="programPoint">Analyzed program point</param>   
        public virtual void FlowThrough(ProgramPoint programPoint)
        {
            //By default there is nothing to do           
        }
    }
}
