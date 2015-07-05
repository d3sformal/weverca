using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyCopyStructure
{
    class LazyCopyObjectDescriptorFactory : IObjectDescriptorFactory
    {
        public IObjectDescriptor CreateObjectDescriptor(IWriteableSnapshotStructure targetStructure, ObjectValue createdObject, TypeValue type, Memory.MemoryIndex memoryIndex)
        {
            LazyCopyObjectDescriptor descriptor = new LazyCopyObjectDescriptor(targetStructure);
            descriptor.SetObjectValue(createdObject);
            descriptor.SetType(type);
            descriptor.SetUnknownIndex(memoryIndex);
            return descriptor;
        }
    }


    class LazyCopyObjectDescriptor : LazyCopyIndexContainer, IObjectDescriptor, IObjectDescriptorBuilder
    {
        private TypeValue type;
        private ObjectValue objectValue;
        private IWriteableSnapshotStructure associatedStructure;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyObjectDescriptor"/> class.
        /// </summary>
        public LazyCopyObjectDescriptor(IWriteableSnapshotStructure associatedStructure)
        {
            this.associatedStructure = associatedStructure;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyObjectDescriptor"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="objectValue">The object value.</param>
        public LazyCopyObjectDescriptor(IWriteableSnapshotStructure associatedStructure, TypeValue type, ObjectValue objectValue)
        {
            this.type = type;
            this.objectValue = objectValue;
            this.associatedStructure = associatedStructure;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyObjectDescriptor"/> class.
        /// New instance contains copy of given object.
        /// </summary>
        /// <param name="objectDescriptor">The object descriptor.</param>
        public LazyCopyObjectDescriptor(IWriteableSnapshotStructure associatedStructure, LazyCopyObjectDescriptor objectDescriptor)
            : base(objectDescriptor)
        {
            this.type = objectDescriptor.type;
            this.objectValue = objectDescriptor.objectValue;
            this.associatedStructure = associatedStructure;
        }

        /// <inheritdoc />
        public TypeValue Type
        {
            get { return type; }
        }

        /// <inheritdoc />
        public ObjectValue ObjectValue
        {
            get { return objectValue; }
        }

        /// <inheritdoc />
        public IObjectDescriptorBuilder Builder(IWriteableSnapshotStructure targetStructure)
        {
            if (targetStructure == associatedStructure)
            {
                return this;
            }
            else
            {
                return new LazyCopyObjectDescriptor(targetStructure, this);
            }
        }


        /// <inheritdoc />
        public void SetType(TypeValue type)
        {
            this.type = type;
        }

        /// <inheritdoc />
        public void SetObjectValue(ObjectValue objectValue)
        {
            this.objectValue = objectValue;
        }

        /// <inheritdoc />
        public IObjectDescriptor Build(IWriteableSnapshotStructure targetStructure)
        {
            if (targetStructure == associatedStructure)
            {
                return this;
            }
            else
            {
                return new LazyCopyObjectDescriptor(targetStructure, this);
            }
        }
    }
}
