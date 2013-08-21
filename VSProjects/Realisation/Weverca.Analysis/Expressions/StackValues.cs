using System;
using System.Collections.Generic;

using PHP.Core.AST;
using Weverca.Analysis.Memory;

namespace Weverca.Analysis.Expressions
{
    /// <summary>
    /// RValue wrapping memory entry
    /// </summary>
    internal class MemoryEntryValue : RValue
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

        public MemoryEntry InitializeIndexer(ExpressionEvaluatorBase evaluator)
        {
            //There is no initialization on memory entry

            return _entry;
        }
    }

    /// <summary>
    /// Stack value wrapping Object field
    /// </summary>
    internal class FieldEntryValue : LValue, RValue
    {
        private readonly MemoryEntry _objectValue;
        private readonly VariableEntry _fieldEntry;

        public LangElement AssociatedPartial { get; private set; }

        public FieldEntryValue(LangElement associatedPartial,MemoryEntry objectValue, VariableEntry fieldEntry)
        {
            AssociatedPartial = associatedPartial;
            _objectValue = objectValue;
            _fieldEntry = fieldEntry;
        }

        public MemoryEntry ReadValue(ExpressionEvaluatorBase evaluator)
        {
            return evaluator.ResolveField(_objectValue, _fieldEntry);
        }

        public MemoryEntry InitializeIndexer(ExpressionEvaluatorBase evaluator)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AliasValue> ReadAlias(ExpressionEvaluatorBase evaluator)
        {
            return evaluator.ResolveAliasedField(_objectValue, _fieldEntry);
        }

        public void AssignValue(ExpressionEvaluatorBase evaluator, MemoryEntry value)
        {
            evaluator.FieldAssign(_objectValue, _fieldEntry, value);
        }

        public void AssignAlias(ExpressionEvaluatorBase evaluator, IEnumerable<AliasValue> possibleAliasses)
        {
            evaluator.AliasedFieldAssign(_objectValue, _fieldEntry, possibleAliasses);
        }


        public VariableEntry GetVariableEntry()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Stack value wrapping variable
    /// </summary>
    internal class VariableEntryValue : LValue, RValue
    {
        private readonly VariableEntry _entry;

        public LangElement AssociatedPartial { get; private set; }

        public VariableEntryValue(LangElement associatedPartial,VariableEntry entry)
        {
            AssociatedPartial = associatedPartial;    
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

        public MemoryEntry InitializeIndexer(ExpressionEvaluatorBase evaluator)
        {
            return evaluator.ResolveIndexedVariable(_entry);
        }

        public IEnumerable<AliasValue> ReadAlias(ExpressionEvaluatorBase evaluator)
        {
            return evaluator.ResolveAlias(_entry);
        }


        public VariableEntry GetVariableEntry()
        {
            return _entry;
        }
    }

    /// <summary>
    /// Stack value wrapping array item
    /// </summary>
    internal class ArrayItem : LValue, RValue
    {
        private readonly MemoryEntry _array;
        private readonly MemoryEntry _index;

        public LangElement AssociatedPartial { get; private set; }

        public ArrayItem(LangElement associatedPartial,MemoryEntry array, MemoryEntry index)
        {
            AssociatedPartial = associatedPartial;
            _array = array;
            _index = index;
        }

        public void AssignValue(ExpressionEvaluatorBase evaluator, MemoryEntry value)
        {
            evaluator.IndexAssign(_array, _index, value);
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

        public MemoryEntry InitializeIndexer(ExpressionEvaluatorBase evaluator)
        {
            throw new NotImplementedException();
        }


        public VariableEntry GetVariableEntry()
        {
            throw new NotImplementedException();
        }
    }
}
