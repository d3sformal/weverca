using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

using Weverca.Analysis.Memory;
using Weverca.Analysis.Expressions;

namespace Weverca.Analysis
{
    /// <summary>
    /// Represents method which merges inSets into outSet
    /// </summary>
    /// <typeparam name="FlowInfo"></typeparam>
    /// <param name="inSet1">Input set to be merged into output</param>
    /// <param name="inSet2">Input set to be merged into output</param>
    /// <param name="outSet">Result of merging</param>
    delegate void MergeDelegate(FlowInputSet inSet1, FlowInputSet inSet2, FlowOutputSet outSet);
    /// <summary>
    /// Represents method which is used for confirming assumption condition. Assumption can be declined - it means that we can prove, that condition CANNOT be ever satisfied.
    /// </summary>        
    /// <param name="condition">Assumption condition.</param>
    /// <returns>False if you can prove that condition cannot be ever satisfied, true otherwise.</returns>
    delegate bool ConfirmAssumptionDelegate(AssumptionCondition condition,MemoryEntry[] expressionParts);
    /// <summary>
    /// Represents method which creates empty flow info set.
    /// </summary>
    /// <typeparam name="FlowInfo"></typeparam>
    /// <returns>Empty flow info set</returns>
    delegate FlowOutputSet EmptySetDelegate();

    delegate PartialWalker WalkerCreatorDelegate();

    /// <summary>
    /// Group of services that are provided by analysis object.
    /// </summary>
    /// <typeparam name="FlowInfo"></typeparam>
    class AnalysisServices
    {
        internal readonly MergeDelegate Merge;
        internal readonly EmptySetDelegate CreateEmptySet;
        internal readonly ConfirmAssumptionDelegate ConfirmAssumption;
        internal readonly WalkerCreatorDelegate CreateWalker;

        public AnalysisServices(MergeDelegate merge, EmptySetDelegate emptySet, ConfirmAssumptionDelegate confirmAssumption, WalkerCreatorDelegate createWalker)
        {
            Merge = merge;
            CreateEmptySet = emptySet;
            ConfirmAssumption = confirmAssumption;
            CreateWalker = createWalker;
        }     
    }
}
