using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Analysis.Memory;
namespace Weverca.Analysis.Expressions
{
    class MemoryEntryValue : RValue
    {
        private readonly MemoryEntry _entry;

        internal MemoryEntryValue(MemoryEntry entry)
        {
            _entry = entry;
        }

        public MemoryEntry ReadValue(ExpressionEvaluator evaluator)
        {
            return _entry;
        }
        
        public IEnumerable<AliasValue> ReadAlias(ExpressionEvaluator evaluator)
        {
            throw new NotSupportedException("Cannot get alias on memory entry");
        }


        public MemoryEntry ReadArray(ExpressionEvaluator evaluator)
        {
            throw new NotImplementedException();
        }
    }

    class VariableEntryValue : LValue,RValue
    {
        private readonly VariableEntry _entry;
        
        public VariableEntryValue(VariableEntry entry)
        {
            _entry = entry;
        }

        public void AssignValue(ExpressionEvaluator evaluator, MemoryEntry value)
        {
            evaluator.Assign(_entry, value);
        }

        public void AliasAssign(ExpressionEvaluator evaluator, IEnumerable<AliasValue> possibleAliasses)
        {
            evaluator.AliasAssign(_entry, possibleAliasses);
        }

        public MemoryEntry ReadValue(ExpressionEvaluator evaluator)
        {
            return evaluator.ResolveVariable(_entry);
        }

        public MemoryEntry ReadArray(ExpressionEvaluator evaluator)
        {
            return evaluator.ResolveArray(_entry);
        }

        public IEnumerable<AliasValue> ReadAlias(ExpressionEvaluator evaluator)
        {
            return evaluator.ResolveAlias(_entry);
        }
         
    }

    class ArrayItem:LValue,RValue
    {
        private readonly MemoryEntry _array;
        private readonly MemoryEntry _index;

        public ArrayItem(MemoryEntry array, MemoryEntry index)
        {            
            _array = array;
            _index = index;
        }
        public void AssignValue(ExpressionEvaluator evaluator, MemoryEntry value)
        {
            evaluator.ArrayAssign(_array, _index,value);            
        }

        public void AliasAssign(ExpressionEvaluator evaluator, IEnumerable<AliasValue> possibleAliasses)
        {
            throw new NotImplementedException();
        }

        public MemoryEntry ReadValue(ExpressionEvaluator evaluator)
        {
           return evaluator.ArrayRead(_array, _index);
        }

        public IEnumerable<AliasValue> ReadAlias(ExpressionEvaluator evaluator)
        {
            throw new NotImplementedException();
        }


        public MemoryEntry ReadArray(ExpressionEvaluator evaluator)
        {
            throw new NotImplementedException();
        }
    }
}
