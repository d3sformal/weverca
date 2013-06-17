using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.MemoryModel.Deprecated
{
    class Table
    {
        Dictionary<string, Variable> data;
        public Variable unknown;

        public Table()
        {
            data = new Dictionary<string, Variable>();
            unknown = new Variable();
        }

        public Variable GetOrCreateVariable(string variableName)
        {
            Variable res;
            if (!data.TryGetValue(variableName, out res))
            {
                res = new Variable();
                data[variableName] = res;
            }
            return res;
        }

        public bool TryGetVariable(string variableName, out Variable res)
        {
            return data.TryGetValue(variableName, out res);
        }

        public void Remove(string variableName)
        {
            data.Remove(variableName);
        }
        
        internal void Clear()
        {
            data.Clear();
        }
        
        public void CopyStructureFrom(Table source, CopyResolver resolver)
        {
            foreach (var pair in source.data)
            {
                Variable newVar = pair.Value.CopyStructure(resolver);
                this.data[pair.Key] = newVar;
            }

            unknown = unknown.CopyStructure(resolver);
        }

        public void MergeCopy(Table source, CopyResolver resolver)
        {
            foreach (var pair in source.data)
            {
                Variable variable;
                if (this.TryGetVariable(pair.Key, out variable))
                {
                    variable.MergeCopy(pair.Value, resolver);
                }
                else
                {
                    this.data[pair.Key] = pair.Value.CopyStructure(resolver);
                }
            }

            this.unknown.MergeCopy(source.unknown, resolver);
        }
    }

}
