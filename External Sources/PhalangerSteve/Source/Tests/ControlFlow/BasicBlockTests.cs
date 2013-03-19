using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PHP.ControlFlow;
using PHP.Core.AST;
using PHP.Core.Parsers;
using PHP.Tests.Utils;
using PHP.Core.ControlFlow;

namespace PHP.Tests.ControlFlow
{
    [TestFixture]
    public class BasicBlockTests
    {
        private ControlFlowGraph g = new ControlFlowGraph();

        [Test]
        public void ThreeAddressStatementsReturnsLinearizedExpression1()
        {
            var b = BasicBlock.Create(this.g);
            b.Statements.Add(
                new BinaryEx(Operations.Add, 
                    new DirectVarUse(new Position(), "a"),
                    new BinaryEx(Operations.Mul,
                        new IntLiteral(new Position(), 12),
                        new DirectVarUse(new Position(), "b"))));
            var lin = b.GetThreeAddressStatements().ToArray();

            Assert.AreEqual(2, lin.Length);

            Assert.IsInstanceOf<BinaryEx>(lin[0]);
            Assert.AreEqual(Operations.Mul, ((BinaryEx)lin[0]).Operation);

            Assert.IsInstanceOf<BinaryEx>(lin[1]);
            Assert.AreEqual(Operations.Add, ((BinaryEx)lin[1]).Operation);
        }

        [Test]
        public void ThreeAddressStatementsReturnsLinearizedExpression2()
        {
            var b = BasicBlock.Create(this.g);
            GlobalCode c = "<?php ($a-4)*$b + $c/1; ".GetAST();
            b.Statements.Add(c.Statements.First());

            var lin = b.GetThreeAddressStatements().OfType<BinaryEx>().ToArray();

            Assert.AreEqual(4, lin.Length);

            CollectionAssert.AreEquivalent(
                new[] { Operations.Add, Operations.Mul, Operations.Sub, Operations.Div },
                lin.Select(x => x.Operation).ToArray(),
                "Returned statements does not contain all four operations expected.");

            Assert.AreEqual(
                Operations.Add, 
                lin[3].Operation,
                "The last operation in the statements list must be the top one in the expression tree");

            int minusPos = lin.Select((x, i) => Tuple.Create(x, i))
                .Where(t => t.Item1.Operation == Operations.Sub).Select(t => t.Item2).First();
            int mulPos = lin.Select((x, i) => Tuple.Create(x, i))
                .Where(t => t.Item1.Operation == Operations.Mul).Select(t => t.Item2).First();

            Assert.IsTrue(
                minusPos < mulPos,
                "The minus must be evaluated before the multiplication");
        }

        [Test]
        public void ThreeAddressStatementsReturnsLinearizedBasicBlock()
        {
            var b = BasicBlock.Create(this.g);
            GlobalCode c = "<?php $x=(($a-4)*$b)+$c; foo($x/2); ".GetAST();
            b.Statements.Add(c.Statements[0]);
            b.Statements.Add(c.Statements[1]);

            var lin = b.GetThreeAddressStatements().ToArray();

            CollectionAssert.AreEqual(
                new[] { typeof(BinaryEx), typeof(BinaryEx), typeof(BinaryEx), 
                    typeof(ValueAssignEx), typeof(BinaryEx), typeof(DirectFcnCall) },
                lin.Select(x => x.GetType()).ToArray());

            CollectionAssert.AreEqual(
                new[] { Operations.Sub, Operations.Mul, Operations.Add, Operations.Div },
                lin.OfType<BinaryEx>().Select(x => x.Operation).ToArray());
        }
    }
}
