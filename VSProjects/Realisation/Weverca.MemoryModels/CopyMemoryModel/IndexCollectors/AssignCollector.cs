using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class AssignCollector : IndexCollector
    {
        public override IEnumerable<MemoryIndex> MustIndexes
        {
            get { throw new NotImplementedException(); }
        }

        public override IEnumerable<MemoryIndex> MayIndexes
        {
            get { throw new NotImplementedException(); }
        }

        public override void Next(Snapshot snapshot, PathSegment segment)
        {
            throw new NotImplementedException();
        }
    }
}
