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
        /// <summary>
        /// Variable where warning values are stored
        /// </summary>
        private static readonly VariableName WARNING_STORAGE = new VariableName(".analysisWarning");

        public static void SetWarning(FlowOutputSet flowOutSet, AnalysisWarning warning)
        {
            var previousWarnings = ReadWarnings(flowOutSet);
            var newEntry = new List<Value>(previousWarnings);
            newEntry.Add(flowOutSet.CreateInfo(warning));

            flowOutSet.FetchFromGlobal(WARNING_STORAGE);
            flowOutSet.Assign(WARNING_STORAGE, new MemoryEntry(newEntry));
        }

        public static IEnumerable<Value> ReadWarnings(FlowOutputSet flowOutSet)
        {
            flowOutSet.FetchFromGlobal(WARNING_STORAGE);
            var result = flowOutSet.ReadValue(WARNING_STORAGE).PossibleValues;
            return from value in result where !(value is UndefinedValue) select value;
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
        DIVISION_BY_ZERO,
        PROPERTY_OF_NON_OBJECT_VARIABLE,
        METHOD_CALL_ON_NON_OBJECT_VARIABLE,
        UNDEFINED_VALUE,
    }
}
