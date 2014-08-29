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


/*
Copyright (c) 2012-2014 David Hauzar and Mirek Vodolan.

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

using PHP.Core.AST;

namespace Weverca.AnalysisFramework.ProgramPoints
{
    /// <summary>
    /// Represents empty program point (it doesn't change flow)
    /// </summary>
    public class EmptyProgramPoint : ProgramPointBase
    {
        /// <inheritdoc />
        public override LangElement Partial { get { return null; } }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            //no action is needed
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitEmpty(this);
        }

        /*
		protected override void extendOutput()
		{
			if (FlowParentsCount == 1) 
			{
				_outSet = _inSet;
				//_outSet.StartTransaction();
				return;
			}

			base.extendOutput ();
		}

		/// <summary>
		/// Extends the input.
		/// </summary>
		protected override void extendInput()
		{
			if (FlowParentsCount == 1) 
			{
				_inSet = FlowParents.First().OutSet;
				return;
			}

			base.extendInput ();
		}

		protected override void commitFlow()
		{
			if (FlowParentsCount == 1) 
			{
				enqueueChildren ();
				return;
			}

			base.commitFlow ();
		}
         * */
    }
}