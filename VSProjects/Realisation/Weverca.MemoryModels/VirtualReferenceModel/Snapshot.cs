/*
Copyright (c) 2012-2014 Miroslav Vodolan.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

using Weverca.MemoryModels.VirtualReferenceModel.Memory;
using Weverca.MemoryModels.VirtualReferenceModel.Containers;
using Weverca.MemoryModels.VirtualReferenceModel.SnapshotEntries;

namespace Weverca.MemoryModels.VirtualReferenceModel
{
    /// <summary>
    /// Implementation of the memory snapshot based on virtual reference semantics.
    /// 
    /// This implementation stores description of variables in VariableContainers. The description contains
    /// references for every declared variable in context of current snapshot. References are used as keys into
    /// data containers, where <see cref="MemoryEntry"/> objects are stored.
    /// 
    /// Array indexes and object fields are stored as simple variables in meta variable containers. This results in 
    /// same implementation for reading and writing variables, array fields and indexes (we will call them storages).
    /// 
    /// Reading of storages is done by getting <see cref="MemoryEntry"/> for every storage's reference. They are merged
    /// for reading result.
    /// 
    /// Writing into storages differs according count of references.
    /// 0 - new reference is allocated with strong update
    /// 1 - strong update
    /// 2 - weak update of all references
    /// 
    /// Nowadays there is not support for unknown storages.
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
        private DataContainer _infoData;

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

        /// <summary>
        /// Enhance visibility of <see cref="MemoryAssistant"/>
        /// </summary>
        internal MemoryAssistantBase MemoryAssistant { get { return Assistant; } }

        /// <summary>
        /// Create snapshot of Virtual Reference memory model
        /// </summary>
        public Snapshot()
        {
            _globals = new VariableContainer(VariableKind.Global, this);
            _locals = new VariableContainer(VariableKind.Local, this, _globals);
            _localControls = new VariableContainer(VariableKind.LocalControl, this, _globalControls);
            _globalControls = new VariableContainer(VariableKind.GlobalControl, this);
            _meta = new VariableContainer(VariableKind.Meta, this);

            _data = new DataContainer(this);
            _infoData = new DataContainer(this);
        }

        #region Snapshot manipulation

        /// <inheritdoc />
        protected override void startTransaction()
        {

            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    startMemoryTransaction();
                    break;
                case SnapshotMode.InfoLevel:
                    startInfoTransaction();
                    break;
                default:
                    throw notSupportedMode();
            }
        }

        private void startInfoTransaction()
        {
            _infoData.FlipBuffers();
        }

        private void startMemoryTransaction()
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

        /// <inheritdoc />
        protected override bool commitTransaction(int simplifyLimit)
        {
            return commit(simplifyLimit);
        }

        /// <inheritdoc />
        protected override bool widenAndCommitTransaction(int simplifyLimit)
        {
            _data.WidenWith(Assistant);
            return commit(simplifyLimit);
        }

        private bool commit(int simplifyLimit)
        {
            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    return commitMemory(simplifyLimit);
                case SnapshotMode.InfoLevel:
                    return commitInfo(simplifyLimit);
                default:
                    throw notSupportedMode();
            }
        }

        private bool commitInfo(int simplifyLimit)
        {

            //TODO this rapidly slows down the performance
            _infoData.Simplify(simplifyLimit);

            return _infoData.DifferInCount || _infoData.CheckChange();
        }

        private bool commitMemory(int simplifyLimit)
        {
            //TODO this rapidly slows down the performance
            _data.Simplify(simplifyLimit);


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

        /// <inheritdoc />
        protected override void extendAsCall(SnapshotBase callerContext, MemoryEntry thisObject, MemoryEntry[] arguments)
        {
            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    extendAsCallMemory(callerContext, thisObject, arguments);
                    break;
                case SnapshotMode.InfoLevel:
                    extendAsCallInfo(callerContext, thisObject, arguments);
                    break;
                default:
                    throw notSupportedMode();
            }
        }

        private void extendAsCallInfo(SnapshotBase callerContext, MemoryEntry thisObject, MemoryEntry[] arguments)
        {
            //TODO semantic about this object and arguments ?

            var input = callerContext as Snapshot;
            _infoData.ExtendBy(input._infoData, true);
        }

        private void extendAsCallMemory(SnapshotBase callerContext, MemoryEntry thisObject, MemoryEntry[] arguments)
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

        /// <inheritdoc />
        protected override void extend(ISnapshotReadonly[] inputs)
        {
            //TODO: this is inefficient
            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    extendMemory(inputs);
                    break;
                case SnapshotMode.InfoLevel:
                    extendInfo(inputs);
                    break;
                default:
                    throw notSupportedMode();
            }
        }

        private void extendInfo(ISnapshotReadonly[] inputs)
        {
            _infoData.ClearCurrent();

            var isFirst = true;
            foreach (Snapshot input in inputs)
            {
                //merge info from extending inputs
                _infoData.ExtendBy(input._infoData, isFirst);

                isFirst = false;
            }
        }

        private void extendMemory(ISnapshotReadonly[] inputs)
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

        /// <inheritdoc />
        protected override void mergeWithCallLevel(ProgramPointBase callerPoint, ISnapshotReadonly[] callOutputs)
        {
            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    mergeWithCallLevelMemory(callOutputs);
                    break;
                case SnapshotMode.InfoLevel:
                    mergeWithCallLevelInfo(callOutputs);
                    break;
                default:
                    throw notSupportedMode();
            }
        }

        private void mergeWithCallLevelInfo(ISnapshotReadonly[] callOutput)
        {
            var isFirst = true;
            foreach (Snapshot callInput in callOutput)
            {
                _infoData.ExtendBy(callInput._infoData, isFirst);
                isFirst = false;
            }
        }

        private void mergeWithCallLevelMemory(ISnapshotReadonly[] callOutput)
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

        /// <inheritdoc />
        protected override ReadWriteSnapshotEntryBase getVariable(VariableIdentifier variable, bool forceGlobalContext)
        {
            var kind = repairKind(VariableKind.Local, forceGlobalContext);

            var storages = new List<VariableKeyBase>();
            foreach (var name in variable.PossibleNames)
            {
                var key = getOrCreateKey(name, kind);
                storages.Add(key);
            }

            return new SnapshotStorageEntry(variable, false, storages.ToArray());
        }

        /// <summary>
        /// Get snapshot entry for variable, used for extra info controlling. Control entries may share names with other variables,
        /// indexes or fields. Control entries are not affected by unknown fields, also they cannot be aliased to non-control
        /// entries.
        /// </summary>
        /// <param name="name">Variable determining control entry</param>
        /// <returns>
        /// Created control entry
        /// </returns>
        protected override ReadWriteSnapshotEntryBase getControlVariable(VariableName name)
        {
            return new SnapshotStorageEntry(new VariableIdentifier(name), false, new[] { getOrCreateKey(name, VariableKind.GlobalControl) });
        }

        /// <summary>
        /// Get snapshot entry for variable, used for extra info controlling in local context. Control entries may share names with other variables,
        /// indexes or fields. Control entries are not affected by unknown fields, also they cannot be aliased to non-control
        /// entries.
        /// </summary>
        /// <param name="name">Variable determining control entry</param>
        /// <returns>
        /// Created control entry
        /// </returns>
        protected override ReadWriteSnapshotEntryBase getLocalControlVariable(VariableName name)
        {
            return new SnapshotStorageEntry(new VariableIdentifier(name), false, new[] { getOrCreateKey(name, VariableKind.LocalControl) });
        }

        /// <inheritdoc />
        protected override ReadWriteSnapshotEntryBase createSnapshotEntry(MemoryEntry entry)
        {
            return new SnapshotMemoryEntry(this, entry);
        }

        #endregion

        #region API for Snapshot entries

        private void writeDirect(VariableKeyBase storage, MemoryEntry value)
        {
            var variable = getOrCreateInfo(storage);
            assign(variable, value);
        }


        internal IEnumerable<TypeValue> ResolveObjectTypes(MemoryEntry entry)
        {
            foreach (var value in entry.PossibleValues)
            {
                if (value is ObjectValue)
                    yield return ObjectType(value as ObjectValue);
            }
        }

        internal void Write(VariableKeyBase[] storages, MemoryEntry value, bool forceStrongWrite, bool noArrayCopy)
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
                    var allocatedReference = storage.CreateImplicitReference(this);
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

        internal IEnumerable<ReferenceAliasEntry> Aliases(VariableKeyBase[] storages)
        {
            foreach (var storage in storages)
            {
                yield return new ReferenceAliasEntry(storage);
            }
        }

        internal void SetAliases(VariableKeyBase[] storages, IEnumerable<AliasEntry> aliases)
        {
            foreach (var storage in storages)
            {
                var variable = getOrCreateInfo(storage);

                assignAlias(variable, aliases);
            }
        }

        internal bool IsDefined(VariableKeyBase storage)
        {
            return storage.GetVariable(this) != null;
        }

        internal MemoryEntry ReadValue(VariableKeyBase key, bool resolveVirtualRefs = true)
        {
            var variable = getOrCreateInfo(key);

            if (variable.References.Count == 0)
            {
                //implicit reference creation, because of possible cross call aliases
                variable.References.Add(key.CreateImplicitReference(this));
            }

            var value = readValue(variable, resolveVirtualRefs);
            return value;
        }

        internal IEnumerable<VariableKeyBase> IndexStorages(Value array, MemberIdentifier index)
        {
            var storages = new List<VariableKeyBase>();
            foreach (var indexName in index.PossibleNames)
            {
                //TODO refactor ContainerIndex API
                var containerIndex = CreateIndex(indexName);
                var key = getIndexStorage(array, containerIndex);

                storages.Add(key);
            }

            return storages;
        }

        internal IEnumerable<VariableKeyBase> FieldStorages(Value objectValue, VariableIdentifier field)
        {
            var storages = new List<VariableKeyBase>();
            foreach (var fieldName in field.PossibleNames)
            {
                //TODO refactor ContainerIndex API
                var fieldIndex = CreateIndex(fieldName.Value);
                var key = getFieldStorage(objectValue, fieldIndex);
                storages.Add(key);
            }

            return storages;
        }

        private IEnumerable<VariableIdentifier> iterateFields(ObjectValue iteratedObject)
        {
            // TODO: Add visibility

            var objPrefix = string.Format("$obj{0}->", iteratedObject.UID);
            var fields = new List<VariableIdentifier>();
            foreach (var varName in _meta.VariableIdentifiers)
            {
                //TODO optimize
                if (!varName.StartsWith(objPrefix))
                {
                    continue;
                }

                var fieldIdentifier = varName.Substring(objPrefix.Length, varName.Length - objPrefix.Length);
                fields.Add(new VariableIdentifier(fieldIdentifier));
            }

            return fields;
        }

        internal IEnumerable<VariableIdentifier> IterateFields(MemoryEntry memory)
        {
            foreach (var value in memory.PossibleValues)
            {
                var objectValue = value as ObjectValue;

                if (objectValue != null)
                {
                    var fields = iterateFields(objectValue);
                    foreach (var field in fields)
                    {
                        yield return field;
                    }
                }
                else
                {
                    //only objects can be iterated for fields
                    Assistant.TriedIterateFields(value);
                }
            }
        }


        private IEnumerable<MemberIdentifier> iterateIndexes(AssociativeArray iteratedArray)
        {
            var arrayPrefix = string.Format("$arr{0}[", iteratedArray.UID);
            var indexes = new List<MemberIdentifier>();
            foreach (var varName in _meta.VariableIdentifiers)
            {
                if (!varName.StartsWith(arrayPrefix))
                {
                    continue;
                }

                var indexIdentifier = varName.Substring(arrayPrefix.Length, varName.Length - 1 - arrayPrefix.Length);

                indexes.Add(new MemberIdentifier(indexIdentifier));
            }

            return indexes;
        }


        internal IEnumerable<MemberIdentifier> IterateIndexes(MemoryEntry memory)
        {
            foreach (var value in memory.PossibleValues)
            {
                var arrayValue = value as AssociativeArray;

                if (arrayValue != null)
                {
                    var indexes = iterateIndexes(arrayValue);
                    foreach (var index in indexes)
                    {
                        yield return index;
                    }
                }
                else
                {
                    //only arra can be iterated for indexes
                    Assistant.TriedIterateIndexes(value);
                }
            }
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

        /// <summary>
        /// Fetch variables from global context into current context
        /// </summary>
        /// <param name="variables">Variables that will be fetched</param>
        /// <example>global x,y;</example>
        protected override void fetchFromGlobal(IEnumerable<VariableName> variables)
        {
            foreach (var variable in variables)
            {
                var global = GetOrCreateInfo(variable, VariableKind.Global);
                _locals.Fetch(global);
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<VariableName> getGlobalVariables()
        {
            return _globals.VariableNames;
        }

        /// <inheritdoc />
        protected override void declareGlobal(TypeValue declaration)
        {
            var storage = getTypeStorage(declaration.QualifiedName.Name.LowercaseValue);

            var entry = readValue(storage);

            ReportMemoryEntryCreation();
            assign(storage, new MemoryEntry(declaration));
        }

        /// <inheritdoc />
        protected override void declareGlobal(FunctionValue declaration)
        {
            var storage = getFunctionStorage(declaration.Name.Value);

            ReportMemoryEntryCreation();
            assign(storage, new MemoryEntry(declaration));
        }

        /// <summary>
        /// Resolves all possible functions for given functionName
        /// NOTE:
        /// Multiple declarations for single functionName can happen for example because of branch merging
        /// </summary>
        /// <param name="functionName">Name of resolved function</param>
        /// <returns>
        /// Resolved functions
        /// </returns>
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

        /// <summary>
        /// Resolves all possible types for given typeName
        /// NOTE:
        /// Multiple declarations for single typeName can happen for example because of branch merging
        /// </summary>
        /// <param name="typeName">Name of resolved type</param>
        /// <returns>
        /// Resolved types
        /// </returns>
        protected override IEnumerable<TypeValue> resolveType(QualifiedName typeName)
        {
            var storage = getTypeStorage(typeName.Name.LowercaseValue);

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

        /// <summary>
        /// Resolves the static method.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>Resolved methods</returns>
        protected override IEnumerable<FunctionValue> resolveStaticMethod(TypeValue value, QualifiedName methodName)
        {
            List<FunctionValue> result = new List<FunctionValue>();
            IEnumerable<FunctionValue> objectMethods;
            objectMethods = TypeMethodResolver.ResolveMethods(value, this);

            var resolvedMethods = MemoryAssistant.ResolveMethods(value, methodName, objectMethods);
            result.AddRange(resolvedMethods);

            return result;
        }

        #endregion

        #region Snapshot memory manipulation

        private void assign(VariableName targetVar, MemoryEntry entry, VariableKind kind = VariableKind.Local)
        {
            var info = GetOrCreateInfo(targetVar, kind);
            assign(info, entry);
        }

        private void assign(VariableKeyBase key, MemoryEntry entry)
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

        private MemoryEntry readValue(VariableKeyBase key)
        {
            var info = getOrCreateInfo(key);

            return readValue(info);
        }

        private MemoryEntry readValue(VariableInfo info, bool resolveVirtualRefs = true)
        {
            if (info == null)
            {
                ReportMemoryEntryCreation();
                return new MemoryEntry(UndefinedValue);
            }

            List<VirtualReference> references = info.References;
            if (!resolveVirtualRefs)
            {
                //This is used for ToString support - we dont want to cause side effects
                references = new List<VirtualReference>(from reference in references where !(reference is CallbackReference) select reference);
            }

            return resolveReferences(references);
        }

        private MemoryEntry readValue(VariableName sourceVar, VariableKind kind)
        {
            var info = GetInfo(sourceVar, kind);

            return readValue(info);
        }

        void assignAlias(VariableInfo target, IEnumerable<AliasEntry> aliases)
        {
            var references = new HashSet<VirtualReference>();
            var aliasedValues = new List<Value>();
            foreach (var alias in aliases)
            {
                var referenceAlias = alias as ReferenceAliasEntry;
                if (referenceAlias != null)
                {
                    var contextVariable = getOrCreateInfo(referenceAlias.Key);

                    if (contextVariable.References.Count == 0)
                    {
                        var implicitRef = referenceAlias.Key.CreateImplicitReference(this);
                        contextVariable.References.Add(implicitRef);
                    }

                    references.UnionWith(contextVariable.References);
                }
                else
                {
                    //TODO this is just workaround
                    var snapshotAlias = alias as SnapshotAliasEntry;
                    aliasedValues.AddRange(snapshotAlias.SnapshotEntry.WrappedEntry.PossibleValues);
                }
            }

            target.References.Clear();
            target.References.AddRange(references);

            if (aliasedValues.Count > 0)
            {
                //TODO strong update
                REPORT(Statistic.MemoryEntryCreation);
                assign(target, new MemoryEntry(aliasedValues.ToArray()));
            }
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

        internal VariableInfo GetOrCreateInfo(VariableName name, VariableKind kind)
        {
            //convert kind according to current scope
            kind = repairKind(kind, IsGlobalScope);

            VariableInfo result;
            var storage = getVariableContainer(kind);

            if (!storage.TryGetValue(name, out result))
            {
                result = CreateEmptyVar(name, kind);
            }

            return result;
        }

        internal VariableInfo GetInfo(VariableName name, VariableKind kind = VariableKind.Local)
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

        /// <summary>
        /// Expect repaired kind
        /// </summary>
        /// <param name="name"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        internal VariableInfo CreateEmptyVar(VariableName name, VariableKind kind)
        {
            var storage = getVariableContainer(kind);
            var result = new VariableInfo(name, kind);
            storage.SetValue(name, result);

            return result;
        }

        private MemoryEntry getEntry(VirtualReference reference)
        {
            return reference.GetEntry(this, getDataContainer());

        }

        private void setEntry(VirtualReference reference, MemoryEntry entry)
        {
            if (entry == null)
            {
                throw new NotSupportedException("Entry cannot be null");
            }

            reference.SetEntry(this, getDataContainer(), entry);
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
        
        /// <inheritdoc />
        protected override void initializeObject(ObjectValue createdObject, TypeValue type)
        {
            var info = getObjectInfoStorage(createdObject);
            //TODO set info
            ReportMemoryEntryCreation();
            assign(info, new MemoryEntry(type));
        }

        /// <inheritdoc />
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


        internal IEnumerable<FunctionValue> ResolveMethod(MemoryEntry thisObject, QualifiedName methodName)
        {
            var result = new List<FunctionValue>();
            foreach (var possibleValue in thisObject.PossibleValues)
            {
                var objectValue = possibleValue as ObjectValue;

                TypeValue type;
                IEnumerable<FunctionValue> objectMethods;
                if (objectValue == null)
                {
                    type = null;
                    objectMethods = new FunctionValue[0];
                }
                else
                {
                    type = objectType(objectValue);
                    objectMethods = TypeMethodResolver.ResolveMethods(type, this);
                }

                var resolvedMethods = MemoryAssistant.ResolveMethods(possibleValue, type, methodName, objectMethods);
                result.AddRange(resolvedMethods);
            }

            return result;
        }


        #endregion

        #region Array operations

        /// <inheritdoc />
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

        private VariableKeyBase getFunctionStorage(string functionName)
        {
            return getMeta("$function-" + functionName);
        }

        private VariableKeyBase getTypeStorage(string typeName)
        {
            return getMeta("$type:" + typeName);
        }

        private VariableKeyBase getFieldStorage(Value obj, ContainerIndex field)
        {
            var name = string.Format("$obj{0}->{1}", obj.UID, field.Identifier);
            return getMeta(name);
        }

        private VariableKeyBase getObjectInfoStorage(ObjectValue obj)
        {
            var name = string.Format("$obj{0}#info", obj.UID);
            return getMeta(name);
        }

        private VariableKeyBase getIndexStorage(Value arr, ContainerIndex index)
        {
            return getIndex(arr, index.Identifier);
        }

        private VariableKeyBase getIndex(Value arr, string indexIdentifier)
        {
            var name = string.Format("$arr{0}[{1}]", arr.UID, indexIdentifier);
            return getMeta(name);
        }

        private VariableKeyBase getThisObjectStorage()
        {
            //TODO this should be control (but it has different scope propagation)
            return new VariableKey(VariableKind.Local, new VariableName("this"), CurrentContextStamp);
        }

        private VariableKeyBase getArrayInfoStorage(AssociativeArray arr)
        {
            var name = string.Format("$arr{0}#info", arr.UID);
            return getMeta(name);
        }

        private VariableKeyBase getMeta(string variableName)
        {
            return new VariableKey(VariableKind.Meta, new VariableName(variableName), CurrentContextStamp);
        }

        #endregion

        #region Private utilities

        private Exception notSupportedMode()
        {
            return new NotSupportedException("Current mode: " + CurrentMode);
        }

        private DataContainer getDataContainer()
        {
            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    return _data;
                case SnapshotMode.InfoLevel:
                    return _infoData;
                default:
                    throw notSupportedMode();
            }
        }

        private VariableKeyBase getOrCreateKey(VariableName name, VariableKind kind = VariableKind.Local)
        {
            var info = GetInfo(name, kind);
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

        private VariableInfo getOrCreateInfo(VariableKeyBase key)
        {
            return key.GetOrCreateVariable(this);
        }

        private VariableInfo getInfo(VariableKeyBase key)
        {
            return key.GetVariable(this);
        }

        #endregion

        #region Building string representation of snapshot


        /// <inheritdoc />
        public override string ToString()
        {
            return Representation;
        }

        /// <summary>
        /// String representation of current snapshot
        /// </summary>
        public string Representation
        {
            get
            {
                return GetRepresentation();
            }
        }

        /// <summary>
        /// Create string representation of current snapshot
        /// </summary>
        /// <returns>Created representation</returns>
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



        private bool tryReadValue(VariableName sourceVar, out MemoryEntry entry, bool forceGlobalContext)
        {
            var kind = repairKind(VariableKind.Local, forceGlobalContext);
            return tryReadValue(new VariableKey(kind, sourceVar, CurrentContextStamp), out entry);
        }

        private bool tryReadValue(VariableKeyBase key, out MemoryEntry entry)
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

        #region Info values manipulation

        /// <inheritdoc />
        protected override void setInfo(Value value, params InfoValue[] info)
        {
            var storage = infoStorage(value);

            ReportMemoryEntryCreation();
            assign(storage, new MemoryEntry(info), VariableKind.Global);
        }

        /// <summary>
        /// Set given info for variable
        /// </summary>
        /// <param name="variable">Variable which info is stored</param>
        /// <param name="info">Info stored for variable</param>
        protected override void setInfo(VariableName variable, params InfoValue[] info)
        {
            var storage = infoStorage(variable);

            ReportMemoryEntryCreation();
            assign(storage, new MemoryEntry(info), VariableKind.Global);
        }

        /// <inheritdoc />
        protected override InfoValue[] readInfo(Value value)
        {
            var storage = infoStorage(value);

            return getInfoValues(storage);
        }

        /// <summary>
        /// Read info stored for given variable
        /// </summary>
        /// <param name="variable">Variable which info is read</param>
        /// <returns>
        /// Stored info
        /// </returns>
        protected override InfoValue[] readInfo(VariableName variable)
        {
            var storage = infoStorage(variable);

            return getInfoValues(storage);
        }

        private VariableName infoStorage(VariableName variable)
        {
            var storage = string.Format(".info_{0}", variable.Value);
            return new VariableName(storage);
        }

        private VariableName infoStorage(Value value)
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

        #endregion
    }
}