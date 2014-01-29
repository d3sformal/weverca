using System;
using System.Collections.Generic;

using PHP.Core;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Set of FlowInfo used as input for statement analysis.
    /// </summary>
    public class FlowInputSet : ISnapshotReadonly
    {
        /// <summary>
        /// Stored snapshot
        /// </summary>
        public readonly SnapshotBase Snapshot;

        internal FlowInputSet(SnapshotBase snapshot)
        {
            Snapshot = snapshot;
        }

        public string Representation
        {
            get
            {
                return Snapshot.ToString();
            }
        }

        public override string ToString()
        {
            return Representation;
        }

        #region ISnapshotReadonly implementation

        #region Value singletons
        public AnyStringValue AnyStringValue { get { return Snapshot.AnyStringValue; } }

        public AnyBooleanValue AnyBooleanValue { get { return Snapshot.AnyBooleanValue; } }

        public AnyIntegerValue AnyIntegerValue { get { return Snapshot.AnyIntegerValue; } }

        public AnyFloatValue AnyFloatValue { get { return Snapshot.AnyFloatValue; } }

        public AnyLongintValue AnyLongintValue { get { return Snapshot.AnyLongintValue; } }

        public AnyObjectValue AnyObjectValue { get { return Snapshot.AnyObjectValue; } }

        public AnyArrayValue AnyArrayValue { get { return Snapshot.AnyArrayValue; } }

        public AnyResourceValue AnyResourceValue { get { return Snapshot.AnyResourceValue; } }

        public AnyValue AnyValue { get { return Snapshot.AnyValue; } }

        public UndefinedValue UndefinedValue { get { return Snapshot.UndefinedValue; } }
        #endregion

        public InfoValue[] ReadInfo(Value value)
        {
            return Snapshot.ReadInfo(value);
        }

        public InfoValue[] ReadInfo(VariableName variable)
        {
            return Snapshot.ReadInfo(variable);
        }

        public IEnumerable<FunctionValue> ResolveFunction(QualifiedName functionName)
        {
            return Snapshot.ResolveFunction(functionName);
        }
    
        public IEnumerable<TypeValue> ResolveType(QualifiedName typeName)
        {
            return Snapshot.ResolveType(typeName);
        }

        public IEnumerable<FunctionValue> ResolveStaticMethod(TypeValue value, QualifiedName methodName)
        {
            return Snapshot.ResolveStaticMethod(value, methodName);
        }

        public TypeValue ObjectType(ObjectValue objectValue)
        {
            return Snapshot.ObjectType(objectValue);
        }

        #endregion

        #region Snapshot entry API

        public ReadSnapshotEntryBase ReadVariable(VariableIdentifier variable, bool forceGlobalContext = false)
        {
            return Snapshot.ReadVariable(variable, forceGlobalContext);
        }

        public ReadSnapshotEntryBase ReadControlVariable(VariableName variable)
        {
            return Snapshot.ReadControlVariable(variable);
        }

        public ReadSnapshotEntryBase ReadLocalControlVariable(VariableName variable)
        {
            return Snapshot.ReadControlVariable(variable);
        }

        #endregion



        
    }
}
