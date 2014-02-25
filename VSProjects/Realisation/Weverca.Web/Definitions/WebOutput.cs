using System;
using System.Drawing;
using System.Text;

using Common.WebDefinitions.Extensions;
using Weverca.Output.Output;

namespace Weverca.Web.Definitions
{
    class WebOutput : OutputBase
    {
        #region Output settings

        static readonly Color Head = Color.White;
        static readonly Color Info = Color.Gray;
        static readonly Color Hint = Color.DarkCyan;
        static readonly Color Variable = Color.Yellow;
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

        protected override void head(string text)
        {
            print(Head, text);
        }

        protected override void info(string text)
        {
            print(Info, text);
        }

        protected override void hint(string text)
        {
            print(Hint, text);
        }

        protected override void comment(string text)
        {
            print(Comment, text);
        }

        protected override void delimiter(string text)
        {
            print(Delimiter, text);
        }

        protected override void variable(string name)
        {
            print(Variable, name);
        }

        protected override void line()
        {
            output.AppendLine("<br />");
            needPrefix = true;
        }

        protected override void error(string error)
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