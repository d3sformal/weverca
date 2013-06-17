using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Weverca.Analysis.Memory;
using Weverca.Analysis;

namespace Weverca.MemoryModel
{
    class ObjectValue 
    {

        protected  bool equals(Analysis.Memory.ObjectValue value)
        {
            throw new NotImplementedException();
        }

        protected  int getHashCode()
        {
            throw new NotImplementedException();
        }

        protected  Analysis.Memory.ObjectValue setField(ContainerIndex field, MemoryEntry value)
        {
            throw new NotImplementedException();
        }

        protected  Analysis.Memory.ObjectValue setFieldAlias(ContainerIndex field, Analysis.Memory.AliasValue alias)
        {
            throw new NotImplementedException();
        }

        public  MemoryEntry GetField(ContainerIndex field)
        {
            throw new NotImplementedException();
        }

        public  Analysis.Memory.AliasValue CreateAlias(ContainerIndex field)
        {
            throw new NotImplementedException();
        }

        public  IEnumerable<PHP.Core.AST.FunctionDecl> GetFunction(string functionName)
        {
            throw new NotImplementedException();
        }

        public  Analysis.Memory.ObjectValue DefineFunction(string functionName, PHP.Core.AST.FunctionDecl function)
        {
            throw new NotImplementedException();
        }
    }
}
