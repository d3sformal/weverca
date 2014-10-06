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


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework.ProgramPoints
{
    /// <summary>
    /// Pseudo-constant use (PHP keywords: __LINE__, __FILE__, __DIR__, __FUNCTION__, __METHOD__, __CLASS__, __NAMESPACE__)
    /// </summary>
    public class PseudoConstantPoint : ValuePoint
    {
        private PseudoConstUse _partial;

        /// <inheritdoc />
        public override LangElement Partial { get { return _partial; } }

        internal PseudoConstantPoint(PseudoConstUse partial)
        {
            _partial = partial;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            switch (_partial.Type)
            {
                case PseudoConstUse.Types.Line:
                    Value = OutSet.CreateSnapshotEntry( new MemoryEntry(OutSet.AnyIntegerValue) );
                    return;
                case PseudoConstUse.Types.File:
                case PseudoConstUse.Types.Dir:
                case PseudoConstUse.Types.Function:
                case PseudoConstUse.Types.Method:
                case PseudoConstUse.Types.Class:
                case PseudoConstUse.Types.Namespace:
                    Value = OutSet.CreateSnapshotEntry(new MemoryEntry(OutSet.AnyStringValue));
                    return;
                default:
                    Value = OutSet.CreateSnapshotEntry(new MemoryEntry(OutSet.AnyValue));
                    return;
                    
            }
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitPseudoConstant(this);
        }
    }
}