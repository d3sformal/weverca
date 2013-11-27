using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class ReferenceCollector
    {
        HashSet<MemoryIndex> mustReferences = new HashSet<MemoryIndex>();
        HashSet<MemoryIndex> allReferences = new HashSet<MemoryIndex>();
        bool mustSet = false;

        public void CopyMustReferencesTo(HashSet<MemoryIndex> references)
        {
            mustSet = true;
            foreach (MemoryIndex index in mustReferences)
            {
                references.Add(index);
            }
        }

        public void CopyMayReferencesTo(HashSet<MemoryIndex> references)
        {
            foreach (MemoryIndex index in allReferences)
            {
                if (!mustReferences.Contains(index))
                {
                    references.Add(index);
                }
            }
        }

        public void CollectMust(IEnumerable<MemoryIndex> references, int callLevel)
        {
            if (!mustSet)
            {
                foreach (MemoryIndex index in references)
                {
                    if (index.CallLevel <= callLevel)
                    {
                        mustReferences.Add(index);
                        allReferences.Add(index);
                    }
                }
                mustSet = true;
            }
            else
            {
                HashSet<MemoryIndex> newMust = new HashSet<MemoryIndex>();
                foreach (MemoryIndex index in references)
                {
                    if (index.CallLevel <= callLevel)
                    {
                        if (mustReferences.Contains(index))
                        {
                            newMust.Add(index);
                        }

                        allReferences.Add(index);
                    }
                }
                mustReferences = newMust;
            }

        }

        public void CollectMay(IEnumerable<MemoryIndex> references, int callLevel)
        {
            foreach (MemoryIndex index in references)
            {
                if (index.CallLevel <= callLevel)
                {
                    allReferences.Add(index);
                }
            }
        }

        public void InvalidateMust()
        {
            mustReferences.Clear();
            mustSet = true;
        }

        internal void SetAliases(MemoryIndex memoryIndex, IReferenceHolder snapshot, bool isMust)
        {
            if (allReferences.Count == 0)
            {
                return;
            }

            if (!isMust)
            {
                InvalidateMust();
            }

            HashSet<MemoryIndex> mayReferences = new HashSet<MemoryIndex>();
            CopyMayReferencesTo(mayReferences);

            foreach (MemoryIndex mustAlias in mustReferences)
            {
                snapshot.AddAlias(mustAlias, memoryIndex, null);
            }

            foreach (MemoryIndex mayAlias in mayReferences)
            {
                snapshot.AddAlias(memoryIndex, null, mayAlias);
            }

            snapshot.AddAliases(memoryIndex, mustReferences, mayReferences);
        }

        public bool HasAliases { get { return allReferences.Count > 0; } }

        public bool HasMustAliases { get { return mustReferences.Count > 0; } }

        internal void AddMustAlias(MemoryIndex memoryIndex)
        {
            allReferences.Add(memoryIndex);
            mustReferences.Add(memoryIndex);
        }
    }
}
