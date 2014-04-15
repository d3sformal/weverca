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
    /// Class containing the taint information - program point extending the flow, taint,
    /// taint priority, previous flow as a list of TaintInfo and null value indicator 
    /// </summary>
    public class TaintInfo
    {
        public ProgramPointBase point;
        public TaintPriority priority = new TaintPriority(false);
        public Taint taint = new Taint(false);
        public List<TaintInfo> possibleTaintFlows = new List<TaintInfo>();
        public bool nullValue = false;

        /// <summary>
        /// Sanitizes the taint flows according to the provided flags. If all the taint flags are removed, 
        /// the flow is removed too. If there is no flow left, the variable is not tainted anymore
        /// </summary>
        /// <param name="flags">flag types to remove from the flows</param>
        public void setSanitized(List<Analysis.FlagType> flags)
        {
            priority.clean(flags);
            taint.clean(flags);
        }

        /// <summary>
        /// Returns a string containing all possible taint flows
        /// </summary>
        /// <returns>string containing all taint flows</returns>
        public String print()
        {
            String currentScript = "";
            /*if (point.OwningPPGraph.OwningScript != null) currentScript = point.OwningPPGraph.OwningScript.FullName;
            String scriptToPrint = currentScript;*/
            List<String> flows = this.toString(ref currentScript);
            return print(flows);
        }

        /// <summary>
        /// Returns a string containing taint flows of the taint specified by a flag
        /// </summary>
        /// <param name="flag">flag specifying the taint</param>
        /// <returns>string containing the taint flows</returns>
        public String print(Analysis.FlagType flag)
        {
            String currentScript = "";
            /*if (point.OwningPPGraph.OwningScript != null) currentScript = point.OwningPPGraph.OwningScript.FullName;
            String scriptToPrint = currentScript; */
            List<String> flows = this.toString(ref currentScript, flag);
            return print(flows);
        }

        /// <summary>
        /// Nicely converts the given flows to a string
        /// </summary>
        /// <param name="flows">flows to convert to a string</param>
        /// <returns>string containing the flows</returns>
        private String print(List<String> flows)
        {
            StringBuilder result = new StringBuilder();

            foreach (String flow in flows)
            {
                result.AppendLine("Possible flow: ");
                result.AppendLine(flow);
                result.AppendLine("End flow");
            }

            return result.ToString();
        }

        /// <summary>
        /// Gets a list of all taint flows
        /// </summary>
        /// <param name="script">last script</param>
        /// <returns>list of taint flows</returns>
        private List<string> toString(ref String script)
        {
            List<string> result = new List<string>();
            String thisPP = null;
            String thisScript = script;

            foreach (TaintInfo flow in possibleTaintFlows)
            {
                String refscript = thisScript;  
                List<String> flows = flow.toString(ref refscript);
                thisPP = currentPointString(ref refscript);
                foreach (String flowString in flows)
                {
                    result.Add(flowString + thisPP);
                }
            }

            if (result.Count == 0)
            {
                String refscript = thisScript;
                thisPP = currentPointString(ref refscript);
                result.Add(thisPP);
            }
            script = "";
            if (point != null && point.OwningPPGraph.OwningScript != null) script = point.OwningPPGraph.OwningScript.FullName;
            return result;
        }

        /// <summary>
        /// Gets a list of all taint flows determined by a flag
        /// </summary>
        /// <param name="script">last script</param>
        /// <param name="flag">flag determining the taint</param>
        /// <returnslist of taint flows></returns>
        private List<String> toString(ref String script, Analysis.FlagType flag)
        {
            List<string> result = new List<string>();
            String thisPP = null;
            if (!taint.get(flag)) return result;
            String thisScript = script;

            foreach (TaintInfo flow in possibleTaintFlows)
            {
                String refscript = thisScript;
                List<String> flows = flow.toString(ref refscript, flag);
                thisPP = currentPointString(ref refscript);
                foreach (String flowString in flows)
                {
                    result.Add(flowString + thisPP);
                }
            }

            if (result.Count == 0)
            {
                String refscript = thisScript;
                thisPP = currentPointString(ref refscript);
                result.Add(thisPP);
            }

            script = "";
            if (point != null && point.OwningPPGraph.OwningScript != null) script = point.OwningPPGraph.OwningScript.FullName;  
            return result;
        }

        /// <summary>
        /// Returns the current program point as a string
        /// </summary>
        /// <returns>current program point as a string</returns>
        private String currentPointString(ref String script)
        {
            StringBuilder thisPP = new StringBuilder();
            if (point != null && point.Partial != null)
            {
                String newScript = "emptyscript";
                if (point.OwningPPGraph.OwningScript != null) newScript = point.OwningPPGraph.OwningScript.FullName;
                if (newScript != script)
                {
                    thisPP.AppendLine();
                    thisPP.AppendLine("File: " + newScript);
                    script = newScript;
                }
                thisPP.Append("-->");
                thisPP.Append(" at position " + point.Partial.Position);
            }
            return thisPP.ToString();
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
