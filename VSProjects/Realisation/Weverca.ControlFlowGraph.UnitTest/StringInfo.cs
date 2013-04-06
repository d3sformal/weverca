using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.ControlFlowGraph.UnitTest
{
    /// <summary>
    /// Info which is collected during string analysis
    /// </summary>
    class StringVarInfo
    {
        /// <summary>
        /// Name of variable which belongs to this info.
        /// </summary>
        internal readonly string Name;

        /// <summary>
        /// Possible values which can be present in variable
        /// </summary>
        internal HashSet<string> PossibleValues=new HashSet<string>();


        internal StringVarInfo(string varName)
        {
            Name = varName;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var o=obj as StringVarInfo;
            if(o==null)
                return false;

            var sameCounts=PossibleValues.Count==o.PossibleValues.Count;
            var sameValues=PossibleValues.Union(o.PossibleValues).Count() == PossibleValues.Count;

            return Name == o.Name && sameCounts && sameValues;
        }
    }
}
