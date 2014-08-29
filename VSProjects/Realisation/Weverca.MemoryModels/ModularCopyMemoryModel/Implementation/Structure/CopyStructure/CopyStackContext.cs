/*
Copyright (c) 2012-2014 Pavel Bastecky.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


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
    class CopyStackContext : IReadonlyStackContext, IWriteableStackContext
    {
        private CopyIndexContainer variables;
        private CopyIndexContainer controllVariables;
        private CopySet<MemoryIndex> temporaryVariables;
        private CopySet<AssociativeArray> arrays;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyStackContext"/> class.
        /// </summary>
        public CopyStackContext()
        {
            this.variables = new CopyIndexContainer();
            this.controllVariables = new CopyIndexContainer();
            this.temporaryVariables = new CopySet<MemoryIndex>();
            this.arrays = new CopySet<AssociativeArray>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyStackContext"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public CopyStackContext(CopyStackContext context)
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
    }
}