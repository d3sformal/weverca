using System;
using System.Collections.Generic;

using PHP.Core;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Represents possible names resolved for single variable selector in source code
    /// </summary>
    public class VariableIdentifier
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
        public VariableIdentifier(IEnumerable<string> possibleNames)
        {
            var variableNames = new List<VariableName>();
            foreach (var name in possibleNames)
            {
                variableNames.Add(new VariableName(name));
            }
            PossibleNames = variableNames.ToArray();
        }

        public VariableIdentifier(params string[] possibleNames)
            : this((IEnumerable<string>)possibleNames)
        {
        }

        /// <summary>
        /// Creates variable entry from direct name
        /// </summary>
        /// <param name="directName">Direct name for variable selector</param>
        public VariableIdentifier(VariableName directName)
        {
            PossibleNames = new VariableName[] { directName };
        }
    }
}
