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

        [Obsolete("Names of variables and their behaviour according to unknown fields etc is up to analysis and wont be handled by framework")]
        public VariableName ReturnValue
        {
            get { return Snapshot.ReturnValue; }
        }

        public InfoValue[] ReadInfo(Value value)
        {
            return Snapshot.ReadInfo(value);
        }

        public InfoValue[] ReadInfo(VariableName variable)
        {
            return Snapshot.ReadInfo(variable);
        }

        [Obsolete("Use snapshot entry API instead")]
        public MemoryEntry ReadValue(VariableName sourceVar)
        {
            return Snapshot.ReadValue(sourceVar);
        }

        [Obsolete("Use snapshot entry API instead")]
        public bool TryReadValue(VariableName sourceVar, out MemoryEntry entry, bool forceGlobalContext = false)
        {
            return Snapshot.TryReadValue(sourceVar, out entry, forceGlobalContext);
        }

        [Obsolete("Use snapshot entry API instead")]
        public ContainerIndex CreateIndex(string identifier)
        {
            return Snapshot.CreateIndex(identifier);
        }

        [Obsolete("Use snapshot entry API instead")]
        public AliasValue CreateAlias(VariableName sourceVar)
        {
            return Snapshot.CreateAlias(sourceVar);
        }

        [Obsolete("Use snapshot entry API instead")]
        public AliasValue CreateIndexAlias(AssociativeArray array, ContainerIndex index)
        {
            return Snapshot.CreateIndexAlias(array, index);
        }

        [Obsolete("Use snapshot entry API instead")]
        public AliasValue CreateFieldAlias(ObjectValue objectValue, ContainerIndex field)
        {
            return Snapshot.CreateFieldAlias(objectValue, field);
        }

        public IEnumerable<FunctionValue> ResolveFunction(QualifiedName functionName)
        {
            return Snapshot.ResolveFunction(functionName);
        }

        [Obsolete("Use snapshot entry API instead")]
        public IEnumerable<FunctionValue> ResolveMethod(ObjectValue objectValue, QualifiedName methodName)
        {
            return Snapshot.ResolveMethod(objectValue, methodName);
        }

        public IEnumerable<TypeValueBase> ResolveType(QualifiedName typeName)
        {
            return Snapshot.ResolveType(typeName);
        }

        [Obsolete("Use snapshot entry API instead")]
        public MemoryEntry GetField(ObjectValue value, ContainerIndex index)
        {
            return Snapshot.GetField(value, index);
        }

        [Obsolete("Use snapshot entry API instead")]
        public bool TryGetField(ObjectValue objectValue, ContainerIndex field, out MemoryEntry entry)
        {
            return Snapshot.TryGetField(objectValue, field, out entry);
        }

        [Obsolete("Use snapshot entry API instead")]
        public MemoryEntry GetIndex(AssociativeArray value, ContainerIndex index)
        {
            return Snapshot.GetIndex(value, index);
        }

        public TypeValueBase ObjectType(ObjectValue objectValue)
        {
            return Snapshot.ObjectType(objectValue);
        }

        [Obsolete("Use snapshot entry API instead")]
        public bool TryGetIndex(AssociativeArray array, ContainerIndex index, out MemoryEntry entry)
        {
            return Snapshot.TryGetIndex(array, index, out entry);
        }

        [Obsolete("Use snapshot entry API instead")]
        public IEnumerable<ContainerIndex> IterateObject(ObjectValue iteratedObject)
        {
            return Snapshot.IterateObject(iteratedObject);
        }

        [Obsolete("Use snapshot entry API instead")]
        public IEnumerable<ContainerIndex> IterateArray(AssociativeArray iteratedArray)
        {
            return Snapshot.IterateArray(iteratedArray);
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
