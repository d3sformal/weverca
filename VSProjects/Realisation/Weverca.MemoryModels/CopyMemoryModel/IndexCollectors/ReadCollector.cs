using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class ReadCollector : IndexCollector, IPathSegmentVisitor
    {
        List<MemoryIndex> mustIndexes = new List<MemoryIndex>();
        List<MemoryIndex> mayIndexes = new List<MemoryIndex>();

        List<MemoryIndex> mustIndexesProcess = new List<MemoryIndex>();
        private Snapshot snapshot;

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

        public ReadCollector(Snapshot snapshot)
        {
            IsDefined = true;
            this.snapshot = snapshot;
        }

        public override void Next(PathSegment segment)
        {
            segment.Accept(this);

            List<MemoryIndex> mustIndexesSwap = mustIndexes;
            mustIndexes = mustIndexesProcess;
            mustIndexesProcess = mustIndexesSwap;
            mustIndexesProcess.Clear();
        }

        public void VisitVariable(VariablePathSegment variableSegment)
        {
            switch (Global)
            {
                case GlobalContext.LocalOnly:
                    process(variableSegment, snapshot.Data.Variables.Local);
                    break;
                case GlobalContext.GlobalOnly:
                    process(variableSegment, snapshot.Data.Variables.Global);
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
                    process(controlPathSegment, snapshot.Data.ContolVariables.Local);
                    break;
                case GlobalContext.GlobalOnly:
                    process(controlPathSegment, snapshot.Data.ContolVariables.Global);
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
            foreach (MemoryIndex parentIndex in mustIndexes)
            {
                ObjectValueContainer objectValues = snapshot.Data.GetObjects(parentIndex);

                if (objectValues.Count > 0)
                {
                    foreach (ObjectValue objectValue in objectValues)
                    {
                        ObjectDescriptor descriptor = snapshot.Data.GetDescriptor(objectValue);
                        process(fieldSegment, descriptor);
                    }
                }
                else
                {
                    IsDefined = false;
                }
            }
        }

        public void VisitIndex(IndexPathSegment indexSegment)
        {
            foreach (MemoryIndex parentIndex in mustIndexes)
            {
                AssociativeArray arrayValue;
                if (snapshot.Data.TryGetArray(parentIndex, out arrayValue))
                {
                    ArrayDescriptor descriptor = snapshot.Data.GetDescriptor(arrayValue);
                    process(indexSegment, descriptor);
                }
                else
                {
                    IsDefined = false;
                }
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
                    mustIndexesProcess.Add(container.UnknownIndex);
                } 
            }
        }
    }
}
