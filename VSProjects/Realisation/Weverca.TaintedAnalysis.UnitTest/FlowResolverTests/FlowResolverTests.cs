using System;
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
            TestCase.Create(new BinaryEx(Operations.Equal, new DirectVarUse(new Position(), new VariableName("a")), ValueFactory.Create("aaa")))
                .AddResult(ConditionForm.All, true, ConditionResults.True).AddResultValue("a", new StringValue("aaa"))
                .AddResult(ConditionForm.None, false, ConditionResults.True)
                .AddResult(ConditionForm.None, true, ConditionResults.False).AddResultValue("a", new AnyValue())
                .Run();
        }

        [TestMethod]
        public void DirectVarEqualsInt()
        {
            TestCase.Create(new BinaryEx(Operations.Equal, ValueFactory.Create(1), new DirectVarUse(new Position(), new VariableName("a"))))
                .AddResult(ConditionForm.All, true, ConditionResults.True).AddResultValue("a", new IntegerValue(1))
                .AddResult(ConditionForm.None, false, ConditionResults.True)
                .AddResult(ConditionForm.None, true, ConditionResults.False).AddResultValue("a", new AnyValue())
                .Run();
        }

        [TestMethod]
        public void DirectVarEqualsFloat()
        {
            TestCase.Create(new BinaryEx(Operations.Equal, new DirectVarUse(new Position(), new VariableName("a")), ValueFactory.Create(1.1)))
                .AddResult(ConditionForm.All, true, ConditionResults.True).AddResultValue("a", new FloatValue(1.1))
                .AddResult(ConditionForm.None, false, ConditionResults.True)
                .AddResult(ConditionForm.None, true, ConditionResults.False).AddResultValue("a", new AnyValue())
                .Run();
        }

        [TestMethod]
        public void DirectVarEqualsBool()
        {
            TestCase.Create(new BinaryEx(Operations.Equal, new DirectVarUse(new Position(), new VariableName("a")), ValueFactory.Create(true)))
                .AddResult(ConditionForm.All, true, ConditionResults.True).AddResultValue("a", new BooleanValue(true))
                .AddResult(ConditionForm.None, false, ConditionResults.True)
                .AddResult(ConditionForm.None, true, ConditionResults.False).AddResultValue("a", new AnyValue())
                .Run();
        }

        [TestMethod]
        public void DirectVarNotEqualsString()
        {
            TestCase.Create(new BinaryEx(Operations.NotEqual, new DirectVarUse(new Position(), new VariableName("a")), ValueFactory.Create("aaa")))
                .AddResult(ConditionForm.All, true, ConditionResults.True).AddResultValue("a", new AnyValue())
                .AddResult(ConditionForm.None, false, ConditionResults.True)
                .AddResult(ConditionForm.None, true, ConditionResults.False).AddResultValue("a", new StringValue("aaa"))
                .Run();
        }

        [TestMethod]
        public void DirectVarNotEqualsInt()
        {
            TestCase.Create(new BinaryEx(Operations.NotEqual, new DirectVarUse(new Position(), new VariableName("a")), ValueFactory.Create(1)))
                .AddResult(ConditionForm.All, true, ConditionResults.True).AddResultValue("a", new AnyValue())
                .AddResult(ConditionForm.None, false, ConditionResults.True)
                .AddResult(ConditionForm.None, true, ConditionResults.False).AddResultValue("a", new IntegerValue(1))
                .Run();
        }

        [TestMethod]
        public void DirectVarNotEqualsFloat()
        {
            TestCase.Create(new BinaryEx(Operations.NotEqual, new DirectVarUse(new Position(), new VariableName("a")), ValueFactory.Create(1.1)))
                .AddResult(ConditionForm.All, true, ConditionResults.True).AddResultValue("a", new AnyValue())
                .AddResult(ConditionForm.None, false, ConditionResults.True)
                .AddResult(ConditionForm.None, true, ConditionResults.False).AddResultValue("a", new FloatValue(1.1))
                .Run();
        }

        [TestMethod]
        public void DirectVarNotEqualsBool()
        {
            TestCase.Create(new BinaryEx(Operations.NotEqual, new DirectVarUse(new Position(), new VariableName("a")), ValueFactory.Create(true)))
                .AddResult(ConditionForm.All, true, ConditionResults.True).AddResultValue("a", new AnyValue())
                .AddResult(ConditionForm.None, false, ConditionResults.True)
                .AddResult(ConditionForm.None, true, ConditionResults.False).AddResultValue("a", new BooleanValue(true))
                .Run();
        }

        [TestMethod]
        public void DirectVarEqualsNull()
        {
            //TODO: "NullValue"
            
            //TestCase.Create(new BinaryEx(Operations.Equal, new DirectVarUse(new Position(), new VariableName("a")), new NullLiteral(new Position())))
            //    .AddResult(ConditionForm.All, true, ConditionResults.True).AddResultValue("a", new value)
            //    .AddResult(ConditionForm.None, false, ConditionResults.True)
            //    .AddResult(ConditionForm.None, true, ConditionResults.False).AddResultValue("a", new AnyValue())
            //    .Run();
        }

        [TestMethod]
        public void DirectTrue()
        {
            //TODO: pridat ruzne unarni operace
            
            TestCase.Create(new GlobalConstUse(new Position(), new QualifiedName(new Name("true")), null))
                .AddResult(ConditionForm.All, true, ConditionResults.True)
                .AddResult(ConditionForm.None, false, ConditionResults.True)
                .Run();
        }

        [TestMethod]
        public void BinaryArithmeticsExpression()
        {
            Tuple<Operations, Literal, Literal, Literal>[] tests = new Tuple<Operations, Literal, Literal, Literal>[]
            {
                new Tuple<Operations, Literal, Literal, Literal>(Operations.Add, ValueFactory.Create(2), ValueFactory.Create(3), ValueFactory.Create(5)),
                new Tuple<Operations, Literal, Literal, Literal>(Operations.Sub, ValueFactory.Create(5), ValueFactory.Create(3), ValueFactory.Create(2)),
                new Tuple<Operations, Literal, Literal, Literal>(Operations.Mul, ValueFactory.Create(2), ValueFactory.Create(3), ValueFactory.Create(6)),
                new Tuple<Operations, Literal, Literal, Literal>(Operations.Div, ValueFactory.Create(6), ValueFactory.Create(3), ValueFactory.Create(2)),
                new Tuple<Operations, Literal, Literal, Literal>(Operations.BitAnd, ValueFactory.Create(1), ValueFactory.Create(1), ValueFactory.Create(1)),
                new Tuple<Operations, Literal, Literal, Literal>(Operations.BitOr, ValueFactory.Create(1), ValueFactory.Create(2), ValueFactory.Create(1 & 2)),
                new Tuple<Operations, Literal, Literal, Literal>(Operations.BitXor, ValueFactory.Create(1), ValueFactory.Create(2), ValueFactory.Create(1 ^ 2)),
                new Tuple<Operations, Literal, Literal, Literal>(Operations.Mod, ValueFactory.Create(5), ValueFactory.Create(2), ValueFactory.Create(1)),
                new Tuple<Operations, Literal, Literal, Literal>(Operations.ShiftLeft, ValueFactory.Create(5), ValueFactory.Create(2), ValueFactory.Create(5 << 2)),
                new Tuple<Operations, Literal, Literal, Literal>(Operations.ShiftRight, ValueFactory.Create(5), ValueFactory.Create(2), ValueFactory.Create(5 >> 2)),
            };

            foreach (var test in tests)
            {
                TestCase.Create(
                    new BinaryEx(Operations.Equal,
                        new BinaryEx(test.Item1, test.Item2, test.Item3),
                        test.Item4))
                    .AddResult(ConditionForm.All, true, ConditionResults.True)
                    .AddResult(ConditionForm.None, false, ConditionResults.True)
                    .Run();
            }
        }
    }
}
