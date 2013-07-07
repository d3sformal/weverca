using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.Analysis.Memory;

namespace Weverca.MemoryModels.MemoryModel
{
    public class ObjectDescriptor
    {
        Dictionary<ContainerIndex, MemoryIndex> attributes;

        MemoryIndex unknownAttribute;

        MemoryIndex parentVariable;

        List<ObjectValue> mustReferences;
        List<ObjectValue> mayReferences;
        private PHP.Core.AST.TypeDecl type;

        public ObjectDescriptor(PHP.Core.AST.TypeDecl type)
        {
            // TODO: Complete member initialization
            this.type = type;
        }
    }
}
