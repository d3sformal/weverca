using System;
using System.Collections.Generic;

using PHP.Core;

namespace Weverca.Analysis
{
    /// <summary>
    /// Represents possible names resolved for single variable selector in source code
    /// </summary>
    public class VariableEntry
    {
        /// <summary>
        /// Possible names of variable
        /// </summary>
        public readonly VariableName[] PossibleNames;

        /// <summary>
        /// Determine that there is only single possible name
        /// </summary>
        public bool IsDirect { get { return PossibleNames.Length == 1; } }

        /// <summary>
        /// If VariableEntry IsDirect we can read it's name
        /// </summary>
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
        /// Creates variable entry from given possible names
        /// </summary>
        /// <param name="possibleNames">Possible names for variable selector</param>
        internal VariableEntry(IEnumerable<string> possibleNames)
        {
            var variableNames = new Stack<VariableName>();
            foreach (var name in possibleNames)
            {
                variableNames.Push(new VariableName(name));
            }
            PossibleNames = variableNames.ToArray();
        }

        /// <summary>
        /// Creates variable entry from direct name
        /// </summary>
        /// <param name="directName">Direct name for variable selector</param>
        internal VariableEntry(VariableName directName)
        {
            PossibleNames = new VariableName[] { directName };
        }
    }
}
