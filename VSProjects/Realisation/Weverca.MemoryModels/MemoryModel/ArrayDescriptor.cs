using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.Analysis.Memory;

namespace Weverca.MemoryModels.MemoryModel
{
    public class ArrayDescriptor
    {
        Dictionary<ContainerIndex, MemoryIndex> fields;

        MemoryIndex unknownField;

        MemoryIndex parentVariable;
    }
}
