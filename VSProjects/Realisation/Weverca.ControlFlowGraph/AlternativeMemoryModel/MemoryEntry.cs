﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Weverca.ControlFlowGraph.AlternativeMemoryModel
{
    using ValueStorage = Dictionary<MemoryContextVersion, IEnumerable<AbstractValue>>;

    /// <summary>
    /// Referenced item in MemoryContext
    /// 
    /// TODO: Current version is CPU inefficient
    /// </summary>
    class MemoryEntry
    {
        internal readonly int CreationVersion;

        private ValueStorage  _values = new ValueStorage();


        internal IEnumerable<AbstractValue> GetPossibleValues(MemoryContextVersion version)
        {
            var currentVersion=version;

            //find newest version
            IEnumerable<AbstractValue> result;
            while (!_values.TryGetValue(currentVersion, out result))
            {
                currentVersion = currentVersion.Parent;
                if (currentVersion == null)
                {
                    //we are at hiearchy root
                    break;
                }
            }

            if (result == null)
            {
                //reference is in current version uninitialized
                return new AbstractValue[0];                
            }

            return result;
        }
    }
}
