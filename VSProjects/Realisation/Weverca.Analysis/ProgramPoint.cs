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
        public IEnumerable<ProgramPointGraph> InvokedGraphs { get { return _invokedGraphs; } }
        public IEnumerable<ProgramPoint> Children { get { return _children; } }
        public IEnumerable<ProgramPoint> Parents { get { return _parents; } }

        public FlowInputSet InSet { get; private set; }
        public FlowOutputSet OutSet { get; private set; }
                
        public readonly bool IsCondition;
        public readonly bool IsEmpty;
        public readonly BasicBlock OuterBlock;

        private List<ProgramPoint> _children = new List<ProgramPoint>();
        private List<ProgramPoint> _parents = new List<ProgramPoint>();
        private bool _isInitialized;

        private HashSet<ProgramPointGraph> _invokedGraphs = new HashSet<ProgramPointGraph>();

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
        
        internal void RemoveInvokedGraph(ProgramPointGraph programPointGraph)
        {
            _invokedGraphs.Remove(programPointGraph);
        }

        internal void AddInvokedGraph(ProgramPointGraph programPointGraph)
        {
            _invokedGraphs.Add(programPointGraph);
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
