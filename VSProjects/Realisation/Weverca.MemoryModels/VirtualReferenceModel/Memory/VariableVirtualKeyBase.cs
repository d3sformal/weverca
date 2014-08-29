/*
Copyright (c) 2012-2014 Miroslav Vodolan.

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
        /// <param name="s">Context snapshot of getter</param>
        /// <param name="storedValues">Values that are already stored</param>
        /// <returns></returns>
        protected abstract MemoryEntry getter(Snapshot s, MemoryEntry storedValues);

        /// <summary>
        /// Setter called for every attempt to write into current variable
        /// </summary>
        /// <param name="s">Context snapshot of getter</param>
        /// <param name="storedValues">Values that are already stored</param>
        /// <param name="writtenValue">Value that is written by setter</param>
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

        ///<inheritdoc />
        internal override VariableInfo GetVariable(Snapshot snapshot)
        {
            return snapshot.GetInfo(getStorage(), VariableKind.Meta);
        }

        ///<inheritdoc />
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

        ///<inheritdoc />
        public override string ToString()
        {
            return getStorageName();
        }
    }
}