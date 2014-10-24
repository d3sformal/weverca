/*
Copyright (c) 2012-2014 Marcel Kikta and David Hauzar.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Parsers;
using PHP.Core.Reflection;
using Weverca.Parsers;
using System.IO;

/* Požadavky na phalanger:
 * 
 * 30.3.2013 - Pavel
 *  Nemožnost přístupu k názvu labelu pro implementaci návěstí
 *  Soubor JumpStmt.cs řádek 324 vlastnost name
 *  Změna modifikátoru internal na public
 * 
 * 
 * */

namespace Weverca.ControlFlowGraph
{
    /// <summary>
    /// Linked stack is stack using linked list and not resizeable field. It is creating only for performance purpose.
    /// </summary>
    /// <typeparam name="T">Anything</typeparam>
    class LinkedStack<T> : LinkedList<T>
    {
        /// <summary>
        /// Inserts an object at the top of the Stack.
        /// </summary>
        /// <param name="t">Object</param>
        public void Push(T t)
        {
            AddLast(t);
        }
        /// <summary>
        /// Returns the object at the top of the Stack without removing it.
        /// </summary>
        /// <returns>Returns the object at the top of the Stack.</returns>
        public T Peek()
        {
            return this.Last();
        }
        /// <summary>
        /// Removes and returns the object at the top of the Stack.
        /// </summary>
        /// <returns>Returns removed object.</returns>
        public T Pop()
        {
            T result = this.Last();
            RemoveLast();
            return result;
        }
    }

    /// <summary>
    /// Stores blocks for jumping from break and continue statements.
    /// </summary>
    class LoopData
    {
        /// <summary>
        /// Stores target for jump from continue statement.
        /// </summary>
        public BasicBlock ContinueTarget { get; private set; }

        /// <summary>
        /// Stores target for jump from break statement.
        /// </summary>
        public BasicBlock BreakTarget { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoopData" /> class.
        /// </summary>
        /// <param name="ContinueTarget">Basic block, which is target for continue statement</param>
        /// <param name="BreakTarget">Basic block, which is target for break statement</param>
        public LoopData(BasicBlock ContinueTarget, BasicBlock BreakTarget)
        {
            this.ContinueTarget = ContinueTarget;
            this.BreakTarget = BreakTarget;
        }
    }


    /// <summary>
    /// Saves reference to the target basic block of the known label 
    /// or queue of the waiting goto blocks to the unknown one.
    /// </summary>
    class LabelData
    {
        readonly LinkedList<BasicBlock> GotoQueue = new LinkedList<BasicBlock>();
        BasicBlock labelBlock = null;

        public bool HasAssociatedLabel { get { return labelBlock != null; } }

        public Position Position { get; private set; }

        public LabelData(Position position)
        {
            Position = position;
        }


        /// <summary>
        /// Saves target block of this label and process all blocks in GOTO queue.
        /// </summary>
        /// <param name="labelBlock">>The label block.</param>
        /// <param name="position">Label position</param>
        public void AsociateLabel(BasicBlock labelBlock, Position position)
        {
            if (HasAssociatedLabel)
            {
                throw new ControlFlowException(ControlFlowExceptionCause.DUPLICATED_LABEL, position);
            }
            else
            {
                this.labelBlock = labelBlock;

                foreach (BasicBlock gotoBlock in GotoQueue)
                {
                    _asociateGoto(gotoBlock);
                }

                GotoQueue.Clear();
            }
        }

        /// <summary>
        /// Connects goto basic block with the basic block of the label 
        /// or inserts it to the queue if there is no associated block of the label.
        /// </summary>
        /// <param name="gotoBlock">The goto block.</param>
        public void AsociateGoto(BasicBlock gotoBlock)
        {
            if (labelBlock != null)
            {
                _asociateGoto(gotoBlock);
            }
            else
            {
                GotoQueue.AddLast(gotoBlock);
            }
        }

        /// <summary>
        /// Connects goto basic block with the basic block of the label.
        /// </summary>
        /// <param name="gotoBlock">The goto block.</param>
        void _asociateGoto(BasicBlock gotoBlock)
        {
            PHP.Core.Debug.Assert(labelBlock != null);

            BasicBlockEdge.ConnectDirectEdge(gotoBlock, labelBlock);
        }
    }

