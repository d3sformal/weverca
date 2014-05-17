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
    class CopyArrayDescriptor : CopyIndexContainer, IArrayDescriptor, IArrayDescriptorBuilder
    {
        private MemoryIndex parentIndex;
        private AssociativeArray arrayValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyArrayDescriptor"/> class.
        /// </summary>
        public CopyArrayDescriptor()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyArrayDescriptor"/> class.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="arrayValue">The array value.</param>
        public CopyArrayDescriptor(MemoryIndex parentIndex, AssociativeArray arrayValue)
        {
            this.parentIndex = parentIndex;
            this.arrayValue = arrayValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyArrayDescriptor"/> class.
        /// New instance contains copy of given object.
        /// </summary>
        /// <param name="arrayDescriptor">The array descriptor.</param>
        public CopyArrayDescriptor(CopyArrayDescriptor arrayDescriptor)
            : base(arrayDescriptor)
        {
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
        public IArrayDescriptorBuilder Builder()
        {
            return new CopyArrayDescriptor(this);
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
        public IArrayDescriptor Build()
        {
            return new CopyArrayDescriptor(this);
        }
    }
}
