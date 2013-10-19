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
        private readonly MemoryEntry _entry;

        public readonly LangElement Element;
        public override LangElement Partial { get { return Element; } }


        internal TestMemoryEntryPoint(LangElement element, MemoryEntry entry)
        {
            Element = element;
            Value = new TestSnasphotEntry(entry);
        }

        protected override void flowThrough()
        {
            throw new NotSupportedException("This node is used only as workaround for testing");
        }
    }

    /// <summary>
    /// VariableEntryWrapper
    /// <remarks>This program point is used for testing purposes only</remarks>
    /// </summary>
    public class TestVariablePoint : LValuePoint
    {
        public readonly LangElement Element;
        public override LangElement Partial { get { return Element; } }

        internal TestVariablePoint(LangElement element, VariableIdentifier entry)
        {
            throw new NotImplementedException();
           /* Element = element;
            VariableEntry = entry;*/
        }

        protected override void flowThrough()
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

        protected override void writeMemory(SnapshotBase context, MemoryEntry value)
        {
            throw new NotImplementedException();
        }

        protected override void setAliases(SnapshotBase context, ReadSnapshotEntryBase aliasedEntry)
        {
            throw new NotImplementedException();
        }

        protected override bool isDefined(SnapshotBase context)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<AliasEntry> aliases(SnapshotBase context)
        {
            throw new NotImplementedException();
        }

        protected override MemoryEntry readMemory(SnapshotBase context)
        {
            return memory;
        }

        protected override ReadWriteSnapshotEntryBase readIndex(SnapshotBase context, MemberIdentifier index)
        {
            throw new NotImplementedException();
        }

        protected override ReadWriteSnapshotEntryBase readField(SnapshotBase context, VariableIdentifier field)
        {
            throw new NotImplementedException();
        }

        protected override VariableIdentifier getVariableIdentifier(SnapshotBase context)
        {
            throw new NotImplementedException();
        }
    }
}
