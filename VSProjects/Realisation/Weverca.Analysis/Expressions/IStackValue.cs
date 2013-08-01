using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Analysis.Memory;
namespace Weverca.Analysis.Expressions
{

    interface IStackValue
    {
    }

    interface RValue:IStackValue
    {
        MemoryEntry ReadValue(ExpressionEvaluatorBase evaluator);
        /// <summary>
        /// Special kind of read - can be used for implicit array creation
        /// </summary>
        /// <param name="evaluator"></param>
        /// <returns></returns>
        MemoryEntry ReadArray(ExpressionEvaluatorBase evaluator);
        IEnumerable<AliasValue> ReadAlias(ExpressionEvaluatorBase evaluator);
    }

    interface LValue:IStackValue
    {
        void AssignValue(ExpressionEvaluatorBase evaluator, MemoryEntry value);

        void AliasAssign(ExpressionEvaluatorBase evaluator, IEnumerable<AliasValue> possibleAliasses);
    }
}