    /// <summary>
    /// improves the basic functionality of Dictionary&lt;string, LabelData&gt; by GetOrCreate method.
    /// </summary>
    class LabelDataDictionary : Dictionary<string, LabelData>
    {
        /// <summary>
        /// Gets or creates label data in the label collection.
        /// </summary>
        /// <param name="key">The name of the label.</param>
        /// <param name="position">label position</param>
        /// <returns>new Instance of LabelData</returns>
        public LabelData GetOrCreateLabelData(VariableName key, Position position)
        {
            LabelData data;
            if (!TryGetValue(key.Value, out data))
            {
                data = new LabelData(position);
                Add(key.Value, data);
            }

            return data;
        }
    }

    /// <summary>
    /// AST visitor, which contructs controlflow graph.
    /// </summary>
    class CFGVisitor : TreeVisitor
    {

        #region fields

        /// <summary>
        /// Resulting controlgflow graph.
        /// </summary>
        ControlFlowGraph graph;

        /// <summary>
        /// Stores current basic block, where the visitor insert statements.
        /// </summary>
        BasicBlock currentBasicBlock;

        /// <summary>
        /// Stack of loops, for purposes of breaking cycles and switch
        /// </summary>
        LinkedStack<LoopData> loopData = new LinkedStack<LoopData>();

        /// <summary>
        /// Improved dictonary storing labels in context.
        /// </summary>
        private LabelDataDictionary labelDictionary = new LabelDataDictionary();

        /// <summary>
        /// Stores the last blocks of defined functions.
        /// </summary>
        LinkedStack<BasicBlock> functionSinkStack = new LinkedStack<BasicBlock>();

        #endregion fields

        /// <summary>
        /// Creates instance of CFGVisitor.
        /// </summary>
        /// <param name="graph"></param>
        public CFGVisitor(ControlFlowGraph graph)
        {
            this.graph = graph;
            currentBasicBlock = new BasicBlock();
            graph.start = currentBasicBlock;
            functionSinkStack.Push(new BasicBlock());
            //throwBlocks.Push(new List<BasicBlock>());
        }

        public void CheckLabels()
        {
            foreach (var labelData in labelDictionary)
            {
                if (!labelData.Value.HasAssociatedLabel)
                {
                    throw new ControlFlowException(ControlFlowExceptionCause.MISSING_LABEL, labelData.Value.Position);
                }
            }
        }

        /// <summary>
        /// Visits LangElement and apends the element to controlflow graph.
        /// </summary>
        /// <param name="element">LangElement</param>
        public override void VisitElement(LangElement element)
        {
            currentBasicBlock.AddElement(element);
        }

        /// <summary>
        /// Visits the Globalcode element and solves the uncatch exceptions in globalcode.
        /// </summary>
        /// <param name="x">Globalcode</param>
        public override void VisitGlobalCode(GlobalCode x)
        {
            foreach (Statement statement in x.Statements)
            {
                statement.VisitMe(this);
            }
            var peek = functionSinkStack.Peek();
            BasicBlockEdge.ConnectDirectEdge(currentBasicBlock, functionSinkStack.Peek());
            /*   foreach (var block in throwBlocks.ElementAt(0)) {
                   block.Statements.RemoveLast();
                   DirectEdge.MakeNewAndConnect(block, functionSinkStack.Peek());
               }*/
        }

        /// <summary>
        /// Visits GlobalStmt and apends the element to controlflow graph.
        /// </summary>
        /// <param name="x">GlobalStmt</param>
        public override void VisitGlobalStmt(GlobalStmt x)
        {
            currentBasicBlock.AddElement(x);
        }

        /// <summary>
        /// Visits BlockStmt and apends the element to controlflow graph.
        /// </summary>
        /// <param name="x">BlockStmt</param>
        public override void VisitBlockStmt(BlockStmt x)
        {
            VisitStatementList(x.Statements);

        }

        #region Labels and jumps

        /// <summary>
        /// Visit LabelStmt, stores the label in dictionary and creates new basic block.
        /// </summary>
        /// <param name="x">LabelStmt</param>
        public override void VisitLabelStmt(LabelStmt x)
        {
            BasicBlock labelBlock = new BasicBlock();

            labelDictionary.GetOrCreateLabelData(x.Name, x.Position)
                .AsociateLabel(labelBlock, x.Position);

            BasicBlockEdge.ConnectDirectEdge(currentBasicBlock, labelBlock);
            currentBasicBlock = labelBlock;

            //Next line could be used for label visualization, label statement shouldnt be in resulting cgf
            //labelBlock.AddElement(x);
        }

