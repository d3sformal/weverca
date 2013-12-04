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

        /* TODO: Not used for now
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
         */

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

        private static readonly MemoryEntry inputEntry = new MemoryEntry(inputValues);

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

        [TestMethod]
        public void IncrementEvaluation()
        {
            TestEvaluationResults("++${0};\n", incrementResults);
        }

        [TestMethod]
        public void DecrementEvaluation()
        {
            TestEvaluationResults("--${0};\n", decrementResults);
        }

        [TestMethod]
        public void UnaryOperationEvaluator()
        {
            TestEvaluation(unaryOperationCode);
        }

        private static void TestEvaluationResults(string pattern, Value[] results)
        {
            var code = GenerateProgramCode(pattern);
            var analysis = TestUtils.GenerateForwardAnalysis(code);
            SetInputArray(analysis.EntryInput);

            var ppg = TestUtils.GeneratePpg(analysis);
            var outSet = TestUtils.GetResultOutputSet(ppg);

            TestVariableResults(outSet, results);
        }

        private static void TestEvaluation(string code)
        {
            var analysis = TestUtils.GenerateForwardAnalysis(code);

            var identifier = new VariableIdentifier(inputVariableName);
            var snapshotEntry = analysis.EntryInput.GetVariable(identifier, true);
            snapshotEntry.WriteMemory(analysis.EntryInput.Snapshot, inputEntry);

            // Do not test results, just test whether all operations are supported
            var ppg = TestUtils.GeneratePpg(analysis);
        }

        private static string GenerateProgramCode(string pattern)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var name in inputVariables)
            {
                builder.AppendFormat(pattern, name);
            }

            return builder.ToString();
        }

        private static void SetInputArray(FlowOutputSet entryInput)
        {
            for (var i = 0; i < inputVariables.Length; ++i)
            {
                var identifier = new VariableIdentifier(inputVariables[i]);
                var snapshotEntry = entryInput.GetVariable(identifier, true);
                snapshotEntry.WriteMemory(entryInput.Snapshot, new MemoryEntry(inputValues[i]));
            }
        }

        private static void TestVariableResults(FlowOutputSet outSet, Value[] results)
        {
            for (var i = 0; i < inputVariables.Length; ++i)
            {
                var identifier = new VariableIdentifier(inputVariables[i]);
                var snapshotEntry = outSet.GetVariable(identifier, true);

                var entry = snapshotEntry.ReadMemory(outSet.Snapshot);
                var enumerator = entry.PossibleValues.GetEnumerator();
                enumerator.MoveNext();
                var value = enumerator.Current as Value;

                Assert.AreEqual(results[i], value);
            }
        }
    }
}
