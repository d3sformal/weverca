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

using System.Collections;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Represents iterator which is used for traversing arrays and objects
    /// </summary>
    public abstract class ContainerIteratorBase
    {
        /// <summary>
        /// Iterate through whole structure index by index.
        /// </summary>
        /// <returns>Next container index or null if not available</returns>
        protected abstract ContainerIndex getNextIndex();

        /// <summary>
        /// Determine that iteration has already ended (means that no other MoveNext calls are allowed)
        /// </summary>
        public bool IterationEnd { get; private set; }

        /// <summary>
        /// Current index pointed by iterator
        /// </summary>
        public ContainerIndex CurrentIndex { get; private set; }
            
        /// <summary>
        /// Moves to next container index in iteration
        /// </summary>
        /// <returns>Current container index</returns>
        public ContainerIndex MoveNext()
        {
            CurrentIndex= getNextIndex();
            if (CurrentIndex == null)
            {
                IterationEnd = true;
            }

            return CurrentIndex;
        }
    }
}