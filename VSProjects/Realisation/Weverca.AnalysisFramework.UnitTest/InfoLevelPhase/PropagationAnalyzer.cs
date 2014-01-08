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
