using NUnit.Framework;
using PHP.Core.ControlFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PHP.Tests.Utils;
using PHP.Core.AST;
using PHP.ControlFlow;

namespace PHP.Tests.ControlFlow
{
    [TestFixture]
    public class WorklistRunnerTests
    {
        [Test]
        public void TestPropagationThroughIfThen()
        {
            // Note: we will use the number of statements to distinguish the nodes:
            // number of statements is the expected value of the flow.
            var globalCode = @"<?php
                foo();
                if ($x == 0) {
                    echo $x;
                    foo();
                    foo();
                }
                foo();
                foo();
                $x++;
                foo();foo();foo();".GetAST();

            var analysis = new DummyAnalysis();
            analysis.MergeAction = (s1, s2) => s1.Value += s2.Value;
            analysis.FlowThroughAction = (set, stmt) =>
                {
                    if (stmt is BinaryEx)
                    {
                        Assert.AreEqual(0, set.Value, "Initial value for entry block should be 0");
                        set.Value = 2;
                        set.HasChanged = true;
                    }
                    else if (stmt is EchoStmt)
                    {
                        Assert.AreEqual(2, set.Value, "then branch should get flow from the if expression (the entry block)");
                        set.Value++;
                        set.HasChanged = true;
                    }
                    else if (stmt is IncDecEx)
                    {
                        Assert.AreEqual(5, set.Value, "after if block should get flow from the then branch and from the if expression");
                        set.Value++;
                        set.HasChanged = true;
                    }
                    else
                    {
                        Assert.IsInstanceOf<DirectFcnCall>(stmt, "Unexpected statement type " + stmt.GetType().Name);
                    }
                };

            var graph = ControlFlowGraph.Construct(globalCode);
            new WorklistAlgorithm().Run(analysis, graph);

            foreach (var block in graph.Blocks)
                Assert.AreEqual(block.Statements.Count, ((DummyFlowSet)block.Tag).Value, 
                    "Flowset does not have expected value for block " + block.ToString(globalCode.SourceUnit));
        }

        [Test]
        public void TestLoopPropagation()
        {
            // Note: we will use the type of statement to distinguish the node
            var globalCode = @"<?php
                echo 'Start';
                do {
                    $x++;
                } while ($x < 10);

                foo();foo();foo();".GetAST();

            var analysis = new DummyAnalysis();
            analysis.MergeAction = (s1, s2) => s1.Value = Math.Max(s1.Value, s2.Value);
            int expectedIn = 1;
            analysis.FlowThroughAction = (set, stmt) =>
                {
                    if (stmt is EchoStmt)
                    {
                        Assert.AreEqual(0, set.Value, "Initial value for entry block should be 0");
                        set.Value = 1;
                        set.HasChanged = true;
                    }
                    else if (stmt is IncDecEx)
                    {
                        Assert.AreEqual(expectedIn, set.Value, "Wrong expected value in the loop body");
                        if (set.Value < 10)
                        {
                            expectedIn = ++set.Value;
                            set.HasChanged = true;
                        }
                    }
                };

            var graph = ControlFlowGraph.Construct(globalCode);
            new WorklistAlgorithm().Run(analysis, graph);

            var lastBlock = graph.Blocks.First(x => x.Statements.Any(s => s is DirectFcnCall));
            Assert.AreEqual(10, ((DummyFlowSet)lastBlock.Tag).Value);
        }

        private class DummyAnalysis : FlowAnalysis<DummyFlowSet>
        {
            public DummyAnalysis() : base(FlowAnalysisDirection.Forward) { }

            public Action<DummyFlowSet, LangElement> FlowThroughAction;

            public Action<DummyFlowSet, DummyFlowSet> MergeAction;

            protected override DummyFlowSet GetInitialFlow()
            {
                return new DummyFlowSet { Value = 0 };
            }

            protected override void Merge(DummyFlowSet resultDataFlow, DummyFlowSet dataFlow)
            {
                this.MergeAction(resultDataFlow, dataFlow);
            }

            protected override void FlowThrough(DummyFlowSet set, Core.AST.LangElement statement)
            {
                this.FlowThroughAction(set, statement);
            }

            protected override void Copy(DummyFlowSet source, DummyFlowSet dest)
            {
                source.Value = dest.Value;
            }
        }


        private class DummyFlowSet : IChangeable
        {
            public int Value { get; set; }

            public bool HasChanged { get; set; }

            public void ResetHasChanged()
            {
                this.HasChanged = false;
            }
        }
    }
}
