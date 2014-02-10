using System;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// The class is used as base class for evaluation of specific type of expression during the analysis.
    /// </summary>
    /// <remarks>
    /// Evaluation of every expression has common characteristics. The evaluation can need context of
    /// the program. Class <see cref="FlowController" /> represent the context, that every construct
    /// in analysis can use. It contains <see cref="FlowOutputSet" /> class, that is necessary, among
    /// other things, for access to variables and creating new values, which most expressions do.
    /// The correct context must be set before every use of class by <see cref="SetContext" /> method.
    /// Sometimes an expression is not correct in some way. Therefore, there is "SetContext" method
    /// to create a warning. Some values ??cannot appear in the expression, i.e. type or function values.
    /// Then the exception is thrown.
    /// </remarks>
    public abstract class PartialExpressionEvaluator : AbstractValueVisitor
    {
        /// <summary>
        /// Flow controller of program point providing data for evaluation (output set, position etc.).
        /// </summary>
        protected FlowController flow;

        /// <summary>
        /// Gets output set of a program point.
        /// </summary>
        public FlowOutputSet OutSet
        {
            get { return flow.OutSet; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartialExpressionEvaluator" /> class.
        /// </summary>
        /// <remarks>
        /// Context must be set by <see cref="SetContext" /> method before the first use of evaluator.
        /// </remarks>
        protected PartialExpressionEvaluator()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartialExpressionEvaluator" /> class.
        /// </summary>
        /// <remarks>
        /// Constructor with <see cref="FlowController" /> parameter is very useful when object is utilized.
        /// immediately for one particular task.
        /// </remarks>
        /// <param name="flowController">Flow controller of program point.</param>
        protected PartialExpressionEvaluator(FlowController flowController)
        {
            SetContext(flowController);
        }

        /// <summary>
        /// Set current evaluation context.
        /// </summary>
        /// <remarks>
        /// The flow controller changes for every expression, so it must be always called again.
        /// </remarks>
        /// <param name="flowController">Flow controller of program point available for evaluation.</param>
        public void SetContext(FlowController flowController)
        {
            flow = flowController;
        }

        #region AbstractValueVisitor Members

        /// <inheritdoc />
        /// <exception cref="System.NotImplementedException">Thrown always</exception>
        public override void VisitValue(Value value)
        {
            throw new NotImplementedException("There is no way to evaluate a value of the type");
        }

        #region Abstract values

        #region Abstract scalar values

        /// <inheritdoc />
        public override void VisitAnyScalarValue(AnyScalarValue value)
        {
            // Skip <see cref="AnyValue" />. The type is super-type of <see cref="AnyScalarValue" />,
            // but it is concrete type and its visit method can contain code that is solving the same
            // problem as the previous visit methods. In this case, use additional method with shared code.
            VisitValue(value);
        }

        #endregion Abstract scalar values

        #region Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyCompoundValue(AnyCompoundValue value)
        {
            // Skip <see cref="AnyValue" />. The type is super-type of <see cref="AnyCompoundValue" />,
            // but it is concrete type and its visit method can contain code that is solving the same
            // problem as the previous visit methods. In this case, use additional method with shared code.
            VisitValue(value);
        }

        #endregion Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyResourceValue(AnyResourceValue value)
        {
            // Skip <see cref="AnyValue" />. The type is super-type of <see cref="AnyResourceValue" />,
            // but it is concrete type and its visit method can contain code that is solving the same
            // problem as the previous visit methods. In this case, use additional method with shared code.
            VisitValue(value);
        }

        #endregion Abstract values

        #region Function values

        /// <inheritdoc />
        /// <exception cref="System.ArgumentException">
        /// Thrown always since function value is not valid in an expression
        /// </exception>
        public override void VisitFunctionValue(FunctionValue value)
        {
            throw new ArgumentException("Expression cannot contain any function value");
        }

        /// <inheritdoc />
        /// <exception cref="System.NotSupportedException">Thrown always</exception>
        public override void VisitLambdaFunctionValue(LambdaFunctionValue value)
        {
            // TODO: There is no special lambda type, it is implemented as Closure object with __invoke()
            throw new NotSupportedException("Lambda function evaluation is not currently supported");
        }

        #endregion Function values

        #region Type values

        /// <inheritdoc />
        /// <exception cref="System.ArgumentException">Thrown always since type value is not valid</exception>
        public override void VisitTypeValue(TypeValue value)
        {
            throw new ArgumentException("Expression cannot contain any type value");
        }

        #endregion Type values

        #region Special values

        /// <inheritdoc />
        /// <exception cref="System.ArgumentException">
        /// Thrown always since special value is not valid in an expression
        /// </exception>
        public override void VisitSpecialValue(SpecialValue value)
        {
            throw new ArgumentException("Expression cannot contain any special value");
        }

        #endregion Special values

        #endregion AbstractValueVisitor Members

        #region Helper methods

        /// <summary>
        /// Report a warning for the position of current expression.
        /// </summary>
        /// <param name="message">Message of the warning.</param>
        protected void SetWarning(string message)
        {
            var warning = new AnalysisWarning(flow.CurrentScript.FullName,message, flow.CurrentPartial);
            AnalysisWarningHandler.SetWarning(OutSet, warning);
        }

        /// <summary>
        /// Report a warning for the position of current expression.
        /// </summary>
        /// <param name="message">Message of the warning.</param>
        /// <param name="cause">Cause of the warning.</param>
        protected void SetWarning(string message, AnalysisWarningCause cause)
        {
            var warning = new AnalysisWarning(flow.CurrentScript.FullName,message, flow.CurrentPartial, cause);
            AnalysisWarningHandler.SetWarning(OutSet, warning);
        }

        #endregion Helper methods
    }
}
