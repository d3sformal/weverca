using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Determines when adds aliases into collection of indexes.
    /// </summary>
    enum AliasesProcessing
    {
        /// <summary>
        /// Add aliased locations after collecting process so the aliases are processed in the next run
        /// </summary>
        AfterCollecting,

        /// <summary>
        /// Add aliased locations before collecting process su the aliases are processed in this run
        /// </summary>
        BeforeCollecting,

        /// <summary>
        /// Do not add aliases
        /// </summary>
        None
    }

    /// <summary>
    /// Updating collecting algorithm which traverse memory tree by given path thru existing locations
    /// and or when the location is not exist adds new location into memory structure. Algorithm also
    /// creates aliases or implicit objects when it is necessary. Also traverse structure by alias
    /// definitions.
    /// 
    /// Result of the algorithm can be found in both must and may lists for locations to strong
    /// or weak update.
    /// </summary>
    class AssignCollector : IndexCollector, IPathSegmentVisitor
    {
        HashSet<MemoryIndex> mustIndexes = new HashSet<MemoryIndex>();
        HashSet<MemoryIndex> mayIndexes = new HashSet<MemoryIndex>();

        HashSet<MemoryIndex> mustIndexesProcess = new HashSet<MemoryIndex>();
        HashSet<MemoryIndex> mayIndexesProcess = new HashSet<MemoryIndex>();

        HashSet<ValueLocation> mustLocation = new HashSet<ValueLocation>();
        HashSet<ValueLocation> mayLocation = new HashSet<ValueLocation>();

        HashSet<ValueLocation> mustLocationProcess = new HashSet<ValueLocation>();
        HashSet<ValueLocation> mayLocationProcess = new HashSet<ValueLocation>();
        private Snapshot snapshot;

        private CreatorVisitor creatorVisitor;

        /// <summary>
        /// Gets a value indicating whether access path is defined or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is defined; otherwise, <c>false</c>.
        /// </value>
        public override bool IsDefined { get; protected set; }

        /// <summary>
        /// Gets or sets the aliases processing.
        /// </summary>
        /// <value>
        /// The aliases processing.
        /// </value>
        public AliasesProcessing AliasesProcessing { get; set; }

        /// <summary>
        /// Gets the list of must indexes to provide the strong operation.
        /// </summary>
        /// <value>
        /// The must indexes.
        /// </value>
        public override IEnumerable<MemoryIndex> MustIndexes
        {
            get { return mustIndexes; }
        }

        /// <summary>
        /// Gets the list of may indexes to provide the weak operation.
        /// </summary>
        /// <value>
        /// The may indexes.
        /// </value>
        public override IEnumerable<MemoryIndex> MayIndexes
        {
            get { return mayIndexes; }
        }

        /// <summary>
        /// Gets the list of must value location to provide the strong operation.
        /// </summary>
        /// <value>
        /// The must location.
        /// </value>
        public override IEnumerable<ValueLocation> MustLocation
        {
            get { return mustLocation; }
        }

        /// <summary>
        /// Gets the list of may value location to provide the weak operation.
        /// </summary>
        /// <value>
        /// The may locaton.
        /// </value>
        public override IEnumerable<ValueLocation> MayLocaton
        {
            get { return mayLocation; }
        }

        /// <summary>
        /// Gets the number of must indexes.
        /// </summary>
        /// <value>
        /// The must indexes count.
        /// </value>
        public override int MustIndexesCount
        {
            get { return mustIndexes.Count; }
        }

        /// <summary>
        /// Gets the number of may indexes.
        /// </summary>
        /// <value>
        /// The may indexes count.
        /// </value>
        public override int MayIndexesCount
        {
            get { return mayIndexes.Count; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignCollector"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        public AssignCollector(Snapshot snapshot)
        {
            this.snapshot = snapshot;
            creatorVisitor = new CreatorVisitor(snapshot, this);

            AliasesProcessing = AliasesProcessing.AfterCollecting;
        }

        /// <summary>
        /// Pocess the next segment.
        /// </summary>
        /// <param name="segment">The segment.</param>
        public override void Next(PathSegment segment)
        {
            if (AliasesProcessing == CopyMemoryModel.AliasesProcessing.BeforeCollecting)
            {
                addAliasesToIndexes();
            }

            segment.Accept(this);

            if (AliasesProcessing == CopyMemoryModel.AliasesProcessing.AfterCollecting)
            {
                addAliasesToIndexes();
            }

            //To prevent creation of collection on each iteration only the two collection are used and swapped here.
            //swaps collections of must indexes
            HashSet<MemoryIndex> indexSwap = mustIndexes;
            mustIndexes = mustIndexesProcess;
            mustIndexesProcess = indexSwap;
            mustIndexesProcess.Clear();

            //may indexes
            indexSwap = mayIndexes;
            mayIndexes = mayIndexesProcess;
            mayIndexesProcess = indexSwap;
            mayIndexesProcess.Clear();

            //myust locations
            HashSet<ValueLocation> locationSwap = mustLocation;
            mustLocation = mustLocationProcess;
            mustLocationProcess = locationSwap;
            mustLocationProcess.Clear();

            //may locations
            locationSwap = mayLocation;
            mayLocation = mayLocationProcess;
            mayLocationProcess = locationSwap;
            mayLocationProcess.Clear();
        }

        /// <summary>
        /// Adds the aliases to must or may lists of indexes.
        /// </summary>
        private void addAliasesToIndexes()
        {
            HashSet<MemoryIndex> mustAliases = new HashSet<MemoryIndex>();
            HashSet<MemoryIndex> mayAliases = new HashSet<MemoryIndex>();
            foreach (MemoryIndex index in mustIndexesProcess)
            {
                MemoryAlias alias;
                if (snapshot.Structure.TryGetAliases(index, out alias))
                {
                    HashSetTools.AddAll(mustAliases, alias.MustAliasses);

                    foreach (MemoryIndex mayIndex in alias.MayAliasses)
                    {
                        mayAliases.Add(mayIndex);
                    }
                }
            }

            foreach (MemoryIndex index in mustAliases)
            {
                addToMust(index);
            }

            foreach (MemoryIndex index in mayIndexesProcess)
            {
                MemoryAlias alias;
                if (snapshot.Structure.TryGetAliases(index, out alias))
                {
                    HashSetTools.AddAll(mayAliases, alias.MustAliasses);
                    HashSetTools.AddAll(mayAliases, alias.MayAliasses);
                }
            }
            foreach (MemoryIndex index in mayAliases)
            {
                addToMay(index);
            }
        }

        /// <summary>
        /// Visits the variable to traverse memory tree from variable root.
        /// </summary>
        /// <param name="segment">The segment.</param>
        public void VisitVariable(VariablePathSegment segment)
        {
            switch (Global)
            {
                case GlobalContext.LocalOnly:
                    processSegment(segment, snapshot.Structure.Variables[CallLevel]);
                    break;
                case GlobalContext.GlobalOnly:
                    processSegment(segment, snapshot.Structure.Variables.Global);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Visits the control variable to traverse memory tree from control variable root.
        /// </summary>
        /// <param name="segment">The segment.</param>
        public void VisitControl(ControlPathSegment segment)
        {
            switch (Global)
            {
                case GlobalContext.LocalOnly:
                    processSegment(segment, snapshot.Structure.ContolVariables[CallLevel]);
                    break;
                case GlobalContext.GlobalOnly:
                    processSegment(segment, snapshot.Structure.ContolVariables.Global);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Visits the temporary variable to traverse memory tree from temporary variable root.
        /// </summary>
        /// <param name="temporaryPathSegment">The temporary path segment.</param>
        public void VisitTemporary(TemporaryPathSegment temporaryPathSegment)
        {
            addToMust(temporaryPathSegment.TemporaryIndex);
        }

        /// <summary>
        /// Visits the field to continue traversing memory tree by object field.
        /// </summary>
        /// <param name="segment">The segment.</param>
        public void VisitField(FieldPathSegment segment)
        {
            FieldLocationVisitor visitor = new FieldLocationVisitor(segment, snapshot.MemoryAssistant, mustLocationProcess, mayLocationProcess);

            visitor.IsMust = true;
            foreach (MemoryIndex index in mustIndexes)
            {
                processField(segment, index, visitor, true);
            }
            foreach (ValueLocation location in mustLocation)
            {
                location.Accept(visitor);
            }

            visitor.IsMust = false;
            foreach (MemoryIndex index in MayIndexes)
            {
                processField(segment, index, visitor, false);
            }
            foreach (ValueLocation location in mayLocation)
            {
                location.Accept(visitor);
            }
        }

        /// <summary>
        /// Visits the index to continue traversing memory tree by array index.
        /// </summary>
        /// <param name="segment">The segment.</param>
        public void VisitIndex(IndexPathSegment segment)
        {
            IndexLocationVisitor visitor = new IndexLocationVisitor(segment, snapshot.MemoryAssistant, mustLocationProcess, mayLocationProcess);

            visitor.IsMust = true;
            foreach (MemoryIndex index in mustIndexes)
            {
                processIndex(segment, index, visitor, true);
            }
            foreach (ValueLocation location in mustLocation)
            {
                location.Accept(visitor);
            }

            visitor.IsMust = false;
            foreach (MemoryIndex index in mayIndexes)
            {
                processIndex(segment, index, visitor, false);
            }
            foreach (ValueLocation location in mayLocation)
            {
                location.Accept(visitor);
            }
        }

        /// <summary>
        /// Processes the field - traverse thru all containing objects. When there is possibility undefined
        /// value in location new object is created.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="visitor">The visitor to process scalar values.</param>
        /// <param name="isMust">if set to <c>true</c> [is must].</param>
        private void processField(PathSegment segment, MemoryIndex parentIndex, FieldLocationVisitor visitor, bool isMust)
        {
            bool processOtherValues = false;
            MemoryEntry entry;
            if (snapshot.Structure.TryGetMemoryEntry(parentIndex, out entry))
            {
                ObjectValueContainer objects = snapshot.Structure.GetObjects(parentIndex);
                if (objects.Count > 0)
                {
                    processOtherValues = entry.Count > objects.Count;
                }
                else if (entry.Count > 0)
                {
                    processOtherValues = true;
                }
                else
                {
                    entry = snapshot.Data.EmptyEntry;
                    processOtherValues = true;
                }
            }
            else
            {
                entry = snapshot.Data.EmptyEntry;
                processOtherValues = true;
            }

            if (processOtherValues)
            {
                if (entry.Count > 1)
                {
                    isMust = false;
                }

                visitor.ProcessValues(parentIndex, entry.PossibleValues, isMust);
                ReadFieldVisitor valueVisitor = visitor.LastValueVisitor;
                bool removeUndefined = isMust;

                if (valueVisitor.ContainsDefinedValue || valueVisitor.ContainsAnyValue)
                {
                    isMust = false;
                }

                if (valueVisitor.ContainsUndefinedValue && snapshot.CurrentMode == SnapshotMode.MemoryLevel)
                {
                    ObjectValue objectValue = snapshot.CreateObject(parentIndex, isMust, removeUndefined);
                }
            }

            ObjectValueContainer objectValues = snapshot.Structure.GetObjects(parentIndex);
            if (objectValues.Count == 1 && snapshot.HasMustReference(parentIndex))
            {
                ObjectDescriptor descriptor = snapshot.Structure.GetDescriptor(objectValues.First());
                creatorVisitor.ObjectValue = objectValues.First();
                processSegment(segment, descriptor, isMust);
            }
            else
            {
                foreach (ObjectValue value in objectValues)
                {
                    ObjectDescriptor descriptor = snapshot.Structure.GetDescriptor(value);
                    creatorVisitor.ObjectValue = value;
                    processSegment(segment, descriptor, false);
                }
            }
        }

        /// <summary>
        /// Processes the index - traverse thru array or sets path to undefined. When memory location contains
        /// no array and undefined value new array is created.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="visitor">The visitor to process scalar values.</param>
        /// <param name="isMust">if set to <c>true</c> [is must].</param>
        private void processIndex(PathSegment segment, MemoryIndex parentIndex, IndexLocationVisitor visitor, bool isMust)
        {
            AssociativeArray arrayValue = null;
            bool processOtherValues = false;
            MemoryEntry entry;
            if (snapshot.Data.TryGetMemoryEntry(parentIndex, out entry))
            {
                if (snapshot.Structure.TryGetArray(parentIndex, out arrayValue))
                {
                    processOtherValues = entry.Count > 1;
                }
                else if (entry.Count > 0)
                {
                    processOtherValues = true;
                }
                else
                {
                    entry = snapshot.Data.EmptyEntry;
                    processOtherValues = true;
                }
            }
            else
            {
                entry = snapshot.Data.EmptyEntry;
                processOtherValues = true;
                snapshot.Structure.TryGetArray(parentIndex, out arrayValue);
            }

            if (processOtherValues)
            {
                visitor.ProcessValues(parentIndex, entry.PossibleValues, isMust);
                ReadIndexVisitor valueVisitor = visitor.LastValueVisitor;
                bool removeUndefined = isMust;

                if (valueVisitor.ContainsDefinedValue || valueVisitor.ContainsAnyValue)
                {
                    isMust = false;
                }

                if (valueVisitor.ContainsUndefinedValue)
                {
                    if (arrayValue == null)
                    {
                        arrayValue = snapshot.CreateArray(parentIndex, isMust, removeUndefined);
                    }
                    else if (removeUndefined)
                    {
                        snapshot.Structure.Data.RemoveUndefined(parentIndex);
                    }
                }
            }

            if (arrayValue != null)
            {
                ArrayDescriptor descriptor = snapshot.Structure.GetDescriptor(arrayValue);
                creatorVisitor.ArrayValue = arrayValue;
                processSegment(segment, descriptor, isMust);
            }
        }

        /// <summary>
        /// Cotinues traversing using specified segment in given index container.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <param name="container">The container.</param>
        /// <param name="isMust">if set to <c>true</c> [is must].</param>
        private void processSegment(PathSegment segment, ReadonlyIndexContainer container, bool isMust = true)
        {
            if (segment.IsAny)
            {
                mayIndexesProcess.Add(container.UnknownIndex);
                addToMay(container.UnknownIndex);

                foreach (var index in container.Indexes)
                {
                    addToMay(index.Value);
                }
            }
            else if (segment.Names.Count == 1)
            {
                MemoryIndex processIndex;
                if (!container.Indexes.TryGetValue(segment.Names[0], out processIndex))
                {
                    creatorVisitor.Name = segment.Names[0];
                    creatorVisitor.IsMust = isMust;
                    segment.Accept(creatorVisitor);
                    processIndex = creatorVisitor.CreatedIndex;
                }

                if (isMust)
                {
                    addToMust(processIndex);
                }
                else
                {
                    addToMay(processIndex);
                }
            }
            else
            {
                creatorVisitor.IsMust = false;

                foreach (String name in segment.Names)
                {
                    MemoryIndex processIndex;
                    if (!container.Indexes.TryGetValue(name, out processIndex))
                    {
                        creatorVisitor.Name = name;
                        segment.Accept(creatorVisitor);
                        processIndex = creatorVisitor.CreatedIndex;
                    }
                    addToMay(processIndex);
                }
            }
        }

        /// <summary>
        /// Adds the index into must collection.
        /// </summary>
        /// <param name="index">The index.</param>
        private void addToMust(MemoryIndex index)
        {
            if (!index.ContainsPrefix(mustIndexesProcess))
            {
                index.RemoveIndexesWithPrefix(mustIndexesProcess);
                index.RemoveIndexesWithPrefix(mayIndexes);

                mustIndexesProcess.Add(index);
            }
        }

        /// <summary>
        /// Adds the index into may collection.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>True whether index in not in the collection of must indexes.</returns>
        private bool addToMay(MemoryIndex index)
        {
            if (!mustIndexesProcess.Contains(index) && !index.ContainsPrefix(mustIndexesProcess))
            {
                mayIndexesProcess.Add(index);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sets all indexes to must.
        /// </summary>
        internal void SetAllToMust()
        {
            HashSetTools.AddAll(mustIndexes, mayIndexes);
            mayIndexes.Clear();
        }
        
        /// <summary>
        /// Segment visitor to get the collection for newly created index in memory colection 
        /// (new variable, control variable, index or field).
        /// </summary>
        private class CreatorVisitor : IPathSegmentVisitor
        {
            private Snapshot snapshot;
            private IndexCollector collector;

            /// <summary>
            /// Gets or sets the new index.
            /// </summary>
            /// <value>
            /// The new index.
            /// </value>
            public MemoryIndex CreatedIndex { get; private set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [is must].
            /// </summary>
            /// <value>
            ///   <c>true</c> if is must; otherwise, <c>false</c>.
            /// </value>
            public bool IsMust { get; set; }

            /// <summary>
            /// Gets or sets the array value.
            /// </summary>
            /// <value>
            /// The array value.
            /// </value>
            public AssociativeArray ArrayValue { get; set; }

            /// <summary>
            /// Gets or sets the object value.
            /// </summary>
            /// <value>
            /// The object value.
            /// </value>
            public ObjectValue ObjectValue { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="CreatorVisitor"/> class.
            /// </summary>
            /// <param name="snapshot">The snapshot.</param>
            /// <param name="collector">The collector.</param>
            public CreatorVisitor(Snapshot snapshot, IndexCollector collector)
            {
                this.snapshot = snapshot;
                this.collector = collector;
            }

            /// <summary>
            /// Visits the variable.
            /// </summary>
            /// <param name="variableSegment">The variable segment.</param>
            public void VisitVariable(VariablePathSegment variableSegment)
            {
                switch (collector.Global)
                {
                    case GlobalContext.LocalOnly:
                        CreatedIndex = snapshot.CreateLocalVariable(Name, collector.CallLevel);
                        break;
                    case GlobalContext.GlobalOnly:
                        CreatedIndex = snapshot.CreateGlobalVariable(Name);
                        break;
                    default:
                        break;
                }
            }

            /// <summary>
            /// Visits the control.
            /// </summary>
            /// <param name="controlPathSegment">The control path segment.</param>
            public void VisitControl(ControlPathSegment controlPathSegment)
            {
                switch (collector.Global)
                {
                    case GlobalContext.LocalOnly:
                        CreatedIndex = snapshot.CreateLocalControll(Name, collector.CallLevel);
                        break;
                    case GlobalContext.GlobalOnly:
                        CreatedIndex = snapshot.CreateGlobalControll(Name);
                        break;
                    default:
                        break;
                }
            }

            /// <summary>
            /// Visits the temporary.
            /// </summary>
            /// <param name="temporaryPathSegment">The temporary path segment.</param>
            /// <exception cref="System.Exception">Acces to undefined temporary variable.</exception>
            public void VisitTemporary(TemporaryPathSegment temporaryPathSegment)
            {
                throw new Exception("Acces to undefined temporary variable.");
            }

            /// <summary>
            /// Visits the field.
            /// </summary>
            /// <param name="fieldSegment">The field segment.</param>
            public void VisitField(FieldPathSegment fieldSegment)
            {
                CreatedIndex = snapshot.CreateField(Name, ObjectValue, IsMust, true);
            }

            /// <summary>
            /// Visits the index.
            /// </summary>
            /// <param name="indexSegment">The index segment.</param>
            public void VisitIndex(IndexPathSegment indexSegment)
            {
                CreatedIndex = snapshot.CreateIndex(Name, ArrayValue, IsMust, true);
            }
        }

        /// <summary>
        /// Traverse memory tree by indexing non array values.
        /// </summary>
        class IndexLocationVisitor : ProcessValueAsLocationVisitor
        {
            IndexPathSegment indexSegment;
            HashSet<ValueLocation> mustLocationProcess;
            HashSet<ValueLocation> mayLocationProcess;

            /// <summary>
            /// Gets or sets the last value visitor.
            /// </summary>
            /// <value>
            /// The last value visitor.
            /// </value>
            public ReadIndexVisitor LastValueVisitor { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="IndexLocationVisitor"/> class.
            /// </summary>
            /// <param name="indexSegment">The index segment.</param>
            /// <param name="assistant">The assistant.</param>
            /// <param name="mustLocationProcess">The must location process.</param>
            /// <param name="mayLocationProcess">The may location process.</param>
            public IndexLocationVisitor(IndexPathSegment indexSegment, MemoryAssistantBase assistant, HashSet<ValueLocation> mustLocationProcess, HashSet<ValueLocation> mayLocationProcess)
                : base(assistant)
            {
                this.indexSegment = indexSegment;
                this.mustLocationProcess = mustLocationProcess;
                this.mayLocationProcess = mayLocationProcess;
            }

            /// <summary>
            /// Allews descendant implementation to continue traversing memory by indexing values ar accesing their fields.
            /// </summary>
            /// <param name="parentIndex">Index of the parent.</param>
            /// <param name="values">The values.</param>
            /// <param name="isMust">if set to <c>true</c> is must.</param>
            public override void ProcessValues(MemoryIndex parentIndex, IEnumerable<Value> values, bool isMust)
            {
                HashSet<ValueLocation> targetSet = mayLocationProcess;
                if (isMust && values.Count() == 1)
                {
                    targetSet = mustLocationProcess;
                }

                LastValueVisitor = new ReadIndexVisitor(parentIndex, indexSegment, targetSet);
                LastValueVisitor.VisitValues(values);
            }
        }

        /// <summary>
        /// Traverse memory tree by fields of non object values.
        /// </summary>
        class FieldLocationVisitor : ProcessValueAsLocationVisitor
        {
            FieldPathSegment fieldSegment;
            HashSet<ValueLocation> mustLocationProcess;
            HashSet<ValueLocation> mayLocationProcess;

            /// <summary>
            /// Gets or sets the last value visitor.
            /// </summary>
            /// <value>
            /// The last value visitor.
            /// </value>
            public ReadFieldVisitor LastValueVisitor { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="FieldLocationVisitor"/> class.
            /// </summary>
            /// <param name="fieldSegment">The field segment.</param>
            /// <param name="assistant">The assistant.</param>
            /// <param name="mustLocationProcess">The must location process.</param>
            /// <param name="mayLocationProcess">The may location process.</param>
            public FieldLocationVisitor(FieldPathSegment fieldSegment, MemoryAssistantBase assistant, HashSet<ValueLocation> mustLocationProcess, HashSet<ValueLocation> mayLocationProcess)
                : base(assistant)
            {
                this.fieldSegment = fieldSegment;
                this.mustLocationProcess = mustLocationProcess;
                this.mayLocationProcess = mayLocationProcess;
            }

            /// <summary>
            /// Allews descendant implementation to continue traversing memory by indexing values ar accesing their fields.
            /// </summary>
            /// <param name="parentIndex">Index of the parent.</param>
            /// <param name="values">The values.</param>
            /// <param name="isMust">if set to <c>true</c> is must.</param>
            public override void ProcessValues(MemoryIndex parentIndex, IEnumerable<Value> values, bool isMust)
            {
                HashSet<ValueLocation> targetSet = mayLocationProcess;
                if (isMust && values.Count() == 1)
                {
                    targetSet = mustLocationProcess;
                }

                LastValueVisitor = new ReadFieldVisitor(parentIndex, fieldSegment, targetSet);
                LastValueVisitor.VisitValues(values);
            }
        }
    }
}
