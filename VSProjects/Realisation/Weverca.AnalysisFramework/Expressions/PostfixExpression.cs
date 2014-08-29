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

using PHP.Core.AST;

namespace Weverca.AnalysisFramework.Expressions
{
    /// <summary>
    /// Postfix representation of LangElement
    /// </summary>
    public class Postfix:IEnumerable<LangElement>
    {
        private List<LangElement> _elements = new List<LangElement>();

        /// <summary>
        /// Length of postfix representation
        /// </summary>
        public int Length { get { return _elements.Count; } }

        /// <summary>
        /// Represented LangElement
        /// </summary>
        public readonly LangElement SourceElement;
        
        /// <summary>
        /// Get element at specified index
        /// </summary>
        /// <param name="elementIndex">Index of element</param>
        /// <returns>Element at specified index</returns>
        public LangElement GetElement(int elementIndex)
        {
            return _elements[elementIndex];
        }

        /// <summary>
        /// Creates postfix epxression for given source element 
        /// NOTE:
        ///     expressions will be filled from outside - converter
        /// </summary>
        /// <param name="sourceElement"></param>
        internal Postfix(LangElement sourceElement)
        {
            SourceElement = sourceElement;
        }

        /// <summary>
        /// Append element into postfix representation
        /// </summary>
        /// <param name="element"></param>
        internal void Append(LangElement element)
        {
            _elements.Add(element);
        }
        
        /// <inheritdoc />
        public IEnumerator<LangElement> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        /// <inheritdoc />
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }
    }
}