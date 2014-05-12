using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    public interface IArrayDescriptor : IReadonlyIndexContainer
    {
        MemoryIndex ParentIndex { get; }

        AssociativeArray ArrayValue { get; }

        IArrayDescriptorBuilder Builder();
    }

    public interface IArrayDescriptorBuilder : IReadonlyIndexContainer, IWriteableIndexContainer
    {
        IArrayDescriptorBuilder SetParentIndex(MemoryIndex parentIndex);

        IArrayDescriptorBuilder SetArrayValue(AssociativeArray arrayValue);

        IArrayDescriptor Build();

        IArrayDescriptorBuilder SetUnknownIndex(MemoryIndex memoryIndex);

        IArrayDescriptorBuilder add(string indexName, MemoryIndex indexIndex);
    }
}
