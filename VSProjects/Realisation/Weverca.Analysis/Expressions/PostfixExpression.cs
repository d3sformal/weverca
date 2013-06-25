﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.Analysis.Expressions
{
    public class Postfix
    {
        List<LangElement> _items = new List<LangElement>();

        public int Length { get { return _items.Count; } }

        public readonly LangElement SourceElement;

        internal Postfix(LangElement sourceElement)
        {
            SourceElement = sourceElement;
        }

        public LangElement GetElement(int i)
        {
            return _items[i];
        }

        internal void Add(LangElement expressionItem)
        {
            _items.Add(expressionItem);
        }
    }
}