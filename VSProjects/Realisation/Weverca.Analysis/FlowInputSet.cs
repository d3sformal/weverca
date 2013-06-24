using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis
{
    /// <summary>
    /// Set of FlowInfo used as input for statement analysis.
    /// </summary>    
    public class FlowInputSet
    {
        protected internal AbstractSnapshot _snapshot;

        public ISnapshotReadonly Input { get { return _snapshot; } }

        internal FlowInputSet(AbstractSnapshot snapshot)
        {
            _snapshot = snapshot;
        }
    }
}
