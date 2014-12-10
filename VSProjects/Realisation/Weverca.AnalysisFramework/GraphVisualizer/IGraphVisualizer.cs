using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.AnalysisFramework.GraphVisualizer
{
    /// <summary>
    /// Instances of this interface are used to create visualisation of the graph data.
    /// 
    /// Used to print images of program point grap, snapshot structure and more.
    /// </summary>
    public interface IGraphVisualizer
    {
        /// <summary>
        /// Adds the node into visualisation.
        /// </summary>
        /// <param name="nodeId">The node identifier.</param>
        /// <param name="nodeLabel">The node label.</param>
        void AddNode(string nodeId, string nodeLabel);

        /// <summary>
        /// Adds directed the edge between two nodes.
        /// </summary>
        /// <param name="sourceNode">The source node.</param>
        /// <param name="targetNode">The target node.</param>
        /// <param name="label">The label.</param>
        void AddEdge(string sourceNode, string targetNode, string label);

        /// <summary>
        /// Adds directed the edge between two nodes. Edge is printed using different style.
        /// </summary>
        /// <param name="sourceNode">The source node.</param>
        /// <param name="targetNode">The target node.</param>
        /// <param name="label">The label.</param>
        void AddMarkedEdge(string sourceNode, string targetNode, string label);
    }
}
