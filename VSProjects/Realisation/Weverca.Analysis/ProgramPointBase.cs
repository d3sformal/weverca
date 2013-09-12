using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;
using Weverca.ControlFlowGraph;

using Weverca.Analysis.Memory;
using Weverca.Analysis.Expressions;

namespace Weverca.Analysis
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


        protected FlowOutputSet _inSet;
        private FlowOutputSet _outSet;

        protected bool NeedsExpressionEvaluator;
        protected bool NeedsFunctionResolver;
        protected bool NeedsFlowResolver;

        protected FlowController Flow;

        #endregion

        public readonly FlowExtension Extension;

        public abstract LangElement Partial { get; }

        /// <summary>
        /// Childrens of this program point
        /// </summary>
        public IEnumerable<ProgramPointBase> FlowChildren { get { return _flowChildren; } }

        /// <summary>
        /// Parents of this program point
        /// </summary>
        public IEnumerable<ProgramPointBase> FlowParents { get { return _flowParents; } }

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
        /// Analysis services available for subclasses to handle flow through method
        /// </summary>
        internal AnalysisServices Services { get; private set; }


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

        protected virtual bool prepareFlow()
        {
            checkInitialized();

            extendInput();
            

            _outSet.StartTransaction();
            _outSet.Extend(_inSet);
            return true;
        }

        protected virtual void extendInput()
        {
            if (_flowParents.Count > 0)
            {
                var inputs = new List<FlowInputSet>();

                for (int i = 0; i < _flowParents.Count; ++i)
                {
                    var outset = _flowParents[i].OutSet;
                    if (outset == null)
                    {
                        //TODO optimize - reenqueue
                        continue;
                    }
                    inputs.Add(outset);
                }
                _inSet.StartTransaction();
                _inSet.Extend(inputs.ToArray());
                _inSet.CommitTransaction();
            }
        }

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

        protected virtual void enqueueChildren()
        {
            foreach (var child in _flowChildren)
            {
                Services.Enqueue(child);
            }
        }

        private void prepareExtension()
        {
            if (!NeedsFunctionResolver)
            {
                Services.FunctionResolver.SetContext(Flow);
            }

            foreach (var ext in Extension.Branches)
            {
                var start=ext.Start;
                
                _outSet.ExtendAsCall(_inSet,Flow.CalledObject,Flow.Arguments);
                if (Flow.Arguments == null)
                    Flow.Arguments = new MemoryEntry[0];
                Services.FunctionResolver.InitializeCall(_outSet, ext, Flow.Arguments);
            }
        }
        
        private void activateResolvers()
        {
            if (NeedsExpressionEvaluator)
                Services.Evaluator.SetContext(Flow);

            if (NeedsFunctionResolver)
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
        }

        internal void SetServices(AnalysisServices services)
        {
            Services = services;
            Extension.Sink.Services = services;
            setNewController();
        }

        /// <summary>
        /// Reset changes reported by output set - is used for Fix point computation
        /// </summary>
        internal void ResetChanges()
        {
            OutSet.ResetChanges();
        }

        #endregion




    }
}
