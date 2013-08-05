using System;
using System.Collections.Generic;
using System.Linq;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis;
using Weverca.Analysis.Expressions;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis
{
    /// <summary>
    /// Expression evaluation is resolved here
    /// </summary>
    class AndvancedExpressionEvaluator : ExpressionEvaluatorBase
    {
        #region ExpressionEvaluator overrides

        public override MemoryEntry ResolveVariable(VariableEntry variable)
        {
            // TODO: If method is analyzed, $this is the object, otherwise a runtime error has occurred.
            // Outset contains property ThisObject with current $this value

            if (variable.IsDirect)
            {
                return OutSet.ReadValue(variable.DirectName);
            }
            else
            {
                var names = variable.PossibleNames;
                // TODO: It need to have no names, in case of $$variable if $variable is uninitialized
                Debug.Assert(names.Length > 0, "Every variable must have at least one name");
                // TODO: Could HashSet be used?
                var entries = new List<MemoryEntry>(names.Length);

                foreach (var name in names)
                {
                    entries.Add(OutSet.ReadValue(name));
                }

                return MemoryEntry.Merge(entries);
            }
        }

        public override MemoryEntry ResolveField(MemoryEntry objectValue, VariableEntry field)
        {
            // TODO: It need to have no names, in case of $$object if $object is uninitialized
            Debug.Assert(objectValue.PossibleValues.GetEnumerator().MoveNext());
            // TODO: Could HashSet be used?
            var entries = new List<MemoryEntry>();

            foreach (var variableValue in objectValue.PossibleValues)
            {
                // TODO: If method is analyzed, objectValue is $this object, otherwise a runtime error
                // has occurred. Outset contains property ThisObject with current $this value

                var objectInstance = variableValue as ObjectValue;
                if (objectInstance != null)
                {
                    if (field.IsDirect)
                    {
                        var index = OutSet.CreateIndex(field.DirectName.Value);
                        entries.Add(OutSet.GetField(objectInstance, index));
                    }
                    else
                    {
                        // TODO: It need to have no names, in case of $$field if $field is uninitialized
                        Debug.Assert(field.PossibleNames.Length > 0,
                            "Every variable must have at least one name");

                        foreach (var fieldName in field.PossibleNames)
                        {
                            var index = OutSet.CreateIndex(fieldName.Value);
                            entries.Add(OutSet.GetField(objectInstance, index));
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException(
                        "Variable accessed by field do not need to be object! "
                        + "Report possible error as special value");
                }
            }

            return MemoryEntry.Merge(entries);
        }

        public override IEnumerable<AliasValue> ResolveAlias(MemoryEntry objectValue, VariableEntry aliasedField)
        {
            throw new NotImplementedException();
        }

        public override void AliasAssign(VariableEntry target, IEnumerable<AliasValue> alias)
        {
            if (alias.Count() == 1)
            {
                var value = alias.GetEnumerator().Current;
                var entry = new MemoryEntry(value);
                Assign(target, entry);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void AliasAssign(MemoryEntry objectValue, VariableEntry fieldEntry, IEnumerable<AliasValue> possibleAliasses)
        {
            throw new NotImplementedException();
        }

        public override void Assign(VariableEntry target, MemoryEntry value)
        {
            if (target.IsDirect)
            {
                var name = target.DirectName;
                if (target.DirectName.IsThisVariableName)
                {
                    throw new NotImplementedException("Cannot re-assign $this variable!");
                }
                else
                {
                    OutSet.Assign(target.DirectName, value);
                }
            }
            else
            {
                // TODO: It can have no names, in case of $$variable if $variable is uninitialized
                Debug.Assert(target.PossibleNames.Length > 0,
                    "Every variable must have at least one name");
                // When saving to multiple variables, only one variable is changed. Others retain
                // their original value. Thus, the value is appended, not assigned, to the current values.
                foreach (var name in target.PossibleNames)
                {
                    if (name.IsThisVariableName)
                    {
                        throw new NotImplementedException("Cannot potentially re-assign $this variable!");
                    }
                    else
                    {
                        var currentValue = OutSet.ReadValue(name);
                        var newValue = MemoryEntry.Merge(currentValue, value);
                        OutSet.Assign(name, newValue);
                    }
                }
            }
        }

        public override void Assign(MemoryEntry objectValue, VariableEntry targetField, MemoryEntry value)
        {
            // TODO: It need to have no values, in case of $$object if $object is uninitialized
            Debug.Assert(objectValue.PossibleValues.GetEnumerator().MoveNext());
            // TODO: It can have no names, in case of $$field if $field is uninitialized
            Debug.Assert(targetField.PossibleNames.Length > 0);

            var objectEnumerator = objectValue.PossibleValues.GetEnumerator();
            objectEnumerator.MoveNext();
            var isDirect = (!objectEnumerator.MoveNext()) && targetField.IsDirect;

            // Field can have "this" name.
            if (isDirect)
            {
                var objectName = objectValue.PossibleValues.GetEnumerator().Current as ObjectValue;
                if (objectName != null)
                {
                    var index = OutSet.CreateIndex(targetField.DirectName.Value);
                    OutSet.SetField(objectName, index, value);
                }
                else
                {
                    throw new NotImplementedException(
                        "Variable of the assigned field is not object! "
                        + "Report possible error as special value");
                }
            }
            else
            {
                // When saving to multiple object or multiple fields of one object (or both), only
                // one memory place is changed. Others retain thier original value. Thus, the value
                // is appended, not assigned, to the current values of all fields of all objects.

                foreach (var variableName in objectValue.PossibleValues)
                {
                    var objectName = variableName as ObjectValue;
                    if (objectName != null)
                    {
                        foreach (var fieldName in targetField.PossibleNames)
                        {
                            var index = OutSet.CreateIndex(fieldName.Value);
                            var currentValue = OutSet.GetField(objectName, index);
                            var newValue = MemoryEntry.Merge(currentValue, value);
                            OutSet.SetField(objectName, index, newValue);
                        }
                    }
                    else
                    {
                        throw new NotImplementedException(
                            "Variable of the assigned field do not need to be object! "
                            + "Report possible error as special value");
                    }
                }
            }
        }

        public override MemoryEntry BinaryEx(MemoryEntry leftOperand, Operations operation, MemoryEntry rightOperand)
        {
            switch (operation)
            {
                case Operations.Equal:
                    return GetUniformityResult(leftOperand, rightOperand);
                default:
                    return GetAritmeticResult(leftOperand, operation, rightOperand);
            }
        }

        public override IEnumerable<string> VariableNames(MemoryEntry value)
        {
            // TODO: convert all value types
            return from StringValue possible in value.PossibleValues select possible.Value;
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

            return result;
        }

        public override MemoryEntry ResolveIndexedVariable(VariableEntry entry)
        {
            // TODO: If method is analyzed, $this is the object, otherwise a runtime error has occurred.
            // Outset contains property ThisObject with current $this value

            if (entry.IsDirect)
            {
                var name = entry.DirectName;
                var variableValue = OutSet.ReadValue(name);
                var resolvedValue = ResolveEntryToArray(name, variableValue);
                OutSet.Assign(name, resolvedValue);
                return resolvedValue;
            }
            else
            {
                var names = entry.PossibleNames;
                // TODO: It need to have no names, in case of $$variable if $variable is uninitialized
                Debug.Assert(names.Length > 0, "Every variable must have at least one name");
                // TODO: Could HashSet be used?
                var entries = new List<MemoryEntry>(names.Length);

                foreach (var name in entry.PossibleNames)
                {
                    var variableValue = OutSet.ReadValue(name);
                    var resolvedValue = ResolveEntryToArray(name, variableValue);
                    OutSet.Assign(name, resolvedValue);
                    entries.Add(resolvedValue);
                }

                return MemoryEntry.Merge(entries);
            }
        }

        #endregion

        #region Comparison operators

        private MemoryEntry GetUniformityResult(MemoryEntry left, MemoryEntry right)
        {
            var result = new List<BooleanValue>();

            if (CanBeDifferent(left, right))
            {
                result.Add(OutSet.CreateBool(false));
            }

            if (CanBeSame(left, right))
            {
                result.Add(OutSet.CreateBool(true));
            }

            return new MemoryEntry(result.ToArray());
        }

        private bool CanBeSame(MemoryEntry left, MemoryEntry right)
        {
            if (ContainsAnyValue(left) || ContainsAnyValue(right))
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

        private bool CanBeDifferent(MemoryEntry left, MemoryEntry right)
        {
            if (ContainsAnyValue(left) || ContainsAnyValue(right))
            {
                return true;
            }

            if (left.PossibleValues.Count() > 1 || left.PossibleValues.Count() > 1)
            {
                return true;
            }

            return !left.Equals(right);
        }

        private bool ContainsAnyValue(MemoryEntry entry)
        {
            // TODO: Undefined value maybe is not correct to be treated as any value
            return entry.PossibleValues.Contains(OutSet.AnyValue);
        }

        #endregion

        #region Arithmetic operators

        private MemoryEntry GetAritmeticResult(MemoryEntry leftOperand, Operations operation, MemoryEntry rightOperand)
        {
            var values = new List<Value>();

            foreach (var leftValue in leftOperand.PossibleValues)
            {
                foreach (var rightValue in rightOperand.PossibleValues)
                {
                    if (leftValue is IntegerValue)
                    {
                        var leftInt = leftValue as IntegerValue;
                        if (rightValue is IntegerValue)
                        {
                            var rightInt = rightValue as IntegerValue;
                            values.Add(GetAritmeticResult(leftInt, operation, rightInt));
                        }
                    }
                    else if (leftValue is StringValue)
                    {
                        var leftString = leftValue as StringValue;
                        if (rightValue is IntegerValue)
                        {
                            var rightString = rightValue as StringValue;

                            if ((leftString != null) && (rightString != null))
                            {
                                var newString = String.Concat(leftString.Value, rightString.Value);
                                var newValue = OutSet.CreateString(newString);
                                values.Add(newValue);
                            }
                        }
                    }
                }
            }

            var entries = new List<MemoryEntry>(values.Count);
            foreach (var value in values)
            {
                entries.Add(new MemoryEntry(value));
            }
            return MemoryEntry.Merge(entries);
        }

        private Value GetAritmeticResult(IntegerValue leftOperand, Operations operation, IntegerValue rightOperand)
        {
            switch (operation)
            {
                case Operations.Equal:
                case Operations.Identical:
                    return OutSet.CreateBool(leftOperand.Value == rightOperand.Value);
                case Operations.NotEqual:
                case Operations.NotIdentical:
                    return OutSet.CreateBool(leftOperand.Value != rightOperand.Value);
                case Operations.LessThan:
                    return OutSet.CreateBool(leftOperand.Value < rightOperand.Value);
                case Operations.LessThanOrEqual:
                    return OutSet.CreateBool(leftOperand.Value <= rightOperand.Value);
                case Operations.GreaterThan:
                    return OutSet.CreateBool(leftOperand.Value > rightOperand.Value);
                case Operations.GreaterThanOrEqual:
                    return OutSet.CreateBool(leftOperand.Value >= rightOperand.Value);
                case Operations.Add:
                    return OutSet.CreateInt(leftOperand.Value + rightOperand.Value);
                case Operations.Sub:
                    return OutSet.CreateInt(leftOperand.Value - rightOperand.Value);
                case Operations.Mul:
                    return OutSet.CreateInt(leftOperand.Value * rightOperand.Value);
                case Operations.Div:
                    return OutSet.CreateDouble((double)leftOperand.Value / rightOperand.Value);
                case Operations.Mod:
                    return OutSet.CreateDouble((double)leftOperand.Value % rightOperand.Value);
                case Operations.BitAnd:
                    return OutSet.CreateInt(leftOperand.Value & rightOperand.Value);
                case Operations.BitOr:
                    return OutSet.CreateInt(leftOperand.Value | rightOperand.Value);
                case Operations.BitXor:
                    return OutSet.CreateInt(leftOperand.Value ^ rightOperand.Value);
                case Operations.ShiftLeft:
                    return OutSet.CreateInt(leftOperand.Value << rightOperand.Value);
                case Operations.ShiftRight:
                    return OutSet.CreateInt(leftOperand.Value >> rightOperand.Value);
                case Operations.And:
                    return OutSet.CreateBool((leftOperand.Value != 0) && (rightOperand.Value != 0));
                case Operations.Or:
                    return OutSet.CreateBool((leftOperand.Value != 0) || (rightOperand.Value != 0));
                case Operations.Xor:
                    return OutSet.CreateBool((leftOperand.Value != 0) != (rightOperand.Value != 0));
                case Operations.Concat:
                    return OutSet.CreateString(leftOperand.Value.ToString() + rightOperand.Value.ToString());
                default:
                    Debug.Fail("There are no other binary operators!");
                    return null;
            }
        }

        #endregion

        private MemoryEntry ResolveEntryToArray(VariableName name, MemoryEntry variableValue)
        {
            var entries = new List<MemoryEntry>();
            var existsUndefinedValue = false;

            foreach (var value in variableValue.PossibleValues)
            {
                if ((value is AssociativeArray) || (value is StringValue))
                {
                    entries.Add(new MemoryEntry(value));
                }
                else if (value is UndefinedValue)
                {
                    existsUndefinedValue = true;
                    // TODO: If variable has more names, undefined value must be included too!
                }
                else
                {
                    throw new NotImplementedException(
                        "Cannot use type other than associative array or string as array!");
                }
            }

            if (existsUndefinedValue)
            {
                // New array is implicitly created if variable is not initialized.
                var newArray = OutSet.CreateArray();
                entries.Add(new MemoryEntry(newArray));
            }

            return MemoryEntry.Merge(entries);
        }

        public override MemoryEntry UnaryEx(Operations operation, MemoryEntry operand)
        {
            throw new NotImplementedException();
        }
    }
}
