/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

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


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

using PHP.Core.AST;
using Weverca.ControlFlowGraph;


using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Program point computed during fix point algorithm.
    /// </summary>
    public abstract class ProgramPointBase
    {
        #region Private members

        /// <summary>
        /// Children of this program point
        /// </summary>
        private List<ProgramPointBase> _flowChildren = new List<ProgramPointBase>();

        /// <summary>
        /// Parents of this program point
        /// </summary>
        private List<ProgramPointBase> _flowParents = new List<ProgramPointBase>();

        /// <summary>
        /// Determine that program point has already been intialized (InSet,OutSet assigned)
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// Input set of current program point
        /// </summary>
        protected FlowOutputSet _inSet;

        /// <summary>
        /// Output set of current program point
        /// </summary>
		protected FlowOutputSet _outSet;

        /// <summary>
        /// Flow controller available for current program point
        /// </summary>
        internal protected FlowController Flow;

        #endregion
		/// <summary>
		/// Gets the number of fixpoint iterations.
		/// </summary>
		/// <value>The number of fixpoint iterations.</value>
		public int FixpointIterationsCount { get { return (_outSet != null) ? _outSet.CommitCount : 0; }}

        /// <summary>
        /// Extension of this program point
        /// </summary>
        public readonly FlowExtension Extension;

        /// <summary>
        /// Partial that defines this program point. Can be null for some program points.
        /// </summary>
        public abstract LangElement Partial { get; }

        /// <summary>
        /// Method for accepting visitors
        /// </summary>
        /// <param name="visitor">Accepted visitor</param>
        internal abstract void Accept(ProgramPointVisitor visitor);

        /// <summary>
        /// Input snapshot of this program point
        /// </summary>
        public SnapshotBase InSnapshot { get { return _inSet.Snapshot; } }

        /// <summary>
        /// Output snapshot of this program point
        /// </summary>
        public SnapshotBase OutSnapshot { get { return _outSet.Snapshot; } }

        /// <summary>
        /// Childrens of this program point
        /// </summary>
        public IEnumerable<ProgramPointBase> FlowChildren { get { return _flowChildren; } }

        /// <summary>
        /// Parents of this program point
        /// </summary>
        public IEnumerable<ProgramPointBase> FlowParents { get { return _flowParents; } }

        /// <summary>
        /// Count of parents of this program point
        /// </summary>
        public int FlowParentsCount { get { return _flowParents.Count; } }

        /// <summary>
        /// Count of children of this program point
        /// </summary>
        public int FlowChildrenCount { get { return _flowChildren.Count; } }

        /// <summary>
        /// Determine that program point has already been initialized (OutSet,InSet assigned)
        /// </summary>
        public bool IsInitialized { get { return _isInitialized; } }

        /// <summary>
        /// Input set of this program point
        /// </summary>
        public FlowInputSet InSet { get { return _inSet; } }

        /// <summary>
        /// Output set of this program point
        /// </summary>
        public FlowOutputSet OutSet { get { return _outSet; } }

        /// <summary>
        /// Program point graph that owns this program point
        /// </summary>
        public virtual ProgramPointGraph OwningPPGraph { get; private set; }

        /// <summary>
        /// The script in that this program point is defined.
        /// Note: can return null.
        /// </summary>
        public virtual FileInfo OwningScript { get {return OwningPPGraph.OwningScript;} }

        /// <summary>
        /// The name of the script in that this program point is defined.
        /// Note: can return "".
        /// </summary>
        public string OwningScriptFullName { get {
                FileInfo script = OwningScript; 
                if (script != null) return OwningScript.FullName;
                else return "";} }

        /// <summary>
        /// Analysis services available for subclasses to handle flow through method
        /// </summary>
        internal virtual ForwardAnalysisServices Services { get; private set; }

        internal ProgramPointBase()
        {
            Extension = new FlowExtension(this);
        }

        #region Program point flow controlling

        /// <summary>
        /// Flow through this program point
        /// </summary>
        protected abstract void flowThrough();

        internal void FlowThrough()
        {
            activateResolvers();
            if (prepareFlow())
            {
                flowThrough();
                if (Extension.IsConnected)
                {
                    //because we are flowing into extension - prepareIt
                    prepareExtension();
                }
                commitFlow();
            }
        }


        private void checkInitialized()
        {
            if (!_isInitialized)
            {
                Initialize(Services.CreateEmptySet(), Services.CreateEmptySet());
            }
        }

        /// <summary>
        /// Prepares the flow.
        /// </summary>
        /// <returns><c>true</c> if flow is prepared</returns>
        protected virtual bool prepareFlow()
        {
            checkInitialized();

            extendInput();
            extendOutput();

            return true;
        }

        /// <summary>
        /// Extends the output.
        /// </summary>
        protected virtual void extendOutput()
        {
            _outSet.StartTransaction();
            _outSet.Extend(_inSet);
        }

        /// <summary>
        /// Extends the input.
        /// </summary>
        protected virtual void extendInput()
        {
            var inputs = getInputsForExtension();

            if (inputs.Length > 0)
            {
                _inSet.StartTransaction();
                _inSet.Extend(inputs.ToArray());
                _inSet.CommitTransaction();
            }
        }

        /// <summary>
        /// Get inputs that should be used for extending the input of this program point.
        /// </summary>
        /// <returns>the inputs that should be used for extending the input of this program point; empty array if the input should not be extended</returns>
        protected ISnapshotReadonly[] getInputsForExtension() 
        {
            if (_flowParents.Count < 0) return new ISnapshotReadonly[0];

            var inputs = new List<FlowInputSet>();

            for (int i = 0; i < _flowParents.Count; ++i)
            {
                var flowParent = _flowParents[i];
                var assumeParent = flowParent as AssumePoint;

                var outset = flowParent.OutSet;
                if (outset == null || (assumeParent != null && !assumeParent.Assumed))
                {
                    //given parent is not computed yet or is unreachable
                    continue;
                }
                inputs.Add(outset);
            }

            return inputs.ToArray();
        }

        /// <summary>
        /// Commits the flow.
        /// </summary>
        protected virtual void commitFlow()
        {
            _outSet.CommitTransaction();
            if (!OutSet.HasChanges)
            {
                //nothing has changed during last commit
                return;
            }

            enqueueChildren();
        }

        /// <summary>
        /// Enqueues the children.
        /// </summary>
        protected virtual void enqueueChildren()
        {
            Services.EnqueueChildren(this);
        }

        private void prepareExtension()
        {
            foreach (var ext in Extension.Branches)
            {
                //pass call info
                ext.Flow.Arguments = Flow.Arguments;
                ext.Flow.CalledObject = Flow.CalledObject;
            }
        }

        private void activateResolvers()
        {
            Services.Evaluator.SetContext(Flow);
            Services.FunctionResolver.SetContext(Flow);
        }

        private void setNewController()
        {
            Flow = new FlowController(Services, this);

        }

        #endregion

        #region ProgramPoint graph building methods

        /// <summary>
        /// Add flow child to this program point
        /// NOTE:
        ///     Parent of child is also set
        /// </summary>
        /// <param name="child">Added child</param>
        internal void AddFlowChild(ProgramPointBase child)
        {
            _flowChildren.Add(child);
            child._flowParents.Add(this);
        }

        internal void RemoveFlowChild(ProgramPointBase child)
        {
            _flowChildren.Remove(child);
            child._flowParents.Remove(this);
        }

        /// <summary>
        /// Removes all flow children
        /// </summary>
        internal void RemoveFlowChildren()
        {
            foreach (var child in _flowChildren)
            {
                child._flowParents.Remove(this);
            }
            _flowChildren.Clear();
        }

        #endregion

        #region Internal API for changes handling

        /// <summary>
        /// Initialize program point with given input and output sets
        /// </summary>
        /// <param name="input">Input set of program point</param>
        /// <param name="output">Output set of program point</param>
        internal void Initialize(FlowOutputSet input, FlowOutputSet output)
        {
            if (_isInitialized)
            {
                throw new NotSupportedException("Initialization can be run only once");
            }
            _isInitialized = true;

            _inSet = input;
            _outSet = output;

            if (_inSet.Snapshot != null)
                //can be null in TestEntry
                _inSet.Snapshot.Assistant.RegisterProgramPoint(this);

            if (_outSet.Snapshot != null)
                //can be null in TestEntry
                _outSet.Snapshot.Assistant.RegisterProgramPoint(this);
        }

        /// <summary>
        /// Reset program point so that next transaction will 
        /// be marked with HasChanges
        /// </summary>
        internal void ResetInitialization()
        {
            if (_inSet != null) _inSet.ResetInitialization();
            if (_outSet != null) _outSet.ResetInitialization();
        }

        internal void SetServices(ForwardAnalysisServices services)
        {
            Services = services;
            Extension.Sink.Services = services;
            Extension.Sink.setNewController();
            setNewController();
        }

        internal void SetOwningGraph(ProgramPointGraph owningGraph)
        {
            this.OwningPPGraph = owningGraph;
            this.Extension.Sink.OwningPPGraph = owningGraph;
        }

        /// <summary>
        /// Set operational mode of current snapshot
        /// </summary>
        /// <param name="mode">Operational mode that will be set</param>
        internal void SetMode(SnapshotMode mode)
        {
            if (_inSet != null)
                _inSet.Snapshot.SetMode(mode);

            if (_outSet != null)
                _outSet.Snapshot.SetMode(mode);
        }


        /// <summary>
        /// Reset changes reported by output set - is used for Fix point computation
        /// </summary>
        internal void ResetChanges()
        {
            OutSet.ResetChanges();
        }

        #endregion

        private ProgramPointBase _afterWorklistSegment = null;

        /// <summary>
        /// The program point after the worklist segment which starts in this block (null elsewhere).
        /// 
        /// If worklist segment starts in this program point, it is constituted by program points between this block and AfterWorklistSegment.
        /// Worklist segment has no semantic meaning, it just serves for more optimal ordering of program 
        /// points in the worklist. 
        /// Fixpoint of program points in worklist segment should be computed before afterWorklistSegment is processed.
        /// </summary>
        internal ProgramPointBase AfterWorklistSegment { get { return _afterWorklistSegment;} }

        /// <summary>
        /// Creates worklist segment constituted by program points between the current program point and afterWorklistSegment.
        /// 
        /// <seealso cref="ProgramPoint.AfterWorklistSegment"/>
        /// </summary>
        /// <param name="afterWorklistSegment">the block after the worklist segement</param>
        public void CreateWorklistSegment(ProgramPointBase afterWorklistSegment)
        {
            _afterWorklistSegment = afterWorklistSegment;
        }

        /// <summary>
        /// Indicates whether in this block starts worklist segment.
        /// 
        /// <seealso cref="ProgramPoint.AfterWorklistSegment"/>
        /// </summary>
        /// <returns>true if in this block starts worklist segment, false elsewhere.</returns>
        public bool WorklistSegmentStart()
        {
            return (_afterWorklistSegment != null) ? true : false;
        }

        #region Debugging helpers

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() + " " + GetHashCode();
        }

        internal string ToStringChildren
        {
            get
            {
                return pointsToString(FlowChildren);
            }
        }

        internal string ToStringParents
        {
            get
            {
                return pointsToString(FlowParents);
            }
        }

        private string pointsToString(IEnumerable<ProgramPointBase> points)
        {
            var result = new StringBuilder();

            foreach (var point in points)
            {
                result.Append(point.ToString());
                result.Append(" | ");
            }

            return result.ToString();
        }

        #endregion
    }
}