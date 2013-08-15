using System;
using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis;
using Weverca.Analysis.Expressions;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.ExpressionEvaluator
{
    /// <summary>
    /// Expression evaluation is resolved here
    /// </summary>
    public class ExpressionEvaluator : ExpressionEvaluatorBase
    {
        private UnaryOperationVisitor unaryOperationVisitor;
        private BinaryOperationVisitor binaryOperationVisitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluator" /> class.
        /// </summary>
        public ExpressionEvaluator()
        {
            unaryOperationVisitor = new UnaryOperationVisitor(this);
            binaryOperationVisitor = new BinaryOperationVisitor(this);
        }

        #region ExpressionEvaluatorBase overrides

        public override MemoryEntry ResolveVariable(VariableEntry variable)
        {
            // Variable $this is absolutely valid, if it does not access to any member.
            // It behaves as undefined value, thus it returns null with warning.

            MemoryEntry entry;
            if (variable.IsDirect)
            {
                entry = OutSet.ReadValue(variable.DirectName);
            }
            else
            {
                var names = variable.PossibleNames;
                Debug.Assert(names.Length > 0, "Every variable must have at least one name");

                var entries = new List<MemoryEntry>(names.Length);
                foreach (var name in names)
                {
                    entries.Add(OutSet.ReadValue(name));
                }

                entry = MemoryEntry.Merge(entries);
            }

            if (ReplaceUndefinedValueByNull(ref entry))
            {
                // Undefined value means that variable was not initializes, we report that.
                if (HasMoreValues(entry))
                {
                    SetWarning("Reading of possible undefined variable, null is returned",
                        AnalysisWarningCause.UNDEFINED_VALUE);
                }
                else
                {
                    SetWarning("Reading of undefined variable, null is returned",
                        AnalysisWarningCause.UNDEFINED_VALUE);
                }
            }

            Debug.Assert(HasValues(entry), "Every resolved variable must give at least one value");
            return entry;
        }

        public override MemoryEntry ResolveField(MemoryEntry objectValue, VariableEntry field)
        {
            Debug.Assert(HasValues(objectValue), "Every object entry must have at least one value");
            Debug.Assert(field.PossibleNames.Length > 0, "Every field variable must have at least one name");

            var entries = new List<MemoryEntry>();
            var isPossibleNonObject = false;
            var isAtLeastOneObject = false;

            var indeces = new List<ContainerIndex>();
            foreach (var fieldName in field.PossibleNames)
            {
                indeces.Add(OutSet.CreateIndex(fieldName.Value));
            }

            foreach (var variableValue in objectValue.PossibleValues)
            {
                // TODO: Inside method, $this variable is an object, otherwise a runtime error has occurred.
                // The problem is that we do not know the name of variable and we cannot detect it.

                var objectInstance = variableValue as ObjectValue;
                if (objectInstance != null)
                {
                    if (!isAtLeastOneObject)
                    {
                        isAtLeastOneObject = true;
                    }

                    foreach (var index in indeces)
                    {
                        if (index.Identifier.Equals(string.Empty))
                        {
                            // Everything, that can be converted to empty string ("", null, false etc.),
                            // produce empty property that does not represent any memory storage
                            // TODO: This must be fatal error
                            SetWarning("Cannot access empty property");
                            // TODO: Add null value instead of undefined one
                            entries.Add(new MemoryEntry(OutSet.UndefinedValue));
                        }
                        else
                        {
                            // TODO: All fields are undefined after creation.
                            entries.Add(OutSet.GetField(objectInstance, index));
                        }
                    }
                }
                else
                {
                    if (!isPossibleNonObject)
                    {
                        isPossibleNonObject = true;
                    }
                }
            }

            if (isPossibleNonObject)
            {
                if (isAtLeastOneObject)
                {
                    SetWarning("Trying to get property of possible non-object variable",
                        AnalysisWarningCause.PROPERTY_OF_NON_OBJECT_VARIABLE);
                    // TODO: Add null value instead of undefined one
                    entries.Add(new MemoryEntry(OutSet.UndefinedValue));
                }
                else
                {
                    SetWarning("Trying to get property of non-object variable",
                        AnalysisWarningCause.PROPERTY_OF_NON_OBJECT_VARIABLE);
                    // TODO: Return null value instead of undefined one
                    return new MemoryEntry(OutSet.UndefinedValue);
                }
            }

            var entry = MemoryEntry.Merge(entries);
            if (ReplaceUndefinedValueByNull(ref entry))
            {
                // Undefined value means that property was not initializes, we report that.
                if (HasMoreValues(entry))
                {
                    SetWarning("Reading of possible undefined property, null is returned",
                        AnalysisWarningCause.UNDEFINED_VALUE);
                }
                else
                {
                    SetWarning("Reading of undefined property, null is returned",
                        AnalysisWarningCause.UNDEFINED_VALUE);
                }
            }

            Debug.Assert(HasValues(entry), "Every resolved field must give at least one value");
            return entry;
        }

        public override IEnumerable<AliasValue> ResolveAliasedField(MemoryEntry objectValue,
            VariableEntry aliasedField)
        {
            throw new NotImplementedException();
        }

        public override void AliasAssign(VariableEntry target, IEnumerable<AliasValue> possibleAliases)
        {
            var entry = new MemoryEntry(possibleAliases);
            Assign(target, entry);
        }

        public override void AliasedFieldAssign(MemoryEntry objectValue, VariableEntry aliasedField,
            IEnumerable<AliasValue> possibleAliases)
        {
            throw new NotImplementedException();
        }

        public override void Assign(VariableEntry target, MemoryEntry entry)
        {
            if (target.IsDirect)
            {
                var name = target.DirectName;
                if (name.IsThisVariableName)
                {
                    // TODO: This must be fatal error
                    SetWarning("Re-assigning of $this variable");
                }
                else
                {
                    OutSet.Assign(target.DirectName, entry);
                }
            }
            else
            {
                var names = target.PossibleNames;
                Debug.Assert(names.Length > 0, "Every variable must have at least one name");

                // When saving to multiple variables, only one variable is changed. Others retain
                // their original value. Thus, the value is appended, not assigned, to the current values.
                foreach (var name in names)
                {
                    if (name.IsThisVariableName)
                    {
                        // TODO: This must be fatal error
                        SetWarning("Possible re-assigning of $this variable");
                    }
                    else
                    {
                        AppendValue(name, entry);
                    }
                }
            }
        }

        public override void FieldAssign(MemoryEntry objectValue, VariableEntry targetField,
            MemoryEntry entry)
        {
            Debug.Assert(HasValues(objectValue), "Every object entry must have at least one value");
            Debug.Assert(targetField.PossibleNames.Length > 0,
                "Every field variable must have at least one name");

            var isOneEntry = HasExactlyOneValue(objectValue) && targetField.IsDirect;
            if (isOneEntry)
            {
                var enumerator = objectValue.PossibleValues.GetEnumerator();
                enumerator.MoveNext();
                var possibleObject = enumerator.Current as ObjectValue;

                if (possibleObject != null)
                {
                    string fieldName = targetField.DirectName.Value;
                    if (fieldName.Equals(string.Empty))
                    {
                        // Everything, that can be converted to empty string ("", null, false etc.),
                        // produce empty property that does not represent any memory storage
                        // TODO: This must be fatal error
                        SetWarning("Cannot access empty property");
                    }
                    else
                    {
                        var index = OutSet.CreateIndex(fieldName);
                        OutSet.SetField(possibleObject, index, entry);
                    }
                }
                else
                {
                    SetWarning("Attempt to assign property of non-object",
                        AnalysisWarningCause.PROPERTY_OF_NON_OBJECT_VARIABLE);
                }
            }
            else
            {
                // TODO: Inside method, $this variable is an object, otherwise a runtime error has occurred.
                // The problem is that we do not know the name of variable and we cannot detect it.

                var isPossibleNonObject = false;
                var isAtLeastOneObject = false;

                // When saving to multiple object or multiple fields of one object (or both), only
                // one memory place is changed. Others retain thier original value. Thus, the value
                // is appended, not assigned, to the current values of all fields of all objects.
                foreach (var variableValue in objectValue.PossibleValues)
                {
                    var possibleObject = variableValue as ObjectValue;
                    if (possibleObject != null)
                    {
                        if (!isAtLeastOneObject)
                        {
                            isAtLeastOneObject = true;
                        }

                        foreach (var fieldName in targetField.PossibleNames)
                        {
                            if (fieldName.Value.Equals(string.Empty))
                            {
                                // Everything, that can be converted to empty string ("", null, false etc.),
                                // produce empty property that does not represent any memory storage
                                // TODO: This must be fatal error
                                SetWarning("Cannot access empty property");
                            }
                            else
                            {
                                FieldAppendValue(fieldName.Value, possibleObject, entry);
                            }
                        }
                    }
                    else
                    {
                        if (!isPossibleNonObject)
                        {
                            isPossibleNonObject = true;
                        }
                    }
                }

                if (isPossibleNonObject)
                {
                    if (isAtLeastOneObject)
                    {
                        SetWarning("Possible attempt to assign property of non-object",
                            AnalysisWarningCause.PROPERTY_OF_NON_OBJECT_VARIABLE);
                    }
                    else
                    {
                        SetWarning("Attempt to assign property of non-object",
                            AnalysisWarningCause.PROPERTY_OF_NON_OBJECT_VARIABLE);
                    }
                }
            }
        }

        public override MemoryEntry BinaryEx(MemoryEntry leftOperand, Operations operation,
            MemoryEntry rightOperand)
        {
            var values = new List<Value>();

            foreach (var leftValue in leftOperand.PossibleValues)
            {
                foreach (var rightValue in rightOperand.PossibleValues)
                {
                    values.Add(binaryOperationVisitor.Evaluate(leftValue, operation, rightValue));
                }
            }

            return new MemoryEntry(values);
        }

        public override MemoryEntry UnaryEx(Operations operation, MemoryEntry operand)
        {
            var values = new List<Value>();

            foreach (var value in operand.PossibleValues)
            {
                values.Add(unaryOperationVisitor.Evaluate(operation, value));
            }

            return new MemoryEntry(values);
        }

        public override MemoryEntry ArrayEx(IEnumerable<KeyValuePair<MemoryEntry, MemoryEntry>> keyValuePairs)
        {
            // TODO: It is not done

            int counter = 0;
            var array = OutSet.CreateArray();
            foreach (var keyValue in keyValuePairs)
            {
                ContainerIndex index;
                if (keyValue.Key != null)
                {
                    var enumerator = keyValue.Key.PossibleValues.GetEnumerator();
                    enumerator.MoveNext();

                    var possibleInteger = enumerator.Current as IntegerValue;
                    if (possibleInteger != null)
                    {
                        if (possibleInteger.Value >= 0)
                        {
                            counter = possibleInteger.Value + 1;
                        }
                    }

                    var key = unaryOperationVisitor.Evaluate(Operations.StringCast, enumerator.Current);
                    var stringKey = key as StringValue;
                    index = OutSet.CreateIndex(stringKey.Value);
                }
                else
                {
                    index = OutSet.CreateIndex(counter.ToString());
                    ++counter;
                }

                OutSet.SetIndex(array, index, keyValue.Value);
            }

            return new MemoryEntry(array);
        }

        public override IEnumerable<string> VariableNames(MemoryEntry variableSpecifier)
        {
            Debug.Assert(HasValues(variableSpecifier), "Every variable must have at least one name");

            var names = new HashSet<string>();
            foreach (var possible in variableSpecifier.PossibleValues)
            {
                var value = unaryOperationVisitor.Evaluate(Operations.StringCast, possible);
                var stringValue = value as StringValue;
                names.Add(stringValue.Value);
            }

            return names;
        }

        public override void IndexAssign(MemoryEntry array, MemoryEntry index, MemoryEntry assignedValue)
        {
            Debug.Assert(HasValues(array), "Every array entry must have at least one value");
            Debug.Assert(HasValues(index), "Every index entry must have at least one value");

            var indexNames = VariableNames(index);
            var indexes = CreateContainterIndexes(indexNames);

            var isOneEntry = HasExactlyOneValue(array) && HasExactlyOneValue(index);
            if (isOneEntry)
            {
                var arrayEnumerator = array.PossibleValues.GetEnumerator();
                arrayEnumerator.MoveNext();

                AssociativeArray possibleArray;
                if (arrayEnumerator.Current is UndefinedValue)
                {
                    // Array is created, if value of variable is null
                    possibleArray = OutSet.CreateArray();
                }
                else
                {
                    possibleArray = arrayEnumerator.Current as AssociativeArray;
                }

                if (possibleArray != null)
                {
                    var indexEnumerator = indexes.GetEnumerator();
                    indexEnumerator.MoveNext();
                    OutSet.SetIndex(possibleArray, indexEnumerator.Current, assignedValue);
                }
                else
                {
                    // TODO: String can be accessed by index too
                    SetWarning("Cannot assign using a key to variable different from array");
                }
            }
            else
            {
                // TODO: Inside method, $this variable is an object, otherwise a runtime error has occurred.
                // The problem is that we do not know the name of variable and we cannot detect it.

                var isPossibleNonArray = false;
                var isAtLeastOneArray = false;

                // When saving to multiple array or multiple array elements (or both), only
                // one memory place is changed. Others retain thier original value. Thus, the value
                // is appended, not assigned, to the current values of all elements of all arrays.
                foreach (var variableValue in array.PossibleValues)
                {
                    var possibleArray = variableValue as AssociativeArray;
                    if (possibleArray != null)
                    {
                        if (!isAtLeastOneArray)
                        {
                            isAtLeastOneArray = true;
                        }

                        foreach (var containerIndex in indexes)
                        {
                            IndexAppendValue(possibleArray, containerIndex, assignedValue);
                        }
                    }
                    else if (variableValue is UndefinedValue)
                    {
                        foreach (var containerIndex in indexes)
                        {
                            // Every value creates new single array
                            possibleArray = OutSet.CreateArray();
                            IndexAppendValue(possibleArray, containerIndex, assignedValue);
                        }
                    }
                    else
                    {
                        // TODO: String can be accessed by index too
                        if (!isPossibleNonArray)
                        {
                            isPossibleNonArray = true;
                        }
                    }
                }

                if (isPossibleNonArray)
                {
                    if (isAtLeastOneArray)
                    {
                        SetWarning("Cannot assign using a key to variable possibly different from array.");
                    }
                    else
                    {
                        SetWarning("Cannot assign using a key to variable different from array.");
                    }
                }
            }
        }

        public override MemoryEntry ResolveIndex(MemoryEntry array, MemoryEntry index)
        {
            Debug.Assert(HasValues(array), "Every array entry must have at least one value");
            Debug.Assert(HasValues(index), "Every field entry must have at least one value");

            var indexNames = VariableNames(index);
            var indexes = CreateContainterIndexes(indexNames);

            var entries = new List<MemoryEntry>();
            var isPossibleNonArray = false;
            var isAtLeastOneArray = false;

            foreach (var variableValue in array.PossibleValues)
            {
                // TODO: Inside method, $this variable is an object, otherwise a runtime error has occurred.
                // The problem is that we do not know the name of variable and we cannot detect it.

                var arrayValue = variableValue as AssociativeArray;
                if (arrayValue != null)
                {
                    if (!isAtLeastOneArray)
                    {
                        isAtLeastOneArray = true;
                    }

                    foreach (var containerIndex in indexes)
                    {
                        entries.Add(OutSet.GetIndex(arrayValue, containerIndex));
                    }
                }
                else
                {
                    // TODO: String can be accessed by index too
                    if (!isPossibleNonArray)
                    {
                        isPossibleNonArray = true;
                    }
                }
            }

            if (isPossibleNonArray)
            {
                if (isAtLeastOneArray)
                {
                    SetWarning("Trying to get element of possible non-array variable");
                    // TODO: Add null value instead of undefined one
                    entries.Add(new MemoryEntry(OutSet.UndefinedValue));
                }
                else
                {
                    SetWarning("Trying to get element of non-array variable");
                    // TODO: Return null value instead of undefined one
                    return new MemoryEntry(OutSet.UndefinedValue);
                }
            }

            var entry = MemoryEntry.Merge(entries);
            if (ReplaceUndefinedValueByNull(ref entry))
            {
                // Undefined value means that array element was not initializes, we report that.
                if (HasMoreValues(entry))
                {
                    SetWarning("Possible undefined array offset, null is returned");
                }
                else
                {
                    SetWarning("Undefined array offset, null is returned");
                }
            }

            Debug.Assert(HasValues(entry), "Every resolved element must give at least one value");
            return entry;
        }

        public override MemoryEntry ResolveIndexedVariable(VariableEntry variable)
        {
            var names = variable.PossibleNames;
            Debug.Assert(names.Length > 0, "Every variable must have at least one name");

            var entries = new List<MemoryEntry>(names.Length);
            foreach (var name in names)
            {
                // TODO: Variable $this cannot be assigned and can be only object or null
                var entry = OutSet.ReadValue(name);
                Debug.Assert(HasValues(entry), "Every resolved variable must give at least one value");

                var values = new List<Value>();
                foreach (var value in entry.PossibleValues)
                {
                    var undefined = value as UndefinedValue;
                    if (undefined != null)
                    {
                        var arrayValue = OutSet.CreateArray();
                        values.Add(arrayValue);
                    }
                    else
                    {
                        values.Add(value);
                    }
                }

                var newEntry = new MemoryEntry(values);
                OutSet.Assign(name, newEntry);
                entries.Add(newEntry);
            }

            return MemoryEntry.Merge(entries);
        }

        #endregion

        public void SetWarning(string message)
        {
            AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(message, Element));
        }

        public void SetWarning(string message, AnalysisWarningCause cause)
        {
            AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(message, Element, cause));
        }

        private void AppendValue(VariableName variable, MemoryEntry entry)
        {
            var currentValue = OutSet.ReadValue(variable);
            var newValue = MemoryEntry.Merge(currentValue, entry);
            OutSet.Assign(variable, newValue);
        }

        private void FieldAppendValue(string fieldName, ObjectValue objectValue, MemoryEntry entry)
        {
            var index = OutSet.CreateIndex(fieldName);
            var currentValue = OutSet.GetField(objectValue, index);
            var newValue = MemoryEntry.Merge(currentValue, entry);
            OutSet.SetField(objectValue, index, newValue);
        }

        private void IndexAppendValue(AssociativeArray arrayValue, ContainerIndex index, MemoryEntry entry)
        {
            var currentValue = OutSet.GetIndex(arrayValue, index);
            var newValue = MemoryEntry.Merge(currentValue, entry);
            OutSet.SetIndex(arrayValue, index, newValue);
        }

        private bool HasExactlyOneValue(MemoryEntry entry)
        {
            var enumerator = entry.PossibleValues.GetEnumerator();
            return enumerator.MoveNext() && !enumerator.MoveNext();
        }

        private bool HasValues(MemoryEntry entry)
        {
            var enumerator = entry.PossibleValues.GetEnumerator();
            return enumerator.MoveNext();
        }

        private bool HasMoreValues(MemoryEntry entry)
        {
            var enumerator = entry.PossibleValues.GetEnumerator();
            return enumerator.MoveNext() && enumerator.MoveNext();
        }

        private bool ReplaceUndefinedValueByNull(ref MemoryEntry entry)
        {
            // TODO: It only finds out if values contain the undefined one.
            foreach (var value in entry.PossibleValues)
            {
                if (value.Equals(OutSet.UndefinedValue))
                {
                    return true;
                }
            }

            return false;
        }

        private List<ContainerIndex> CreateContainterIndexes(IEnumerable<string> indexNames)
        {
            var indexes = new List<ContainerIndex>();
            foreach (var indexName in indexNames)
            {
                var index = OutSet.CreateIndex(indexName);
                indexes.Add(index);
            }

            return indexes;
        }
    }
}
