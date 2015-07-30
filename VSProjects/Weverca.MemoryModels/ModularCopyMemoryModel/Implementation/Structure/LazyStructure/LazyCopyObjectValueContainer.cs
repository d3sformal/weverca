using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyStructure
{
    class LazyCopyObjectValueContainerFactory : IObjectValueContainerFactory
    {
        public IObjectValueContainer CreateObjectValueContainer(IWriteableSnapshotStructure targetStructure, IEnumerable<ObjectValue> objects)
        {
            LazyCopyObjectValueContainer container = new LazyCopyObjectValueContainer(targetStructure);
            container.AddAll(objects);
            return container;
        }


        public IObjectValueContainer CreateObjectValueContainer(IWriteableSnapshotStructure targetStructure)
        {
            return new LazyCopyObjectValueContainer(targetStructure);
        }
    }

    /// <summary>
    /// Lazy implementation of object value container. Creation of builder prevents creating a new copy when 
    /// builder is created for the same version of the structure object. This behavior allows to 
    /// group all changes made in single transaction and to prevent unnecessary copying.
    /// </summary>
    class LazyCopyObjectValueContainer : LazyCopySet<ObjectValue>, IObjectValueContainer, IObjectValueContainerBuilder
    {
        private IWriteableSnapshotStructure associatedStructure;
        /// <summary>
        /// Initializes a new instance of the <see cref="LazyCopyObjectValueContainer" /> class.
        /// </summary>
        /// <param name="associatedStructure">The associated structure.</param>
        public LazyCopyObjectValueContainer(IWriteableSnapshotStructure associatedStructure)
        {
            this.associatedStructure = associatedStructure;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyCopyObjectValueContainer" /> class.
        /// </summary>
        /// <param name="associatedStructure">The associated structure.</param>
        /// <param name="objectContainer">The object container.</param>
        public LazyCopyObjectValueContainer(IWriteableSnapshotStructure associatedStructure, LazyCopyObjectValueContainer objectContainer)
            : base(objectContainer)
        {
            this.associatedStructure = associatedStructure;
        }

        /// <inheritdoc />
        public IObjectValueContainerBuilder Builder(IWriteableSnapshotStructure targetStructure)
        {
            if (targetStructure == associatedStructure)
            {
                return this;
            }
            else
            {
                return new LazyCopyObjectValueContainer(targetStructure, this);
            }
        }

        /// <inheritdoc />
        public new System.Collections.IEnumerator GetEnumerator()
        {
            return ((LazyCopySet<ObjectValue>)this).GetEnumerator();
        }

        /// <inheritdoc />
        public IObjectValueContainer Build(IWriteableSnapshotStructure targetStructure)
        {
            if (targetStructure == associatedStructure)
            {
                return this;
            }
            else
            {
                return new LazyCopyObjectValueContainer(targetStructure, this);
            }
        }
    }
}
