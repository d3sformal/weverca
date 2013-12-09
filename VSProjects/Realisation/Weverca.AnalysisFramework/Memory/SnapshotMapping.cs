using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Class used for mapping between snapshots.
    /// Is used for mapping used in NextPhaseAnalysis
    /// </summary>
    public class SnapshotMapping
    {
        public readonly ProgramPointGraph MappedGraph;

        internal SnapshotMapping(ProgramPointGraph mappedGraph)
        {
            MappedGraph = mappedGraph;
        }
    }
}
