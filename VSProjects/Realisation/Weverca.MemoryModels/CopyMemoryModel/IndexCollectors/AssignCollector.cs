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
    enum AliasesProcessing
    {
        AfterCollecting, BeforeCollecting, None
    }

    class AssignCollector : IndexCollector, IPathSegmentVisitor
    {
        HashSet<MemoryIndex> mustIndexes = new HashSet<MemoryIndex>();
        HashSet<MemoryIndex> mayIndexes = new HashSet<MemoryIndex>();

        HashSet<MemoryIndex> mustIndexesProcess = new HashSet<MemoryIndex>();
        HashSet<MemoryIndex> mayIndexesProcess = new HashSet<MemoryIndex>();

        HashSet<CollectedLocation> mustLocation = new HashSet<CollectedLocation>();
        HashSet<CollectedLocation> mayLocation = new HashSet<CollectedLocation>();

        HashSet<CollectedLocation> mustLocationProcess = new HashSet<CollectedLocation>();
        HashSet<CollectedLocation> mayLocationProcess = new HashSet<CollectedLocation>();
        private Snapshot snapshot;

        private CreatorVisitor creatorVisitor;
        private bool createNewStructure = true;

        public override bool IsDefined { get; protected set; }

        public AliasesProcessing AliasesProcessing { get; set; }

        public override IEnumerable<MemoryIndex> MustIndexes
        {
            get { return mustIndexes; }
        }

        public override IEnumerable<MemoryIndex> MayIndexes
        {
            get { return mayIndexes; }
        }

        public override IEnumerable<CollectedLocation> MustLocation
        {
            get { return mustLocation; }
        }

        public override IEnumerable<CollectedLocation> MayLocaton
        {
            get { return mayLocation; }
        }

        public override int MustIndexesCount
        {
            get { return mustIndexes.Count; }
        }

        public override int MayIndexesCount
        {
            get { return mayIndexes.Count; }
        }

        public AssignCollector(Snapshot snapshot)
        {
            this.snapshot = snapshot;
            creatorVisitor = new CreatorVisitor(snapshot, this);

            AliasesProcessing = AliasesProcessing.AfterCollecting;
        }

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

            HashSet<MemoryIndex> indexSwap = mustIndexes;
            mustIndexes = mustIndexesProcess;
            mustIndexesProcess = indexSwap;
            mustIndexesProcess.Clear();

            indexSwap = mayIndexes;
            mayIndexes = mayIndexesProcess;
            mayIndexesProcess = indexSwap;
            mayIndexesProcess.Clear();

            HashSet<CollectedLocation> locationSwap = mustLocation;
            mustLocation = mustLocationProcess;
            mustLocationProcess = locationSwap;
            mustLocationProcess.Clear();

            locationSwap = mayLocation;
            mayLocation = mayLocationProcess;
            mayLocationProcess = locationSwap;
            mayLocationProcess.Clear();
        }

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

        public void VisitTemporary(TemporaryPathSegment temporaryPathSegment)
        {
            addToMust(temporaryPathSegment.TemporaryIndex);
        }

        public void VisitField(FieldPathSegment segment)
        {
            FieldLocationVisitor visitor = new FieldLocationVisitor(segment, snapshot.Assistant, mustLocationProcess, mayLocationProcess);

            visitor.IsMust = true;
            foreach (MemoryIndex index in mustIndexes)
            {
                processField(segment, index, visitor, true);
            }
            foreach (CollectedLocation location in mustLocation)
            {
                location.Accept(visitor);
            }

            visitor.IsMust = false;
            foreach (MemoryIndex index in MayIndexes)
            {
                processField(segment, index, visitor, false);
            }
            foreach (CollectedLocation location in mayLocation)
            {
                location.Accept(visitor);
            }
        }

        public void VisitIndex(IndexPathSegment segment)
        {
            IndexLocationVisitor visitor = new IndexLocationVisitor(segment, snapshot.Assistant, mustLocationProcess, mayLocationProcess);

            visitor.IsMust = true;
            foreach (MemoryIndex index in mustIndexes)
            {
                processIndex(segment, index, visitor, true);
            }
            foreach (CollectedLocation location in mustLocation)
            {
                location.Accept(visitor);
            }

            visitor.IsMust = false;
            foreach (MemoryIndex index in mayIndexes)
            {
                processIndex(segment, index, visitor, false);
            }
            foreach (CollectedLocation location in mayLocation)
            {
                location.Accept(visitor);
            }
        }


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

        private void addToMust(MemoryIndex index)
        {
            if (!mayIndexesProcess.Contains(index))
            {
                mayIndexesProcess.Remove(index);
            }
            mustIndexesProcess.Add(index);
        }

        private bool addToMay(MemoryIndex index)
        {
            if (!mustIndexesProcess.Contains(index))
            {
                mayIndexesProcess.Add(index);
                return true;
            }
            else
            {
                return false;
            }
        }

        private class CreatorVisitor : IPathSegmentVisitor
        {
            private Snapshot snapshot;
            private IndexCollector collector;
            public MemoryIndex CreatedIndex { get; private set; }

            public string Name { get; set; }

            public bool IsMust { get; set; }

            public AssociativeArray ArrayValue { get; set; }

            public ObjectValue ObjectValue { get; set; }

            public CreatorVisitor(Snapshot snapshot, IndexCollector collector)
            {
                this.snapshot = snapshot;
                this.collector = collector;
            }

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

            public void VisitTemporary(TemporaryPathSegment temporaryPathSegment)
            {
                throw new Exception("Acces to undefined temporary variable.");
            }

            public void VisitField(FieldPathSegment fieldSegment)
            {
                CreatedIndex = snapshot.CreateField(Name, ObjectValue, IsMust, true);
            }

            public void VisitIndex(IndexPathSegment indexSegment)
            {
                CreatedIndex = snapshot.CreateIndex(Name, ArrayValue, IsMust, true);
            }
        }



        internal void SetAllToMust()
        {
            HashSetTools.AddAll(mustIndexes, mayIndexes);
            mayIndexes.Clear();
        }

        internal void CreateNewSctucture(bool createNewStructure)
        {
            this.createNewStructure = createNewStructure;
        }


        class IndexLocationVisitor : ProcessValueAsLocationVisitor
        {
            IndexPathSegment indexSegment;
            HashSet<CollectedLocation> mustLocationProcess;
            HashSet<CollectedLocation> mayLocationProcess;

            public ReadIndexVisitor LastValueVisitor { get; private set; }

            public IndexLocationVisitor(IndexPathSegment indexSegment, MemoryAssistantBase assistant, HashSet<CollectedLocation> mustLocationProcess, HashSet<CollectedLocation> mayLocationProcess)
                : base(assistant)
            {
                this.indexSegment = indexSegment;
                this.mustLocationProcess = mustLocationProcess;
                this.mayLocationProcess = mayLocationProcess;
            }

            public override void ProcessValues(MemoryIndex parentIndex, IEnumerable<Value> values, bool isMust)
            {
                HashSet<CollectedLocation> targetSet = mayLocationProcess;
                if (isMust && values.Count() == 1)
                {
                    targetSet = mustLocationProcess;
                }

                LastValueVisitor = new ReadIndexVisitor(parentIndex, indexSegment, targetSet);
                LastValueVisitor.VisitValues(values);
            }
        }

        class FieldLocationVisitor : ProcessValueAsLocationVisitor
        {
            FieldPathSegment fieldSegment;
            HashSet<CollectedLocation> mustLocationProcess;
            HashSet<CollectedLocation> mayLocationProcess;

            public ReadFieldVisitor LastValueVisitor { get; private set; }

            public FieldLocationVisitor(FieldPathSegment fieldSegment, MemoryAssistantBase assistant, HashSet<CollectedLocation> mustLocationProcess, HashSet<CollectedLocation> mayLocationProcess)
                : base(assistant)
            {
                this.fieldSegment = fieldSegment;
                this.mustLocationProcess = mustLocationProcess;
                this.mayLocationProcess = mayLocationProcess;
            }

            public override void ProcessValues(MemoryIndex parentIndex, IEnumerable<Value> values, bool isMust)
            {
                HashSet<CollectedLocation> targetSet = mayLocationProcess;
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
