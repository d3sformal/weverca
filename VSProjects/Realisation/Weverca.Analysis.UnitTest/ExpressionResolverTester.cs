using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.UnitTest
{
    [TestClass]
    public class ExpressionEvaluatorTester
    {
        private static readonly QualifiedName standardClassName = new QualifiedName(new Name("stdClass"));

        private static readonly string inputVariableName = "inputVariable";

        private static readonly Operations[] unaryOperations = new Operations[]
        {
            Operations.Plus,
            Operations.Minus,
            Operations.LogicNegation,
            Operations.BitNegation,
            Operations.AtSign,
            Operations.Print,
            Operations.Clone,
            Operations.BoolCast,
            // Not supported: Operations.Int8Cast,
            // Not supported: Operations.Int16Cast,
            Operations.Int32Cast,
            // Not supported: Operations.Int64Cast,
            // Not supported: Operations.UInt8Cast,
            // Not supported: Operations.UInt16Cast,
            // Not supported: Operations.UInt32Cast,
            // Not supported: Operations.UInt64Cast,
            Operations.DoubleCast,
            Operations.FloatCast,
            // Not supported: Operations.DecimalCast,
            // String converter solves it: Operations.StringCast,
            // Not supported: Operations.BinaryCast,
            Operations.UnicodeCast,
            Operations.ObjectCast,
            Operations.ArrayCast,
            Operations.UnsetCast,
        };

        private static readonly string unaryOperationCode = @"
            $result = +$inputVariable;
            $result = -$inputVariable;
            $result = !$inputVariable;
            $result = ~$inputVariable;
            $result = @$inputVariable;
            $result = print $inputVariable;
            $result = clone $inputVariable;
            $result = (bool)$inputVariable;
            $result = (int)$inputVariable;
            $result = (double)$inputVariable;
            $result = (string)$inputVariable;
            $result = (array)$inputVariable;
            $result = (unset)$inputVariable;
        ";

        private static readonly Operations[] binaryOperations = new Operations[]
        {
            Operations.Equal,
            Operations.Identical,
            Operations.NotEqual,
            Operations.NotIdentical,
            Operations.LessThan,
            Operations.LessThanOrEqual,
            Operations.GreaterThan,
            Operations.GreaterThanOrEqual,
            Operations.Add,
            Operations.Sub,
            Operations.Mul,
            Operations.Div,
            Operations.Mod,
            Operations.And,
            Operations.Or,
            Operations.Xor,
            Operations.BitAnd,
            Operations.BitOr,
            Operations.BitXor,
            Operations.ShiftLeft,
            Operations.ShiftRight,
            Operations.Concat,
        };

        private static readonly string binaryOperationCode = @"
            $result = $inputVariable == $inputVariable;
            $result = $inputVariable === $inputVariable;
            $result = $inputVariable != $inputVariable;
            $result = $inputVariable !== $inputVariable;
            $result = $inputVariable < $inputVariable;
            $result = $inputVariable <= $inputVariable;
            $result = $inputVariable > $inputVariable;
            $result = $inputVariable >= $inputVariable;
            $result = $inputVariable + $inputVariable;
            $result = $inputVariable - $inputVariable;
            $result = $inputVariable * $inputVariable;
            $result = $inputVariable / $inputVariable;
            $result = $inputVariable % $inputVariable;
            $result = $inputVariable and $inputVariable;
            $result = $inputVariable or $inputVariable;
            $result = $inputVariable xor $inputVariable;
            $result = $inputVariable & $inputVariable;
            $result = $inputVariable | $inputVariable;
            $result = $inputVariable ^ $inputVariable;
            $result = $inputVariable << $inputVariable;
            $result = $inputVariable >> $inputVariable;
            $result = $inputVariable.$inputVariable;
        ";

        private static readonly string[] inputVariables = new string[]
        {
            "falseBooleanVariable",
            "trueBooleanVariable",
            "integerValue",
            "longintVariable",
            "floatVariable",
            "scientificFloatVariable",
            "stringVariable",
            "integerStringVariable",
            "objectVariable",
            "arrayVariable",
            // "resourceVariable",
            "undefinedVariable",
            "intervalIntegerValue",
            "intervalLongintVariable",
            "intervalFloatVariable",
            "anyValueVariable",
            "anyBooleanVariable",
            "anyIntegerValue",
            "anyLongintVariable",
            "anyFloatVariable",
            "anyStringVariable",
            "anyObjectVariable",
            "anyArrayVariable",
            "anyResourceVariable",
        };

        private static readonly Value[] inputValues = new Value[]
        {
            new BooleanValue(false),
            new BooleanValue(true),
            new IntegerValue(1618033),
            new LongintValue(-273L),
            new FloatValue(3.141592),
            new FloatValue(-271000000000000000000.0),
            new StringValue("Weverka"),
            new StringValue("256"),
            new ObjectValue(),
            new AssociativeArray(),
            // new ResourceValue(),
            new UndefinedValue(),
            new IntegerIntervalValue(-541, 954),
            new LongintIntervalValue(-789, -111),
            new FloatIntervalValue(1057.785, 2457.445),
            new AnyValue(),
            new AnyBooleanValue(),
            new AnyIntegerValue(),
            new AnyLongintValue(),
            new AnyFloatValue(),
            new AnyStringValue(),
            new AnyObjectValue(),
            new AnyArrayValue(),
            new AnyResourceValue(),
        };

        /// <summary>
        /// Input values that are used for testing binary operations
        /// </summary>
        private static readonly Value[] inputValuesBinary = new Value[]
        {
            new BooleanValue(false),
            new BooleanValue(true),
            new IntegerValue(1618033),
            //new LongintValue(-273L),
            new FloatValue(3.141592),
            new FloatValue(-271000000000000000000.0),
            new StringValue("Weverka"),
            new StringValue("256"),
            new ObjectValue(),
            new AssociativeArray(),
            // new ResourceValue(),
            new UndefinedValue(),
            new IntegerIntervalValue(-541, 954),
            //new LongintIntervalValue(-789, -111),
            new FloatIntervalValue(1057.785, 2457.445),
           
            new AnyValue(),
            new AnyBooleanValue(),
            new AnyIntegerValue(),
            new AnyLongintValue(),
            new AnyFloatValue(),
            new AnyStringValue(),
            new AnyObjectValue(),
            new AnyArrayValue(),
            new AnyResourceValue(),
        };

        private static readonly MemoryEntry inputEntry = new MemoryEntry(inputValues);
        private static readonly MemoryEntry inputEntryBinary = new MemoryEntry(inputValuesBinary);

        private static readonly Value[] stringConversionResults = new Value[]
        {
            new StringValue(string.Empty),
            new StringValue("1"),
            new StringValue("1618033"),
            new StringValue("-273"),
            new StringValue("3.141592"),
            new StringValue("-2.71E+20"),
            new StringValue("Weverka"),
            new StringValue("256"),
            // TODO: The object should return the result of __toString magic method
            new AnyStringValue(),
            new StringValue("Array"),
            // new ResourceValue(),
            new StringValue(string.Empty),
            new AnyStringValue(),
            new AnyStringValue(),
            new AnyStringValue(),
            // TODO: Any value can fail because of any objects
            new AnyStringValue(),
            new AnyStringValue(),
            new AnyStringValue(),
            new AnyStringValue(),
            new AnyStringValue(),
            new AnyStringValue(),
            // TODO: Any object does not need to have __toString magic method
            new AnyStringValue(),
            new StringValue("Array"),
            new AnyStringValue(),
        };

        private static readonly Value[] incrementResults = new Value[]
        {
            new BooleanValue(false),
            new BooleanValue(true),
            new IntegerValue(1618034),
            new LongintValue(-272L),
            new FloatValue(4.141592),
            new FloatValue(-271000000000000000000.0),
            // TODO: The operation depends on byte representation, not supported right now
            new AnyStringValue(),
            new AnyStringValue(),
            inputValues[8],
            inputValues[9],
            // new ResourceValue(),
            new IntegerValue(1),
            new IntegerIntervalValue(-540, 955),
            new LongintIntervalValue(-788, -110),
            new FloatIntervalValue(1058.785, 2458.445),
            new AnyValue(),
            new AnyBooleanValue(),
            new AnyValue(),
            new AnyValue(),
            new AnyFloatValue(),
            new AnyStringValue(),
            new AnyObjectValue(),
            new AnyArrayValue(),
            new AnyResourceValue(),
        };

        private static readonly Value[] decrementResults = new Value[]
        {
            new BooleanValue(false),
            new BooleanValue(true),
            new IntegerValue(1618032),
            new LongintValue(-274L),
            new FloatValue(2.141592),
            new FloatValue(-271000000000000000000.0),
            new StringValue("Weverka"),
            new StringValue("256"),
            inputValues[8],
            inputValues[9],
            // new ResourceValue(),
            new UndefinedValue(),
            new IntegerIntervalValue(-542, 953),
            new LongintIntervalValue(-790, -112),
            new FloatIntervalValue(1056.785, 2456.445),
            new AnyValue(),
            new AnyBooleanValue(),
            new AnyValue(),
            new AnyValue(),
            new AnyFloatValue(),
            new AnyStringValue(),
            new AnyObjectValue(),
            new AnyArrayValue(),
            new AnyResourceValue(),
        };

        [TestMethod]
        public void StringConverter()
        {
            TestEvaluationResults("${0} = (string)${0};\n", stringConversionResults);
        }

        /// <summary>
        /// Tests whether the increment of all variables inputVariables with values inputValues results
        /// in values results.
        /// </summary>
        [TestMethod]
        public void IncrementEvaluation()
        {
            TestEvaluationResults("++${0};\n", incrementResults);
        }

        /// <summary>
        /// Tests whether the decrement of all variables inputVariables with values inputValues results
        /// in values results.
        /// </summary>
        [TestMethod]
        public void DecrementEvaluation()
        {
            TestEvaluationResults("--${0};\n", decrementResults);
        }

        /// <summary>
        /// Tests whether all operations defined in unaryOperationCode are supported
        /// with all values in inputValues.
        /// Note that this test tests only whether all operations are supported, does not test whether
        /// the results are correct.
        /// </summary>
        [TestMethod]
        public void AreUnaryOperationsImplemented()
        {
            TestEvaluation(unaryOperationCode, inputEntry);
        }

        /// <summary>
        /// Tests whether all operations defined in binaryOperationCode are supported
        /// with all values in inputValuesBinary.
        /// Note that this test tests only whether all operations are supported, does not test whether
        /// the results are correct.
        /// </summary>
        [TestMethod]
        public void AreBinaryOperationsImplemented()
        {
            TestEvaluation(binaryOperationCode, inputEntryBinary);
        }

        [TestMethod]
        public void ModuloOperationEvaluator()
        {
            var dividends = new Value[]
            {
                new IntegerIntervalValue(2, 17),
                new IntegerIntervalValue(9, 14),
                new IntegerIntervalValue(94, 113),
                new IntegerIntervalValue(98, 105),
                new IntegerIntervalValue(228, 239),
                new IntegerIntervalValue(517, 530),
                new IntegerIntervalValue(7, 158),
                new IntegerIntervalValue(123, 598),
                new IntegerIntervalValue(16, 16),
                new IntegerIntervalValue(498, 498),
                new IntegerIntervalValue(13, 1271),
                new IntegerIntervalValue(-17, -2),
                new IntegerIntervalValue(-14, -9),
                new IntegerIntervalValue(-113, -94),
                new IntegerIntervalValue(-105, -98),
                new IntegerIntervalValue(-239, -228),
                new IntegerIntervalValue(-530, -517),
                new IntegerIntervalValue(-158, -7),
                new IntegerIntervalValue(-598, -123),
                new IntegerIntervalValue(-10, -10),
                new IntegerIntervalValue(-339, -339),
                new IntegerIntervalValue(-1271, -13),
                new IntegerIntervalValue(int.MinValue, -723),
                new IntegerIntervalValue(int.MinValue, int.MinValue),
                new IntegerIntervalValue(-17, 9),
                new IntegerIntervalValue(-14, 2),
                new IntegerIntervalValue(-7, 98),
                new IntegerIntervalValue(-12, 94),
                new IntegerIntervalValue(-239, 18),
                new IntegerIntervalValue(-530, 27),
                new IntegerIntervalValue(-347, 789),
                new IntegerIntervalValue(-568, 478),
                new IntegerIntervalValue(-2547, 1271),
                new IntegerIntervalValue(int.MinValue, int.MaxValue),
                new IntegerValue(0),
                new IntegerValue(13),
                new IntegerValue(-13),
                new IntegerValue(13),
                new IntegerValue(-13),
                new IntegerValue(48),
                new IntegerValue(17),
                new IntegerValue(-17),
                new IntegerValue(17),
                new IntegerValue(-17),
                new IntegerValue(31),
                new IntegerValue(-31),
                new IntegerValue(31),
                new IntegerValue(-31),
                new IntegerValue(31),
                new IntegerValue(-31),
                new IntegerValue(31),
                new IntegerValue(-31),
                new IntegerValue(int.MinValue),
                new IntegerValue(int.MinValue),
                new IntegerValue(int.MinValue),
                new IntegerValue(0),
            };

            var divisors = new Value[]
            {
                new IntegerValue(67),
                new IntegerValue(-67),
                new IntegerValue(29),
                new IntegerValue(-29),
                new IntegerValue(29),
                new IntegerValue(-29),
                new IntegerValue(23),
                new IntegerValue(-23),
                new IntegerValue(1568),
                new IntegerValue(-1568),
                new IntegerValue(int.MinValue),
                new IntegerValue(67),
                new IntegerValue(-67),
                new IntegerValue(29),
                new IntegerValue(-29),
                new IntegerValue(29),
                new IntegerValue(-29),
                new IntegerValue(23),
                new IntegerValue(-23),
                new IntegerValue(1568),
                new IntegerValue(-1568),
                new IntegerValue(int.MinValue),
                new IntegerValue(int.MinValue),
                new IntegerValue(int.MinValue),
                new IntegerValue(67),
                new IntegerValue(-67),
                new IntegerValue(29),
                new IntegerValue(-29),
                new IntegerValue(29),
                new IntegerValue(-29),
                new IntegerValue(51),
                new IntegerValue(-51),
                new IntegerValue(int.MinValue),
                new IntegerValue(int.MinValue),
                new IntegerIntervalValue(15, 4785),
                new IntegerIntervalValue(28, 478),
                new IntegerIntervalValue(28, 478),
                new IntegerIntervalValue(-658, -41),
                new IntegerIntervalValue(-658, -41),
                new IntegerIntervalValue(-12, 17),
                new IntegerIntervalValue(7, 26),
                new IntegerIntervalValue(7, 26),
                new IntegerIntervalValue(-26, -7),
                new IntegerIntervalValue(-26, -7),
                new IntegerIntervalValue(4, 25),
                new IntegerIntervalValue(4, 25),
                new IntegerIntervalValue(-25, -4),
                new IntegerIntervalValue(-25, -4),
                new IntegerIntervalValue(4, 12),
                new IntegerIntervalValue(4, 12),
                new IntegerIntervalValue(-12, -4),
                new IntegerIntervalValue(-12, -4),
                new IntegerIntervalValue(-246, 547),
                new IntegerIntervalValue(157, 5478),
                new IntegerIntervalValue((int.MinValue / 2) - 100, -5478),
                new IntegerIntervalValue(5, 1457),
            };

            var results = new Value[]
            {
                new IntegerIntervalValue(2, 17),
                new IntegerIntervalValue(9, 14),
                new IntegerIntervalValue(7, 26),
                new IntegerIntervalValue(11, 18),
                new IntegerIntervalValue(0, 28),
                new IntegerIntervalValue(0, 28),
                new IntegerIntervalValue(0, 22),
                new IntegerIntervalValue(0, 22),
                new IntegerValue(16),
                new IntegerValue(498),
                new IntegerIntervalValue(13, 1271),
                new IntegerIntervalValue(-17, -2),
                new IntegerIntervalValue(-14, -9),
                new IntegerIntervalValue(-26, -7),
                new IntegerIntervalValue(-18, -11),
                new IntegerIntervalValue(-28, 0),
                new IntegerIntervalValue(-28, 0),
                new IntegerIntervalValue(-22, 0),
                new IntegerIntervalValue(-22, 0),
                new IntegerValue(-10),
                new IntegerValue(-339),
                new IntegerIntervalValue(-1271, -13),
                new IntegerIntervalValue(int.MinValue + 1, 0),
                new IntegerValue(0),
                new IntegerIntervalValue(-17, 9),
                new IntegerIntervalValue(-14, 2),
                new IntegerIntervalValue(-7, 28),
                new IntegerIntervalValue(-12, 28),
                new IntegerIntervalValue(-28, 18),
                new IntegerIntervalValue(-28, 27),
                new IntegerIntervalValue(-50, 50),
                new IntegerIntervalValue(-50, 50),
                new IntegerIntervalValue(-2547, 1271),
                new IntegerIntervalValue(int.MinValue + 1, int.MaxValue),
                new IntegerValue(0),
                new IntegerValue(13),
                new IntegerValue(-13),
                new IntegerValue(13),
                new IntegerValue(-13),
                new AnyValue(),
                new IntegerIntervalValue(0, 17),
                new IntegerIntervalValue(-17, 0),
                new IntegerIntervalValue(0, 17),
                new IntegerIntervalValue(-17, 0),
                new IntegerIntervalValue(0, 15),
                new IntegerIntervalValue(-15, 0),
                new IntegerIntervalValue(0, 15),
                new IntegerIntervalValue(-15, 0),
                new IntegerIntervalValue(0, 11),
                new IntegerIntervalValue(-11, 0),
                new IntegerIntervalValue(0, 11),
                new IntegerIntervalValue(-11, 0),
                new AnyValue(),
                new IntegerIntervalValue(-5477, 0),
                new IntegerIntervalValue((int.MinValue / 2) + 1, 0),
                new IntegerValue(0),
            };

            TestEvaluationResults("$result{0} = $left{0} % $right{0};\n", dividends, divisors, results);
        }

        private static void TestEvaluationResults(string pattern, Value[] results)
        {
            var code = GenerateProgramCode(pattern);
            var analysis = TestUtils.GenerateForwardAnalysis(code);
            SetInputArray(analysis.EntryInput);

            var ppg = TestUtils.GeneratePpg(analysis);
            var outSet = TestUtils.GetResultOutputSet(ppg);

            TestVariableResults(outSet, results, GetInputVariableName);
        }

        private static void TestEvaluationResults(string pattern, Value[] leftOperands,
            Value[] rightOperands, Value[] results)
        {
            Debug.Assert(leftOperands.Length == rightOperands.Length,
                "Number of left and right operands is the same");
            Debug.Assert(leftOperands.Length == results.Length,
                "Number of operands and results is the same");

            var code = GenerateProgramCode(pattern, results.Length);
            var analysis = TestUtils.GenerateForwardAnalysis(code);
            SetInputOperands(analysis.EntryInput, leftOperands, rightOperands);

            var ppg = TestUtils.GeneratePpg(analysis);
            var outSet = TestUtils.GetResultOutputSet(ppg);

            TestVariableResults(outSet, results, GenerateResultVariableName);
        }

        private static void TestEvaluation(string code, MemoryEntry testInputEntry)
        {
            var analysis = TestUtils.GenerateForwardAnalysis(code);

            var identifier = new VariableIdentifier(inputVariableName);
            var snapshotEntry = analysis.EntryInput.GetVariable(identifier, true);
            snapshotEntry.WriteMemory(analysis.EntryInput.Snapshot, testInputEntry);

            // Do not test results, just test whether all operations are supported
            var ppg = TestUtils.GeneratePpg(analysis);
        }

        private static string GenerateProgramCode(string pattern)
        {
            var builder = new StringBuilder();

            foreach (var name in inputVariables)
            {
                builder.AppendFormat(pattern, name);
            }

            return builder.ToString();
        }

        private static string GenerateProgramCode(string pattern, int lineCount)
        {
            Debug.Assert(lineCount > 0, "We must generate at least one code line");

            var builder = new StringBuilder();

            for (var i = 0; i < lineCount; ++i)
            {
                builder.AppendFormat(pattern, i);
            }

            return builder.ToString();
        }

        private static void SetInputArray(FlowOutputSet entryInput)
        {
            for (var i = 0; i < inputVariables.Length; ++i)
            {
                var identifier = new VariableIdentifier(inputVariables[i]);
                var snapshotEntry = entryInput.GetVariable(identifier, true);
                snapshotEntry.WriteMemoryWithoutCopy(entryInput.Snapshot, new MemoryEntry(inputValues[i]));
            }
        }

        private static void SetInputOperands(FlowOutputSet entryInput, Value[] leftOperands,
            Value[] rightOperands)
        {
            Debug.Assert(leftOperands.Length == rightOperands.Length,
                "Number of left and right operands is the same");

            for (var i = 0; i < leftOperands.Length; ++i)
            {
                var identifier = new VariableIdentifier(string.Concat("left", i.ToString()));
                var snapshotEntry = entryInput.GetVariable(identifier, true);
                snapshotEntry.WriteMemoryWithoutCopy(entryInput.Snapshot, new MemoryEntry(leftOperands[i]));

                identifier = new VariableIdentifier(string.Concat("right", i.ToString()));
                snapshotEntry = entryInput.GetVariable(identifier, true);
                snapshotEntry.WriteMemoryWithoutCopy(entryInput.Snapshot, new MemoryEntry(rightOperands[i]));
            }
        }

        private static void TestVariableResults(FlowOutputSet outSet, Value[] results,
            Func<int, string> nameGenerator)
        {
            for (var i = 0; i < results.Length; ++i)
            {
                var identifier = new VariableIdentifier(nameGenerator(i));
                var snapshotEntry = outSet.GetVariable(identifier, true);

                var entry = snapshotEntry.ReadMemory(outSet.Snapshot);
                var enumerator = entry.PossibleValues.GetEnumerator();
                enumerator.MoveNext();
                var value = enumerator.Current as Value;

                Assert.AreEqual(results[i], value);
            }
        }

        private static string GetInputVariableName(int i)
        {
            Debug.Assert((i >= 0) && (i < inputVariables.Length),
                "Parameter must be an index into input variables array");

            return inputVariables[i];
        }

        private static string GenerateResultVariableName(int i)
        {
            return string.Concat("result", i.ToString());
        }
    }
}
