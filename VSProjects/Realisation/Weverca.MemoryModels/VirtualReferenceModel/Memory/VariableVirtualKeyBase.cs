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
    abstract class VariableVirtualKeyBase : VariableKeyBase
    {
        /// <summary>
        /// Storage of indexed value - here is pointing current key
        /// </summary>
        protected abstract string getStorageName();

        /// <summary>
        /// Getter called for every attempt to read current variable
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        protected abstract MemoryEntry getter(Snapshot s, MemoryEntry storedValues);

        /// <summary>
        /// Setter called for every attempt to write into current variable
        /// </summary>
        /// <param name="s"></param>
        /// <param name="writtenValue"></param>
        /// <returns>Memory entry for backwrite into parentVariable</returns>
        protected abstract MemoryEntry setter(Snapshot s, MemoryEntry storedValues, MemoryEntry writtenValue);

        /// <summary>
        /// Variable which is base for virtual key base
        /// </summary>
        protected readonly VariableKeyBase ParentVariable;

        internal VariableVirtualKeyBase(VariableKeyBase parentVariable)
        {
            ParentVariable = parentVariable;
        }

        internal override VariableInfo GetOrCreateVariable(Snapshot snapshot)
        {
            var storage = getStorage();
            var variable = snapshot.GetInfo(storage, VariableKind.Meta);
            if (variable == null)
            {
                variable = snapshot.CreateEmptyVar(storage, VariableKind.Meta);
                variable.References.Add(createProxyReference());
            }

            return variable;
        }

        internal override VariableInfo GetVariable(Snapshot snapshot)
        {
            return snapshot.GetInfo(getStorage(), VariableKind.Meta);
        }

        internal override VirtualReference CreateImplicitReference(Snapshot snapshot)
        {
            //there shouldnt be needed for creating implicit reference
            return createProxyReference();
        }

        private VirtualReference createProxyReference()
        {
            return new CallbackReference(getStorage(), _getter, setIndex);
        }

        private MemoryEntry _getter(Snapshot s)
        {
            var values = getStoredValues(s);

            return getter(s, values);
        }

        private void setIndex(Snapshot s, MemoryEntry writtenValue)
        {
            var values = getStoredValues(s);
            var backWrite = setter(s, values, writtenValue);

            s.Write(new[] { ParentVariable }, backWrite, true, true);
        }

        private MemoryEntry getStoredValues(Snapshot s)
        {
            var values = s.ReadValue(ParentVariable);
            return values;
        }

        private VariableName getStorage()
        {
            var storageName = getStorageName();

            return new VariableName(storageName);
        }

        public override string ToString()
        {
            return getStorageName();
        }
    }
}
