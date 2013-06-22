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
    /// Set of FlowInfo used as output from statement analysis.   
    /// </summary>
    public class FlowOutputSet :FlowInputSet
    {
        public ISnapshotReadWrite Output { get; private set; }

        internal void Commit()
        {
            throw new NotImplementedException();
        }

        internal void StartTransaction()
        {
            throw new NotImplementedException();
        }
    }
}
