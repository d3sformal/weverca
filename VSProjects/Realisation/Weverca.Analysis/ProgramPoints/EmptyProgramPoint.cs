using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

namespace Weverca.Analysis.ProgramPoints
{
    /// <summary>
    /// Represents empty program point (it doesn't change flow)
    /// </summary>
    public class EmptyProgramPoint : ProgramPointBase
    {
        public override LangElement Partial { get { return null; } }

        protected override void flowThrough()
        {
            //no action is needed
        }
    }
}
