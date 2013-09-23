using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis
{
    /// <summary>
    /// Set of FlowInfo used as input for statement analysis.
    /// </summary>    
    public class FlowInputSet : ISnapshotReadonly
    {
        /// <summary>
        /// Stored snapshot
        /// </summary>
        protected internal readonly SnapshotBase Snapshot;

        internal FlowInputSet(SnapshotBase snapshot)
        {
            Snapshot = snapshot;
        }

        #region ISnapshotReadonly implementation

        public VariableName ReturnValue { get { return Snapshot.ReturnValue; } }

        public bool VariableExists(VariableName variable, bool forceGlobalContext = false)
        {
            return Snapshot.VariableExists(variable, forceGlobalContext);
        }

        public InfoValue[] ReadInfo(Value value)
        {
            return Snapshot.ReadInfo(value);
        }

        public InfoValue[] ReadInfo(VariableName variable)
        {
            return Snapshot.ReadInfo(variable);
        }

        public MemoryEntry ReadValue(VariableName sourceVar)
        {
            return Snapshot.ReadValue(sourceVar);
        }

        public ContainerIndex CreateIndex(string identifier)
        {
            return Snapshot.CreateIndex(identifier);
        }

        public AliasValue CreateAlias(VariableName sourceVar)
        {
            return Snapshot.CreateAlias(sourceVar);
        }

        public AliasValue CreateIndexAlias(AssociativeArray array, ContainerIndex index)
        {
            return Snapshot.CreateIndexAlias(array, index);
        }

        public AliasValue CreateFieldAlias(ObjectValue objectValue, ContainerIndex field)
        {
            return Snapshot.CreateFieldAlias(objectValue, field);
        }

        public IEnumerable<FunctionValue> ResolveFunction(QualifiedName functionName)
        {
            return Snapshot.ResolveFunction(functionName);
        }

        public IEnumerable<FunctionValue> ResolveMethod(ObjectValue objectValue, QualifiedName methodName)
        {
            return Snapshot.ResolveMethod(objectValue, methodName);
        }

        public IEnumerable<TypeValue> ResolveType(QualifiedName typeName)
        {
            return Snapshot.ResolveType(typeName);
        }

        public MemoryEntry GetField(ObjectValue value, ContainerIndex index)
        {
            return Snapshot.GetField(value, index);
        }

        public MemoryEntry GetIndex(AssociativeArray value, ContainerIndex index)
        {
            return Snapshot.GetIndex(value, index);
        }

        public IEnumerable<ContainerIndex> IterateObject(ObjectValue iteratedObject)
        {
            return Snapshot.IterateObject(iteratedObject);
        }

        public IEnumerable<ContainerIndex> IterateArray(AssociativeArray iteratedArray)
        {
            return Snapshot.IterateArray(iteratedArray);
        }
        #endregion


        public override string ToString()
        {
            return Representation;
        }

        public string Representation
        {
            get
            {
                return Snapshot.ToString();
            }
        }

    }
}
