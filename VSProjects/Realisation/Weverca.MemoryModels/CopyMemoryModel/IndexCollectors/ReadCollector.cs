using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.CopyMemoryModel;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class ReadCollector : IndexCollector, IPathSegmentVisitor
    {
        HashSet<MemoryIndex> mustIndexes = new HashSet<MemoryIndex>();
        List<MemoryIndex> mayIndexes = new List<MemoryIndex>();

        List<CollectedLocation> mustLocation = new List<CollectedLocation>();
        List<CollectedLocation> mayLocation = new List<CollectedLocation>();

        HashSet<MemoryIndex> mustIndexesProcess = new HashSet<MemoryIndex>();
        List<CollectedLocation> mustLocationProcess = new List<CollectedLocation>();
        Snapshot snapshot;

        public override bool IsDefined { get; protected set; }

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

        public ReadCollector(Snapshot snapshot)
        {
            IsDefined = true;
            this.snapshot = snapshot;
        }

        public override void Next(PathSegment segment)
        {
            segment.Accept(this);

            HashSet<MemoryIndex> mustIndexesSwap = mustIndexes;
            mustIndexes = mustIndexesProcess;
            mustIndexesProcess = mustIndexesSwap;
            mustIndexesProcess.Clear();

            List<CollectedLocation> mustLocationSwap = mustLocation;
            mustLocation = mustLocationProcess;
            mustLocationProcess = mustLocationSwap;
            mustLocationProcess.Clear();
        }

        public void VisitVariable(VariablePathSegment variableSegment)
        {
            switch (Global)
            {
                case GlobalContext.LocalOnly:
                    int level = CallLevel;
                    if (CallLevel > snapshot.CallLevel)
                    {
                        level = snapshot.CallLevel;
                    }

                    process(variableSegment, snapshot.Structure.Variables[level]);
                    break;
                case GlobalContext.GlobalOnly:
                    process(variableSegment, snapshot.Structure.Variables.Global);
                    break;
                default:
                    break;
            }
        }

        public void VisitControl(ControlPathSegment controlPathSegment)
        {
            switch (Global)
            {
                case GlobalContext.LocalOnly:
                    process(controlPathSegment, snapshot.Structure.ContolVariables[CallLevel]);
                    break;
                case GlobalContext.GlobalOnly:
                    process(controlPathSegment, snapshot.Structure.ContolVariables.Global);
                    break;
                default:
                    break;
            }
        }

        public void VisitTemporary(TemporaryPathSegment temporaryPathSegment)
        {
            mustIndexesProcess.Add(temporaryPathSegment.TemporaryIndex);
        }

        public void VisitField(FieldPathSegment fieldSegment)
        {
            FieldLocationVisitor visitor = new FieldLocationVisitor(fieldSegment, this);
            foreach (MemoryIndex parentIndex in mustIndexes)
            {
                processField(parentIndex, fieldSegment, visitor);
            }

            foreach (CollectedLocation parentLocation in mustLocation)
            {
                parentLocation.Accept(visitor);
            }
        }

        public void VisitIndex(IndexPathSegment indexSegment)
        {
            IndexLocationVisitor visitor = new IndexLocationVisitor(indexSegment, this);
            foreach (MemoryIndex parentIndex in mustIndexes)
            {
                processIndex(parentIndex, indexSegment, visitor);
            }

            foreach (CollectedLocation parentLocation in mustLocation)
            {
                parentLocation.Accept(visitor);
            }
        }

        private void processField(MemoryIndex parentIndex, FieldPathSegment fieldSegment, ProcessValueAsLocationVisitor visitor)
        {
            MemoryEntry entry;
            if (snapshot.Data.TryGetMemoryEntry(parentIndex, out entry))
            {
                bool processOtherValues = false;
                ObjectValueContainer objectValues = snapshot.Structure.GetObjects(parentIndex);
                if (objectValues.Count > 0)
                {
                    foreach (ObjectValue objectValue in objectValues)
                    {
                        ObjectDescriptor descriptor = snapshot.Structure.GetDescriptor(objectValue);
                        process(fieldSegment, descriptor);
                    }

                    processOtherValues = entry.Count > objectValues.Count;
                }
                else if (entry.Count > 0)
                {
                    processOtherValues = true;
                }
                else
                {
                    IsDefined = false;
                }

                if (processOtherValues)
                {
                    visitor.ProcessValues(parentIndex, entry.PossibleValues, true);
                }
            }
            else
            {
                IsDefined = false;
            }
        }

        private void processIndex(MemoryIndex parentIndex, IndexPathSegment indexSegment, ProcessValueAsLocationVisitor visitor)
        {
            MemoryEntry entry;
            if (snapshot.Data.TryGetMemoryEntry(parentIndex, out entry))
            {
                bool processOtherValues = false;
                AssociativeArray arrayValue;
                if (snapshot.Structure.TryGetArray(parentIndex, out arrayValue))
                {
                    ArrayDescriptor descriptor = snapshot.Structure.GetDescriptor(arrayValue);
                    process(indexSegment, descriptor);

                    processOtherValues = entry.Count > 1;
                }
                else if (entry.Count > 0)
                {
                    processOtherValues = true;
                }
                else
                {
                    IsDefined = false;
                }

                if (processOtherValues)
                {
                    visitor.ProcessValues(parentIndex, entry.PossibleValues, true);
                }
            }
            else
            {
                IsDefined = false;
            }
        }

        private void process(PathSegment segment, ReadonlyIndexContainer container)
        {
            if (segment.IsAny)
            {
                mustIndexesProcess.Add(container.UnknownIndex);
                foreach (var index in container.Indexes)
                {
                    mustIndexesProcess.Add(index.Value);
                }
            }
            else
            {
                bool isUnknown = false;
                foreach (String name in segment.Names)
                {
                    MemoryIndex index;
                    if (container.Indexes.TryGetValue(name, out index))
                    {
                        mustIndexesProcess.Add(index);
                    }
                    else
                    {
                        isUnknown = true;
                    }
                }

                if (isUnknown)
                {
                    IsDefined = false;
                    mustIndexesProcess.Add(container.UnknownIndex);
                } 
            }
        }

        class IndexLocationVisitor : ProcessValueAsLocationVisitor
        {
            IndexPathSegment indexSegment;
            ReadCollector collector;

            public IndexLocationVisitor(IndexPathSegment indexSegment, ReadCollector collector)
                : base(collector.snapshot.MemoryAssistant)
            {
                this.indexSegment = indexSegment;
                this.collector = collector;
            }

            public override void ProcessValues(MemoryIndex parentIndex, IEnumerable<Value> values, bool isMust)
            {
                ReadIndexVisitor visitor = new ReadIndexVisitor(parentIndex, indexSegment, collector.mustLocationProcess);
                visitor.VisitValues(values);
            }
        }

        class FieldLocationVisitor : ProcessValueAsLocationVisitor
        {
            FieldPathSegment fieldSegment;
            ReadCollector collector;

            public FieldLocationVisitor(FieldPathSegment fieldSegment, ReadCollector collector)
                : base(collector.snapshot.MemoryAssistant)
            {
                this.fieldSegment = fieldSegment;
                this.collector = collector;
            }

            public override void ProcessValues(MemoryIndex parentIndex, IEnumerable<Value> values, bool isMust)
            {
                ReadFieldVisitor visitor = new ReadFieldVisitor(parentIndex, fieldSegment, collector.mustLocationProcess);
                visitor.VisitValues(values);
            }
        }

    }
}
