/*
Copyright (c) 2012-2014 Pavel Bastecky.

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
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.Analysis.ExpressionEvaluator;
using Weverca.Analysis;
using Weverca.MemoryModels.UnitTest.SnapshotTestFramework;

namespace Weverca.MemoryModels.UnitTest.SnapshotTestFramework
{
    /// <inheritdoc />
    internal class MemoryAssistant : MemoryAssistantBase
    {
        #region MemoryAssistantBase overrides

        /// <inheritdoc />
        public override MemoryEntry ReadAnyValueIndex(AnyValue value, MemberIdentifier index)
        {
            // TODO: Copy info
            if (value is AnyStringValue)
            {
                // Element of string is one charachter but since PHP has no character type,
                // it returns string with one character. The character do not need to be initialized
                SetWarning("Possibly uninitialized string offset");
                return new MemoryEntry(Context.AnyStringValue);
            }
            else if ((value is AnyNumericValue) || (value is AnyBooleanValue) || (value is AnyResourceValue))
            {
                SetWarning("Trying to get element of scalar value",
                    AnalysisWarningCause.ELEMENT_OF_NON_ARRAY_VARIABLE);
                return new MemoryEntry(Context.UndefinedValue);
            }
            else if (value is AnyObjectValue)
            {
                // TODO: This must be error
                SetWarning("Cannot use object as array");
                return new MemoryEntry(Context.AnyValue);
            }
            else
            {
                // This is case of AnyArrayValue, AnyValue and possibly others.
                // If value is AnyValue, it can be any object too, so it can cause an error.
                Value newValue = Context.AnyValue;
                newValue = FlagsHandler.CopyFlags(value, newValue);
                return new MemoryEntry(newValue);
            }
        }

        /// <inheritdoc />
        public override MemoryEntry ReadAnyField(AnyValue value, VariableIdentifier field)
        {
            // TODO: Copy info
            if (value is AnyObjectValue)
            {
                SetWarning("Possibly undefined property");
                return new MemoryEntry(Context.AnyValue);
            }
            else if ((value is AnyScalarValue) || (value is AnyArrayValue) || (value is AnyResourceValue))
            {
                SetWarning("Trying to get property of non-object",
                    AnalysisWarningCause.PROPERTY_OF_NON_OBJECT_VARIABLE);
                return new MemoryEntry(Context.UndefinedValue);
            }
            else
            {
                // This is case of AnyValue and possibly others.
                return new MemoryEntry(Context.AnyValue);
            }
        }

        /// <inheritdoc />
        public override MemoryEntry Widen(MemoryEntry old, MemoryEntry current)
        {
            if (current == null)
            {
                current = new MemoryEntry();
            }
            if (old == null)
            {
                old = new MemoryEntry();
            }

            //todo copy info
            var visitor = new WidenningVisitor();

            //todo maybe make more precise
            List<Value> allValues = new List<Value>(old.PossibleValues);
            allValues.AddRange(current.PossibleValues);
            return visitor.Widen(allValues, Context);
        }

        #endregion

        /// <summary>
        /// Generates a warning with the given message
        /// </summary>
        /// <param name="message">Text of warning</param>
        public void SetWarning(string message)
        {
            AnalysisWarningHandler.SetWarning(Context, new AnalysisWarning(Point.OwningPPGraph.OwningScript.FullName, message, Point.Partial, Point));
        }

        /// <summary>
        /// Generates a warning of the proper type and with the given message
        /// </summary>
        /// <param name="message">Text of warning</param>
        /// <param name="cause">More specific warning type</param>
        public void SetWarning(string message, AnalysisWarningCause cause)
        {
            AnalysisWarningHandler.SetWarning(Context, new AnalysisWarning(Point.OwningPPGraph.OwningScript.FullName, message, Point.Partial, Point, cause));
        }


        /// <inheritdoc />
        public override IEnumerable<FunctionValue> ResolveMethods(Value thisObject, TypeValue type, PHP.Core.QualifiedName methodName, IEnumerable<FunctionValue> objectMethods)
        {

            foreach (var method in objectMethods)
            {
                //bool isstatic = (type.Declaration.ModeledMethods.Where(a => a.Key.Name == methodName.Name && a.Value.IsStatic == true).Count() > 0 || type.Declaration.SourceCodeMethods.Where(a => a.Key.Name == methodName.Name && a.Value.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Static) == true).Count() > 0);
                if (method.Name.Value == methodName.Name.Value)// && isstatic == false) 
                {
                    if (CheckVisibility(type, method))
                    {
                        yield return method;
                    }
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<FunctionValue> ResolveMethods(TypeValue value, PHP.Core.QualifiedName methodName, IEnumerable<FunctionValue> objectMethods)
        {
            if (value != null)
            {

                foreach (var method in objectMethods)
                {
                    //bool isstatic = (value.Declaration.ModeledMethods.Where(a => a.Key.Name == methodName.Name && a.Value.IsStatic == true).Count() > 0 || value.Declaration.SourceCodeMethods.Where(a => a.Key.Name == methodName.Name && a.Value.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Static) == true).Count() > 0);
                    if (method.Name.Value == methodName.Name.Value)// && isstatic == true)
                    {
                        if (CheckVisibility(value, method))
                        {
                            yield return method;
                        }
                    }
                }
            }
            foreach (var method in objectMethods)
            {
                if (method.Name.Value == methodName.Name.Value)
                {
                    yield return method;
                }
            }
        }

        private bool CheckVisibility(TypeValue type, FunctionValue method)
        {
            List<TypeValue> methodTypes = new List<TypeValue>();
            if (Context.ReadLocalControlVariable(FunctionResolver.calledObjectTypeName).IsDefined(Context))
            {
                foreach (var value in Context.ReadLocalControlVariable(FunctionResolver.calledObjectTypeName).ReadMemory(Context).PossibleValues)
                {
                    if (value is TypeValue)
                    {
                        methodTypes.Add(value as TypeValue);
                    }
                }
            }
            Name name = method.Name;
            var visibility = type.Declaration.GetMethodVisibility(name);
            if (visibility.HasValue)
            {
                if (methodTypes.Count() == 0)
                {
                    if (visibility != Visibility.PUBLIC)
                    {
                        SetWarning("Calling inaccessible method", AnalysisWarningCause.CALLING_INACCESSIBLE_METHOD);
                        return false;
                    }

                }
                else
                {
                    int numberOfWarings = 0;
                    foreach (var methodType in methodTypes)
                    {
                        if (visibility == Visibility.PRIVATE)
                        {
                            if (!methodType.Declaration.QualifiedName.Equals(type.Declaration.QualifiedName))
                            {
                                SetWarning("Calling inaccessible method", AnalysisWarningCause.CALLING_INACCESSIBLE_METHOD);
                                numberOfWarings++;
                            }
                        }
                        else if (visibility == Visibility.NOT_ACCESSIBLE)
                        {
                            SetWarning("Calling inaccessible method", AnalysisWarningCause.CALLING_INACCESSIBLE_METHOD);
                            numberOfWarings++;
                        }
                        else if (visibility == Visibility.PROTECTED)
                        {
                            List<QualifiedName> typeHierarchy = new List<QualifiedName>(type.Declaration.BaseClasses);
                            typeHierarchy.Add(type.Declaration.QualifiedName);
                            List<QualifiedName> methodTypeHierarchy = new List<QualifiedName>(methodType.Declaration.BaseClasses);
                            methodTypeHierarchy.Add(methodType.Declaration.QualifiedName);
                            bool isInHierarchy = false;
                            foreach (var className in typeHierarchy)
                            {
                                if (methodTypeHierarchy.Contains(className))
                                {
                                    isInHierarchy = true;
                                    break;
                                }
                            }
                            if (isInHierarchy == false)
                            {
                                SetWarning("Calling inaccessible method", AnalysisWarningCause.CALLING_INACCESSIBLE_METHOD);
                                numberOfWarings++;
                            }
                        }
                    }
                    if (numberOfWarings == methodTypes.Count)
                    {
                        return false;
                    }

                }
            }
            return true;
        }

        /// <inheritdoc />
        public override ObjectValue CreateImplicitObject()
        {
            ClassDeclBuilder builder=new ClassDeclBuilder();
            builder.QualifiedName=new QualifiedName(new Name("stdClass"));
            return Context.CreateObject(Context.CreateType(builder.Build()));
        }

        /// <inheritdoc />
        public override void TriedIterateFields(Value value)
        {
            SetWarning("Field iteration has wrong argument type");
            return;
        }

        /// <inheritdoc />
        public override void TriedIterateIndexes(Value value)
        {
            SetWarning("Index iteration has wrong argument type");
        }

        private IEnumerable<int> ResolveStringIndex(StringValue value, MemberIdentifier Index)
        {
            var NumberIndices = new HashSet<int>();
            foreach (var i in Index.PossibleNames)
            {
                int p;
                if (int.TryParse(i, out p))
                {
                    NumberIndices.Add(p);
                }
                else
                {
                    NumberIndices.Add(0);
                }
            }

            if (NumberIndices.Count == 0)
            {
                for (int i = 0; i < value.Value.Count(); i++)
                {
                    NumberIndices.Add(i);
                }
            }
            return NumberIndices;
        }


        /// <inheritdoc />
        public override IEnumerable<Value> ReadStringIndex(StringValue value, MemberIdentifier index)
        {
            HashSet<Value> result = new HashSet<Value>();
            foreach (var number in ResolveStringIndex(value, index))
            {
                if (number < 0)
                {
                    result.Add(value);
                    SetWarning("Cannot index string with negative numbers", AnalysisWarningCause.INDEX_OUT_OF_RANGE);
                }
                else if (number < value.Value.Count())
                {
                    result.Add(Context.CreateString(value.Value[number].ToString()));
                }
                else
                {
                    result.Add(Context.CreateString(""));
                }
            }
            return result;
        }

        /// <inheritdoc />
        public override IEnumerable<Value> WriteStringIndex(StringValue indexed, MemberIdentifier index, MemoryEntry writtenValue)
        {
            HashSet<Value> result = new HashSet<Value>();
            SimpleStringConverter converter = new SimpleStringConverter(Context);
            bool isConcrete = false;
            foreach (var value in converter.Evaluate(writtenValue.PossibleValues, out isConcrete))
            {
                string WrittenChar = value.Value;

                foreach (var number in ResolveStringIndex(indexed, index))
                {
                    Value newValue;
                    if (number < 0)
                    {
                        newValue = indexed;
                        SetWarning("Cannot index string with negative numbers", AnalysisWarningCause.INDEX_OUT_OF_RANGE);
                    }
                    else if (number < indexed.Value.Count())
                    {
                        StringBuilder newString = new StringBuilder();
                        newString.Append(indexed.Value);
                        newString[number] = WrittenChar[0];
                        newValue = FlagsHandler.CopyFlags(indexed, Context.CreateString(newString.ToString()));
                    }
                    else if (number == indexed.Value.Count())
                    {
                        newValue = FlagsHandler.CopyFlags(indexed, Context.CreateString(indexed.Value + "" + WrittenChar[0].ToString()));
                    }
                    else
                    {
                        newValue = FlagsHandler.CopyFlags(indexed, Context.CreateString(indexed.Value + " " + WrittenChar[0].ToString()));
                    }
                    result.Add(newValue);
                }
            }
            if (!isConcrete)
            {
                result.Add(FlagsHandler.CopyFlags(indexed, Context.AnyStringValue));
            }
            return result;
        }

        /// <inheritdoc />
        public override IEnumerable<Value> ReadValueIndex(Value value, MemberIdentifier index)
        {
            if (!(value is UndefinedValue))
            {
                SetWarning("Cannot use operator [] on variable other than string or array", AnalysisWarningCause.CANNOT_ACCESS_FIELD_OPERATOR_ON_NON_ARRAY);
            }

            //reading of index returns undefined value
            yield return Context.UndefinedValue;
        }

        /// <inheritdoc />
        public override IEnumerable<Value> WriteValueIndex(Value indexed, MemberIdentifier index, MemoryEntry writtenValue)
        {
            if (!(indexed is UndefinedValue))
            {
                SetWarning("Cannot use operator [] on variable other than string or array", AnalysisWarningCause.CANNOT_ACCESS_FIELD_OPERATOR_ON_NON_ARRAY);
            }

            //we dont want to change indexed value itself
            yield return indexed;
        }

        /// <inheritdoc />
        public override MemoryEntry Simplify(MemoryEntry entry)
        {
            var simplifier = new Simplifier(Context);
            return new MemoryEntry(simplifier.Simplify(entry));
        }

        /// <inheritdoc />
        public override IEnumerable<Value> WriteValueField(Value fielded, VariableIdentifier field, MemoryEntry writtenValue)
        {
            if (!(fielded is UndefinedValue))
            {
                SetWarning("Cannot use operator -> on variable other than object", AnalysisWarningCause.CANNOT_ACCESS_OBJECT_OPERATOR_ON_NON_OBJECT);
            }
            yield return fielded;
        }

        /// <inheritdoc />
        public override IEnumerable<Value> ReadValueField(Value fielded, VariableIdentifier field)
        {
            if (!(fielded is UndefinedValue))
            {
                SetWarning("Cannot use operator -> on variable other than object", AnalysisWarningCause.CANNOT_ACCESS_OBJECT_OPERATOR_ON_NON_OBJECT);
            }
            yield return Context.UndefinedValue;
        }

    }


    /// <summary>
    /// Visitor, for all visited values finds common abstract value
    /// </summary>
    public class WidenningVisitor : AbstractValueVisitor
    {
        /// <summary>
        /// Indicates if only boolean were visited
        /// </summary>
        private bool containsOnlyBool = true;

        /// <summary>
        /// Indicates if only numeric values were visited
        /// </summary>
        private bool containsOnlyNumvericValues = true;

        /// <summary>
        /// Indicates if only string values were visited
        /// </summary>
        private bool containsOnlyString = true;

        private Flags flags = new Flags();

        /// <summary>
        /// Widens given values
        /// </summary>
        /// <param name="values">input values</param>
        /// <param name="Context">Snapshot</param>
        /// <returns>Memory entry with widen values</returns>
        public MemoryEntry Widen(IEnumerable<Value> values, SnapshotBase Context)
        {
            flags = FlagsHandler.GetFlags(values);
            foreach (var value in values)
            {
                value.Accept(this);
            }
            return GetResult(Context);
        }

        /// <summary>
        /// Return Widen memory entry for all visited values
        /// </summary>
        /// <param name="Context">Output set</param>
        /// <returns>Widen memory entry for all visited values</returns>
        private MemoryEntry GetResult(SnapshotBase Context)
        {
            List<Value> result = new List<Value>();
            if (containsOnlyBool)
            {
                result.Add(Context.AnyBooleanValue);
                return new MemoryEntry(result);
            }
            if (containsOnlyNumvericValues)
            {
                result.Add(Context.AnyFloatValue);
                return new MemoryEntry(result);
            }
            if (containsOnlyString)
            {
                result.Add(Context.AnyStringValue.SetInfo(flags));
                return new MemoryEntry(result);
            }
            result.Add(Context.AnyValue.SetInfo(flags));
            return new MemoryEntry(result);
        }

        /// <summary>
        /// Indicates that numbered value was visited
        /// </summary>
        private void numberFound()
        {
            containsOnlyBool = false;
            containsOnlyString = false;
        }

        /// <summary>
        /// Indicates that boolean value was visited
        /// </summary>
        private void booleanFound()
        {
            containsOnlyNumvericValues = false;
            containsOnlyString = false;
        }

        /// <summary>
        /// Indicates that string value was visited
        /// </summary>
        private void stringFound()
        {
            containsOnlyNumvericValues = false;
            containsOnlyBool = false;
        }


        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            containsOnlyBool = false;
            containsOnlyString = false;
            containsOnlyNumvericValues = false;
        }

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            booleanFound();
        }

        /// <inheritdoc />
        public override void VisitAnyBooleanValue(AnyBooleanValue value)
        {
            booleanFound();
        }

        /// <inheritdoc />
        public override void VisitStringValue(StringValue value)
        {
            stringFound();
        }

        /// <inheritdoc />
        public override void VisitAnyStringValue(AnyStringValue value)
        {
            stringFound();
        }

        /// <inheritdoc />
        public override void VisitIntegerValue(IntegerValue value)
        {
            numberFound();
        }

        /// <inheritdoc />
        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            numberFound();
        }

        /// <inheritdoc />
        public override void VisitAnyIntegerValue(AnyIntegerValue value)
        {
            numberFound();
        }

        /// <inheritdoc />
        public override void VisitLongintValue(LongintValue value)
        {
            numberFound();
        }

        /// <inheritdoc />
        public override void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            numberFound();
        }

        /// <inheritdoc />
        public override void VisitAnyLongintValue(AnyLongintValue value)
        {
            numberFound();
        }

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            numberFound();
        }

        /// <inheritdoc />
        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            numberFound();
        }

        /// <inheritdoc />
        public override void VisitAnyFloatValue(AnyFloatValue value)
        {
            numberFound();
        }

    }

    class SimpleStringConverter : PartialExpressionEvaluator
    {
        bool IsConcrete = true;
        string result;
        private SnapshotBase Context;

        public SimpleStringConverter(SnapshotBase Context)
        {
            this.Context = Context;
        }
        public List<StringValue> Evaluate(IEnumerable<Value> input, out bool isConcrete)
        {
            List<StringValue> res = new List<StringValue>();

            foreach (var value in input)
            {
                result = "";
                value.Accept(this);
                if (result != "")
                {
                    res.Add(Context.CreateString(result));
                }
            }
            isConcrete = IsConcrete;
            return res;
        }

        /// <inheritdoc />
        public override void VisitAnyValue(AnyValue value)
        {
            IsConcrete = false;
        }

        /// <inheritdoc />
        public override void VisitGenericIntervalValue<T>(IntervalValue<T> value)
        {
            IsConcrete = false;
        }

        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            IsConcrete = false;
        }

        /// <inheritdoc />
        public override void VisitStringValue(StringValue value)
        {
            result = value.Value;
        }

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            if (value.Value)
                result = "1";
            else
                result = "0";
        }

        /// <inheritdoc />
        public override void VisitIntegerValue(IntegerValue value)
        {
            result = value.Value.ToString();
        }

        /// <inheritdoc />
        public override void VisitLongintValue(LongintValue value)
        {
            result = value.Value.ToString();
        }

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            result = value.Value.ToString();
        }

        /// <inheritdoc />
        public override void VisitAnyArrayValue(AnyArrayValue value)
        {
            result = "Array";
        }

        /// <inheritdoc />
        public override void VisitAssociativeArray(AssociativeArray value)
        {
            result = "Array";
        }
    }



    /// <summary>
    /// Performs the simplification of memory entry, inca it contains to many values.
    /// In some cases resulting memory entry contains leas accurate information
    /// </summary>
    public class Simplifier : AbstractValueVisitor
    {
        private HashSet<Value> result;
        private HashSet<Value> booleans;
        private HashSet<Value> strings;
        private bool containsInt = false;
        private int minInt = int.MaxValue;
        private int maxInt = int.MinValue;
        private bool containsLong = false;
        private long minLong = long.MaxValue;
        private long maxLong = long.MinValue;
        private bool containsFloat = false;
        private double minFloat = double.MaxValue;
        private double maxFloat = double.MinValue;
        private SnapshotBase context;

        /// <summary>
        /// Creates new instance of Simplifier
        /// </summary>
        /// <param name="context">SnapshotBase</param>
        public Simplifier(SnapshotBase context)
        {
            this.context = context;
            result = new HashSet<Value>();
            booleans = new HashSet<Value>();
            strings = new HashSet<Value>();
        }

        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            result.Add(value);
        }

        /// <summary>
        /// Perform simplification of memory entry
        /// In some cases resulting memory entry contains leas accurate information
        /// </summary>
        /// <param name="entry">Memory entry</param>
        /// <returns>simplified memory entry</returns>
        public IEnumerable<Value> Simplify(MemoryEntry entry)
        {
            foreach (var value in entry.PossibleValues)
            {
                value.Accept(this);
            }

            if (booleans.Count >= 2)
            {
                result.Add(context.AnyBooleanValue);
            }
            else
            {
                foreach (var boolean in booleans)
                {
                    result.Add(boolean);
                }
            }

            if (strings.Count >= 2)
            {
                result.Add(FlagsHandler.CopyFlags(strings, context.AnyStringValue));
            }
            else
            {
                foreach (var str in strings)
                {
                    result.Add(str);
                }
            }

            if (containsInt)
            {
                if (minInt == maxInt)
                {
                    result.Add(context.CreateInt(minInt));
                }
                else
                {
                    result.Add(context.CreateIntegerInterval(minInt, maxInt));
                }
            }

            if (containsLong)
            {
                if (minLong == maxLong)
                {
                    result.Add(context.CreateLong(minLong));
                }
                else
                {
                    result.Add(context.CreateLongintInterval(minLong, maxLong));
                }
            }

            if (containsFloat)
            {
                if (minFloat == maxFloat)
                {
                    result.Add(context.CreateDouble(minFloat));
                }
                else
                {
                    result.Add(context.CreateFloatInterval(minFloat, maxFloat));
                }
            }


            return result;
        }

        /// <inheritdoc />
        public override void VisitIntegerValue(IntegerValue value)
        {
            containsInt = true;
            minInt = Math.Min(minInt, value.Value);
            maxInt = Math.Max(maxInt, value.Value);
        }

        /// <inheritdoc />
        public override void VisitLongintValue(LongintValue value)
        {
            containsLong = true;
            minLong = Math.Min(minLong, value.Value);
            maxLong = Math.Max(maxLong, value.Value);
        }

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            containsFloat = true;
            minFloat = Math.Min(minFloat, value.Value);
            maxFloat = Math.Max(maxFloat, value.Value);
        }

        /// <inheritdoc />
        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            containsInt = true;
            minInt = Math.Min(Math.Min(minInt, value.Start), value.End);
            maxInt = Math.Max(Math.Max(maxInt, value.Start), value.End);
        }

        /// <inheritdoc />
        public override void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            containsLong = true;
            minLong = Math.Min(Math.Min(minLong, value.Start), value.End);
            maxLong = Math.Max(Math.Max(maxLong, value.Start), value.End);
        }

        /// <inheritdoc />
        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            containsFloat = true;
            minFloat = Math.Min(Math.Min(minFloat, value.Start), value.End);
            maxFloat = Math.Max(Math.Max(maxFloat, value.Start), value.End);
        }

        /// <inheritdoc />
        public override void VisitAnyIntegerValue(AnyIntegerValue value)
        {
            containsInt = true;
            minInt = Math.Min(minInt, int.MinValue);
            maxInt = Math.Max(maxInt, int.MaxValue);
        }

        /// <inheritdoc />
        public override void VisitAnyLongintValue(AnyLongintValue value)
        {
            containsLong = true;
            minLong = Math.Min(minLong, long.MinValue);
            maxLong = Math.Max(maxLong, long.MaxValue);
        }

        /// <inheritdoc />
        public override void VisitAnyFloatValue(AnyFloatValue value)
        {
            containsFloat = true;
            minFloat = Math.Min(minFloat, double.MinValue);
            maxFloat = Math.Max(maxFloat, double.MaxValue);
        }

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            booleans.Add(value);
        }

        /// <inheritdoc />
        public override void VisitAnyBooleanValue(AnyBooleanValue value)
        {
            booleans.Add(value);
        }

        /// <inheritdoc />
        public override void VisitStringValue(StringValue value)
        {
            strings.Add(value);
        }

        /// <inheritdoc />
        public override void VisitAnyStringValue(AnyStringValue value)
        {
            strings.Add(value);
        }

    }

    public class SnapshotTester<T> where T : SnapshotBase, new()
    {
        List<TestOperation<T>> operations = new List<TestOperation<T>>();

        TestOperationLogger logger = new BlankLogger();

        public T Snapshot { get; set; }

        public SnapshotTester()
        {
            MemoryAssistant assistant = new MemoryAssistant();

            Snapshot = new T();
            Snapshot.InitAssistant(assistant);
            Snapshot.StartTransaction();

            assistant.InitContext(Snapshot);
        }

        public SnapshotTester(T snapshot)
        {
            Snapshot = snapshot;
            Snapshot.StartTransaction();
        }

        public void SetLogger(TestOperationLogger logger)
        {
            this.logger = logger;
        }

        public SnapshotEntryFactory<T> Var(params string[] names)
        {
            return new SnapshotEntryFactory<T>(this, Snapshot.GetVariable(new VariableIdentifier(names)));
        }

        public void AddOperation(TestOperation<T> operation)
        {
            operations.Add(operation);
        }

        public void Test()
        {
            logger.Init(Snapshot);

            logger.WriteLine("Empty snapshot");
            logger.WriteLine(Snapshot.ToString());
            logger.WriteLine("------------------------------------------------");

            foreach (TestOperation<T> operation in operations)
            {
                operation.DoOperation(Snapshot, logger);
            }

            Snapshot.CommitTransaction();
            logger.Close(Snapshot);
        }

        public void Read(ReadWriteSnapshotEntryBase snapshotEntry)
        {
            AddOperation(new ReadOperation<T>(snapshotEntry));
        }

        public void Write(ReadWriteSnapshotEntryBase snapshotEntry, Value value)
        {
            AddOperation(new WriteOperation<T>(snapshotEntry, value));
        }

        public void Write(ReadWriteSnapshotEntryBase snapshotEntry, int value)
        {
            Write(snapshotEntry, Snapshot.CreateInt(value));
        }

        public void Write(ReadWriteSnapshotEntryBase snapshotEntry, string value)
        {
            Write(snapshotEntry, Snapshot.CreateString(value));
        }

        internal void Write(ReadWriteSnapshotEntryBase targetEntry, ReadWriteSnapshotEntryBase sourceEntry)
        {
            AddOperation(new WriteFromMemoryOperation<T>(targetEntry, sourceEntry));
        }

        internal void Merge(params T[] snapshots)
        {
            AddOperation(new MergeOperation<T>(snapshots));
        }

        internal void Alias(ReadWriteSnapshotEntryBase targetEntry, ReadWriteSnapshotEntryBase sourceEntry)
        {
            AddOperation(new AliasOperation<T>(targetEntry, sourceEntry));
        }
    }
}