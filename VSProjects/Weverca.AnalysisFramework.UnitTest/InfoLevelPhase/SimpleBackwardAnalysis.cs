/*
Copyright (c) 2012-2014 Natalia Tyrpakova, David Hauzar

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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;


namespace Weverca.AnalysisFramework.UnitTest.InfoLevelPhase
{
    class SimpleBackwardAnalysis : NextPhaseAnalysis
    {
        public SimpleBackwardAnalysis(ProgramPointGraph analyzedGraph)
            : base(analyzedGraph, AnalysisDirection.Backward, new PropagationAnalyzer())
        {
        }
    }
}