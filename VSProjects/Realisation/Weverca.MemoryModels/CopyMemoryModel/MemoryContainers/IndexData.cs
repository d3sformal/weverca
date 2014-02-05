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
        public MemoryAlias Aliases { get; private set; }
        public AssociativeArray Array { get; private set; }
        public ObjectValueContainer Objects { get; private set; }

        public IndexData(MemoryAlias aliases, AssociativeArray array, ObjectValueContainer objects)
        {
            Aliases = aliases;
            Array = array;
            Objects = objects;
        }
        
        public IndexData(IndexDataBuilder data)
        {
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

            if (this.Aliases != other.Aliases)
            {
                if (!MemoryAlias.AreEqual(Aliases, other.Aliases))
                {
                    return false;
                }
            }

            if (this.Array != other.Array)
            {
                if (Array == null || other.Array == null || !this.Array.Equals(other.Array))
                {
                    return false;
                }
            }

            if (this.Objects != other.Objects)
            {
                if (!ObjectValueContainer.AreEqual(Objects, other.Objects))
                {
                    return false;
                }
            }

            return true;
        }
    }
    class IndexDataBuilder
    {
        public MemoryAlias Aliases { get; set; }
        public AssociativeArray Array { get; set; }
        public ObjectValueContainer Objects { get; set; }

        public IndexDataBuilder(IndexData data)
        {
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
