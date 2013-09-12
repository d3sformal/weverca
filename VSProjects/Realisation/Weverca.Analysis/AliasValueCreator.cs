using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.Analysis.Expressions
{
    class AliasValueCreator : TreeVisitor
    {
        AliasPoint _resultPoint;

        private readonly ElementExpander _valueCreator;

        internal AliasValueCreator(ElementExpander valueCreator)
        {
            _valueCreator = valueCreator;
        }

        internal AliasPoint CreateValue(LangElement el)
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

        private void Result(AliasPoint lValue)
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
            Result(new AliasVariablePoint(x));
        }

        public override void VisitIndirectVarUse(IndirectVarUse x)
        {
            throw new NotImplementedException();
        }

        public override void VisitItemUse(ItemUse x)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
