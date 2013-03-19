using PHP.Core.AST;

namespace PHP.Core.ControlFlow
{
    public class ControlFlowEdge
    {
        public ControlFlowEdge(BasicBlock target, Expression branchExpression)
        {
            this.Target = target;
            this.BranchExpression = branchExpression;
            this.Negation = false;
        }

        public static ControlFlowEdge WithNegatedExpression(BasicBlock target, Expression branchExpression)
        {
            var result = new ControlFlowEdge(target, branchExpression);
            result.Negation = true;
            return result;
        }

        public BasicBlock Target { get; internal set; }

        /// <summary>
        /// The expression that determines whether this branch/edge 
        /// is taken. <see cref="Negation"/> detemines whether we 
        /// branch on Expression evaluated to 
        /// <c>true</c> or to <c>false</c>. Expression can be null 
        /// on special edges (for example edges from try block to catch block 
        /// or when a jump is not conditional).
        /// </summary>
        public Expression BranchExpression { get; private set; }

        public bool HasBranchExpression
        {
            get { return this.BranchExpression != null; }
        }

        /// <summary>
        /// <see cref="Expression"/>
        /// </summary>
        public bool Negation { get; private set; }
    }
}
