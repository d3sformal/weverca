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
    class CreateCollector
    {
        List<MemoryIndex> mustIndexes = new List<MemoryIndex>();
        List<MemoryIndex> mayIndexes = new List<MemoryIndex>();
        List<CollectedValue> collectedValues = new List<CollectedValue>();

        List<MemoryIndex> mustIndexesProcess = new List<MemoryIndex>();

        /*public override bool IsDefined { get; protected set; }

        public override IEnumerable<MemoryIndex> MustIndexes
        {
            get { return mustIndexes; }
        }

        public override IEnumerable<MemoryIndex> MayIndexes
        {
            get { return mayIndexes; }
        }

        public override IEnumerable<CollectedValue> CollectedValues
        {
            get { return collectedValues; }
        }


        public override int MustIndexesCount
        {
            get { return mustIndexes.Count; }
        }

        public override int MayIndexesCount
        {
            get { return mayIndexes.Count; }
        }

        public override void Next(PathSegment segment)
        {
            throw new NotImplementedException();
        }*/
    }
}
