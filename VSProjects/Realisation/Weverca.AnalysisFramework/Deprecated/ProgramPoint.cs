using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;
using Weverca.ControlFlowGraph;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Program point computed during fix point algorithm.
    /// </summary>
    /// <typeparam name="FlowInfo"></typeparam>
    public class ProgramPoint<FlowInfo>
    {
        public FlowInputSet<FlowInfo> InSet { get; internal set; }
        public FlowOutputSet<FlowInfo> OutSet { get; private set; }
        public FlowOutputSet<FlowInfo> OutSetUpdate { get { return _outSetUpdate; } }

        public IEnumerable<ProgramPoint<FlowInfo>> Children { get { return _children; } }
        public IEnumerable<ProgramPoint<FlowInfo>> Parents { get { return _parents; } }



        private List<ProgramPoint<FlowInfo>> _children = new List<ProgramPoint<FlowInfo>>();
        private List<ProgramPoint<FlowInfo>> _parents = new List<ProgramPoint<FlowInfo>>();

        AssumptionCondition_deprecated _condition;
        LangElement _statement;

        public readonly bool IsCondition;
        public readonly bool IsEmpty;
        public readonly BasicBlock OuterBlock;

        public AssumptionCondition_deprecated Condition
        {
            get
            {
                if (!IsCondition || IsEmpty)
                    throw new NotSupportedException("Program point doesn't have condition");
                return _condition;
            }
        }

        public LangElement Statement
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

        internal ProgramPoint(AssumptionCondition_deprecated condition, BasicBlock outerBlock)
        {
            _condition = condition;
            IsCondition = true;
            OuterBlock = outerBlock;
        }

        internal ProgramPoint(LangElement statement, BasicBlock outerBlock)
        {
            _statement = statement;
            OuterBlock = outerBlock;
        }

        internal ProgramPoint()
        {
            IsEmpty = true;
        }

        /// <summary>
        /// Determine that some updates has been requested.
        /// </summary>
        internal bool HasUpdate { get; private set; }

        /// <summary>
        /// Requested update for output set.
        /// </summary>
        private FlowOutputSet<FlowInfo> _outSetUpdate;

        /// <summary>
        /// Request update on output set.
        /// </summary>
        /// <param name="outSet"></param>
        internal void UpdateOutSet(FlowOutputSet<FlowInfo> outSet)
        {
            HasUpdate = true;
            _outSetUpdate = outSet;
        }

        /// <summary>
        /// Commit updates on program point. 
        /// </summary>
        /// <returns>True if any changes has been changed, false otherwise.</returns>
        internal bool CommitUpdate()
        {
            if (!HasUpdate || _outSetUpdate.Equals(OutSet))
            {
                HasUpdate = false;
                return false;
            }

            HasUpdate = false;
            OutSet = _outSetUpdate;
            _outSetUpdate = null;
            return true;
        }

        internal void AddChild(ProgramPoint<FlowInfo> child)
        {
            _children.Add(child);
            child._parents.Add(this);
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            foreach (var parent in Parents)
            {
                b.Append("parent(").Append(parent.InSet).Append("->").Append(parent.OutSet).AppendLine(")");
            }

            foreach (var child in Children)
            {
                b.Append("child(").Append(child.InSet).Append("->").Append(child.OutSet).AppendLine(")");
            }
            return b.ToString();
        }
    }
}
