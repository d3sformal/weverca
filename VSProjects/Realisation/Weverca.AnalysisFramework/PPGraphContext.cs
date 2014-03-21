using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.Parsers;
using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Represents a context of a program point graph.
    /// Consists of program points that have this graph as an extension (that include or call this graph).
    /// </summary>
    public class PPGraphContext
    {
        internal readonly List<ProgramPointBase> _callers = new List<ProgramPointBase>();

        /// <summary>
        /// Set of program points from which the owning program point graph is used (called).
        /// </summary>
        public IEnumerable<ProgramPointBase> Callers { get { return _callers; } }
        /// <summary>
        /// The program point graph which context is represented.
        /// </summary>
        public readonly ProgramPointGraph OwningPPGraph;

        internal PPGraphContext(ProgramPointGraph owningPPGraph) { OwningPPGraph = owningPPGraph; }

        /// <inheritdoc />
        public override string ToString()
        {
            var result = new StringBuilder();

            //result.AppendLine(OwningPPGraph.OwningScript.FullName);
            result.Append("->");
            foreach (var caller in _callers)
            {
                // If the program point calls itself, continue (this occurs, e.g., in case sharing graphs in recursive calls)
                if (OwningPPGraph == caller.OwningPPGraph)
                    continue;

                result.Append("(");

                result.Append(caller.OwningPPGraph.OwningScript.FullName + " at " + caller.Partial.Position);
                result.Append(caller.OwningPPGraph.Context);

                result.Append(")");
                result.AppendLine("or");

                //result.Append("( " + caller.ToString() + " )");
            }
            //result.AppendLine();

            return result.ToString();
        }

    }
}
