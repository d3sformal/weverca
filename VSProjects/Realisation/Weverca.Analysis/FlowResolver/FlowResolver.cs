using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.FlowResolver
{
    /// <summary>
    /// This class is used for evaluating conditions and assumptions.
    /// According to the result of the assumption the environment inside of the code block is set up.
    /// </summary>
    class FlowResolver : FlowResolverBase
    {
        #region FlowResolverBase overrides

        /// <summary>
        /// Represents method which is used for confirming assumption condition. Assumption can be declined - it means that we can prove, that condition CANNOT be ever satisfied.
        /// </summary>
        /// <param name="outSet">Output set where condition will be assumed</param>
        /// <param name="condition">Assumed condition</param>
        /// <param name="log"></param>
        /// <returns>
        ///   <c>false</c> if condition cannot be ever satisfied, true otherwise.
        /// </returns>
        public override bool ConfirmAssumption(FlowOutputSet outSet, AssumptionCondition condition, EvaluationLog log)
        {
            //TODO: How to resolve not-bool conditions, like if (1) etc.?
            //TODO: if(False) there is empty avaluated parts --> is evaluated like "can be true".

            ConditionParts conditionParts = new ConditionParts(condition.Form, outSet, log, condition.Parts);
            return conditionParts.MakeAssumption();
        }

        /// <summary>
        /// Is called after each invoked call - has to merge data from dispatched calls into callerOutput
        /// </summary>
        /// <param name="callerOutput">Output of caller, which dispatch calls</param>
        /// <param name="dispatchedProgramPointGraphs">Program point graphs obtained during analysis</param>
        /// <param name="dispatchType">Type of merged call</param>
        public override void CallDispatchMerge(FlowOutputSet callerOutput,IEnumerable<ProgramPointGraph> dispatchedProgramPointGraphs, ExtensionType dispatchType)
        {
            var ends = dispatchedProgramPointGraphs.Select(c => c.End.OutSet as ISnapshotReadonly).ToArray();
            callerOutput.MergeWithCallLevel(ends);
        }

        /// <summary>
        /// Is called after each include/require/include_once/require_once expression (can be resolved according to flow.CurrentPartial)
        /// </summary>
        /// <param name="flow">Flow controller where include extensions can be stored</param>
        /// <param name="includeFile">File argument of include statement</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void Include(FlowController flow, MemoryEntry includeFile)
        {
            throw new NotImplementedException();
        }

        public override void TryScopeStart(FlowOutputSet outSet, IEnumerable<Tuple<PHP.Core.GenericQualifiedName, ProgramPointBase>> catchBlockStarts)
        {
            throw new NotImplementedException();
        }

        public override void TryScopeEnd(FlowOutputSet outSet, IEnumerable<Tuple<PHP.Core.GenericQualifiedName, ProgramPointBase>> catchBlockStarts)
        {
            throw new NotImplementedException();
        }
        #endregion

        public override IEnumerable<ProgramPointBase> Throw(FlowOutputSet outSet, PHP.Core.AST.ThrowStmt throwStmt, MemoryEntry throwedValue)
        {
            throw new NotImplementedException();
        }
    }
}
