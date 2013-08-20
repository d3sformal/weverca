using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Parsers;

using Weverca.Analysis;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.UnitTest.FlowResolverTests
{
    [TestClass]
    public class FlowResolverTests
    {
        [TestMethod]
        public void DirectVarEqualsString()
        {
            TestCase testCase = new TestCase(
                new Expression[] { new BinaryEx(Operations.Equal, new DirectVarUse(new Position(), new VariableName("a")), new StringLiteral(new Position(), "aaa")) },
                new TestCase.ConditionResults[] { TestCase.ConditionResults.True });

            testCase.AddResult(ConditionForm.All, true).AddResultValue("a", new StringValue("aaa"));
            testCase.Run();
        }

        [TestMethod]
        public void DirectVarEqualsInt()
        {
            TestCase testCase = new TestCase(
                new Expression[] { new BinaryEx(Operations.Equal, new DirectVarUse(new Position(), new VariableName("a")), new IntLiteral(new Position(), 1)) },
                new TestCase.ConditionResults[] { TestCase.ConditionResults.True });

            testCase.AddResult(ConditionForm.All, true).AddResultValue("a", new IntegerValue(1));
            testCase.Run();
        }
    }
}
