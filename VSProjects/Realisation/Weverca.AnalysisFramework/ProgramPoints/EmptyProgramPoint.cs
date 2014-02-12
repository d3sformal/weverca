using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

namespace Weverca.AnalysisFramework.ProgramPoints
{
    /// <summary>
    /// Represents empty program point (it doesn't change flow)
    /// </summary>
    public class EmptyProgramPoint : ProgramPointBase
    {
        /// <inheritdoc />
        public override LangElement Partial { get { return null; } }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            //no action is needed
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitEmpty(this);
        }
    }
}
