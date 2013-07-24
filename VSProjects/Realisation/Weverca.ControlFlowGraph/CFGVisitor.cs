using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Parsers;
using PHP.Core.Reflection;

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
    class LinkedStack<T> : LinkedList<T> {    
        /// <summary>
        /// Inserts an object at the top of the Stack.
        /// </summary>
        /// <param name="t">Object</param>
        public void Push(T t) {
            AddLast(t);
        }
        /// <summary>
        /// Returns the object at the top of the Stack without removing it.
        /// </summary>
        /// <returns>Returns the object at the top of the Stack.</returns>
        public T Peek() {
            return this.Last();
        }
        /// <summary>
        /// Removes and returns the object at the top of the Stack.
        /// </summary>
        /// <returns>Returns removed object.</returns>
        public T Pop() {
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

        /// <summary>
        /// Saves target block of this label and process all blocks in GOTO queue.
        /// </summary>
        /// <param name="labelBlock">The label block.</param>
        public void AsociateLabel(BasicBlock labelBlock)
        {
            this.labelBlock = labelBlock;

            foreach (BasicBlock gotoBlock in GotoQueue)
            {
                _asociateGoto(gotoBlock);
            }

            GotoQueue.Clear();
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
            System.Diagnostics.Debug.Assert(labelBlock != null);

            DirectEdge.MakeNewAndConnect(gotoBlock, labelBlock);
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
        /// <param name="key">´The name of the label.</param>
        /// <returns></returns>
        public LabelData GetOrCreateLabelData(VariableName key)
        {
            LabelData data;
            if (!TryGetValue(key.Value, out data))
            {
                data = new LabelData();
                Add(key.Value, data);
            }

            return data;
        }
    }

    //TODO multiple labels, no labels for jump. Is there an exception???

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
        /// Stack of block which ends by throw
        /// </summary>
        LinkedStack<List<BasicBlock>> throwBlocks = new LinkedStack<List<BasicBlock>>();
                
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
            throwBlocks.Push(new List<BasicBlock>());
        }


        public override void VisitElement(LangElement element)
        {
            currentBasicBlock.AddElement(element);
        }

        public override void VisitGlobalCode(GlobalCode x)
        {
            foreach(Statement statement in x.Statements)
            {
                statement.VisitMe(this);
            }
            DirectEdge.MakeNewAndConnect(currentBasicBlock, functionSinkStack.Peek());
            foreach (var block in throwBlocks.ElementAt(0)) {
                block.Statements.RemoveLast();
                DirectEdge.MakeNewAndConnect(block, functionSinkStack.Peek());
            }
       }

        public override void VisitGlobalStmt(GlobalStmt x)
        {
            currentBasicBlock.AddElement(x);
        }

        public override void VisitBlockStmt(BlockStmt x)
        {
            VisitStatementList(x.Statements);

        }

        #region Labels and jumps

        public override void VisitLabelStmt(LabelStmt x)
        {
            BasicBlock labelBlock = new BasicBlock();

            labelDictionary.GetOrCreateLabelData(x.Name)
                .AsociateLabel(labelBlock);

            DirectEdge.MakeNewAndConnect(currentBasicBlock, labelBlock);
            currentBasicBlock = labelBlock;

            //Next line could be used for label visualization
            labelBlock.AddElement(x);
        }

        public override void VisitGotoStmt(GotoStmt x)
        {
            labelDictionary.GetOrCreateLabelData(x.LabelName)
                .AsociateGoto(currentBasicBlock);

            //Next line could be used for label visualization
            currentBasicBlock.AddElement(x);

            //THIS COULD BE AN UNREACHABLE BLOCK
            currentBasicBlock = new BasicBlock();
        }

        #endregion Labels and jumps

        #region conditions

        public override void VisitIfStmt(IfStmt x)
        {
            //Merge destination for if and else branch
            BasicBlock bottomBox = new BasicBlock();

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
            DirectEdge.MakeNewAndConnect(currentBasicBlock, bottomBox);
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
            ConditionalEdge.MakeNewAndConnect(currentBasicBlock, thenBranchBlock, condition.Condition);

            BasicBlock elseBranchBlock = new BasicBlock();
            DirectEdge.MakeNewAndConnect(currentBasicBlock, elseBranchBlock);

            currentBasicBlock = thenBranchBlock;
            condition.Statement.VisitMe(this);
            DirectEdge.MakeNewAndConnect(currentBasicBlock, bottomBox);

            return elseBranchBlock;
        }

        #endregion

        #region declarations

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

        public override void VisitTypeDecl(TypeDecl x)
        {
            currentBasicBlock.AddElement(x);
        }

        public override void VisitMethodDecl(MethodDecl x)
        {
             BasicBlock functionBasicBlock = MakeFunctionCFG(x, x.Body);
        }

        public override void VisitFunctionDecl(FunctionDecl x)
        {
            currentBasicBlock.AddElement(x);
            /*
            BasicBlock functionBasicBlock = MakeFunctionCFG(x, x.Body);
            graph.AddFunctionDeclaration(x, functionBasicBlock);*/
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
            LabelDataDictionary oldLabelData = labelDictionary;
            labelDictionary = new LabelDataDictionary();

            //Add function sink to the stack for resolving returns
            BasicBlock functionSink = new BasicBlock();
            functionSinkStack.Push(functionSink);        
            
            VisitStatementList(functionBody);

            foreach (var block in throwBlocks.ElementAt(0))
            {
                block.Statements.RemoveLast();
                DirectEdge.MakeNewAndConnect(block, functionSink);
            }


            //Connects return destination
            functionSinkStack.Pop();
            DirectEdge.MakeNewAndConnect(currentBasicBlock, functionSink);

            //Loads previous labels
            labelDictionary = oldLabelData;
            currentBasicBlock = current;

            return functionBasicBlock;
        }

        #endregion 

        #region cycles

        public override void VisitForeachStmt(ForeachStmt x)
        {
            BasicBlock foreachHead = new BasicBlock();
            BasicBlock foreachBody = new BasicBlock();
            BasicBlock foreachSink = new BasicBlock();

            //Input edge to the foreach statement
            DirectEdge.MakeNewAndConnect(currentBasicBlock, foreachHead);
            foreachHead.AddElement(x);

            //Conditional edge to the foreach body
            ConditionalEdge.MakeNewAndConnect(foreachHead, foreachBody, x.ValueVariable.Variable);

            //Visits foreach body
            loopData.Push(new LoopData(foreachHead, foreachSink));
            currentBasicBlock = foreachBody;
            x.Body.VisitMe(this);

            //Connect end of foreach with foreach head
            DirectEdge.MakeNewAndConnect(currentBasicBlock, foreachHead);

            //Output edge to the sink
            DirectEdge.MakeNewAndConnect(foreachHead, foreachSink);
            currentBasicBlock = foreachSink;
        }

        public override void VisitForStmt(ForStmt x)
        {
            BasicBlock forTest = new BasicBlock();
            BasicBlock forBody = new BasicBlock();
            BasicBlock forEnd = new BasicBlock();
            BasicBlock forIncrement = new BasicBlock();
            //Adds initial connection from previos to the test block
            DirectEdge.MakeNewAndConnect(currentBasicBlock, forTest);

            if (x.CondExList.Count > 0)
            {
                //Adds connection into the loop body
                Expression forCondition = constructSimpleCondition(x.CondExList);
                ConditionalEdge.MakeNewAndConnect(forTest, forBody, forCondition);
            }
            else { 
                //if there is no condition
                ConditionalEdge.MakeNewAndConnect(forTest, forBody, new BoolLiteral(Position.Invalid,true));
            }
            //Adds connection behind the cycle
            DirectEdge.MakeNewAndConnect(forTest, forEnd);
            
            //Loop body
            VisitExpressionList(x.InitExList);
            currentBasicBlock = forBody;
            loopData.Push(new LoopData(forIncrement, forEnd));
            x.Body.VisitMe(this);
            loopData.Pop();
            BasicBlock forBodyEnd = currentBasicBlock;
            currentBasicBlock = forIncrement;
            DirectEdge.MakeNewAndConnect(forBodyEnd, currentBasicBlock);
            VisitExpressionList(x.ActionExList);

            //Adds loop connection to test block
            DirectEdge.MakeNewAndConnect(currentBasicBlock, forTest);

            currentBasicBlock = forEnd;
        }


        public override void VisitWhileStmt(WhileStmt x)
        {
            BasicBlock aboveLoop = currentBasicBlock;
            BasicBlock startLoop = new BasicBlock();
            if (x.LoopType == WhileStmt.Type.While)
            {
                ConditionalEdge.MakeNewAndConnect(aboveLoop, startLoop, x.CondExpr);
            }
            else
            {
                DirectEdge.MakeNewAndConnect(aboveLoop, startLoop);
            }
            currentBasicBlock = startLoop;
            BasicBlock underLoop = new BasicBlock();
            loopData.Push(new LoopData(startLoop, underLoop));
            x.Body.VisitMe(this);
            loopData.Pop();

            BasicBlock endLoop = currentBasicBlock;


            DirectEdge.MakeNewAndConnect(endLoop, underLoop);

            if (x.LoopType == WhileStmt.Type.While)
            {
                DirectEdge.MakeNewAndConnect(aboveLoop, underLoop);
            }
            ConditionalEdge.MakeNewAndConnect(endLoop, startLoop, x.CondExpr);

            currentBasicBlock = underLoop;
        }

        #endregion

        #region switch

        public override void VisitSwitchStmt(SwitchStmt x)
        {
            BasicBlock above=currentBasicBlock;
            BasicBlock last;
            bool containsDefault=false;
            currentBasicBlock = new BasicBlock();
            //in case of switch statement, continue and break means the same so we make the egde allways to the block under the switch
            BasicBlock underLoop = new BasicBlock();
            loopData.Push(new LoopData(underLoop, underLoop));
            
            foreach (var switchItem in x.SwitchItems) {

                Expression right = null;
                if (switchItem.GetType() == typeof(CaseItem))
                {
                    right = ((CaseItem)switchItem).CaseVal;
                    ConditionalEdge.MakeNewAndConnect(above, currentBasicBlock, new BinaryEx(Operations.Equal, x.SwitchValue, right));
                }
                else 
                {
                    DirectEdge.MakeNewAndConnect(above, currentBasicBlock);

                    if (containsDefault == false)
                    {
                        containsDefault = true;
                    }
                    else {
                        throw new ControlFlowException(ControlFlowExceptionCause.MULTIPLE_DEFAULT_USE);
                    }
                }
                
                
                switchItem.VisitMe(this);
                last = currentBasicBlock;
                currentBasicBlock = new BasicBlock();
                DirectEdge.MakeNewAndConnect(last, currentBasicBlock);
   
            }
            loopData.Pop();
            DirectEdge.MakeNewAndConnect(currentBasicBlock, underLoop);
            currentBasicBlock = underLoop;
            if (containsDefault == false)
            {
                DirectEdge.MakeNewAndConnect(above, currentBasicBlock);
            }
        }

        public override void VisitCaseItem(CaseItem x)
        {
            VisitStatementList(x.Statements);
        }

        public override void VisitDefaultItem(DefaultItem x)
        {
            VisitStatementList(x.Statements);
        }

        #endregion

        #region break contniue return 

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
                                throw new ControlFlowException(ControlFlowExceptionCause.BREAK_NOT_IN_CYCLE);
                            }
                            else 
                            {
                                throw new ControlFlowException(ControlFlowExceptionCause.CONTINUE_NOT_IN_CYCLE);
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
                        DirectEdge.MakeNewAndConnect(currentBasicBlock, target);
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
                            ConditionalEdge.MakeNewAndConnect(currentBasicBlock, target, new BinaryEx(Operations.Equal, new IntLiteral(Position.Invalid, breakValue), x.Expression));
                            ++breakValue;
                        }
                    }
                    break;

                case JumpStmt.Types.Return:

                    System.Diagnostics.Debug.Assert(functionSinkStack.Count > 0);

                    currentBasicBlock.AddElement(x);
                    DirectEdge.MakeNewAndConnect(currentBasicBlock, functionSinkStack.Peek());

                    currentBasicBlock = new BasicBlock();
                    
                    break;
            }
            

            currentBasicBlock = new BasicBlock();

        }

        #endregion

        #region Forwarding to VisitStatementList or VisitExpressionList

        private void VisitStatementList(List<Statement> list)
        {
            foreach (var stmt in list)
                stmt.VisitMe(this);
        }

        private void VisitExpressionList(List<Expression> list)
        {
            foreach (var e in list)
                e.VisitMe(this);
        }

        #endregion

        #region Forwarding to default VisitElement

        public override void VisitStringLiteral(StringLiteral x)
        {
            this.VisitElement(x);
        }

        public override void VisitDirectVarUse(DirectVarUse x)
        {
            this.VisitElement(x);
        }

        public override void VisitConstantUse(ConstantUse x)
        {
            this.VisitElement(x);
        }

        public override void VisitEchoStmt(EchoStmt x)
        {
            this.VisitElement(x);
        }

        public override void VisitBinaryEx(BinaryEx x)
        {
            this.VisitElement(x);
        }

        public override void VisitValueAssignEx(ValueAssignEx x)
        {
            this.VisitElement(x);
        }

        public override void VisitIncDecEx(IncDecEx x)
        {
            this.VisitElement(x);
        }

        #endregion

        #region handling Exceptions


        public override void VisitTryStmt(TryStmt x)
        {
            BasicBlock followingBlock = new BasicBlock();
         

            throwBlocks.Push(new List<BasicBlock>());
            VisitStatementList(x.Statements);
            DirectEdge.MakeNewAndConnect(currentBasicBlock, followingBlock);
            
            
            foreach(var catchItem in x.Catches){

                BasicBlock catchBlock = new BasicBlock();

                foreach (var throwBlock in throwBlocks.Peek())
                {
                    ThrowStmt throwStatement = (ThrowStmt)throwBlock.Statements.Last();
                    //throwBlock.Statements.RemoveLast();
                    List<ActualParam> parameters = new List<ActualParam>();
                    parameters.Add(new ActualParam(Position.Invalid, throwStatement.Expression, false));
                    parameters.Add(new ActualParam(Position.Invalid, new StringLiteral(Position.Invalid, catchItem.ClassName.QualifiedName.ToString()), false));
                    ConditionalEdge.MakeNewAndConnect(throwBlock, catchBlock, new DirectFcnCall(Position.Invalid, new QualifiedName(new Name("is_subclass_of")), null, Position.Invalid, parameters, new List<TypeRef>()));
                }
               
                
                currentBasicBlock=catchBlock;
                VisitStatementList(catchItem.Statements);
                DirectEdge.MakeNewAndConnect(currentBasicBlock,followingBlock);
            }
            
            
            throwBlocks.Pop();
            currentBasicBlock=followingBlock;
            
        }

        public override void VisitThrowStmt(ThrowStmt x)
        {
            currentBasicBlock.AddElement(x);
            foreach(var item in throwBlocks)
            {
                item.Add(currentBasicBlock);
            }
            currentBasicBlock=new BasicBlock();
        }

        #endregion

        #region other

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