        /// <summary>
        /// Visit GotoStmt, and creates the jump to defined label.
        /// </summary>
        /// <param name="x">GotoStmt</param>
        public override void VisitGotoStmt(GotoStmt x)
        {
            labelDictionary.GetOrCreateLabelData(x.LabelName, x.Position)
                .AsociateGoto(currentBasicBlock);

            //Next line could be used for label visualization, label statement shouldnt be in resulting cgf
            //currentBasicBlock.AddElement(x);

            //THIS COULD BE AN UNREACHABLE BLOCK
            currentBasicBlock = new BasicBlock();
        }

        #endregion

        #region conditions

        /// <summary>
        /// Visits IfStmt and constructs if branch basic block.
        /// 
        /// Does not decompose the condition expression using logical operations (&&, ||, and xor).
        /// Note that this is no longer supported - analyzer now expects that the expression is decomposed
        /// on the level of CFG and it does not deal with logical operations explicitly.
        /// See <see cref="VisitIfStmt"/>.
        /// </summary>
        /// <param name="x">IfStmt</param>
        public void VisitIfStmtOld(IfStmt x)
        {
            //Merge destination for if and else branch
            BasicBlock bottomBox = new BasicBlock();

            currentBasicBlock.CreateWorklistSegment(bottomBox);

            foreach (var condition in x.Conditions)
            {
                if (condition.Condition != null)
                {
                    //IF or ELSEIF branch
                    currentBasicBlock = constructIfBranch(bottomBox, condition);
                }
                else
                {
                    //ELSE branch
                    condition.Statement.VisitMe(this);
                }
            }

            //Connect else branch to bottomBox
            //Must be here becouse in the construc phase we dont know whether the else block would split in the future
            BasicBlockEdge.ConnectDirectEdge(currentBasicBlock, bottomBox);
            currentBasicBlock = bottomBox;
        }

        /// <summary>
        /// Visits IfStmt and constructs if branch basic block.
        /// 
        /// Decomposes the condition expression using logical operations with respect to shortcircuit evaluation.
        /// </summary>
        /// <param name="x">IfStmt</param>
        //public override void VisitIfStmt(IfStmt x)
        public override void VisitIfStmt(IfStmt x)
        {
            //Merge destination for if and else branch
            BasicBlock bottomBox = new BasicBlock();

            currentBasicBlock.CreateWorklistSegment(bottomBox);

            foreach (var cond in x.Conditions)
            {
                if (cond.Condition != null)
                {
                    //IF or ELSEIF branch (then branch)
                    var thenBranchBlock = new BasicBlock();
                    var elseBranchBlock = new BasicBlock();

                    // Decompose the condition
                    BasicBlockEdge.ConnectConditionalBranching(cond.Condition, currentBasicBlock, thenBranchBlock, elseBranchBlock);

                    // Create CFG for then branch
                    currentBasicBlock = thenBranchBlock;
                    cond.Statement.VisitMe(this);

                    // Connect the end of then branch to the bottom box
                    BasicBlockEdge.ConnectDirectEdge(currentBasicBlock, bottomBox);
                    
                    currentBasicBlock = elseBranchBlock;
                }
                else
                {
                    //ELSE branch
                    cond.Statement.VisitMe(this);
                }
            }

            //Connect then end of else branch to bottomBox
            //Must be here becouse in the construc phase we dont know whether the else block would split in the future
            BasicBlockEdge.ConnectDirectEdge(currentBasicBlock, bottomBox);
            currentBasicBlock = bottomBox;
        }

        /// <summary>
        /// Constructs if branch basic block.
        /// </summary>
        /// <param name="bottomBox">Merge destination for if and else branch.</param>
        /// <param name="condition">The condition of the if branch.</param>
        /// <returns>Empty basic block for the else branch</returns>
        private BasicBlock constructIfBranch(BasicBlock bottomBox, ConditionalStmt condition)
        {
            BasicBlock thenBranchBlock = new BasicBlock();
            BasicBlock elseBranchBlock = new BasicBlock();
            BasicBlockEdge.ConnectConditionalBranching(condition.Condition, currentBasicBlock, thenBranchBlock, elseBranchBlock, false);


            currentBasicBlock = thenBranchBlock;
            condition.Statement.VisitMe(this);
            BasicBlockEdge.ConnectDirectEdge(currentBasicBlock, bottomBox);

            return elseBranchBlock;
        }

