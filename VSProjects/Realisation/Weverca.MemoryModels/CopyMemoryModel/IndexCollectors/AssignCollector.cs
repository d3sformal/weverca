﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class AssignCollector : IndexCollector, IPathSegmentVisitor
    {
        HashSet<MemoryIndex> mustIndexes = new HashSet<MemoryIndex>();
        HashSet<MemoryIndex> mayIndexes = new HashSet<MemoryIndex>();

        HashSet<MemoryIndex> mustIndexesProcess = new HashSet<MemoryIndex>();
        HashSet<MemoryIndex> mayIndexesProcess = new HashSet<MemoryIndex>();
        private Snapshot snapshot;

        private CreatorVisitor creatorVisitor;

        public override bool IsDefined { get; protected set; }

        public override IEnumerable<MemoryIndex> MustIndexes
        {
            get { return mustIndexes; }
        }

        public override IEnumerable<MemoryIndex> MayIndexes
        {
            get { return mayIndexes; }
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
            creatorVisitor = new CreatorVisitor(snapshot);
        }

        public override void Next(PathSegment segment)
        {
            segment.Accept(this);

            //TODO pavel - reseni aliasu

            HashSet<MemoryIndex> indexSwap = mustIndexes;
            mustIndexes = mustIndexesProcess;
            mustIndexesProcess = indexSwap;
            mustIndexesProcess.Clear();

            indexSwap = mayIndexes;
            mayIndexes = mayIndexesProcess;
            mayIndexesProcess = indexSwap;
            mayIndexesProcess.Clear();
        }

        public void VisitVariable(VariablePathSegment segment)
        {
            IndexContainer container = snapshot.Variables;
            processSegment(segment, container);
        }

        public void VisitField(FieldPathSegment segment)
        {
            foreach (MemoryIndex parentIndex in mustIndexes)
            {
                processField(snapshot, segment, parentIndex, mustIndexesProcess, true);
            }

            foreach (MemoryIndex parentIndex in MayIndexes)
            {
                processField(snapshot, segment, parentIndex, mayIndexesProcess, false);
            }
        }

        public void VisitIndex(IndexPathSegment segment)
        {
            foreach (MemoryIndex parentIndex in mustIndexes)
            {
                processIndex(snapshot, segment, parentIndex, mustIndexesProcess, true);
            }

            foreach (MemoryIndex parentIndex in mayIndexes)
            {
                processIndex(snapshot, segment, parentIndex, mayIndexesProcess, false);
            }
        }


        private void processField(Snapshot snapshot, PathSegment segment, MemoryIndex parentIndex,
            HashSet<MemoryIndex> mustTarget, bool isMust)
        {
            if (!snapshot.HasObjects(parentIndex))
            {
                ObjectValue objectValue = snapshot.CreateObject(parentIndex, isMust);
            }
            else if (snapshot.IsUndefined(parentIndex))
            {
                ObjectValue objectValue = snapshot.CreateObject(parentIndex, false);
            }
            
            if (isMust)
            {
                snapshot.ClearForObjects(parentIndex);
            }

            ObjectValueContainer objectValues = snapshot.GetObjects(parentIndex);
            foreach (ObjectValue value in objectValues)
            {
                ObjectDescriptor descriptor = snapshot.GetDescriptor(value);
                creatorVisitor.ObjectValue = value;
                
                if (isMust && descriptor.MustReferences.Contains(parentIndex))
                {
                    processSegment(segment, descriptor);
                }
                else
                {
                    processSegment(segment, descriptor, false);
                }
            }
        }

        private void processIndex(Snapshot snapshot, PathSegment segment, MemoryIndex parentIndex,
            HashSet<MemoryIndex> mustTarget, bool isMust)
        {
            AssociativeArray arrayValue;
            if (!snapshot.TryGetArray(parentIndex, out arrayValue))
            {
                arrayValue = snapshot.CreateArray(parentIndex, isMust);
            }
            else if (isMust)
            {
                snapshot.ClearForArray(parentIndex);
            }

            ArrayDescriptor descriptor = snapshot.GetDescriptor(arrayValue);
            creatorVisitor.ArrayValue = arrayValue;
            processSegment(segment, descriptor, isMust);
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

        private void addToMay(MemoryIndex index)
        {
            if (!mustIndexesProcess.Contains(index))
            {
                mayIndexesProcess.Add(index);
            }
        }

        private class CreatorVisitor : IPathSegmentVisitor
        {
            private Snapshot snapshot;
            public MemoryIndex CreatedIndex { get; private set; }

            public string Name { get; set; }

            public bool IsMust { get; set; }

            public AssociativeArray ArrayValue { get; set; }

            public ObjectValue ObjectValue { get; set; }


            public CreatorVisitor(Snapshot snapshot)
            {
                this.snapshot = snapshot;
            }

            public void VisitVariable(VariablePathSegment variableSegment)
            {
                CreatedIndex = snapshot.CreateVariable(Name);
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
    }
}
