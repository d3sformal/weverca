using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    public interface IObjectValueContainer
    {
        int Count { get; set; }

        IObjectValueContainerBuilder Builder();
    }

    public interface IObjectValueContainerBuilder
    {
        IObjectValueContainer Build();

        void Add(AnalysisFramework.Memory.ObjectValue objectValue);

        void Remove(AnalysisFramework.Memory.ObjectValue value);
    }
}
