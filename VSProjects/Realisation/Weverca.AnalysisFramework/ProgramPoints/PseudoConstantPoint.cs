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
    /// Pseudo-constant use (PHP keywords: __LINE__, __FILE__, __DIR__, __FUNCTION__, __METHOD__, __CLASS__, __NAMESPACE__)
    /// </summary>
    public class PseudoConstantPoint : ValuePoint
    {
        private PseudoConstUse _partial;

        /// <inheritdoc />
        public override LangElement Partial { get { return _partial; } }

        internal PseudoConstantPoint(PseudoConstUse partial)
        {
            _partial = partial;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            switch (_partial.Type)
            {
                case PseudoConstUse.Types.Line:
                    Value = OutSet.CreateSnapshotEntry( new MemoryEntry(OutSet.AnyIntegerValue) );
                    return;
                case PseudoConstUse.Types.File:
                case PseudoConstUse.Types.Dir:
                case PseudoConstUse.Types.Function:
                case PseudoConstUse.Types.Method:
                case PseudoConstUse.Types.Class:
                case PseudoConstUse.Types.Namespace:
                    Value = OutSet.CreateSnapshotEntry(new MemoryEntry(OutSet.AnyStringValue));
                    return;
                default:
                    Value = OutSet.CreateSnapshotEntry(new MemoryEntry(OutSet.AnyValue));
                    return;
                    
            }
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitPseudoConstant(this);
        }
    }
}
