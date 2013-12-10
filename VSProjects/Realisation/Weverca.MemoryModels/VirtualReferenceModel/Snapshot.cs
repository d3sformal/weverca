using System;
using System.Collections.Generic;
using System.Text;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

using Weverca.MemoryModels.VirtualReferenceModel.Memory;
using Weverca.MemoryModels.VirtualReferenceModel.Containers;
using Weverca.MemoryModels.VirtualReferenceModel.SnapshotEntries;

namespace Weverca.MemoryModels.VirtualReferenceModel
{
    /// <summary>
    /// Simple (non efficient) implementation as proof of concept - will be heavily optimized, refactored
    /// </summary>
    public class Snapshot : SnapshotBase
    {
        /// <summary>
        /// Hold unique id for every snapshot. The id is used
        /// for storing info about snapshot sequences.
        /// </summary>
        private static int _uniqueStamp = 1;

        private VariableContainer _locals;
        private VariableContainer _globals;
        private VariableContainer _meta;
        private VariableContainer _globalControls;
        private VariableContainer _localControls;

        private DataContainer _data;

        /// <summary>
        /// Unique stamp for current snapshot
        /// </summary>
        internal readonly int SnapshotStamp = ++_uniqueStamp;

        /// <summary>
        /// Determine stamp of snapshot that is leader of current call context
        /// </summary>
        internal int CurrentContextStamp { get; private set; }

        /// <summary>
        /// Determine that this snapshot is pointed to global scope
        /// </summary>
        internal bool IsGlobalScope { get { return CurrentContextStamp == 0; } }

        internal MemoryAssistantBase MemoryAssistant { get { return Assistant; } }

        public Snapshot()
        {
            _globals = new VariableContainer(VariableKind.Global, this);
            _locals = new VariableContainer(VariableKind.Local, this, _globals);
            _localControls = new VariableContainer(VariableKind.LocalControl, this, _globalControls);
            _globalControls = new VariableContainer(VariableKind.GlobalControl, this);
            _meta = new VariableContainer(VariableKind.Meta, this);

            _data = new DataContainer(this);
        }

        #region Snapshot manipulation

        protected override void startTransaction()
        {
            _locals.FlipBuffers();
            _globals.FlipBuffers();
            _localControls.FlipBuffers();
            _globalControls.FlipBuffers();
            _meta.FlipBuffers();
            _data.FlipBuffers();


            // when not extend as call or from non global snapshot, context is global
            CurrentContextStamp = 0;
        }

        protected override bool commitTransaction()
        {
            return commit();
        }


        protected override bool widenAndCommitTransaction()
        {
            _data.WidenWith(Assistant);
            return commit();
        }

        private bool commit()
        {
            if (
            _locals.DifferInCount ||
            _meta.DifferInCount ||
            _globals.DifferInCount ||
            _localControls.DifferInCount ||
            _globalControls.DifferInCount ||
            _data.DifferInCount
            )
            {
                //there is some difference in size of containers
                //it means that there is change according to previous transaction
                return true;
            }


            //check variables according to old ones
            if (
                _locals.CheckChange() ||
                _meta.CheckChange() ||
                _globalControls.CheckChange() ||
                _globals.CheckChange() ||
                _localControls.CheckChange()
                )
            {
                //there is change in variable info
                return true;
            }

            return _data.CheckChange();
        }

        protected override void extendAsCall(SnapshotBase callerContext, MemoryEntry thisObject, MemoryEntry[] arguments)
        {
            // called context is extended only at begining of call
            CurrentContextStamp = SnapshotStamp;

            var input = callerContext as Snapshot;

            _globals.ExtendBy(input._globals, true);
            _meta.ExtendBy(input._meta, true);
            _globalControls.ExtendBy(input._globalControls, true);
            _data.ExtendBy(input._data, true);

            if (thisObject != null)
            {
                assign(getThisObjectStorage(), thisObject);
            }
        }

