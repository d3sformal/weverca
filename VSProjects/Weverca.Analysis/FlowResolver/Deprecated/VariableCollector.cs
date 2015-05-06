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


using System.Linq;
using System.Collections.Generic;

using PHP.Core.AST;

namespace Weverca.Analysis.FlowResolver.Deprecated
{
    /// <summary>
    /// This visitor will find all variable uses in an AST.
    /// </summary>
    public class VariableCollector : TreeVisitor
    {
        /// <summary>
        /// Gets the variable uses.
        /// </summary>
        public IEnumerable<VariableUse> Variables {
            get {
                return directlyUsed.Cast<VariableUse> ().Concat (indirectlyUsed).Cast<VariableUse> ();
            }
        }

        List<DirectVarUse> directlyUsed = new List<DirectVarUse> ();
        List<IndirectVarUse> indirectlyUsed = new List<IndirectVarUse> ();

        /// <summary>
        /// Visits the direct variable use.
        /// </summary>
        /// <param name="x">The executable.</param>
        public override void VisitDirectVarUse (DirectVarUse x)
        {
            base.VisitDirectVarUse (x);
            if (directlyUsed.FirstOrDefault (a => a.VarName != x.VarName) == null) {
                directlyUsed.Add (x);
            }
        }

        /// <summary>
        /// Visits the indirect variable use.
        /// </summary>
        /// <param name="x">The executable.</param>
        public override void VisitIndirectVarUse (IndirectVarUse x)
        {
            base.VisitIndirectVarUse (x);

            //TODO: find a way to get only distinct uses.
            if (indirectlyUsed.FirstOrDefault (a => a.VarNameEx.Value != x.VarNameEx.Value) == null) {
                indirectlyUsed.Add (x);
            }
        }
    }
}