        #endregion

        #region declarations

        /// <summary>
        /// Visit declaration of namespace.
        /// </summary>
        /// <param name="x">NamespaceDecl</param>
        public override void VisitNamespaceDecl(NamespaceDecl x)
        {
            if (x.Statements != null)
            {
                foreach (Statement s in x.Statements)
                {
                    s.VisitMe(this);
                }
            }
        }

        /// <summary>
        /// Visits TypeDecl and adds it in the controlflow graph. Analysis will create the controlflow graph of methods in the type.
        /// </summary>
        /// <param name="x">TypeDecl</param>
        public override void VisitTypeDecl(TypeDecl x)
        {
            currentBasicBlock.AddElement(x);
        }

        /// <summary>
        /// Visits FunctionDecl and adds it in the controlflow graph. Analysis will create the controlflow of this method.
        /// </summary>
        /// <param name="x">FunctionDecl</param>
        public override void VisitFunctionDecl(FunctionDecl x)
        {
            currentBasicBlock.AddElement(x);
        }

        /// <summary>
        /// Visits MethodDecl. This method will be not called during constructuion cfg. Analysis creates cfg of method via MakeFunctionCFG &lt;T&gt; method.
        /// </summary>
        /// <param name="x">MethodDecl</param>
        public override void VisitMethodDecl(MethodDecl x)
        {
            //BasicBlock functionBasicBlock = MakeFunctionCFG(x, x.Body);
        }

        /// <summary>
        /// Makes the control flow graph for the function or method declaration.
        /// </summary>
        /// <typeparam name="T">Type of function declaration container</typeparam>
        /// <param name="functionDeclaration">The function declaration.</param>
        /// <param name="functionBody">The function body.</param>
        /// <returns>The first basic block of the function's CFG.</returns>
        public BasicBlock MakeFunctionCFG<T>(T functionDeclaration, List<Statement> functionBody) where T : LangElement
        {

            //Store actual basic block
            BasicBlock current = currentBasicBlock;
            BasicBlock functionBasicBlock = new BasicBlock();
            currentBasicBlock = functionBasicBlock;

            //Store actual Label data - function has its own label namespace 

            labelDictionary = new LabelDataDictionary();

            //Add function sink to the stack for resolving returns
            BasicBlock functionSink = new BasicBlock();
            functionSinkStack.Push(functionSink);

            VisitStatementList(functionBody);

            /* foreach (var block in throwBlocks.ElementAt(0))
             {
                 block.Statements.RemoveLast();
                 DirectEdge.MakeNewAndConnect(block, functionSink);
             }*/


            //Connects return destination
            functionSinkStack.Pop();
            BasicBlockEdge.ConnectDirectEdge(currentBasicBlock, functionSink);

            //Loads previous labels
            currentBasicBlock = current;

            return functionBasicBlock;
        }

        #endregion

        #region cycles

        /// <summary>
        /// Visits foreach statement and creates the foreach contruct in cfg. 
        /// </summary>
        /// <param name="x">ForeachStmt</param>
        public override void VisitForeachStmt(ForeachStmt x)
        {
            BasicBlock foreachHead = new BasicBlock();
            BasicBlock foreachBody = new BasicBlock();
            BasicBlock foreachSink = new BasicBlock();

            foreachHead.CreateWorklistSegment(foreachSink);

            //Input edge to the foreach statement
            BasicBlockEdge.ConnectDirectEdge(currentBasicBlock, foreachHead);
            foreachHead.AddElement(x);

            //Conditional edge to the foreach body
            BasicBlockEdge.ConnectForeachEdge(foreachHead, foreachBody);

            //Visits foreach body
            loopData.Push(new LoopData(foreachHead, foreachSink));
            currentBasicBlock = foreachBody;
            x.Body.VisitMe(this);
            loopData.Pop();

            //Connect end of foreach with foreach head
            BasicBlockEdge.ConnectDirectEdge(currentBasicBlock, foreachHead);

            //Output edge to the sink
            BasicBlockEdge.ConnectDirectEdge(foreachHead, foreachSink);
            currentBasicBlock = foreachSink;
        }

