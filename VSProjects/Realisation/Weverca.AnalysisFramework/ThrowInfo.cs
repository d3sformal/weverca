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
