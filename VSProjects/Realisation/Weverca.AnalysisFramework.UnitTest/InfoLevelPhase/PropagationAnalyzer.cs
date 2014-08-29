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
using Weverca.AnalysisFramework.ProgramPoints;


namespace Weverca.AnalysisFramework.UnitTest.InfoLevelPhase
{
    class PropagationAnalyzer : NextPhaseAnalyzer
    {
        public override void VisitPoint(ProgramPointBase p)
        {
            //nothing to do
        }

        public override void VisitAssign(AssignPoint p)
        {
            var source = p.LOperand.LValue;
            var targetPoint = p.ROperand as LValuePoint;

            if (source == null || targetPoint == null || targetPoint.LValue == null)
                //Variable has to be LValue
                return;

            var target = targetPoint.LValue;

            var sourcePropagation = getPropagation(source);
            var targetPropagation = getPropagation(target);

            var finalPropagation = targetPropagation.PropagateTo(sourcePropagation);

            setPropagation(target, finalPropagation);
        }

        private void setPropagation(ReadWriteSnapshotEntryBase variable, PropagationInfo propagation)
        {
            var infoValue = Output.CreateInfo(propagation);
            variable.WriteMemory(Output, new MemoryEntry(infoValue));
        }

        private PropagationInfo getPropagation(ReadSnapshotEntryBase lValue)
        {
            var variable = lValue.GetVariableIdentifier(Input);
            if (!variable.IsDirect)
            {
                throw new NotImplementedException();
            }

            var info = lValue.ReadMemory(Output);
            if (info.Count != 1)
            {
                throw new NotImplementedException();
            }

            var infoValue = info.PossibleValues.First();
            if (infoValue is UndefinedValue)
            {
                //variable hasn't been propagated nowhere already
                return new PropagationInfo(variable.DirectName.Value);
            }
            else
            {
                //we have propagation info from previous steps
                var result = ((InfoValue<PropagationInfo>)infoValue).Data;
                return result;
            }
        }
    }
}