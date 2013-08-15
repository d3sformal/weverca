using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.MemoryModel
{
    public class MemoryInfo
    {
        public ReadOnlyCollection<MemoryIndex> MayAliasses { get; private set; }

        public ReadOnlyCollection<MemoryIndex> MustAliasses { get; private set; }

        internal MemoryInfo(MemoryIndex index)
        {
            MayAliasses = new ReadOnlyCollection<MemoryIndex>(new MemoryIndex[] { });
            MustAliasses = new ReadOnlyCollection<MemoryIndex>(new MemoryIndex[]{ index });
        }

        internal MemoryInfo(MemoryInfoBuilder builder)
        {
            MayAliasses = new ReadOnlyCollection<MemoryIndex>(builder.MayAliasses);
            MustAliasses = new ReadOnlyCollection<MemoryIndex>(builder.MustAliasses);
        }

        internal MemoryInfo(MemoryIndex index, List<MemoryIndex> mayAliasses, List<MemoryIndex> mustAliasses)
        {
            MayAliasses = new ReadOnlyCollection<MemoryIndex>(mayAliasses);
            MustAliasses = new ReadOnlyCollection<MemoryIndex>(mustAliasses);
        }

        internal void PostAssign(Snapshot snapshot, Analysis.Memory.MemoryEntry entry)
        {
            foreach (MemoryIndex alias in MayAliasses) {
                snapshot.WeakAssignMemoryEntry(alias, entry);
            }
        }

        public MemoryInfoBuilder Builder()
        {
            return new MemoryInfoBuilder(this);
        }
    }

    public class MemoryInfoBuilder
    {
        public List<MemoryIndex> MayAliasses { get; private set; }

        public List<MemoryIndex> MustAliasses { get; private set; }

        public MemoryInfoBuilder(MemoryIndex index)
        {
            MayAliasses = new List<MemoryIndex>();
            MustAliasses = new List<MemoryIndex>();

            MustAliasses.Add(index);
        }

        public MemoryInfoBuilder(MemoryInfo memoryInfo)
        {
            MayAliasses = new List<MemoryIndex>(memoryInfo.MayAliasses);
            MustAliasses = new List<MemoryIndex>(memoryInfo.MustAliasses);
        }

        public MemoryInfo Build()
        {
            return new MemoryInfo(this);
        }

        public MemoryInfoBuilder RemoveMustAlias(MemoryIndex index)
        {
            MustAliasses.Remove(index);
            return this;
        }

        public MemoryInfoBuilder RemoveMayAlias(MemoryIndex index)
        {
            MayAliasses.Remove(index);
            return this;
        }

        public MemoryInfoBuilder AddMustAlias(MemoryIndex index)
        {
            MustAliasses.Add(index);
            return this;
        }

        public MemoryInfoBuilder AddMayAlias(MemoryIndex index)
        {
            MayAliasses.Add(index);
            return this;
        }
    }
}