        /// <summary>
        /// TODO this implementation is inefficient
        /// </summary>
        /// <param name="inputs"></param>
        protected override void extend(ISnapshotReadonly[] inputs)
        {
            _data.ClearCurrent();
            _locals.ClearCurrent();
            _localControls.ClearCurrent();

            var isFirst = true;
            foreach (Snapshot input in inputs)
            {
                //merge info from extending inputs
                _globals.ExtendBy(input._globals, isFirst);
                _globalControls.ExtendBy(input._globalControls, isFirst);
                _locals.ExtendBy(input._locals, isFirst);
                _localControls.ExtendBy(input._localControls, isFirst);
                _meta.ExtendBy(input._meta, isFirst);

                _data.ExtendBy(input._data, isFirst);

                //all context stamps on extend level should be same
                CurrentContextStamp = input.CurrentContextStamp;
                isFirst = false;
            }
        }

        protected override void mergeWithCallLevel(ISnapshotReadonly[] callOutput)
        {
            var isFirst = true;
            foreach (Snapshot callInput in callOutput)
            {
                //Local variables are not extended
                _globals.ExtendBy(callInput._globals, isFirst);
                _meta.ExtendBy(callInput._meta, isFirst);
                _globalControls.ExtendBy(callInput._globalControls, isFirst);

                _data.ExtendBy(callInput._data, isFirst);
                isFirst = false;
            }
        }

        #endregion

        #region Snapshot entry API

        protected override ReadWriteSnapshotEntryBase getVariable(VariableIdentifier variable, bool forceGlobal)
        {
            var kind = repairKind(VariableKind.Local, forceGlobal);

            var storages = new List<VariableKey>();
            foreach (var name in variable.PossibleNames)
            {
                var key = getOrCreateKey(name, kind);
                storages.Add(key);
            }

            return new SnapshotStorageEntry(variable, false, storages.ToArray());
        }

        protected override ReadWriteSnapshotEntryBase getControlVariable(VariableName name)
        {
            return new SnapshotStorageEntry(new VariableIdentifier(name), false, new[] { getOrCreateKey(name, VariableKind.GlobalControl) });
        }

        protected override ReadWriteSnapshotEntryBase getLocalControlVariable(VariableName name)
        {
            return new SnapshotStorageEntry(new VariableIdentifier(name), false, new[] { getOrCreateKey(name, VariableKind.LocalControl) });
        }

        protected override ReadWriteSnapshotEntryBase createSnapshotEntry(MemoryEntry entry)
        {
            return new SnapshotMemoryEntry(entry);
        }

        #endregion

        #region API for Snapshot entries

        private void writeDirect(VariableKey storage, MemoryEntry value)
        {
            var variable = getOrCreateInfo(storage);
            assign(variable, value);
        }

        internal void Write(VariableKey[] storages, MemoryEntry value, bool forceStrongWrite, bool noArrayCopy)
        {
            var references = new List<VirtualReference>();
            var copiedValue = noArrayCopy ? value : copyArrays(value);

            foreach (var storage in storages)
            {
                //translation because of memory model cross-contexts
                var variable = getOrCreateInfo(storage);

                if (variable.References.Count == 0)
                {
                    //reserve new virtual reference for assigning
                    var allocatedReference = new VirtualReference(variable.Name, variable.Kind, storage.ContextStamp);
                    variable.References.Add(allocatedReference);
                }

                foreach (var reference in variable.References)
                {
                    if (references.Contains(reference))
                        continue;

                    if (forceStrongWrite)
                        setEntry(reference, copiedValue);

                    references.Add(reference);
                }
            }

            if (forceStrongWrite)
            {
                //references has already been set while collecting references
                return;
            }

            assign(references, copiedValue);
        }

