using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.Analysis.Expressions;

namespace Weverca.TaintedAnalysis.Widening
{
    class Widening : ExpressionEvaluatorBase
    {
        public override IEnumerable<string> VariableNames(Analysis.Memory.MemoryEntry variableSpecifier)
        {
            throw new NotImplementedException();
        }

        public override Analysis.Memory.MemoryEntry ResolveVariable(Analysis.VariableEntry variable)
        {
            throw new NotImplementedException();
        }

        public override Analysis.Memory.MemoryEntry ResolveIndexedVariable(Analysis.VariableEntry variable)
        {
            throw new NotImplementedException();
        }

        public override Analysis.Memory.MemoryEntry ResolveField(Analysis.Memory.MemoryEntry objectValue, Analysis.VariableEntry field)
        {
            throw new NotImplementedException();
        }

        public override Analysis.Memory.MemoryEntry ResolveIndex(Analysis.Memory.MemoryEntry indexedValue, Analysis.Memory.MemoryEntry index)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Analysis.Memory.AliasValue> ResolveAliasedField(Analysis.Memory.MemoryEntry objectValue, Analysis.VariableEntry aliasedField)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Analysis.Memory.AliasValue> ResolveAliasedIndex(Analysis.Memory.MemoryEntry arrayValue, Analysis.Memory.MemoryEntry aliasedIndex)
        {
            throw new NotImplementedException();
        }

        public override void AliasAssign(Analysis.VariableEntry target, IEnumerable<Analysis.Memory.AliasValue> possibleAliases)
        {
            throw new NotImplementedException();
        }

        public override void AliasedFieldAssign(Analysis.Memory.MemoryEntry objectValue, Analysis.VariableEntry aliasedField, IEnumerable<Analysis.Memory.AliasValue> possibleAliases)
        {
            throw new NotImplementedException();
        }

        public override void AliasedIndexAssign(Analysis.Memory.MemoryEntry arrayValue, Analysis.Memory.MemoryEntry aliasedIndex, IEnumerable<Analysis.Memory.AliasValue> possibleAliases)
        {
            throw new NotImplementedException();
        }

        public override void Assign(Analysis.VariableEntry target, Analysis.Memory.MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        public override void FieldAssign(Analysis.Memory.MemoryEntry objectValue, Analysis.VariableEntry targetField, Analysis.Memory.MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        public override void IndexAssign(Analysis.Memory.MemoryEntry indexedValue, Analysis.Memory.MemoryEntry index, Analysis.Memory.MemoryEntry assignedValue)
        {
            throw new NotImplementedException();
        }

        public override Analysis.Memory.MemoryEntry BinaryEx(Analysis.Memory.MemoryEntry leftOperand, PHP.Core.AST.Operations operation, Analysis.Memory.MemoryEntry rightOperand)
        {
            throw new NotImplementedException();
        }

        public override Analysis.Memory.MemoryEntry UnaryEx(PHP.Core.AST.Operations operation, Analysis.Memory.MemoryEntry operand)
        {
            throw new NotImplementedException();
        }

        public override Analysis.Memory.MemoryEntry IncDecEx(PHP.Core.AST.IncDecEx operation, Analysis.Memory.MemoryEntry incrementedValue)
        {
            throw new NotImplementedException();
        }

        public override Analysis.Memory.MemoryEntry ArrayEx(IEnumerable<KeyValuePair<Analysis.Memory.MemoryEntry, Analysis.Memory.MemoryEntry>> keyValuePairs)
        {
            throw new NotImplementedException();
        }

        public override void Foreach(Analysis.Memory.MemoryEntry enumeree, Analysis.VariableEntry keyVariable, Analysis.VariableEntry valueVariable)
        {
            throw new NotImplementedException();
        }

        public override Analysis.Memory.MemoryEntry Concat(IEnumerable<Analysis.Memory.MemoryEntry> parts)
        {
            throw new NotImplementedException();
        }

        public override void Echo(PHP.Core.AST.EchoStmt echo, Analysis.Memory.MemoryEntry[] entries)
        {
            throw new NotImplementedException();
        }

        public override Analysis.Memory.MemoryEntry Constant(PHP.Core.AST.GlobalConstUse x)
        {
            throw new NotImplementedException();
        }

        public override void ConstantDeclaration(PHP.Core.AST.ConstantDecl x, Analysis.Memory.MemoryEntry constantValue)
        {
            throw new NotImplementedException();
        }

        public override Analysis.Memory.MemoryEntry CreateObject(PHP.Core.QualifiedName typeName)
        {
            throw new NotImplementedException();
        }
    }
}
