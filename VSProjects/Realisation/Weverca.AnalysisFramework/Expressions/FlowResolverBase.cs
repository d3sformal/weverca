using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.ProgramPoints;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework.Expressions
{
    /// <summary>
    /// Base class for resolvers controlling program flow computation
    /// </summary>
    public abstract class FlowResolverBase
    {
        /// <summary>
        /// Represents method which is used for confirming assumption condition. 
        /// Assumption can be declined - it means that we can prove, that condition CANNOT be ever satisfied.
        /// </summary>  
        /// <param name="outSet">Output set where condition will be assumed</param>
        /// <param name="condition">Assumed condition</param>
        /// <param name="log">Provide access to values computed during analysis</param>
        /// <returns>False if you can prove that condition cannot be ever satisfied, true otherwise.</returns>
        public abstract bool ConfirmAssumption(FlowOutputSet outSet, AssumptionCondition condition, EvaluationLog log);

        /// <summary>
        /// Is called after each invoked call - has to merge data from dispatched calls into callerOutput
        /// </summary>
        /// <param name="callerOutput">Output of caller, which dispatch calls</param>
        /// <param name="dispatchedExtensions">Program points connecting dispatched extensions</param>
        public abstract void CallDispatchMerge(FlowOutputSet callerOutput, IEnumerable<ExtensionPoint> dispatchedExtensions);

        /// <summary>
        /// Reports about try block scope start
        /// </summary>
        /// <param name="catchBlockStarts">Catch blocks associated with starting try block</param>
        /// <param name="outSet">Flow output set</param>
        public abstract void TryScopeStart(FlowOutputSet outSet, IEnumerable<CatchBlockDescription> catchBlockStarts);

        /// <summary>
        /// Reports about try block scope end
        /// </summary>
        /// <param name="catchBlockStarts">Catch blocks associated with ending try block</param> 
        /// <param name="outSet">Flow output set</param>
        public abstract void TryScopeEnd(FlowOutputSet outSet, IEnumerable<CatchBlockDescription> catchBlockStarts);

        /// <summary>
        /// Process throw statement according to current flow
        /// </summary>        
        /// <param name="throwStmt">Processed throw statement</param>
        /// <param name="throwedValue">Value that was supplied into throw statement</param>
        /// <param name="flow">Flow controller which provides API usefull for throw resolvings</param>
        /// <param name="outSet">Flow output set</param>
        /// <returns>All possible ThrowInfo branches</returns>
        public abstract IEnumerable<ThrowInfo> Throw(FlowController flow, FlowOutputSet outSet, ThrowStmt throwStmt, MemoryEntry throwedValue);

        /// <summary>
        /// Process catch statement according to given catch block
        /// </summary>
        /// <param name="catchPoint">Point describing state of current catch block</param>
        /// <param name="outSet">Flow output set</param>
        public abstract void Catch(CatchPoint catchPoint, FlowOutputSet outSet);

        /// <summary>
        /// Is called after each include/require/include_once/require_once expression (can be resolved according to flow.CurrentPartial)
        /// </summary>
        /// <param name="flow">Flow controller where include extensions can be stored</param>
        /// <param name="includeFile">File argument of include statement</param>        
        public abstract void Include(FlowController flow, MemoryEntry includeFile);

        /// <summary>
        /// Is called for resolving eval expression. Should be resolved in similar way as include
        /// </summary>
        /// <param name="flow">Flow controller where eval extensions can be stored</param>
        /// <param name="code">Evaluated code</param>
        public abstract void Eval(FlowController flow, MemoryEntry code);

        /// <summary>
        /// Handler called before programPoint analysis starts 
        /// </summary>
        /// <param name="programPoint">Analyzed program point</param>   
        public virtual void FlowThrough(ProgramPointBase programPoint)
        {
            //By default there is nothing to do           
        }


    }
}
