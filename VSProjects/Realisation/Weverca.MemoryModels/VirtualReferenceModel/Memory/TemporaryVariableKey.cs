using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel.Memory
{
    /// <summary>
    /// Is used for storing temporary variable within callback references
    /// </summary>
    class TemporaryVariableKey : VariableKeyBase
    {
        /// <summary>
        /// Temporary variable info stored within current key
        /// </summary>
        private readonly VariableInfo _tmpInfo;

        /// <summary>
        /// Temporary value stored withing current key
        /// </summary>
        private MemoryEntry _storedValue;

        internal TemporaryVariableKey(MemoryEntry tmpValue)
        {
            _storedValue = tmpValue;

            _tmpInfo = new VariableInfo(new VariableName(".tmp"), VariableKind.Meta);
            var reference = new CallbackReference(_tmpInfo.Name, _getter, _setter);

            _tmpInfo.References.Add(reference);
        }

        /// <inheritdoc />
        internal override VariableInfo GetOrCreateVariable(Snapshot snapshot)
        {
            return GetVariable(snapshot);
        }

        /// <inheritdoc />
        internal override VariableInfo GetVariable(Snapshot snapshot)
        {
            return _tmpInfo;
        }

        /// <inheritdoc />
        internal override VirtualReference CreateImplicitReference(Snapshot snapshot)
        {
            throw new NotSupportedException("Implicit references on temporary variables are not supported");
        }

        /// <summary>
        /// Gettter used for obtaining stored value
        /// </summary>
        /// <param name="context">Context where value is read</param>
        /// <returns>Obtained value</returns>
        private MemoryEntry _getter(Snapshot context)
        {
            return _storedValue;
        }

        /// <summary>
        /// Setter used for setting stored value
        /// </summary>
        /// <param name="context">Context where value is set</param>
        /// <param name="value">Set value</param>
        private void _setter(Snapshot context, MemoryEntry value)
        {
            _storedValue = value;
        }
    }
}
