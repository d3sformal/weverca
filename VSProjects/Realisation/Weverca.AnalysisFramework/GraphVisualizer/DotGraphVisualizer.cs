using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.AnalysisFramework.GraphVisualizer
{
    /// <summary>
    /// Graph visualiser to print graph image using DOT tool.
    /// </summary>
    public class DotGraphVisualizer : IGraphVisualizer
    {
        public static readonly string GRAPH_FILE_EXTENSION = ".cfg";
        public static readonly string IMAGE_FILE_EXTENSION = ".png";

        StringBuilder nodesBuilder = new StringBuilder();
        StringBuilder edgesBuilder = new StringBuilder();
        
        private readonly string dotFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotGraphVisualizer"/> class.
        /// </summary>
        /// <param name="dotFilePath">Path of the dot tool within the system.</param>
        public DotGraphVisualizer(string dotFilePath)
        {
            this.dotFilePath = dotFilePath;
        }

        /// <inheritdoc />
        public void AddNode(string nodeId, string nodeLabel)
        {
            nodesBuilder.AppendLine(string.Format("{0}[label=\"{1}\"]",
                nodeId, nodeLabel));
        }

        /// <inheritdoc />
        public void AddEdge(string sourceNode, string targetNode, string label)
        {
            edgesBuilder.AppendLine(string.Format(
                "{0} -> {1}[headport=n, tailport=s,label=\"{2}\"]",
                sourceNode, targetNode, label));
        }

        /// <inheritdoc />
        public void AddMarkedEdge(string sourceNode, string targetNode, string label)
        {
            edgesBuilder.AppendLine(string.Format(
                "{0} -> {1}[headport=n, tailport=s,label=\"{2}\",style=dashed,arrowType=empty]",
                sourceNode, targetNode, label));
        }

        /// <summary>
        /// Creates and stores graph into the given file.
        /// </summary>
        /// <param name="visualizationFileName">Name of the visualization file.</param>
        public void CreateVisualization(string visualizationFileName)
        {
            string visualizationText = buildVisualizationRepresentation();

            //Saves CFG representation into the file
            string cfgFileName = visualizationFileName + GRAPH_FILE_EXTENSION;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(cfgFileName))
            {
                file.WriteLine(visualizationText);
            }

            //Runs the graphviz component
            System.Diagnostics.Process imageMaker = new System.Diagnostics.Process();
            imageMaker.StartInfo.FileName = dotFilePath;
            imageMaker.StartInfo.Arguments = "-Tpng " + cfgFileName;
            imageMaker.StartInfo.UseShellExecute = false;
            imageMaker.StartInfo.RedirectStandardOutput = true;
            imageMaker.StartInfo.RedirectStandardError = true;
            imageMaker.Start();

            //And writes the generated image into file
            string imageFileName = visualizationFileName + IMAGE_FILE_EXTENSION;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(imageFileName))
            {
                imageMaker.StandardOutput.BaseStream.CopyTo(file.BaseStream);
            }
        }

        private string buildVisualizationRepresentation()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("digraph g {node [shape=box] graph[rankdir=\"TB\", concentrate=true];");
            builder.AppendLine(nodesBuilder.ToString());
            builder.AppendLine(edgesBuilder.ToString());
            builder.AppendLine("}");

            return builder.ToString();
        }
    }
}
