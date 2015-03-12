/*
Copyright (c) 2012-2015 David Hauzar

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

using PHP.Core.AST;

namespace Weverca.Common
{
    /// <summary>
    /// This visitor will find all DirectVarUse, IndirectVarUse and ItemUse constructs in given AST.
    /// </summary>
    public class VariableCollector : TreeVisitor
    {
        private HashSet<VarLikeConstructUse> vars = new HashSet<VarLikeConstructUse> ();

        /// <summary>
        /// Gets all DirectVarUse, IndirectVarUse and ItemUse constructs in given AST.
        /// </summary>
        public IEnumerable<VarLikeConstructUse> Variables {
            get {
                return vars;
            }
        }

        /// <inheritdoc />
        public override void VisitDirectVarUse (DirectVarUse x)
        {
            base.VisitDirectVarUse (x);
            vars.Add (x);
        }

        /// <inheritdoc />
        public override void VisitIndirectVarUse (IndirectVarUse x)
        {
            base.VisitIndirectVarUse (x);

            vars.Add (x);
        }

        /// <inheritdoc />
        public override void VisitItemUse (ItemUse x)
        {
            base.VisitItemUse (x);

            vars.Add (x);
        }
    }
}