        internal MemoryEntry copyArrays(MemoryEntry input, Dictionary<AssociativeArray, AssociativeArray> closure = null)
        {
            if (!input.ContainsAssociativeArray)
                return input;


            var values = new List<Value>();
            foreach (var value in input.PossibleValues)
            {
                if (value is AssociativeArray)
                {
                    var copy = deepArrayCopy(value as AssociativeArray, closure);
                    values.Add(copy);
                }
                else
                {
                    values.Add(value);
                }
            }

            REPORT(Statistic.MemoryEntryCreation);
            return new MemoryEntry(values);
        }

        internal IEnumerable<ReferenceAliasEntry> Aliases(VariableKey[] storages)
        {
            foreach (var storage in storages)
            {
                yield return new ReferenceAliasEntry(storage, storage.ContextStamp);
            }
        }

        internal void SetAliases(VariableKey[] storages, IEnumerable<ReferenceAliasEntry> aliases)
        {
            foreach (var storage in storages)
            {
                var variable = getOrCreateInfo(storage);

                assignAlias(variable, aliases);
            }
        }

        internal bool IsDefined(VariableKey storage)
        {
            var variables = getVariableContainer(storage.Kind);
            return variables.ContainsKey(storage.Name);
        }

        internal MemoryEntry ReadValue(VariableKey key)
        {
            var variable = getOrCreateInfo(key);

            if (variable.References.Count == 0)
            {
                //implicit reference creation, because of possible cross call aliases
                variable.References.Add(new VirtualReference(variable, key.ContextStamp));
            }

            var value = readValue(variable);
            return value;
        }

        internal IEnumerable<VariableKey> IndexStorages(Value array, MemberIdentifier index)
        {
            var storages = new List<VariableKey>();
            foreach (var indexName in index.PossibleNames)
            {
                //TODO refactor ContainerIndex API
                var containerIndex = CreateIndex(indexName);
                var key = getIndexStorage(array, containerIndex);

                storages.Add(key);
            }

            return storages;
        }

        internal IEnumerable<VariableKey> FieldStorages(Value objectValue, VariableIdentifier field)
        {
            var storages = new List<VariableKey>();
            foreach (var fieldName in field.PossibleNames)
            {
                //TODO refactor ContainerIndex API
                var fieldIndex = CreateIndex(fieldName.Value);
                var key = getFieldStorage(objectValue, fieldIndex);
                storages.Add(key);
            }

            return storages;
        }

        /// <summary>
        /// Method allowing statitic reporting via Snapshot entries
        /// </summary>
        /// <param name="statistic">Reported statistic</param>
        internal void ReportStatistic(Statistic statistic)
        {
            REPORT(statistic);
        }

        #endregion

        #region Global scope operations

        protected override void fetchFromGlobal(IEnumerable<VariableName> variables)
        {
            foreach (var variable in variables)
            {
                var global = getOrCreateInfo(variable, VariableKind.Global);
                _locals.Fetch(global);
            }
        }

        protected override IEnumerable<VariableName> getGlobalVariables()
        {
            return _globals.VariableNames;
        }

        protected override void declareGlobal(TypeValue declaration)
        {
            var storage = getTypeStorage(declaration.QualifiedName.Name.Value);

            var entry = readValue(storage);

            ReportMemoryEntryCreation();
            assign(storage, new MemoryEntry(declaration));
        }

        protected override void declareGlobal(FunctionValue function)
        {
            var storage = getFunctionStorage(function.Name.Value);

            ReportMemoryEntryCreation();
            assign(storage, new MemoryEntry(function));
        }


        protected override IEnumerable<FunctionValue> resolveFunction(QualifiedName functionName)
        {
            var storage = getFunctionStorage(functionName.Name.Value);

            MemoryEntry entry;
            if (tryReadValue(storage, out entry))
            {
                var functions = new List<FunctionValue>(entry.Count);
                foreach (var value in entry.PossibleValues)
                {
                    var function = value as FunctionValue;
                    Debug.Assert(function != null, "Every value read from function storage is a function");
                    functions.Add(function);
                }

                return functions;
            }
            else
            {
                return new List<FunctionValue>(0);
            }
        }

