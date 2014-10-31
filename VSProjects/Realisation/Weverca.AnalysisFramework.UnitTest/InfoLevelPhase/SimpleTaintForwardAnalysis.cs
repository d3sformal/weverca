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
    class SimpleTaintForwardAnalysis : NextPhaseAnalysis 
    {
        public SimpleTaintForwardAnalysis(ProgramPointGraph analyzedGraph)
            : base(analyzedGraph, AnalysisDirection.Forward, new TaintAnalyzer())
        {
            initialize(EntryInput);
        }

        /// <summary>
        /// Initializer which sets environment for tests before analyzing
        /// </summary>
        /// <param name="outSet"></param>
        private static void initialize(FlowOutputSet outSet)
        {
            outSet.Snapshot.SetMode(SnapshotMode.InfoLevel);
            var POSTVar = outSet.GetVariable(new VariableIdentifier("_POST"), true);
            var POST = outSet.CreateInfo(true);

            POSTVar.WriteMemory(outSet.Snapshot, new MemoryEntry(POST));

            POSTVar = outSet.GetVariable(new VariableIdentifier("_POST"), true);
            POST = outSet.CreateInfo(true);

            POSTVar.WriteMemory(outSet.Snapshot, new MemoryEntry(POST));
        }
    }
}