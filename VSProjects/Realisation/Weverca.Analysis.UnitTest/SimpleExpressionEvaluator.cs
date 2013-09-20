using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.ProgramPoints;
using Weverca.Analysis.Expressions;
using Weverca.Analysis.Memory;

namespace Weverca.Analysis.UnitTest
{
    /// <summary>
    /// Expression evaluation is resolved here
    /// </summary>
    class SimpleExpressionEvaluator : ExpressionEvaluatorBase
    {
        public override void Assign(VariableEntry target, MemoryEntry value)
        {
            if (target.IsDirect)
            {
                OutSet.Assign(target.DirectName, value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void FieldAssign(MemoryEntry objectValue, VariableEntry targetField, MemoryEntry value)
        {
            if (!targetField.IsDirect)
            {
                throw new NotImplementedException();
            }

            var index = OutSet.CreateIndex(targetField.DirectName.Value);
            foreach (ObjectValue obj in objectValue.PossibleValues)
            {
                OutSet.SetField(obj, index, value);
            }
        }

        public override MemoryEntry ResolveField(MemoryEntry objectValue, VariableEntry field)
        {
            if (!field.IsDirect || objectValue.PossibleValues.Count() != 1)
            {
                throw new NotImplementedException();
            }

            var obj = objectValue.PossibleValues.First() as ObjectValue;
            var index = OutSet.CreateIndex(field.DirectName.Value);
            return OutSet.GetField(obj, index);
        }

        public override MemoryEntry ResolveIndex(MemoryEntry array, MemoryEntry index)
        {
            if (index.PossibleValues.Count() != 1)
            {
                throw new NotImplementedException();
            }

            var values = new HashSet<Value>();
            var indexValue = index.PossibleValues.First() as PrimitiveValue;
            var containerIndex = OutSet.CreateIndex(indexValue.RawValue.ToString());

            foreach (var arrayValue in array.PossibleValues)
            {
                if (arrayValue is AssociativeArray)
                {

                    var possibleIndexValues = OutSet.GetIndex((AssociativeArray)arrayValue, containerIndex).PossibleValues;
                    values.UnionWith(possibleIndexValues);
                }
                else
                {
                    values.Add(OutSet.AnyValue);
                }
            }

            var result = new MemoryEntry(values.ToArray());
            keepParentInfo(array, result);

            return result;
        }

        public override MemoryEntry ResolveIndexedVariable(VariableEntry entry)
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

        public override void IndexAssign(MemoryEntry array, MemoryEntry index, MemoryEntry assignedValue)
        {
            if (array.PossibleValues.Count() != 1 && index.PossibleValues.Count() != 1)
            {
                throw new NotImplementedException();
            }

            var arrayValue = array.PossibleValues.First();
            var indexValue = index.PossibleValues.First() as PrimitiveValue;



            var containerIndex = OutSet.CreateIndex(indexValue.RawValue.ToString());

            OutSet.SetIndex(arrayValue as AssociativeArray, containerIndex, assignedValue);
        }

        public override void AliasAssign(VariableEntry target, IEnumerable<AliasValue> alias)
        {
            if (alias.Count() != 1)
            {
                throw new NotImplementedException();
            }

            if (target.IsDirect)
            {
                OutSet.Assign(target.DirectName, alias.First());
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override MemoryEntry ResolveVariable(VariableEntry variable)
        {
            var values = new HashSet<Value>();
            foreach (var varName in variable.PossibleNames)
            {
                values.UnionWith(OutSet.ReadValue(varName).PossibleValues);
            }

            return new MemoryEntry(values); ;
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

        public override MemoryEntry ArrayEx(IEnumerable<KeyValuePair<MemoryEntry, MemoryEntry>> keyValuePairs)
        {
            throw new NotImplementedException();
        }

        #region Expression evaluation helpers

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
                return true;

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
                return true;

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

        public override IEnumerable<AliasValue> ResolveAliasedField(MemoryEntry objectValue, VariableEntry aliasedField)
        {
            throw new NotImplementedException();
        }

        public override void AliasedFieldAssign(MemoryEntry objectValue, VariableEntry fieldEntry, IEnumerable<AliasValue> possibleAliasses)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<AliasValue> ResolveAliasedIndex(MemoryEntry arrayValue, MemoryEntry aliasedIndex)
        {
            foreach (AssociativeArray array in arrayValue.PossibleValues)
            {
                foreach (PrimitiveValue indexValue in aliasedIndex.PossibleValues)
                {
                    var index=OutSet.CreateIndex(indexValue.RawValue.ToString());
                    yield return OutSet.CreateIndexAlias(array, index);
                }
            }
        }

        public override void AliasedIndexAssign(MemoryEntry arrayValue, MemoryEntry aliasedIndex, IEnumerable<AliasValue> possibleAliases)
        {
            throw new NotImplementedException();
        }

        public override void Foreach(MemoryEntry enumeree, VariableEntry keyVariable, VariableEntry valueVariable)
        {
            if (valueVariable.IsDirect)
            {
                var values = new HashSet<Value>();

                var array = enumeree.PossibleValues.First() as AssociativeArray;
                var indexes = OutSet.IterateArray(array);

                foreach (var index in indexes)
                {
                    values.UnionWith(OutSet.GetIndex(array, index).PossibleValues);
                }

                OutSet.Assign(valueVariable.DirectName, new MemoryEntry(values));
            }
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

        public override MemoryEntry Concat(MemoryEntry leftOperand, MemoryEntry rightOperand)
        {
            throw new NotImplementedException();
        }

        public override void Echo(EchoStmt echo, MemoryEntry[] values)
        {
            throw new NotImplementedException();
        }
    }
}
