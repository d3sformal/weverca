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

using PHP.Core;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Represents possible names resolved for single variable selector in source code
    /// </summary>
    public class VariableIdentifier
    {
        /// <summary>
        /// Possible names of variable
        /// </summary>
        public readonly VariableName[] PossibleNames;

        /// <summary>
        /// Determine that there is only single possible name
        /// </summary>
        public bool IsDirect { get { return PossibleNames.Length == 1; } }        
        
        /// <summary>
        /// Determine that variable identifier name is not known
        /// </summary>
        public bool IsUnknown { get { return PossibleNames.Length == 0; } }

        /// <summary>
        /// If VariableEntry IsDirect we can read it's name
        /// </summary>
        public VariableName DirectName
        {
            get
            {
                if (!IsDirect)
                {
                    throw new NotSupportedException("Cannot get direct variable name on InDirect entry");
                }

                return PossibleNames[0];
            }
        }

        /// <summary>
        /// Creates variable entry from given possible names
        /// </summary>
        /// <param name="possibleNames">Possible names for variable selector</param>
        public VariableIdentifier(IEnumerable<string> possibleNames)
        {
            var variableNames = new List<VariableName>();
            foreach (var name in possibleNames)
            {
                variableNames.Add(new VariableName(name));
            }
            PossibleNames = variableNames.ToArray();
        }

        /// <summary>
        /// Creates variable entry from given possible names
        /// </summary>
        /// <param name="possibleNames">Possible names for variable selector</param>
        public VariableIdentifier(params string[] possibleNames)
            : this((IEnumerable<string>)possibleNames)
        {
        }

        /// <summary>
        /// Creates variable entry from direct name
        /// </summary>
        /// <param name="directName">Direct name for variable selector</param>
        public VariableIdentifier(VariableName directName)
        {
            PossibleNames = new VariableName[] { directName };
        }
    }
}