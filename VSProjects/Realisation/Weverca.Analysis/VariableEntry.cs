using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

namespace Weverca.Analysis
{
    public class VariableEntry
    {
        /// <summary>
        /// Determine that there is only single possible name
        /// </summary>
        public bool IsDirect { get { return PossibleNames.Length == 1; } }

        public VariableName DirectName
        {
            get
            {
                if (!IsDirect)
                {
                    throw new NotSupportedException("Cannot get direct variable name on InDirect entry");
                }

                return PossibleNames[0];
            }
        }

        /// <summary>
        /// Possible names of variable
        /// </summary>
        public readonly VariableName[] PossibleNames;        


        internal VariableEntry(IEnumerable<string> variableNames)
        {
            var possibleNames = from name in variableNames select new VariableName(name);
            PossibleNames = possibleNames.ToArray();            
        }

        public VariableEntry(VariableName variableName)
        {
            PossibleNames = new VariableName[] { variableName };
        }
    }
}
