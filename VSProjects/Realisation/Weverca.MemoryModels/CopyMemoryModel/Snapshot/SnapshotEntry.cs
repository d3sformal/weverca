using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class SnapshotEntry : ReadWriteSnapshotEntryBase
    {

        HashSet<MemoryIndex> memoryIndexes;

        public override void WriteMemory(MemoryEntry value)
        {
            throw new NotImplementedException();
        }

        public override void SetAliases(IEnumerable<AliasEntry> aliases)
        {
            throw new NotImplementedException();
        }

        protected override bool isDefined()
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<AliasEntry> aliases()
        {
            throw new NotImplementedException();
        }

        protected override MemoryEntry readMemory()
        {
            throw new NotImplementedException();
        }

        protected override ReadWriteSnapshotEntryBase readIndex(MemberIdentifier index)
        {
            throw new NotImplementedException();
        }

        protected override ReadWriteSnapshotEntryBase readField(AnalysisFramework.VariableIdentifier field)
        {
            throw new NotImplementedException();
        }

        protected override AnalysisFramework.VariableIdentifier getVariableIdentifier()
        {
            throw new NotImplementedException();
        }
    }
}
