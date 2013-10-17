using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class ReadCollector : IndexCollector
    {
        HashSet<MemoryIndex> mustIndexes = new HashSet<MemoryIndex>();
        HashSet<MemoryIndex> mayIndexes = new HashSet<MemoryIndex>();

        HashSet<MemoryIndex> mustIndexesSwap = new HashSet<MemoryIndex>();
        HashSet<MemoryIndex> mayIndexesSwap = new HashSet<MemoryIndex>();

        public override IEnumerable<MemoryIndex> MustIndexes
        {
            get { return mustIndexes; }
        }

        public override IEnumerable<MemoryIndex> MayIndexes
        {
            get { return mayIndexes; }
        }

        public override void Next(Snapshot snapshot, PathSegment segment)
        {
            switch (segment.Type)
            {
                case PathType.Variable:
                    processVariable(snapshot, segment);
                    break;
                case PathType.Field:
                    processField(snapshot, segment);
                    break;
                case PathType.Index:
                    processIndex(snapshot, segment);
                    break;

                default: throw new NotImplementedException();
            }

        }

        private void processVariable(Snapshot snapshot, PathSegment segment)
        {
            mustIndexesSwap.Clear();
            mayIndexesSwap.Clear();

            if (segment.IsAny)
            {

            }
            else if (segment.IsDirect)
            {

            }
            else
            {

            }
        }

        private void processField(Snapshot snapshot, PathSegment segment)
        {
            throw new NotImplementedException();
        }

        private void processIndex(Snapshot snapshot, PathSegment segment)
        {
            throw new NotImplementedException();
        }
    }
}
