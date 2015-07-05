using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyCopyStructure
{
    class LazyCopyArrayDescriptorFactory : IArrayDescriptorFactory
    {
        public IArrayDescriptor CreateArrayDescriptor(IWriteableSnapshotStructure targetStructure, AssociativeArray createdArray, MemoryIndex memoryIndex)
        {
            LazyCopyArrayDescriptor descriptor = new LazyCopyArrayDescriptor(targetStructure);
            descriptor.SetArrayValue(createdArray);
            descriptor.SetParentIndex(memoryIndex);
            descriptor.SetUnknownIndex(memoryIndex.CreateUnknownIndex());
            return descriptor;
        }
    }


    class LazyCopyArrayDescriptor : LazyCopyIndexContainer, IArrayDescriptor, IArrayDescriptorBuilder
    {
        private MemoryIndex parentIndex;
        private AssociativeArray arrayValue;
        private IWriteableSnapshotStructure associatedStrucutre;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyCopyArrayDescriptor"/> class.
        /// </summary>
        public LazyCopyArrayDescriptor(IWriteableSnapshotStructure associatedStrucutre)
        {
            this.associatedStrucutre = associatedStrucutre;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyCopyArrayDescriptor"/> class.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="arrayValue">The array value.</param>
        public LazyCopyArrayDescriptor(IWriteableSnapshotStructure associatedStrucutre, MemoryIndex parentIndex, AssociativeArray arrayValue)
        {
            this.associatedStrucutre = associatedStrucutre;
            this.parentIndex = parentIndex;
            this.arrayValue = arrayValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyCopyArrayDescriptor"/> class.
        /// New instance contains copy of given object.
        /// </summary>
        /// <param name="arrayDescriptor">The array descriptor.</param>
        public LazyCopyArrayDescriptor(IWriteableSnapshotStructure associatedStrucutre, LazyCopyArrayDescriptor arrayDescriptor)
            : base(arrayDescriptor)
        {
            this.associatedStrucutre = associatedStrucutre;
            this.parentIndex = arrayDescriptor.parentIndex;
            this.arrayValue = arrayDescriptor.arrayValue;
        }

        /// <inheritdoc />
        public MemoryIndex ParentIndex
        {
            get { return parentIndex; }
        }

        /// <inheritdoc />
        public AssociativeArray ArrayValue
        {
            get { return arrayValue; }
        }

        /// <inheritdoc />
        public IArrayDescriptorBuilder Builder(IWriteableSnapshotStructure targetStructure)
        {
            if (targetStructure == associatedStrucutre)
            {
                return this;
            }
            else
            {
                return new LazyCopyArrayDescriptor(targetStructure, this);
            }
        }

        /// <inheritdoc />
        public void SetParentIndex(MemoryIndex parentIndex)
        {
            this.parentIndex = parentIndex;
        }

        /// <inheritdoc />
        public void SetArrayValue(AssociativeArray arrayValue)
        {
            this.arrayValue = arrayValue;
        }

        /// <inheritdoc />
        public IArrayDescriptor Build(IWriteableSnapshotStructure targetStructure)
        {
            if (targetStructure == associatedStrucutre)
            {
                return this;
            }
            else
            {
                return new LazyCopyArrayDescriptor(targetStructure, this);
            }
        }

    }
}
