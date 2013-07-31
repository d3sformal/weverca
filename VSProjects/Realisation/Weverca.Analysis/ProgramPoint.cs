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
    /// <typeparam name="FlowInfo"></typeparam>
    public class ProgramPoint
    {
        public IEnumerable<PartialExtension<LangElement>> ContainedCallExtensions { get { return _containedCallExtensions.Values; } }
        public IEnumerable<PartialExtension<string>> ContainedIncludeExtensions { get { return _containedIncludeExtensions.Values; } }

        public IEnumerable<ProgramPoint> Children { get { return _children; } }
        public IEnumerable<ProgramPoint> Parents { get { return _parents; } }
        public bool IsInitialized { get { return _isInitialized; } }

        public FlowInputSet InSet { get; private set; }
        public FlowOutputSet OutSet { get; private set; }

        public readonly bool IsCondition;
        public readonly bool IsEmpty;
        public readonly BasicBlock OuterBlock;

        private List<ProgramPoint> _children = new List<ProgramPoint>();
        private List<ProgramPoint> _parents = new List<ProgramPoint>();
        private bool _isInitialized;

        /// <summary>
        /// Extensions indexed by partials which creates them
        /// </summary>
        private readonly Dictionary<LangElement, PartialExtension<LangElement>> _containedCallExtensions = new Dictionary<LangElement, PartialExtension<LangElement>>();
        private readonly Dictionary<LangElement, PartialExtension<string>> _containedIncludeExtensions = new Dictionary<LangElement, PartialExtension<string>>();

        AssumptionCondition _condition;


        /// <summary>
        /// Represented statement in postfix representation
        /// </summary>
        Postfix _statement;


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
        internal ProgramPoint(AssumptionCondition condition, BasicBlock outerBlock)
        {
            _condition = condition;
            IsCondition = true;
            OuterBlock = outerBlock;
        }

        internal ProgramPoint(LangElement statement, BasicBlock outerBlock)
        {
            _statement = Converter.GetPostfix(statement);
            OuterBlock = outerBlock;
        }

        internal ProgramPoint()
        {
            IsEmpty = true;
        }

        internal void AddChild(ProgramPoint child)
        {
            _children.Add(child);
            child._parents.Add(this);
        }

        #endregion


        #region Call extension handling
        internal PartialExtension<LangElement> GetCallExtension(LangElement partial)
        {
            PartialExtension<LangElement> result;
            _containedCallExtensions.TryGetValue(partial, out result);

            return result;
        }

        internal void AddCallBranch(LangElement partial, LangElement branchKey, ProgramPointGraph branchGraph,FlowOutputSet branchInput)
        {
            var extension = GetCallExtension(partial);
            if (extension == null)
            {
                extension = new PartialExtension<LangElement>(InSet.Snapshot, OutSet.Snapshot);
                _containedCallExtensions.Add(partial, extension);
            }
            extension.AddBranch(branchKey, branchGraph,branchInput);
            branchGraph.AddCallExtension(extension);
        }

        internal void RemoveCallExtension(LangElement partial, LangElement branchKey)
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
                branch.RemoveCallExtension(extension);
            }
        }
        #endregion

        #region Call extension handling
        internal PartialExtension<string> GetIncludeExtension(LangElement partial)
        {
            PartialExtension<string> result;
            _containedIncludeExtensions.TryGetValue(partial, out result);

            return result;
        }

        internal void AddIncludeBranch(LangElement partial, string branchKey, ProgramPointGraph branchGraph, FlowOutputSet branchInput)
        {
            var extension = GetIncludeExtension(partial);
            if (extension == null)
            {
                extension = new PartialExtension<string>(InSet.Snapshot, OutSet.Snapshot);
                _containedIncludeExtensions.Add(partial, extension);
            }
            extension.AddBranch(branchKey, branchGraph, branchInput);
            branchGraph.AddIncludeExtension(extension);
        }

        internal void RemoveIncludeExtension(LangElement partial, string branchKey)
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
                branch.RemoveIncludeExtension(extension);
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
