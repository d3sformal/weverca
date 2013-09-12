using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.Analysis.Expressions
{
    class LValueCreator : TreeVisitor
    {
        LValuePoint _resultPoint;

        private readonly ElementExpander _valueCreator;

        internal LValueCreator(ElementExpander valueCreator)
        {
            _valueCreator = valueCreator;
        }

        internal LValuePoint CreateValue(LangElement el)
        {
            _resultPoint = null;

            el.VisitMe(this);
            Debug.Assert(_resultPoint != null);

            var result = _resultPoint;
            _resultPoint = null;

            return result;
        }

        public override void VisitElement(LangElement element)
        {
            throw new NotSupportedException("Given element is not supported RValue");
        }

        private void Result(LValuePoint lValue)
        {
            _resultPoint = lValue;
        }

        private RValuePoint CreateRValue(LangElement el)
        {
            return _valueCreator.CreateRValue(el);
        }

        #region TreeVisitor overrides

        public override void VisitDirectVarUse(DirectVarUse x)
        {
            Result(new LVariablePoint(x));
        }

        public override void VisitIndirectVarUse(IndirectVarUse x)
        {
            var varName = CreateRValue(x.VarNameEx);
            Result(new LIndirectVariablePoint(x, varName));
        }

        public override void VisitItemUse(ItemUse x)
        {
            if (x.IsMemberOf != null)
            {
                throw new NotImplementedException();
            }

            var array = CreateRValue(x.Array);
            var index = CreateRValue(x.Index);

            Result(new LItemUsePoint(x, array, index));

        }

        #endregion
    }
}
