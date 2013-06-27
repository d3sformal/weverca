using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis.Expressions
{
    public abstract class FlowResolver
    {

        public FlowControler Flow { get; private set; }
  

        /// <summary>
        /// Represents method which is used for confirming assumption condition. Assumption can be declined - it means that we can prove, that condition CANNOT be ever satisfied.
        /// </summary>  
        /// <returns>False if you can prove that condition cannot be ever satisfied, true otherwise.</returns>
        public abstract bool ConfirmAssumption(AssumptionCondition condition, MemoryEntry[] expressionParts);

        public abstract void CallDispatchMerge(FlowOutputSet callerOutput, ProgramPointGraph[] dispatchedProgramPointGraphs);

        public virtual void FlowThrough(ProgramPoint programPoint)
        {
            //By default there is nothing to do
        }

        internal void SetContext(FlowControler flow)
        {
            Flow = flow;
        }
    }
}
