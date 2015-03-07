/*
Copyright (c) 2012-2014 David Hauzar, Miroslav Vodolan, Marcel Kikta, Pavel Bastecky, David Skorvaga, and Matyas Brenner

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

using Weverca.Output.Output;

namespace Weverca.Output
{
    class ConsoleOutput : OutputBase
    {
        #region Output settings

        static readonly ConsoleColor Head = ConsoleColor.White;
        static readonly ConsoleColor Info = ConsoleColor.Gray;
        static readonly ConsoleColor Hint = ConsoleColor.DarkCyan;
        static readonly ConsoleColor Variable = ConsoleColor.Yellow;
        static readonly ConsoleColor Delimiter = ConsoleColor.Red;
        static readonly ConsoleColor ErrorColor = ConsoleColor.Red;
        static readonly ConsoleColor Comment = ConsoleColor.Green;
        static readonly int indentationLength = 4;


        #endregion

        #region Private members

        private bool needPrefix = true;
        private string prefix = "";

        #endregion

        #region OutputBase implementation

        protected override void setIndentation(int level)
        {
            prefix = "".PadLeft(level * indentationLength, ' ');
        }

        protected override void head(string text)
        {
            print(Head, text);
        }

        protected override void head2(string text)
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
            Console.WriteLine();
            needPrefix = true;
        }

        protected override void error(string error)
        {
            print(ErrorColor, error);
        }

        #endregion

        #region Private utilities

        private void print(ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;

            if (needPrefix)
            {
                text = prefix + text;
                needPrefix = false;
            }
            Console.Write(text);
        }

        #endregion
    }
}