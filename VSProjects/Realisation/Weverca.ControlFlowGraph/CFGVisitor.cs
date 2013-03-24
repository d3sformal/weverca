using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;
using PHP.Core.Parsers;
using PHP.Core.Reflection;

namespace Weverca.ControlFlowGraph
{
    class LoopData
    {
        public BasicBlock LoopStart { get; private set; }
        public BasicBlock LoopEnd { get; set; }

        public LoopData(BasicBlock loopStart)
        {
            LoopStart = loopStart;
        }
        public LoopData(BasicBlock loopStart, BasicBlock loopEnd)
        {
            LoopStart = loopStart;
            LoopEnd = loopEnd;
        }
    }

    class CFGVisitor : TreeVisitor
    {
        ControlFlowGraph graph;
        BasicBlock currentBasicBlock;

        LinkedList<LoopData> loopData = new LinkedList<LoopData>();
        private Dictionary<string, BasicBlock> labels = new Dictionary<string, BasicBlock>();


        public CFGVisitor(ControlFlowGraph graph)
        {
            this.graph = graph;
            currentBasicBlock = new BasicBlock();
            graph.start = currentBasicBlock;
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
        }

        public override void VisitLabelStmt(LabelStmt x)
        {
            throw new NotImplementedException();
        }

        public override void VisitGotoStmt(GotoStmt x)
        {
            throw new NotImplementedException();
        }

        public override void VisitIfStmt(IfStmt x)
        {
            throw new NotImplementedException();
        }

        public override void VisitSwitchStmt(SwitchStmt x)
        {
            throw new NotImplementedException();
        }

        public override void VisitJumpStmt(JumpStmt x)
        {
            throw new NotImplementedException();
        }

        public override void VisitForStmt(ForStmt x)
        {
            throw new NotImplementedException();
        }

        public override void VisitWhileStmt(WhileStmt x)
        {
            BasicBlock aboveLoop = currentBasicBlock;
            BasicBlock startLoop = new BasicBlock();
            BasicBlockEdge edge;
            if (x.LoopType == WhileStmt.Type.While)
            {
                edge = new BasicBlockEdge(aboveLoop, startLoop, x.CondExpr);
            }
            else
            {
                edge = new BasicBlockEdge(aboveLoop, startLoop, new BoolLiteral(Position.Invalid, true));
            }
            currentBasicBlock = startLoop;
            x.Body.VisitMe(this);
            BasicBlock endLoop = currentBasicBlock;
            BasicBlock underLoop = new BasicBlock();
            edge = new BasicBlockEdge(endLoop, underLoop, new UnaryEx(Operations.LogicNegation, x.CondExpr));
            if (x.LoopType == WhileStmt.Type.While)
            {
                edge = new BasicBlockEdge(aboveLoop, underLoop, new UnaryEx(Operations.LogicNegation, x.CondExpr));
            }
            edge = new BasicBlockEdge(endLoop, startLoop, x.CondExpr);

            currentBasicBlock = underLoop;
        }


        public override void VisitSwitchItem(SwitchItem x)
        {
            throw new NotImplementedException();
        }

        public override void VisitBlockStmt(BlockStmt x)
        {
            VisitStatementList(x.Statements);

        }

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

        

        private void VisitConditionalStatements(List<ConditionalStmt> list)
        {
            throw new NotImplementedException();
        }
    }
}
