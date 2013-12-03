using System;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// The class is used as base class for evaluation of specific type of expression during the analysis
    /// </summary>
    public abstract class PartialExpressionEvaluator : AbstractValueVisitor
    {
        /// <summary>
        /// Flow controller of program point providing data for evaluation (output set, position etc.)
        /// </summary>
        protected FlowController flow;

        /// <summary>
        /// Gets output set of a program point
        /// </summary>
        public FlowOutputSet OutSet
        {
            get { return flow.OutSet; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartialExpressionEvaluator" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        public PartialExpressionEvaluator(FlowController flowController)
        {
            SetContext(flowController);
        }

        /// <summary>
        /// Set current evaluation context.
        /// </summary>
        /// <remarks>
        /// The flow controller changes for every expression, so it must be always called again
        /// </remarks>
        /// <param name="flowController">Flow controller of program point available for evaluation</param>
        public void SetContext(FlowController flowController)
        {
            flow = flowController;
        }

        #region AbstractValueVisitor Members

        /// <inheritdoc />
        /// <exception cref="NotImplementedException">Thrown always</exception>
        public override void VisitValue(Value value)
        {
            throw new NotImplementedException("There is no way to evaluate a value of the type");
        }

        #region Function values

        /// <inheritdoc />
        /// <exception cref="ArgumentException">Thrown always since function value is not valid</exception>
        public override void VisitFunctionValue(FunctionValue value)
        {
            throw new ArgumentException("Expression cannot contain any function value");
        }

        /// <inheritdoc />
        public override void VisitLambdaFunctionValue(LambdaFunctionValue value)
        {
            // TODO: There is no special lambda type, it is implemented as Closure object with __invoke()
            throw new NotSupportedException("Lambda function evaluation is not currently supported");
        }

        #endregion Function values

        #region Type values

        /// <inheritdoc />
        /// <exception cref="ArgumentException">Thrown always since type value is not valid</exception>
        public override void VisitTypeValue(TypeValue value)
        {
            throw new ArgumentException("Expression cannot contain any type value");
        }

        #endregion Type values

        #region Special values

        /// <inheritdoc />
        /// <exception cref="ArgumentException">Thrown always since special value is not valid</exception>
        public override void VisitSpecialValue(SpecialValue value)
        {
            throw new ArgumentException("Expression cannot contain any special value");
        }

        #endregion Special values

        #endregion AbstractValueVisitor Members

        #region Helper methods

        /// <summary>
        /// Report a warning for the position of current expression
        /// </summary>
        /// <param name="message">Message of the warning</param>
        protected void SetWarning(string message)
        {
            var warning = new AnalysisWarning(message, flow.CurrentPartial);
            AnalysisWarningHandler.SetWarning(OutSet, warning);
        }

        /// <summary>
        /// Report a warning for the position of current expression
        /// </summary>
        /// <param name="message">Message of the warning</param>
        /// <param name="cause">Cause of the warning</param>
        protected void SetWarning(string message, AnalysisWarningCause cause)
        {
            var warning = new AnalysisWarning(message, flow.CurrentPartial, cause);
            AnalysisWarningHandler.SetWarning(OutSet, warning);
        }

        #endregion Helper methods
    }
}
