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
        public override MemoryEntry ReadIndex(AnyValue value, MemberIdentifier index)
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
        public override MemoryEntry ReadField(AnyValue value, VariableIdentifier field)
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
            //todo copy info
            var visitor = new WidenningVisitor();

            //todo maybe make more precise
            List<Value> allValues = new List<Value>(old.PossibleValues);
            allValues.AddRange(current.PossibleValues);
           
            foreach (var value in allValues)
            {
                value.Accept(visitor); 
            }
            return visitor.GetResult(Context);          
        }

        #endregion

        /// <summary>
        /// Generates a warning with the given message
        /// </summary>
        /// <param name="message">Text of warning</param>
        public void SetWarning(string message)
        {
            // TODO: AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(message, Element));
        }

        /// <summary>
        /// Generates a warning of the proper type and with the given message
        /// </summary>
        /// <param name="message">Text of warning</param>
        /// <param name="cause">More specific warning type</param>
        public void SetWarning(string message, AnalysisWarningCause cause)
        {
            // TODO: AnalysisWarningHandler.SetWarning(OutSet, new AnalysisWarning(message, Element, cause));
        }

        public override IEnumerable<FunctionValue> ResolveMethods(Value thisObject,TypeValue type, PHP.Core.QualifiedName methodName, IEnumerable<FunctionValue> objectMethods)
        {
            
            foreach (var method in objectMethods)
            {
                bool isstatic = (type.Declaration.ModeledMethods.Where(a => a.Key.Name == methodName.Name && a.Value.IsStatic == true).Count() > 0 || type.Declaration.SourceCodeMethods.Where(a => a.Key.Name == methodName.Name && a.Value.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Static) == true).Count() > 0);
                if (method.Name.Value == methodName.Name.Value && isstatic == false) 
                {
                    yield return method;
                }
            }
        }


        public override IEnumerable<FunctionValue> ResolveMethods(TypeValue value, PHP.Core.QualifiedName methodName, IEnumerable<FunctionValue> objectMethods)
        {
            foreach (var method in objectMethods)
            {
                bool isstatic = (value.Declaration.ModeledMethods.Where(a => a.Key.Name == methodName.Name && a.Value.IsStatic == true).Count() > 0 || value.Declaration.SourceCodeMethods.Where(a => a.Key.Name == methodName.Name && a.Value.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Static) == true).Count() > 0);
                if (method.Name.Value == methodName.Name.Value && isstatic == true)
                {
                    yield return method;
                }
            }
        }
    }



    public class WidenningVisitor : AbstractValueVisitor
    {
        bool containsOnlyBool = true;
        bool containsOnlyNumvericValues = true;
        bool containsOnlyString = true;
        public MemoryEntry GetResult(SnapshotBase Context)
        {
            if (containsOnlyBool)
            {
                return new MemoryEntry(Context.AnyBooleanValue);
            }
            if (containsOnlyNumvericValues)
            {
               return new MemoryEntry(Context.AnyFloatValue);
                
            }
            if (containsOnlyString)
            {
                return new MemoryEntry(Context.AnyStringValue);
            }

            return new MemoryEntry(Context.AnyValue);
        }

        public override void VisitValue(Value value)
        {
            containsOnlyBool = false;
            containsOnlyString = false;
            containsOnlyNumvericValues = false;
        }

        public override void VisitBooleanValue(BooleanValue value)
        {
            booleanFound();
        }

        public override void VisitAnyBooleanValue(AnyBooleanValue value)
        {
            booleanFound();
        }

        private void booleanFound()
        {
            containsOnlyNumvericValues = false;
            containsOnlyString = false;
        }

        public override void VisitStringValue(StringValue value)
        {
            stringFound();
        }

        public override void VisitAnyStringValue(AnyStringValue value)
        {
            stringFound();
        }

        private void stringFound()
        {
            containsOnlyNumvericValues = false;
            containsOnlyBool = false;
        }

        public override void VisitIntegerValue(IntegerValue value)
        {
            numberFound();
        }

        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            numberFound();
        }

        public override void VisitAnyIntegerValue(AnyIntegerValue value)
        {
            numberFound();
        }

        public override void VisitLongintValue(LongintValue value)
        {
            numberFound();
        }

        public override void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            numberFound();
        }

        public override void VisitAnyLongintValue(AnyLongintValue value)
        {
            numberFound();
        }

        public override void VisitFloatValue(FloatValue value)
        {
            numberFound();
        }

        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            numberFound();
        }

        public override void VisitAnyFloatValue(AnyFloatValue value)
        {
            numberFound();
        }

        private void numberFound()
        {
            containsOnlyBool = false;
            containsOnlyString = false;
        }

    }

}
