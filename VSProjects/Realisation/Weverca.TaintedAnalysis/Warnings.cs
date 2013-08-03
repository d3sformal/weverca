using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Analysis;
using PHP.Core;
using Weverca.Analysis.Memory;
using Weverca.Parsers;

using PHP.Core.Parsers;

namespace Weverca.TaintedAnalysis
{
    class AnalysisWarningHandler
    {
        public static void SetWarning(FlowOutputSet flowOutSet, AnalysisWarning warning) 
        {
            List<Value> previousWarnings = new List<Value>(ReadWarnings(flowOutSet));
            previousWarnings.Add(flowOutSet.CreateInfo(warning));
            flowOutSet.Assign(new VariableName(".analysisWarning"), new MemoryEntry(previousWarnings.ToArray()));
        }

        public static Value[] ReadWarnings(FlowOutputSet flowOutSet)
        {
            return flowOutSet.ReadValue(new VariableName(".analysisWarning")).PossibleValues.ToArray();
        }
    }

    class AnalysisWarning
    {
        public string Message { private set; get; }
        public Position Position { private set; get; }
        public AnalysisWarning(string message, Position postion)
        {
            Message = message;
            postion = Position;
        }

        public override string ToString()
        {
            return Message.ToString();
        }
    }
}
