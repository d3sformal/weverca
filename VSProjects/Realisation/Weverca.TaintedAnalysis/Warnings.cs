using System.Collections.Generic;
using System.Linq;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis;
using Weverca.Analysis.Memory;

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
        public string Message { get; private set; }
        public LangElement LangElement { get; private set; }
        public AnalysisWarningCause Cause { get; private set; }

        public AnalysisWarning(string message, LangElement element)
        {
            Message = message;
            LangElement = element;
        }

        public AnalysisWarning(string message, LangElement element, AnalysisWarningCause cause)
        {
            Message = message;
            LangElement = element;
            Cause = cause;
        }

        public override string ToString()
        {
            return "Warning at line " + LangElement.Position.FirstLine + " char " + LangElement.Position.FirstColumn + ": " + Message.ToString();
        }
    }

    public enum AnalysisWarningCause
    {
        WRONG_NUMBER_OF_ARGUMENTS,
        WRONG_ARGUMENTS_TYPE,
        PROPERTY_OF_NON_OBJECT_VARIABLE,
        UNDEFINED_VALUE,
    }
}
