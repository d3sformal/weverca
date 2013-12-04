using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class IndexData
    {
        public MemoryEntry MemoryEntry { get; private set; }
        public MemoryAlias Aliases { get; private set; }
        public AssociativeArray Array { get; private set; }
        public ObjectValueContainer Objects { get; private set; }

        public IndexData(MemoryEntry memoryEntry, MemoryAlias aliases, AssociativeArray array, ObjectValueContainer objects)
        {
            MemoryEntry = memoryEntry;
            Aliases = aliases;
            Array = array;
            Objects = objects;
        }
        
        public IndexData(IndexDataBuilder data)
        {
            MemoryEntry = data.MemoryEntry;
            Aliases = data.Aliases;
            Array = data.Array;
            Objects = data.Objects;
        }

        public IndexDataBuilder Builder()
        {
            return new IndexDataBuilder(this);
        }

        internal bool DataEquals(IndexData other)
        {
            if (this == other)
            {
                return true;
            }

            if (this.MemoryEntry != other.MemoryEntry)
            {
                if (MemoryEntry == null || !this.MemoryEntry.Equals(other.MemoryEntry))
                {
                    return false;
                }
            }

            if (this.Aliases != other.Aliases)
            {
                if (Aliases == null || !this.Aliases.DataEquals(other.Aliases))
                {
                    return false;
                }
            }

            if (this.Array != other.Array)
            {
                if (Array == null || !this.Array.Equals(other.Array))
                {
                    return false;
                }
            }

            if (this.Objects != other.Objects)
            {
                if (Objects == null || !this.Objects.DataEquals(other.Objects))
                {
                    return false;
                }
            }

            return true;
        }
    }
    class IndexDataBuilder
    {
        public MemoryEntry MemoryEntry { get; set; }
        public MemoryAlias Aliases { get; set; }
        public AssociativeArray Array { get; set; }
        public ObjectValueContainer Objects { get; set; }

        public IndexDataBuilder(IndexData data)
        {
            MemoryEntry = data.MemoryEntry;
            Aliases = data.Aliases;
            Array = data.Array;
            Objects = data.Objects;
        }

        public IndexData Build()
        {
            return new IndexData(this);
        }
    }
}
