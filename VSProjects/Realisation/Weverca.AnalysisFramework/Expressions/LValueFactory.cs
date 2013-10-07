﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.ProgramPoints;

namespace Weverca.Analysis.Expressions
{
    /// <summary>
    /// Creates LValue points (created values has stack edges connected, but no flow edges)
    /// <remarks>LValuePoint provides possibility to assign value or alias</remarks>
    /// </summary>
    class LValueFactory : TreeVisitor
    {
        /// <summary>
        /// Here is stored result of CreateValue operation
        /// </summary>
        LValuePoint _resultPoint;

        /// <summary>
        /// Expander is used for creating sub values
        /// </summary>
        private readonly ElementExpander _valueCreator;

        internal LValueFactory(ElementExpander valueCreator)
        {
            _valueCreator = valueCreator;
        }

        /// <summary>
        /// Create LValue point from given element
        /// </summary>
        /// <param name="el">Element from which RValue point will be created</param>
        /// <returns>Created LValue</returns>
        internal LValuePoint CreateValue(LangElement el)
        {
            //empty current result, because of avoiding incorrect use
            _resultPoint = null;

            el.VisitMe(this);
            //assert that result has been set
            Debug.Assert(_resultPoint != null);

            //empty result because of incorrect use
            var result = _resultPoint;
            _resultPoint = null;

            return result;
        }

        /// <summary>
        /// Set given LValue as CreateValue result
        /// </summary>
        /// <param name="value">Result value</param>
        private void Result(LValuePoint lValue)
        {
            Debug.Assert(_resultPoint == null, "Value has to be null - someone doesn't read it's value");
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
            RValuePoint thisObj = null;
            if (x.IsMemberOf != null)
            {
                thisObj = CreateRValue(x.IsMemberOf);
            }

            Result(new LVariablePoint(x,thisObj));
        }

        public override void VisitIndirectVarUse(IndirectVarUse x)
        {
            RValuePoint thisObj = null;
            if (x.IsMemberOf != null)
            {
                thisObj = CreateRValue(x.IsMemberOf);
            }

            var varName = CreateRValue(x.VarNameEx);
            Result(new LIndirectVariablePoint(x, varName,thisObj));
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