        /// <summary>
        /// Visits ForStmt and builds controlflow graf for for cycle.
        /// </summary>
        /// <param name="x">ForStmt</param>
        public override void VisitForStmt(ForStmt x)
        {
            BasicBlock forTest = new BasicBlock();
            BasicBlock forBody = new BasicBlock();
            BasicBlock forEnd = new BasicBlock();
            BasicBlock forIncrement = new BasicBlock();

            currentBasicBlock.CreateWorklistSegment(forEnd);

            // Create CFG for initialization of the for cycle
            VisitExpressionList(x.InitExList);

            //Adds initial connection from initialization to the test block
            BasicBlockEdge.ConnectDirectEdge(currentBasicBlock, forTest);

            // Connects test block with body of the for cycle and end of the for cycle
            var forCondition = (x.CondExList.Count > 0) ? constructSimpleCondition(x.CondExList) : new BoolLiteral(Position.Invalid, true);
            var currBl = currentBasicBlock;
            BasicBlockEdge.ConnectConditionalBranching(forCondition, forTest, forBody, forEnd);

            // Create CFG for Loop body
            loopData.Push(new LoopData(forIncrement, forEnd));
            currentBasicBlock = forBody;
            x.Body.VisitMe(this);
            loopData.Pop();

            // Connect end of the loop body to the increment
            BasicBlockEdge.ConnectDirectEdge(currentBasicBlock, forIncrement);

            // Generate CFG for the loop increment
            currentBasicBlock = forIncrement;
            VisitExpressionList(x.ActionExList);

            // Connect for increment to the test block
            BasicBlockEdge.ConnectDirectEdge(forIncrement, forTest);

            currentBasicBlock = forEnd;
        }

        /// <summary>
        /// Visits WhileStmt(do while and while) and builds controlflow graf for while cycle.
        /// </summary>
        /// <param name="x">WhileStmt</param>
        public override void VisitWhileStmt(WhileStmt x)
        {
            BasicBlock aboveLoop = currentBasicBlock;
            BasicBlock startLoop = new BasicBlock();
            BasicBlock underLoop = new BasicBlock();

            aboveLoop.CreateWorklistSegment(underLoop);
            
            // Connect above loop to start of the loop and under loop
            if (x.LoopType == WhileStmt.Type.While)
            {
                // while => above loop is connected conditionally to start of the loop and under loop
                BasicBlockEdge.ConnectConditionalBranching(DeepCopyAstExpressionCopyVisitor(x.CondExpr), currentBasicBlock, startLoop, underLoop);
            }
            else
            {
                // do ... while => above loop is connected just to the start of the loop
                BasicBlockEdge.ConnectDirectEdge(aboveLoop, startLoop);
            }

            // Generate CFG for loop body
            loopData.Push(new LoopData(startLoop, underLoop));
            currentBasicBlock = startLoop;
            x.Body.VisitMe(this);
            loopData.Pop();

            // Connect the end of loop body to start of the loop and under the loop
            BasicBlockEdge.ConnectConditionalBranching(x.CondExpr, currentBasicBlock, startLoop, underLoop);
            
            currentBasicBlock = underLoop;
        }

        #endregion

        #region switch

