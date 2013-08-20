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
            TestCase.Create(new BinaryEx(Operations.Equal, new DirectVarUse(new Position(), new VariableName("a")), new StringLiteral(new Position(), "aaa")))
                .AddResult(ConditionForm.All, true, ConditionResults.True).AddResultValue("a", new StringValue("aaa"))
                .AddResult(ConditionForm.None, false, ConditionResults.True)
                .AddResult(ConditionForm.None, true, ConditionResults.False).AddResultValue("a", new AnyValue())
                .Run();
        }

        [TestMethod]
        public void DirectVarEqualsInt()
        {
            TestCase.Create(new BinaryEx(Operations.Equal, new DirectVarUse(new Position(), new VariableName("a")), new IntLiteral(new Position(), 1)))
                .AddResult(ConditionForm.All, true, ConditionResults.True).AddResultValue("a", new IntegerValue(1))
                .AddResult(ConditionForm.None, false, ConditionResults.True)
                .AddResult(ConditionForm.None, true, ConditionResults.False).AddResultValue("a", new AnyValue())
                .Run();
        }
    }
}
