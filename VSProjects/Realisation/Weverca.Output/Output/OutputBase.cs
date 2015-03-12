/*
Copyright (c) 2012-2014 Miroslav Vodolan

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

using Weverca.AnalysisFramework;
using Weverca.Analysis;

namespace Weverca.Output.Output
{
    /// <summary>
    /// Abstract class represeting output writer
    /// </summary>
    public abstract class OutputBase
    {
        /// <summary>
        /// Prefixes for comment lines
        /// </summary>
        static List<string> _commentPrefixes = new List<string>() { "===", "---" };

        /// <summary>
        /// Prefixes for hint texts
        /// </summary>
        static List<string> _hintPrefixes = new List<string>() { "#" };

        /// <summary>
        /// Current level of indentation
        /// </summary>
        private int _indentationLevel = 0;

        #region Template methods for implementors

        /// <summary>
        /// Headline for section
        /// </summary>
        /// <param name="text">Text of headline</param>
        protected abstract void head(string text);

        /// <summary>
        /// Seconfd level headline
        /// </summary>
        /// <param name="text">Text of headline</param>
        protected abstract void head2(string text);

        /// <summary>
        /// Info used in section
        /// </summary>
        /// <param name="text">Info text</param>
        protected abstract void info(string text);

        /// <summary>
        /// Hint used as additional description to some info
        /// </summary>
        /// <param name="text">Hint text</param>
        protected abstract void hint(string text);

        /// <summary>
        /// Comment used in section
        /// </summary>
        /// <param name="text">Text of comment</param>
        protected abstract void comment(string text);

        /// <summary>
        /// Delimiter of line elements
        /// </summary>
        /// <param name="text">Text of delimiter</param>
        protected abstract void delimiter(string text);

        /// <summary>
        /// Variable name (used because of highliting)
        /// </summary>
        /// <param name="name">Name of variable</param>
        protected abstract void variable(string name);

        /// <summary>
        /// Error
        /// </summary>
        /// <param name="error">Error text</param>
        protected abstract void error(string error);

        /// <summary>
        /// Force new line into output
        /// </summary>
        protected abstract void line();

        /// <summary>
        /// Set level for indentation for new lines
        /// </summary>
        /// <param name="level">Zero based indentation level</param>
        protected abstract void setIndentation(int level);

        #endregion

        #region Output API

        /// <summary>
        /// Increase indentation level for next output
        /// </summary>
        public void Indent()
        {
            ++_indentationLevel;
            setIndentation(_indentationLevel);
        }

        /// <summary>
        /// Decreates indentation level for next output
        /// </summary>
        public void Dedent()
        {
            if (_indentationLevel == 0)
            {
                throw new NotSupportedException("Cannot dedent to negative");
            }

            --_indentationLevel;
            setIndentation(_indentationLevel);
        }

        /// <summary>
        /// Print representative info for given program point.
        /// </summary>
        /// <param name="pointCaption">Caption which specifies program point</param>
        /// <param name="point">Program point which info is printed</param>
        public void ProgramPointInfo(string pointCaption, ProgramPointBase point)
        {
            Headline("PROGRAM POINT: " + pointCaption);

            comment("Call context:");
            line();
            comment(point.OwningPPGraph.Context.ToString());

            Indent();
            line();

            if (point.OutSet == null)
            {
                line();
                Headline("Point not reached");
                line();
                line();
            }
            else
            {
                foreach (var infoLine in lineSplit(point.OutSet.Representation))
                {
                    if (isCommentLine(infoLine))
                    {
                        comment(infoLine);
                    }
                    else if (!isEmpty(infoLine))
                    {
                        variableInfoLine(infoLine);
                    }

                    line();
                }
            }
            Dedent();
        }

        /// <summary>
        /// Print given text as comment line
        /// </summary>
        /// <param name="text">Comment text</param>
        public void CommentLine(string text)
        {
            comment(text);
            line();
        }

        /// <summary>
        /// Print empty line
        /// </summary>
        public void EmptyLine()
        {
            line();
        }

        /// <summary>
        /// Print given text as error line
        /// </summary>
        /// <param name="text">Error text</param>
        public void Error(string text)
        {
            error(text);
            line(); 
            line();
        }

        /// <summary>
        /// Print given text as header line
        /// </summary>
        /// <param name="text">Text</param>
        public void VariableLine(string variableName, string textDelimiter, string text)
        {
            variable(variableName);
            delimiter(textDelimiter);
            info(text);
            line();
        }

        /// <summary>
        /// Prints head and line with given text
        /// </summary>
        /// <param name="text">Text of headline</param>
        public void Headline(string text)
        {
            head(text);
            line();
        }

        /// <summary>
        /// Prints head and line with given text
        /// </summary>
        /// <param name="text">Text of headline</param>
        public void Headline2(string text)
        {
            head2(text);
            line();
        }

        /// <summary>
        /// Prints warning to output
        /// </summary>
        /// <param name="analysisWarnigs">list of analysis warning</param>
        /// <param name="securityWarnings">list of security warning</param>
        public void Warnings<T>(IReadOnlyCollection<AnalysisWarning> analysisWarnigs, IReadOnlyCollection<T> securityWarnings, bool displayTaintFlows) where T : AnalysisWarning

        {
            Headline("Warnings");
            CommentLine("Total number of warnings: " + (analysisWarnigs.Count + securityWarnings.Count));
            CommentLine("Number of first-phase analysis warnings: " + analysisWarnigs.Count);
            CommentLine("Number of taint analysis warnings: " + securityWarnings.Count);
            Warnings(analysisWarnigs, "Analysis warnings:", displayTaintFlows);
            Warnings(securityWarnings, "Security warnings:", displayTaintFlows);
        }

		/// <summary>
		/// Prints taint warnings to output..
		/// </summary>
		/// <param name="taintWarnings">Taint warnings.</param>
        public void WarningsTaint(IReadOnlyCollection<AnalysisTaintWarning> taintWarnings) 
		{
			Headline("Taint analysis warnings");
			CommentLine("Number: " + taintWarnings.Count);
            Warnings(taintWarnings, "Taint analysis warnings:", true);
		}

        /// <summary>
        /// Print metric to output
        /// </summary>
        /// <param name="metric">metric</param>
        public void Metric(string metric)
        {
            variableInfoLine(metric);
            line();
        }

        private void Warnings<T>(IReadOnlyCollection<T> warnings, string headLine, bool displayTaintFlows) where T : AnalysisWarning
        {
            line();
            Headline(headLine);
            if (warnings.Count == 0)
            {
                line();
                comment("No warnings");
            }
            string fileName = "/";
            foreach (var s in warnings)
            {
                if (fileName != s.FullFileName)
                {
                    comment("------------------------------------------------------------------------");
                    line();

                    fileName = s.FullFileName;
                    head2("File: " + fileName);
                    line();
                    comment("------------------------------------------------------------------------");
                    line();
                }
                variableInfoLine(s.ToString());
                line();
                comment("Called from: ");
                comment(s.ProgramPoint.OwningPPGraph.Context.ToString());
                if (s is AnalysisTaintWarning && displayTaintFlows) 
				{
					line();
					comment("Taint propagation: ");
                    line();
					comment(((AnalysisTaintWarning)(object)s).TaintFlow);
				}

                line();
                line();
            }

            line();

        }

        #endregion

        #region Private utilities

        /// <summary>
        /// Splits and trim text into single lines
        /// </summary>
        /// <param name="text">Text to be splitted</param>
        /// <returns>Splitted lines</returns>
        private IEnumerable<string> lineSplit(string text)
        {
            foreach (var line in text.Split('\n'))
            {
                yield return line.Trim();
            }
        }

        /// <summary>
        /// Prints line with variable info
        /// </summary>
        /// <param name="variableLine">Line with variable info</param>
        private void variableInfoLine(string variableLine)
        {
            var parts = variableLine.Split(new char[] { ':' }, 2);
            variable(parts[0].Trim());
            delimiter(": ");

            var text = parts[1].Trim();

            if (isHint(text))
            {
                hint(text);
            }
            else
            {
                info(text);
            }
        }

        /// <summary>
        /// Determine that text is hint text
        /// </summary>
        /// <param name="text">Tested text</param>
        /// <returns>True if text is hint, false otherwise</returns>
        private bool isHint(string text)
        {
            foreach (var hintPrefix in _hintPrefixes)
            {
                if (text.StartsWith(hintPrefix))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determine that line is comment line
        /// </summary>
        /// <param name="line">Tested line</param>
        /// <returns>True if line is comment, false otherwise</returns>
        private bool isCommentLine(string line)
        {
            foreach (var commentPrefix in _commentPrefixes)
            {
                if (line.StartsWith(commentPrefix))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determine that given line is empty
        /// </summary>
        /// <param name="line">Tested line</param>
        /// <returns>True if line is empty, false otherwise</returns>
        private bool isEmpty(string line)
        {
            return line.Length == 0;
        }

        #endregion
    }
}