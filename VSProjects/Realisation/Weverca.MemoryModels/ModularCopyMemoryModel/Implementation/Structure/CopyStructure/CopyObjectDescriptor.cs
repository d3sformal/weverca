using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.CopyStructure
{
    class CopyObjectDescriptor : CopyIndexContainer, IObjectDescriptor, IObjectDescriptorBuilder
    {
        private TypeValue type;
        private ObjectValue objectValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyObjectDescriptor"/> class.
        /// </summary>
        public CopyObjectDescriptor()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyObjectDescriptor"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="objectValue">The object value.</param>
        public CopyObjectDescriptor(TypeValue type, ObjectValue objectValue)
        {
            this.type = type;
            this.objectValue = objectValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyObjectDescriptor"/> class.
        /// New instance contains copy of given object.
        /// </summary>
        /// <param name="objectDescriptor">The object descriptor.</param>
        public CopyObjectDescriptor(CopyObjectDescriptor objectDescriptor)
            : base(objectDescriptor)
        {
            this.type = objectDescriptor.type;
            this.objectValue = objectDescriptor.objectValue;
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
        public IObjectDescriptorBuilder Builder()
        {
            return new CopyObjectDescriptor(this);
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
        public IObjectDescriptor Build()
        {
            return new CopyObjectDescriptor(this);
        }
    }
}
