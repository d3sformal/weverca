using PHP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    public interface IDeclarationContainerFactory
    {
        IWriteableDeclarationContainer<T> CreateWriteableDeclarationContainer<T>();
    }

    public interface IReadonlyDeclarationContainer<T>
    {
        int Count { get; }

        bool Contains(QualifiedName key);

        bool TryGetValue(QualifiedName key, out IEnumerable<T> value);

        IEnumerable<T> GetValue(QualifiedName key);

        IEnumerable<QualifiedName> GetNames();

        IWriteableDeclarationContainer<T> Copy();
    }

    public interface IWriteableDeclarationContainer<T> : IReadonlyDeclarationContainer<T>
    {
        void Add(QualifiedName key, T value);

        void SetAll(QualifiedName key, IEnumerable<T> values);
    }
}
