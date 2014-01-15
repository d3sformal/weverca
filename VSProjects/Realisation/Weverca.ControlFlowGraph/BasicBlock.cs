using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Parsers;

namespace Weverca.ControlFlowGraph
{
    /// <summary>
    /// 
    /// </summary>
    public class BasicBlock
    {
        /// <summary>
        /// The statements of this basic block
        /// </summary>
        public List<LangElement> Statements;

        /// <summary>
        /// The outgoing edges
        /// </summary>
        public List<IBasicBlockEdge> OutgoingEdges;

        /// <summary>
        /// The incomming edges
        /// </summary>
        public List<IBasicBlockEdge> IncommingEdges;

        /// <summary>
        /// The default branch for the direct unconditional connection between basic blocks
        /// </summary>
        public DirectEdge DefaultBranch;
        
        /// <summary>
        /// Hold the references of the basic block which ends with this basic block
        /// </summary>
        public List<TryBasicBlock> EndIngTryBlocks;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicBlock"/> class.
        /// </summary>
        public BasicBlock()
        {
            Statements = new List<LangElement>();
            OutgoingEdges = new List<IBasicBlockEdge>();
            IncommingEdges = new List<IBasicBlockEdge>();
            DefaultBranch = null;
            EndIngTryBlocks = new List<TryBasicBlock>();
        }


        /// <summary>
        /// Adds the element.
        /// </summary>
        /// <param name="element">The element.</param>
        public void AddElement(LangElement element)
        {
            Statements.Add(element);
        }

        /// <summary>
        /// Adds the incomming edge.
        /// </summary>
        /// <param name="edge">The edge.</param>
        public void AddIncommingEdge(IBasicBlockEdge edge)
        {
            IncommingEdges.Add(edge);
        }

        /// <summary>
        /// Adds the outgoing edge.
        /// </summary>
        /// <param name="edge">The edge.</param>
        public void AddOutgoingEdge(IBasicBlockEdge edge)
        {
            OutgoingEdges.Add(edge);
        }

        /// <summary>
        /// Sets the default branch.
        /// </summary>
        /// <param name="edge">The edge.</param>
        public void SetDefaultBranch(DirectEdge edge)
        {
            DefaultBranch = edge;
        }

        /// <summary>
        /// Removes empty bacic block with only one direct output connection
        /// </summary>
        internal void SimplifyGraph()
        {
            HashSet<BasicBlock> processed = new HashSet<BasicBlock>();
            LinkedList<BasicBlock> queue = new LinkedList<BasicBlock>();

            queue.AddLast(this);
            processed.Add(this);

            while (queue.Count > 0)
            {
                BasicBlock accBlock = queue.First.Value;
                queue.RemoveFirst();

                //If actual block is empty with only one output
                if (accBlock.OutgoingEdges.Count == 0
                    && accBlock.Statements.Count == 0
                    && accBlock.IncommingEdges.Count > 0
                    && accBlock.DefaultBranch != null)
                {
                    //For all incoming edges set new location and skip this block
                    BasicBlock newDestination = accBlock.DefaultBranch.To;
                    foreach (IBasicBlockEdge edge in accBlock.IncommingEdges)
                    {
                        edge.ChangeDestination(newDestination);
                    }

                    IncommingEdges.Clear();

                    newDestination.IncommingEdges.Remove(accBlock.DefaultBranch);
                    accBlock.DefaultBranch = null;

                }
                //Or adds next blocks to the queue
                else
                {
                    if (accBlock.OutgoingEdges.Count > 0)
                    {
                        foreach (IBasicBlockEdge edge in accBlock.OutgoingEdges)
                        {
                            if (!processed.Contains(edge.To))
                            {
                                queue.AddLast(edge.To);
                                processed.Add(edge.To);
                            }
                        }
                    }

                    if (accBlock.IncommingEdges.Count > 0)
                    {
                        foreach (IBasicBlockEdge edge in accBlock.IncommingEdges)
                        {
                            if (!processed.Contains(edge.From))
                            {
                                queue.AddLast(edge.From);
                                processed.Add(edge.From);
                            }
                        }
                    }

                    if (accBlock.DefaultBranch != null)
                    {
                        if (!processed.Contains(accBlock.DefaultBranch.To))
                        {
                            queue.AddLast(accBlock.DefaultBranch.To);
                            processed.Add(accBlock.DefaultBranch.To);
                        }
                    }
                }
            }
        }
    }

    public class TryBasicBlock : BasicBlock
    {
        public List<CatchBasicBlock> catchBlocks = new List<CatchBasicBlock>();
        public TryBasicBlock() : base ()
        {
            
        }
    }


    public class CatchBasicBlock : BasicBlock
    {
        public DirectVarUse/*!*/ Variable { private set; get; }
        public GenericQualifiedName ClassName { private set;  get; }
        public CatchBasicBlock(DirectVarUse/*!*/ variable, GenericQualifiedName className)
            : base()
        {
            Variable = variable;
            ClassName = className;
        }
    
    
    }

}
