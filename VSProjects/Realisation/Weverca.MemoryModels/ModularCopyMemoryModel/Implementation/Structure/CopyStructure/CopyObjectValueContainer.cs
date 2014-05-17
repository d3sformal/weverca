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
    class CopyObjectValueContainer : CopySet<ObjectValue>, IObjectValueContainer, IObjectValueContainerBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CopyObjectValueContainer"/> class.
        /// </summary>
        public CopyObjectValueContainer()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyObjectValueContainer"/> class.
        /// </summary>
        /// <param name="objectContainer">The object container.</param>
        public CopyObjectValueContainer(CopyObjectValueContainer objectContainer)
            : base(objectContainer)
        {

        }

        /// <inheritdoc />
        public IObjectValueContainerBuilder Builder()
        {
            return new CopyObjectValueContainer(this);
        }

        /// <inheritdoc />
        public new System.Collections.IEnumerator GetEnumerator()
        {
            return ((CopySet<ObjectValue>)this).GetEnumerator();
        }

        /// <inheritdoc />
        public IObjectValueContainer Build()
        {
            return new CopyObjectValueContainer(this);
        }
    }
}
