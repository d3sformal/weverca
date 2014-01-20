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
        /// <summary>
        /// Convertor of values to strings used by evaluator
        /// </summary>
        private StringConverter stringConverter;

        /// <summary>
        /// The partial evaluator of one unary operation
        /// </summary>
        private UnaryOperationEvaluator unaryOperationEvaluator;

        /// <summary>
        /// The partial evaluator of one prefix or postfix increment or decrement operation
        /// </summary>
        private IncrementDecrementEvaluator incrementDecrementEvaluator;

        /// <summary>
        /// The partial evaluator of values that can be used as index of array
        /// </summary>
        private ArrayIndexEvaluator arrayIndexEvaluator;

        /// <summary>
        /// The partial evaluator of one binary operation
        /// </summary>
        private BinaryOperationEvaluator binaryOperationVisitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluator" /> class.
        /// </summary>
        public ExpressionEvaluator()
        {
            stringConverter = new StringConverter(Flow);
            unaryOperationEvaluator = new UnaryOperationEvaluator(Flow, stringConverter);
            incrementDecrementEvaluator = new IncrementDecrementEvaluator(Flow);
            arrayIndexEvaluator = new ArrayIndexEvaluator(Flow);
            binaryOperationVisitor = new BinaryOperationEvaluator(Flow, stringConverter);
        }

        #region ExpressionEvaluatorBase overrides

        /// <inheritdoc />
        public override MemberIdentifier MemberIdentifier(MemoryEntry memberRepresentation)
        {
            Debug.Assert(memberRepresentation.Count > 0,
                "Every expresiion in offset must have at least one value");

            // TODO: How to indicate, that index can be illegal, i.e. compound type?
            bool isAlwaysLegal;

            arrayIndexEvaluator.SetContext(Flow);
            return arrayIndexEvaluator.EvaluateToIdentifiers(memberRepresentation, out isAlwaysLegal);
        }

        /// <inheritdoc />
        public override ReadWriteSnapshotEntryBase ResolveVariable(VariableIdentifier variable)
        {
            return OutSet.GetVariable(variable);
        }

        /// <inheritdoc />
        public override ReadWriteSnapshotEntryBase ResolveField(ReadSnapshotEntryBase objectValue,
            VariableIdentifier field)
        {
            return objectValue.ReadField(OutSnapshot, field);
        }

        /// <inheritdoc />
        public override void AliasAssign(ReadWriteSnapshotEntryBase target,
            ReadSnapshotEntryBase aliasedValue)
        {
            target.SetAliases(OutSnapshot, aliasedValue);
        }

        /// <inheritdoc />
        public override void Assign(ReadWriteSnapshotEntryBase target, MemoryEntry entry)
        {
            target.WriteMemory(OutSnapshot, entry);
        }

        /// <inheritdoc />
        public override void FieldAssign(ReadSnapshotEntryBase objectValue, VariableIdentifier targetField,
            MemoryEntry assignedValue)
        {
            var fieldEntry = objectValue.ReadField(OutSnapshot, targetField);
            fieldEntry.WriteMemory(OutSnapshot, assignedValue);
        }

        /// <inheritdoc />
        public override MemoryEntry BinaryEx(MemoryEntry leftOperand, Operations operation,
            MemoryEntry rightOperand)
        {
            binaryOperationVisitor.SetContext(Flow);
            return binaryOperationVisitor.Evaluate(leftOperand, operation, rightOperand);
        }

        /// <inheritdoc />
        public override MemoryEntry UnaryEx(Operations operation, MemoryEntry operand)
        {
            unaryOperationEvaluator.SetContext(Flow);
            return unaryOperationEvaluator.Evaluate(operation, operand);
        }

        /// <inheritdoc />
        public override MemoryEntry IncDecEx(IncDecEx operation, MemoryEntry incrementedValue)
        {
            incrementDecrementEvaluator.SetContext(Flow);
            return incrementDecrementEvaluator.Evaluate(operation.Inc, incrementedValue);
        }

        /// <inheritdoc />
        public override MemoryEntry ArrayEx(
            IEnumerable<KeyValuePair<MemoryEntry, MemoryEntry>> keyValuePairs)
        {
            var array = OutSet.CreateArray();
            var arrayEntry = OutSet.CreateSnapshotEntry(new MemoryEntry(array));

            var currentIntegerIndices = new HashSet<int>();
            currentIntegerIndices.Add(0);

            arrayIndexEvaluator.SetContext(Flow);

            foreach (var keyValue in keyValuePairs)
            {
                var indices = new HashSet<string>();
                bool isAlwaysConcrete;

                if (keyValue.Key != null)
                {
                    var integerValues = new HashSet<IntegerValue>();
                    var stringValues = new HashSet<StringValue>();

                    bool isAlwaysInteger;
                    bool isAlwaysLegal;

                    arrayIndexEvaluator.Evaluate(keyValue.Key, integerValues, stringValues,
                        out isAlwaysConcrete, out isAlwaysInteger, out isAlwaysLegal);

                    // Create default indices for next element
                    var nextIntegerIndices = new HashSet<int>();
                    foreach (var integerValue in integerValues)
                    {
                        if (integerValue.Value >= 0)
                        {
                            if (integerValue.Value < int.MaxValue)
                            {
                                nextIntegerIndices.Add(integerValue.Value + 1);
                            }
                        }
                        else
                        {
                            nextIntegerIndices.Add(0);
                        }
                    }

                    // If all new indices are integer, previous default indices are rewritten
                    if (isAlwaysInteger)
                    {
                        currentIntegerIndices = nextIntegerIndices;
                    }
                    else
                    {
                        currentIntegerIndices.UnionWith(nextIntegerIndices);
                    }

                    if (isAlwaysConcrete)
                    {
                        foreach (var integerValue in integerValues)
                        {
                            indices.Add(TypeConversion.ToString(integerValue.Value));
                        }

                        foreach (var stringValue in stringValues)
                        {
                            indices.Add(stringValue.Value);
                        }
                    }

                    if (!isAlwaysLegal)
                    {
                        SetWarning("Possible illegal offset type in array initilization");
                    }
                }
                else
                {
                    isAlwaysConcrete = true;

                    var nextIndices = new HashSet<int>();
                    foreach (var defaultIndex in currentIntegerIndices)
                    {
                        indices.Add(TypeConversion.ToString(defaultIndex));
                        if (defaultIndex < int.MaxValue)
                        {
                            nextIndices.Add(defaultIndex + 1);
                        }
                    }

                    currentIntegerIndices = nextIndices;
                }

                MemberIdentifier indexIdentifier;
                if (isAlwaysConcrete)
                {
                    indexIdentifier = new MemberIdentifier(indices);
                }
                else
                {
                    // There are some values that cannot be store to exact index, so unknown index is used
                    indexIdentifier = new MemberIdentifier();
                }

                var indexEntry = arrayEntry.ReadIndex(OutSnapshot, indexIdentifier);
                indexEntry.WriteMemory(OutSnapshot, keyValue.Value);
            }

            return new MemoryEntry(array);
        }

        /// <inheritdoc />
        public override IEnumerable<string> VariableNames(MemoryEntry variableSpecifier)
        {
            Debug.Assert(variableSpecifier.Count > 0, "Every variable must have at least one name");

            // TODO: What should I return if value cannot be converted to concrete string?
            bool isAlwaysConcrete;

            stringConverter.SetContext(Flow);
            var stringValues = stringConverter.Evaluate(variableSpecifier, out isAlwaysConcrete);

            var names = new HashSet<string>();
            foreach (var value in stringValues)
            {
                names.Add(value.Value);
            }

            return names;
        }

        /// <inheritdoc />
        public override void IndexAssign(ReadSnapshotEntryBase indexedValue, MemoryEntry index,
            MemoryEntry assignedValue)
        {
            var indexIdentifier = MemberIdentifier(index);
            var indexEntry = indexedValue.ReadIndex(OutSnapshot, indexIdentifier);
            indexEntry.WriteMemory(OutSnapshot, assignedValue);
        }

        /// <inheritdoc />
        public override ReadWriteSnapshotEntryBase ResolveIndex(ReadSnapshotEntryBase indexedValue,
            MemberIdentifier index)
        {
            return indexedValue.ReadIndex(OutSnapshot, index);
        }

        /// <inheritdoc />
        public override MemoryEntry ResolveIndexedVariable(VariableIdentifier variable)
        {
            foreach (var name in variable.PossibleNames)
            {
                if (name.IsThisVariableName)
                {
                    // Variable $this cannot be assigned even if it is an object or null
                    // TODO: This must be error
                    SetWarning("Trying to create array in $this variable");
                }
            }

            var snapshotEntry = ResolveVariable(variable);
            var entry = snapshotEntry.ReadMemory(OutSnapshot);
            Debug.Assert(entry.Count > 0, "Every resolved variable must give at least one value");

            var values = new List<Value>();
            var isEntryUnchanged = true;
            foreach (var value in entry.PossibleValues)
            {
                var undefinedValue = value as UndefinedValue;
                if (undefinedValue != null)
                {
                    if (isEntryUnchanged)
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

            if (isEntryUnchanged)
            {
                return entry;
            }
            else
            {
                var newEntry = new MemoryEntry(values);
                snapshotEntry.WriteMemory(OutSnapshot, newEntry);
                return newEntry;
            }
        }

        /// <inheritdoc />
        public override void Foreach(MemoryEntry enumeree, ReadWriteSnapshotEntryBase keyVariable,
            ReadWriteSnapshotEntryBase valueVariable)
        {
            // TODO: This is only basic functionality, for instance, reference of element is not recognized

            Debug.Assert(enumeree.Count > 0, "Enumeree must always have at least one value");

            bool isAlwaysArray;
            bool isAlwaysConcrete;
            var arrays = ResolveArraysForIndex(enumeree, out isAlwaysArray, out isAlwaysConcrete);

            var keys = new HashSet<Value>();
            var values = new HashSet<Value>();

            foreach (var array in arrays)
            {
                var arrayEntry = OutSet.CreateSnapshotEntry(new MemoryEntry(array));

                var indices = OutSet.IterateArray(array);
                foreach (var index in indices)
                {
                    int convertedInteger;
                    if (TypeConversion.TryIdentifyInteger(index.Identifier, out convertedInteger))
                    {
                        keys.Add(OutSnapshot.CreateInt(convertedInteger));
                    }
                    else
                    {
                        keys.Add(OutSnapshot.CreateString(index.Identifier));
                    }

                    var indexIdentifier = new MemberIdentifier(index.Identifier);
                    var indexEntry = arrayEntry.ReadIndex(OutSnapshot, indexIdentifier);
                    var element = indexEntry.ReadMemory(OutSnapshot);
                    values.UnionWith(element.PossibleValues);
                }
            }

            if (!isAlwaysConcrete)
            {
                if (keyVariable != null)
                {
                    keys.Add(OutSet.AnyValue);
                }

                values.Add(OutSet.AnyValue);
            }

            // There could be no values because array could have no elements
            // However it is fine because in this case, foreach will not trace to loop body
            if (keys.Count > 0)
            {
                if (keyVariable != null)
                {
                    var keyEntry = new MemoryEntry(keys);
                    keyVariable.WriteMemory(OutSnapshot, keyEntry);
                }

                Debug.Assert(values.Count > 0);
                var valueEntry = new MemoryEntry(values);
                valueVariable.WriteMemory(OutSnapshot, valueEntry);
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

        /// <inheritdoc />
        public override MemoryEntry Concat(IEnumerable<MemoryEntry> parts)
        {
            MemoryEntry leftOperand = null;

            stringConverter.SetContext(Flow);
            foreach (var part in parts)
            {
                if (leftOperand != null)
                {
                    leftOperand = stringConverter.EvaluateConcatenation(leftOperand, part);
                }
                else
                {
                    leftOperand = part;
                }
            }

            Debug.Assert(leftOperand != null, "There must be at least one operand to concat");
            return leftOperand;
        }

        /// <inheritdoc />
        public override void Echo(EchoStmt echo, MemoryEntry[] entries)
        {
            // TODO: Optimalize, implementation is provided only for faultless progress and testing

            stringConverter.SetContext(Flow);
            foreach (var entry in entries)
            {
                bool isAlwaysConcrete;
                stringConverter.Evaluate(entry, out isAlwaysConcrete);
            }
        }

        /// <inheritdoc />
        public override MemoryEntry IssetEx(IEnumerable<VariableIdentifier> variables)
        {
            Debug.Assert(variables.GetEnumerator().MoveNext(),
                "isset expression must have at least one parameter");

            var isAlwaysDefined = true;

            foreach (var variable in variables)
            {
                var snapshotEntry = OutSet.GetVariable(variable);
                if (snapshotEntry.IsDefined(OutSnapshot))
                {
                    var entry = snapshotEntry.ReadMemory(OutSnapshot);
                    Debug.Assert(entry.PossibleValues.GetEnumerator().MoveNext(),
                        "Memory entry must always have at least one value");

                    var isOnlyUndefined = true;
                    foreach (var value in entry.PossibleValues)
                    {
                        var undefinedValue = value as UndefinedValue;
                        if (undefinedValue != null)
                        {
                            if (isAlwaysDefined)
                            {
                                isAlwaysDefined = false;
                                if (!isOnlyUndefined)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (isOnlyUndefined)
                            {
                                isOnlyUndefined = false;
                                if (!isAlwaysDefined)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    if (isOnlyUndefined)
                    {
                        return new MemoryEntry(OutSet.CreateBool(false));
                    }
                }
                else
                {
                    return new MemoryEntry(OutSet.CreateBool(false));
                }
            }

            if (isAlwaysDefined)
            {
                return new MemoryEntry(OutSet.CreateBool(true));
            }
            else
            {
                return new MemoryEntry(OutSet.AnyBooleanValue);
            }
        }

        /// <inheritdoc />
        public override MemoryEntry EmptyEx(VariableIdentifier variable)
        {
            var snapshotEntry = OutSet.GetVariable(variable);
            if (snapshotEntry.IsDefined(OutSnapshot))
            {
                var entry = snapshotEntry.ReadMemory(OutSnapshot);
                Debug.Assert(entry.PossibleValues.GetEnumerator().MoveNext(),
                    "Memory entry must always have at least one value");

                unaryOperationEvaluator.SetContext(Flow);

                var isAlwaysEmpty = true;
                var isNeverEmpty = true;

                foreach (var value in entry.PossibleValues)
                {
                    var convertedValue = unaryOperationEvaluator.Evaluate(Operations.BoolCast, value);

                    var booleanValue = convertedValue as BooleanValue;
                    if (booleanValue != null)
                    {
                        if (booleanValue.Value)
                        {
                            if (isAlwaysEmpty)
                            {
                                isAlwaysEmpty = false;
                                if (!isNeverEmpty)
                                {
                                    return new MemoryEntry(OutSet.AnyBooleanValue);
                                }
                            }
                        }
                        else
                        {
                            if (isNeverEmpty)
                            {
                                isNeverEmpty = false;
                                if (!isAlwaysEmpty)
                                {
                                    return new MemoryEntry(OutSet.AnyBooleanValue);
                                }
                            }
                        }
                    }
                    else
                    {
                        var anyBooleanValue = convertedValue as AnyBooleanValue;
                        Debug.Assert(anyBooleanValue != null,
                            "Casting to boolean can result only to boolean value");

                        return new MemoryEntry(OutSet.AnyBooleanValue);
                    }
                }

                if (isAlwaysEmpty)
                {
                    return new MemoryEntry(OutSet.CreateBool(true));
                }
                else
                {
                    Debug.Assert(isNeverEmpty, "Undecidable cases are solved inside loop");
                    return new MemoryEntry(OutSet.CreateBool(false));
                }
            }
            else
            {
                return new MemoryEntry(OutSet.CreateBool(true));
            }
        }

        /// <inheritdoc />
        public override MemoryEntry Exit(ExitEx exit, MemoryEntry status)
        {
            // TODO: It must jump to the end of program and print status, if it is a string

            // Exit expression never returns, but it is still expression so it must return something
            return new MemoryEntry(OutSet.AnyValue);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override void ConstantDeclaration(ConstantDecl x, MemoryEntry constantValue)
        {
            var name = new QualifiedName(new Name(x.Name.Value));
            UserDefinedConstantHandler.insertConstant(OutSet, name, constantValue, false);
        }

        /// <inheritdoc />
        public override MemoryEntry CreateObject(QualifiedName typeName)
        {
            var typeNames = new QualifiedName[] { typeName };
            return CreateObjectFromNames(typeNames);
        }

        /// <inheritdoc />
        public override MemoryEntry IndirectCreateObject(MemoryEntry possibleNames)
        {
            var typeNames = ResolveTypeNames(possibleNames);
            if (typeNames != null)
            {
                return CreateObjectFromNames(typeNames);
            }
            else
            {
                SetWarning("Class name cannot be resolved when new object is created indirectly");
                return new MemoryEntry(OutSet.AnyObjectValue);
            }
        }

        /// <inheritdoc />
        public override MemoryEntry InstanceOfEx(MemoryEntry expression, QualifiedName typeName)
        {
            var typeNames = new QualifiedName[] { typeName };
            return InstanceOfTypeNames(expression, typeNames);
        }

        /// <inheritdoc />
        public override MemoryEntry IndirectInstanceOfEx(MemoryEntry expression, MemoryEntry possibleNames)
        {
            var typeNames = ResolveTypeNames(possibleNames);
            if (typeNames != null)
            {
                return InstanceOfTypeNames(expression, typeNames);
            }
            else
            {
                SetWarning("Class name cannot be resolved when using instanceof construct");
                return new MemoryEntry(OutSet.AnyBooleanValue);
            }
        }

        #endregion ExpressionEvaluatorBase overrides

        /// <summary>
        /// Generates a warning with the given message
        /// </summary>
        /// <param name="message">Text of warning</param>
        public void SetWarning(string message)
        {
            AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(message, Element));
        }

        /// <summary>
        /// Generates a warning of the proper type and with the given message
        /// </summary>
        /// <param name="message">Text of warning</param>
        /// <param name="cause">More specific warning type</param>
        public void SetWarning(string message, AnalysisWarningCause cause)
        {
            AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(message, Element, cause));
        }

        /// <summary>
        /// Find all arrays in list of universal values
        /// </summary>
        /// <param name="entry">Memory entry of values to resolve to arrays</param>
        /// <param name="isAlwaysArray">Indicates whether all values are array</param>
        /// <param name="isAlwaysConcrete">Indicates whether all values are concrete array</param>
        /// <returns>All resolved concrete arrays</returns>
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

        /// <summary>
        /// Resolve source or native type from name
        /// </summary>
        /// <param name="typeName">Name of type to resolve</param>
        /// <returns><c>null</c> whether type cannot be resolver, otherwise the type value</returns>
        private IEnumerable<TypeValue> ResolveSourceOrNativeType(QualifiedName typeName)
        {
            var typeValues = OutSet.ResolveType(typeName);
            if (!typeValues.GetEnumerator().MoveNext())
            {
                var objectAnalyzer = NativeObjectAnalyzer.GetInstance(Flow.OutSet);
                ClassDecl nativeDeclaration;
                if (objectAnalyzer.TryGetClass(typeName, out nativeDeclaration))
                {
                    var type = OutSet.CreateType(nativeDeclaration);
                    OutSet.DeclareGlobal(type);
                    var newTypes = new List<TypeValue>();
                    newTypes.Add(type);
                    typeValues = newTypes;
                }
                else
                {
                    // TODO: This must be error
                    SetWarning("Class not found when creating new object");
                    return null;
                }
            }

            return typeValues;
        }

        /// <summary>
        /// Resolve names from values
        /// </summary>
        /// <param name="possibleNames">Memory entry of values to resolve to type names</param>
        /// <returns><c>null</c> whether any value cannot be resolved to string, otherwise names</returns>
        private List<QualifiedName> ResolveTypeNames(MemoryEntry possibleNames)
        {
            Debug.Assert(possibleNames.Count > 0, "Every memory entry must have at least one value");

            stringConverter.SetContext(Flow);
            bool isAlwaysConcrete;
            var names = stringConverter.Evaluate(possibleNames, out isAlwaysConcrete);

            if (isAlwaysConcrete)
            {
                var typeNames = new List<QualifiedName>();
                foreach (var name in names)
                {
                    typeNames.Add(new QualifiedName(new Name(name.Value)));
                }

                return typeNames;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates new objects of specified type names, directly and indirectly too
        /// </summary>
        /// <param name="typeNames">Possible names of classes or interfaces</param>
        /// <returns>Memory entry with all new possible objects</returns>
        private MemoryEntry CreateObjectFromNames(IEnumerable<QualifiedName> typeNames)
        {
            var values = new List<ObjectValue>();
            var isTypeAlwaysDefined = true;

            foreach (var typeName in typeNames)
            {
                var typeValues = ResolveSourceOrNativeType(typeName);
                if (typeValues != null)
                {
                    foreach (var type in typeValues)
                    {
                        var newObject = CreateInitializedObject(type);
                        values.Add(newObject);
                    }
                }
                else
                {
                    // TODO: This must be error
                    SetWarning("Class not found when creating new object");

                    if (isTypeAlwaysDefined)
                    {
                        isTypeAlwaysDefined = false;
                    }
                }
            }

            // TODO: If type does not exist, exceptiom must raise, but it must return something for now
            if ((!isTypeAlwaysDefined) && (values.Count <= 0))
            {
                return new MemoryEntry(OutSet.AnyObjectValue);
            }

            return new MemoryEntry(values);
        }

        /// <summary>
        /// Determine whether an expression is instance of directly or indirectly resolved class or interface
        /// </summary>
        /// <param name="expression">Expression to be determined whether it is instance of a class</param>
        /// <param name="typeNames">Possible names of classes or interfaces</param>
        /// <returns>
        /// <c>true</c> whether expression is instance of all the specified types,
        /// <c>false</c> whether it is not instance of any of types and otherwise any boolean value
        /// </returns>
        private MemoryEntry InstanceOfTypeNames(MemoryEntry expression, IEnumerable<QualifiedName> typeNames)
        {
            var isAlwaysInstanceOf = true;
            var isNeverInstanceOf = true;

            foreach (var typeName in typeNames)
            {
                var typeValues = ResolveSourceOrNativeType(typeName);
                if (typeValues == null)
                {
                    // TODO: This must be error
                    SetWarning("Class not found when creating new object");
                    continue;
                }

                foreach (var value in expression.PossibleValues)
                {
                    var objectValue = value as ObjectValue;
                    if (objectValue != null)
                    {
                        // TODO: Resolving of type inheritance
                        // var typeValue = OutSet.ObjectType(objectValue);
                        var isInstanceOf = OutSet.AnyBooleanValue as Value;

                        var booleanValue = isInstanceOf as BooleanValue;
                        if (booleanValue != null)
                        {
                            if (booleanValue.Value)
                            {
                                if (isNeverInstanceOf)
                                {
                                    isNeverInstanceOf = false;
                                    if (!isAlwaysInstanceOf)
                                    {
                                        return new MemoryEntry(OutSet.AnyBooleanValue);
                                    }
                                }
                            }
                            else
                            {
                                if (isAlwaysInstanceOf)
                                {
                                    isAlwaysInstanceOf = false;
                                    if (!isNeverInstanceOf)
                                    {
                                        return new MemoryEntry(OutSet.AnyBooleanValue);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var anyBooleanValue = isInstanceOf as AnyBooleanValue;
                            Debug.Assert(anyBooleanValue != null,
                                "Casting to boolean can result only to boolean value");

                            return new MemoryEntry(anyBooleanValue);
                        }
                    }
                    else
                    {
                        if (isAlwaysInstanceOf)
                        {
                            isAlwaysInstanceOf = false;
                            if (!isNeverInstanceOf)
                            {
                                return new MemoryEntry(OutSet.AnyBooleanValue);
                            }
                        }
                    }
                }
            }

            if (isAlwaysInstanceOf)
            {
                return new MemoryEntry(OutSet.CreateBool(true));
            }
            else
            {
                Debug.Assert(isNeverInstanceOf, "Undecidable cases are solved inside loop");
                return new MemoryEntry(OutSet.CreateBool(false));
            }
        }

        /// <inheritdoc />
        public override MemoryEntry ClassConstant(MemoryEntry thisObject, VariableName variableName)
        {
            List<Value> result = new List<Value>();

            foreach (var value in thisObject.PossibleValues)
            {
                var visitor = new StaticObjectVisitor(OutSet);
                value.Accept(visitor);
                switch (visitor.Result)
                { 
                    case StaticObjectVisitorResult.NO_RESULT:
                        SetWarning("Cannot access constant on non object", AnalysisWarningCause.CANNOT_ACCESS_CONSTANT_ON_NON_OBJECT);
                        result.Add(OutSet.UndefinedValue);
                        break;
                    case StaticObjectVisitorResult.ONE_RESULT:
                        result.AddRange(ClassConstant(visitor.className, variableName).PossibleValues);
                        break;
                    case StaticObjectVisitorResult.MULTIPLE_RESULTS:
                        result.Add(OutSet.AnyValue);
                        break;                
                }
            }

            return new MemoryEntry(result);
        }

        /// <inheritdoc />
        public override MemoryEntry ClassConstant(QualifiedName qualifiedName, VariableName variableName)
        {
            NativeObjectAnalyzer analyzer = NativeObjectAnalyzer.GetInstance(OutSet);
            if (analyzer.ExistClass(qualifiedName))
            {
                var constants = analyzer.GetClass(qualifiedName).Constants;
                ConstantInfo value;
                if (constants.TryGetValue(new FieldIdentifier(qualifiedName, variableName), out value))
                {
                    return value.Value;
                }
                else 
                {
                    SetWarning("Constant " + qualifiedName.Name + "::" + variableName+" doesn't exist", AnalysisWarningCause.CLASS_CONSTANT_DOESNT_EXIST);
                    return new MemoryEntry(OutSet.UndefinedValue);
                }
            }
            else
            {
                var constant=OutSet.GetControlVariable(new VariableName(".class(" + qualifiedName.Name.LowercaseValue + ")->constant(" + variableName.Value + ")"));
                if (constant.IsDefined(OutSet.Snapshot))
                {
                    return constant.ReadMemory(OutSet.Snapshot);
                }
                else 
                {
                    SetWarning("Constant " + qualifiedName.Name + "::" + variableName + " doesnt exist", AnalysisWarningCause.CLASS_CONSTANT_DOESNT_EXIST);
                    return new MemoryEntry(OutSet.UndefinedValue);
                }
            }
        }
    }
}
