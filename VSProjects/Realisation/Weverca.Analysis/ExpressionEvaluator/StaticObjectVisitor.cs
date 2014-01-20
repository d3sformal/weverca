using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.Analysis.ExpressionEvaluator
{
    public enum StaticObjectVisitorResult
    {
        NO_RESULT,
        ONE_RESULT,
        MULTIPLE_RESULTS
    }

    public class StaticObjectVisitor : AbstractValueVisitor
    {
        public StaticObjectVisitorResult Result;
        public QualifiedName className;
        private FlowOutputSet OutSet;

        public StaticObjectVisitor(FlowOutputSet OutSet)
        {
            this.OutSet = OutSet;
        }

        public override void VisitValue(Value value)
        {
            //add warning
            Result = StaticObjectVisitorResult.NO_RESULT;
        }

        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            Result = StaticObjectVisitorResult.MULTIPLE_RESULTS;
        }

        public override void VisitAnyStringValue(AnyStringValue value)
        {
            Result = StaticObjectVisitorResult.MULTIPLE_RESULTS;
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            className = OutSet.ObjectType(value).Declaration.QualifiedName;
            Result = StaticObjectVisitorResult.ONE_RESULT;
        }

        public override void VisitStringValue(StringValue value)
        {
            className = new QualifiedName(new Name(value.Value));
            Result = StaticObjectVisitorResult.ONE_RESULT;
        }



    }
}
