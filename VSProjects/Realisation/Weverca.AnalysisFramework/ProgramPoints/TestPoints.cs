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
    public class TestMemoryEntryPoint : RValuePoint
    {
        public readonly LangElement Element;
        public override LangElement Partial { get { return Element; } }

        internal TestMemoryEntryPoint(LangElement element, MemoryEntry entry)
        {
            Element = element;
            Value = entry;
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
    public class TestVariablePoint : LVariableEntryPoint
    {
        public readonly LangElement Element;
        public override LangElement Partial { get { return Element; } }

        internal TestVariablePoint(LangElement element, VariableEntry entry)
            : base(null)
        {
            Element = element;
            VariableEntry = entry;
        }

        protected override void flowThrough()
        {
            throw new NotSupportedException("This node is used only as workaround for testing");
        }
    }
}
