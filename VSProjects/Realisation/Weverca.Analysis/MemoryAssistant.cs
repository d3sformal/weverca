using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.Analysis.ExpressionEvaluator;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis
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
            return visitor.Widen(allValues,Context);          
        }

        #endregion

        /// <summary>
        /// Generates a warning with the given message
        /// </summary>
        /// <param name="message">Text of warning</param>
        public void SetWarning(string message)
        {
            AnalysisWarningHandler.SetWarning(Context, new AnalysisWarning(Point.OwningPPGraph.OwningScript.FullName, message, Point.Partial));
        }

        /// <summary>
        /// Generates a warning of the proper type and with the given message
        /// </summary>
        /// <param name="message">Text of warning</param>
        /// <param name="cause">More specific warning type</param>
        public void SetWarning(string message, AnalysisWarningCause cause)
        {
            AnalysisWarningHandler.SetWarning(Context, new AnalysisWarning(Point.OwningPPGraph.OwningScript.FullName, message, Point.Partial, cause));
        }

        /// <inheritdoc />
        public override IEnumerable<FunctionValue> ResolveMethods(Value thisObject,TypeValue type, PHP.Core.QualifiedName methodName, IEnumerable<FunctionValue> objectMethods)
        {
            
            foreach (var method in objectMethods)
            {
                //bool isstatic = (type.Declaration.ModeledMethods.Where(a => a.Key.Name == methodName.Name && a.Value.IsStatic == true).Count() > 0 || type.Declaration.SourceCodeMethods.Where(a => a.Key.Name == methodName.Name && a.Value.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Static) == true).Count() > 0);
                if (method.Name.Value == methodName.Name.Value)// && isstatic == false) 
                {
                    yield return method;
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<FunctionValue> ResolveMethods(TypeValue value, PHP.Core.QualifiedName methodName, IEnumerable<FunctionValue> objectMethods)
        {
            foreach (var method in objectMethods)
            {
                //bool isstatic = (value.Declaration.ModeledMethods.Where(a => a.Key.Name == methodName.Name && a.Value.IsStatic == true).Count() > 0 || value.Declaration.SourceCodeMethods.Where(a => a.Key.Name == methodName.Name && a.Value.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Static) == true).Count() > 0);
                if (method.Name.Value == methodName.Name.Value)// && isstatic == true)
                {
                    yield return method;
                }
            }
        }

        /// <inheritdoc />
        public override ObjectValue CreateImplicitObject()
        {
            return Context.CreateObject(Context.CreateType(ForwardAnalysis.nativeObjectAnalyzer.GetClass(new QualifiedName(new Name("stdClass")))));
        }

        /// <inheritdoc />
        public override void TriedIterateFields(Value value)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<int> ResolveStringIndex(StringValue value,MemberIdentifier Index)
        {
            var NumberIndices=new HashSet<int>();
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
            foreach (var number in ResolveStringIndex(value,index))
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
                        newValue=indexed;
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
                result.Add(FlagsHandler.CopyFlags(indexed,Context.AnyStringValue));
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

        private Flags flags=new Flags();

        /// <summary>
        /// Widens given values
        /// </summary>
        /// <param name="values">input values</param>
        /// <param name="Context">Snapshot</param>
        /// <returns>Memory entry with widen values</returns>
        public MemoryEntry Widen(IEnumerable<Value> values,SnapshotBase Context)
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
                result.Add(Context.AnyBooleanValue);
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




    class Simplifier : AbstractValueVisitor
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

        public Simplifier(SnapshotBase context)
        {
            this.context = context;
            result = new HashSet<Value>();
            booleans = new HashSet<Value>();
        }

        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            result.Add(value);
        }

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
                result.Add(FlagsHandler.CopyFlags(strings,context.AnyStringValue));
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
                    result.Add(context.CreateIntegerInterval(minInt,maxInt));
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
            minInt = Math.Min(Math.Min(minInt, value.Start),value.End);
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
            minFloat = Math.Min(minFloat, float.MinValue);
            maxFloat = Math.Max(maxFloat, float.MaxValue);
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
}
