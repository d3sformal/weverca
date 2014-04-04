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

        /*
		protected override void extendOutput()
		{
			if (FlowParentsCount == 1) 
			{
				_outSet = _inSet;
				//_outSet.StartTransaction();
				return;
			}

			base.extendOutput ();
		}

		/// <summary>
		/// Extends the input.
		/// </summary>
		protected override void extendInput()
		{
			if (FlowParentsCount == 1) 
			{
				_inSet = FlowParents.First().OutSet;
				return;
			}

			base.extendInput ();
		}

		protected override void commitFlow()
		{
			if (FlowParentsCount == 1) 
			{
				enqueueChildren ();
				return;
			}

			base.commitFlow ();
		}
         * */
    }
}
