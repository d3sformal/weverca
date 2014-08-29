/*
Copyright (c) 2012-2014 Natalia Tyrpakova and David Hauzar

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

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;
using Weverca.Analysis;

namespace Weverca.Taint
{
    public class TaintForwardAnalysis : NextPhaseAnalysis
    {
        public List<AnalysisTaintWarning> analysisTaintWarnings;
      //  private TaintAnalyzer

        public TaintForwardAnalysis(ProgramPointGraph analyzedGraph)
            : base(analyzedGraph, AnalysisDirection.Forward, new TaintAnalyzer())
        {
            TaintInitializer();
        }

        public override void Analyse()
        {
            base.Analyse();
            NextPhaseAnalyzer analyzer = getNextPhaseAnalyzer();
            analysisTaintWarnings = (analyzer as TaintAnalyzer).analysisTaintWarnings;
        }

        private void TaintInitializer()
        {
            FlowOutputSet outSet = this.EntryInput;
            initTaintedVariable(outSet,"_POST");
            initTaintedVariable(outSet,"_GET");
            initTaintedVariable(outSet,"_SERVER");
            initTaintedVariable(outSet,"_COOKIE");
            initTaintedVariable(outSet,"_SESSION");
            initTaintedVariable(outSet,"_FILES");
            initTaintedVariable(outSet,"_REQUEST");
            initTaintedVariable(outSet,"GLOBALS");
        }

        private void initTaintedVariable(FlowOutputSet outSet, String name)
        {  
            outSet.Snapshot.SetMode(SnapshotMode.InfoLevel);

            var TaintedVar = outSet.GetVariable(new VariableIdentifier(name), true);

            TaintInfo taint = new TaintInfo();
             taint.taint = new Taint(true);
            taint.priority = new TaintPriority(true);
            taint.tainted = true;

            var Taint = outSet.CreateInfo(taint);

            TaintedVar.WriteMemory(outSet.Snapshot, new MemoryEntry(Taint));
			/*
            TaintedVar = outSet.GetVariable(new VariableIdentifier(name), true);
            Taint = outSet.CreateInfo(taint);

            TaintedVar.WriteMemory(outSet.Snapshot, new MemoryEntry(Taint));*/
        }		
    }
}