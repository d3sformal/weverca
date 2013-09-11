﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Analysis;

namespace Weverca.Output
{
    abstract class OutputBase
    {
        /// <summary>
        /// Prefixes for comment lines
        /// </summary>
        static List<string> _commentPrefixes = new List<string>() { "===", "---" };

        /// <summary>
        /// Prefixes for hint texts
        /// </summary>
        static List<string> _hintPrefixes=new List<string>(){"#"};

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
        internal void Indent()
        {
            ++_indentationLevel;
            setIndentation(_indentationLevel);
        }

        /// <summary>
        /// Decreates indentation level for next output
        /// </summary>
        internal void Dedent()
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
        internal void ProgramPointInfo(string pointCaption, ProgramPoint point)
        {            
            headline("PROGRAM POINT: " + pointCaption);

            Indent();
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
            Dedent();
        }

        /// <summary>
        /// Print given text as comment line
        /// </summary>
        /// <param name="text">Comment text</param>
        internal void CommentLine(string text)
        {
            comment(text);
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
        /// Prints head and line with given text
        /// </summary>
        /// <param name="text">Text of headline</param>
        private void headline(string text)
        {
            head(text);
            line();
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