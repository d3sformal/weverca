using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.Analysis.Memory;
using Weverca.Analysis.Expressions;

namespace Weverca.Analysis
{
    /// <summary>
    /// Provide access to values computed for every partial. These values are
    /// </summary>
    public class EvaluationLog
    {
        #region Partial associations members
        /// <summary>
        /// Value associations for partials
        /// </summary>
        private Dictionary<LangElement, MemoryEntry> _partialValues = new Dictionary<LangElement, MemoryEntry>();

        /// <summary>
        /// Variable associations for partials
        /// </summary>
        private Dictionary<LangElement, VariableEntry> _partialVariables = new Dictionary<LangElement, VariableEntry>();
        #endregion
        
        #region Internal methods for associating partials
        /// <summary>
        /// Associate value for given partial. The value was computed for given partial.
        /// </summary>
        /// <param name="partial">Partial which value is associated</param>
        /// <param name="value">Associated value</param>
        internal void AssociateValue(LangElement partial, MemoryEntry value)
        {
            _partialValues[partial] = value;
        }

        /// <summary>
        /// Associate variable for given partial. The variable was computed for given partial.
        /// </summary>
        /// <param name="partial">Partial which variable is associated</param>
        /// <param name="value">Associated variable</param>
        internal void AssociateVariable(LangElement partial, VariableEntry variable)
        {
            _partialVariables[partial] = variable;
        }
        #endregion

        #region Public methods for retrieving associations for partials
        /// <summary>
        /// Get value associated (computed during analysis) for given partial.         
        /// </summary>
        /// <param name="partial">Partial which value will be returned</param>
        /// <returns>Associated value, or null if there is no associated value</returns>
        public MemoryEntry GetValue(LangElement partial)
        {
            MemoryEntry result;
            _partialValues.TryGetValue(partial, out result);
            return result;
        }

        /// <summary>
        /// Get variable associated (computed during analysis) for given partial.         
        /// </summary>
        /// <param name="partial">Partial which variable will be returned</param>
        /// <returns>Associated variable, or null if there is no associated variable</returns>
        public VariableEntry GetVariable(LangElement partial)
        {
            VariableEntry result;
            _partialVariables.TryGetValue(partial,out result);
            return result;
        }
        #endregion
    }
}
