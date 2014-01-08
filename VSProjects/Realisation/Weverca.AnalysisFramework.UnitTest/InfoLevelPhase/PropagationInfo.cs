using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.AnalysisFramework.UnitTest.InfoLevelPhase
{
    class PropagationInfo
    {
        internal readonly IEnumerable<string> Targets;

        internal PropagationInfo(params string[] propagationTargets)
        {
            Targets = propagationTargets.ToArray();
        }

        internal PropagationInfo PropagateTo(PropagationInfo childPropagation)
        {
            var merged = new HashSet<string>(Targets);
            merged.UnionWith(childPropagation.Targets);
            return new PropagationInfo(merged.ToArray());
        }
    }
}
