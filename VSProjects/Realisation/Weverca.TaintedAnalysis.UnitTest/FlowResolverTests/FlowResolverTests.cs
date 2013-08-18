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
            TestCase testCase = new TestCase(ConditionForm.All,
                new Expression[] { new BinaryEx(Operations.Equal, new DirectVarUse(new Position(), new VariableName("a")), new StringLiteral(new Position(), "aaa")) },
                new TestCase.ConditionResults[] { TestCase.ConditionResults.True });

            testCase.AddResult("a", new StringValue("aaa"));
            testCase.Run(true);
        }
    }
}
