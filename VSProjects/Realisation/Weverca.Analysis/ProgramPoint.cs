using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;
using Weverca.ControlFlowGraph;

using Weverca.Analysis.Memory;
using Weverca.Analysis.Expressions;

namespace Weverca.Analysis
{
    /// <summary>
    /// Program point computed during fix point algorithm.
    /// </summary>    
    public class ProgramPoint
    {
        #region Private members
        /// <summary>
        /// Children of this program point
        /// </summary>
        private List<ProgramPoint> _children = new List<ProgramPoint>();

        /// <summary>
        /// Parents of this program point
        /// </summary>
        private List<ProgramPoint> _parents = new List<ProgramPoint>();

        /// <summary>
        /// Determine that program point has already been intialized (InSet,OutSet assigned)
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// Call extensions indexed by partials which creates them
        /// </summary>
        private readonly Dictionary<LangElement, PartialExtension<LangElement>> _containedCallExtensions = new Dictionary<LangElement, PartialExtension<LangElement>>();

        /// <summary>
        /// Include extensions indexed by partials which creates them
        /// </summary>
        private readonly Dictionary<LangElement, PartialExtension<string>> _containedIncludeExtensions = new Dictionary<LangElement, PartialExtension<string>>();


        /// <summary>
        /// Represented condition
        /// NOTE:
        ///     * If is null, program point represent statement
        /// </summary>
        private AssumptionCondition _condition;

        /// <summary>
        /// Represented statement in postfix representation
        /// NOTE:
        ///     * If is null, program point represent condition
        /// </summary>
        private Postfix _statement;
        #endregion

        /// <summary>
        /// Enumeration of contained call extensions
        /// </summary>
        public IEnumerable<PartialExtension<LangElement>> ContainedCallExtensions { get { return _containedCallExtensions.Values; } }

        /// <summary>
        /// Enumeration of contained include extensions
        /// </summary>
        public IEnumerable<PartialExtension<string>> ContainedIncludeExtensions { get { return _containedIncludeExtensions.Values; } }

        /// <summary>
        /// Childrens of this program point
        /// </summary>
        public IEnumerable<ProgramPoint> Children { get { return _children; } }

        /// <summary>
        /// Parents of this program point
        /// </summary>
        public IEnumerable<ProgramPoint> Parents { get { return _parents; } }

        /// <summary>
        /// Determine that program point has already been initialized (OutSet,InSet assigned)
        /// </summary>
        public bool IsInitialized { get { return _isInitialized; } }

        /// <summary>
        /// Input set of this program point
        /// </summary>
        public FlowInputSet InSet { get; private set; }

        /// <summary>
        /// Output set of this program point
        /// </summary>
        public FlowOutputSet OutSet { get; private set; }

        /// <summary>
        /// Determine that this program point represents condition
        /// NOTE:
        ///     * If doesnt represent condition, can represent empty or statement
        /// </summary>
        public readonly bool IsCondition;

        /// <summary>
        /// Determine that this program point represents empty program point (Used as start/end program points)
        /// </summary>
        public readonly bool IsEmpty;

        /// <summary>
        /// Basic block where represetned statement/condition is located
        /// </summary>
        public readonly BasicBlock OuterBlock;
        
        /// <summary>
        /// Get assumption condition if this program point IsCondition
        /// </summary>
        public AssumptionCondition Condition
        {
            get
            {
                if (!IsCondition || IsEmpty)
                    throw new NotSupportedException("Program point doesn't have condition");
                return _condition;
            }
        }

        /// <summary>
        /// Get statement converted into Postfix representation
        /// </summary>
        public Postfix Statement
        {
            get
            {
                if (IsCondition || IsEmpty)
                {
                    throw new NotSupportedException("Program point does'nt have statement");
                }
                return _statement;
            }
        }

        #region ProgramPoint graph building methods

        /// <summary>
        /// Create program point representing given condition
        /// </summary>
        /// <param name="condition">Condition represented by created program point</param>
        /// <param name="outerBlock">Basic block containing condition</param>
        internal ProgramPoint(AssumptionCondition condition, BasicBlock outerBlock)
        {
            _condition = condition;
            IsCondition = true;
            OuterBlock = outerBlock;
        }

        /// <summary>
        /// Create program point representing given statement
        /// </summary>
        /// <param name="statement">Statement represented by created program point</param>
        /// <param name="outerBlock">Basic block containing statement</param>
        internal ProgramPoint(LangElement statement, BasicBlock outerBlock)
        {
            _statement = Converter.GetPostfix(statement);
            OuterBlock = outerBlock;
        }

        /// <summary>
        /// Creates empty program point
        /// </summary>
        internal ProgramPoint()
        {
            IsEmpty = true;
        }

