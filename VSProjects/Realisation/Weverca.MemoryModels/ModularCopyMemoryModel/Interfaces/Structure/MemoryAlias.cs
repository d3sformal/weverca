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
        IEnumerable<MemoryIndex> MayAliasses { get; }

        IEnumerable<MemoryIndex> MustAliasses { get; }

        IMemoryAliasBuilder Builder();

        int MustAliassesCount { get; set; }

        int MayAliassesCount { get; set; }

        bool ContainsMustAlias(MemoryIndex mayAlias);

        bool ContainsMayAlias(MemoryIndex mustIndex);

        MemoryIndex SourceIndex { get; set; }
    }

    public interface IMemoryAliasBuilder
    {
        IMemoryAlias Build();

        IEnumerable<MemoryIndex> MayAliasses { get; }

        IEnumerable<MemoryIndex> MustAliasses { get; }

        int MustAliassesCount { get; set; }

        int MayAliassesCount { get; set; }

        bool ContainsMustAlias(MemoryIndex mayAlias);

        bool ContainsMayAlias(MemoryIndex mustIndex);

        void AddMustAlias(IEnumerable<MemoryIndex> mustAliases);

        void AddMayAlias(IEnumerable<MemoryIndex> mayAliases);

        void AddMustAlias(MemoryIndex mustAlias);

        void AddMayAlias(MemoryIndex mayAlias);

        void RemoveMustAlias(MemoryIndex index);

        void RemoveMayAlias(MemoryIndex mustIndex);

        void ClearMustAliases();

        void ClearMayAliases();
    }
}