        protected override IEnumerable<TypeValue> resolveType(QualifiedName typeName)
        {
            var storage = getTypeStorage(typeName.Name.Value);

            MemoryEntry entry;
            if (tryReadValue(storage, out entry))
            {
                var types = new List<TypeValue>(entry.Count);
                foreach (var value in entry.PossibleValues)
                {
                    var type = value as TypeValue;
                    Debug.Assert(type != null, "Every value read from type storage is a type");
                    types.Add(type);
                }

                return types;
            }
            else
            {
                return new List<TypeValue>(0);
            }
        }

        #endregion

        #region Snapshot memory manipulation

        private void assign(VariableName targetVar, MemoryEntry entry, VariableKind kind = VariableKind.Local)
        {
            var info = getOrCreateInfo(targetVar, kind);
            assign(info, entry);
        }

        private void assign(VariableKey key, MemoryEntry entry)
        {
            var info = getOrCreateInfo(key);
            assign(info, entry);
        }

        private void assign(VariableInfo info, MemoryEntry entry)
        {
            var references = info.References;

            switch (references.Count)
            {
                case 0:
                    //reserve new virtual reference
                    var allocatedReference = new VirtualReference(info.Name, info.Kind, CurrentContextStamp);
                    info.References.Add(allocatedReference);

                    setEntry(allocatedReference, entry);
                    break;
                case 1:
                    setEntry(references[0], entry);
                    break;

                default:
                    weakUpdate(references, entry);
                    break;
            }
        }

        private void assign(List<VirtualReference> references, MemoryEntry entry)
        {
            switch (references.Count)
            {
                case 0:
                    //there is no place where to set the value
                    break;
                case 1:
                    setEntry(references[0], entry);
                    break;

                default:
                    weakUpdate(references, entry);
                    break;
            }
        }

        private MemoryEntry readValue(VariableKey key)
        {
            var info = getOrCreateInfo(key);

            return readValue(info);
        }

        private MemoryEntry readValue(VariableInfo info)
        {
            if (info == null)
            {
                ReportMemoryEntryCreation();
                return new MemoryEntry(UndefinedValue);
            }

            return resolveReferences(info.References);
        }

        private MemoryEntry readValue(VariableName sourceVar, VariableKind kind)
        {
            var info = getInfo(sourceVar, kind);

            return readValue(info);
        }

        void assignAlias(VariableInfo target, IEnumerable<ReferenceAliasEntry> aliases)
        {
            var references = new HashSet<VirtualReference>();
            foreach (var alias in aliases)
            {
                var contextVariable = getOrCreateInfo(alias.Key);

                if (contextVariable.References.Count == 0)
                {
                    var implicitRef = new VirtualReference(contextVariable, alias.ContextStamp);
                    contextVariable.References.Add(implicitRef);
                }

                references.UnionWith(contextVariable.References);
            }

            target.References.Clear();
            target.References.AddRange(references);
        }

        private void weakUpdate(List<VirtualReference> references, MemoryEntry update)
        {
            foreach (var reference in references)
            {
                var entry = getEntry(reference);
                ReportMemoryEntryMerge();
                var updated = MemoryEntry.Merge(entry, update);

                setEntry(reference, updated);
            }
        }

        private MemoryEntry resolveReferences(List<VirtualReference> references)
        {
            switch (references.Count)
            {
                case 0:
                    ReportMemoryEntryCreation();
                    return new MemoryEntry(UndefinedValue);
                case 1:
                    return getEntry(references[0]);
                default:
                    var values = new HashSet<Value>();
                    foreach (var reference in references)
                    {
                        var entry = getEntry(reference);
                        values.UnionWith(entry.PossibleValues);
                    }

                    return new MemoryEntry(values);
            }

        }

