using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;
using Weverca.ControlFlowGraph;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis
{
    /// <summary>
    /// Program point computed during fix point algorithm.
    /// </summary>
    /// <typeparam name="FlowInfo"></typeparam>
    public class ProgramPoint
    {
        /// <summary>
        /// Snapshot provided by this program point
        /// </summary>
        internal AbstractSnapshot Snapshot { get; private set; }
        
        public IEnumerable<ProgramPoint> Children { get { return _children; } }
        public IEnumerable<ProgramPoint> Parents { get { return _parents; } }
        
        private List<ProgramPoint> _children = new List<ProgramPoint>();
        private List<ProgramPoint> _parents = new List<ProgramPoint>();

        AssumptionCondition _condition;
        //TODO convert into POSTFIX representation
        LangElement _statement;

        public readonly bool IsCondition;
        public readonly bool IsEmpty;
        public readonly BasicBlock OuterBlock;

        public AssumptionCondition Condition
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

        internal ProgramPoint(AssumptionCondition condition, BasicBlock outerBlock)
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

        internal void AddChild(ProgramPoint child)
        {
            _children.Add(child);
            child._parents.Add(this);
        }



        public FlowInputSet InSet { get; set; }

        public FlowOutputSet OutSet { get; set; }
    }
}
