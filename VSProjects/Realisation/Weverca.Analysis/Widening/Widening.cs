using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Expressions;

namespace Weverca.Analysis.Widening
{
    class Widening : ExpressionEvaluatorBase
    {
        public override IEnumerable<string> VariableNames(AnalysisFramework.Memory.MemoryEntry variableSpecifier)
        {
            throw new NotImplementedException();
        }

        public override AnalysisFramework.Memory.MemoryEntry ResolveVariable(AnalysisFramework.VariableEntry variable)
        {
            throw new NotImplementedException();
        }

        public override AnalysisFramework.Memory.MemoryEntry ResolveIndexedVariable(AnalysisFramework.VariableEntry variable)
        {
            throw new NotImplementedException();
        }

        public override AnalysisFramework.Memory.MemoryEntry ResolveField(AnalysisFramework.Memory.MemoryEntry objectValue, AnalysisFramework.VariableEntry field)
        {
            throw new NotImplementedException();
        }

        public override AnalysisFramework.Memory.MemoryEntry ResolveIndex(AnalysisFramework.Memory.MemoryEntry indexedValue, AnalysisFramework.Memory.MemoryEntry index)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<AnalysisFramework.Memory.AliasValue> ResolveAliasedField(AnalysisFramework.Memory.MemoryEntry objectValue, AnalysisFramework.VariableEntry aliasedField)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<AnalysisFramework.Memory.AliasValue> ResolveAliasedIndex(AnalysisFramework.Memory.MemoryEntry arrayValue, AnalysisFramework.Memory.MemoryEntry aliasedIndex)
        {
            throw new NotImplementedException();
        }

        public override void AliasAssign(AnalysisFramework.VariableEntry target, IEnumerable<AnalysisFramework.Memory.AliasValue> possibleAliases)
        {
            throw new NotImplementedException();
        }

        public override void AliasedFieldAssign(AnalysisFramework.Memory.MemoryEntry objectValue, AnalysisFramework.VariableEntry aliasedField, IEnumerable<AnalysisFramework.Memory.AliasValue> possibleAliases)
        {
            throw new NotImplementedException();
        }

        public override void AliasedIndexAssign(AnalysisFramework.Memory.MemoryEntry arrayValue, AnalysisFramework.Memory.MemoryEntry aliasedIndex, IEnumerable<AnalysisFramework.Memory.AliasValue> possibleAliases)
        {
            throw new NotImplementedException();
        }

        public override void Assign(AnalysisFramework.VariableEntry target, AnalysisFramework.Memory.MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        public override void FieldAssign(AnalysisFramework.Memory.MemoryEntry objectValue, AnalysisFramework.VariableEntry targetField, AnalysisFramework.Memory.MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        public override void IndexAssign(AnalysisFramework.Memory.MemoryEntry indexedValue, AnalysisFramework.Memory.MemoryEntry index, AnalysisFramework.Memory.MemoryEntry assignedValue)
        {
            throw new NotImplementedException();
        }

        public override AnalysisFramework.Memory.MemoryEntry BinaryEx(AnalysisFramework.Memory.MemoryEntry leftOperand, PHP.Core.AST.Operations operation, AnalysisFramework.Memory.MemoryEntry rightOperand)
        {
            throw new NotImplementedException();
        }

        public override AnalysisFramework.Memory.MemoryEntry UnaryEx(PHP.Core.AST.Operations operation, AnalysisFramework.Memory.MemoryEntry operand)
        {
            throw new NotImplementedException();
        }

        public override AnalysisFramework.Memory.MemoryEntry IncDecEx(PHP.Core.AST.IncDecEx operation, AnalysisFramework.Memory.MemoryEntry incrementedValue)
        {
            throw new NotImplementedException();
        }

        public override AnalysisFramework.Memory.MemoryEntry ArrayEx(IEnumerable<KeyValuePair<AnalysisFramework.Memory.MemoryEntry, AnalysisFramework.Memory.MemoryEntry>> keyValuePairs)
        {
            throw new NotImplementedException();
        }

        public override void Foreach(AnalysisFramework.Memory.MemoryEntry enumeree, AnalysisFramework.VariableEntry keyVariable, AnalysisFramework.VariableEntry valueVariable)
        {
            throw new NotImplementedException();
        }

        public override AnalysisFramework.Memory.MemoryEntry Concat(IEnumerable<AnalysisFramework.Memory.MemoryEntry> parts)
        {
            throw new NotImplementedException();
        }

        public override void Echo(PHP.Core.AST.EchoStmt echo, AnalysisFramework.Memory.MemoryEntry[] entries)
        {
            throw new NotImplementedException();
        }

        public override AnalysisFramework.Memory.MemoryEntry Constant(PHP.Core.AST.GlobalConstUse x)
        {
            throw new NotImplementedException();
        }

        public override void ConstantDeclaration(PHP.Core.AST.ConstantDecl x, AnalysisFramework.Memory.MemoryEntry constantValue)
        {
            throw new NotImplementedException();
        }

        public override AnalysisFramework.Memory.MemoryEntry CreateObject(PHP.Core.QualifiedName typeName)
        {
            throw new NotImplementedException();
        }
    }
}
