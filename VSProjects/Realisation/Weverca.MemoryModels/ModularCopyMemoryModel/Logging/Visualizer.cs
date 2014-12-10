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
        void BuildGraphVisualisation(IGraphVisualizer visualizer);

        void InitializeSnapshot(Snapshot snapshot);

        void StartTransaction(Snapshot snapshot);

        void FinishTransaction(Snapshot snapshot);

        void SetParents(Snapshot snapshot, Snapshot[] parents);

        void SetParent(Snapshot snapshot, Snapshot parent);

        void SetLabel(Snapshot snapshot, string label);

        void Clear();

        void Enabled(bool enabled);
    }
}
