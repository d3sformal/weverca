using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    public interface IReadonlySet<T>
    {
        bool Contains(Memory.TemporaryIndex temporaryIndex);
    }

    public interface IWriteableSet<T>
    {

        void Add(Memory.TemporaryIndex tmp);

        void Remove(Memory.TemporaryIndex temporaryIndex);
    }
}
