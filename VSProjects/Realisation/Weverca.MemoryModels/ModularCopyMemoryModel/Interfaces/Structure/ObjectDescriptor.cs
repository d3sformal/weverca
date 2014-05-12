using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    public interface IObjectDescriptor : IReadonlyIndexContainer
    {
        TypeValue Type { get; }

        ObjectValue ObjectValue { get; }

        IObjectDescriptorBuilder Builder();
    }

    public interface IObjectDescriptorBuilder : IObjectDescriptor, IWriteableIndexContainer
    {
        void SetType(TypeValue type);

        void SetObjectvalue(ObjectValue objectValue);

        IObjectDescriptor Build();

        IObjectDescriptorBuilder add(string fieldName, Memory.MemoryIndex fieldIndex);
    }
}
