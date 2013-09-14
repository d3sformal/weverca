using System;
using System.Collections.Generic;
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
        // variable value, expected variable value in assumed block
        static List<Tuple<Literal, Value>> equalTests = new List<Tuple<Literal, Value>>();

        static FlowResolverTests()
        {
            equalTests.Add(LiteralValueFactory.Create("aaa"));
            equalTests.Add(LiteralValueFactory.Create(1));
            equalTests.Add(LiteralValueFactory.Create(1L));
            equalTests.Add(LiteralValueFactory.Create(1.1));
            equalTests.Add(LiteralValueFactory.Create(true));
        }
        
        [TestMethod]
        public void DirectVarEquals()
        {
            string variableName = "a";

            foreach (var test in equalTests)
            {
                TestCase.Create(new BinaryEx(Operations.Equal, new DirectVarUse(new Position(), new VariableName(variableName)), test.Item1))
                    .AddResult(ConditionForm.All, true, ConditionResults.True).AddResultValue(variableName, test.Item2)
                    .AddResult(ConditionForm.None, false, ConditionResults.True)
                    .AddResult(ConditionForm.None, true, ConditionResults.False).AddResultValue(variableName, new AnyValue())
                    .Run();

                //flipped order: var == value --> value == var
                TestCase.Create(new BinaryEx(Operations.Equal, test.Item1, new DirectVarUse(new Position(), new VariableName(variableName))))
                    .AddResult(ConditionForm.All, true, ConditionResults.True).AddResultValue(variableName, test.Item2)
                    .AddResult(ConditionForm.None, false, ConditionResults.True)
                    .AddResult(ConditionForm.None, true, ConditionResults.False).AddResultValue(variableName, new AnyValue())
                    .Run();
            }
        }

        [TestMethod]
        public void DirectVarNotEquals()
        {
            string variableName = "a";

            foreach (var test in equalTests)
            {
                TestCase.Create(new BinaryEx(Operations.NotEqual, new DirectVarUse(new Position(), new VariableName(variableName)), test.Item1))
                    .AddResult(ConditionForm.All, true, ConditionResults.True).AddResultValue("a", new AnyValue())
                    .AddResult(ConditionForm.None, false, ConditionResults.True)
                    .AddResult(ConditionForm.None, true, ConditionResults.False).AddResultValue("a", test.Item2)
                    .Run();

                //flipped order: var != value --> value != var
                TestCase.Create(new BinaryEx(Operations.NotEqual, test.Item1, new DirectVarUse(new Position(), new VariableName(variableName))))
                    .AddResult(ConditionForm.All, true, ConditionResults.True).AddResultValue("a", new AnyValue())
                    .AddResult(ConditionForm.None, false, ConditionResults.True)
                    .AddResult(ConditionForm.None, true, ConditionResults.False).AddResultValue("a", test.Item2)
                    .Run();
            }
        }

        [TestMethod]
        public void DirectVarEqualsNull()
        {
            //TODO: Is that proper null?
            TestCase.Create(new BinaryEx(Operations.Equal, new DirectVarUse(new Position(), new VariableName("a")), new NullLiteral(new Position())))
                .AddResult(ConditionForm.All, true, ConditionResults.True).AddResultValue("a", new UndefinedValue())
                .AddResult(ConditionForm.None, false, ConditionResults.True)
                .AddResult(ConditionForm.None, true, ConditionResults.False).AddResultValue("a", new AnyValue())
                .Run();
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
        public void BinaryArithmeticsExpressions()
        {
            //BinaryEx.Operator, left operane, right operand, result of the operation
            Tuple<Operations, Literal, Literal, Literal>[] tests = new Tuple<Operations, Literal, Literal, Literal>[]
            {
                new Tuple<Operations, Literal, Literal, Literal>(Operations.Add, LiteralFactory.Create(2), LiteralFactory.Create(3), LiteralFactory.Create(5)),
                new Tuple<Operations, Literal, Literal, Literal>(Operations.Sub, LiteralFactory.Create(5), LiteralFactory.Create(3), LiteralFactory.Create(2)),
                new Tuple<Operations, Literal, Literal, Literal>(Operations.Mul, LiteralFactory.Create(2), LiteralFactory.Create(3), LiteralFactory.Create(6)),
                new Tuple<Operations, Literal, Literal, Literal>(Operations.Div, LiteralFactory.Create(6), LiteralFactory.Create(3), LiteralFactory.Create(2)),
                new Tuple<Operations, Literal, Literal, Literal>(Operations.BitAnd, LiteralFactory.Create(1), LiteralFactory.Create(1), LiteralFactory.Create(1)),
                new Tuple<Operations, Literal, Literal, Literal>(Operations.BitOr, LiteralFactory.Create(1), LiteralFactory.Create(2), LiteralFactory.Create(1 & 2)),
                new Tuple<Operations, Literal, Literal, Literal>(Operations.BitXor, LiteralFactory.Create(1), LiteralFactory.Create(2), LiteralFactory.Create(1 ^ 2)),
                new Tuple<Operations, Literal, Literal, Literal>(Operations.Mod, LiteralFactory.Create(5), LiteralFactory.Create(2), LiteralFactory.Create(1)),
                new Tuple<Operations, Literal, Literal, Literal>(Operations.ShiftLeft, LiteralFactory.Create(5), LiteralFactory.Create(2), LiteralFactory.Create(5 << 2)),
                new Tuple<Operations, Literal, Literal, Literal>(Operations.ShiftRight, LiteralFactory.Create(5), LiteralFactory.Create(2), LiteralFactory.Create(5 >> 2))
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

        [TestMethod]
        public void NonequelityExpressions()
        {
            //BinaryEx.Operation, boudary (like a > boundary), positiveResult, negativeResult
            Tuple<Operations, Literal, Value, Value>[] tests = new Tuple<Operations, Literal, Value, Value>[]
            {
                new Tuple<Operations, Literal, Value, Value>(Operations.GreaterThan, LiteralFactory.Create(2), new IntegerIntervalValue(3, int.MaxValue), new IntegerIntervalValue(int.MinValue, 2)),
                new Tuple<Operations, Literal, Value, Value>(Operations.GreaterThanOrEqual, LiteralFactory.Create(2), new IntegerIntervalValue(2, int.MaxValue), new IntegerIntervalValue(int.MinValue, 1)),
                new Tuple<Operations, Literal, Value, Value>(Operations.LessThan, LiteralFactory.Create(2), new IntegerIntervalValue(int.MinValue, 1), new IntegerIntervalValue(2, int.MaxValue)),
                new Tuple<Operations, Literal, Value, Value>(Operations.LessThanOrEqual, LiteralFactory.Create(2), new IntegerIntervalValue(int.MinValue, 2), new IntegerIntervalValue(3, int.MaxValue)),

                new Tuple<Operations, Literal, Value, Value>(Operations.GreaterThan, LiteralFactory.Create(2L), new LongintIntervalValue(3, long.MaxValue), new LongintIntervalValue(long.MinValue, 2)),
                new Tuple<Operations, Literal, Value, Value>(Operations.GreaterThanOrEqual, LiteralFactory.Create(2L), new LongintIntervalValue(2, long.MaxValue), new LongintIntervalValue(long.MinValue, 1)),
                new Tuple<Operations, Literal, Value, Value>(Operations.LessThan, LiteralFactory.Create(2L), new LongintIntervalValue(long.MinValue, 1), new LongintIntervalValue(2, long.MaxValue)),
                new Tuple<Operations, Literal, Value, Value>(Operations.LessThanOrEqual, LiteralFactory.Create(2L), new LongintIntervalValue(long.MinValue, 2), new LongintIntervalValue(3, long.MaxValue)),
                
                new Tuple<Operations, Literal, Value, Value>(Operations.GreaterThan, LiteralFactory.Create(2.2), new FloatIntervalValue(2.2 + double.Epsilon, double.MaxValue), new FloatIntervalValue(double.MinValue, 2.2)),
                new Tuple<Operations, Literal, Value, Value>(Operations.GreaterThanOrEqual, LiteralFactory.Create(2.2), new FloatIntervalValue(2.2, double.MaxValue), new FloatIntervalValue(double.MinValue, 2.2 - double.Epsilon)),
                new Tuple<Operations, Literal, Value, Value>(Operations.LessThan, LiteralFactory.Create(2.2), new FloatIntervalValue(double.MinValue, 2.2 - double.Epsilon), new FloatIntervalValue(2.2, double.MaxValue)),
                new Tuple<Operations, Literal, Value, Value>(Operations.LessThanOrEqual, LiteralFactory.Create(2.2), new FloatIntervalValue(double.MinValue, 2.2), new FloatIntervalValue(2.2 + double.Epsilon, double.MaxValue)),

                new Tuple<Operations, Literal, Value, Value>(Operations.GreaterThan, LiteralFactory.Create("aaa"), new AnyStringValue(), new AnyStringValue()),
                new Tuple<Operations, Literal, Value, Value>(Operations.GreaterThanOrEqual, LiteralFactory.Create("aaa"), new AnyStringValue(), new AnyStringValue()),
                new Tuple<Operations, Literal, Value, Value>(Operations.LessThan, LiteralFactory.Create("aaa"), new AnyStringValue(), new AnyStringValue()),
                new Tuple<Operations, Literal, Value, Value>(Operations.LessThanOrEqual, LiteralFactory.Create("aaa"), new AnyStringValue(), new AnyStringValue())
            };

            string variableName = "a";

            foreach (var test in tests)
            {
                TestCase.Create(new BinaryEx(test.Item1, new DirectVarUse(new Position(), new VariableName(variableName)), test.Item2))
                    .AddResult(ConditionForm.All, true, ConditionResults.True).AddResultValue(variableName, test.Item3)
                    .AddResult(ConditionForm.None, false, ConditionResults.True)
                    .AddResult(ConditionForm.None, true, ConditionResults.False).AddResultValue(variableName, test.Item4)
                    .Run();

                //flipped
                TestCase.Create(new BinaryEx(test.Item1, test.Item2, new DirectVarUse(new Position(), new VariableName(variableName))))
                    .AddResult(ConditionForm.All, true, ConditionResults.True).AddResultValue(variableName, test.Item3)
                    .AddResult(ConditionForm.None, false, ConditionResults.True)
                    .AddResult(ConditionForm.None, true, ConditionResults.False).AddResultValue(variableName, test.Item4)
                    .Run();
            }
        }

        [TestMethod]
        public void ComposedNonequelityExpressions()
        {
            // a > 5 && a < 10
            // a > 5 || a == "ahoj"
        }

        [TestMethod]
        public void MultiVariableNonequelityExpressions()
        {
            // a > b
            // a > 5 && b < 7
        }

        [TestMethod]
        public void FunctionCallExpressions()
        {
            // if(a = fnc(...))
            // if (isSet(a))
        }

        [TestMethod]
        public void AssumeUnary()
        {
            // if !(...)
            //TODO: run all tests with negation
            string variableName = "a";
            foreach (var test in equalTests)
            {
                TestCase.Create(new UnaryEx(Operations.LogicNegation, new BinaryEx(Operations.Equal, new DirectVarUse(new Position(), new VariableName(variableName)), test.Item1)))
                        .AddResult(ConditionForm.All, true, ConditionResults.True).AddResultValue("a", new AnyValue())
                        .AddResult(ConditionForm.None, false, ConditionResults.True)
                        .AddResult(ConditionForm.None, true, ConditionResults.False).AddResultValue("a", test.Item2)
                        .Run();
            }
        }

        [TestMethod]
        public void AdvancedArithmeticsExpressions()
        {
            // a + 3 < 12
            // if (a + 5 < b)
            // if (a + b < 12)
            // a * a > 5
            // a.b == "ahoj"
        }

        [TestMethod]
        public void IndirectVariableUseExpressions()
        {
            // $a[1]
            // $a[$b]
            // $a->b
            // $a::b
        }
    }
}
