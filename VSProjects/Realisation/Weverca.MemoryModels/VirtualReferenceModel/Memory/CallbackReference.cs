using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework.Memory;

using Weverca.MemoryModels.VirtualReferenceModel.Containers;

namespace Weverca.MemoryModels.VirtualReferenceModel.Memory
{

    delegate MemoryEntry GetEntry(Snapshot snapshot);

    delegate void SetEntry(Snapshot snapshot, MemoryEntry entry);

    class CallbackReference : VirtualReference
    {

        private readonly GetEntry _getter;

        private readonly SetEntry _setter;

        internal CallbackReference(VariableName originatedVar, GetEntry getter, SetEntry setter)
            : base(originatedVar, VariableKind.Meta, -1)
        {
            _getter = getter;
            _setter = setter;
        }

        internal override MemoryEntry GetEntry(Snapshot snapshot, DataContainer data)
        {
            return _getter(snapshot);
        }

        internal override void SetEntry(Snapshot snapshot, DataContainer data, MemoryEntry entry)
        {
            _setter(snapshot, entry);
        }

    }
}
