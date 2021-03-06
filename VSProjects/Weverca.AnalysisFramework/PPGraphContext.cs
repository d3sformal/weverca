/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

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


﻿using System;
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
            
            for (int c = 0; c < _callers.Count; c++)
            {
                ProgramPointBase caller = _callers[c];
                // If the program point calls itself, continue (this occurs, e.g., in case sharing graphs in recursive calls)
                if (OwningPPGraph == caller.OwningPPGraph)
                    continue;

                result.Append(" -> ");
                //result.Append("(");

				//result.Append(caller.OwningPPGraph.OwningScript.FullName + " at position " + caller.Partial.Position);
                result.Append(caller.OwningScriptFullName + " at position " + caller.Partial.Position);
                result.Append(caller.OwningPPGraph.Context);

                //result.Append(")");
                if (c != _callers.Count - 1) result.Append("or");

                //result.Append("( " + caller.ToString() + " )");
            }
                

            //result.AppendLine();

            return result.ToString();
        }

    }
}