        private IEnumerable<Value> resolveValuesFrom(VirtualReference reference)
        {
            var entry = getEntry(reference);
            if (entry == null)
            {
                return new Value[0];
            }

            return entry.PossibleValues;
        }

        private VariableInfo getOrCreateInfo(VariableName name, VariableKind kind)
        {
            //convert kind according to current scope
            kind = repairKind(kind, IsGlobalScope);

            VariableInfo result;
            var storage = getVariableContainer(kind);

            if (!storage.TryGetValue(name, out result))
            {
                result = new VariableInfo(name, kind);
                storage.SetValue(name, result);
            }

            return result;
        }

        private MemoryEntry getEntry(VirtualReference reference)
        {
            return _data.GetEntry(reference);
        }

        private void setEntry(VirtualReference reference, MemoryEntry entry)
        {
            if (entry == null)
            {
                throw new NotSupportedException("Entry cannot be null");
            }

            _data.SetEntry(reference, entry);
        }

        private VariableInfo getInfo(VariableName name, VariableKind kind = VariableKind.Local)
        {
            ReportSimpleHashSearch();

            var storage = getVariableContainer(kind);

            VariableInfo info;
            if (storage.TryGetValue(name, out info))
            {
                return info;
            }

            return null;
        }

        private bool hasSameReferences(VariableInfo info, List<VirtualReference> references)
        {
            if (info.References.Count != references.Count)
            {
                return false;
            }

            foreach (var reference in references)
            {
                if (!info.References.Contains(reference))
                {
                    return false;
                }
            }

            return true;
        }


        private void fetchFromGlobalAll(params VariableName[] variables)
        {
            fetchFromGlobal(variables);
        }




        #endregion

        #region Object operations


        protected override void initializeObject(ObjectValue createdObject, TypeValue type)
        {
            var info = getObjectInfoStorage(createdObject);
            //TODO set info
            ReportMemoryEntryCreation();
            assign(info, new MemoryEntry(type));
        }

        protected override TypeValue objectType(ObjectValue objectValue)
        {
            var info = getObjectInfoStorage(objectValue);
            var objectInfo = readValue(info);

            TypeValue type = null;
            foreach (var typeInfo in objectInfo.PossibleValues)
            {
                if (!(typeInfo is TypeValue))
                    continue;

                if (type != null)
                    Debug.Fail("Object cannot have more than one type");

                type = typeInfo as TypeValue;
            }

            if (type == null)
                Debug.Fail("Object has to have exactly one type");

            return type;
        }

        protected override IEnumerable<FunctionValue> resolveMethod(ObjectValue objectValue, QualifiedName methodName)
        {
            var type = objectType(objectValue);
            var methods = TypeMethodResolver.ResolveMethods(type, this);

            foreach (var method in methods)
            {
                if (method.Name.Value == methodName.Name.Value)
                {
                    yield return method;
                }
            }
        }

        #endregion

        #region Array operations

        protected override void initializeArray(AssociativeArray createdArray)
        {
            //TODO initialize array
            var info = getArrayInfoStorage(createdArray);
            ReportMemoryEntryCreation();
            assign(info, new MemoryEntry());
        }

        private AssociativeArray deepArrayCopy(AssociativeArray array, Dictionary<AssociativeArray, AssociativeArray> closure = null)
        {
            AssociativeArray copy;

            if (closure == null)
            {
                closure = new Dictionary<AssociativeArray, AssociativeArray>();
            }
            else
            {
                if (closure.TryGetValue(array, out copy))
                {
                    //array is already copied
                    return copy;
                }
            }

            copy = CreateArray();
            closure[array] = copy;

            var indexes = getArrayIndexes(array);
            foreach (var index in indexes)
            {
                var indexKey = getIndex(array, index);
                var targetIndexKey = getIndex(copy, index);

                var value = readValue(indexKey);
                writeDirect(targetIndexKey, value);
            }

            return copy;
        }

