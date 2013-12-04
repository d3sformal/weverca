using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHP.Core;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class DeclarationContainer<T>
    {
        private Dictionary<QualifiedName, HashSet<T>> declarations;

        public int Count { get { return declarations.Count; } }

        public DeclarationContainer()
        {
            declarations = new Dictionary<QualifiedName, HashSet<T>>();
        }

        public DeclarationContainer(DeclarationContainer<T> container)
        {
            declarations = new Dictionary<QualifiedName, HashSet<T>>();
            foreach (var decl in container.declarations)
            {
                declarations[decl.Key] = new HashSet<T>(decl.Value);
            }
        }

        public bool ContainsKey(QualifiedName key)
        {
            return declarations.ContainsKey(key);
        }

        public bool TryGetValue(QualifiedName key, out IEnumerable<T> value)
        {
            HashSet<T> val;
            bool ret = declarations.TryGetValue(key, out val);

            value = val;
            return ret;
        }

        public IEnumerable<T> GetValue(QualifiedName key)
        {
            return declarations[key];
        }

        public void Add(QualifiedName key, T value)
        {
            HashSet<T> set;
            if (!declarations.TryGetValue(key, out set))
            {
                set = new HashSet<T>();
                declarations[key] = set;
            }

            if (!set.Contains(value))
            {
                set.Add(value);
            }
        }

        public IEnumerable<QualifiedName> GetNames()
        {
            return declarations.Keys;
        }

        internal bool DataEquals(DeclarationContainer<T> other)
        {
            foreach (var decl in this.declarations)
            {
                HashSet<T> otherDecl;
                if (other.declarations.TryGetValue(decl.Key, out otherDecl))
                {
                    if (!HashSetTools.EqualsSet(decl.Value, otherDecl))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
