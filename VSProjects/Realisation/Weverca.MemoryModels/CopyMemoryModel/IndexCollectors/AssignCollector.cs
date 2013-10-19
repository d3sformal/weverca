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
    class AssignCollector : IndexCollector
    {
        List<MemoryIndex> mustIndexes = new List<MemoryIndex>();
        List<MemoryIndex> mayIndexes = new List<MemoryIndex>();

        List<MemoryIndex> mustIndexesProcess = new List<MemoryIndex>();
        List<MemoryIndex> mayIndexesProcess = new List<MemoryIndex>();

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

        public override void Next(Snapshot snapshot, PathSegment segment)
        {
            switch (segment.Type)
            {
                case PathType.Variable:
                    processVariable(snapshot, segment);
                    break;
                case PathType.Field:
                    processField(snapshot, segment);
                    break;
                case PathType.Index:
                    processIndex(snapshot, segment);
                    break;

                default: throw new NotImplementedException();
            }

            //TODO pavel - reseni aliasu

            List<MemoryIndex> indexSwap = mustIndexes;
            mustIndexes = mustIndexesProcess;
            mustIndexesProcess = indexSwap;
            mustIndexesProcess.Clear();

            indexSwap = mayIndexes;
            mayIndexes = mayIndexesProcess;
            mayIndexesProcess = indexSwap;
            mayIndexesProcess.Clear();
        }

        private void processVariable(Snapshot snapshot, PathSegment segment)
        {
            if (segment.IsAny)
            {
                mayIndexesProcess.Add(snapshot.UnknownVariable);

                foreach (var variable in snapshot.Variables)
                {
                    mayIndexesProcess.Add(variable.Value);
                }
            }
            else if (segment.Names.Count == 1)
            {
                MemoryIndex variable;
                if (!snapshot.Variables.TryGetValue(segment.Names[0], out variable))
                {
                    variable = snapshot.CreateVariable(segment.Names[0]);
                }
                mustIndexesProcess.Add(variable);
            }
            else
            {
                foreach (String name in segment.Names)
                {
                    MemoryIndex variable;
                    if (!snapshot.Variables.TryGetValue(segment.Names[0], out variable))
                    {
                        variable = snapshot.CreateVariable(segment.Names[0]);
                    }
                    mayIndexesProcess.Add(variable);
                }
            }
        }

        private void processField(Snapshot snapshot, PathSegment segment)
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

        private void processField(Snapshot snapshot, PathSegment segment, MemoryIndex parentIndex, 
            List<MemoryIndex> mustTarget, bool isMust)
        {
            ObjectValue objectValue;
            if (!snapshot.TryGetObject(parentIndex, out objectValue))
            {
                objectValue = snapshot.CreateObject(parentIndex, isMust);
            }

            ObjectDescriptor descriptor = snapshot.GetDescriptor(objectValue);

            if (segment.IsAny)
            {
                mayIndexesProcess.Add(descriptor.UnknownField);

                foreach (var field in descriptor.Fields)
                {
                    mayIndexesProcess.Add(field.Value);
                }
            }
            else if (segment.Names.Count == 1)
            {
                MemoryIndex field;
                if (!descriptor.Fields.TryGetValue(segment.Names[0], out field))
                {
                    field = snapshot.CreateField(segment.Names[0], descriptor);
                }
                mustTarget.Add(field);
            }
            else
            {
                foreach (String name in segment.Names)
                {
                    MemoryIndex field;
                    if (!descriptor.Fields.TryGetValue(segment.Names[0], out field))
                    {
                        field = snapshot.CreateField(segment.Names[0], descriptor);
                    }
                    mayIndexesProcess.Add(field);
                }
            }
        }

        private void processIndex(Snapshot snapshot, PathSegment segment)
        {
            foreach (MemoryIndex parentIndex in mustIndexes)
            {
                processIndex(snapshot, segment, parentIndex, mustIndexesProcess, true);
            }

            foreach (MemoryIndex parentIndex in mayIndexesProcess)
            {
                processIndex(snapshot, segment, parentIndex, mayIndexesProcess, false);
            }
        }

        private void processIndex(Snapshot snapshot, PathSegment segment, MemoryIndex parentIndex, 
            List<MemoryIndex> mustTarget, bool isMust)
        {
            AssociativeArray arrayValue;
            if (!snapshot.TryGetArray(parentIndex, out arrayValue))
            {
                arrayValue = snapshot.CreateArray(parentIndex, isMust);
            }

            ArrayDescriptor descriptor = snapshot.GetDescriptor(arrayValue);

            if (segment.IsAny)
            {
                mayIndexesProcess.Add(descriptor.UnknownIndex);

                foreach (var field in descriptor.Indexes)
                {
                    mayIndexesProcess.Add(field.Value);
                }
            }
            else if (segment.Names.Count == 1)
            {
                MemoryIndex field;
                if (!descriptor.Indexes.TryGetValue(segment.Names[0], out field))
                {
                    field = snapshot.CreateIndex(segment.Names[0], descriptor);
                }
                mustTarget.Add(field);
            }
            else
            {
                foreach (String name in segment.Names)
                {
                    MemoryIndex field;
                    if (!descriptor.Indexes.TryGetValue(segment.Names[0], out field))
                    {
                        field = snapshot.CreateIndex(segment.Names[0], descriptor);
                    }
                    mayIndexesProcess.Add(field);
                }
            }
        }

    }
}
