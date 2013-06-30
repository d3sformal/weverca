using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using Weverca.Analysis.Memory;
using Weverca.Analysis;

namespace Weverca.MemoryModel
{
    class MemorySnapshot : AbstractSnapshot
    {
        Dictionary<VariableName, VariableInfo> variables = new Dictionary<VariableName, VariableInfo>();
        Dictionary<VariableName, VariableInfo> oldVariables;

        bool changedInTransaction = false;
        bool transactionStarted = false;

        TransactionCounter counterObj;
        public uint TransactionCounter { get; private set; }

        public MemorySnapshot()
        {
            TransactionCounter = 0;
        }

        protected override void startTransaction()
        {
            oldVariables = variables;
            variables = new Dictionary<VariableName, VariableInfo>(oldVariables);

            TransactionCounter++;
            counterObj = new MemoryModel.TransactionCounter(this);
            changedInTransaction = false;
            transactionStarted = true;
        }

        protected override bool commitTransaction()
        {
            transactionStarted = false;
            return changedInTransaction;
        }

      
        protected override Analysis.Memory.AliasValue createAlias(VariableName sourceVar)
        {
            return new AliasValue(sourceVar);
        }

        protected override AbstractSnapshot createCall(MemoryEntry ThisObject, MemoryEntry[] arguments)
        {
            throw new NotImplementedException();
        }

        protected override void assign(VariableName targetVar, Analysis.Memory.MemoryEntry entry)
        {
            

            VariableInfo variable;
            if (variables.TryGetValue(targetVar, out variable))
            {
                if (variable.Values.Equals(entry))
                {
                    return;
                }
                else
                {
                    updateMayAliases(variable, entry);
                    updateMustAliases(variable, entry);
                }
            }
            else
            {
                variable = new VariableInfo(entry, counterObj);
            }
        }

        private void updateMustAliases(VariableInfo variable, MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        private void updateMayAliases(VariableInfo variable, MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        protected override void assignAlias(VariableName targetVar, Analysis.Memory.AliasValue alias)
        {
            throw new NotImplementedException();
        }

        protected override void extend(ISnapshotReadonly[] inputs)
        {
            throw new NotImplementedException();
        }

        protected override void mergeWithCallLevel(ISnapshotReadonly[] callOutput)
        {
            throw new NotImplementedException();
        }

        protected override Analysis.Memory.MemoryEntry readValue(VariableName sourceVar)
        {
            return variables[sourceVar].Values;
        }


        protected override void setField(Analysis.Memory.ObjectValue value, ContainerIndex index, MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        protected override void setIndex(AssociativeArray value, ContainerIndex index, MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        protected override void setFieldAlias(Analysis.Memory.ObjectValue value, ContainerIndex index, Analysis.Memory.AliasValue alias)
        {
            throw new NotImplementedException();
        }

        protected override void setIndexAlias(AssociativeArray value, ContainerIndex index, Analysis.Memory.AliasValue alias)
        {
            throw new NotImplementedException();
        }

        protected override MemoryEntry getField(Analysis.Memory.ObjectValue value, ContainerIndex index)
        {
            throw new NotImplementedException();
        }

        protected override MemoryEntry getIndex(AssociativeArray value, ContainerIndex index)
        {
            throw new NotImplementedException();
        }

        protected override void fetchFromGlobal(IEnumerable<VariableName> variables)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<VariableName> getGlobalVariables()
        {
            throw new NotImplementedException();
        }

        protected override void initializeObject(Analysis.Memory.ObjectValue createdObject, PHP.Core.AST.TypeDecl type)
        {
            throw new NotImplementedException();
        }

        protected override void initializeArray(AssociativeArray createdArray)
        {
            throw new NotImplementedException();
        }
    }
}
