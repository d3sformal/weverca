using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Represents method which merges inSets into outSet
    /// </summary>
    /// <typeparam name="FlowInfo"></typeparam>
    /// <param name="inSet1">Input set to be merged into output</param>
    /// <param name="inSet2">Input set to be merged into output</param>
    /// <param name="outSet">Result of merging</param>
    delegate void MergeDelegate<FlowInfo>(FlowInputSet<FlowInfo> inSet1, FlowInputSet<FlowInfo> inSet2, FlowOutputSet<FlowInfo> outSet);
    /// <summary>
    /// Represents method which is used for confirming assumption condition. Assumption can be declined - it means that we can prove, that condition CANNOT be ever satisfied.
    /// </summary>
    /// <typeparam name="FlowInfo"></typeparam>
    /// <param name="flow">Controler that determine program flow.</param>
    /// <param name="condition">Assumption condition.</param>
    /// <returns>False if you can prove that condition cannot be ever satisfied, true otherwise.</returns>
    delegate bool ConfirmAssumptionDelegate<FlowInfo>(FlowControler<FlowInfo> flow, AssumptionCondition_deprecated condition);
    /// <summary>
    /// Represents method which creates empty flow info set.
    /// </summary>
    /// <typeparam name="FlowInfo"></typeparam>
    /// <returns>Empty flow info set</returns>
    delegate FlowOutputSet<FlowInfo> EmptySetDelegate<FlowInfo>();

    /// <summary>
    /// Group of services that are provided by analysis object.
    /// </summary>
    /// <typeparam name="FlowInfo"></typeparam>
    class AnalysisServices<FlowInfo>
    {
        internal readonly MergeDelegate<FlowInfo> Merge;
        internal readonly EmptySetDelegate<FlowInfo> CreateEmptySet;
        internal readonly ConfirmAssumptionDelegate<FlowInfo> ConfirmAssumption;

        public AnalysisServices(MergeDelegate<FlowInfo> merge, EmptySetDelegate<FlowInfo> emptySet, ConfirmAssumptionDelegate<FlowInfo> confirmAssumption)
        {
            Merge = merge;
            CreateEmptySet = emptySet;
            ConfirmAssumption = confirmAssumption;
        }
    }
}
