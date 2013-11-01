using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework.UnitTest
{
    /// <summary>
    /// Expression evaluation is resolved here
    /// </summary>
    internal class SimpleExpressionEvaluator : ExpressionEvaluatorBase
    {
        public override void Assign(ReadWriteSnapshotEntryBase target, MemoryEntry value)
        {
            target.WriteMemory(OutSnapshot, value);
        }

        public override ReadWriteSnapshotEntryBase ResolveField(ReadSnapshotEntryBase objectValue, VariableIdentifier field)
        {
            return objectValue.ReadField(OutSnapshot, field);
        }

        public override ReadWriteSnapshotEntryBase ResolveIndex(ReadSnapshotEntryBase arrayValue, MemberIdentifier index)
        {
            return arrayValue.ReadIndex(OutSnapshot, index);
        }

        public override void AliasAssign(ReadWriteSnapshotEntryBase target, ReadSnapshotEntryBase aliasedValue)
        {
            target.SetAliases(OutSnapshot, aliasedValue);
        }

        public override MemoryEntry ResolveIndexedVariable(VariableIdentifier entry)
        {
            if (!entry.IsDirect)
            {
                throw new NotImplementedException();
            }

            var array = OutSet.ReadValue(entry.DirectName);
            //NOTE there should be precise resolution of multiple values

            var arrayValue = array.PossibleValues.First();
            if (arrayValue is UndefinedValue)
            {
                //new array is implicitly created
                arrayValue = OutSet.CreateArray();
                array = new MemoryEntry(arrayValue);
                OutSet.Assign(entry.DirectName, array);
            }

            return array;
        }

        public override ReadWriteSnapshotEntryBase ResolveVariable(VariableIdentifier variable)
        {
            return OutSet.GetVariable(variable);
        }

        public override IEnumerable<string> VariableNames(MemoryEntry value)
        {
            //TODO convert all value types
            return from StringValue possible in value.PossibleValues select possible.Value;
        }

        public override MemoryEntry BinaryEx(MemoryEntry leftOperand, Operations operation, MemoryEntry rightOperand)
        {
            switch (operation)
            {
                case Operations.Equal:
                    return areEqual(leftOperand, rightOperand);
                case Operations.Add:
                    return add(leftOperand, rightOperand);
                case Operations.Sub:
                    return sub(leftOperand, rightOperand);
                default:
                    throw new NotImplementedException();
            }
        }

        public override MemoryEntry UnaryEx(Operations operation, MemoryEntry operand)
        {
            var result = new HashSet<IntegerValue>();
            switch (operation)
            {
                case Operations.Minus:
                    var negations = from IntegerValue number in operand.PossibleValues select Flow.OutSet.CreateInt(-number.Value);
                    result.UnionWith(negations);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return new MemoryEntry(result.ToArray());
        }

        public override MemoryEntry IncDecEx(IncDecEx operation, MemoryEntry incrementedValue)
        {
            var inc = operation.Inc ? 1 : -1;

            var values = new List<Value>();

            foreach (IntegerValue incremented in incrementedValue.PossibleValues)
            {
                var result = OutSet.CreateInt(incremented.Value + inc);
                values.Add(result);
            }

            return new MemoryEntry(values);
        }

        public override MemoryEntry ArrayEx(IEnumerable<KeyValuePair<MemoryEntry, MemoryEntry>> keyValuePairs)
        {
            throw new NotImplementedException();
        }

        #region Expression evaluation helpers

        private MemoryEntry add(MemoryEntry left, MemoryEntry right)
        {
            if (left.Count != 1 || right.Count != 1)
            {
                throw new NotImplementedException();
            }

            var leftValue = left.PossibleValues.First() as IntegerValue;
            var rightValue = right.PossibleValues.First() as IntegerValue;

            return new MemoryEntry(OutSet.CreateInt(leftValue.Value + rightValue.Value));
        }

        private MemoryEntry sub(MemoryEntry left, MemoryEntry right)
        {
            if (left.Count != 1 || right.Count != 1)
            {
                throw new NotImplementedException();
            }

            var leftValue = left.PossibleValues.First() as IntegerValue;
            var rightValue = right.PossibleValues.First() as IntegerValue;

            return new MemoryEntry(OutSet.CreateInt(leftValue.Value - rightValue.Value));
        }

        private void keepParentInfo(MemoryEntry parent, MemoryEntry child)
        {
            AnalysisTestUtils.CopyInfo(OutSet, parent, child);
        }

        private MemoryEntry areEqual(MemoryEntry left, MemoryEntry right)
        {
            var result = new List<BooleanValue>();
            if (canBeDifferent(left, right))
            {
                result.Add(OutSet.CreateBool(false));
            }

            if (canBeSame(left, right))
            {
                result.Add(OutSet.CreateBool(true));
            }

            return new MemoryEntry(result.ToArray());
        }

        private bool canBeSame(MemoryEntry left, MemoryEntry right)
        {
            if (containsAnyValue(left) || containsAnyValue(right))
            {
                return true;
            }

            foreach (var possibleValue in left.PossibleValues)
            {
                if (right.PossibleValues.Contains(possibleValue))
                {
                    return true;
                }
            }

            return false;
        }

        private bool canBeDifferent(MemoryEntry left, MemoryEntry right)
        {
            if (containsAnyValue(left) || containsAnyValue(right))
            {
                return true;
            }

            if (left.PossibleValues.Count() > 1 || left.PossibleValues.Count() > 1)
            {
                return true;
            }

            return !left.Equals(right);
        }

        private bool containsAnyValue(MemoryEntry entry)
        {
            //TODO Undefined value maybe is not correct to be treated as any value
            return entry.PossibleValues.Any((val) => val is AnyValue);
        }

        #endregion

        public override void Foreach(MemoryEntry enumeree, ReadWriteSnapshotEntryBase keyVariable, ReadWriteSnapshotEntryBase valueVariable)
        {
            var values = new HashSet<Value>();

            var array = enumeree.PossibleValues.First() as AssociativeArray;
            var indexes = OutSet.IterateArray(array);

            foreach (var index in indexes)
            {
                values.UnionWith(OutSet.GetIndex(array, index).PossibleValues);
            }

            valueVariable.WriteMemory(OutSnapshot, new MemoryEntry(values));
        }

        public override MemoryEntry Constant(GlobalConstUse x)
        {
            Value result;
            switch (x.Name.Name.Value)
            {
                case "true":
                    result = OutSet.CreateBool(true);
                    break;
                case "false":
                    result = OutSet.CreateBool(false);
                    break;
                default:
                    var constantName = ".constant_" + x.Name;
                    var constantVar = new VariableName(constantName);
                    OutSet.FetchFromGlobal(constantVar);
                    return OutSet.ReadValue(constantVar);
            }

            return new MemoryEntry(result);
        }

        public override void ConstantDeclaration(ConstantDecl x, MemoryEntry constantValue)
        {
            var constName = new VariableName(".constant_" + x.Name);
            OutSet.FetchFromGlobal(constName);
            OutSet.Assign(constName, constantValue);
        }

        public override MemoryEntry Concat(IEnumerable<MemoryEntry> parts)
        {
            var result = new StringBuilder();
            foreach (var part in parts)
            {
                if (part.Count != 1)
                {
                    throw new NotImplementedException();
                }

                var partValue = part.PossibleValues.First() as ScalarValue;
                result.Append(partValue.RawValue);
            }

            return new MemoryEntry(Flow.OutSet.CreateString(result.ToString()));
        }

        public override void Echo(EchoStmt echo, MemoryEntry[] values)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry CreateObject(QualifiedName typeName)
        {
            var types = OutSet.ResolveType(typeName);
            if (!types.GetEnumerator().MoveNext())
            {
                // TODO: If no type is resolved, exception should be thrown
                Debug.Fail("No type resolved");
            }

            var values = new List<ObjectValue>();
            foreach (var type in types)
            {
                var newObject = CreateInitializedObject(type);
                values.Add(newObject);
            }

            return new MemoryEntry(values);
        }

        public override MemberIdentifier MemberIdentifier(MemoryEntry memberRepresentation)
        {
            var possibleNames = new List<string>();
            foreach (var possibleMember in memberRepresentation.PossibleValues)
            {
                var value = possibleMember as ScalarValue;
                if (value == null)
                {
                    continue;
                }

                possibleNames.Add(value.RawValue.ToString());
            }

            return new MemberIdentifier(possibleNames);
        }

        public override void FieldAssign(ReadSnapshotEntryBase objectValue, VariableIdentifier targetField, MemoryEntry assignedValue)
        {
            throw new NotImplementedException();
        }

        public override void IndexAssign(ReadSnapshotEntryBase indexedValue, MemoryEntry index, MemoryEntry assignedValue)
        {
            throw new NotImplementedException();
        }
    }
}
