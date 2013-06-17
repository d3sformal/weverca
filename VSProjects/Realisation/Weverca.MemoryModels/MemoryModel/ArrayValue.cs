using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Weverca.Analysis.Memory;
using Weverca.Analysis;

namespace Weverca.MemoryModel
{
    class ArrayValue 
    {
        Dictionary<ContainerIndex, VariableInfo> data = new Dictionary<ContainerIndex, VariableInfo>();

        protected bool equals(AssociativeArray value)
        {
            throw new NotImplementedException();
        }

        protected  int getHashCode()
        {
            throw new NotImplementedException();
        }

        protected  AssociativeArray setIndex(ContainerIndex index, MemoryEntry value)
        {
            throw new NotImplementedException();
        }

        protected  AssociativeArray setIndexAlias(ContainerIndex index, Analysis.Memory.AliasValue alias)
        {
            throw new NotImplementedException();
        }

        public  MemoryEntry GetIndex(ContainerIndex index)
        {
            throw new NotImplementedException();
        }

        public  Analysis.Memory.AliasValue CreateAlias(ContainerIndex index)
        {
            throw new NotImplementedException();
        }
    }
}
