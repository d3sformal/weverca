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
using PHP.Core.AST;

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
            var result = flowOutSet.ReadValue(new VariableName(".analysisWarning")).PossibleValues;
            if ((result.Count() == 1) && (result.ElementAt(0).GetType() == typeof(UndefinedValue)))
            {
                return new Value[0];
            }
            else
            {
                return flowOutSet.ReadValue(new VariableName(".analysisWarning")).PossibleValues.ToArray();
            }
        }
    }

    class AnalysisWarning
    {
        public string Message { private set; get; }
        public LangElement LangElement { private set; get; }
        public AnalysisWarning(string message, LangElement element)
        {
            Message = message;
            LangElement = element;
        }

        public override string ToString()
        {
            return Message.ToString();
        }
    }
}
