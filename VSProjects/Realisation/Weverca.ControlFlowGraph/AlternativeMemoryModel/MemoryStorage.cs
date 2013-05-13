using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using PHP.Core;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel
{
    /// <summary>
    /// Real storage for MemoryContext abstraction
    /// </summary>
    class MemoryStorage
    {
        /// <summary>
        /// References according their originated variables - is used because of getting same VirtualReference
        /// for all Context branches
        /// </summary>
        Dictionary<VariableName, VirtualReference> _varReferences = new Dictionary<VariableName, VirtualReference>();

        Dictionary<MemoryEntryId, MemoryEntry> _entries = new Dictionary<MemoryEntryId, MemoryEntry>();

        HashSet<MemoryContextVersion> _contextVersions = new HashSet<MemoryContextVersion>();

        MemoryContextVersion _writeableVersion;


        internal IEnumerable<AbstractValue> GetPossibleValues(MemoryContext context, VirtualReference reference)
        {
            MemoryEntry entry;
            if (!_entries.TryGetValue(reference.MemoryEntryId, out entry))
            {
                throw new NotSupportedException("Reference contains invalid MemoryEntryId - probably belongs to another MemoryStorage");
            }
            System.Diagnostics.Debug.Assert(entry.CreationVersion == reference.CreationVersion,"Versions has to match, possible invalid implementation of reference creating");
            

            return entry.GetPossibleValues(context.Version);
        }


        internal void Write(IEnumerable<VirtualReference> references, IEnumerable<AbstractValue> values)
        {
            var isMayAlias = references.Count() > 1;

            foreach (var reference in references)
            {
                var entry = getEntry(reference.MemoryEntryId);

                if (isMayAlias)
                {
                    entry.Add(_writeableVersion,values);
                }
                else
                {
                    entry.Set(_writeableVersion, values);
                }
            }
        }

        internal void OpenWriting(MemoryContextVersion buildedVersion)
        {
            if (_writeableVersion != null)
            {
                throw new NotSupportedException("Cannot open multiple memory context versions for writing");
            }

            if (buildedVersion == null)
            {
                throw new ArgumentNullException("buildedVersion");
            }

            if (!_contextVersions.Add(buildedVersion))
            {
                throw new NotSupportedException("Cannot open context version for writing twice");
            }

            _writeableVersion = buildedVersion;
        }

        internal void CloseWriting()
        {
            if (_writeableVersion == null)
            {
                throw new NotSupportedException("There is no opened memory context version that can be closed");
            }

            _writeableVersion = null;
        }


        internal VirtualReference ReferenceForVariable(VariableName variableName)
        {
            VirtualReference reference;
            if(_varReferences.TryGetValue(variableName, out reference)){
                return reference;
            }

            if(_writeableVersion==null){
                throw new NotSupportedException("Cannot create reference, if there is no opened memory context version for writing");
            }

            var memoryEntryId=allocateMemoryEntry();

            reference=new VirtualReference(memoryEntryId,getEntry(memoryEntryId).CreationVersion);

            return reference;
        }

        private MemoryEntryId allocateMemoryEntry()
        {
            var entryId=new MemoryEntryId(_entries.Count+1);
            var entry=new MemoryEntry(_contextVersions.Count+1);

            _entries[entryId] = entry;
            return entryId;
        }

        private MemoryEntry getEntry(MemoryEntryId id)
        {
            return _entries[id];
        }
    }
}