        /// <summary>
        /// TODO: This is very inefficient - store indexes in meta info
        /// </summary>
        /// <param name="iteratedArray"></param>
        /// <returns></returns>
        private IEnumerable<string> getArrayIndexes(AssociativeArray iteratedArray)
        {
            var arrayPrefix = string.Format("$arr{0}[", iteratedArray.UID);
            var indexes = new List<string>();
            foreach (var varName in _meta.VariableIdentifiers)
            {
                if (!varName.StartsWith(arrayPrefix))
                {
                    continue;
                }

                var indexIdentifier = varName.Substring(arrayPrefix.Length, varName.Length - 1 - arrayPrefix.Length);

                indexes.Add(indexIdentifier);
            }

            return indexes;
        }

        #endregion

        #region Special storages

        private VariableKey getFunctionStorage(string functionName)
        {
            return getMeta("$function-" + functionName);
        }

        private VariableKey getTypeStorage(string typeName)
        {
            return getMeta("$type:" + typeName);
        }

        private VariableKey getFieldStorage(Value obj, ContainerIndex field)
        {
            var name = string.Format("$obj{0}->{1}", obj.UID, field.Identifier);
            return getMeta(name);
        }

        private VariableKey getObjectInfoStorage(ObjectValue obj)
        {
            var name = string.Format("$obj{0}#info", obj.UID);
            return getMeta(name);
        }

        private VariableKey getIndexStorage(Value arr, ContainerIndex index)
        {
            return getIndex(arr, index.Identifier);
        }

        private VariableKey getIndex(Value arr, string indexIdentifier)
        {
            var name = string.Format("$arr{0}[{1}]", arr.UID, indexIdentifier);
            return getMeta(name);
        }

        private VariableKey getThisObjectStorage()
        {
            //TODO this should be control (but it has different scope propagation)
            return new VariableKey(VariableKind.Local, new VariableName("this"), CurrentContextStamp);
        }

        private VariableKey getArrayInfoStorage(AssociativeArray arr)
        {
            var name = string.Format("$arr{0}#info", arr.UID);
            return getMeta(name);
        }

        private VariableKey getMeta(string variableName)
        {
            return new VariableKey(VariableKind.Meta, new VariableName(variableName), CurrentContextStamp);
        }

        #endregion

        #region Private utilities

        private VariableKey getOrCreateKey(VariableName name, VariableKind kind = VariableKind.Local)
        {
            var info = getInfo(name, kind);
            if (info != null)
                return new VariableKey(info.Kind, info.Name, CurrentContextStamp);

            return new VariableKey(kind, name, CurrentContextStamp);
        }


        /// <summary>
        /// Repair kind according to global scope and forceGlobal directives
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="forceGlobal"></param>
        /// <returns></returns>
        private VariableKind repairKind(VariableKind kind, bool forceGlobal)
        {
            if (kind == VariableKind.GlobalControl)
                return kind;

            if (kind == VariableKind.Meta)
                return kind;

            if (forceGlobal || IsGlobalScope)
                return VariableKind.Global;

            return kind;
        }

        private VariableContainer getVariableContainer(VariableKind kind)
        {
            kind = repairKind(kind, IsGlobalScope);
            switch (kind)
            {
                case VariableKind.Global:
                    return _globals;
                case VariableKind.Local:
                    return _locals;
                case VariableKind.GlobalControl:
                    return _globalControls;
                case VariableKind.LocalControl:
                    return _localControls;
                case VariableKind.Meta:
                    return _meta;
                default:
                    throw new NotSupportedException("Variable kind");
            }
        }

        private VariableInfo getOrCreateInfo(VariableKey key)
        {
            var variable = getOrCreateInfo(key.Name, key.Kind);
            return variable;
        }

        private VariableInfo getInfo(VariableKey key)
        {
            var variable = getInfo(key.Name, key.Kind);

            return variable;
        }

        #endregion

