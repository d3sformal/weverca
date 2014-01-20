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
        public readonly IEnumerable<Tuple<GenericQualifiedName, ProgramPointBase>> CatchStarts;

        public override LangElement Partial { get { return null; } }

        public TryScopeStartsPoint(IEnumerable<Tuple<GenericQualifiedName, ProgramPointBase>> scopeStarts)
        {
            CatchStarts = scopeStarts;
        }

        protected override void flowThrough()
        {
            Services.FlowResolver.TryScopeStart(OutSet, CatchStarts);
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitTryScopeStarts(this);
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
        public readonly IEnumerable<Tuple<GenericQualifiedName, ProgramPointBase>> CatchStarts;

        public override LangElement Partial { get { return null; } }

        public TryScopeEndsPoint(IEnumerable<Tuple<GenericQualifiedName, ProgramPointBase>> catchStarts)
        {
            CatchStarts = catchStarts;
        }
        protected override void flowThrough()
        {
            Services.FlowResolver.TryScopeEnd(OutSet, CatchStarts);
        }

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
        public readonly EvaluationLog Log;

        public override LangElement Partial { get { return null; } }

        /// <summary>
        /// Result of assumption
        /// </summary>
        public bool Assumed { get; private set; }

        internal AssumePoint(AssumptionCondition condition, IEnumerable<ValuePoint> expressionParts)
        {
            Condition = condition;
            Log = new EvaluationLog(this, expressionParts);
        }

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
        public override LangElement Partial { get { return null; } }

        /// <summary>
        /// Graph connected via current connect point
        /// </summary>
        public readonly ProgramPointGraph Graph;

        /// <summary>
        /// Type of extension connection
        /// </summary>
        public readonly ExtensionType Type;

        public readonly ProgramPointBase Caller;

        internal ExtensionPoint(ProgramPointBase caller, ProgramPointGraph graph, ExtensionType type)
            :base(graph)
        {
            Graph = graph;
            Type = type;
            Caller = caller;

            AddFlowChild(Graph.Start);
        }

        internal void Disconnect()
        {
            RemoveFlowChild(Graph.Start);
        }

        protected override void extendInput()
        {
            _inSet.StartTransaction();

            if (Type == ExtensionType.ParallelCall)
            {
                _inSet.ExtendAsCall(Caller.OutSet, Flow.CalledObject, Flow.Arguments);
            }
            else
            {
                _inSet.Extend(Caller.OutSet);
            }

            _inSet.CommitTransaction();
        }


        protected override void flowThrough()
        {
            ForwardAnalysisServices.CurrentScript = ppGraph.OwningScript;

            if (Flow.Arguments == null)
                Flow.Arguments = new MemoryEntry[0];

            Services.FunctionResolver.InitializeCall(Graph, Flow.Arguments);
        }

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
        private static readonly VariableName returnVarName = new VariableName(".resultSinkPoint");

        public override LangElement Partial { get { return null; } }

        internal ExtensionSinkPoint(FlowExtension owningExtension)
        {
            OwningExtension = owningExtension;
        }

        protected override void extendInput()
        {
            _inSet.StartTransaction();
            _inSet.Extend(OwningExtension.Owner.OutSet);
            Services.FlowResolver.CallDispatchMerge(_inSet, OwningExtension.Branches);
            _inSet.CommitTransaction();
        }

        protected override void flowThrough()
        {
            ResolveReturnValue();
            ForwardAnalysisServices.CurrentScript = ppGraph.OwningScript;
        }

        public void ResolveReturnValue()
        {
            var returnValue = Services.FunctionResolver.ResolveReturnValue(OwningExtension.Branches);
            Value = OutSet.CreateSnapshotEntry(returnValue);
        }

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

        public override LangElement Partial { get { return Analyzer; } }

        internal NativeAnalyzerPoint(NativeAnalyzer analyzer)
        {
            Analyzer = analyzer;
        }

        protected override void flowThrough()
        {
            Analyzer.Method(Flow);
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitNativeAnalyzer(this);
        }
    }
}
