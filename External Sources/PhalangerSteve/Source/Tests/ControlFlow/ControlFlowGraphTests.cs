using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PHP.Tests.Utils;
using PHP.ControlFlow;

namespace PHP.Tests.ControlFlow
{
    [TestFixture]
    public class ControlFlowGraphTests
    {
        [Test]
        public void ConstructGraphForIfElse()
        {
            var globalCode = 
                @"<?php 
                    $x = 3;
                    if ($x == 3) echo $x;
                    else if ($x == 4) echo 'ahoj';
                    else echo 'xxx';
                ".GetAST();
            var graph = ControlFlowGraph.Construct(globalCode);

            Assert.AreEqual(6, graph.Blocks.Count());
        }

        [Test]
        [TestCase("for ($i=0;$i<100;++$i) echo $i;")]
        [TestCase("while ($x < $y) echo $x;")]
        [TestCase("$dummy = 1; do { echo $x; } while ($x < $y);")]        
        public void ConstructGraphForLoop(string code)
        {
            var globalCode = ("<?php " + code).GetAST();
            var graph = ControlFlowGraph.Construct(globalCode);

            Assert.AreEqual(3, graph.Blocks.Count());

            var forBody = graph.Blocks.ElementAt(1);
            Assert.AreEqual(2, forBody.Next.Count());
            CollectionAssert.Contains(forBody.Next, forBody);
            CollectionAssert.Contains(forBody.Next, graph.Blocks.Last());
        }

        public void ConstructGraphForGoto()
        {
            var globalCode =
                @"<?php 
                    $x = 4;
                    label:
                        $x++;
                        if ($x<3) goto label;
                ".GetAST();
            var graph = ControlFlowGraph.Construct(globalCode);
        }
    }
}