        #region Building string representation of snapshot

        public override string ToString()
        {
            return Representation;
        }

        public string Representation
        {
            get
            {
                return GetRepresentation();
            }
        }
        public string GetRepresentation()
        {
            var result = new StringBuilder();

            if (!IsGlobalScope)
            {
                result.AppendLine("===LOCALS===");
                result.AppendLine(_locals.ToString());
            }
            result.AppendLine("===GLOBALS===");
            result.AppendLine(_globals.ToString());

            result.AppendLine("===GLOBAL CONTROLS===");
            result.AppendLine(_globalControls.ToString());

            result.AppendLine("===LOCAL CONTROLS===");
            result.AppendLine(_localControls.ToString());

            result.AppendLine("\n===META===");
            result.AppendLine(_meta.ToString());

            return result.ToString();
        }



        private void fillWithVariables(StringBuilder result, VariableKind kind)
        {
            var variables = getVariableContainer(kind);

            foreach (var variableInfo in variables.VariableInfos)
            {

                if (kind != VariableKind.Global && variableInfo.IsGlobal)
                {
                    result.AppendFormat("{0}: #Fetched from global scope", variableInfo);
                    result.AppendLine();
                }
                else
                {
                    result.AppendFormat("{0}: {{", variableInfo);


                    var entry = readValue(variableInfo);
                    foreach (var value in entry.PossibleValues)
                    {
                        result.AppendFormat("'{0}', ", value);
                    }

                    result.Length -= 2;
                    result.AppendLine("}");
                }
            }
        }

        #endregion


        //========================OLD API IMPLEMENTATION=======================
        #region OLD API related methods (will be removed after backcompatibility won't be needed)


        protected override AliasValue createAlias(VariableName sourceVar)
        {
            return createAlias(sourceVar);
        }

        private AliasValue createAlias(VariableKey key)
        {
            var info = getOrCreateInfo(key);
            if (info.References.Count == 0)
            {
                assign(info, new MemoryEntry(UndefinedValue));
            }
            return new ReferenceAlias(info.References);
        }

        private AliasValue createAlias(VariableName sourceVar, VariableKind kind = VariableKind.Local)
        {
            var info = getInfo(sourceVar, kind);
            if (info == null)
            {
                //force reference creation
                ReportMemoryEntryCreation();
                assign(sourceVar, new MemoryEntry(UndefinedValue), kind);
                info = getInfo(sourceVar, kind);
            }

            return new ReferenceAlias(info.References);
        }

        protected override AliasValue createIndexAlias(AssociativeArray array, ContainerIndex index)
        {
            var storage = getIndexStorage(array, index);

            return createAlias(storage);
        }

        protected override AliasValue createFieldAlias(ObjectValue objectValue, ContainerIndex field)
        {
            var storage = getFieldStorage(objectValue, field);

            return createAlias(storage);
        }

        protected override void assign(VariableName targetVar, MemoryEntry entry)
        {
            assign(targetVar, entry, VariableKind.Local);
        }

        protected override MemoryEntry readValue(VariableName sourceVar)
        {
            return readValue(sourceVar, VariableKind.Local);
        }

        protected override bool tryReadValue(VariableName sourceVar, out MemoryEntry entry, bool forceGlobalContext)
        {
            var kind = repairKind(VariableKind.Local, forceGlobalContext);
            return tryReadValue(new VariableKey(kind, sourceVar, CurrentContextStamp), out entry);
        }

        private bool tryReadValue(VariableKey key, out MemoryEntry entry)
        {
            var info = getInfo(key);

            if (info == null)
            {
                ReportMemoryEntryCreation();
                entry = new MemoryEntry(UndefinedValue);
                return false;
            }

            entry = resolveReferences(info.References);
            return true;
        }

        protected override void setField(ObjectValue value, ContainerIndex index, MemoryEntry entry)
        {
            var storage = getFieldStorage(value, index);
            assign(storage, entry);
        }

