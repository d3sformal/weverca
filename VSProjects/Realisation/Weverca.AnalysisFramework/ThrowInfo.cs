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

using PHP.Core;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Info used for throwing exception within framework
    /// </summary>
    public class ThrowInfo
    {
        /// <summary>
        /// Catch block description where thrown exception is handled
        /// </summary>
        public readonly CatchBlockDescription Catch;

        /// <summary>
        /// Value that has been throwed
        /// </summary>
        public readonly MemoryEntry ThrowedValue;

        /// <summary>
        /// Creates new instance of ThrowInfo
        /// </summary>
        /// <param name="catchBlock">inforamation about catchblocks</param>
        /// <param name="throwedValue">possible throwd values</param>
        public ThrowInfo(CatchBlockDescription catchBlock, MemoryEntry throwedValue)
        {
            Catch = catchBlock;
            ThrowedValue = throwedValue;
        }
    }
}