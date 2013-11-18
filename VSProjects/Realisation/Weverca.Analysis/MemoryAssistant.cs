using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.Analysis.ExpressionEvaluator;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis
{
    class MemoryAssistant : MemoryAssistantBase
    {
        public override MemoryEntry ReadIndex(AnyValue value, MemberIdentifier index)
        {
            //todo copy info
            return new MemoryEntry(Context.AnyValue);
        }

        public override MemoryEntry ReadField(AnyValue value, AnalysisFramework.VariableIdentifier field)
        {
            //todo copy info
            return new MemoryEntry(Context.AnyValue);
        }

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
    }
}
