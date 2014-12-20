using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.GraphVisualizer;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Logging
{
    public interface IVisualizer
    {
        void BuildGraphVisualisation(IGraphVisualizer visualizer, int snapshotRangeMin = -1, int snapshotRangeMax = -1);

        void InitializeSnapshot(Snapshot snapshot);

        void StartTransaction(Snapshot snapshot);

        void FinishTransaction(Snapshot snapshot, string customNodeId = null);

        void SetParents(Snapshot snapshot, Snapshot[] parents);

        void SetParent(Snapshot snapshot, Snapshot parent);

        void SetLabel(Snapshot snapshot, string label);

        void SetCommitDiffers(Snapshot snapshot, bool differs);

        void SetNumberOfTransactions(Snapshot snapshot, int numberOfTransactions);

        void Clear();

        void Enabled(bool enabled);
    }
}
