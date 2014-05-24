using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    public interface IReadonlyDeclarationContainer<T>
    {

    }

    public interface IWriteableDeclarationContainer<T> : IReadonlyDeclarationContainer<T>
    {

    }
}
