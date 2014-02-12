using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;


namespace Weverca.AnalysisFramework.ProgramPoints
{
    /// <summary>
    /// Provider of constant(not changed during time) value, the value is not dependent on outer arguments
    /// </summary>
    /// <param name="evaluator">Evaluator which is used for creating value</param>
    /// <returns>Created value</returns>
    internal delegate MemoryEntry ConstantProvider(ExpressionEvaluatorBase evaluator);

    /// <summary>
    /// Constant value representation
    /// <remarks>Is usually used for storing literal value providers, etc.</remarks>
    /// </summary>
    public class ConstantPoint : ValuePoint
    {
        /// <summary>
        /// Provider of constant value
        /// <remarks>Is called on every program flow iteration, because of possible associating FlowInfo</remarks>
        /// </summary>
        private readonly ConstantProvider _constantProvider;

        private readonly LangElement _partial;

        public override LangElement Partial { get { return _partial; } }

        internal ConstantPoint(LangElement partial, ConstantProvider constantProvider)
        {
            _constantProvider = constantProvider;
            _partial = partial;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            var value= _constantProvider(Services.Evaluator);
            Value = OutSet.CreateSnapshotEntry(value);
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitConstant(this);
        }
    }

    public class ClassConstPoint : ValuePoint
    {
        public override LangElement Partial { get { return _partial; } }
        private readonly ClassConstUse _partial;
        public ValuePoint ThisObj;

        public ClassConstPoint(ClassConstUse x, ValuePoint thisObj)
        {
            _partial = x;
            this.ThisObj = thisObj;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            MemoryEntry value;
            if (ThisObj == null)
            {
                value=Services.Evaluator.ClassConstant(_partial.ClassName.QualifiedName, _partial.Name);
            }
            else
            {
                value=Services.Evaluator.ClassConstant(ThisObj.Value.ReadMemory(OutSnapshot), _partial.Name);
            }
            Value = OutSet.CreateSnapshotEntry(value);
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitClassConstPoint(this);
        }

        
    }
}
