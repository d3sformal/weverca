#define MEMORY_VISUALIZER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.GraphVisualizer;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Logging
{
    public class MemoryModelVisualizer : IVisualizer
    {
#if MEMORY_VISUALIZER
        private class SnapshotNode
        {
            public readonly List<string> Parents = new List<string>();

            public string Id;
            public string Name;
            public string Label;
            public bool Differs;
            public int NumberOfTransactions = 0;

            public int PreviousStructureId = -1;
            public int PreviousDataId = -1;

            public bool IsStructureReadonly = true;
            public bool IsDataReadonly = true;
        }

        private Dictionary<string, SnapshotNode> nodes = new Dictionary<string, SnapshotNode>();

        // Dictionaries to convert structure/data mapping into snapshot IDs
        private Dictionary<int, string> structureMapping = new Dictionary<int, string>();
        private Dictionary<int, string> dataMapping = new Dictionary<int, string>();

        private SnapshotNode currentNode;
        private Snapshot currentSnapshot = null;
        private bool enabled = true;

#endif

        public void Clear()
        {
#if MEMORY_VISUALIZER
            nodes.Clear();
            structureMapping.Clear();
            dataMapping.Clear();

            currentNode = null;
            currentSnapshot = null;
#endif
        }

        public void Enabled(bool enabled)
        {
#if MEMORY_VISUALIZER
            this.enabled = enabled;
#endif
        }


        public void BuildGraphVisualisation(IGraphVisualizer visualizer)
        {
#if MEMORY_VISUALIZER

            if (!enabled)
            {
                return;
            }

            foreach (var item in nodes)
            {
                SnapshotNode node = item.Value;

                bool skipNode = node.IsStructureReadonly && node.IsDataReadonly && node.Label == "extend";
                if (!skipNode)
                {
                    string structureState = node.IsStructureReadonly ? "Readonly" : "Writeable";
                    string dataState = node.IsDataReadonly ? "Readonly" : "Writeable";
                    string commitState = node.Differs ? "DIFFERENT" : "SAME";

                    string label = string.Format("{0}\n{1}\nStructure: {2} | Data: {3}\nTransactions: {4} {5}",
                        node.Name, node.Label, structureState, dataState, node.NumberOfTransactions, commitState);

                    visualizer.AddNode(node.Id, label);

                    int parentsCount = 0;
                    HashSet<string> processed = new HashSet<string>();
                    LinkedList<string> parentQueue = new LinkedList<string>();
                    foreach (var parent in node.Parents)
                    {
                        parentQueue.AddLast(parent);
                        processed.Add(parent);
                    }
                    while (parentQueue.Count > 0)
                    {
                        string nodeId = parentQueue.First.Value;
                        parentQueue.RemoveFirst();

                        SnapshotNode parentNode;
                        if (nodes.TryGetValue(nodeId, out parentNode))
                        {
                            bool skipParent = parentNode.IsStructureReadonly && parentNode.IsDataReadonly && parentNode.Label == "extend";
                            if (!skipParent)
                            {
                                visualizer.AddEdge(parentNode.Id, node.Id, "");
                                parentsCount++;
                            }
                            else
                            {
                                foreach (string p in parentNode.Parents)
                                {
                                    if (!processed.Contains(p))
                                    {
                                        parentQueue.AddLast(p);
                                        processed.Add(p);
                                    }
                                }
                            }
                        }
                    }

                    if (parentsCount > 1)
                    {
                        if (!node.IsStructureReadonly)
                        {
                            string previousStructure;
                            if (structureMapping.TryGetValue(node.PreviousStructureId, out previousStructure))
                            {
                                visualizer.AddMarkedEdge(previousStructure, node.Id, "structure");
                            }
                        }

                        if (!node.IsDataReadonly)
                        {
                            string previousData;
                            if (dataMapping.TryGetValue(node.PreviousDataId, out previousData))
                            {
                                visualizer.AddMarkedEdge(previousData, node.Id, "data");
                            }
                        }
                    }
                }
            }
#endif
        }

        public void InitializeSnapshot(Snapshot snapshot)
        {
#if MEMORY_VISUALIZER
#endif
        }

        public void StartTransaction(Snapshot snapshot)
        {
#if MEMORY_VISUALIZER
            if (!enabled)
            {
                return;
            }

            if (this.currentSnapshot == null)
            {
                currentNode = new SnapshotNode();
                this.currentSnapshot = snapshot;
            }
            else
            {
                throw new Exception("Previous transaction has not been finished");
            }
#endif
        }

        public void FinishTransaction(Snapshot snapshot)
        {
#if MEMORY_VISUALIZER

            if (!enabled)
            {
                return;
            }

            if (this.currentSnapshot == snapshot)
            {
                currentNode.Id = getNodeId(snapshot);
                currentNode.Name = snapshot.getSnapshotIdentification();

                currentNode.IsStructureReadonly = snapshot.Structure.IsReadonly;
                currentNode.IsDataReadonly = snapshot.CurrentData.IsReadonly;

                var structureTracker = snapshot.Structure.Readonly.ReadonlyChangeTracker;
                if (!structureMapping.ContainsKey(structureTracker.TrackerId))
                {
                    structureMapping.Add(structureTracker.TrackerId, currentNode.Id);
                }
                if (structureTracker.PreviousTracker != null)
                {
                    currentNode.PreviousStructureId = structureTracker.PreviousTracker.TrackerId;
                }

                var dataTracker = snapshot.Data.Readonly.ReadonlyChangeTracker;
                if (!dataMapping.ContainsKey(dataTracker.TrackerId))
                {
                    dataMapping.Add(dataTracker.TrackerId, currentNode.Id);
                }
                if (dataTracker.PreviousTracker != null)
                {
                    currentNode.PreviousDataId = dataTracker.PreviousTracker.TrackerId;
                }

                nodes.Add(currentNode.Id, currentNode);
            }
            else
            {
                throw new Exception("Snapshot transactions does not match");
            }

            this.currentSnapshot = null;
            currentNode = null;
#endif
        }

        public void SetLabel(Snapshot snapshot, string label)
        {
#if MEMORY_VISUALIZER

            if (!enabled)
            {
                return;
            }

            if (this.currentSnapshot == snapshot)
            {
                currentNode.Label = label;
            }
            else
            {
                throw new Exception("Snapshot transactions does not match");
            }
#endif
        }


        public void SetCommitDiffers(Snapshot snapshot, bool differs)
        {
#if MEMORY_VISUALIZER

            if (!enabled)
            {
                return;
            }

            if (this.currentSnapshot == snapshot)
            {
                currentNode.Differs = differs;
            }
            else
            {
                throw new Exception("Snapshot transactions does not match");
            }
#endif
        }


        public void SetNumberOfTransactions(Snapshot snapshot, int numberOfTransactions)
        {
#if MEMORY_VISUALIZER

            if (!enabled)
            {
                return;
            }

            if (this.currentSnapshot == snapshot)
            {
                currentNode.NumberOfTransactions = numberOfTransactions;
            }
            else
            {
                throw new Exception("Snapshot transactions does not match");
            }
#endif
        }

        public void SetParents(Snapshot snapshot, Snapshot[] parents)
        {
#if MEMORY_VISUALIZER

            if (!enabled)
            {
                return;
            }

            if (this.currentSnapshot == snapshot)
            {
                foreach (Snapshot parent in parents)
                {
                    currentNode.Parents.Add(getNodeId(parent));
                }
            }
            else
            {
                throw new Exception("Snapshot transactions does not match");
            }
#endif
        }

        public void SetParent(Snapshot snapshot, Snapshot parent)
        {
#if MEMORY_VISUALIZER

            if (!enabled)
            {
                return;
            }

            if (this.currentSnapshot == snapshot)
            {
                currentNode.Parents.Add(getNodeId(parent));
            }
            else
            {
                throw new Exception("Snapshot transactions does not match");
            }
#endif
        }

        private string getNodeId(Snapshot snapshot)
        {
            return String.Format("ss{0}_s{1}_d{2}", 
                snapshot.SnapshotId, snapshot.Structure.Readonly.StructureId, snapshot.CurrentData.Readonly.DataId);
        }
    }
}
