using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;

/*
 * Program points that have no corresponding elements in Phalanger AST.
 */

namespace Weverca.AnalysisFramework.ProgramPoints
{

    /// <summary>
    /// Report that from this point starts scope of specified catch blocks
    /// <remarks>Scope is explicitly ended with CatchScopeEndsPoint, 
    /// or implicitly because of stack unwinding (that has to solve analysis itself)
    /// </remarks>
    /// </summary>
    public class TryScopeStartsPoint : ProgramPointBase
    {
        /// <summary>
        /// Starting points of catch blocks with scope in starting try block
        /// </summary>
        public readonly IEnumerable<CatchBlockDescription> CatchStarts;

        /// <inheritdoc />
        public override LangElement Partial { get { return null; } }

        internal TryScopeStartsPoint(IEnumerable<CatchBlockDescription> scopeStarts)
        {
            CatchStarts = scopeStarts;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            Services.FlowResolver.TryScopeStart(OutSet, CatchStarts);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitTryScopeStarts(this);
        }
    }

    /// <summary>
    /// Program point used for connection between throw and according catch block
    /// </summary>
    public class CatchPoint : ProgramPointBase
    {
        /// <inheritdoc />
        public override LangElement Partial { get { return null; } }

        /// <summary>
        /// Point that has thrown catched exception
        /// </summary>
        public readonly ProgramPointBase ThrowingPoint;

        /// <summary>
        /// Point where execution continues
        /// </summary>
        public ProgramPointBase TargetPoint { get { return CatchDescription.TargetPoint; } }

        /// <summary>
        /// Value that has been thrown
        /// </summary>
        public MemoryEntry ThrowedValue { get { return _info.ThrowedValue; } }

        /// <summary>
        /// Description of catching block
        /// </summary>
        public readonly CatchBlockDescription CatchDescription;

        /// <summary>
        /// Current throw info
        /// </summary>
        private ThrowInfo _info;

        internal CatchPoint(ProgramPointBase throwingPoint, CatchBlockDescription catchDescription)
        {
            ThrowingPoint = throwingPoint;
            CatchDescription = catchDescription;
        }

        /// <summary>
        /// Updates throw information stored in current point
        /// </summary>
        /// <param name="info">Throw information that has to be compatilbe with current point</param>
        internal void ReThrow(ThrowInfo info)
        {
            if (!CatchDescription.Equals(info.Catch))
                throw new NotSupportedException("Cannot rethrow with given info");

            _info = info;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            Flow.FlowResolver.Catch(this, OutSet);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitCatch(this);
        }
    }

    /// <summary>
    /// Report explicit scope ending of specified catch blocks   
    /// </summary>
    public class TryScopeEndsPoint : ProgramPointBase
    {
        /// <summary>
        /// Starting points of catch blocks with scope in ending try block
        /// </summary>
        public readonly IEnumerable<CatchBlockDescription> CatchStarts;

        /// <inheritdoc />
        public override LangElement Partial { get { return null; } }

        internal TryScopeEndsPoint(IEnumerable<CatchBlockDescription> catchStarts)
        {
            CatchStarts = catchStarts;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            Services.FlowResolver.TryScopeEnd(OutSet, CatchStarts);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitTryScopeEnds(this);
        }
    }

    /// <summary>
    /// Process assumption in program point graph
    /// <remarks>Enqueue flow children only if assumption condition is assumed</remarks>
    /// </summary>
    public class AssumePoint : ProgramPointBase
    {
        /// <summary>
        /// Condition to be assumed
        /// </summary>
        public readonly AssumptionCondition Condition;

        /// <summary>
        /// Evaluated parts of assumed expression parts
        /// </summary>
        public readonly IEnumerable<ValuePoint> ExpressionParts;

        /// <summary>
        /// Evaluation log provide access to partial expression results
        /// </summary>
        public EvaluationLog Log { get { return OwningPPGraph.EvaluationLog; } }

        /// <inheritdoc />
        public override LangElement Partial { get { return null; } }

        /// <summary>
        /// Result of assumption
        /// </summary>
        public bool Assumed { get; private set; }

        internal AssumePoint(AssumptionCondition condition, IEnumerable<ValuePoint> expressionParts)
        {
            Condition = condition;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            Assumed = Services.FlowResolver.ConfirmAssumption(OutSet, Condition, Log);
        }

