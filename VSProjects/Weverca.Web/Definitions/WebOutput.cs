/*
Copyright (c) 2012-2014 Matyas Brenner and David Hauzar

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
using System.Drawing;
using System.Text;

using Common.WebDefinitions.Extensions;
using Weverca.Output.Output;
using System.Web;

namespace Weverca.Web.Definitions
{
    class WebOutput : OutputBase
    {
        #region Output settings

        static readonly Color Head = Color.Black;
        static readonly Color Info = Color.Gray;
        static readonly Color Hint = Color.DarkCyan;
        static readonly Color Variable = Color.OrangeRed;
        static readonly Color Delimiter = Color.Red;
        static readonly Color ErrorColor = Color.Red;
        static readonly Color Comment = Color.Green;
        static readonly int indentationLength = 4;


        #endregion

        #region Private members

        private bool needPrefix = true;
        private string prefix = string.Empty;

        StringBuilder output = new StringBuilder();

        #endregion

        #region Properties

        public string Output { get { return output.ToString(); } }

        #endregion

        #region OutputBase implementation

        protected override void setIndentation(int level)
        {
            prefix = string.Empty.PadLeft(level * indentationLength, ' ');
            prefix = prefix.Replace(" ", "&nbsp;");
        }

        public override void head(string text)
        {
            print(Head, text);
        }

        public override void head2(string text)
        {
            print(Head, text);
        }

        public override void info(string text)
        {
              print(Info, System.Web.HttpUtility.HtmlEncode(text));
        }

        public override void hint(string text)
        {
            print(Hint, System.Web.HttpUtility.HtmlEncode(text));
        }

        public override void comment(string text)
        {
            print(Comment, text);
        }

        public override void delimiter(string text)
        {
            print(Delimiter, text);
        }

        public override void variable(string name)
        {
            print(Variable, System.Web.HttpUtility.HtmlEncode(name));
        }

        public override void line()
        {
            output.AppendLine("<br />");
            needPrefix = true;
        }

        public override void error(string error)
        {
            print(ErrorColor, error);
        }

        #endregion

        #region Private utilities

        private void print(Color color, string text)
        {
            output.Append(string.Format("<font color=\"{0}\">", color.ToHexadecimal()));

            if (needPrefix)
            {
                text = prefix + text;
                needPrefix = false;
            }
            output.Append(text);
            output.Append("</font>");
        }

        #endregion


        
    }
}