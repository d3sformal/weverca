using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel.Memory
{
    class VariableFieldKey : VariableVirtualKeyBase
    {
        private readonly VariableIdentifier _field;

        internal VariableFieldKey(VariableKeyBase fieldedVariable, VariableIdentifier field)
            : base(fieldedVariable)
        {
            _field = field;
        }

        protected override string getStorageName()
        {
            //TODO what about multiple names ?
            return string.Format("{0}_field-{1}", ParentVariable, _field.DirectName);
        }

        protected override MemoryEntry getter(Snapshot s, MemoryEntry storedValues)
        {
            var subResults = new HashSet<Value>();

            foreach (var value in storedValues.PossibleValues)
            {
                if (value is ObjectValue)
                    continue;

                if (value is UndefinedValue)
                    continue;

                if (value is AnyValue)
                    continue;

                var subResult = s.MemoryAssistant.ReadValueField(value, _field);
                subResults.UnionWith(subResult);
            }

            return new MemoryEntry(subResults);
        }

        protected override MemoryEntry setter(Snapshot s, MemoryEntry storedValues, MemoryEntry writtenValue)
        {
            var subResults = new HashSet<Value>();

            foreach (var value in storedValues.PossibleValues)
            {
                if (value is ObjectValue)
                    continue;

                if (value is UndefinedValue)
                    continue;

                if (value is AnyValue)
                    continue;

                var subResult = s.MemoryAssistant.WriteValueField(value, _field, writtenValue);
                subResults.UnionWith(subResult);
            }

            return new MemoryEntry(subResults);
        }
    }
}