        /// <summary>
        /// Enqueue children only if condition has been assumed
        /// </summary>
        protected override void enqueueChildren()
        {
            if (Assumed)
            {
                //only if assumption is made, process children
                base.enqueueChildren();
            }
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitAssume(this);
        }
    }

    /// <summary>
    /// Point for connecting extensions into program point graphs
    /// </summary>
    public class ExtensionPoint : ProgramPointBase
    {
        /// <inheritdoc />
        public override LangElement Partial { get { return null; } }

        /// <summary>
        /// Graph connected via current connect point
        /// </summary>
        public readonly ProgramPointGraph Graph;

        /// <summary>
        /// Type of extension connection
        /// </summary>
        public readonly ExtensionType Type;

        /// <summary>
        /// Caller which call creats current extension point
        /// </summary>
        public readonly ProgramPointBase Caller;

        /// <inheritdoc />
        public override ProgramPointGraph OwningPPGraph
        {
            get
            {
                return Graph;
            }
        }

        /// <inheritdoc />
        internal override ForwardAnalysisServices Services
        {
            get
            {
                return Caller.Services;
            }
        }

        internal ExtensionPoint(ProgramPointBase caller, ProgramPointGraph graph, ExtensionType type)
        {
            Graph = graph;
            Type = type;
            Caller = caller;

            AddFlowChild(Graph.Start);
        }

        /// <summary>
        /// Disconnect current point from Graph
        /// </summary>
        internal void Disconnect()
        {
            RemoveFlowChild(Graph.Start);
        }

        /// <inheritdoc />
        protected override void extendInput()
        {
            _inSet.StartTransaction();

            if (Type == ExtensionType.ParallelCall)
            {

                var calledObject = Services.FunctionResolver.InitializeCalledObject(Caller, Graph, Flow.CalledObject);
                _inSet.ExtendAsCall(Caller.OutSet, calledObject, Flow.Arguments);
            }
            else
            {
                _inSet.Extend(Caller.OutSet);
            }

            _inSet.CommitTransaction();
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            if (Flow.Arguments == null)
                Flow.Arguments = new MemoryEntry[0];

            Services.FunctionResolver.InitializeCall(Caller, Graph, Flow.Arguments);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitExtension(this);
        }
    }

    /// <summary>
    /// Sink for extension results. Merge caller context with call context.
    /// <remarks>Is used as reference to call result</remarks>
    /// </summary>
    public class ExtensionSinkPoint : ValuePoint
    {
        /// <summary>
        /// Extension which owns this sink
        /// <remarks>One sink is used per extension</remarks>
        /// </summary>
        public readonly FlowExtension OwningExtension;

        /// <inheritdoc />
        public override LangElement Partial { get { return null; } }

        /// <inheritdoc />
        internal override ForwardAnalysisServices Services
        {
            get
            {
                return OwningExtension.Owner.Services;
            }
        }

        /// <inheritdoc />
        public override ProgramPointGraph OwningPPGraph
        {
            get
            {
                return OwningExtension.Owner.OwningPPGraph;
            }
        }

        internal ExtensionSinkPoint(FlowExtension owningExtension)
        {
            OwningExtension = owningExtension;
        }

        /// <inheritdoc />
        protected override void extendInput()
        {
            _inSet.StartTransaction();
            _inSet.Extend(OwningExtension.Owner.OutSet);
            Services.FlowResolver.CallDispatchMerge(_inSet, OwningExtension.Branches);
            _inSet.CommitTransaction();
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            ResolveReturnValue();
        }

        /// <summary>
        /// Resolves return value of current sink. Resolved value is stored within Value.
        /// </summary>
        public void ResolveReturnValue()
        {
            var returnValue = Services.FunctionResolver.ResolveReturnValue(OwningExtension.Branches);
            Value = OutSet.CreateSnapshotEntry(returnValue);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitExtensionSink(this);
        }
    }

    /// <summary>
    /// Native analyzer point representation
    /// </summary>
    public class NativeAnalyzerPoint : ValuePoint
    {
        /// <summary>
        /// Native analyzer contained in this point
        /// </summary>
        public readonly NativeAnalyzer Analyzer;

        /// <inheritdoc />
        public override LangElement Partial { get { return Analyzer; } }

        internal NativeAnalyzerPoint(NativeAnalyzer analyzer)
        {
            Analyzer = analyzer;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            Analyzer.Method(Flow);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitNativeAnalyzer(this);
        }
    }
}
