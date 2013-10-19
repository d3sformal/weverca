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
    class ReadCollector : IndexCollector
    {
        List<MemoryIndex> mustIndexes = new List<MemoryIndex>();
        List<MemoryIndex> mayIndexes = new List<MemoryIndex>();

        List<MemoryIndex> mustIndexesProcess = new List<MemoryIndex>();

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

        public ReadCollector()
        {
            IsDefined = true;
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

            List<MemoryIndex> mustIndexesSwap = mustIndexes;
            mustIndexes = mustIndexesProcess;
            mustIndexesProcess = mustIndexesSwap;
            mustIndexesProcess.Clear();
        }

        private void processVariable(Snapshot snapshot, PathSegment segment)
        {
            if (segment.IsAny)
            {
                mustIndexesProcess.Add(snapshot.UnknownVariable);
                foreach (var variable in snapshot.Variables)
                {
                    mustIndexesProcess.Add(variable.Value);
                }
            }
            else
            {
                foreach (String name in segment.Names)
                {
                    MemoryIndex variable;
                    if (snapshot.Variables.TryGetValue(name, out variable))
                    {
                        mustIndexesProcess.Add(variable);
                    }
                    else
                    {
                        IsDefined = false;
                    }
                }
            }
        }

        private void processField(Snapshot snapshot, PathSegment segment)
        {
            foreach (MemoryIndex parentIndex in mustIndexes)
            {
                ObjectValue objectValue;
                if (snapshot.TryGetObject(parentIndex, out objectValue))
                {
                    ObjectDescriptor descriptor = snapshot.GetDescriptor(objectValue);

                    if (segment.IsAny)
                    {
                        mustIndexesProcess.Add(descriptor.UnknownField);
                        foreach (var field in descriptor.Fields)
                        {
                            mustIndexesProcess.Add(field.Value);
                        }
                    }
                    else
                    {
                        foreach (String name in segment.Names)
                        {
                            MemoryIndex field;
                            if (descriptor.Fields.TryGetValue(name, out field))
                            {
                                mustIndexesProcess.Add(field);
                            }
                            else
                            {
                                IsDefined = false;
                            }
                        }
                    }
                }
                else
                {
                    IsDefined = false;
                }
            }
        }

        private void processIndex(Snapshot snapshot, PathSegment segment)
        {
            foreach (MemoryIndex parentIndex in mustIndexes)
            {
                AssociativeArray arrayValue;
                if (snapshot.TryGetArray(parentIndex, out arrayValue))
                {
                    ArrayDescriptor descriptor = snapshot.GetDescriptor(arrayValue);

                    if (segment.IsAny)
                    {
                        mustIndexesProcess.Add(descriptor.UnknownIndex);
                        foreach (var index in descriptor.Indexes)
                        {
                            mustIndexesProcess.Add(index.Value);
                        }
                    }
                    else
                    {
                        foreach (String name in segment.Names)
                        {
                            MemoryIndex index;
                            if (descriptor.Indexes.TryGetValue(name, out index))
                            {
                                mustIndexesProcess.Add(index);
                            }
                            else
                            {
                                IsDefined = false;
                            }
                        }
                    }
                }
                else
                {
                    IsDefined = false;
                }
            }
        }
    }
}