        /// <summary>
        /// Visits switch statement and builds controlflow graf for switch construct.
        /// 
        /// </summary>
        /// <param name="x">SwitchStmt</param>
        public override void VisitSwitchStmt(SwitchStmt x)
        {
            var aboveCurrentCaseBlock = currentBasicBlock;
            BasicBlock lastDefaultStartBlock = null;
            BasicBlock lastDefaultEndBlock = null;
            currentBasicBlock = new BasicBlock();
            //in case of switch statement, continue and break means the same so we make the edge always to the block under the switch
            BasicBlock underLoop = new BasicBlock();
            loopData.Push(new LoopData(underLoop, underLoop));

            aboveCurrentCaseBlock.CreateWorklistSegment(underLoop);

            for (int j = 0; j < x.SwitchItems.Count; j++)
            {

                var switchItem = x.SwitchItems[j];
                var caseItem = switchItem as CaseItem;

                // The basic block corresponding to current switch item
                var switchBlock = new BasicBlock();

                // Connect previous switch item (implicitly, subsequent switch items are connected - break must 
                // be there to force the flow to not go to subsequent switch item)
                if (j > 0)
                    BasicBlockEdge.ConnectDirectEdge(currentBasicBlock, switchBlock);

                if (caseItem == null) 
                {
                    // Default branch

                    // Just mark the last default branch
                    lastDefaultStartBlock = switchBlock;
                } 
                else
                {
                    // Case branch

                    // Create condition of the current case branch
                    BinaryEx condition = new BinaryEx(caseItem.CaseVal.Position, Operations.Equal, x.SwitchValue, caseItem.CaseVal);
                    graph.cfgAddedElements.Add(condition);

                    // Create conditional branching: true condition goes to the case, else condition goes above the next switch item
                    var elseBlock = new BasicBlock();
                    BasicBlockEdge.ConnectConditionalBranching(condition, aboveCurrentCaseBlock, switchBlock, elseBlock);
                    aboveCurrentCaseBlock = elseBlock;
                }

                // Builds CFG for the body of the switch element
                currentBasicBlock = switchBlock;
                switchItem.VisitMe(this);

                if (caseItem == null)
                    // Just to mark the last default branch
                    lastDefaultEndBlock = currentBasicBlock;

            }

            loopData.Pop();

            if (lastDefaultStartBlock == null) // No default branch
            {
                // Connect the last case with the code under the switch
                BasicBlockEdge.ConnectDirectEdge(currentBasicBlock, underLoop);
                // Connect the else of the last case with the code under the switch
                BasicBlockEdge.ConnectDirectEdge(aboveCurrentCaseBlock, underLoop);
            }
            else // There is default branch
            {
                // The last default branch is the else of the last case
                BasicBlockEdge.ConnectDirectEdge(aboveCurrentCaseBlock, lastDefaultStartBlock);
                if (lastDefaultEndBlock.DefaultBranch == null) // break/continue in the default branch
                    // Connect it with the code under the swithch
                    BasicBlockEdge.ConnectDirectEdge(lastDefaultEndBlock, underLoop);
                else // no break/continue in the default branch
                    // Connect the last case with the code under the switch
                    BasicBlockEdge.ConnectDirectEdge(currentBasicBlock, underLoop);
            }

            currentBasicBlock = underLoop;
        }

        /// <summary>
        /// Visits CaseItem, and forwards visitor for all statements.
        /// </summary>
        /// <param name="x">CaseItem</param>
        public override void VisitCaseItem(CaseItem x)
        {
            VisitStatementList(x.Statements);
        }

        /// <summary>
        /// Visits DefaultItem, and forwards visitor for all statements.
        /// </summary>
        /// <param name="x">DefaultItem</param>
        public override void VisitDefaultItem(DefaultItem x)
        {
            VisitStatementList(x.Statements);
        }

        #endregion

        #region break continue return

        /// <summary>
        /// Visits JumpStmt (break, continue, return).
        /// </summary>
        /// <param name="x">JumpStmt</param>
        public override void VisitJumpStmt(JumpStmt x)
        {

            switch (x.Type)
            {
                case JumpStmt.Types.Break:
                case JumpStmt.Types.Continue:
                    if (x.Expression == null)//break without saying how many loops to break
                    {
                        BasicBlock target;
                        //break/continue
                        if (loopData.Count == 0)
                        {
                            if (x.Type == JumpStmt.Types.Break)
                            {
                                throw new ControlFlowException(ControlFlowExceptionCause.BREAK_NOT_IN_CYCLE, x.Position);
                            }
                            else
                            {
                                throw new ControlFlowException(ControlFlowExceptionCause.CONTINUE_NOT_IN_CYCLE, x.Position);
                            }
                        }
                        if (x.Type == JumpStmt.Types.Break)
                        {
                            target = loopData.Peek().BreakTarget;
                        }
                        else
                        {
                            target = loopData.Peek().ContinueTarget;
                        }
                        BasicBlockEdge.ConnectDirectEdge(currentBasicBlock, target);
                    }
                    else
                    {
                        int breakValue = 1;
                        for (int i = loopData.Count - 1; i >= 0; --i)
                        {
                            BasicBlock target;
                            if (x.Type == JumpStmt.Types.Break)
                            {
                                target = loopData.ElementAt(i).BreakTarget;
                            }
                            else
                            {
                                target = loopData.ElementAt(i).ContinueTarget;
                            }
                            BinaryEx condition = new BinaryEx(x.Position, Operations.Equal, new IntLiteral(Position.Invalid, breakValue), x.Expression);
                            graph.cfgAddedElements.Add(condition);
                            BasicBlockEdge.ConnectConditionalBranching(condition, currentBasicBlock, target, new BasicBlock());
                            //BasicBlockEdge.AddConditionalEdge(currentBasicBlock, target, condition);
                            ++breakValue;
                        }
                    }
                    break;

                case JumpStmt.Types.Return:

                    PHP.Core.Debug.Assert(functionSinkStack.Count > 0);

                    currentBasicBlock.AddElement(x);
                    BasicBlockEdge.ConnectDirectEdge(currentBasicBlock, functionSinkStack.Peek());

                    currentBasicBlock = new BasicBlock();

                    break;
            }


            currentBasicBlock = new BasicBlock();

        }

