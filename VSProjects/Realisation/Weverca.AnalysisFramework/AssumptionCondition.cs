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

using PHP.Core.AST;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Form of condition parts conjunction.
    /// </summary>
    public enum ConditionForm
    {
        /// <summary>
        /// Any true part is enough.
        /// </summary>
        Some,
        /// <summary>
        /// All parts has to be true.
        /// </summary>
        All,
        /// <summary>
        /// None part can be true.
        /// </summary>
        None,
        /// <summary>
        /// Some part has to be false.
        /// </summary>
        SomeNot,

        /// <summary>
        /// Exactly one part has to be true
        /// </summary>
        ExactlyOne,

        /// <summary>
        /// Exactly one part has to be false
        /// </summary>
        NotExactlyOne
    }

    /// <summary>
    /// Represents assumption condition in program flow.
    /// NOTE: Overrides GetHashCode and Equals methods so they can be used in hash containers.
    /// WARNING: All empty conditions with same form returns true for Equals, with same hashocode.
    /// </summary>
    public class AssumptionCondition
    {
        /// <summary>
        /// Holds initial parts for condition equality resolution
        /// </summary>
        private readonly Expression[] _initialParts;

        /// <summary>
        /// Form of condition parts conjunction.
        /// </summary>
        public readonly ConditionForm Form;

        /// <summary>
        /// Condition parts that are joined according to ConditionForm.
        /// </summary>
        public readonly IEnumerable<Expressions.Postfix> Parts;


        /// <summary>
        /// Initializes a new instance of the <see cref="AssumptionCondition" /> class.
        /// </summary>
        /// <param name="form">Form of condition parts conjunction</param>
        /// <param name="parts">Condition parts</param>
        internal AssumptionCondition(ConditionForm form, params Expression[] parts)
        {
            _initialParts = parts;
            Parts = from part in parts select Expressions.Converter.GetPostfix(part);
            Form = form;
        }
    }
}