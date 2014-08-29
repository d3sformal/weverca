/*
Copyright (c) 2012-2014 David Skorvaga and David Hauzar

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.Linq;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.NativeAnalyzers;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Expression evaluation is resolved here.
    /// </summary>
    public partial class ExpressionEvaluator : ExpressionEvaluatorBase
    {
        /// <summary>
        /// Convertor of values to boolean used by evaluator.
        /// </summary>
        private BooleanConverter booleanConverter;

        /// <summary>
        /// Convertor of values to strings used by evaluator.
        /// </summary>
        private StringConverter stringConverter;

        /// <summary>
        /// The partial evaluator of one unary operation.
        /// </summary>
        private UnaryOperationEvaluator unaryOperationEvaluator;

        /// <summary>
        /// The partial evaluator of one prefix or postfix increment or decrement operation.
        /// </summary>
        private IncrementDecrementEvaluator incrementDecrementEvaluator;

        /// <summary>
        /// The partial evaluator of values that can be used as index of array.
        /// </summary>
        private ArrayIndexEvaluator arrayIndexEvaluator;

        /// <summary>
        /// The partial evaluator of one binary operation.
        /// </summary>
        private BinaryOperationEvaluator binaryOperationVisitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluator" /> class.
        /// </summary>
        public ExpressionEvaluator()
        {
            booleanConverter = new BooleanConverter();
            stringConverter = new StringConverter();
            unaryOperationEvaluator = new UnaryOperationEvaluator(booleanConverter, stringConverter);
            incrementDecrementEvaluator = new IncrementDecrementEvaluator();
            arrayIndexEvaluator = new ArrayIndexEvaluator();
            binaryOperationVisitor = new BinaryOperationEvaluator(booleanConverter, stringConverter);
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
            var types = new HashSet<TypeValue>();
            var entry = objectValue.ReadMemory(OutSnapshot);
            foreach (var obj in entry.PossibleValues)
            {
                var value = obj as ObjectValue;
                if (value != null)
                {
                    var type = OutSet.ObjectType(value);
                    if (type != null)
                    {
                        types.Add(type);
                    }
                }
            }

            var fieldNames = CheckVisibility(types, field, false);
            if (field.PossibleNames.Length == 0)
            {
                return objectValue.ReadField(OutSnapshot, field);
            }
            else
            {
                if (fieldNames.Count() == 0)
                {
                    return getStaticVariableSink();
                }
                else
                {
                    if (fieldNames.Count() < field.PossibleNames.Length)
                    {
                        fatalError(false);
                    }

                    return objectValue.ReadField(OutSnapshot, new VariableIdentifier(fieldNames));
                }
            }
        }

        private IEnumerable<string> CheckVisibility(IEnumerable<TypeValue> types,
            VariableIdentifier field, bool isStatic)
        {
            var result = new HashSet<string>();
            var methodTypes = new List<TypeValue>();
            var snapshotEntry = OutSet.ReadLocalControlVariable(FunctionResolver.calledObjectTypeName);

            if (snapshotEntry.IsDefined(OutSnapshot))
            {
                var entry = snapshotEntry.ReadMemory(OutSnapshot);
                foreach (var value in entry.PossibleValues)
                {
                    var typeValue = value as TypeValue;
                    if (typeValue != null)
                    {
                        methodTypes.Add(typeValue);
                    }
                }
            }

            if (types.Count() == 0)
            {
                foreach (var f in field.PossibleNames)
                {
                    result.Add(f.Value);
                }
            }

            foreach (var type in types)
            {
                foreach (var name in field.PossibleNames)
                {
                    var visibility = type.Declaration.GetFieldVisibility(name, isStatic);
                    if (visibility.HasValue)
                    {
                        if (methodTypes.Count == 0)
                        {
                            if (visibility != Visibility.PUBLIC)
                            {
                                SetWarning("Accessing inaccessible field",
                                    AnalysisWarningCause.ACCESSING_INACCESSIBLE_FIELD);
                            }
                            else
                            {
                                result.Add(name.Value);
                            }
                        }
                        else
                        {
                            foreach (var methodType in methodTypes)
                            {
                                if (visibility == Visibility.PRIVATE)
                                {
                                    if (!methodType.Declaration.QualifiedName.Equals(
                                        type.Declaration.QualifiedName))
                                    {
                                        SetWarning("Accessing inaccessible field",
                                            AnalysisWarningCause.ACCESSING_INACCESSIBLE_FIELD);
                                    }
                                    else
                                    {
                                        result.Add(name.Value);
                                    }
                                }
                                else if (visibility == Visibility.NOT_ACCESSIBLE)
                                {
                                    SetWarning("Accessing inaccessible field",
                                        AnalysisWarningCause.ACCESSING_INACCESSIBLE_FIELD);
                                }
                                else if (visibility == Visibility.PROTECTED)
                                {
                                    var typeHierarchy = new List<QualifiedName>(type.Declaration.BaseClasses);
                                    typeHierarchy.Add(type.Declaration.QualifiedName);
                                    var methodTypeHierarchy
                                        = new List<QualifiedName>(methodType.Declaration.BaseClasses);
                                    methodTypeHierarchy.Add(methodType.Declaration.QualifiedName);
                                    var isInHierarchy = false;

                                    foreach (var className in typeHierarchy)
                                    {
                                        if (methodTypeHierarchy.Contains(className))
                                        {
                                            isInHierarchy = true;
                                            break;
                                        }
                                    }

                                    if (!isInHierarchy)
                                    {
                                        SetWarning("Accessing inaccessible field",
                                            AnalysisWarningCause.ACCESSING_INACCESSIBLE_FIELD);
                                    }
                                    else
                                    {
                                        result.Add(name.Value);
                                    }
                                }
                                else
                                {
                                    result.Add(name.Value);
                                }
                            }
                        }
                    }
                    else
                    {
                        result.Add(name.Value);
                    }
                }
            }

            return result;
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

        /// <summary>
        /// Process binary operation on given operands
        /// </summary>
        /// <param name="leftOperand">Left operand of operation</param>
        /// <param name="operation">Binary operation</param>
        /// <param name="rightOperand">Right operand of operation</param>
        /// <returns>
        /// Result of binary expression
        /// </returns>
        public override MemoryEntry BinaryEx(MemoryEntry leftOperand, Operations operation,
            MemoryEntry rightOperand)
        {
            binaryOperationVisitor.SetContext(Flow);
            var result = binaryOperationVisitor.Evaluate(leftOperand, operation, rightOperand);

            return result;
        }

        /// <summary>
        /// Process unary operation on given operand
        /// </summary>
        /// <param name="operation">Unary operation</param>
        /// <param name="operand">Operand of operation</param>
        /// <returns>
        /// Result of unary expression
        /// </returns>
        public override MemoryEntry UnaryEx(Operations operation, MemoryEntry operand)
        {
            if (operation == Operations.Print)
            {
                if (FlagsHandler.IsDirty(operand.PossibleValues, FlagType.HTMLDirty))
                {
                    var warning = new AnalysisSecurityWarning(Flow.CurrentScript.FullName,
                        Element, Flow.CurrentProgramPoint, FlagType.HTMLDirty);
                    AnalysisWarningHandler.SetWarning(OutSet, warning);
                }
            }

            unaryOperationEvaluator.SetContext(Flow);
            return unaryOperationEvaluator.Evaluate(operation, operand);
        }

        /// <summary>
        /// Process increment or decrement operation, in both prefix and postfix form
        /// </summary>
        /// <param name="operation">Increment or decrement operation</param>
        /// <param name="incrementedValue">Value that has to be incremented/decremented</param>
        /// <returns>
        /// Result of incremented or decremented
        /// </returns>
        /// <remarks>
        /// This method doesn't provide assigning into incremented LValue
        /// </remarks>
        public override MemoryEntry IncDecEx(IncDecEx operation, MemoryEntry incrementedValue)
        {
            incrementDecrementEvaluator.SetContext(Flow);
            return incrementDecrementEvaluator.Evaluate(operation.Inc, incrementedValue);
        }

        /// <summary>
        /// Process n-ary operation on given operands
        /// </summary>
        /// <param name="keyValuePairs">Collection of key/value pairs that initialize the new array</param>
        /// <returns>
        /// Result of n-ary expression
        /// </returns>
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
                        SetWarning("Possible illegal offset type in array initialization");
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
                    // There are some values that cannot be store to exact index, so any index is used
                    indexIdentifier = Weverca.AnalysisFramework.Memory.MemberIdentifier.getAnyMemberIdentifier();
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
                    if (index.DirectName == null) // unknown field
                    {
                        keys.Add(OutSnapshot.AnyStringValue);
                    }
                    else if (TypeConversion.TryIdentifyInteger(index.DirectName, out convertedInteger))
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

                Debug.Assert(values.Count > 0, "If there is an index, there must be value too");
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

        /// <summary>
        /// Process echo statement with given values
        /// </summary>
        /// <param name="echo">Echo statement</param>
        /// <param name="entries">Values to be converted to string and printed out</param>
        public override void Echo(EchoStmt echo, MemoryEntry[] entries)
        {
            // TODO: Optimalize, implementation is provided only for faultless progress and testing
            var isDirty = false;
            stringConverter.SetContext(Flow);

            foreach (var entry in entries)
            {
                isDirty |= FlagsHandler.IsDirty(entry.PossibleValues, FlagType.HTMLDirty);
                bool isAlwaysConcrete;
                stringConverter.Evaluate(entry, out isAlwaysConcrete);
            }

            if (isDirty)
            {
                var warning = new AnalysisSecurityWarning(Flow.CurrentScript.FullName,
                    Element, Flow.CurrentProgramPoint, FlagType.HTMLDirty);
                AnalysisWarningHandler.SetWarning(OutSet, warning);
            }
        }

        /// <inheritdoc />
        public override IEnumerable<Value> IssetEx(IEnumerable<ReadSnapshotEntryBase> entries)
        {
            Debug.Assert(entries.GetEnumerator().MoveNext(),
                "isset expression must have at least one parameter");

            var isAlwaysSet = true;
            var isAlwaysUnset = true;

            foreach (var snapshotEntry in entries)
            {
                if (snapshotEntry.IsDefined(OutSnapshot))
                {
                    var entry = snapshotEntry.ReadMemory(OutSnapshot);
                    Debug.Assert(entry.PossibleValues.GetEnumerator().MoveNext(),
                        "Memory entry must always have at least one value");

                    foreach (var value in entry.PossibleValues)
                    {
                        var undefinedValue = value as UndefinedValue;
                        if (undefinedValue != null)
                        {
                            if (isAlwaysSet)
                            {
                                isAlwaysSet = false;
                                if (!isAlwaysUnset)
                                {
                                    return new[] { OutSet.AnyBooleanValue };
                                }
                            }
                        }
                        else if (value is AnyValue)
                        {
                            // Value can be null ot not null too.
                            return new[] { OutSet.AnyBooleanValue };
                        }
                        else
                        {
                            if (isAlwaysUnset)
                            {
                                isAlwaysUnset = false;
                                if (!isAlwaysSet)
                                {
                                    return new[] { OutSet.AnyBooleanValue };
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (isAlwaysSet)
                    {
                        isAlwaysSet = false;
                        if (!isAlwaysUnset)
                        {
                            return new[] { OutSet.AnyBooleanValue };
                        }
                    }
                }
            }

            if (isAlwaysSet == isAlwaysUnset)
            {
                return new[] { OutSet.AnyBooleanValue };
            }
            else
            {
                return new[] { OutSet.CreateBool(isAlwaysSet) };
            }
        }

        /// <inheritdoc />
        public override MemoryEntry EmptyEx(ReadWriteSnapshotEntryBase snapshotEntry)
        {
            if (snapshotEntry.IsDefined(OutSnapshot))
            {
                var entry = snapshotEntry.ReadMemory(OutSnapshot);
                Debug.Assert(entry.PossibleValues.GetEnumerator().MoveNext(),
                    "Memory entry must always have at least one value");

                booleanConverter.SetContext(OutSnapshot);

                var isAlwaysEmpty = true;
                var isNeverEmpty = true;

                foreach (var value in entry.PossibleValues)
                {
                    var booleanValue = booleanConverter.EvaluateToBoolean(value);
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

        /// <summary>
        /// Terminates execution of the PHP code. It gives order to analysis to jump at the end of program.
        /// </summary>
        /// <param name="exit"><c>exit</c> expression</param>
        /// <param name="status">Exit status printed if it is string or returned if it is integer</param>
        /// <returns>
        /// Anything, return value is ignored
        /// </returns>
        public override MemoryEntry Exit(ExitEx exit, MemoryEntry status)
        {
            fatalError(true);
            // Exit expression never returns, but it is still expression so it must return something
            return new MemoryEntry(OutSet.AnyValue);
        }

        /// <summary>
        /// Get value representation of given constant
        /// </summary>
        /// <param name="x">Constant representation</param>
        /// <returns>
        /// Represented value
        /// </returns>
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
                        SetWarning("Use of undefined constant", AnalysisWarningCause.UNDEFINED_VALUE);
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

        /// <summary>
        /// Is called on <c>const x = 5</c> declarations
        /// </summary>
        /// <param name="x">Constant declaration</param>
        /// <param name="constantValue">Value assigned into constant</param>
        public override void ConstantDeclaration(ConstantDecl x, MemoryEntry constantValue)
        {
            var name = new QualifiedName(new Name(x.Name.Value));
            UserDefinedConstantHandler.insertConstant(OutSet, name, constantValue, false);
        }

        /// <summary>
        /// Create object value of given type
        /// </summary>
        /// <param name="typeName">Object type specifier</param>
        /// <returns>
        /// Memory entry with all new possible objects
        /// </returns>
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

        /// <summary>
        /// Determine whether an expression is instance of a class or interface
        /// </summary>
        /// <param name="expression">Expression to be determined whether it is instance of a class</param>
        /// <param name="typeName">Name of the type that the expression is checked to</param>
        /// <returns>
        ///   <c>true</c> whether all values are objects inherited from the type, <c>false</c> whether
        /// values are not objects or not instances of the type and otherwise any boolean value.
        /// </returns>
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

        #region Type Conversions overrides
        public override bool TryIdentifyInteger(string value, out int convertedValue) 
        {
            return TypeConversion.TryIdentifyInteger (value, out convertedValue);
        }
        #endregion Type Conversions overrides

        #endregion ExpressionEvaluatorBase overrides

        /// <summary>
        /// Generates a warning with the given message.
        /// </summary>
        /// <param name="message">Text of warning.</param>
        public void SetWarning(string message)
        {
            AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(Flow.CurrentScript.FullName,
                message, Element, Flow.CurrentProgramPoint));
        }

        /// <summary>
        /// Generates a warning of the proper type and with the given message.
        /// </summary>
        /// <param name="message">Text of warning.</param>
        /// <param name="cause">More specific warning type.</param>
        public void SetWarning(string message, AnalysisWarningCause cause)
        {
            AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(Flow.CurrentScript.FullName,
                message, Element, Flow.CurrentProgramPoint, cause));
        }

        /// <summary>
        /// Generates a warning of the proper type in the given element and with the given message.
        /// </summary>
        /// <param name="message">Text of warning.</param>
        /// <param name="element">Element of language where the warning is reported.</param>
        /// <param name="cause">More specific warning type.</param>
        public void SetWarning(string message, LangElement element, AnalysisWarningCause cause)
        {
            AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(Flow.CurrentScript.FullName,
                message, element, Flow.CurrentProgramPoint, cause));
        }

        private void fatalError(bool removeFlowChildren)
        {
            fatalError(Flow, removeFlowChildren);
        }

        private void fatalError(FlowController flow, bool removeFlowChildren)
        {
            var catchedType = new GenericQualifiedName(new QualifiedName(new Name(string.Empty)));
            var catchVariable = new VariableIdentifier(string.Empty);
            var description = new CatchBlockDescription(flow.ProgramEnd, catchedType, catchVariable);
            var info = new ThrowInfo(description, new MemoryEntry());

            var throws = new ThrowInfo[] { info };
            flow.SetThrowBranching(throws, removeFlowChildren);
        }

        /// <summary>
        /// Find all arrays in list of universal values.
        /// </summary>
        /// <param name="entry">Memory entry of values to resolve to arrays.</param>
        /// <param name="isAlwaysArray">Indicates whether all values are array.</param>
        /// <param name="isAlwaysConcrete">Indicates whether all values are concrete array.</param>
        /// <returns>All resolved concrete arrays.</returns>
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
        /// Resolve source or native type from name.
        /// </summary>
        /// <param name="typeName">Name of type to resolve.</param>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="element">AST nodes of Language element which is currently evaluated.</param>
        /// <returns><c>null</c> whether type cannot be resolver, otherwise the type value.</returns>
        public static IEnumerable<TypeValue> ResolveSourceOrNativeType(QualifiedName typeName,
            FlowController flow, LangElement element)
        {
            var outSet = flow.OutSet;
            var typeValues = outSet.ResolveType(typeName);
            if (!typeValues.GetEnumerator().MoveNext())
            {
                var objectAnalyzer = NativeObjectAnalyzer.GetInstance(outSet);
                ClassDecl nativeDeclaration;
                if (objectAnalyzer.TryGetClass(typeName, out nativeDeclaration))
                {
                    foreach (var baseClass in nativeDeclaration.BaseClasses)
                    {
                        if (!outSet.ResolveType(nativeDeclaration.QualifiedName).GetEnumerator().MoveNext())
                        {
                            outSet.DeclareGlobal(outSet.CreateType(nativeDeclaration));
                        }
                    }

                    var type = outSet.CreateType(nativeDeclaration);
                    outSet.DeclareGlobal(type);
                    var newTypes = new List<TypeValue>();
                    newTypes.Add(type);
                    typeValues = newTypes;
                }
                else
                {
                    // TODO: This must be error
                    var warning = new AnalysisWarning(flow.CurrentScript.FullName,
                        "Class does not exist", element, flow.CurrentProgramPoint, AnalysisWarningCause.CLASS_DOESNT_EXIST);
                    AnalysisWarningHandler.SetWarning(outSet, warning);
                    return null;
                }
            }

            return typeValues;
        }

        /// <summary>
        /// Resolve names from values.
        /// </summary>
        /// <param name="possibleNames">Memory entry of values to resolve to type names.</param>
        /// <returns><c>null</c> whether any value cannot be resolved to string, otherwise names.</returns>
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
        /// Creates new objects of specified type names, directly and indirectly too.
        /// </summary>
        /// <param name="typeNames">Possible names of classes or interfaces.</param>
        /// <returns>Memory entry with all new possible objects.</returns>
        private MemoryEntry CreateObjectFromNames(IEnumerable<QualifiedName> typeNames)
        {
            var values = new List<ObjectValue>();
            var isTypeAlwaysDefined = true;

            foreach (var typeName in typeNames)
            {
                var typeValues = ResolveSourceOrNativeType(typeName, Flow, Element);
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
        /// <param name="expression">Expression to be determined whether it is instance of a class.</param>
        /// <param name="typeNames">Possible names of classes or interfaces.</param>
        /// <returns>
        /// <c>true</c> whether expression is instance of all the specified types,
        /// <c>false</c> whether it is not instance of any of types and otherwise any boolean value.
        /// </returns>
        private MemoryEntry InstanceOfTypeNames(MemoryEntry expression, IEnumerable<QualifiedName> typeNames)
        {
            var isAlwaysInstanceOf = true;
            var isNeverInstanceOf = true;

            foreach (var typeName in typeNames)
            {
                var typeValues = ResolveSourceOrNativeType(typeName, Flow, Element);
                if (typeValues == null)
                {
                    // TODO: This must be error
                    SetWarning("Class not found when creating new object");
                    continue;
                }

                var typeValue = typeValues.First();

                foreach (var value in expression.PossibleValues)
                {
                    var objectValue = value as ObjectValue;
                    if (objectValue == null)
                    {
                        if (isAlwaysInstanceOf)
                        {
                            isAlwaysInstanceOf = false;
                            if (!isNeverInstanceOf)
                            {
                                return new MemoryEntry(OutSet.AnyBooleanValue);
                            }
                        }

                        continue;
                    }

                    var objectType = OutSet.ObjectType(objectValue);
                    var booleanValue = (objectType == typeValue);
                    if (booleanValue)
                    {
                        if (isNeverInstanceOf)
                        {
                            isNeverInstanceOf = false;
                            if (!isAlwaysInstanceOf)
                            {
                                return new MemoryEntry(OutSet.AnyBooleanValue);
                            }
                        }

                        continue;
                    }

                    foreach (var baseClass in objectType.Declaration.BaseClasses)
                    {
                        if (baseClass == typeName)
                        {
                            booleanValue = true;
                        }
                    }

                    if (booleanValue)
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
                if (typeDeclaration.Declaration.IsAbstract)
                {
                    var message = "Cannot instantiate abstract class "
                        + typeDeclaration.Declaration.QualifiedName.Name.Value;
                    SetWarning(message, AnalysisWarningCause.CANNOT_INSTANCIATE_ABSTRACT_CLASS);
                    fatalError(true);
                }
                else if (typeDeclaration.Declaration.IsInterface)
                {
                    var message = "Cannot instantiate interface "
                        + typeDeclaration.Declaration.QualifiedName.Name.Value;
                    SetWarning(message, AnalysisWarningCause.CANNOT_INSTANCIATE_INTERFACE);
                    fatalError(true);
                }
                else
                {
                    var initializer = new ObjectInitializer(this);
                    initializer.InitializeObject(newObject, typeDeclaration.Declaration);
                }
            }

            return newObject;
        }

        /// <summary>
        /// Get value representation of given class constant
        /// </summary>
        /// <param name="thisObject">Object</param>
        /// <param name="variableName">Constant name</param>
        /// <returns>
        /// Value of class constant
        /// </returns>
        public override MemoryEntry ClassConstant(MemoryEntry thisObject, VariableName variableName)
        {
            var result = new List<Value>();
            int numberOfWarnings = 0;
            foreach (var value in thisObject.PossibleValues)
            {
                var visitor = new StaticObjectVisitor(Flow);
                value.Accept(visitor);
                switch (visitor.Result)
                {
                    case StaticObjectVisitorResult.NO_RESULT:
                        SetWarning("Cannot access constant on non object",
                            AnalysisWarningCause.CANNOT_ACCESS_CONSTANT_ON_NON_OBJECT);
                        numberOfWarnings++;
                        result.Add(OutSet.UndefinedValue);
                        break;
                    case StaticObjectVisitorResult.ONE_RESULT:
                        bool success = true;
                        var res = classConstant(visitor.className, variableName, out success);
                        if (success == false)
                        {
                            numberOfWarnings++;
                        }

                        result.AddRange(res.PossibleValues);

                        break;
                    case StaticObjectVisitorResult.MULTIPLE_RESULTS:
                        result.Add(OutSet.AnyValue);
                        break;
                }
            }

            if (numberOfWarnings > 0)
            {
                if (numberOfWarnings >= thisObject.Count)
                {
                    fatalError(true);
                }
                else
                {
                    fatalError(false);
                }
            }

            return new MemoryEntry(result);
        }

        /// <summary>
        /// Get value representation of given class constant
        /// </summary>
        /// <param name="qualifiedName">Class Name</param>
        /// <param name="variableName">Constant name</param>
        /// <returns>
        /// Value of class constant
        /// </returns>
        public override MemoryEntry ClassConstant(QualifiedName qualifiedName, VariableName variableName)
        {
            var success = true;
            var result = classConstant(qualifiedName, variableName, out success);
            if (success == false)
            {
                fatalError(true);
            }

            return result;
        }

        private MemoryEntry classConstant(QualifiedName qualifiedName, VariableName variableName,
            out bool success)
        {
            var typeNames = ResolveSpecialClassKeywords(qualifiedName);

            var enumerator = typeNames.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                success = false;
                return new MemoryEntry(OutSet.UndefinedValue);
            }

            qualifiedName = enumerator.Current;

            if (enumerator.MoveNext())
            {
                var names = new HashSet<StringValue>();
                foreach (var typeName in typeNames)
                {
                    names.Add(OutSet.CreateString(typeName.Name.Value));
                }

                success = true;
                var memoryEntry = new MemoryEntry(names);
                return ClassConstant(memoryEntry, variableName);
            }

            var analyzer = NativeObjectAnalyzer.GetInstance(OutSet);
            if (analyzer.ExistClass(qualifiedName))
            {
                var constants = analyzer.GetClass(qualifiedName).Constants;
                ConstantInfo value;
                if (constants.TryGetValue(new FieldIdentifier(qualifiedName, variableName), out value))
                {
                    success = true;
                    return value.Value;
                }
                else
                {
                    var message = "Constant " + qualifiedName.Name + "::" + variableName + " doesn't exist";
                    SetWarning(message, AnalysisWarningCause.CLASS_CONSTANT_DOESNT_EXIST);
                    success = false;
                    return new MemoryEntry(OutSet.UndefinedValue);
                }
            }
            else
            {
                var name = ".class(" + qualifiedName.Name.LowercaseValue + ")->constant("
                    + variableName.Value + ")";
                var constant = OutSet.GetControlVariable(new VariableName(name));
                if (constant.IsDefined(OutSet.Snapshot))
                {
                    success = true;
                    return constant.ReadMemory(OutSet.Snapshot);
                }
                else
                {
                    var message = "Constant " + qualifiedName.Name + "::" + variableName + " does not exist";
                    SetWarning(message, AnalysisWarningCause.CLASS_CONSTANT_DOESNT_EXIST);
                    success = false;
                    return new MemoryEntry(OutSet.UndefinedValue);
                }
            }
        }

        private IEnumerable<QualifiedName> ResolveSpecialClassKeywords(QualifiedName qualifiedName)
        {
            var result = new List<QualifiedName>();
            var typeName = qualifiedName.Name.Value;

            if (string.Equals(typeName, "self", StringComparison.Ordinal)
                || string.Equals(typeName, "parent", StringComparison.Ordinal))
            {
                var snapshotEntry = OutSet.ReadLocalControlVariable(FunctionResolver.calledObjectTypeName);

                if (snapshotEntry.IsDefined(OutSet.Snapshot))
                {
                    var calledObjectTypes = snapshotEntry.ReadMemory(OutSet.Snapshot);

                    if (string.Equals(typeName, "self", StringComparison.Ordinal))
                    {
                        foreach (var calledObjectType in calledObjectTypes.PossibleValues)
                        {
                            var classValue = calledObjectType as TypeValue;
                            Debug.Assert(classValue != null, "The control variable is always type value");
                            result.Add(classValue.QualifiedName);
                        }
                    }
                    else
                    {
                        Debug.Assert(string.Equals(typeName, "parent", StringComparison.Ordinal),
                            "There must be \"parent\" value");

                        foreach (var calledObjectType in calledObjectTypes.PossibleValues)
                        {
                            var classValue = calledObjectType as TypeValue;
                            Debug.Assert(classValue != null, "The control variable is always type value");
                            var baseClassDeclarations = classValue.Declaration.BaseClasses;

                            if (baseClassDeclarations.Count > 0)
                            {
                                result.Add(baseClassDeclarations.Last());
                            }
                            else
                            {
                                SetWarning("Cannot access to parent class, class has no parent",
                                    AnalysisWarningCause.CANNOT_ACCCES_PARENT_CURRENT_CLASS_HAS_NO_PARENT);
                            }
                        }
                    }
                }
                else
                {
                    if (string.Equals(typeName, "self", StringComparison.Ordinal))
                    {
                        SetWarning("Cannot access to itself when it is not in class",
                            AnalysisWarningCause.CANNOT_ACCCES_SELF_WHEN_NOT_IN_CLASS);
                    }
                    else
                    {
                        SetWarning("Cannot access to parent when it is not in class",
                            AnalysisWarningCause.CANNOT_ACCCES_PARENT_WHEN_NOT_IN_CLASS);
                    }
                }
            }
            else
            {
                result.Add(qualifiedName);
            }

            return result;
        }

        /// <summary>
        /// Resolve value, determined by given static field specifier on given type
        /// </summary>
        /// <param name="type">Type which static field is resolved</param>
        /// <param name="field">Specifier of resolved field</param>
        /// <returns>
        /// Snapshot entry for static field
        /// </returns>
        public override ReadWriteSnapshotEntryBase ResolveStaticField(GenericQualifiedName type,
            VariableIdentifier field)
        {
            var analyzer = NativeObjectAnalyzer.GetInstance(OutSet);
            var resolvedTypes = FunctionResolver.ResolveType(type.QualifiedName, Flow, OutSet, Element);

            if (resolvedTypes.Count() > 0)
            {
                var resolvedType = resolvedTypes.First();

                if (!analyzer.ExistClass(resolvedType))
                {
                    var snapshotEntry = OutSet.ReadControlVariable(FunctionResolver.staticVariables);
                    var identifier = new MemberIdentifier(resolvedType.Name.LowercaseValue);
                    var classStorage = snapshotEntry.ReadIndex(OutSet.Snapshot, identifier);

                    if (!classStorage.IsDefined(OutSet.Snapshot))
                    {
                        var message = "Class " + resolvedType.Name.Value + " doesn't exist";
                        SetWarning(message, AnalysisWarningCause.CLASS_DOESNT_EXIST);
                        return getStaticVariableSink();
                    }
                }

                var names = new List<QualifiedName>();
                names.Add(resolvedType);
                return resolveStaticVariable(names, field, true);
            }
            else
            {
                return getStaticVariableSink();
            }
        }

        /// <inheritdoc />
        public override ReadWriteSnapshotEntryBase ResolveIndirectStaticField(
            MemoryEntry possibleTypes, VariableIdentifier field)
        {
            var analyzer = NativeObjectAnalyzer.GetInstance(OutSet);
            var classes = new List<QualifiedName>();
            bool isAllwasConcrete = true;
            foreach (var name in typeNames(possibleTypes, out isAllwasConcrete))
            {
                if (!analyzer.ExistClass(name.QualifiedName))
                {
                    var identifier = new MemberIdentifier(name.QualifiedName.Name.LowercaseValue);
                    var snapshotEntry = OutSet.ReadControlVariable(FunctionResolver.staticVariables);
                    var classStorage = snapshotEntry.ReadIndex(OutSet.Snapshot, identifier);

                    if (!classStorage.IsDefined(OutSet.Snapshot))
                    {
                        var message = "Class " + name.QualifiedName.Name.Value + " doesn't exist";
                        SetWarning(message, AnalysisWarningCause.CLASS_DOESNT_EXIST);
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

            return resolveStaticVariable(classes, field, isAllwasConcrete);
        }

        private ReadWriteSnapshotEntryBase resolveStaticVariable(List<QualifiedName> classes,
            VariableIdentifier field, bool isAllwasConcreteClass)
        {
            var names = new HashSet<string>();
            var fieldNames = new HashSet<string>();
            var analyzer = NativeObjectAnalyzer.GetInstance(OutSet);

            foreach (var typeName in classes)
            {
                foreach (var fieldName in field.PossibleNames)
                {
                    var snapshotEntry = OutSet.ReadControlVariable(FunctionResolver.staticVariables);
                    var identifier = new MemberIdentifier(typeName.Name.LowercaseValue);
                    var classStorage = snapshotEntry.ReadIndex(OutSet.Snapshot, identifier);

                    if (!classStorage.IsDefined(OutSet.Snapshot))
                    {
                        if (analyzer.ExistClass(typeName) && (!analyzer.GetClass(typeName).IsInterface))
                        {
                            InsertNativeObjectStaticVariablesIntoMM(typeName);

                            var controlVariable
                                = OutSet.ReadControlVariable(FunctionResolver.staticVariables);
                            classStorage = controlVariable.ReadIndex(OutSet.Snapshot, identifier);

                            var fieldIdentifier = new MemberIdentifier(fieldName.Value);
                            var indexSnapshot = classStorage.ReadIndex(OutSet.Snapshot, fieldIdentifier);

                            if (indexSnapshot.IsDefined(OutSet.Snapshot))
                            {
                                names.Add(typeName.Name.LowercaseValue);
                                fieldNames.Add(fieldName.Value);
                            }
                            else
                            {
                                var message = "Static variable " + typeName.Name.Value + "::"
                                    + field.DirectName.Value + " wasn't declared";
                                SetWarning(message, AnalysisWarningCause.STATIC_VARIABLE_DOESNT_EXIST);
                            }
                        }
                        else
                        {
                            var message = "Class " + typeName.Name.Value + " doesn't exist";
                            SetWarning(message, AnalysisWarningCause.CLASS_DOESNT_EXIST);
                        }
                    }
                    else
                    {
                        var fieldIdentifier = new MemberIdentifier(fieldName.Value);
                        var indexSnapshot = classStorage.ReadIndex(OutSet.Snapshot, fieldIdentifier);

                        if (indexSnapshot.IsDefined(OutSet.Snapshot))
                        {
                            names.Add(typeName.Name.LowercaseValue);
                            fieldNames.Add(fieldName.Value);
                        }
                        else
                        {
                            var message = "Static variable " + typeName.Name.Value + "::"
                                + field.DirectName.Value + " wasn't declared";
                            SetWarning(message, AnalysisWarningCause.STATIC_VARIABLE_DOESNT_EXIST);
                        }
                    }
                }
            }

            if (field.PossibleNames.Length > 0 && fieldNames.Count == 0)
            {
                if (isAllwasConcreteClass == true)
                {
                    return getStaticVariableSink();
                }
                else
                {
                    var snapshotEntry = OutSet.GetControlVariable(FunctionResolver.staticVariableSink);
                    snapshotEntry.WriteMemory(OutSet.Snapshot, new MemoryEntry(OutSet.AnyValue));
                    return OutSet.GetControlVariable(FunctionResolver.staticVariableSink);
                }
            }
            else
            {
                var types = new HashSet<TypeValue>();
                foreach (var className in names)
                {
                    var qualifiedName = new QualifiedName(new Name(className));
                    if (analyzer.ExistClass(qualifiedName))
                    {
                        types.Add(OutSet.CreateType(analyzer.GetClass(qualifiedName)));
                    }
                    else
                    {
                        foreach (var type in OutSet.ResolveType(qualifiedName))
                        {
                            types.Add(type);
                        }
                    }
                }

                if (field.PossibleNames.Length == 0)
                {
                    var snapshotEntry = OutSet.ReadControlVariable(FunctionResolver.staticVariables);
                    var storage = snapshotEntry.ReadIndex(OutSet.Snapshot, new MemberIdentifier(names));
                    return storage.ReadIndex(OutSet.Snapshot, new MemberIdentifier(fieldNames));
                }
                else
                {
                    var newNames = CheckVisibility(types, new VariableIdentifier(fieldNames), true);
                    if (newNames.Count() == 0)
                    {
                        if (isAllwasConcreteClass == true)
                        {
                            return getStaticVariableSink();
                        }
                        else
                        {
                            var entry = OutSet.GetControlVariable(FunctionResolver.staticVariableSink);
                            entry.WriteMemory(OutSet.Snapshot, new MemoryEntry(OutSet.AnyValue));
                            return OutSet.GetControlVariable(FunctionResolver.staticVariableSink);
                        }
                    }

                    if (newNames.Count() < fieldNames.Count)
                    {
                        fatalError(false);
                    }

                    var snapshotEntry = OutSet.ReadControlVariable(FunctionResolver.staticVariables);
                    var storage = snapshotEntry.ReadIndex(OutSet.Snapshot, new MemberIdentifier(names));
                    return storage.ReadIndex(OutSet.Snapshot, new MemberIdentifier(newNames));
                }
            }
        }

        private IEnumerable<GenericQualifiedName> typeNames(MemoryEntry typeValue, out bool isAllwasConcrete)
        {
            isAllwasConcrete = true;
            var result = new List<GenericQualifiedName>();
            foreach (var value in typeValue.PossibleValues)
            {
                var visitor = new StaticObjectVisitor(Flow);
                value.Accept(visitor);
                switch (visitor.Result)
                {
                    case StaticObjectVisitorResult.NO_RESULT:
                        SetWarning("Cannot access static variable on non-object",
                            AnalysisWarningCause.CANNOT_ACCES_STATIC_VARIABLE_ON_NON_OBJECT);
                        break;
                    case StaticObjectVisitorResult.ONE_RESULT:
                        result.Add(new GenericQualifiedName(visitor.className));
                        break;
                    case StaticObjectVisitorResult.MULTIPLE_RESULTS:
                        isAllwasConcrete = false;
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Resolve value, determined by given static field specifier on given type
        /// </summary>
        /// <param name="type">Type which static field is resolved</param>
        /// <param name="field">Possible fields</param>
        /// <returns>
        /// Snapshot entry for static field
        /// </returns>
        public override ReadWriteSnapshotEntryBase ResolveStaticField(GenericQualifiedName type,
            MemoryEntry field)
        {
            return ResolveStaticField(type, new VariableIdentifier(this.VariableNames(field)));
        }

        /// <inheritdoc />
        public override ReadWriteSnapshotEntryBase ResolveIndirectStaticField(
            MemoryEntry possibleTypes, MemoryEntry field)
        {
            var identifier = new VariableIdentifier(this.VariableNames(field));
            return ResolveIndirectStaticField(possibleTypes, identifier);
        }

        /// <summary>
        /// Returns static variable sink, when static variable does not exist,
        /// this empty space in memory model is returned.
        /// </summary>
        /// <returns>Empty space in memory model.</returns>
        private ReadWriteSnapshotEntryBase getStaticVariableSink()
        {
            fatalError(true);
            var snapshotEntry = OutSet.GetControlVariable(FunctionResolver.staticVariableSink);
            snapshotEntry.WriteMemory(OutSet.Snapshot, new MemoryEntry(OutSet.UndefinedValue));
            return OutSet.GetControlVariable(FunctionResolver.staticVariableSink);
        }
    }
}