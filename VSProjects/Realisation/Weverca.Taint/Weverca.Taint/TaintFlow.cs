/*
Copyright (c) 2012-2014 Natalia Tyrpakova and David Hauzar

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.ProgramPoints;
using PHP.Core;

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
        public bool tainted = false;
        public List<TaintFlow> possibleTaintFlows = new List<TaintFlow>();
        public bool nullValue = false;

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            String HTMLpriority = (priority.getHTML()) ? "H" : "L";
            String SQLpriority = (priority.getSQL()) ? "H" : "L";
            String FilePathPriority = (priority.getFilePath()) ? "H" : "L";
            if (taint.getHTML()) result.Append("HTML dirty " + HTMLpriority + ",");
            if (taint.getSQL()) result.Append("SQL dirty " + SQLpriority + ",");
            if (taint.getFilePath()) result.Append("File path dirty " + FilePathPriority + ",");
            if (nullValue) result.Append("Possible null value N,");
            return result.ToString();
        }

		/// <inheritdoc />
		public override int GetHashCode ()
		{         
            if (!tainted) return tainted.GetHashCode();
            int pointHashCode = 0;
            if (point != null) pointHashCode = point.GetHashCode();
            int result = priority.GetHashCode() + taint.GetHashCode() + tainted.GetHashCode() + nullValue.GetHashCode() + getFlowHashCode() + pointHashCode;
            return result;
        }

        /// <summary>
        /// Returns hash codes of set of program points that the current taint is from
        /// </summary>
        /// <returns>hash code of set of program points</returns>
        private int getFlowHashCode()
        {
            List<ProgramPointBase> processed = new List<ProgramPointBase>();
            int result = 0;

            foreach (TaintFlow flow in possibleTaintFlows)
            {
                if (processed.Contains(flow.flow.point)) continue;
                processed.Add(flow.flow.point);
                if (flow.flow.point != null) result += flow.flow.point.GetHashCode();
            }

            return result;
        }
		
		/// <inheritdoc />
		public override bool Equals(Object obj) {
            bool result = GetHashCode() == obj.GetHashCode();
            return result;
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
        }

        /// <summary>
        /// Returns a string containing all possible taint flows
        /// </summary>
        /// <returns>string containing all taint flows</returns>
        public String print()
        {
            return print(false);
        }

        /// <summary>
        /// Returns a string containing all possible null flows
        /// </summary>
        /// <returns>string containing all null flows</returns>
        public String printNullFlows()
        {
            return print(true);
        }

        private String print(bool nullFlow)
        {
            String currentScript = "";
            List<String> resultFlows = new List<String>();

            if (!nullFlow && !tainted) return print(resultFlows);
            if (nullFlow && !nullValue) return print(resultFlows);

            foreach (TaintFlow flow in possibleTaintFlows)
            {
                String refScript = currentScript;
                List<String> flowAsStrings = flow.toString(ref refScript, nullFlow);
                foreach (String flowAsString in flowAsStrings)
                {
                    String script = refScript;
                    resultFlows.Add(flowAsString + currentPointString(ref script));
                }
            }
            return print(resultFlows);
        }

        /// <summary>
        /// Returns a string containing taint flows of the taint specified by a flag
        /// </summary>
        /// <param name="flag">flag specifying the taint</param>
        /// <returns>string containing the taint flows</returns>
        public String print(Analysis.FlagType flag)
        {
            String currentScript = "";
            List<String> resultFlows = new List<String>();

            if (!taint.get(flag)) return print(resultFlows);

            foreach (TaintFlow flow in possibleTaintFlows)
            {
                String refScript = currentScript;
                List<String> flowAsStrings = flow.toString(ref refScript, flag);
                foreach (String flowAsString in flowAsStrings)
                {
                    String script = refScript;
                    resultFlows.Add(flowAsString + currentPointString(ref script));
                }
            }
            return print(resultFlows);
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
        /// Returns the current program point as a string
        /// </summary>
        /// <param name="script">script from previous point</param>
        /// <returns>current program point as a string</returns>
        public String currentPointString(ref String script)
        {
            StringBuilder thisPP = new StringBuilder();
            if (point != null && point.Partial != null)
            {
                if (point.OwningScript != null)
                {
                    String newScript = point.OwningScriptFullName;
                    if (newScript != script)
                    {
                        thisPP.AppendLine();
                        thisPP.AppendLine("File: " + newScript);
                        script = newScript;
                    }
                }
                thisPP.Append("-->");
                thisPP.Append(" at position " + point.Partial.Position);
            }
            return thisPP.ToString();
        }
        
        /// <summary>
        /// Returns current point information along with variable names as a String
        /// </summary>
        /// <param name="script">script from previous point</param>
        /// <param name="vars">variable names</param>
        /// <returns>point information along with variable names as a String</returns>
        public String printPoint(ref String script, VariableName[] vars)
        {
            StringBuilder thisPoint = new StringBuilder();

            thisPoint.AppendLine(currentPointString(ref script));

            thisPoint.Append("From: ");

            int count = vars.Length;
            for (int i = 0; i < count; i++)
            {
                thisPoint.Append(vars[i]);
                if (i < count - 1) thisPoint.Append(", ");
            }
            thisPoint.AppendLine();
            return thisPoint.ToString();
        }
    }






     public class TaintFlow
     {
         public TaintInfo flow;
         public VariableIdentifier var;

         public TaintFlow(TaintInfo info, VariableIdentifier varID)
         {
             flow = info;
             var = varID;
         }

         /// <inheritdoc />
         public override int GetHashCode()
         {
             int result = flow.GetHashCode();
             return result;
         }

         /// <inheritdoc />
         public override bool Equals(Object obj)
         {
             /*if (obj == null) return false;
             if (!(obj is TaintFlow)) return false;
             TaintFlow other = obj as TaintFlow;
             if (var != null && other.var != null && !var.Equals(other.var)) return false;
             if (!(flow.Equals(other.flow))) return false;*/
             return GetHashCode() == obj.GetHashCode();
         }

         /// <summary>
         /// Gets a list of all taint flows or null flows
         /// </summary>
         /// <param name="script">last script</param>
         /// <param name="nullFlow">determines whether to show null flow</param>
         /// <returns>list of taint or null flows</returns>
         public List<string> toString(ref String script, Boolean nullFlow)
         {
             List<string> result = new List<string>();           
             if (!nullFlow && !flow.tainted) return result;
             if (nullFlow && !flow.nullValue) return result;

             String thisScript = script;

             foreach (TaintFlow childFlow in flow.possibleTaintFlows)
             {
                 String refscript = thisScript;
                 List<String> flows = childFlow.toString(ref refscript, nullFlow);
                 String thisPoint = getThisPoint(ref refscript);
                 foreach (String flowString in flows)
                 {
                     result.Add(flowString + thisPoint);
                 }
             }

             if (result.Count == 0)
             {
                 String refscript = thisScript;
                 String thisPoint = getThisPoint(ref refscript);
                 result.Add(thisPoint.ToString());
             }

             script = "";
             if (flow.point != null && flow.point.OwningScript != null) 
                script = flow.point.OwningScriptFullName;
             return result;
         }


         /// <summary>
         /// Gets a list of all taint flows determined by a flag
         /// </summary>
         /// <param name="script">last script</param>
         /// <param name="flag">flag determining the taint</param>
         /// <returnslist of taint flows></returns>
         public List<String> toString(ref String script, Analysis.FlagType flag)
         {
             List<string> result = new List<string>();          
             if (!flow.taint.get(flag)) return result;
             String thisScript = script;

             foreach (TaintFlow childFlow in flow.possibleTaintFlows)
             {
                 String refscript = thisScript;
                 List<String> flows = childFlow.toString(ref refscript, flag);
                 String thisPoint = getThisPoint(ref refscript);
                 foreach (String flowString in flows)
                 {
                     result.Add(flowString + thisPoint);
                 }
             }

             if (result.Count == 0)
             {
                 String refscript = thisScript;
                 String thisPoint = getThisPoint(ref refscript);
                 result.Add(thisPoint);
             }

             script = "";
             if (flow.point != null && flow.point.OwningScript != null) 
                script = flow.point.OwningScriptFullName;
             return result;
         }

         /// <summary>
         /// Returns current point as a string along with variable names
         /// </summary>
         /// <param name="script">script from previous point</param>
         /// <returns>current point as a string along with variable names</returns>
         private String getThisPoint(ref String script)
         {
             VariableName[] varNames;
             if (var != null) varNames = var.PossibleNames;
             else varNames = new VariableName[] { };
             return flow.printPoint(ref script, varNames);    
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

		private int hashCode;

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

        public bool getHTML() { return HTML; }
        public void setHTML(bool b) { HTML = b; }
        public bool getSQL() { return SQL; }
        public void setSQL(bool b) { SQL = b; }
        public bool getFilePath() { return FilePath; }
        public void setFilePath(bool b) { FilePath = b; }

		/// <inheritdoc />
		public override bool Equals (object obj)
		{
			return GetHashCode() == obj.GetHashCode();
		}

		/// <inheritdoc />
		public override int GetHashCode ()
		{
            return HTML.GetHashCode() + FilePath.GetHashCode() + SQL.GetHashCode();
		}

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