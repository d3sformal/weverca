using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

namespace Weverca.Analysis.ProgramPoints
{
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
        public readonly IEnumerable<RValuePoint> ExpressionParts;

        /// <summary>
        /// Evaluation log provide access to partial expression results
        /// </summary>
        public readonly EvaluationLog Log;

        public override LangElement Partial { get { return null; } }

        /// <summary>
        /// Result of assumption
        /// </summary>
        public bool Assumed { get; private set; }

        internal AssumePoint(AssumptionCondition condition, IEnumerable<RValuePoint> expressionParts)
        {
            NeedsFlowResolver = true;
            Condition = condition;
            Log = new EvaluationLog(expressionParts);
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
    }

    /// <summary>
    /// Sink for extension results. Merge caller context with call context.
    /// <remarks>Is used as reference to call result</remarks>
    /// </summary>
    public class ExtensionSinkPoint : RValuePoint
    {
        /// <summary>
        /// Extension which owns this sink
        /// <remarks>One sink is used per extension</remarks>
        /// </summary>
        public readonly FlowExtension OwningExtension;

        public override LangElement Partial { get { return null; } }

        internal ExtensionSinkPoint(FlowExtension owningExtension)
        {
            NeedsFlowResolver = true;
            NeedsFunctionResolver = true;

            OwningExtension = owningExtension;
        }

        protected override void flowThrough()
        {
            Services.FlowResolver.CallDispatchMerge(OutSet, OwningExtension.Branches, OwningExtension.Type);
            Value = Services.FunctionResolver.ResolveReturnValue(OwningExtension.Branches);
        }

        /// <summary>
        /// Input for sink is pre call set of owner - it cause merging caller context with call context
        /// </summary>
        protected override void extendInput()
        {
            _inSet.StartTransaction();
            //skip outset because of it belongs into call context
            _inSet.Extend(OwningExtension.Owner.InSet);
            _inSet.CommitTransaction();
        }
    }

    /// <summary>
    /// Native analyzer point representation
    /// </summary>
    public class NativeAnalyzerPoint : RValuePoint
    {
        /// <summary>
        /// Native analyzer contained in this point
        /// </summary>
        public readonly NativeAnalyzer Analyzer;

        public override LangElement Partial { get { return Analyzer; } }

        internal NativeAnalyzerPoint(NativeAnalyzer analyzer)
        {
            NeedsFunctionResolver = true;
            Analyzer = analyzer;
        }

        protected override void flowThrough()
        {
            Analyzer.Method(Flow);
        }
    }
}