        /// <summary>
        /// Add child to this program point
        /// NOTE:
        ///     Parent of child is also set
        /// </summary>
        /// <param name="child">Added child</param>
        internal void AddChild(ProgramPoint child)
        {
            _children.Add(child);
            child._parents.Add(this);
        }

        #endregion

        #region Call extension handling

        /// <summary>
        /// Get call extensions created by given partial
        /// </summary>
        /// <param name="partial">Partial which extensions are searched</param>
        /// <returns>Found extensions</returns>
        internal PartialExtension<LangElement> GetCallExtension(LangElement partial)
        {
            PartialExtension<LangElement> result;
            _containedCallExtensions.TryGetValue(partial, out result);

            return result;
        }

        /// <summary>
        /// Add call branch for given partial
        /// </summary>
        /// <param name="partial">Branch which branch is added</param>
        /// <param name="branchKey">Key of added branchGraph</param>
        /// <param name="branchGraph">Graph of added branch</param>
        /// <param name="branchInput">Input of added branch</param>
        internal void AddCallBranch(LangElement partial, LangElement branchKey, ProgramPointGraph branchGraph,FlowOutputSet branchInput)
        {
            var extension = GetCallExtension(partial);
            if (extension == null)
            {
                extension = new PartialExtension<LangElement>(InSet.Snapshot, OutSet.Snapshot);
                _containedCallExtensions.Add(partial, extension);
            }
            extension.AddBranch(branchKey, branchGraph,branchInput);
            branchGraph.AddContainingCallExtension(extension);
        }

        /// <summary>
        /// Remove call branch of given partial 
        /// </summary>
        /// <param name="partial">Partial which branch is removed</param>
        /// <param name="branchKey">Key of removed branch</param>
        internal void RemoveCallBranch(LangElement partial, LangElement branchKey)
        {
            var extension = GetCallExtension(partial);
            if (extension == null)
            {
                //nothing to remove
                return;
            }

            var branch=extension.GetBranch(branchKey);                        
            if (branch != null)
            {
                extension.RemoveBranch(branchKey);
                branch.RemoveContainingCallExtension(extension);
            }
        }

        #endregion

        #region Include extension handling

        /// <summary>
        /// Get include extensions created by given partial
        /// </summary>
        /// <param name="partial">Partial which extensions are searched</param>
        /// <returns>Found extensions</returns>
        internal PartialExtension<string> GetIncludeExtension(LangElement partial)
        {
            PartialExtension<string> result;
            _containedIncludeExtensions.TryGetValue(partial, out result);

            return result;
        }

        /// <summary>
        /// Add include branch for given partial
        /// </summary>
        /// <param name="partial">Branch which branch is added</param>
        /// <param name="branchKey">Key of added branchGraph</param>
        /// <param name="branchGraph">Graph of added branch</param>
        /// <param name="branchInput">Input of added branch</param>
        internal void AddIncludeBranch(LangElement partial, string branchKey, ProgramPointGraph branchGraph, FlowOutputSet branchInput)
        {
            var extension = GetIncludeExtension(partial);
            if (extension == null)
            {
                extension = new PartialExtension<string>(InSet.Snapshot, OutSet.Snapshot);
                _containedIncludeExtensions.Add(partial, extension);
            }
            extension.AddBranch(branchKey, branchGraph, branchInput);
            branchGraph.AddContainingIncludeExtension(extension);
        }
        
        /// <summary>
        /// Remove call branch of given partial 
        /// </summary>
        /// <param name="partial">Partial which branch is removed</param>
        /// <param name="branchKey">Key of removed branch</param>
        internal void RemoveIncludeBranch(LangElement partial, string branchKey)
        {
            var extension = GetIncludeExtension(partial);
            if (extension == null)
            {
                //nothing to remove
                return;
            }

            var branch = extension.GetBranch(branchKey);
            if (branch != null)
            {
                extension.RemoveBranch(branchKey);
                branch.RemoveContainingIncludeExtension(extension);
            }
        }
        #endregion

        /// <summary>
        /// Initialize program point with given input and output sets
        /// </summary>
        /// <param name="input">Input set of program point</param>
        /// <param name="output">Output set of program point</param>
        internal void Initialize(FlowInputSet input, FlowOutputSet output)
        {
            if (_isInitialized)
            {
                throw new NotSupportedException("Initialization can be run only once");
            }
            _isInitialized = true;
            InSet = input;
            OutSet = output;
        }
        
        /// <summary>
        /// Reset changes reported by output set - is used for Fix point computation
        /// </summary>
        internal void ResetChanges()
        {
            OutSet.ResetChanges();
        }
    }
}
