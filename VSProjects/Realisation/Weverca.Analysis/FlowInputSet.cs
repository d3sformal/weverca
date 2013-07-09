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
        protected internal AbstractSnapshot _snapshot;

        internal FlowInputSet(AbstractSnapshot snapshot)
        {
            _snapshot = snapshot;
        }

        #region ISnapshotReadonly implementation

        public VariableName ReturnValue { get { return _snapshot.ReturnValue; } }

        public MemoryEntry ThisObject { get { return _snapshot.ThisObject; } }

        public VariableName Argument(int index)
        {
            return _snapshot.Argument(index);
        }

        public MemoryEntry ReadValue(VariableName sourceVar)
        {
            return _snapshot.ReadValue(sourceVar);
        }
        
        public ContainerIndex CreateIndex(string identifier)
        {
            return _snapshot.CreateIndex(identifier);
        }
        
        public IEnumerable<FunctionValue> ResolveFunction(QualifiedName functionName)
        {
            return _snapshot.ResolveFunction(functionName);
        }


        public IEnumerable<MethodDecl> ResolveMethod(ObjectValue objectValue, QualifiedName methodName)
        {
            return _snapshot.ResolveMethod(objectValue, methodName);
        }
        
        public IEnumerable<TypeValue> ResolveType(QualifiedName typeName)
        {
            return _snapshot.ResolveType(typeName);
        }

        public MemoryEntry GetField(ObjectValue value, ContainerIndex index)
        {
            return _snapshot.GetField(value, index);
        }

        public MemoryEntry GetIndex(AssociativeArray value, ContainerIndex index)
        {
            return _snapshot.GetIndex(value, index);
        }
        #endregion


        public override string ToString()
        {
            return _snapshot.ToString();
        }





    }
}
