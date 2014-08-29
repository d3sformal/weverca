/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


/*
Copyright (c) 2012-2014 David Hauzar and Mirek Vodolan.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


﻿using System;
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
        /// Partial represented by current point
        /// </summary>
        private readonly LangElement _partial;

        /// <summary>
        /// Provider of constant value
        /// <remarks>Is called on every program flow iteration, because of possible associating FlowInfo</remarks>
        /// </summary>
        private readonly ConstantProvider _constantProvider;


        /// <inheritdoc />
        public override LangElement Partial { get { return _partial; } }

        internal ConstantPoint(LangElement partial, ConstantProvider constantProvider)
        {
            _constantProvider = constantProvider;
            _partial = partial;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            var value = _constantProvider(Services.Evaluator);
            Value = OutSet.CreateSnapshotEntry(value);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitConstant(this);
        }
    }

    /// <summary>
    /// Represents use of Class constant
    /// </summary>
    public class ClassConstPoint : ValuePoint
    {
        /// <summary>
        /// Partial represented by current point
        /// </summary>
        private readonly ClassConstUse _partial;

        /// <inheritdoc />
        public override LangElement Partial { get { return _partial; } }

        /// <summary>
        /// This object of class constant use
        /// </summary>
        public ValuePoint ThisObj;

        internal ClassConstPoint(ClassConstUse x, ValuePoint thisObj)
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
                value = Services.Evaluator.ClassConstant(_partial.ClassName.QualifiedName, _partial.Name);
            }
            else
            {
                value = Services.Evaluator.ClassConstant(ThisObj.Value.ReadMemory(OutSnapshot), _partial.Name);
            }
            Value = OutSet.CreateSnapshotEntry(value);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitClassConstPoint(this);
        }
    }
}