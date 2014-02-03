using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using System.IO;
using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.Memory;
using PHP.Core.Reflection;

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
        public override MemoryEntry BinaryEx(MemoryEntry leftOperand, Operations operation,
            MemoryEntry rightOperand)
        {
            binaryOperationVisitor.SetContext(Flow);
            return binaryOperationVisitor.Evaluate(leftOperand, operation, rightOperand);
        }

        /// <inheritdoc />
        public override MemoryEntry UnaryEx(Operations operation, MemoryEntry operand)
        {
            if (operation == Operations.Print)
            {
                if (FlagsHandler.IsDirty(operand.PossibleValues, FlagType.HTMLDirty))
                {
                    AnalysisWarningHandler.SetWarning(OutSet, new AnalysisSecurityWarning(Element, FlagType.HTMLDirty));
                }
            }

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

                var indices = arrayEntry.IterateIndexes(OutSnapshot);
                foreach (var index in indices)
                {
                    int convertedInteger;
                    if (TypeConversion.TryIdentifyInteger(index.DirectName, out convertedInteger))
                    {
                        keys.Add(OutSnapshot.CreateInt(convertedInteger));
                    }
                    else
                    {
                        keys.Add(OutSnapshot.CreateString(index.DirectName));
                    }

                    var indexEntry = arrayEntry.ReadIndex(OutSnapshot, index);
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
            bool isDirty = false;
            stringConverter.SetContext(Flow);
            foreach (var entry in entries)
            {
                isDirty |= FlagsHandler.IsDirty(entry.PossibleValues, FlagType.HTMLDirty);
                bool isAlwaysConcrete;
                stringConverter.Evaluate(entry, out isAlwaysConcrete);
            }

            if (isDirty)
            {
                AnalysisWarningHandler.SetWarning(OutSet, new AnalysisSecurityWarning(Element, FlagType.HTMLDirty));
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

        public void SetWarning(string message, LangElement element, AnalysisWarningCause cause)
        {
            AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(message, element, cause));
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
        public static IEnumerable<TypeValue> ResolveSourceOrNativeType(QualifiedName typeName, FlowOutputSet OutSet, LangElement element)
        {
            var typeValues = OutSet.ResolveType(typeName);
            if (!typeValues.GetEnumerator().MoveNext())
            {
                var objectAnalyzer = NativeObjectAnalyzer.GetInstance(OutSet);
                ClassDecl nativeDeclaration;
                if (objectAnalyzer.TryGetClass(typeName, out nativeDeclaration))
                {
                    foreach (var baseClass in nativeDeclaration.BaseClasses)
                    {
                        if (!OutSet.ResolveType(nativeDeclaration.QualifiedName).GetEnumerator().MoveNext())
                        {
                            OutSet.DeclareGlobal(OutSet.CreateType(nativeDeclaration));
                        }
                    }
                    var type = OutSet.CreateType(nativeDeclaration);
                    OutSet.DeclareGlobal(type);
                    var newTypes = new List<TypeValue>();
                    newTypes.Add(type);
                    typeValues = newTypes;
                }
                else
                {
                    // TODO: This must be error
                    AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning("Class doesn't exist", element, AnalysisWarningCause.CLASS_DOESNT_EXIST));
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
                var typeValues = ResolveSourceOrNativeType(typeName, OutSet, Element);
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
                var typeValues = ResolveSourceOrNativeType(typeName, OutSet, Element);
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
        public override ObjectValue CreateInitializedObject(TypeValue type)
        {
            var newObject = OutSet.CreateObject(type);

            var typeDeclaration = type as TypeValue;
            if (typeDeclaration != null)
            {
                if (typeDeclaration.Declaration.IsAbstract == true)
                {
                    SetWarning("Cannot instantiate abstract class " + typeDeclaration.Declaration.QualifiedName.Name.Value, AnalysisWarningCause.CANNOT_INSTANCIATE_ABSTRACT_CLASS);
                }
                else if (typeDeclaration.Declaration.IsInterface == true)
                {
                    SetWarning("Cannot instantiate interface " + typeDeclaration.Declaration.QualifiedName.Name.Value, AnalysisWarningCause.CANNOT_INSTANCIATE_INTERFACE);
                }
                else
                {
                    var initializer = new ObjectInitializer(this);
                    initializer.InitializeObject(newObject, typeDeclaration.Declaration);
                }

            }

            return newObject;
        }


        /// <inheritdoc />
        public override MemoryEntry ClassConstant(MemoryEntry thisObject, VariableName variableName)
        {
            List<Value> result = new List<Value>();

            foreach (var value in thisObject.PossibleValues)
            {
                var visitor = new StaticObjectVisitor(Flow);
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
                    SetWarning("Constant " + qualifiedName.Name + "::" + variableName + " doesn't exist", AnalysisWarningCause.CLASS_CONSTANT_DOESNT_EXIST);
                    return new MemoryEntry(OutSet.UndefinedValue);
                }
            }
            else
            {
                var constant = OutSet.GetControlVariable(new VariableName(".class(" + qualifiedName.Name.LowercaseValue + ")->constant(" + variableName.Value + ")"));
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

        /// <inheritdoc />
        public override ReadWriteSnapshotEntryBase ResolveStaticField(GenericQualifiedName type, VariableIdentifier field)
        {
            NativeObjectAnalyzer analyzer = NativeObjectAnalyzer.GetInstance(OutSet);
            string value = type.QualifiedName.Name.Value;
            IEnumerable<QualifiedName> resolverdTypes=FunctionResolver.ResolveType(type.QualifiedName, OutSet, Element);
            if (resolverdTypes.Count() > 0)
            {
                if (!analyzer.ExistClass(resolverdTypes.First()))
                {
                    var classStorage = OutSet.ReadControlVariable(FunctionResolver.staticVariables).ReadIndex(OutSet.Snapshot, new MemberIdentifier(resolverdTypes.First().Name.LowercaseValue));
                    if (!classStorage.IsDefined(OutSet.Snapshot))
                    {
                        AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning("Class " + resolverdTypes.First().Name.Value + " doesn't exist", Element, AnalysisWarningCause.CLASS_DOESNT_EXIST));
                        return getStaticVariableSink();
                    }
                }
              

                var list = new List<QualifiedName>();
                list.Add(resolverdTypes.First());
                return resolveStaticVariable(list, field);
            }
            else
            {
                return getStaticVariableSink();
            }
        }

        /// <inheritdoc />
        public override ReadWriteSnapshotEntryBase ResolveIndirectStaticField(IEnumerable<GenericQualifiedName> possibleTypes, VariableIdentifier field)
        {
            NativeObjectAnalyzer analyzer = NativeObjectAnalyzer.GetInstance(OutSet);
            List<QualifiedName> classes = new List<QualifiedName>();
            foreach (var name in possibleTypes)
            {
                if (!analyzer.ExistClass(name.QualifiedName))
                {
                    var classStorage = OutSet.ReadControlVariable(FunctionResolver.staticVariables).ReadIndex(OutSet.Snapshot, new MemberIdentifier(name.QualifiedName.Name.LowercaseValue));
                    if (!classStorage.IsDefined(OutSet.Snapshot))
                    {
                        AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning("Class " + name.QualifiedName.Name.Value + " doesn't exist", Element, AnalysisWarningCause.CLASS_DOESNT_EXIST));
                    }
                    else
                    {
                        classes.Add(name.QualifiedName);
                    }
                }
                else
                {
                    classes.Add(name.QualifiedName);
                }
            }

            return resolveStaticVariable(classes, field);
        }

        private ReadWriteSnapshotEntryBase resolveStaticVariable(List<QualifiedName> classes, VariableIdentifier field)
        {
            List<string> names = new List<string>();

            foreach (var typeName in classes)
            {
                var classStorage = OutSet.ReadControlVariable(FunctionResolver.staticVariables).ReadIndex(OutSet.Snapshot, new MemberIdentifier(typeName.Name.LowercaseValue));
                if (!classStorage.IsDefined(OutSet.Snapshot))
                {
                    NativeObjectAnalyzer analyzer = NativeObjectAnalyzer.GetInstance(OutSet);
                    if (analyzer.ExistClass(typeName) && analyzer.GetClass(typeName).IsInterface == false)
                    {
                        InsertNativeObjectStaticVariablesIntoMM(typeName);
                        classStorage = OutSet.ReadControlVariable(FunctionResolver.staticVariables).ReadIndex(OutSet.Snapshot, new MemberIdentifier(typeName.Name.LowercaseValue));
                        if (classStorage.ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.DirectName.Value)).IsDefined(OutSet.Snapshot))
                        {
                            names.Add(typeName.Name.LowercaseValue);
                        }
                        else
                        {
                            AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning("Static variable " + typeName.Name.Value + "::" + field.DirectName.Value + " wasn't declared", Element, AnalysisWarningCause.STATIC_VARIABLE_DOESNT_EXIST));                   
                        }
                    }
                    else
                    {
                        AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning("Class " + typeName.Name.Value + " doesn't exist", Element, AnalysisWarningCause.CLASS_DOESNT_EXIST));
                    }
                }
                else
                {
                    if (classStorage.ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.DirectName.Value)).IsDefined(OutSet.Snapshot))
                    {
                        names.Add(typeName.Name.LowercaseValue);
                    }
                    else
                    {
                        AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning("Static variable " + typeName.Name.Value + "::" + field.DirectName.Value + " wasn't declared", Element, AnalysisWarningCause.STATIC_VARIABLE_DOESNT_EXIST));
                    }
                }
            }

             var storage = OutSet.ReadControlVariable(FunctionResolver.staticVariables).ReadIndex(OutSet.Snapshot, new MemberIdentifier(names));
             return storage.ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.DirectName.Value));

        }




        /// <inheritdoc />
        public override IEnumerable<GenericQualifiedName> TypeNames(MemoryEntry typeValue)
        {
            List<GenericQualifiedName> result = new List<GenericQualifiedName>();
            foreach(var value in typeValue.PossibleValues)
            {
                StaticObjectVisitor visitor = new StaticObjectVisitor(Flow);
                value.Accept(visitor);
                switch (visitor.Result)
                {
                    case StaticObjectVisitorResult.NO_RESULT:
                        AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning("Cannot acces static variable on non object", Element, AnalysisWarningCause.CANNOT_ACCES_STATIC_VARIABLE_OM_NON_OBJECT));
                        break;
                    case StaticObjectVisitorResult.ONE_RESULT:
                        result.Add(new GenericQualifiedName(visitor.className));
                        break;
                    case StaticObjectVisitorResult.MULTIPLE_RESULTS:
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns static variable sink, when static variable doesn't exist, this empty space in memory model is returned.
        /// </summary>
        /// <returns>Empty space in memory model</returns>
        private ReadWriteSnapshotEntryBase getStaticVariableSink()
        {
            OutSet.GetControlVariable(FunctionResolver.staticVariableSink).WriteMemory(OutSet.Snapshot, new MemoryEntry(OutSet.UndefinedValue));
            return OutSet.GetControlVariable(FunctionResolver.staticVariableSink);
        }


        #region Object Model

        /// <inheritdoc />
        public override void DeclareGlobal(TypeDecl declaration)
        {
            var objectAnalyzer = NativeObjectAnalyzer.GetInstance(Flow.OutSet);
            ClassDeclBuilder type = convertToClassDecl(declaration);

            foreach (var method in type.SourceCodeMethods)
            {
                FunctionResolver.methodToClass.Add(method.Value.DeclaringElement, type.QualifiedName);
            }
            if (objectAnalyzer.ExistClass(declaration.Type.QualifiedName))
            {
                SetWarning("Cannot redeclare class/interface " + declaration.Type.QualifiedName, AnalysisWarningCause.CLASS_ALLREADY_EXISTS);
            }
            else if (OutSet.ResolveType(declaration.Type.QualifiedName).Count() != 0)
            {
                SetWarning("Cannot redeclare class/interface " + declaration.Type.QualifiedName, AnalysisWarningCause.CLASS_ALLREADY_EXISTS);
            }
            else
            {
                if (type.IsInterface)
                {
                    DeclareInterface(declaration, objectAnalyzer, type);
                }
                else
                {
                    if (declaration.BaseClassName != null)
                    {
                        if (objectAnalyzer.ExistClass(declaration.BaseClassName.Value.QualifiedName))
                        {
                            ClassDecl baseClass = objectAnalyzer.GetClass(declaration.BaseClassName.Value.QualifiedName);
                            if (baseClass.IsInterface == true)
                            {
                                SetWarning("Class " + declaration.BaseClassName.Value.QualifiedName + " not found", AnalysisWarningCause.CLASS_DOESNT_EXIST);
                            }
                            else if (baseClass.IsFinal)
                            {
                                SetWarning("Cannot extend final class " + declaration.Type.QualifiedName, AnalysisWarningCause.FINAL_CLASS_CANNOT_BE_EXTENDED);
                            }
                            else
                            {
                                ClassDeclBuilder newType = CopyInfoFromBaseClass(baseClass, type);
                                ClassDecl finalNewType = checkClassAndCopyConstantsFromInterfaces(newType, declaration);
                                insetStaticVariablesIntoMM(finalNewType);
                                OutSet.DeclareGlobal(OutSet.CreateType(finalNewType));
                            }
                        }
                        else
                        {
                            IEnumerable<TypeValue> types = OutSet.ResolveType(declaration.BaseClassName.Value.QualifiedName);
                            if (types.Count() == 0)
                            {
                                SetWarning("Class " + declaration.BaseClassName.Value.QualifiedName + " not found", AnalysisWarningCause.CLASS_DOESNT_EXIST);
                            }
                            else
                            {
                                foreach (var value in types)
                                {
                                    if (value is TypeValue)
                                    {
                                        if ((value as TypeValue).Declaration.IsInterface)
                                        {
                                            SetWarning("Class " + (value as TypeValue).Declaration.QualifiedName.Name + " not found", AnalysisWarningCause.CLASS_DOESNT_EXIST);
                                        }
                                        else if ((value as TypeValue).Declaration.IsFinal)
                                        {
                                            SetWarning("Cannot extend final class " + declaration.Type.QualifiedName, AnalysisWarningCause.FINAL_CLASS_CANNOT_BE_EXTENDED);
                                        }
                                        ClassDeclBuilder newType = CopyInfoFromBaseClass((value as TypeValue).Declaration, type);
                                        ClassDecl finalNewType = checkClassAndCopyConstantsFromInterfaces(newType, declaration);
                                        insetStaticVariablesIntoMM(finalNewType);
                                        OutSet.DeclareGlobal(OutSet.CreateType(finalNewType));
                                    }
                                    else
                                    {
                                        SetWarning("Class " + declaration.BaseClassName.Value.QualifiedName + " not found", AnalysisWarningCause.CLASS_DOESNT_EXIST);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        ClassDecl finalType = checkClassAndCopyConstantsFromInterfaces(type, declaration);
                        insetStaticVariablesIntoMM(finalType);
                        OutSet.DeclareGlobal(OutSet.CreateType(finalType));
                    }
                }
            }
        }


        private ClassDecl checkClassAndCopyConstantsFromInterfaces(ClassDeclBuilder type, TypeDecl element)
        {
            foreach (var entry in type.SourceCodeMethods)
            {
                if (entry.Key.ClassName.Equals(type.QualifiedName))
                {
                    MethodDecl method = entry.Value.MethodDecl;
                    if (method.Modifiers.HasFlag(PhpMemberAttributes.Abstract))
                    {
                        if (method.Body != null)
                        {
                            SetWarning("Abstract method cannot have body", element, AnalysisWarningCause.ABSTRACT_METHOD_CANNOT_HAVE_BODY);
                        }
                    }
                    else
                    {
                        if (method.Body == null)
                        {
                            SetWarning("Non abstract method must have body", element, AnalysisWarningCause.NON_ABSTRACT_METHOD_MUST_HAVE_BODY);
                        }
                    }
                }
            }
            //cannot contain abstract method
            if (type.IsAbstract == false)
            {
                Dictionary<Name, bool> methods = new Dictionary<Name, bool>();

                foreach (var entry in type.SourceCodeMethods)
                {
                    if (methods.ContainsKey(entry.Key.Name))
                    {
                        methods[entry.Key.Name] &= entry.Value.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Abstract);
                    }
                    else
                    {
                        methods.Add(entry.Key.Name, entry.Value.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Abstract));
                    }
                }

                foreach (var entry in type.ModeledMethods)
                {
                    if (methods.ContainsKey(entry.Key.Name))
                    {
                        methods[entry.Key.Name] &= entry.Value.IsAbstract;
                    }
                    else
                    {
                        methods.Add(entry.Key.Name, entry.Value.IsAbstract);
                    }
                }

                foreach (var entry in methods)
                {
                    if (entry.Value == true)
                    {
                        SetWarning("Non abstract class cannot contain abstract method " + entry.Key, element, AnalysisWarningCause.NON_ABSTRACT_CLASS_CONTAINS_ABSTRACT_METHOD);
                    }
                }
            }

            List<ClassDecl> interfaces = getImplementedInterfaces(element);
            foreach (var Interface in interfaces)
            {
                foreach (var constant in Interface.Constants)
                {
                    var query = type.Constants.Where(a => a.Key.Name == constant.Key.Name);
                    if (query.Count() > 0)
                    {
                        SetWarning("Cannot override interface constant " + constant.Key.Name, element, AnalysisWarningCause.CANNOT_OVERRIDE_INTERFACE_CONSTANT);
                    }
                    else
                    {
                        type.Constants.Add(new FieldIdentifier(type.QualifiedName, constant.Key.Name), constant.Value.CloneWithNewQualifiedName(type.QualifiedName));
                    }
                }

                foreach (var method in Interface.ModeledMethods)
                {
                    if (!type.SourceCodeMethods.ContainsKey(new MethodIdentifier(type.QualifiedName, method.Key.Name)))
                    {
                        SetWarning("Class " + type.QualifiedName + " doesn't implement method " + method.Key.Name, element, AnalysisWarningCause.CLASS_DOENST_IMPLEMENT_ALL_INTERFACE_METHODS);
                    }
                    else
                    {
                        var classMethod = type.SourceCodeMethods[new MethodIdentifier(type.QualifiedName, method.Key.Name)];
                        checkIfStaticMatch(method.Value, classMethod.MethodDecl, element);
                        if (!AreMethodsCompatible(classMethod.MethodDecl, method.Value))
                        {
                            SetWarning("Can't inherit abstract function " + classMethod.MethodDecl.Name + " beacuse arguments doesn't match", classMethod.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                        }
                    }
                }

                foreach (var method in Interface.SourceCodeMethods)
                {
                    if (!type.SourceCodeMethods.ContainsKey(new MethodIdentifier(type.QualifiedName, method.Key.Name)))
                    {
                        SetWarning("Class " + type.QualifiedName + " doesn't implement method " + method.Key.Name, element, AnalysisWarningCause.CLASS_DOENST_IMPLEMENT_ALL_INTERFACE_METHODS);
                    }
                    else
                    {
                        var classMethod = type.SourceCodeMethods[new MethodIdentifier(type.QualifiedName, method.Key.Name)];

                        checkIfStaticMatch(method.Value.MethodDecl, classMethod.
                            MethodDecl, element);
                        if (!AreMethodsCompatible(classMethod.MethodDecl, method.Value.MethodDecl))
                        {
                            SetWarning("Can't inherit abstract function " + classMethod.MethodDecl.Name + " beacuse arguments doesn't match", classMethod.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                        }
                    }
                }
            }

            return type.Build();
        }

        private void DeclareInterface(TypeDecl declaration, NativeObjectAnalyzer objectAnalyzer, ClassDeclBuilder type)
        {
            ClassDeclBuilder result = new ClassDeclBuilder();
            result.QualifiedName = type.QualifiedName;
            result.IsInterface = true;
            result.IsFinal = false;
            result.IsAbstract = false;
            result.Constants = type.Constants;
            result.SourceCodeMethods = new Dictionary<MethodIdentifier, FunctionValue>(type.SourceCodeMethods);
            result.ModeledMethods = new Dictionary<MethodIdentifier, MethodInfo>(type.ModeledMethods);


            if (type.Fields.Count != 0)
            {
                SetWarning("Interface cannot contain fields", AnalysisWarningCause.INTERFACE_CANNOT_CONTAIN_FIELDS);
            }

            foreach (var method in type.SourceCodeMethods.Values)
            {
                if (method.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Private) || method.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Protected))
                {
                    SetWarning("Interface method must be public", method.MethodDecl, AnalysisWarningCause.INTERFACE_METHOD_MUST_BE_PUBLIC);
                }
                if (method.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Final))
                {
                    SetWarning("Interface method cannot be final", method.MethodDecl, AnalysisWarningCause.INTERFACE_METHOD_CANNOT_BE_FINAL);
                }
                if (method.MethodDecl.Body != null)
                {
                    SetWarning("Interface method cannot have body", method.MethodDecl, AnalysisWarningCause.INTERFACE_METHOD_CANNOT_HAVE_IMPLEMENTATION);
                }
            }

            List<ClassDecl> interfaces = getImplementedInterfaces(declaration);

            if (interfaces.Count != 0)
            {
                foreach (var value in interfaces)
                {
                    if (value.IsInterface == false)
                    {
                        SetWarning("Interface " + value.QualifiedName + " not found", AnalysisWarningCause.INTERFACE_DOESNT_EXIST);
                    }
                    else
                    {
                        //interface cannot have implement
                        foreach (var method in value.SourceCodeMethods.Values)
                        {
                            if (result.ModeledMethods.Values.Where(a => a.Name == method.MethodDecl.Name).Count() > 0 || result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                            {
                                if (result.ModeledMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                                {
                                    var match = result.ModeledMethods.Values.Where(a => a.Name == method.Name).First();
                                    if (!AreMethodsCompatible(match, method.MethodDecl))
                                    {
                                        SetWarning("Can't inherit abstract function " + method.MethodDecl.Name + " beacuse arguments doesn't match", method.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                                    }
                                    checkIfStaticMatch(method.MethodDecl, match, declaration);
                                }
                                else if (result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                                {
                                    var match = result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).First();
                                    if (!AreMethodsCompatible(match.MethodDecl, method.MethodDecl))
                                    {
                                        SetWarning("Can't inherit abstract function " + method.MethodDecl.Name + " beacuse arguments doesn't match", method.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                                    }
                                    checkIfStaticMatch(method.MethodDecl, match.MethodDecl, declaration);
                                }

                            }
                            else
                            {
                                result.SourceCodeMethods.Add(new MethodIdentifier(result.QualifiedName, method.Name), method);
                            }
                        }
                        foreach (var method in value.ModeledMethods.Values)
                        {
                            if (result.ModeledMethods.Values.Where(a => a.Name == method.Name).Count() > 0 || result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                            {

                                if (result.ModeledMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                                {
                                    var match = result.ModeledMethods.Values.Where(a => a.Name == method.Name).First();
                                    if (!AreMethodsCompatible(match, method))
                                    {
                                        SetWarning("Can't inherit abstract function " + method.Name + " beacuse arguments doesn't match", declaration, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                                    }
                                    checkIfStaticMatch(method, match, declaration);
                                }
                                else if (result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).Count() > 0)
                                {
                                    var match = result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).First();
                                    if (!AreMethodsCompatible(match.MethodDecl, method))
                                    {
                                        SetWarning("Can't inherit abstract function " + method.Name + " beacuse arguments doesn't match", match.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                                    }
                                    checkIfStaticMatch(method, match.MethodDecl, declaration);
                                }

                            }
                            else
                            {
                                result.ModeledMethods.Add(new MethodIdentifier(result.QualifiedName, method.Name), method);
                            }
                        }

                        foreach (var constant in value.Constants.Values)
                        {
                            var query = type.Constants.Values.Where(a => a.Name.Equals(constant.Name));
                            if (query.Count() > 0)
                            {
                                SetWarning("Cannot override interface constant " + constant.Name, declaration, AnalysisWarningCause.CANNOT_OVERRIDE_INTERFACE_CONSTANT);
                            }
                            else
                            {
                                type.Constants.Add(new FieldIdentifier(value.QualifiedName, constant.Name), constant);
                            }
                        }
                    }
                }
            }

            var finalResult = result.Build();
            insetStaticVariablesIntoMM(finalResult);
            OutSet.DeclareGlobal(OutSet.CreateType(finalResult));
        }

        private List<ClassDecl> getImplementedInterfaces(TypeDecl declaration)
        {
            NativeObjectAnalyzer objectAnalyzer = NativeObjectAnalyzer.GetInstance(Flow.OutSet);
            List<ClassDecl> interfaces = new List<ClassDecl>();
            foreach (GenericQualifiedName Interface in declaration.ImplementsList)
            {
                if (objectAnalyzer.ExistClass(Interface.QualifiedName))
                {
                    var interfaceType = objectAnalyzer.GetClass(Interface.QualifiedName);
                    interfaces.Add(interfaceType);
                }
                else if (OutSet.ResolveType(Interface.QualifiedName).Count() == 0)
                {
                    SetWarning("Interface " + Interface.QualifiedName + " not found", AnalysisWarningCause.INTERFACE_DOESNT_EXIST);
                }
                else
                {
                    foreach (var interfaceValue in OutSet.ResolveType(Interface.QualifiedName))
                    {
                        if (interfaceValue is TypeValue)
                        {
                            interfaces.Add((interfaceValue as TypeValue).Declaration);
                        }
                        else
                        {
                            SetWarning("Interface " + Interface.QualifiedName + " not found", AnalysisWarningCause.INTERFACE_DOESNT_EXIST);
                        }
                    }
                }
            }
            return interfaces;
        }

        private bool AreMethodsCompatible(MethodDecl a, MethodDecl b)
        {
            if (a.Signature.FormalParams.Count == b.Signature.FormalParams.Count)
            {
                for (int i = 0; i < a.Signature.FormalParams.Count; i++)
                {
                    if (a.Signature.FormalParams[i].PassedByRef != b.Signature.FormalParams[i].PassedByRef)
                    {
                        return false;
                    }
                    if (a.Signature.FormalParams[i].InitValue == null ^ a.Signature.FormalParams[i].InitValue == null)
                    {
                        return false;

                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool AreMethodsCompatible(MethodDecl a, MethodInfo b)
        {
            return AreMethodsCompatible(b, a);
        }

        private bool AreMethodsCompatible(MethodInfo a, MethodDecl b)
        {
            if (a.Arguments.Count == b.Signature.FormalParams.Count)
            {
                for (int i = 0; i < a.Arguments.Count; i++)
                {
                    if (a.Arguments[i].ByReference != b.Signature.FormalParams[i].PassedByRef)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool AreMethodsCompatible(MethodInfo a, MethodInfo b)
        {
            if (a.Arguments.Count == b.Arguments.Count)
            {
                for (int i = 0; i < a.Arguments.Count; i++)
                {
                    if (a.Arguments[i].ByReference != b.Arguments[i].ByReference)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool checkIfStaticMatch(MethodInfo method, MethodDecl overridenMethod, LangElement element)
        {
            if (overridenMethod.Modifiers.HasFlag(PhpMemberAttributes.Static) != method.IsStatic)
            {
                if (method.IsStatic)
                {
                    SetWarning("Cannot redeclare static method with non static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_STATIC_METHOD_WITH_NON_STATIC);
                }
                else
                {
                    SetWarning("Cannot redeclare non static method with static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC);
                }
                return false;
            }
            return true;
        }

        private bool checkIfStaticMatch(MethodInfo method, MethodInfo overridenMethod, LangElement element)
        {
            if (overridenMethod.IsStatic != method.IsStatic)
            {
                if (method.IsStatic)
                {
                    SetWarning("Cannot redeclare static method with non static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_STATIC_METHOD_WITH_NON_STATIC);
                }
                else
                {
                    SetWarning("Cannot redeclare non static method with static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC);
                }
                return false;
            }
            return true;
        }

        private bool checkIfStaticMatch(MethodDecl method, MethodDecl overridenMethod, LangElement element)
        {
            if (overridenMethod.Modifiers.HasFlag(PhpMemberAttributes.Static) != method.Modifiers.HasFlag(PhpMemberAttributes.Static))
            {
                if (method.Modifiers.HasFlag(PhpMemberAttributes.Static))
                {
                    SetWarning("Cannot redeclare static method with non static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_STATIC_METHOD_WITH_NON_STATIC);
                }
                else
                {
                    SetWarning("Cannot redeclare non static method with static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC);
                } return false;
            }
            return true;
        }

        private bool checkIfStaticMatch(MethodDecl method, MethodInfo overridenMethod, LangElement element)
        {
            if (overridenMethod.IsStatic != method.Modifiers.HasFlag(PhpMemberAttributes.Static))
            {
                if (method.Modifiers.HasFlag(PhpMemberAttributes.Static))
                {
                    SetWarning("Cannot redeclare static method with non static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_STATIC_METHOD_WITH_NON_STATIC);
                }
                else
                {
                    SetWarning("Cannot redeclare non static method with static " + method.Name, element, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC);
                }
                return false;
            }
            return true;
        }

        private ClassDeclBuilder CopyInfoFromBaseClass(ClassDecl baseClass, ClassDeclBuilder currentClass)
        {

            ClassDeclBuilder result = new ClassDeclBuilder();
            result.Fields = new Dictionary<FieldIdentifier, FieldInfo>(baseClass.Fields);
            result.Constants = new Dictionary<FieldIdentifier, ConstantInfo>(baseClass.Constants);
            result.SourceCodeMethods = new Dictionary<MethodIdentifier, FunctionValue>(baseClass.SourceCodeMethods);
            result.ModeledMethods = new Dictionary<MethodIdentifier, MethodInfo>(baseClass.ModeledMethods);
            result.QualifiedName = currentClass.QualifiedName;
            result.BaseClasses = new List<QualifiedName>(baseClass.BaseClasses);
            result.BaseClasses.Add(baseClass.QualifiedName);
            result.IsFinal = currentClass.IsFinal;
            result.IsInterface = currentClass.IsInterface;
            result.IsAbstract = currentClass.IsAbstract;

            foreach (var field in currentClass.Fields)
            {
                var query = result.Fields.Keys.Where(a => a.Name == field.Key.Name);
                FieldIdentifier newFieldIdentifier = new FieldIdentifier(result.QualifiedName, field.Key.Name);
                if (query.Count() == 0)
                {
                    result.Fields.Add(newFieldIdentifier, field.Value);
                }
                else
                {
                    FieldIdentifier fieldIdentifier = query.First();
                    if (result.Fields[fieldIdentifier].IsStatic != field.Value.IsStatic)
                    {
                        var fieldName = result.Fields[fieldIdentifier].Name;
                        if (field.Value.IsStatic)
                        {
                            SetWarning("Cannot redeclare non static " + fieldName + " with static " + fieldName, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_FIELD_WITH_STATIC);
                        }
                        else
                        {
                            SetWarning("Cannot redeclare static " + fieldName + " with non static " + fieldName, AnalysisWarningCause.CANNOT_REDECLARE_STATIC_FIELD_WITH_NON_STATIC);

                        }
                    }
                    else
                    {
                        result.Fields.Add(newFieldIdentifier, field.Value);
                    }
                }
            }

            foreach (var constant in currentClass.Constants)
            {
                result.Constants.Add(constant.Key, constant.Value);
            }
            //todo test method overriding
            foreach (var method in currentClass.SourceCodeMethods.Values)
            {
                MethodIdentifier methodIdentifier = new MethodIdentifier(result.QualifiedName, method.Name);
                if (result.SourceCodeMethods.Values.Where(a => a.Name == method.Name).Count() == 0)
                {
                    if (result.ModeledMethods.Values.Where(a => a.Name == method.Name).Count() == 0)
                    {
                        result.SourceCodeMethods.Add(methodIdentifier, method);
                    }
                    else
                    {
                        var overridenMethod = result.ModeledMethods.Values.Where(a => a.Name == method.Name).First();
                        bool containsErrors = false;
                        if (overridenMethod.IsFinal)
                        {
                            SetWarning("Cannot redeclare final method " + method.MethodDecl.Name, method.MethodDecl, AnalysisWarningCause.CANNOT_REDECLARE_FINAL_METHOD);
                            containsErrors = true;
                        }
                        if (!checkIfStaticMatch(method.MethodDecl, overridenMethod, method.MethodDecl))
                        {
                            containsErrors = true;
                        }

                        if (!AreMethodsCompatible(overridenMethod, method.MethodDecl))
                        {
                            SetWarning("Can't inherit function " + method.MethodDecl.Name + ", beacuse arguments doesn't match", method.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                            containsErrors = true;
                        }
                        if (method.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Abstract))
                        {
                            SetWarning("Can't override function " + method.MethodDecl.Name + ", with abstract function", method.MethodDecl, AnalysisWarningCause.CANNOT_OVERRIDE_FUNCTION_WITH_ABSTRACT);
                            containsErrors = true;
                        }
                        if (containsErrors == false)
                        {
                            result.SourceCodeMethods[new MethodIdentifier(currentClass.QualifiedName, method.Name)] = method;
                            result.SourceCodeMethods.Remove(new MethodIdentifier(overridenMethod.ClassName, overridenMethod.Name));
                        }
                    }
                }
                else
                {
                    var key = result.SourceCodeMethods.Keys.Where(a => a.Name == method.Name).First();
                    var overridenMethod = result.SourceCodeMethods[key];
                    bool containsErrors = false;
                    if (overridenMethod.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Final))
                    {
                        SetWarning("Cannot redeclare final method " + method.MethodDecl.Name, method.MethodDecl, AnalysisWarningCause.CANNOT_REDECLARE_FINAL_METHOD);
                        containsErrors = true;
                    }

                    if (!checkIfStaticMatch(method.MethodDecl, overridenMethod.MethodDecl, method.MethodDecl))
                    {
                        containsErrors = true;
                    }

                    if (!AreMethodsCompatible(overridenMethod.MethodDecl, method.MethodDecl))
                    {
                        SetWarning("Can't inherit function " + method.MethodDecl.Name + ", beacuse arguments doesn't match", method.MethodDecl, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION);
                        containsErrors = true;
                    }
                    if (method.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Abstract))
                    {
                        SetWarning("Can't override function " + method.MethodDecl.Name + ", with abstract function", method.MethodDecl, AnalysisWarningCause.CANNOT_OVERRIDE_FUNCTION_WITH_ABSTRACT);
                        containsErrors = true;
                    }
                    if (containsErrors == false)
                    {
                        result.SourceCodeMethods.Remove(key);
                        result.SourceCodeMethods[new MethodIdentifier(currentClass.QualifiedName, method.Name)] = method;
                    }
                }
            }

            return result;
        }

        private ClassDeclBuilder convertToClassDecl(TypeDecl declaration)
        {
            ClassDeclBuilder result = new ClassDeclBuilder();
            result.BaseClasses = new List<QualifiedName>();
            if (declaration.BaseClassName.HasValue)
            {
                result.BaseClasses.Add(declaration.BaseClassName.Value.QualifiedName);
            }
            result.IsFinal = declaration.Type.IsFinal;
            result.IsInterface = declaration.Type.IsInterface;
            result.IsAbstract = declaration.Type.IsAbstract;
            result.QualifiedName = new QualifiedName(declaration.Name);

            foreach (var member in declaration.Members)
            {
                if (member is FieldDeclList)
                {
                    foreach (FieldDecl field in (member as FieldDeclList).Fields)
                    {
                        Visibility visibility;
                        if (member.Modifiers.HasFlag(PhpMemberAttributes.Private))
                        {
                            visibility = Visibility.PRIVATE;
                        }
                        else if (member.Modifiers.HasFlag(PhpMemberAttributes.Protected))
                        {
                            visibility = Visibility.PROTECTED;
                        }
                        else
                        {
                            visibility = Visibility.PUBLIC;
                        }
                        bool isStatic = member.Modifiers.HasFlag(PhpMemberAttributes.Static);
                        //multiple declaration of fields
                        if (result.Fields.ContainsKey(new FieldIdentifier(result.QualifiedName, field.Name)))
                        {
                            SetWarning("Cannot redeclare field " + field.Name, member, AnalysisWarningCause.CLASS_MULTIPLE_FIELD_DECLARATION);
                        }
                        else
                        {
                            result.Fields.Add(new FieldIdentifier(result.QualifiedName, field.Name), new FieldInfo(field.Name, result.QualifiedName, "any", visibility, field.Initializer, isStatic));
                        }
                    }

                }
                else if (member is ConstDeclList)
                {
                    foreach (var constant in (member as ConstDeclList).Constants)
                    {
                        if (result.Constants.ContainsKey(new FieldIdentifier(result.QualifiedName, constant.Name)))
                        {
                            SetWarning("Cannot redeclare constant " + constant.Name, member, AnalysisWarningCause.CLASS_MULTIPLE_CONST_DECLARATION);
                        }
                        else
                        {
                            //in php all object constatns are public
                            Visibility visbility = Visibility.PUBLIC;
                            result.Constants.Add(new FieldIdentifier(result.QualifiedName, constant.Name), new ConstantInfo(constant.Name, result.QualifiedName, visbility, constant.Initializer));
                        }
                    }
                }
                else if (member is MethodDecl)
                {
                    var methosIdentifier = new MethodIdentifier(result.QualifiedName, (member as MethodDecl).Name);
                    if (!result.SourceCodeMethods.ContainsKey(methosIdentifier))
                    {
                        result.SourceCodeMethods.Add(methosIdentifier, OutSet.CreateFunction(member as MethodDecl));
                    }
                    else
                    {
                        SetWarning("Cannot redeclare method " + (member as MethodDecl).Name, member, AnalysisWarningCause.CLASS_MULTIPLE_FUNCTION_DECLARATION);
                    }
                }
                else
                {
                    //ignore traits are not supported by AST, only by parser
                }
            }

            return result;
        }

        private void insetStaticVariablesIntoMM(ClassDecl result)
        {
            Dictionary<VariableName, ConstantInfo> constants = new Dictionary<VariableName, ConstantInfo>();
            List<QualifiedName> classes = new List<QualifiedName>(result.BaseClasses);
            classes.Add(result.QualifiedName);

            NativeObjectAnalyzer objectAnalyzer = NativeObjectAnalyzer.GetInstance(Flow.OutSet);

            foreach (var currentClass in classes)
            {
                foreach (var constant in result.Constants.Values.Where(a => a.ClassName == currentClass))
                {
                    constants[constant.Name] = constant;
                }
            }

            var initiliazer = new ObjectInitializer(this);
            foreach (var constant in constants.Values)
            {

                var variable = OutSet.GetControlVariable(new VariableName(".class(" + result.QualifiedName.Name.LowercaseValue + ")->constant(" + constant.Name + ")"));
                List<Value> constantValues = new List<Value>();
                if (variable.IsDefined(OutSet.Snapshot))
                {
                    constantValues.AddRange(variable.ReadMemory(OutSet.Snapshot).PossibleValues);
                }
                if (constant.Value != null)
                {
                    constantValues.AddRange(constant.Value.PossibleValues);
                    variable.WriteMemory(OutSet.Snapshot, new MemoryEntry(constantValues));
                }
                else
                {
                    string index = ".class(" + result.QualifiedName.Name.LowercaseValue + ")->constant(" + constant.Name + ")";
                    constant.Initializer.VisitMe(initiliazer);
                    variable.WriteMemory(OutSet.Snapshot, initiliazer.initializationValue);
                }
            }
            if (result.IsInterface == false)
            {
                var staticVariables = OutSet.GetControlVariable(FunctionResolver.staticVariables).ReadIndex(OutSet.Snapshot, new MemberIdentifier(result.QualifiedName.Name.LowercaseValue));
                staticVariables.WriteMemory(OutSet.Snapshot, new MemoryEntry(OutSet.CreateArray()));


                if (result.BaseClasses.Count > 0)
                {
                    for (int i = 0; i<result.BaseClasses.Count ; i++)
                    {
                        if (Flow.OutSet.ResolveType(result.BaseClasses[i]).Count() == 0)
                        {
                            InsertNativeObjectStaticVariablesIntoMM(result.BaseClasses[i]);

                        }
                    }
                }

                foreach (var field in result.Fields.Values.Where(a => (a.IsStatic == true && a.ClassName != result.QualifiedName)))
                {
                    var baseClassVariable = OutSet.GetControlVariable(FunctionResolver.staticVariables).ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.ClassName.Name.LowercaseValue)).ReadIndex(OutSet.Snapshot,new MemberIdentifier(field.Name.Value));

                    var currentClassVariable = OutSet.GetControlVariable(FunctionResolver.staticVariables).ReadIndex(OutSet.Snapshot, new MemberIdentifier(result.QualifiedName.Name.LowercaseValue)).ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.Name.Value));
                    if (result.Fields.Values.Where(a => (a.IsStatic == true && a.ClassName == result.QualifiedName && a.Name == field.Name)).Count() == 0)
                    {
                        currentClassVariable.SetAliases(OutSet.Snapshot, baseClassVariable);
                    }
                }
                HashSet<VariableName> usedFields = new HashSet<VariableName>();
                foreach (var field in result.Fields.Values.Where(a => (a.IsStatic == true && a.ClassName == result.QualifiedName)))
                {
                    usedFields.Add(field.Name);
                    if (field.Initializer != null)
                    {
                        field.Initializer.VisitMe(initiliazer);
                        staticVariables.ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.Name.Value)).WriteMemory(OutSet.Snapshot, initiliazer.initializationValue);
                    }
                    else
                    {
                        MemoryEntry fieldValue;
                        if (field.InitValue != null)
                        {
                            fieldValue = field.InitValue;
                        }
                        else
                        {
                            fieldValue = new MemoryEntry(OutSet.UndefinedValue);
                        }

                        staticVariables.ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.Name.Value)).WriteMemory(OutSet.Snapshot, fieldValue);
                    }
                }

                //insert parent values
                for (int i = result.BaseClasses.Count - 1; i >= 0; i--)
                {
                    foreach (var field in result.Fields.Values.Where(a => (a.IsStatic == true && a.ClassName == result.BaseClasses[i])))
                    {
                        if (usedFields.Contains(field.Name))
                        {
                            //add
                            usedFields.Add(field.Name);
                        }
                    }
                }
            }
        }

        private void InsertNativeObjectStaticVariablesIntoMM(QualifiedName qualifiedName)
        {
            NativeObjectAnalyzer objectAnalyzer = NativeObjectAnalyzer.GetInstance(OutSet);
            Debug.Assert(objectAnalyzer.ExistClass(qualifiedName));
            ClassDecl classs = objectAnalyzer.GetClass(qualifiedName);
            List<QualifiedName> classHierarchy = new List<QualifiedName>(classs.BaseClasses);
            classHierarchy.Add(qualifiedName);
            foreach (var name in classHierarchy)
            {
                ClassDecl currentClass = objectAnalyzer.GetClass(name);
                var staticVariables = OutSet.GetControlVariable(FunctionResolver.staticVariables).ReadIndex(OutSet.Snapshot, new MemberIdentifier(name.Name.LowercaseValue));
                if (!staticVariables.IsDefined(OutSet.Snapshot))
                {
                    staticVariables.WriteMemory(OutSet.Snapshot, new MemoryEntry(OutSet.CreateArray()));

                    foreach (var field in currentClass.Fields.Values.Where(a => (a.ClassName != currentClass.QualifiedName && a.IsStatic == true)))
                    {
                        var baseClassVariable = OutSet.GetControlVariable(FunctionResolver.staticVariables).ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.ClassName.Name.LowercaseValue)).ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.Name.Value));
                        if (currentClass.Fields.Values.Where(a => (a.IsStatic == true && a.ClassName == name && a.Name == field.Name)).Count() == 0)
                        {
                            var currentClassVariable = OutSet.GetControlVariable(FunctionResolver.staticVariables).ReadIndex(OutSet.Snapshot, new MemberIdentifier(name.Name.LowercaseValue)).ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.Name.Value));
                            currentClassVariable.SetAliases(OutSet.Snapshot, baseClassVariable);
                        }
                    }

                    foreach (var field in currentClass.Fields.Values.Where(a => (a.ClassName == currentClass.QualifiedName && a.IsStatic == true)))
                    {
                        var fieldIndex = staticVariables.ReadIndex(OutSet.Snapshot, new MemberIdentifier(field.Name.Value));
                        fieldIndex.WriteMemory(OutSet.Snapshot, field.InitValue);
                    }
                }
            }
        }

        #endregion

    }
}
