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
        /// <summary>
        /// Available services obtained from analysis
        /// </summary>
        private readonly AnalysisServices _services;

        /// <summary>
        /// Flow resolver from analysis services
        /// </summary>
        internal FlowResolverBase FlowResolver { get { return _services.FlowResolver; } }        

        /// <summary>
        /// Currently analyzed program point
        /// </summary>
        public readonly ProgramPoint ProgramPoint;

        /// <summary>
        /// Currently analyzed partial (elementary part of analyzed statement)
        /// </summary>
        public readonly LangElement CurrentPartial;
      
        /// <summary>
        /// Input set of program analysis flow
        /// </summary>
        public FlowInputSet InSet { get { return ProgramPoint.InSet; } }

        /// <summary>
        /// Output set of program analysis flow
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

        /// <summary>
        /// Determine that current partial has any call extension in program point graph
        /// </summary>
        public bool HasCallExtension{get{return CurrentCallExtension != null && !CurrentCallExtension.IsEmpty;}}

        /// <summary>
        /// Get call extension for current partial
        /// </summary>
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

        /// <summary>
        /// Get keys for current call extensions
        /// </summary>
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

        /// <summary>
        /// Add call branch into current partial extension according to given branchKey
        /// </summary>
        /// <param name="branchKey">Key of call branch</param>
        /// <param name="branchGraph">Graph of call branch</param>
        public void AddCallBranch(LangElement branchKey, ProgramPointGraph branchGraph)
        {
            var input = _services.CreateEmptySet();
            ProgramPoint.AddCallBranch(CurrentPartial, branchKey, branchGraph,input);
        }

        /// <summary>
        /// Remove call branch from current partial extension indexed by given branchKey
        /// </summary>
        /// <param name="branchKey">Key of removed call branch</param>
        public void RemoveCallBranch(LangElement branchKey)
        {
            ProgramPoint.RemoveCallBranch(CurrentPartial, branchKey);
        }

        #endregion

        #region Include extension handling

        /// <summary>
        /// Determine that current partial has any include extension in program point graph
        /// </summary>
        public bool HasIncludeExtension { get { return CurrentIncludeExtension != null && !CurrentIncludeExtension.IsEmpty; } }

        /// <summary>
        /// Get include extension for current partial
        /// </summary>
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

        /// <summary>
        /// Get keys for current include extensions
        /// </summary>
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

        /// <summary>
        /// Add include branch into current partial extension according to given branchKey
        /// </summary>
        /// <param name="branchKey">Key of include branch</param>
        /// <param name="branchGraph">Graph of include branch</param>
        public void AddIncludeBranch(string branchKey, ProgramPointGraph branchGraph)
        {
            var input = _services.CreateEmptySet();
            ProgramPoint.AddIncludeBranch(CurrentPartial, branchKey, branchGraph, input);
        }

        /// <summary>
        /// Remove include branch from current partial extension indexed by given branchKey
        /// </summary>
        /// <param name="branchKey">Key of removed include branch</param>
        public void RemoveIncludeBranch(string branchKey)
        {
            ProgramPoint.RemoveIncludeBranch(CurrentPartial,branchKey);
        }

        #endregion
    }
}
