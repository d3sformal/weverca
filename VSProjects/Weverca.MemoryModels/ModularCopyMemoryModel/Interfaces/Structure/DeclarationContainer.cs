using PHP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    /// <summary>
    /// Factory class which creates instances implementing an interface
    /// IWriteableDeclarationContainer.
    /// </summary>
    public interface IDeclarationContainerFactory
    {
        /// <summary>
        /// Creates the new empty writeable declaration container.
        /// </summary>
        /// <typeparam name="T">Type of the declaration stored within the container</typeparam>
        /// <returns>The new empty writeable declaration container.</returns>
        IWriteableDeclarationContainer<T> CreateWriteableDeclarationContainer<T>();
    }

    /// <summary>
    /// Readonly version of the declaration container. Declaration container is used by the 
    /// snapshot structure to store all possible declared classes or functions.
    /// 
    /// It is an associative container which maps QualifiedName to the set of possible declarations.
    /// </summary>
    /// <typeparam name="T">Type of the declaration stored within the container</typeparam>
    public interface IReadonlyDeclarationContainer<T>
    {
        /// <summary>
        /// Gets the number of declarations stored within this container.
        /// </summary>
        /// <value>
        /// The number of declarations stored within this container..
        /// </value>
        int Count { get; }

        /// <summary>
        /// Determines whether this container contains the declaration specified by given name.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if this container contains the declaration; otherwise false.</returns>
        bool Contains(QualifiedName key);

        /// <summary>
        /// Gets the possible declarations associated with the specified name.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if the container contains an element with the specified key; otherwise, false.</returns>
        bool TryGetValue(QualifiedName key, out IEnumerable<T> value);

        /// <summary>
        /// Gets the possible declarations associated with the specified name.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value stored with specified key.</returns>
        IEnumerable<T> GetValue(QualifiedName key);

        /// <summary>
        /// Gets the list of all names stored within this container.
        /// </summary>
        /// <returns>The list of all names stored within this container.</returns>
        IEnumerable<QualifiedName> GetNames();

        /// <summary>
        /// Creates new instance and copy all data from this container.
        /// </summary>
        /// <returns>New instance with all data from this container.</returns>
        IWriteableDeclarationContainer<T> Copy();
    }

    /// <summary>
    /// Writeable version of the declaration container. Declaration container is used by the 
    /// snapshot structure to store all possible declared classes or functions.
    /// 
    /// It is an associative container which maps QualifiedName to the set of possible declarations.
    /// </summary>
    /// <typeparam name="T">Type of the declaration stored within the container</typeparam>
    public interface IWriteableDeclarationContainer<T> : IReadonlyDeclarationContainer<T>
    {
        /// <summary>
        /// Adds the given declaration to the set of the given name.
        /// 
        /// If the container doesn't contain given name then new empty set of possible declarations 
        /// will be created and given value will be the only possible declaration.
        /// If container contains given name then the given declaration will be inserted the theset.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        void Add(QualifiedName key, T value);

        /// <summary>
        /// Sets the given set as the possible declaration of the given name.
        /// 
        /// If the container contains set with the given name then the set will be removed.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        void SetAll(QualifiedName key, IEnumerable<T> values);
    }
}
