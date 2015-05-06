/*
Copyright (c) 2012-2014 Pavel Bastecky.

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.ValueVisitors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.IndexCollectors
{
    /// <summary>
    /// Basic collecting algorithm which traverse memory tree by given path thru existing locations
    /// and do not modify memory structure.
    /// 
    /// Result of the algorithm is set of indexes in MustLocation list.
    /// </summary>
    class ReadCollector : IndexCollector, IPathSegmentVisitor
    {
        Snapshot snapshot;

        HashSet<MemoryIndex> mustIndexes = new HashSet<MemoryIndex>();
        List<MemoryIndex> mayIndexes = new List<MemoryIndex>();

        List<ValueLocation> mustLocation = new List<ValueLocation>();
        List<ValueLocation> mayLocation = new List<ValueLocation>();

        HashSet<MemoryIndex> mustIndexesProcess = new HashSet<MemoryIndex>();
        List<ValueLocation> mustLocationProcess = new List<ValueLocation>();

        /// <summary>
        /// Gets a value indicating whether access path is defined or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is defined]; otherwise, <c>false</c>.
        /// </value>
        public override bool IsDefined { get; protected set; }

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
        /// Initializes a new instance of the <see cref="ReadCollector"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        public ReadCollector(Snapshot snapshot)
        {
            IsDefined = true;
            this.snapshot = snapshot;
        }

        /// <summary>
        /// Pocess the next segment.
        /// </summary>
        /// <param name="segment">The segment.</param>
        public override void Next(PathSegment segment)
        {
            segment.Accept(this);

            HashSet<MemoryIndex> mustIndexesSwap = mustIndexes;
            mustIndexes = mustIndexesProcess;
            mustIndexesProcess = mustIndexesSwap;
            mustIndexesProcess.Clear();

            List<ValueLocation> mustLocationSwap = mustLocation;
            mustLocation = mustLocationProcess;
            mustLocationProcess = mustLocationSwap;
            mustLocationProcess.Clear();
        }

        /// <summary>
        /// Visits the variable to traverse memory tree from variable root.
        /// </summary>
        /// <param name="variableSegment">The variable segment.</param>
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

                    process(variableSegment, 
                        snapshot.Structure.Readonly.
                            GetReadonlyStackContext(level).ReadonlyVariables);
                    break;

                case GlobalContext.GlobalOnly:
                    process(variableSegment,
                        snapshot.Structure.Readonly.
                            ReadonlyGlobalContext.ReadonlyVariables);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Visits the control variable to traverse memory tree from control variable root.
        /// </summary>
        /// <param name="controlPathSegment">The control path segment.</param>
        public void VisitControl(ControlPathSegment controlPathSegment)
        {
            switch (Global)
            {
                case GlobalContext.LocalOnly:
                    process(controlPathSegment,
                        snapshot.Structure.Readonly.
                            GetReadonlyStackContext(CallLevel).ReadonlyControllVariables);
                    break;

                case GlobalContext.GlobalOnly:
                    process(controlPathSegment, 
                        snapshot.Structure.Readonly.
                            ReadonlyGlobalContext.ReadonlyControllVariables);
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
            mustIndexesProcess.Add(temporaryPathSegment.TemporaryIndex);
        }

        /// <summary>
        /// Visits the field to continue traversing memory tree by object field.
        /// </summary>
        /// <param name="fieldSegment">The field segment.</param>
        public void VisitField(FieldPathSegment fieldSegment)
        {
            FieldLocationVisitor visitor = new FieldLocationVisitor(fieldSegment, this);
            foreach (MemoryIndex parentIndex in mustIndexes)
            {
                processField(parentIndex, fieldSegment, visitor);
            }

            foreach (ValueLocation parentLocation in mustLocation)
            {
                parentLocation.Accept(visitor);
            }
        }

        /// <summary>
        /// Visits the index to continue traversing memory tree by array index.
        /// </summary>
        /// <param name="indexSegment">The index segment.</param>
        public void VisitIndex(IndexPathSegment indexSegment)
        {
            IndexLocationVisitor visitor = new IndexLocationVisitor(indexSegment, this);
            foreach (MemoryIndex parentIndex in mustIndexes)
            {
                processIndex(parentIndex, indexSegment, visitor);
            }

            foreach (ValueLocation parentLocation in mustLocation)
            {
                parentLocation.Accept(visitor);
            }
        }

        /// <summary>
        /// Processes the field - traverse thru all containing objects or sets path to undefined.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="fieldSegment">The field segment.</param>
        /// <param name="visitor">The visitor to process scalar values.</param>
        private void processField(MemoryIndex parentIndex, FieldPathSegment fieldSegment, ProcessValueAsLocationVisitor visitor)
        {
            MemoryEntry entry;
            if (snapshot.Data.Readonly.TryGetMemoryEntry(parentIndex, out entry))
            {
                bool processOtherValues = false;
                IObjectValueContainer objectValues = snapshot.Structure.Readonly.GetObjects(parentIndex);
                if (objectValues.Count > 0)
                {
                    foreach (ObjectValue objectValue in objectValues)
                    {
                        IObjectDescriptor descriptor = snapshot.Structure.Readonly.GetDescriptor(objectValue);
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

        /// <summary>
        /// Processes the index - traverse thru array or sets path to undefined.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="indexSegment">The index segment.</param>
        /// <param name="visitor">The visitor to process scalar values.</param>
        private void processIndex(MemoryIndex parentIndex, IndexPathSegment indexSegment, ProcessValueAsLocationVisitor visitor)
        {
            MemoryEntry entry;
            if (snapshot.Data.Readonly.TryGetMemoryEntry(parentIndex, out entry))
            {
                bool processOtherValues = false;
                AssociativeArray arrayValue;
                if (snapshot.Structure.Readonly.TryGetArray(parentIndex, out arrayValue))
                {
                    IArrayDescriptor descriptor = snapshot.Structure.Readonly.GetDescriptor(arrayValue);
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

        /// <summary>
        /// Cotinues traversing using specified segment in given index container.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <param name="container">The container.</param>
        private void process(PathSegment segment, IReadonlyIndexContainer container)
        {
            if (segment.IsAny)
            {
                mustIndexesProcess.Add(container.UnknownIndex);
                foreach (var index in container.Indexes)
                {
                    mustIndexesProcess.Add(index.Value);
                }
            } else if (segment.IsUnknown) 
            {
                mustIndexesProcess.Add(container.UnknownIndex);
            }
            else
            {
                bool isUnknown = false;
                foreach (String name in segment.Names)
                {
                    MemoryIndex index;
                    if (container.TryGetIndex(name, out index))
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

        /// <summary>
        /// Traverse memory tree by indexing non array values.
        /// </summary>
        class IndexLocationVisitor : ProcessValueAsLocationVisitor
        {
            IndexPathSegment indexSegment;
            ReadCollector collector;

            /// <summary>
            /// Initializes a new instance of the <see cref="IndexLocationVisitor"/> class.
            /// </summary>
            /// <param name="indexSegment">The index segment.</param>
            /// <param name="collector">The collector.</param>
            public IndexLocationVisitor(IndexPathSegment indexSegment, ReadCollector collector)
                : base(collector.snapshot.MemoryAssistant)
            {
                this.indexSegment = indexSegment;
                this.collector = collector;
            }

            /// <summary>
            /// Allews descendant implementation to continue traversing memory by indexing values ar accesing their fields.
            /// </summary>
            /// <param name="parentIndex">Index of the parent.</param>
            /// <param name="values">The values.</param>
            /// <param name="isMust">if set to <c>true</c> is must.</param>
            public override void ProcessValues(MemoryIndex parentIndex, IEnumerable<Value> values, bool isMust)
            {
                ReadIndexVisitor visitor = new ReadIndexVisitor(parentIndex, indexSegment, collector.mustLocationProcess);
                visitor.VisitValues(values);
            }
        }

        /// <summary>
        /// Traverse memory tree by fields of non object values.
        /// </summary>
        class FieldLocationVisitor : ProcessValueAsLocationVisitor
        {
            FieldPathSegment fieldSegment;
            ReadCollector collector;

            /// <summary>
            /// Initializes a new instance of the <see cref="FieldLocationVisitor"/> class.
            /// </summary>
            /// <param name="fieldSegment">The field segment.</param>
            /// <param name="collector">The collector.</param>
            public FieldLocationVisitor(FieldPathSegment fieldSegment, ReadCollector collector)
                : base(collector.snapshot.MemoryAssistant)
            {
                this.fieldSegment = fieldSegment;
                this.collector = collector;
            }

            /// <summary>
            /// Allews descendant implementation to continue traversing memory by indexing values ar accesing their fields.
            /// </summary>
            /// <param name="parentIndex">Index of the parent.</param>
            /// <param name="values">The values.</param>
            /// <param name="isMust">if set to <c>true</c> is must.</param>
            public override void ProcessValues(MemoryIndex parentIndex, IEnumerable<Value> values, bool isMust)
            {
                ReadFieldVisitor visitor = new ReadFieldVisitor(parentIndex, fieldSegment, collector.mustLocationProcess);
                visitor.VisitValues(values);
            }
        }

        protected override void FinishPath()
        {
        }
    }
}