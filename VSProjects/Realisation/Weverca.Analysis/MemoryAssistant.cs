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
                return new MemoryEntry(Context.AnyValue);
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


            //todo maybe make more precise
            List<Value> allValues = new List<Value>(old.PossibleValues);
            allValues.AddRange(current.PossibleValues);
            bool containsFloat = false;
            bool containsLong = false;
            bool containsOnlyBool = true;
            bool containsOnlyNumvericValues = true;
            bool containsOnlyString = true;
            foreach (var value in allValues)
            {
                if (ValueTypeResolver.IsBool(value))
                {
                    containsOnlyNumvericValues = false;
                    containsOnlyString = false;
                }
                else if (ValueTypeResolver.IsInt(value))
                {
                    containsOnlyBool = false;
                    containsOnlyString = false;
                }
                else if (ValueTypeResolver.IsLong(value))
                {
                    containsLong = true;
                    containsOnlyBool = false;
                    containsOnlyString = false;
                }
                else if (ValueTypeResolver.IsFloat(value))
                {
                    containsFloat = true;
                    containsOnlyBool = false;
                    containsOnlyString = false;
                }
                else if (ValueTypeResolver.IsString(value))
                {
                    containsOnlyNumvericValues = false;
                    containsOnlyBool = false;
                }
                else
                {
                    containsOnlyBool = false;
                    containsOnlyString = false;
                    containsOnlyNumvericValues = false;
                }
            }

            if (containsOnlyBool)
            {
                return new MemoryEntry(Context.AnyBooleanValue);
            }
            if (containsOnlyNumvericValues)
            {
                if (containsFloat)
                {
                    return new MemoryEntry(Context.AnyFloatValue);
                }
                else if (containsLong)
                {
                    return new MemoryEntry(Context.AnyLongintValue);
                }
                else
                {
                    return new MemoryEntry(Context.AnyIntegerValue);
                }
            }
            if (containsOnlyString)
            {
                return new MemoryEntry(Context.AnyStringValue);
            }

            return new MemoryEntry(Context.AnyValue);
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
    }
}
