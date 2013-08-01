using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Analysis.Memory;
namespace Weverca.Analysis.Expressions
{

    /// <summary>
    /// RValue wrapping memory entry
    /// </summary>
    class MemoryEntryValue : RValue
    {
        private readonly MemoryEntry _entry;

        internal MemoryEntryValue(MemoryEntry entry)
        {
            _entry = entry;
        }

        public MemoryEntry ReadValue(ExpressionEvaluatorBase evaluator)
        {
            return _entry;
        }
        
        public IEnumerable<AliasValue> ReadAlias(ExpressionEvaluatorBase evaluator)
        {
            throw new NotSupportedException("Cannot get alias on memory entry");
        }
        
        public MemoryEntry ReadIndex(ExpressionEvaluatorBase evaluator)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Stack value wrapping Object field
    /// </summary>
    class FieldEntryValue : LValue, RValue
    {
        private readonly MemoryEntry _objectValue;
        private readonly VariableEntry _fieldEntry;

        public FieldEntryValue(MemoryEntry objectValue, VariableEntry fieldEntry)
        {
            _objectValue=objectValue;
            _fieldEntry = fieldEntry;
        }

        public MemoryEntry ReadValue(ExpressionEvaluatorBase evaluator)
        {
            return evaluator.ResolveField(_objectValue, _fieldEntry);
        }

        public MemoryEntry ReadIndex(ExpressionEvaluatorBase evaluator)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AliasValue> ReadAlias(ExpressionEvaluatorBase evaluator)
        {
            return evaluator.ResolveAlias(_objectValue, _fieldEntry);
        }

        public void AssignValue(ExpressionEvaluatorBase evaluator, MemoryEntry value)
        {
            evaluator.Assign(_objectValue, _fieldEntry, value);
        }

        public void AssignAlias(ExpressionEvaluatorBase evaluator, IEnumerable<AliasValue> possibleAliasses)
        {
            evaluator.AliasAssign(_objectValue, _fieldEntry, possibleAliasses);
        }
    }

    /// <summary>
    /// Stack value wrapping variable
    /// </summary>
    class VariableEntryValue : LValue,RValue
    {
        private readonly VariableEntry _entry;
        
        public VariableEntryValue(VariableEntry entry)
        {
            _entry = entry;
        }

        public void AssignValue(ExpressionEvaluatorBase evaluator, MemoryEntry value)
        {
            evaluator.Assign(_entry, value);
        }

        public void AssignAlias(ExpressionEvaluatorBase evaluator, IEnumerable<AliasValue> possibleAliasses)
        {
            evaluator.AliasAssign(_entry, possibleAliasses);
        }

        public MemoryEntry ReadValue(ExpressionEvaluatorBase evaluator)
        {
            return evaluator.ResolveVariable(_entry);
        }

        public MemoryEntry ReadIndex(ExpressionEvaluatorBase evaluator)
        {
            return evaluator.ResolveIndexedVariable(_entry);
        }

        public IEnumerable<AliasValue> ReadAlias(ExpressionEvaluatorBase evaluator)
        {
            return evaluator.ResolveAlias(_entry);
        }
         
    }

    /// <summary>
    /// Stack value wrapping array item
    /// </summary>
    class ArrayItem:LValue,RValue
    {
        private readonly MemoryEntry _array;
        private readonly MemoryEntry _index;

        public ArrayItem(MemoryEntry array, MemoryEntry index)
        {            
            _array = array;
            _index = index;
        }
        public void AssignValue(ExpressionEvaluatorBase evaluator, MemoryEntry value)
        {
            evaluator.IndexAssign(_array, _index,value);            
        }

        public void AssignAlias(ExpressionEvaluatorBase evaluator, IEnumerable<AliasValue> possibleAliasses)
        {
            throw new NotImplementedException();
        }

        public MemoryEntry ReadValue(ExpressionEvaluatorBase evaluator)
        {
           return evaluator.ResolveIndex(_array, _index);
        }

        public IEnumerable<AliasValue> ReadAlias(ExpressionEvaluatorBase evaluator)
        {
            throw new NotImplementedException();
        }


        public MemoryEntry ReadIndex(ExpressionEvaluatorBase evaluator)
        {
            throw new NotImplementedException();
        }
    }
}