        #endregion

        #region Forwarding to VisitStatementList or VisitExpressionList

        /// <summary>
        /// Visits list of statements and forwards the visitor to children.
        /// </summary>
        /// <param name="list">list</param>
        private void VisitStatementList(List<Statement> list)
        {
            foreach (var stmt in list)
                stmt.VisitMe(this);
        }

        /// <summary>
        /// Visits list of expressions and forwarrds the visitor to children.
        /// </summary>
        /// <param name="list">list</param>
        private void VisitExpressionList(List<Expression> list)
        {
            foreach (var e in list)
                e.VisitMe(this);
        }

        #endregion

        #region Forwarding to default VisitElement

        /// <summary>
        /// Visits the StringLiteral.
        /// </summary>
        /// <param name="x">StringLiteral</param>
        public override void VisitStringLiteral(StringLiteral x)
        {
            this.VisitElement(x);
        }

        /// <summary>
        /// Visits DirectVarUse.
        /// </summary>
        /// <param name="x">DirectVarUse</param>
        public override void VisitDirectVarUse(DirectVarUse x)
        {
            this.VisitElement(x);
        }

        /// <summary>
        /// Visits the ConstantUse statement.
        /// </summary>
        /// <param name="x">ConstantUse</param>
        public override void VisitConstantUse(ConstantUse x)
        {
            this.VisitElement(x);
        }

        /// <summary>
        /// Visits EchoStmt.
        /// </summary>
        /// <param name="x">EchoStmt</param>
        public override void VisitEchoStmt(EchoStmt x)
        {
            this.VisitElement(x);
        }

        /// <summary>
        /// Visits BinaryEx.
        /// </summary>
        /// <param name="x"></param>
        public override void VisitBinaryEx(BinaryEx x)
        {
            this.VisitElement(x);
        }

        /// <summary>
        /// Visits ValueAssignEx.
        /// </summary>
        /// <param name="x">ValueAssignEx</param>
        public override void VisitValueAssignEx(ValueAssignEx x)
        {
            this.VisitElement(x);
        }

        /// <summary>
        /// Visits IncDecEx.
        /// </summary>
        /// <param name="x">IncDecEx</param>
        public override void VisitIncDecEx(IncDecEx x)
        {
            this.VisitElement(x);
        }

        #endregion

        #region handling Exceptions

        /// <summary>
        /// Visits try catch statement and connects thrown exceptions to catch blocks or function sink.
        /// </summary>
        /// <param name="x">TryStmt</param>
        public override void VisitTryStmt(TryStmt x)
        {
            BasicBlock followingBlock = new BasicBlock();

            TryBasicBlock tryBlock = new TryBasicBlock();

            BasicBlockEdge.ConnectDirectEdge(currentBasicBlock, tryBlock);
            currentBasicBlock = tryBlock;

            //throwBlocks.Push(new List<BasicBlock>());
            VisitStatementList(x.Statements);
            currentBasicBlock.EndIngTryBlocks.Add(tryBlock);
            BasicBlockEdge.ConnectDirectEdge(currentBasicBlock, followingBlock);


            foreach (var catchItem in x.Catches)
            {

                CatchBasicBlock catchBlock = new CatchBasicBlock(catchItem.Variable, catchItem.ClassName);

                tryBlock.catchBlocks.Add(catchBlock);
                currentBasicBlock = catchBlock;
                VisitStatementList(catchItem.Statements);
                BasicBlockEdge.ConnectDirectEdge(currentBasicBlock, followingBlock);
            }


            //throwBlocks.Pop();
            currentBasicBlock = followingBlock;

        }

