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
        MemoryEntry ReadValue(ExpressionEvaluator evaluator);
        /// <summary>
        /// Special kind of read - can be used for implicit array creation
        /// </summary>
        /// <param name="evaluator"></param>
        /// <returns></returns>
        MemoryEntry ReadArray(ExpressionEvaluator evaluator);
        IEnumerable<AliasValue> ReadAlias(ExpressionEvaluator evaluator);
    }

    interface LValue:IStackValue
    {
        void AssignValue(ExpressionEvaluator evaluator, MemoryEntry value);

        void AliasAssign(ExpressionEvaluator evaluator, IEnumerable<AliasValue> possibleAliasses);
    }
}
