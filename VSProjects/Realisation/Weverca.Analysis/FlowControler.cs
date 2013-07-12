using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

using Weverca.Analysis.Memory;
using Weverca.Analysis.Expressions;

namespace Weverca.Analysis
{
    /// <summary>
    /// Controller used for accessing flow sets and dispatching
    /// </summary>
    public class FlowController
    {
        public readonly ProgramPoint ProgramPoint;
        public readonly LangElement CurrentPartial;
        /// <summary>
        /// Input set
        /// </summary>
        public FlowInputSet InSet { get { return ProgramPoint.InSet; } }
        /// <summary>
        /// Output set
        /// </summary>
        public FlowOutputSet OutSet { get { return ProgramPoint.OutSet; } }

        /// <summary>
        /// Get/Set arguments used for call branches
        /// NOTE:
        ///     Default arguments are set by framework (you can override them)
        /// </summary>
        public MemoryEntry[] Arguments { get; set; }
        /// <summary>
        /// Get/Set this object for call branches
        /// NOTE:
        ///     Default called object is set by framework (you can override it)
        /// </summary>
        public MemoryEntry CalledObject { get; set; }

        public PartialExtension CurrentExtension
        {
            get            
            {
                if (CurrentPartial == null)
                {
                    return null;
                }
                return ProgramPoint.GetExtension(CurrentPartial);
            }
        }

        public bool HasCallExtension{get{return CurrentExtension != null && !CurrentExtension.IsEmpty;}}

        /// <summary>
        /// Create flow controller for given input and output set
        /// </summary>
        internal FlowController(ProgramPoint programPoint, LangElement currentPartial)
        {
            ProgramPoint = programPoint;
            CurrentPartial = currentPartial;
        }

        public IEnumerable<LangElement> CallBranchingKeys
        {
            get
            {
                if (CurrentExtension == null)
                {
                    return new LangElement[0];
                }

                return CurrentExtension.BranchingKeys;
            }
        }

        public void AddCallBranch(LangElement branchKey, ProgramPointGraph branchGraph)
        {
            ProgramPoint.AddCallBranch(CurrentPartial, branchKey, branchGraph);
        }

        public void RemoveCallBranch(LangElement branchKey)
        {
            ProgramPoint.RemoveCallExtension(branchKey);
        }
    }
}
