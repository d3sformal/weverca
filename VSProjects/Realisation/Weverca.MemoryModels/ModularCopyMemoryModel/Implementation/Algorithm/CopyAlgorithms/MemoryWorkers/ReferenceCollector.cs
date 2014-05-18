using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers
{
    /// <summary>
    /// Collet must or may aliases in order to use it in merge and assign algoritms.
    /// </summary>
    public class ReferenceCollector
    {
        HashSet<MemoryIndex> mustReferences = new HashSet<MemoryIndex>();
        HashSet<MemoryIndex> allReferences = new HashSet<MemoryIndex>();
        bool mustSet = false;

        /// <summary>
        /// Copies the collected must references to given set of references.
        /// </summary>
        /// <param name="references">The references.</param>
        public void CopyMustReferencesTo(HashSet<MemoryIndex> references)
        {
            mustSet = true;
            foreach (MemoryIndex index in mustReferences)
            {
                references.Add(index);
            }
        }

        /// <summary>
        /// Copies the collected may references to given set of references.
        /// </summary>
        /// <param name="references">The references.</param>
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

        /// <summary>
        /// Collects must aliases from the specified set of references. When collected reference is already
        /// in may reference container collector removes it from collection of may aliases.
        /// </summary>
        /// <param name="references">The references.</param>
        /// <param name="callLevel">The call level.</param>
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

        /// <summary>
        /// Collects the may aliases. When the alias is already in collection of myst aliases the index is
        /// not inserted into any container.
        /// </summary>
        /// <param name="references">The references.</param>
        /// <param name="callLevel">The call level.</param>
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

        /// <summary>
        /// Transfers all must aliases into may.
        /// </summary>
        public void InvalidateMust()
        {
            mustReferences.Clear();
            mustSet = true;
        }

        /// <summary>
        /// Sets the collected aliases to specified index in the given spanshot.
        /// </summary>
        /// <param name="memoryIndex">Index of the memory.</param>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="isMust">if set to <c>true</c> uses must and may aliases otherwise all must aliases are invalidated.</param>
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
                snapshot.AddAlias(mayAlias, null, memoryIndex);
            }

            snapshot.AddAliases(memoryIndex, mustReferences, mayReferences);
        }

        /// <summary>
        /// Gets a value indicating whether collection has some alias.
        /// </summary>
        /// <value>
        ///   <c>true</c> if collection has some alias; otherwise, <c>false</c>.
        /// </value>
        public bool HasAliases { get { return allReferences.Count > 0; } }

        /// <summary>
        /// Gets a value indicating whether collection has some must alias.
        /// </summary>
        /// <value>
        ///   <c>true</c> if collection has some must alias; otherwise, <c>false</c>.
        /// </value>
        public bool HasMustAliases { get { return mustReferences.Count > 0; } }

        /// <summary>
        /// Adds the must alias into collection.
        /// </summary>
        /// <param name="memoryIndex">Index of the memory.</param>
        public void AddMustAlias(MemoryIndex memoryIndex)
        {
            allReferences.Add(memoryIndex);
            mustReferences.Add(memoryIndex);
        }
    }
}