        /// <summary>
        /// Visits throw statement adds it to controlflow graph.
        /// </summary>
        /// <param name="x">ThrowStmt</param>
        public override void VisitThrowStmt(ThrowStmt x)
        {
            currentBasicBlock.AddElement(x);
            /* foreach(var item in throwBlocks)
             {
                 item.Add(currentBasicBlock);
             }*/
            currentBasicBlock = new BasicBlock();
        }

        #endregion

        #region other

        /// <summary>
        /// Makes a deep copy of the AST expression e in a parameter using AST Copy visitor.
        /// 
        /// See <see cref="DeepCopyAstExpressionByParsing"/> to other method
        /// of creating a copy of an expression.
        /// 
        /// This method of copying expressions is faster but the visitor is not mature and can contain bugs.
        /// </summary>
        /// <param name="e">expression to copy.</param>
        /// <returns></returns>
        public static Expression DeepCopyAstExpressionCopyVisitor(Expression e)
        {
            var expressionCopyVisitor = new ExpressionCopyVisitor();
            return expressionCopyVisitor.MakeDeepCopy(e);
        }

        /// <summary>
        /// Makes a deep copy of the AST expression e in a parameter.
        /// Finds the code of e and than creates a copy of e by parsing the code.
        /// 
        /// See <see cref="DeepCopyAstExpressionCopyVisitor"/> to other method
        /// of creating a copy of an expression.
        /// </summary>
        /// <param name="e">expression to copy.</param>
        /// <returns></returns>
        private static Expression DeepCopyAstExpressionByParsing(ControlFlowGraph graph, Expression e)
        {

            //known bug
            //when creating cfg from function of method, we need global code
            //probably need lot of refacotring
            //or getting the global code for every function or method
            if (graph.globalCode == null)
                return e;
            StringBuilder s = new StringBuilder("<? ");
            if (e.Position.FirstLine == 1)
            {
                for (int i = 3; i < e.Position.FirstOffset; i++)
                {
                    s.Append(" ");
                }
            }
            else
            {
                for (int i = 3 + e.Position.FirstLine - 1 + e.Position.FirstColumn - 1; i < e.Position.FirstOffset; i++)
                {
                    s.Append(" ");
                }

                for (int i = 1; i < e.Position.FirstLine; i++)
                {
                    s.Append("\n");
                }

                for (int i = 1; i < e.Position.FirstColumn; i++)
                {
                    s.Append(" ");
                }
            }
            s.Append(graph.globalCode.SourceUnit.GetSourceCode(e.Position));
            s.Append(" ?>");

            var sourceFile = new PhpSourceFile(new FullPath(Path.GetDirectoryName(graph.File.FullName)),
                new FullPath(graph.File.FullName));
            SyntaxParser parser = new SyntaxParser(sourceFile, s.ToString());
            parser.Parse();
            return (parser.Ast.Statements[0] as ExpressionStmt).Expression;

        }

        /// <summary>
        /// Constructs the simple condition from the given list.
        /// </summary>
        /// <param name="conditionList">The condition list.</param>
        /// <returns></returns>
        private Expression constructSimpleCondition(List<Expression> conditionList)
        {
            Expression groupCondition;
            if (conditionList.Count > 0)
            {
                groupCondition = conditionList[0];
                for (int index = 1; index < conditionList.Count; index++)
                {
                    Position newPosition = mergePositions(groupCondition.Position, conditionList[index].Position);
                    groupCondition = new BinaryEx(newPosition, Operations.And, groupCondition, conditionList[index]);
                }
            }
            else
            {
                groupCondition = new BoolLiteral(Position.Invalid, true);
            }

            return groupCondition;
        }

        /// <summary>
        /// Merges the positions.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="last">The last.</param>
        /// <returns></returns>
        private Position mergePositions(Position first, Position last)
        {
            return new Position(
                first.FirstLine,
                first.FirstColumn,
                first.FirstOffset,
                last.LastLine,
                last.LastColumn,
                last.LastOffset);
        }

        #endregion
    }
}