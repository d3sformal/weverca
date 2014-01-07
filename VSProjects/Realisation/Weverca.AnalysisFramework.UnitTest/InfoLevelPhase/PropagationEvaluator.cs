using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;

namespace Weverca.AnalysisFramework.UnitTest.InfoLevelPhase
{
    class PropagationEvaluator : ExpressionEvaluatorBase
    {
        public override void Assign(ReadWriteSnapshotEntryBase target, MemoryEntry entry)
        {
            throw new NotImplementedException("Track variables");
        }

        public override IEnumerable<string> VariableNames(MemoryEntry variableSpecifier)
        {
            throw new NotImplementedException();
        }

        public override MemberIdentifier MemberIdentifier(MemoryEntry memberRepresentation)
        {
            throw new NotImplementedException();
        }

        public override ReadWriteSnapshotEntryBase ResolveVariable(VariableIdentifier variable)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry ResolveIndexedVariable(VariableIdentifier variable)
        {
            throw new NotImplementedException();
        }

        public override ReadWriteSnapshotEntryBase ResolveField(ReadSnapshotEntryBase objectValue, VariableIdentifier field)
        {
            throw new NotImplementedException();
        }

        public override ReadWriteSnapshotEntryBase ResolveIndex(ReadSnapshotEntryBase indexedValue, MemberIdentifier index)
        {
            throw new NotImplementedException();
        }

        public override void AliasAssign(ReadWriteSnapshotEntryBase target, ReadSnapshotEntryBase aliasedValue)
        {
            throw new NotImplementedException();
        }

        public override void FieldAssign(ReadSnapshotEntryBase objectValue, VariableIdentifier targetField, MemoryEntry assignedValue)
        {
            throw new NotImplementedException();
        }

        public override void IndexAssign(ReadSnapshotEntryBase indexedValue, MemoryEntry index, MemoryEntry assignedValue)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry BinaryEx(MemoryEntry leftOperand, PHP.Core.AST.Operations operation, MemoryEntry rightOperand)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry UnaryEx(PHP.Core.AST.Operations operation, MemoryEntry operand)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry IncDecEx(PHP.Core.AST.IncDecEx operation, MemoryEntry incrementedValue)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry ArrayEx(IEnumerable<KeyValuePair<MemoryEntry, MemoryEntry>> keyValuePairs)
        {
            throw new NotImplementedException();
        }

        public override void Foreach(MemoryEntry enumeree, ReadWriteSnapshotEntryBase keyVariable, ReadWriteSnapshotEntryBase valueVariable)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry Concat(IEnumerable<MemoryEntry> parts)
        {
            throw new NotImplementedException();
        }

        public override void Echo(PHP.Core.AST.EchoStmt echo, MemoryEntry[] entries)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry IssetEx(IEnumerable<VariableIdentifier> variables)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry EmptyEx(VariableIdentifier variable)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry Exit(PHP.Core.AST.ExitEx exit, MemoryEntry status)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry Constant(PHP.Core.AST.GlobalConstUse x)
        {
            throw new NotImplementedException();
        }

        public override void ConstantDeclaration(PHP.Core.AST.ConstantDecl x, MemoryEntry constantValue)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry CreateObject(PHP.Core.QualifiedName typeName)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry IndirectCreateObject(MemoryEntry possibleNames)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry InstanceOfEx(MemoryEntry expression, PHP.Core.QualifiedName typeName)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry IndirectInstanceOfEx(MemoryEntry expression, MemoryEntry possibleNames)
        {
            throw new NotImplementedException();
        }
    }
}
