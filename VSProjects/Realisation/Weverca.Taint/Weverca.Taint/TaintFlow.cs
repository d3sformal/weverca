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
        //public bool highPriority = false;
        public TaintPriority priority = new TaintPriority(false);
        public Taint taint = new Taint(false);
        //public bool tainted = false;
        public List<TaintFlow> possibleTaintFlows = new List<TaintFlow>();

       
        /// <inheritdoc />
        public override string ToString()
        {
            var result = new StringBuilder();

            foreach (TaintFlow possibleFlow in possibleTaintFlows)
            {
                result.Append("Possible flow: ");
                result.Append(possibleFlow.ToString());
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
            priority.clean(flags);
            taint.clean(flags);
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
                priority.setAll(false);
                taint.setAll(false);
            }
        }
    }

    /// <summary>
    /// Base class for HTML, SQL and FilePath indicators. Taint and TaintPriority classes are inherited from this.
    /// </summary>
    public class Indicator
    {
        protected bool HTML = false;
        protected bool SQL = false;
        protected bool FilePath = false;

        /// <summary>
        /// This constructor initializes all field to the one specific value.
        /// </summary>
        /// <param name="initial">boolean initializer</param>
        public Indicator(bool initial)
        {
            setAll(initial); 
        }

        /// <summary>
        /// This constructor initializes each field to its value
        /// </summary>
        /// <param name="HTML">HTML indicator value</param>
        /// <param name="SQL">SQL indicator value</param>
        /// <param name="FilePath">FilePath indicator value</param>
        public Indicator(bool HTML, bool SQL, bool FilePath)
        {
            this.HTML = HTML;
            this.SQL = SQL;
            this.FilePath = FilePath;
        }

        public bool getHTMLtaint() { return HTML; }
        public void setHTMLtaint(bool b) { HTML = b; }
        public bool getSQLtaint() { return SQL; }
        public void setSQLtaint(bool b) { SQL = b; }
        public bool getFilePathtaint() { return FilePath; }
        public void setFilePathtaint(bool b) { FilePath = b; }

        /// <summary>
        /// Returns true if all the indicators are true
        /// </summary>
        /// <returns>true if all the indicators are true</returns>
        public bool allTrue()
        {
            return (HTML && SQL && FilePath);
        }

        /// <summary>
        /// Returns true if all the indicators are false
        /// </summary>
        /// <returns>true if all the indicators are false</returns>
        public bool allFalse()
        {
            return (!HTML && !SQL && !FilePath);
        }

        /// <summary>
        /// Sets all the indicator to the specific value
        /// </summary>
        /// <param name="b">value to set</param>
        public void setAll(bool b)
        {
            HTML = b;
            SQL = b;
            FilePath = b;
        }

        /// <summary>
        /// Copies all fields with givec value to this instance
        /// </summary>
        /// <param name="b">value to copy</param>
        /// <param name="other">indicator to copy from</param>
        public void copyTaint(bool b, Indicator other)
        {
            if (other.HTML == b) this.HTML = other.HTML;
            if (other.SQL == b) this.SQL = other.SQL;
            if (other.FilePath == b) this.FilePath = other.FilePath;
        }

        /// <summary>
        /// Sets the fields determined by list of flag types to false
        /// </summary>
        /// <param name="flags">list of flag types to be set false</param>
        public void clean(List<Analysis.FlagType> flags)
        {
            if (flags.Contains(Analysis.FlagType.HTMLDirty)) HTML = false;
            if (flags.Contains(Analysis.FlagType.SQLDirty)) SQL = false;
            if (flags.Contains(Analysis.FlagType.FilePathDirty)) FilePath = false;
        }

        /// <summary>
        /// Gets the corresponding value to a flag type
        /// </summary>
        /// <param name="flag"></param>
        /// <returns>true if flag type value is true, false otherwise</returns>
        public bool get(Analysis.FlagType flag)
        {
            if (flag == Analysis.FlagType.HTMLDirty) return HTML;
            if (flag == Analysis.FlagType.SQLDirty) return SQL;
            return FilePath;
        }

    }

    /// <summary>
    /// Indicator determining the taint priority
    /// </summary>
    public class TaintPriority : Indicator
    {
        public TaintPriority(bool initial) : base(initial) { }

        public TaintPriority(bool HTML, bool SQL, bool FilePath) : base(HTML, SQL, FilePath) { }

        public bool equalTo(TaintPriority other)
        {
            return (other.HTML == HTML && other.SQL == SQL && other.FilePath == FilePath);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ("HTML priority " + HTML + ", SQL priority " + SQL + ", file path priority " + FilePath);
        }
    }

    public class Taint : Indicator
    {
        public Taint(bool initial) : base(initial) { }

        public Taint(bool HTML, bool SQL, bool FilePath) : base(HTML, SQL, FilePath) { }

        public bool equalTo(Taint other)
        {
            return (other.HTML == HTML && other.SQL == SQL && other.FilePath == FilePath);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ("HTML taint " + HTML + ", SQL taint " + SQL + ", file path taint " + FilePath);
        }
    }

}
