using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyCopyStructure
{
    class LazyCopyIndexDefinition : IIndexDefinition, IIndexDefinitionBuilder
    {
        private IMemoryAlias aliases;
        private IObjectValueContainer objects;
        private AssociativeArray arrayValue;
        private IWriteableSnapshotStructure associatedStructure;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyIndexDefinition"/> class.
        /// </summary>
        public LazyCopyIndexDefinition(IWriteableSnapshotStructure associatedStructure)
        {
            this.associatedStructure = associatedStructure;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyIndexDefinition"/> class.
        /// </summary>
        /// <param name="indexDefinition">The index definition.</param>
        public LazyCopyIndexDefinition(IWriteableSnapshotStructure writeableSnapshotStrucure, LazyCopyIndexDefinition indexDefinition)
        {
            this.aliases = indexDefinition.aliases;
            this.objects = indexDefinition.objects;
            this.arrayValue = indexDefinition.arrayValue;
            this.associatedStructure = writeableSnapshotStrucure;
        }

        /// <inheritdoc />
        public IMemoryAlias Aliases
        {
            get { return aliases; }
        }

        /// <inheritdoc />
        public IObjectValueContainer Objects
        {
            get { return objects; }
        }

        /// <inheritdoc />
        public AssociativeArray Array
        {
            get { return arrayValue; }
        }

        /// <inheritdoc />
        public IIndexDefinitionBuilder Builder(IWriteableSnapshotStructure targetStructure)
        {
            if (associatedStructure == targetStructure)
            {
                return this;
            }
            else
            {
                return new LazyCopyIndexDefinition(targetStructure, this);
            }
        }

        /// <inheritdoc />
        public void SetArray(AssociativeArray arrayValue)
        {
            this.arrayValue = arrayValue;
        }

        /// <inheritdoc />
        public void SetObjects(IObjectValueContainer objects)
        {
            this.objects = objects;
        }

        /// <inheritdoc />
        public void SetAliases(IMemoryAlias aliases)
        {
            this.aliases = aliases;
        }

        /// <inheritdoc />
        public IIndexDefinition Build(IWriteableSnapshotStructure targetStructure)
        {
            if (associatedStructure == targetStructure)
            {
                return this;
            }
            else
            {
                return new LazyCopyIndexDefinition(targetStructure, this);
            }
        }
    }
}
