using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework.ProgramPoints
{
    /// <summary>
    /// Memory entry wrapper
    /// <remarks>This program point is used for testing purposes only</remarks>
    /// </summary>
    public class TestMemoryEntryPoint : ValuePoint
    {
        /// <summary>
        /// Element represented by current point
        /// </summary>
        public readonly LangElement Element;

        /// <inheritdoc />
        public override LangElement Partial { get { return Element; } }
        
        internal TestMemoryEntryPoint(LangElement element, MemoryEntry entry)
        {
            Element = element;
            Value = new TestSnasphotEntry(entry);
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            throw new NotSupportedException("This node is used only as workaround for testing");
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            throw new NotSupportedException("This node is used only as workaround for testing");
        }
    }
     

    /// <summary>
    /// This is only workaround class for back compatibilty with flow resolver tests 
    /// </summary>
    internal class TestSnasphotEntry : ReadWriteSnapshotEntryBase
    {
        private MemoryEntry memory;

        internal TestSnasphotEntry(MemoryEntry entry)
        {
            memory = entry;
        }

        /// <inheritdoc />
        protected override void writeMemory(SnapshotBase context, MemoryEntry value, bool forceStrongWrite)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override void setAliases(SnapshotBase context, ReadSnapshotEntryBase aliasedEntry)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override bool isDefined(SnapshotBase context)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override IEnumerable<AliasEntry> aliases(SnapshotBase context)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override MemoryEntry readMemory(SnapshotBase context)
        {
            return memory;
        }

        /// <inheritdoc />
        protected override ReadWriteSnapshotEntryBase readIndex(SnapshotBase context, MemberIdentifier index)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override ReadWriteSnapshotEntryBase readField(SnapshotBase context, VariableIdentifier field)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override VariableIdentifier getVariableIdentifier(SnapshotBase context)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override void writeMemoryWithoutCopy(SnapshotBase context, MemoryEntry value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override IEnumerable<FunctionValue> resolveMethod(SnapshotBase context, PHP.Core.QualifiedName methodName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override IEnumerable<VariableIdentifier> iterateFields(SnapshotBase context)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override IEnumerable<MemberIdentifier> iterateIndexes(SnapshotBase context)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override IEnumerable<TypeValue> resolveType(SnapshotBase context)
        {
            throw new NotImplementedException();
        }
    }
}
