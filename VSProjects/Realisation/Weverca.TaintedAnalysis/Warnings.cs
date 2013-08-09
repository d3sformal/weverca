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
    public class AnalysisWarningHandler
    {
        public static void SetWarning(FlowOutputSet flowOutSet, AnalysisWarning warning) 
        {
            IEnumerable<Value> previousWarnings = ReadWarnings(flowOutSet);
            List<Value> newEntry = new List<Value>(previousWarnings);
            newEntry.Add(flowOutSet.CreateInfo(warning));
            flowOutSet.Assign(new VariableName(".analysisWarning"), new MemoryEntry(newEntry));
        }

        public static IEnumerable<Value> ReadWarnings(FlowOutputSet flowOutSet)
        {
            var result = flowOutSet.ReadValue(new VariableName(".analysisWarning")).PossibleValues;
            if ((result.Count() == 1) && (result.ElementAt(0).GetType() == typeof(UndefinedValue)))
            {
                return new List<Value>();
            }
            else
            {
                return flowOutSet.ReadValue(new VariableName(".analysisWarning")).PossibleValues;
            }
        }
    }

    public class AnalysisWarning
    {
        public string Message { private set; get; }
        public LangElement LangElement { private set; get; }
        public AnalysisWarningCause Cause { private set; get; }
        public AnalysisWarning(string message, LangElement element)
        {
            Message = message;
            LangElement = element;
        }

        public AnalysisWarning(string message, LangElement element,AnalysisWarningCause cause)
        {
            Message = message;
            LangElement = element;
            Cause = cause;
        }

        public override string ToString()
        {
            return "Warning at line "+LangElement.Position.FirstLine+" char "+LangElement.Position.FirstColumn+": "+Message.ToString();
        }
    }

    public enum AnalysisWarningCause
    {
        WRONG_NUMBER_OF_ARGUMENTS,
        WRONG_ARGUMENTS_TYPE
    }
}
