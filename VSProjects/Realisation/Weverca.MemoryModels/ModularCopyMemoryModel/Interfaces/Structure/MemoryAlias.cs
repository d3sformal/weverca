using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    public interface IMemoryAlias
    {
        MemoryIndex SourceIndex { get; }

        IReadonlySet<MemoryIndex> MayAliases { get; }

        IReadonlySet<MemoryIndex> MustAliases { get; }

        IMemoryAliasBuilder Builder();
    }

    public interface IMemoryAliasBuilder
    {
        MemoryIndex SourceIndex { get; }

        IWriteableSet<MemoryIndex> MayAliases { get; }

        IWriteableSet<MemoryIndex> MustAliases { get; }

        void SetSourceIndex(MemoryIndex index);

        IMemoryAlias Build();
    }
}