        protected override MemoryEntry getField(ObjectValue value, ContainerIndex index)
        {
            var storage = getFieldStorage(value, index);
            return readValue(storage);
        }

        protected override bool tryGetField(ObjectValue objectValue, ContainerIndex field, out MemoryEntry entry)
        {
            var storage = getFieldStorage(objectValue, field);
            return tryReadValue(storage, out entry);
        }


        protected override IEnumerable<ContainerIndex> iterateObject(ObjectValue iteratedObject)
        {
            // TODO: Add visibility

            var arrayPrefix = string.Format("$obj{0}->", iteratedObject.UID);
            var indexes = new List<ContainerIndex>();
            foreach (var varName in _meta.VariableIdentifiers)
            {
                if (!varName.StartsWith(arrayPrefix))
                {
                    continue;
                }

                var indexIdentifier = varName.Substring(arrayPrefix.Length, varName.Length - arrayPrefix.Length);

                indexes.Add(CreateIndex(indexIdentifier));
            }

            return indexes;
        }

        protected override void setInfo(Value value, params InfoValue[] info)
        {
            var storage = infoStorage(value);

            ReportMemoryEntryCreation();
            assign(storage, new MemoryEntry(info), VariableKind.Global);
        }

        protected override void setInfo(VariableName variable, params InfoValue[] info)
        {
            var storage = infoStorage(variable);

            ReportMemoryEntryCreation();
            assign(storage, new MemoryEntry(info), VariableKind.Global);
        }

        protected override InfoValue[] readInfo(Value value)
        {
            var storage = infoStorage(value);

            return getInfoValues(storage);
        }

        protected override InfoValue[] readInfo(VariableName variable)
        {
            var storage = infoStorage(variable);

            return getInfoValues(storage);
        }

        protected VariableName infoStorage(VariableName variable)
        {
            var storage = string.Format(".info_{0}", variable.Value);
            return new VariableName(storage);
        }

        protected VariableName infoStorage(Value value)
        {
            var storage = string.Format(".value_info-{0}", value.UID);
            return new VariableName(storage);
        }

        private InfoValue[] getInfoValues(VariableName storage)
        {
            MemoryEntry entry;
            if (!tryReadValue(storage, out entry, true))
            {
                return new InfoValue[0];
            }

            var possibleValues = entry.PossibleValues;
            var infoValues = new List<InfoValue>(entry.Count);

            foreach (var value in possibleValues)
            {
                var infoValue = value as InfoValue;
                Debug.Assert(infoValue != null, "Every value read from info storage is an info value");
                infoValues.Add(infoValue);
            }

            return infoValues.ToArray();
        }


        protected override void setIndex(AssociativeArray value, ContainerIndex index, MemoryEntry entry)
        {
            var storage = getIndexStorage(value, index);
            assign(storage, entry);
        }

        protected override MemoryEntry getIndex(AssociativeArray value, ContainerIndex index)
        {
            var storage = getIndexStorage(value, index);
            return readValue(storage);
        }

        protected override bool tryGetIndex(AssociativeArray array, ContainerIndex index, out MemoryEntry entry)
        {
            var storage = getIndexStorage(array, index);
            return tryReadValue(storage, out entry);
        }


        protected override IEnumerable<ContainerIndex> iterateArray(AssociativeArray iteratedArray)
        {
            var arrayPrefix = string.Format("$arr{0}[", iteratedArray.UID);
            var indexes = new List<ContainerIndex>();
            foreach (var varName in _meta.VariableIdentifiers)
            {
                if (!varName.StartsWith(arrayPrefix))
                {
                    continue;
                }

                var indexIdentifier = varName.Substring(arrayPrefix.Length, varName.Length - 1 - arrayPrefix.Length);

                indexes.Add(CreateIndex(indexIdentifier));
            }

            return indexes;
        }

        #endregion


    }
}
