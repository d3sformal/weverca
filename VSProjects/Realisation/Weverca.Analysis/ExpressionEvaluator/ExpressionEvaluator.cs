using System;
using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
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

        public override MemoryEntry ResolveVariable(VariableIdentifier variable)
        {
            MemoryEntry entry;
            bool noValueExists;

            if (variable.IsDirect)
            {
                // Special case of direct access is $this variable. It has always just one value
                // Variable $this is absolutely valid outside method, if it does not access to any member.
                // It behaves as undefined value, thus it returns null with warning.
                noValueExists = !OutSet.TryReadValue(variable.DirectName, out entry);
            }
            else
            {
                var names = variable.PossibleNames;
                Debug.Assert(names.Length > 0, "Every variable must have at least one name");

                noValueExists = true;
                var valueAlwaysExists = true;
                var values = new HashSet<Value>();

                foreach (var name in names)
                {
                    MemoryEntry variableEntry;
                    if (OutSet.TryReadValue(name, out variableEntry))
                    {
                        if (noValueExists)
                        {
                            noValueExists = false;
                        }
                    }
                    else
                    {
                        if (valueAlwaysExists)
                        {
                            valueAlwaysExists = false;
                        }
                    }

                    values.UnionWith(variableEntry.PossibleValues);
                }

                if (!(valueAlwaysExists || noValueExists))
                {
                    SetWarning("Reading of possible undefined variable, null is included",
                        AnalysisWarningCause.UNDEFINED_VALUE);
                }

                entry = new MemoryEntry(values);
            }

            if (noValueExists)
            {
                // Undefined value means that variable was not initialized, we report that.
                SetWarning("Reading of undefined variable, null is returned",
                    AnalysisWarningCause.UNDEFINED_VALUE);
            }

            Debug.Assert(entry.Count > 0, "Every resolved variable must give at least one value");
            return entry;
        }

        public override MemoryEntry ResolveField(MemoryEntry objectValue, VariableIdentifier field)
        {
            Debug.Assert(field.PossibleNames.Length > 0, "Every field variable must have at least one name");

            var indices = new HashSet<ContainerIndex>();
            foreach (var fieldName in field.PossibleNames)
            {
                if (fieldName.Value.Length > 0)
                {
                    indices.Add(OutSet.CreateIndex(fieldName.Value));
                }
                else
                {
                    // Everything, that can be converted to empty string ("", null, false etc.),
                    // produce empty property that does not represent any memory storage
                    // TODO: This must be fatal error
                    SetWarning("Cannot access empty property");
                }
            }

            Debug.Assert(objectValue.Count > 0, "Every object entry must have at least one value");

            bool isAlwaysObject;
            bool isAlwaysConcrete;
            var objectValues = ResolveObjectsForMember(objectValue, out isAlwaysObject, out isAlwaysConcrete);

            var noValueExists = true;
            var valueAlwaysExists = isAlwaysConcrete;
            var values = new HashSet<Value>();

            foreach (var objectInstance in objectValues)
            {
                foreach (var index in indices)
                {
                    MemoryEntry entry;
                    if (OutSet.TryGetField(objectInstance, index, out entry))
                    {
                        if (noValueExists)
                        {
                            noValueExists = false;
                        }
                    }
                    else
                    {
                        if (valueAlwaysExists)
                        {
                            valueAlwaysExists = false;
                        }
                    }

                    values.UnionWith(entry.PossibleValues);
                }
            }

            if (!isAlwaysObject)
            {
                if ((objectValues.Count > 0) || (!isAlwaysConcrete))
                {
                    SetWarning("Trying to get property of possible non-object variable",
                        AnalysisWarningCause.PROPERTY_OF_NON_OBJECT_VARIABLE);
                    values.Add(OutSet.UndefinedValue);
                }
                else
                {
                    SetWarning("Trying to get property of non-object variable",
                        AnalysisWarningCause.PROPERTY_OF_NON_OBJECT_VARIABLE);
                    return new MemoryEntry(OutSet.UndefinedValue);
                }
            }

            if (!isAlwaysConcrete)
            {
                values.Add(OutSet.AnyValue);
            }

            if (!valueAlwaysExists)
            {
                // Undefined value means that property was not initializes, we report that.
                if (noValueExists)
                {
                    SetWarning("Reading of undefined property, null is returned",
                        AnalysisWarningCause.UNDEFINED_VALUE);
                }
                else
                {
                    SetWarning("Reading of possible undefined property, null is included",
                        AnalysisWarningCause.UNDEFINED_VALUE);
                }
            }

            Debug.Assert(values.Count > 0, "Every resolved field must give at least one value");
            return new MemoryEntry(values);
        }

        public override IEnumerable<AliasValue> ResolveAliasedField(MemoryEntry objectValue,
            VariableIdentifier aliasedField)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<AliasValue> ResolveAliasedIndex(MemoryEntry arrayValue,
            MemoryEntry aliasedIndex)
        {
            throw new NotImplementedException();
        }

        public override void AliasAssign(VariableIdentifier target, IEnumerable<AliasValue> possibleAliases)
        {
            var entry = new MemoryEntry(possibleAliases);
            Assign(target, entry);
        }

        public override void AliasedFieldAssign(MemoryEntry objectValue, VariableIdentifier aliasedField,
            IEnumerable<AliasValue> possibleAliases)
        {
            throw new NotImplementedException();
        }

        public override void AliasedIndexAssign(MemoryEntry arrayValue, MemoryEntry aliasedIndex,
            IEnumerable<AliasValue> possibleAliases)
        {
            throw new NotImplementedException();
        }

        public override void Assign(VariableIdentifier target, MemoryEntry entry)
        {
            Debug.Assert(entry.Count > 0, "Memory entry assigned to variable must have at least one value");

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
                    OutSet.Assign(name, entry);
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

        public override void FieldAssign(MemoryEntry objectValue, VariableIdentifier targetField,
            MemoryEntry entry)
        {
            var isOneEntry = (objectValue.Count == 1) && targetField.IsDirect;
            if (isOneEntry)
            {
                var enumerator = objectValue.PossibleValues.GetEnumerator();
                enumerator.MoveNext();
                Debug.Assert(!(enumerator.Current is UndefinedValue), "Undefined value has been changed to object");

                var objectInstance = enumerator.Current as ObjectValue;
                if (objectInstance != null)
                {
                    string fieldName = targetField.DirectName.Value;
                    if (fieldName.Length > 0)
                    {
                        var index = OutSet.CreateIndex(fieldName);
                        OutSet.SetField(objectInstance, index, entry);
                    }
                    else
                    {
                        // Everything, that can be converted to empty string ("", null, false etc.),
                        // produce empty property that does not represent any memory storage
                        // TODO: This must be fatal error
                        SetWarning("Cannot access empty property");
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
                Debug.Assert(targetField.PossibleNames.Length > 0,
                    "Every field variable must have at least one name");

                var indices = new HashSet<ContainerIndex>();
                foreach (var fieldName in targetField.PossibleNames)
                {
                    if (fieldName.Value.Length > 0)
                    {
                        indices.Add(OutSet.CreateIndex(fieldName.Value));
                    }
                    else
                    {
                        // Everything, that can be converted to empty string ("", null, false etc.),
                        // produce empty property that does not represent any memory storage
                        // TODO: This must be fatal error
                        SetWarning("Cannot access empty property");
                    }
                }

                Debug.Assert(objectValue.Count > 0, "Every object entry must have at least one value");

                bool isAlwaysObject;
                bool isAlwaysConcrete;
                var objectValues = ResolveObjectsForMember(objectValue, out isAlwaysObject, out isAlwaysConcrete);

                // When saving to multiple object or multiple fields of one object (or both), only
                // one memory place is changed. Others retain thier original value. Thus, the value
                // is appended, not assigned, to the current values of all fields of all objects.
                foreach (var objectInstance in objectValues)
                {
                    foreach (var index in indices)
                    {
                        FieldAppendValue(objectInstance, index, entry);
                    }
                }

                if (!isAlwaysObject)
                {
                    if ((objectValues.Count > 0) || (!isAlwaysConcrete))
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
            var values = new HashSet<Value>();

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
            var values = new HashSet<Value>();

            foreach (var value in operand.PossibleValues)
            {
                values.Add(unaryOperationVisitor.Evaluate(operation, value));
            }

            return new MemoryEntry(values);
        }

        public override MemoryEntry IncDecEx(IncDecEx operation, MemoryEntry incrementedValue)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry ArrayEx(
            IEnumerable<KeyValuePair<MemoryEntry, MemoryEntry>> keyValuePairs)
        {
            // TODO: It is not done

            var keyCollections = new List<KeyValuePair<MemoryEntry, MemoryEntry>>();
            foreach (var keyValue in keyValuePairs)
            {
                if (keyValue.Key == null)
                {
                    keyCollections.Add(keyValue);
                    continue;
                }

                var keys = new HashSet<ScalarValue>();

                // TODO: It is too complicated
                foreach (var key in keyValue.Key.PossibleValues)
                {
                    if (key is IntegerValue)
                    {
                        keys.Add(key as IntegerValue);
                    }
                    else if (key is FloatValue)
                    {
                        IntegerValue integerValue;
                        if (TypeConversion.TryConvertToInteger(OutSet, key as FloatValue, out integerValue))
                        {
                            keys.Add(integerValue);
                        }
                    }
                    else if (key is BooleanValue)
                    {
                        keys.Add(TypeConversion.ToInteger(OutSet, key as BooleanValue));
                    }
                    else if (key is StringValue)
                    {
                        IntegerValue integerValue;
                        if (TypeConversion.TryConvertToInteger(OutSet, key as StringValue, out integerValue))
                        {
                            keys.Add(integerValue);
                        }
                        else
                        {
                            keys.Add(key as StringValue);
                        }
                    }
                    else if (key is UndefinedValue)
                    {
                        keys.Add(TypeConversion.ToString(OutSet, key as UndefinedValue));
                    }
                }

                var entry = new MemoryEntry(keys);
                keyCollections.Add(new KeyValuePair<MemoryEntry, MemoryEntry>(entry, keyValue.Value));
            }

            int counter = 0;
            var array = OutSet.CreateArray();
            foreach (var keyValue in keyCollections)
            {
                ContainerIndex index;
                if (keyValue.Key != null)
                {
                    if (keyValue.Key.Count <= 0)
                    {
                        continue;
                    }

                    // TODO: All possible keys must be evaluated
                    var enumerator = keyValue.Key.PossibleValues.GetEnumerator();
                    enumerator.MoveNext();

                    var possibleInteger = enumerator.Current as IntegerValue;
                    if (possibleInteger != null)
                    {
                        if (possibleInteger.Value >= 0)
                        {
                            counter = possibleInteger.Value + 1;
                        }
                        index = OutSet.CreateIndex(TypeConversion.ToString(counter));
                    }
                    else
                    {
                        Debug.Assert(enumerator.Current is StringValue);
                        var stringKey = enumerator.Current as StringValue;
                        index = OutSet.CreateIndex(stringKey.Value);
                    }
                }
                else
                {
                    index = OutSet.CreateIndex(TypeConversion.ToString(counter));
                    counter = ((counter >= 0) ? (counter + 1) : 0);
                }

                OutSet.SetIndex(array, index, keyValue.Value);
            }

            return new MemoryEntry(array);
        }

        public override IEnumerable<string> VariableNames(MemoryEntry variableSpecifier)
        {
            Debug.Assert(variableSpecifier.Count > 0, "Every variable must have at least one name");

            var names = new HashSet<string>();
            foreach (var possible in variableSpecifier.PossibleValues)
            {
                var value = unaryOperationVisitor.Evaluate(Operations.StringCast, possible);
                var stringValue = value as StringValue;
                if (stringValue != null)
                {
                    names.Add(stringValue.Value);
                }
                else
                {
                    // TODO: What to return?
                    Debug.Assert(value is AnyStringValue);
                }
            }

            return names;
        }

        public override void IndexAssign(MemoryEntry array, MemoryEntry index, MemoryEntry assignedValue)
        {
            // TODO: Inside method, $this variable is an object, otherwise a runtime error has occurred.
            // The problem is that we do not know the name of variable and we cannot detect it.

            Debug.Assert(index.Count > 0, "Every index entry must have at least one value");

            var indexNames = VariableNames(index);
            var indices = CreateContainterIndexes(indexNames);

            var isOneEntry = (array.Count == 1) && (index.Count == 1);
            if (isOneEntry)
            {
                var arrayEnumerator = array.PossibleValues.GetEnumerator();
                arrayEnumerator.MoveNext();
                Debug.Assert(!(arrayEnumerator.Current is UndefinedValue), "Undefined value has been changed to array");

                var arrayValue = arrayEnumerator.Current as AssociativeArray;
                if (arrayValue != null)
                {
                    var indexEnumerator = indices.GetEnumerator();
                    indexEnumerator.MoveNext();
                    OutSet.SetIndex(arrayValue, indexEnumerator.Current, assignedValue);
                }
                else
                {
                    // TODO: String can be accessed by index too
                    SetWarning("Cannot assign using a key to variable different from array",
                        AnalysisWarningCause.ELEMENT_OF_NON_ARRAY_VARIABLE);
                }
            }
            else
            {
                Debug.Assert(array.Count > 0, "Every array entry must have at least one value");

                bool isAlwaysObject;
                bool isAlwaysConcrete;
                var arrayValues = ResolveArraysForIndex(array, out isAlwaysObject, out isAlwaysConcrete);

                // When saving to multiple array or multiple array elements (or both), only
                // one memory place is changed. Others retain thier original value. Thus, the value
                // is appended, not assigned, to the current values of all elements of all arrays.
                foreach (var arrayValue in arrayValues)
                {
                    foreach (var containerIndex in indices)
                    {
                        IndexAppendValue(arrayValue, containerIndex, assignedValue);
                    }
                }

                if (!isAlwaysObject)
                {
                    if ((arrayValues.Count > 0) || (!isAlwaysConcrete))
                    {
                        SetWarning("Cannot assign using a key to variable possibly different from array",
                            AnalysisWarningCause.ELEMENT_OF_NON_ARRAY_VARIABLE);
                    }
                    else
                    {
                        SetWarning("Cannot assign using a key to variable different from array",
                            AnalysisWarningCause.ELEMENT_OF_NON_ARRAY_VARIABLE);
                    }
                }
            }
        }

        public override MemoryEntry ResolveIndex(MemoryEntry array, MemoryEntry index)
        {
            Debug.Assert(index.Count > 0, "Every index entry must have at least one value");

            var indexNames = VariableNames(index);
            var indices = CreateContainterIndexes(indexNames);

            Debug.Assert(array.Count > 0, "Every array entry must have at least one value");

            bool isAlwaysObject;
            bool isAlwaysConcrete;
            var arrayValues = ResolveArraysForIndex(array, out isAlwaysObject, out isAlwaysConcrete);

            var noValueExists = true;
            var valueAlwaysExists = isAlwaysConcrete;
            var values = new HashSet<Value>();

            foreach (var arrayValue in arrayValues)
            {
                foreach (var containerIndex in indices)
                {
                    MemoryEntry entry;
                    if (OutSet.TryGetIndex(arrayValue, containerIndex, out entry))
                    {
                        if (noValueExists)
                        {
                            noValueExists = false;
                        }
                    }
                    else
                    {
                        if (valueAlwaysExists)
                        {
                            valueAlwaysExists = false;
                        }
                    }

                    values.UnionWith(entry.PossibleValues);
                }
            }

            if (!isAlwaysObject)
            {
                if ((arrayValues.Count > 0) || (!isAlwaysConcrete))
                {
                    SetWarning("Trying to get element of possible non-array variable",
                        AnalysisWarningCause.ELEMENT_OF_NON_ARRAY_VARIABLE);
                    values.Add(OutSet.UndefinedValue);
                }
                else
                {
                    SetWarning("Trying to get element of non-array variable",
                        AnalysisWarningCause.ELEMENT_OF_NON_ARRAY_VARIABLE);
                    return new MemoryEntry(OutSet.UndefinedValue);
                }
            }

            if (!isAlwaysConcrete)
            {
                values.Add(OutSet.AnyArrayValue);
            }

            if (!valueAlwaysExists)
            {
                // Undefined value means that property was not initializes, we report that.
                if (noValueExists)
                {
                    SetWarning("Undefined array offset, null is included",
                        AnalysisWarningCause.UNDEFINED_VALUE);
                }
                else
                {
                    SetWarning("Possible undefined array offset, null is returned",
                        AnalysisWarningCause.UNDEFINED_VALUE);
                }
            }

            Debug.Assert(values.Count > 0, "Every resolved element must give at least one value");
            return new MemoryEntry(values);
        }

        public override MemoryEntry ResolveIndexedVariable(VariableIdentifier variable)
        {
            var names = variable.PossibleNames;
            Debug.Assert(names.Length > 0, "Every variable must have at least one name");

            var allValues = new HashSet<Value>();
            foreach (var name in names)
            {
                var entry = OutSet.ReadValue(name);
                Debug.Assert(entry.Count > 0, "Every resolved variable must give at least one value");

                var values = new List<Value>();
                var isEntryUnchanged = true;
                foreach (var value in entry.PossibleValues)
                {
                    var undefined = value as UndefinedValue;
                    if ((undefined != null) && isEntryUnchanged)
                    {
                        if (name.IsThisVariableName)
                        {
                            // Variable $this cannot be assigned even if it is an object or null
                            // TODO: This must be error
                            SetWarning("Trying to create array in $this variable");
                        }
                        else
                        {
                            var arrayValue = OutSet.CreateArray();
                            values.Add(arrayValue);
                            isEntryUnchanged = false;
                        }
                    }
                    else
                    {
                        values.Add(value);
                    }
                }

                if (!isEntryUnchanged)
                {
                    var newEntry = new MemoryEntry(values);
                    OutSet.Assign(name, newEntry);
                }

                allValues.UnionWith(values);
            }

            return new MemoryEntry(allValues);
        }

        public override void Foreach(MemoryEntry enumeree, VariableIdentifier keyVariable,
            VariableIdentifier valueVariable)
        {
            // TODO: This is only basic functionality, for instance, reference of element is not recognized

            Debug.Assert(enumeree.Count > 0, "Enumeree must always have at least one value");

            bool isAlwaysArray;
            bool isAlwaysConcrete;
            var arrays = ResolveArraysForIndex(enumeree, out isAlwaysArray, out isAlwaysConcrete);

            var values = new HashSet<Value>();
            foreach (var array in arrays)
            {
                var indices = OutSet.IterateArray(array);
                foreach (var index in indices)
                {
                    var element = OutSet.GetIndex(array, index);
                    values.UnionWith(element.PossibleValues);
                }
            }

            if (!isAlwaysConcrete)
            {
                values.Add(OutSet.AnyValue);
            }

            foreach (var valueName in valueVariable.PossibleNames)
            {
                // There could be no values because array could have no elements
                // However it is fine because in this case, foreach will not trace to loop body
                var entry = new MemoryEntry(values);
                OutSet.Assign(valueName, entry);
            }

            if (!isAlwaysArray)
            {
                if ((arrays.Count > 0) || (!isAlwaysConcrete))
                {
                    SetWarning("Possibly invalid argument supplied for foreach");
                }
                else
                {
                    SetWarning("Invalid argument supplied for foreach");
                }
            }
        }

        public override MemoryEntry Concat(IEnumerable<MemoryEntry> parts)
        {
            // TODO: Optimalize
            MemoryEntry leftOperand = null;

            foreach (var part in parts)
            {
                if (leftOperand != null)
                {
                    leftOperand = BinaryEx(leftOperand, Operations.Concat, part);
                }
                else
                {
                    leftOperand = part;
                }
            }

            Debug.Assert(leftOperand != null);
            return leftOperand;
        }

        public override void Echo(EchoStmt echo, MemoryEntry[] entries)
        {
            // TODO: Optimalize, implementation is provided only for faultless progress and testing

            foreach (var entry in entries)
            {
                foreach (var value in entry.PossibleValues)
                {
                    unaryOperationVisitor.Evaluate(Operations.StringCast, value);
                }
            }
        }

        public override MemoryEntry Constant(GlobalConstUse x)
        {
            var constantAnalyzer = NativeConstantAnalyzer.Create(OutSet);
            var name = x.Name;

            if (constantAnalyzer.ExistContant(name))
            {
                var value = constantAnalyzer.GetConstantValue(name);
                return new MemoryEntry(value);
            }
            else
            {
                MemoryEntry entry;
                bool isNotDefined;
                if (!UserDefinedConstantHandler.TryGetConstant(OutSet, name, out entry, out isNotDefined))
                {
                    if (isNotDefined)
                    {
                        SetWarning("Use of undefined constant",
                            AnalysisWarningCause.UNDEFINED_VALUE);
                    }
                    else
                    {
                        SetWarning("Possible use of undefined constant",
                            AnalysisWarningCause.UNDEFINED_VALUE);
                    }
                }

                return entry;
            }
        }

        public override void ConstantDeclaration(ConstantDecl x, MemoryEntry constantValue)
        {
            var name = new QualifiedName(new Name(x.Name.Value));
            UserDefinedConstantHandler.insertConstant(OutSet, name, constantValue, false);
        }

        /// <summary>
        /// Create object value of given type
        /// </summary>
        /// <param name="typeName">Object type specifier</param>
        /// <returns>Created object</returns>
        public override MemoryEntry CreateObject(QualifiedName typeName)
        {
            var types = OutSet.ResolveType(typeName);
            if (!types.GetEnumerator().MoveNext())
            {
                var objectAnalyzer = NativeObjectAnalyzer.GetInstance(Flow);
                NativeTypeDecl nativeDeclaration;
                if (objectAnalyzer.TryGetClass(typeName, out nativeDeclaration))
                {
                    var type = OutSet.CreateType(nativeDeclaration);
                    OutSet.DeclareGlobal(type);
                    var newTypes = new List<TypeValue>();
                    newTypes.Add(type);
                    types = newTypes;
                }
                else
                {
                    // TODO: If no type is resolved, exception should be thrown
                    Debug.Fail("No type resolved");
                }
            }

            var values = new List<ObjectValue>();
            foreach (var type in types)
            {
                var newObject = CreateInitializedObject(type);
                values.Add(newObject);
            }

            return new MemoryEntry(values);
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

        private static List<ObjectValue> ResolveObjectsForMember(MemoryEntry entry,
            out bool isAlwaysObject, out bool isAlwaysConcrete)
        {
            Debug.Assert(entry.Count > 0, "Every object entry must have at least one value");

            var objectValues = new List<ObjectValue>();
            isAlwaysObject = true;
            isAlwaysConcrete = true;

            foreach (var value in entry.PossibleValues)
            {
                // TODO: Inside method, $this variable is an object, otherwise a runtime error has occurred.
                // The problem is that we do not know the name of variable and we cannot detect it.

                var objectInstance = value as ObjectValue;
                if (objectInstance != null)
                {
                    objectValues.Add(objectInstance);
                }
                else
                {
                    var anyObjectInstance = value as AnyObjectValue;
                    if (anyObjectInstance != null)
                    {
                        if (isAlwaysConcrete)
                        {
                            isAlwaysConcrete = false;
                        }
                    }
                    else
                    {
                        if (isAlwaysObject)
                        {
                            isAlwaysObject = false;
                        }
                    }
                }
            }

            return objectValues;
        }

        private static List<AssociativeArray> ResolveArraysForIndex(MemoryEntry entry,
            out bool isAlwaysArray, out bool isAlwaysConcrete)
        {
            Debug.Assert(entry.Count > 0, "Every array entry must have at least one value");

            var arrayValues = new List<AssociativeArray>();
            isAlwaysArray = true;
            isAlwaysConcrete = true;

            foreach (var value in entry.PossibleValues)
            {
                // TODO: Inside method, $this variable is an object, otherwise a runtime error has occurred.
                // The problem is that we do not know the name of variable and we cannot detect it.

                var arrayValue = value as AssociativeArray;
                if (arrayValue != null)
                {
                    arrayValues.Add(arrayValue);
                }
                else
                {
                    var anyArrayValue = value as AnyArrayValue;
                    if (anyArrayValue != null)
                    {
                        if (isAlwaysConcrete)
                        {
                            isAlwaysConcrete = false;
                        }
                    }
                    else
                    {
                        // TODO: String can be accessed by index too
                        if (isAlwaysArray)
                        {
                            isAlwaysArray = false;
                        }
                    }
                }
            }

            return arrayValues;
        }

        private void AppendValue(VariableName variable, MemoryEntry entry)
        {
            var currentValue = OutSet.ReadValue(variable);
            var newValue = MemoryEntry.Merge(currentValue, entry);
            OutSet.Assign(variable, newValue);
        }

        private void FieldAppendValue(ObjectValue objectValue, ContainerIndex index, MemoryEntry entry)
        {
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
