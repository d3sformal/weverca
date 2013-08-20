using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Analysis.Memory;
using PHP.Core.AST;

namespace Weverca.Analysis.Expressions
{

    /// <summary>
    /// Base for stack value used for partial evaluation of statements
    /// </summary>
    interface IStackValue
    {
    }

    interface RValue:IStackValue
    {
        /// <summary>
        /// Read as simple value
        /// </summary>
        /// <param name="evaluator">Evaluator used for reading value</param>
        /// <returns>Read values</returns>
        MemoryEntry ReadValue(ExpressionEvaluatorBase evaluator);

        /// <summary>
        /// Read value as index - can be used for implicit array creation
        /// </summary>
        /// <param name="evaluator">Evaluator used for reading value</param>
        /// <returns>Read values</returns>
        MemoryEntry ReadIndex(ExpressionEvaluatorBase evaluator);

        /// <summary>
        /// Read value as alias 
        /// </summary>
        /// <param name="evaluator">Evaluator used for reading value</param>
        /// <returns>Read alias</returns>
        IEnumerable<AliasValue> ReadAlias(ExpressionEvaluatorBase evaluator);

        
    }

    interface LValue:IStackValue
    {
        LangElement AssociatedPartial { get; }
        /// <summary>
        /// Simple value assign to LValue
        /// </summary>
        /// <param name="evaluator">Evaluator used for assigning value</param>
        /// <param name="value">Assigned value</param>
        void AssignValue(ExpressionEvaluatorBase evaluator, MemoryEntry value);

        /// <summary>
        /// Simple alias assign to LValue
        /// </summary>
        /// <param name="evaluator">Evaluator used for assigning value</param>
        /// <param name="possibleAliasses">Assigned aliases</param>
        void AssignAlias(ExpressionEvaluatorBase evaluator, IEnumerable<AliasValue> possibleAliasses);

        VariableEntry GetVariableEntry();
    }
}
