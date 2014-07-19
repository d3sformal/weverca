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
    class LazyCopyStackContext : IReadonlyStackContext, IWriteableStackContext, IGenericCloneable<LazyCopyStackContext>
    {
        private LazyCopyIndexContainer variables;
        private LazyCopyIndexContainer controllVariables;
        private LazyCopySet<MemoryIndex> temporaryVariables;
        private LazyCopySet<AssociativeArray> arrays;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyCopyStackContext"/> class.
        /// </summary>
        public LazyCopyStackContext()
        {
            this.variables = new LazyCopyIndexContainer();
            this.controllVariables = new LazyCopyIndexContainer();
            this.temporaryVariables = new LazyCopySet<MemoryIndex>();
            this.arrays = new LazyCopySet<AssociativeArray>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyCopyStackContext"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public LazyCopyStackContext(LazyCopyStackContext context)
        {
            this.variables = context.variables.Clone();
            this.controllVariables = context.controllVariables.Clone();
            this.temporaryVariables = context.temporaryVariables.Clone();
            this.arrays = context.arrays.Clone();
        }

        /// <inheritdoc />
        public IReadonlyIndexContainer ReadonlyVariables
        {
            get { return variables; }
        }

        /// <inheritdoc />
        public IReadonlyIndexContainer ReadonlyControllVariables
        {
            get { return controllVariables; }
        }

        /// <inheritdoc />
        public IReadonlySet<MemoryIndex> ReadonlyTemporaryVariables
        {
            get { return temporaryVariables; }
        }

        /// <inheritdoc />
        public IReadonlySet<AssociativeArray> ReadonlyArrays
        {
            get { return arrays; }
        }

        /// <inheritdoc />
        public IWriteableIndexContainer WriteableVariables
        {
            get { return variables; }
        }

        /// <inheritdoc />
        public IWriteableIndexContainer WriteableControllVariables
        {
            get { return controllVariables; }
        }

        /// <inheritdoc />
        public IWriteableSet<MemoryIndex> WriteableTemporaryVariables
        {
            get { return temporaryVariables; }
        }

        /// <inheritdoc />
        public IWriteableSet<AssociativeArray> WriteableArrays
        {
            get { return arrays; }
        }

        public LazyCopyStackContext Clone()
        {
            return new LazyCopyStackContext(this);
        }
    }
}
