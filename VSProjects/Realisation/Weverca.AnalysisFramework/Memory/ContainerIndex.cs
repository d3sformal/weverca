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

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Representation of index in container or object
    /// 
    /// NOTE:
    ///     Can be used as hash itself
    /// </summary>
    public class ContainerIndex
    {
        /// <summary>
        /// Container index identifier
        /// </summary>
        public readonly string Identifier;

        /// <summary>
        /// Creates container index identified by given identifier
        /// </summary>
        /// <param name="identifier">Index indentifier for container</param>
        internal ContainerIndex(string identifier)
        {
            Identifier = identifier;
        }
        
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            var o = obj as ContainerIndex;

            if (o == null)
            {
                return false;
            }

            return o.Identifier == Identifier;
        }
    }
}