using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;
using Weverca.Analysis;
using Weverca.Analysis.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel
{

    /// <summary>
    /// Simple (non efficient) implementation as proof of concept - will be heavily optimized, refactored
    /// </summary>
    public class Snapshot : SnapshotBase
    {
        Dictionary<VariableName, VariableInfo> _oldVariables;
        Dictionary<VariableName, VariableInfo> _locals = new Dictionary<VariableName, VariableInfo>();
        Dictionary<VariableName, VariableInfo> _oldGlobals;
        Dictionary<VariableName, VariableInfo> _globals = new Dictionary<VariableName, VariableInfo>();
        Dictionary<VirtualReference, MemoryEntry> _oldData;
        Dictionary<VirtualReference, MemoryEntry> _data = new Dictionary<VirtualReference, MemoryEntry>();

        /// <summary>
        /// Determine that this snapshot is pointed to global scope
        /// </summary>
        bool _isGlobalScope;

        bool _hasSemanticChange;

        protected override void startTransaction()
        {
            _oldData = _data;
            _oldVariables = _locals;
            _oldGlobals = _globals;

            _data = new Dictionary<VirtualReference, MemoryEntry>(_data);
            _locals = new Dictionary<VariableName, VariableInfo>(_locals);
            _globals = new Dictionary<VariableName, VariableInfo>(_globals);


            _isGlobalScope = true;// when not extend as call or from non global snapshot is global
            _hasSemanticChange = false;
        }

        protected override bool commitTransaction()
        {
            if (_hasSemanticChange)
            {
                return true;
            }
            else
            {
                _hasSemanticChange =
                    _data.Count != _oldData.Count ||
                    _locals.Count != _oldVariables.Count ||
                    _globals.Count != _oldGlobals.Count
                    ;

                if (_hasSemanticChange)
                {
                    //evident change
                    return true;
                }

                //check variables according to old ones
                if (checkChange(_oldVariables, _locals) || checkChange(_oldGlobals, _globals))
                {
                    return true;
                }

                foreach (var oldData in _oldData)
                {
                    ReportSimpleHashSearch();
                    MemoryEntry currEntry;
                    if (!_data.TryGetValue(oldData.Key, out currEntry))
                    {
                        //differ in presence of some reference
                        return true;
                    }

                    ReportMemoryEntryComparison();
                    if (!currEntry.Equals(oldData.Value))
                    {
                        //differ in stored data
                        return true;
                    }
                }
            }

            return false;
        }


        private bool checkChange(Dictionary<VariableName, VariableInfo> oldVariables, Dictionary<VariableName, VariableInfo> variables)
        {
            foreach (var oldVar in oldVariables)
            {
                ReportSimpleHashSearch();
                VariableInfo currVar;
                if (!variables.TryGetValue(oldVar.Key, out currVar))
                {
                    //differ in some variable presence
                    return true;
                }

                if (!currVar.Equals(oldVar.Value))
                {
                    //differ in variable definition
                    return true;
                }
            }
            return false;
        }

        protected override AliasValue createAlias(VariableName sourceVar)
        {
            var info = getInfo(sourceVar);
            return new ReferenceAlias(info.References);
        }

        protected override void assign(VariableName targetVar, MemoryEntry entry)
        {
            assign(targetVar, entry, false);
        }

        protected void assign(VariableName targetVar, MemoryEntry entry, bool asGlobal = false)
        {
            var info = getOrCreate(targetVar, asGlobal);
            var references = info.References;

            switch (references.Count)
            {
                case 0:
                    //reserve new virtual reference
                    var allocatedReference = new VirtualReference(targetVar, info.IsGlobal);
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


        protected override void assignAlias(VariableName targetVar, AliasValue alias)
        {
            var info = getOrCreate(targetVar);
            var refAlias = alias as ReferenceAlias;

            if (!hasSameReferences(info, refAlias.References))
            {
                reportSemanticChange();
                info.References.Clear();
                info.References.AddRange(refAlias.References);
            }
        }


        protected override void extendAsCall(SnapshotBase callerContext, MemoryEntry thisObject, MemoryEntry[] arguments)
        {
            //called context cannot be global scope
            _isGlobalScope = false;


            var input = callerContext as Snapshot;
            extendVariables(input._globals, _globals, false);
            extendData(input,true);

            if (thisObject != null)
            {
                assign(thisObjectStorage(), thisObject);
            }
        }


        /// <summary>
        /// TODO this implementation is inefficient
        /// </summary>
        /// <param name="inputs"></param>
        protected override void extend(ISnapshotReadonly[] inputs)
        {
            _data = new Dictionary<VirtualReference, MemoryEntry>();
            _locals = new Dictionary<VariableName, VariableInfo>();

            var isFirst = true;
            foreach (Snapshot input in inputs)
            {
                //merge info from extending inputs
                extendVariables(input._globals, _globals, false);
                extendVariables(input._locals, _locals, true);
                extendData(input,isFirst);

                _isGlobalScope &= input._isGlobalScope;
                isFirst = false;
            }
        }


        protected override void mergeWithCallLevel(ISnapshotReadonly[] callOutput)
        {
            var isFirst = true;
            foreach (Snapshot callInput in callOutput)
            {
                //Local variables are not extended
                extendVariables(callInput._globals, _globals, false);
                extendData(callInput, isFirst);
                isFirst = false;
            }
        }

        private void extendData(Snapshot input, bool directExtend)
        {
            foreach (var dataPair in input._data)
            {
                MemoryEntry oldEntry;
                ReportSimpleHashSearch();

                if (!_data.TryGetValue(dataPair.Key, out oldEntry))
                {
                    //copy reference, because its immutable
                    ReportSimpleHashAssign();
                    _data[dataPair.Key] = dataPair.Value;
                }
                else
                {
                    ReportMemoryEntryComparison();
                    if (!dataPair.Value.Equals(oldEntry))
                    {
                        //merge two memory entries
                        ReportMemoryEntryMerge();
                        if (directExtend)
                        {
                            _data[dataPair.Key] = dataPair.Value;
                        }
                        else
                        {
                            _data[dataPair.Key] = MemoryEntry.Merge(oldEntry, dataPair.Value);
                        }
                    }
                }
            }
        }

        private void extendVariables(Dictionary<VariableName, VariableInfo> inputVariables, Dictionary<VariableName, VariableInfo> variables, bool canFetchFromGlobals)
        {
            foreach (var varPair in inputVariables)
            {
                VariableInfo oldVar;
                ReportSimpleHashSearch();
                if (!variables.TryGetValue(varPair.Key, out oldVar))
                {
                    //copy variable info, so we can process changes on it
                    ReportSimpleHashAssign();
                    if (canFetchFromGlobals && varPair.Value.IsGlobal)
                    {
                        //fetch from globals
                        variables[varPair.Key] = _globals[varPair.Key];
                    }
                    else
                    {
                        variables[varPair.Key] = varPair.Value.Clone();
                    }
                }
                else
                {
                    //merge variable references
                    foreach (var reference in varPair.Value.References)
                    {
                        if (!oldVar.References.Contains(reference))
                        {
                            oldVar.References.Add(reference);
                        }
                    }
                }
            }
        }

        protected override MemoryEntry readValue(VariableName sourceVar)
        {
            return readValue(sourceVar, false);
        }

        protected MemoryEntry readValue(VariableName sourceVar, bool asGlobal = false)
        {
            var info = getInfo(sourceVar, asGlobal);

            if (info == null)
            {
                ReportMemoryEntryCreation();
                return new MemoryEntry(UndefinedValue);
            }

            return resolveReferences(info.References);
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
                    var entries = from reference in references select getEntry(reference);

                    return MemoryEntry.Merge(entries);

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

        private VariableInfo getOrCreate(VariableName name, bool asGlobal = false)
        {
            VariableInfo result;

            var storage = getVariableStorage(asGlobal);

            ReportSimpleHashSearch();
            if (!storage.TryGetValue(name, out result))
            {
                storage[name] = result = new VariableInfo(asGlobal || _isGlobalScope);
                ReportSimpleHashAssign();
            }

            return result;
        }

        private void fetch(VariableName name, VariableInfo info)
        {
            ReportSimpleHashAssign();
            _locals[name] = info;
        }


        private MemoryEntry getEntry(VirtualReference reference)
        {
            MemoryEntry entry;
            ReportSimpleHashSearch();
            _data.TryGetValue(reference, out entry);

            return entry;
        }

        private void setEntry(VirtualReference reference, MemoryEntry entry)
        {
            if (entry == null)
            {
                throw new NotSupportedException("Entry cannot be null");
            }
            ReportSimpleHashAssign();
            _data[reference] = entry;
        }

        private VariableInfo getInfo(VariableName name, bool asGlobal = false)
        {
            ReportSimpleHashSearch();

            var storage = getVariableStorage(asGlobal);

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
                    return false;
            }

            return true;
        }

        private void reportSemanticChange()
        {
            if (_hasSemanticChange)
                return;

            _hasSemanticChange = true;
        }

        protected override void fetchFromGlobal(IEnumerable<VariableName> variables)
        {
            foreach (var variable in variables)
            {
                var global = getOrCreate(variable, true);
                fetch(variable, global);
            }
        }

        private void fetchFromGlobalAll(params VariableName[] variables)
        {
            fetchFromGlobal(variables);
        }

        protected override IEnumerable<VariableName> getGlobalVariables()
        {
            return _globals.Keys;
        }

        private VariableName functionStorage(string functionName)
        {
            return new VariableName("$function-" + functionName);
        }

        protected override void declareGlobal(FunctionValue function)
        {
            var storage = functionStorage(function.Name.Value);

            ReportMemoryEntryCreation();
            assign(storage, new MemoryEntry(function), true);
        }
        protected override IEnumerable<FunctionValue> resolveFunction(QualifiedName functionName)
        {
            var storage = functionStorage(functionName.Name.Value);
            var entry = readValue(storage, true);

            return from FunctionValue function in entry.PossibleValues select function;
        }

        protected VariableName typeStorage(string typeName)
        {
            return new VariableName("$type:" + typeName);
        }

        protected override void declareGlobal(TypeValue declaration)
        {
            var storage = typeStorage(declaration.QualifiedName.Name.Value);

            var entry = readValue(storage);

            ReportMemoryEntryCreation();
            assign(storage, new MemoryEntry(declaration), true);
        }

        protected override IEnumerable<TypeValue> resolveType(QualifiedName typeName)
        {
            var storage = typeStorage(typeName.Name.Value);
            var entry = readValue(storage, true);

            return from TypeValue type in entry.PossibleValues select type;
        }

        #region Object operations
        protected override void setField(ObjectValue value, ContainerIndex index, MemoryEntry entry)
        {
            var storage = getFieldStorage(value, index);
            assign(storage, entry, true);
        }

        protected override void setFieldAlias(ObjectValue value, ContainerIndex index, AliasValue alias)
        {
            var storage = getFieldStorage(value, index);
            assignAlias(storage, alias);
        }

        protected override MemoryEntry getField(ObjectValue value, ContainerIndex index)
        {
            var storage = getFieldStorage(value, index);
            return readValue(storage, true);
        }
        protected override void initializeObject(ObjectValue createdObject, TypeValue type)
        {
            var info = getObjectInfoStorage(createdObject);
            //TODO set info
            ReportMemoryEntryCreation();
            assign(info, new MemoryEntry(type), true);
        }

        protected override IEnumerable<FunctionValue> resolveMethod(ObjectValue objectValue, QualifiedName methodName)
        {
            var info = getObjectInfoStorage(objectValue);
            var objInfo = readValue(info, true);
            if (objInfo.PossibleValues.Count() != 1)
            {
                throw new NotImplementedException();
            }

            foreach (var method in TypeMethodResolver.ResolveMethods(objInfo.PossibleValues.First() as TypeValue, this))
            {
                if (method.Name.Value == methodName.Name.Value)
                {
                    yield return method;
                }
            }
        }

        private VariableName getFieldStorage(ObjectValue obj, ContainerIndex field)
        {
            var name = string.Format("$obj{0}->{1}", obj.UID, field.Identifier);
            return new VariableName(name);
        }

        private VariableName getObjectInfoStorage(ObjectValue obj)
        {
            var name = string.Format("$obj{0}#info", obj.UID);
            return new VariableName(name);
        }
        #endregion

        #region Array operations
        protected override void setIndex(AssociativeArray value, ContainerIndex index, MemoryEntry entry)
        {
            var storage = getIndexStorage(value, index);
            assign(storage, entry, true);
        }

        protected override void setIndexAlias(AssociativeArray value, ContainerIndex index, AliasValue alias)
        {
            var storage = getIndexStorage(value, index);
            assignAlias(storage, alias);
        }

        protected override MemoryEntry getIndex(AssociativeArray value, ContainerIndex index)
        {
            var storage = getIndexStorage(value, index);
            return readValue(storage, true);
        }

        protected override void initializeArray(AssociativeArray createdArray)
        {
            //TODO initialize array
            var info = getArrayInfoStorage(createdArray);
            ReportMemoryEntryCreation();
            assign(info, new MemoryEntry(), true);
        }

        private VariableName getIndexStorage(AssociativeArray arr, ContainerIndex index)
        {
            var name = string.Format("$arr{0}[{1}]", arr.UID, index.Identifier);
            return new VariableName(name);
        }

        private VariableName thisObjectStorage()
        {
            return new VariableName("this");
        }

        private VariableName getArrayInfoStorage(AssociativeArray arr)
        {
            var name = string.Format("$arr{0}#info", arr.UID);
            return new VariableName(name);
        }


        protected override IEnumerable<ContainerIndex> iterateArray(AssociativeArray iteratedArray)
        {
            var arrayPrefix = string.Format("$arr{0}[", iteratedArray.UID);
            var indexes = new List<ContainerIndex>();
            foreach (var variable in _globals.Keys)
            {
                var varName = variable.Value;
                if (!varName.StartsWith(arrayPrefix))
                    continue;

                var indexIdentifier = varName.Substring(arrayPrefix.Length, varName.Length - 1 - arrayPrefix.Length);

                indexes.Add(CreateIndex(indexIdentifier));
            }

            return indexes;
        }
        #endregion



        protected override void setInfo(Value value, params InfoValue[] info)
        {
            var storage = infoStorage(value);

            ReportMemoryEntryCreation();
            assign(storage, new MemoryEntry(info), true);
        }

        protected override void setInfo(VariableName variable, params InfoValue[] info)
        {
            var storage = infoStorage(variable);

            ReportMemoryEntryCreation();
            assign(storage, new MemoryEntry(info), true);
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
            var possibleValues = readValue(storage, true).PossibleValues;

            var info = from value in possibleValues where value is InfoValue select value as InfoValue;

            return info.ToArray();
        }

        protected override IEnumerable<ContainerIndex> iterateObject(ObjectValue iteratedObject)
        {
            throw new NotImplementedException();
        }

        private Dictionary<VariableName, VariableInfo> getVariableStorage(bool asGlobal)
        {
            if (_isGlobalScope)
                return _globals;

            return asGlobal ? _globals : _locals;
        }

        public override string ToString()
        {
            return Representation;
        }

        public string Representation
        {
            get
            {
                var result = new StringBuilder();

                if (!_isGlobalScope)
                {
                    result.AppendLine("===LOCALS===");
                    fillWithVariables(result, false);
                }
                result.AppendLine("\n===GLOBALS===");
                fillWithVariables(result, true);

                return result.ToString();
            }
        }

        private void fillWithVariables(StringBuilder result, bool asGlobal)
        {
            var variables = getVariableStorage(asGlobal);

            foreach (var variable in variables.Keys)
            {
                if (!asGlobal && _locals[variable].IsGlobal)
                {
                    result.AppendFormat("{0}: #Fetched from global scope", variable);
                    result.AppendLine();
                }
                else
                {
                    result.AppendFormat("{0}: {{", variable);

                    var entry = readValue(variable, asGlobal);
                    foreach (var value in entry.PossibleValues)
                    {
                        result.AppendFormat("'{0}', ", value);
                    }

                    result.Length -= 2;
                    result.AppendLine("}");

                }
            }
        }
    }
}
