using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.AnalysisFramework.Expressions
{
    /// <summary>
    /// Creates AliasValue points (created values has stack edges connected, but no flow edges)
    /// <remarks>AliasValuePoint provides AliasValues as result</remarks>
    /// </summary>
    class AliasValueFactory : TreeVisitor
    {
        /// <summary>
        /// Here is stored result of CreateValue operation
        /// </summary>
        AliasPoint _resultPoint;

        /// <summary>
        /// Expander is used for creating sub values
        /// </summary>
        private readonly ElementExpander _valueCreator;

        internal AliasValueFactory(ElementExpander valueCreator)
        {
            _valueCreator = valueCreator;
        }

        /// <summary>
        /// Create AliasValue point from given element
        /// </summary>
        /// <param name="el">Element from which AliasValue point will be created</param>
        /// <returns>Created RValue</returns>
        internal AliasPoint CreateValue(LangElement el)
        {
            _resultPoint = null;

            el.VisitMe(this);
            Debug.Assert(_resultPoint != null);

            var result = _resultPoint;
            _resultPoint = null;

            return result;
        }

        /// <summary>
        /// Set given RValue as CreateValue result
        /// </summary>
        /// <param name="value">Result value</param>
        private void Result(AliasPoint lValue)
        {
            _resultPoint = lValue;
        }

        /// <summary>
        /// Create RValue from given element
        /// </summary>
        /// <param name="el">Element which value will be created</param>
        /// <returns>Created value</returns>
        private RValuePoint CreateRValue(LangElement el)
        {
            return _valueCreator.CreateRValue(el);
        }

        #region TreeVisitor overrides


        public override void VisitElement(LangElement element)
        {
            throw new NotSupportedException("Given element is not supported RValue");
        }

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
