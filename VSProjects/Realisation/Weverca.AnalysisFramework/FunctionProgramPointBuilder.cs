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

using System.IO;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework
{
    class FunctionProgramPointBuilder : AbstractValueVisitor
    {
        internal ProgramPointGraph Output;

        public override void VisitValue(Value value)
        {
            throw new NotSupportedException("Creating program point from given value is not supported");
        }

        public override void VisitFunctionValue(FunctionValue value)
        {
            throw new NotSupportedException("Building progrma point from given function value is not supported yet");
        }

        public override void VisitSourceFunctionValue(SourceFunctionValue value)
        {
            Output = ProgramPointGraph.FromSource(value.Declaration, value.DeclaringScript);
        }

        public override void VisitSourceMethodValue(SourceMethodValue value)
        {
            Output = ProgramPointGraph.FromSource(value.Declaration, value.DeclaringScript);
        }

        public override void VisitNativeAnalyzerValue(NativeAnalyzerValue value)
        {
            Output = ProgramPointGraph.FromNative(value.Analyzer);
        }
    }
}