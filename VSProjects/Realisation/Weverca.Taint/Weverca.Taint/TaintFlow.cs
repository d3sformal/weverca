using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.Taint
{
    /// <summary>
    /// Stores information about a taint flow.
    /// </summary>
    public class TaintFlow
    {
        public List<ProgramPointBase> flow;
        public bool nullValue = false;
        public List<Analysis.FlagType> flags;

        /// <summary>
        /// Creates a new, dirty flow
        /// </summary>
        public TaintFlow()
        {
            flow = new List<ProgramPointBase>();
            flags = new List<Analysis.FlagType>() { Analysis.FlagType.SQLDirty, Analysis.FlagType.HTMLDirty, Analysis.FlagType.FilePathDirty };
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="oldFlow">TaintFlow to copy</param>
        public TaintFlow(TaintFlow oldFlow)
        {
            flow = new List<ProgramPointBase>(oldFlow.flow);
            nullValue = oldFlow.nullValue;
            flags = new List<Analysis.FlagType>(oldFlow.flags);
        }

        /// <summary>
        /// Adds a new program point to this taint flow
        /// </summary>
        /// <param name="programPoint">ProgramPointBase to add</param>
        public void addPointToTaintFlow(ProgramPointBase programPoint)
        {
            flow.Add(programPoint);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var result = new StringBuilder();

            foreach (ProgramPointBase pPoint in flow)
            {
                if (pPoint.Partial == null) continue;
                String script = "";
                if (pPoint.OwningPPGraph.OwningScript != null) script = pPoint.OwningPPGraph.OwningScript.FullName;
                result.Append("->");
                result.Append("script: " + script + " at position " + pPoint.Partial.Position);
            }

            return result.ToString();
        }

    }
    
    /// <summary>
    /// Class containing the taint information
    /// </summary>
    public class TaintInfo
    {
        public bool highPriority = false;
        public bool tainted = false;
        public List<TaintFlow> possibleTaintFlows = new List<TaintFlow>();

       
        /// <inheritdoc />
        public override string ToString()
        {
            var result = new StringBuilder();

            foreach (TaintFlow possibleFlow in possibleTaintFlows)
            {
                result.Append("Possible flow: ");
                result.AppendLine(possibleFlow.ToString());
            }

            return result.ToString();
        }

        /// <summary>
        /// Converts to String only those flows that are dirty according to the flag
        /// </summary>
        /// <param name="flag">The taint flag determining which flows to show</param>
        /// <returns></returns>
        public string ToString(Analysis.FlagType flag)
        {
            var result = new StringBuilder();

            foreach (TaintFlow possibleFlow in possibleTaintFlows)
            {
                if (possibleFlow.flags.Contains(flag))
                {
                    result.Append("Possible flow: ");
                    result.AppendLine(possibleFlow.ToString());
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Sanitizes the taint flows according to the provided flags. If all the taint flags are removed, 
        /// the flow is removed too. If there is no flow left, the variable is not tainted anymore
        /// </summary>
        /// <param name="flags">flag types to remove from the flows</param>
        public void setSanitized(List<Analysis.FlagType> flags)
        {
            List<TaintFlow> toRemove = new List<TaintFlow>();
            foreach (TaintFlow flow in possibleTaintFlows)
            {
                if (flow.nullValue) continue;
                if (flags.Contains(Analysis.FlagType.HTMLDirty)) flow.flags.Remove(Analysis.FlagType.HTMLDirty);
                if (flags.Contains(Analysis.FlagType.SQLDirty)) flow.flags.Remove(Analysis.FlagType.SQLDirty);
                if (flags.Contains(Analysis.FlagType.FilePathDirty)) flow.flags.Remove(Analysis.FlagType.FilePathDirty);
                if (flow.flags.Count == 0) toRemove.Add(flow);
            }
            foreach (TaintFlow flow in toRemove)
            {
                possibleTaintFlows.Remove(flow);
            }
            if (possibleTaintFlows.Count == 0)
            {
                highPriority = false;
                tainted = false;
            }

        }

    }
}
