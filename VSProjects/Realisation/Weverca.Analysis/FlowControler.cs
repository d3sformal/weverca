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

        internal FlowResolverBase FlowResolver { get { return _services.FlowResolver; } }

        private readonly AnalysisServices _services;
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
        
        /// <summary>
        /// Create flow controller for given input and output set
        /// </summary>
        internal FlowController(AnalysisServices services,ProgramPoint programPoint, LangElement currentPartial)
        {
            _services = services;
            ProgramPoint = programPoint;
            CurrentPartial = currentPartial;
        }

        #region Call extension handling

        public bool HasCallExtension{get{return CurrentCallExtension != null && !CurrentCallExtension.IsEmpty;}}

        public PartialExtension<LangElement> CurrentCallExtension
        {
            get
            {
                if (CurrentPartial == null)
                {
                    return null;
                }
                return ProgramPoint.GetCallExtension(CurrentPartial);
            }
        }

        public IEnumerable<LangElement> CallBranchingKeys
        {
            get
            {
                if (CurrentCallExtension == null)
                {
                    return new LangElement[0];
                }

                return CurrentCallExtension.BranchingKeys;
            }
        }

        public void AddCallBranch(LangElement branchKey, ProgramPointGraph branchGraph)
        {
            var input = _services.CreateEmptySet();
            ProgramPoint.AddCallBranch(CurrentPartial, branchKey, branchGraph,input);
        }

        public void RemoveCallBranch(LangElement branchKey)
        {
            ProgramPoint.RemoveCallExtension(CurrentPartial, branchKey);
        }

        #endregion

        #region Include extension handling
        public bool HasIncludeExtension { get { return CurrentIncludeExtension != null && !CurrentIncludeExtension.IsEmpty; } }

        public PartialExtension<string> CurrentIncludeExtension
        {
            get
            {
                if (CurrentPartial == null)
                {
                    return null;
                }
                return ProgramPoint.GetIncludeExtension(CurrentPartial);
            }
        }

        public IEnumerable<string> IncludeBranchingKeys
        {
            get
            {
                if (CurrentIncludeExtension == null)
                {
                    return new string[0];
                }

                return CurrentIncludeExtension.BranchingKeys;
            }
        }

        public void AddIncludeBranch(string branchKey, ProgramPointGraph branchGraph)
        {
            var input = _services.CreateEmptySet();
            ProgramPoint.AddIncludeBranch(CurrentPartial, branchKey, branchGraph, input);
        }

        public void RemoveIncludeBranch(string branchKey)
        {
            ProgramPoint.RemoveIncludeExtension(CurrentPartial,branchKey);
        }

        #endregion
    }
}
