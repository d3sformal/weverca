using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyCopyStructure
{
    class LazyCopyObjectValueContainer : LazyCopySet<ObjectValue>, IObjectValueContainer, IObjectValueContainerBuilder
    {
        private IWriteableSnapshotStructure associatedStructure;
        /// <summary>
        /// Initializes a new instance of the <see cref="CopyObjectValueContainer"/> class.
        /// </summary>
        public LazyCopyObjectValueContainer(IWriteableSnapshotStructure associatedStructure)
        {
            this.associatedStructure = associatedStructure;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyObjectValueContainer"/> class.
        /// </summary>
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
