using System.Collections.Generic;
using System.Linq;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using System.Collections;
using System.Text;
using System;

namespace Weverca.Analysis
{

    /// <summary>
    /// Handler, which provides functionality for reading and storiny analysis warnings
    /// </summary>
    public class AnalysisWarningHandler
    {
        /// <summary>
        /// Variable where warning values are stored
        /// </summary>
        private static readonly VariableName WARNING_STORAGE = new VariableName(".analysisWarning");

        public static HashSet<AnalysisWarning> Warnings = new HashSet<AnalysisWarning>();



        /// <summary>
        /// Insert warning inte FlowOutputSet
        /// </summary>
        /// <param name="flowOutSet"></param>
        /// <param name="warning"></param>
        public static void SetWarning(FlowOutputSet flowOutSet, AnalysisWarning warning)
        {
            var previousWarnings = ReadWarnings(flowOutSet);
            var newEntry = new List<Value>(previousWarnings);
            newEntry.Add(flowOutSet.CreateInfo(warning));

           // flowOutSet.FetchFromGlobal(WARNING_STORAGE);
            var warnings=flowOutSet.GetControlVariable(WARNING_STORAGE);
            warnings.WriteMemory(flowOutSet.Snapshot, new MemoryEntry(newEntry));
            Warnings.Add(warning);
        }

        /// <summary>
        /// Read warnings from FlowOutputSet
        /// </summary>
        /// <param name="flowOutSet"></param>
        /// <returns></returns>
        public static IEnumerable<Value> ReadWarnings(FlowOutputSet flowOutSet)
        {
            //flowOutSet.FetchFromGlobal(WARNING_STORAGE);
            var result = flowOutSet.ReadControlVariable(WARNING_STORAGE).ReadMemory(flowOutSet.Snapshot).PossibleValues;
            return from value in result where !(value is UndefinedValue) select value;
        }


        public static string GetWarningsToOutput()
        {
            
            StringBuilder result=new StringBuilder();
            var warnings=Analysis.AnalysisWarningHandler.Warnings.ToArray();
            if (warnings.Count() == 0)
            {
                return "No analysis warnings.";
            }
            Array.Sort(warnings, warnings[0]);
            foreach (var warning in warnings)
            {
                result.Append(warning.ToString());
                result.Append("\n");
            }
            return result.ToString();
        }
    }

    /// <summary>
    /// Class, which contains information about analysis warning
    /// </summary>
    public class AnalysisWarning : IComparer<AnalysisWarning>
    {
        /// <summary>
        /// Warning message
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Langelement of AST, which produced the warning
        /// </summary>
        public LangElement LangElement { get; private set; }

        /// <summary>
        /// Cause of the warning(Why was the warning added)
        /// </summary>
        public AnalysisWarningCause Cause { get; private set; }

        /// <summary>
        /// Construct new instance of AnalysisWarning, without cause
        /// </summary>
        /// <param name="message">Warning message</param>
        /// <param name="element">Element, where the warning was produced</param>
        public AnalysisWarning(string message, LangElement element)
        {
            Message = message;
            LangElement = element;
        }

        /// <summary>
        /// Construct new instance of AnalysisWarning
        /// </summary>
        /// <param name="message">Warning message</param>
        /// <param name="element">Element, where the warning was produced</param>
        /// <param name="cause">Warning cause</param>
        public AnalysisWarning(string message, LangElement element, AnalysisWarningCause cause)
        {
            Message = message;
            LangElement = element;
            Cause = cause;
        }

        /// <summary>
        /// Return the warning message, with position in source code
        /// </summary>
        /// <returns>Return the warning message, with position in source code</returns>
        public override string ToString()
        {
            return "Warning at line " + LangElement.Position.FirstLine + " char " + LangElement.Position.FirstColumn + ": " + Message.ToString();
        }

        public int Compare(AnalysisWarning x, AnalysisWarning y)
        {
            if (x.LangElement.Position.FirstOffset < y.LangElement.Position.FirstOffset)
            {
                return -1;
            }
            else if (x.LangElement.Position.FirstOffset > y.LangElement.Position.FirstOffset)
            {
                return 1;
            }
            return 0;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AnalysisWarning))
            {
                return false;
            }

            AnalysisWarning other = obj as AnalysisWarning;
            if (other.Message == this.Message && other.LangElement.Position.FirstOffset == this.LangElement.Position.FirstOffset && Cause==other.Cause)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Message.GetHashCode() + LangElement.Position.FirstOffset.GetHashCode();
        }
    }

    /// <summary>
    /// Posiible warning causes, Fell free to add more.
    /// </summary>
    public enum AnalysisWarningCause
    {
        WRONG_NUMBER_OF_ARGUMENTS,
        WRONG_ARGUMENTS_TYPE,
        DIVISION_BY_ZERO,
        PROPERTY_OF_NON_OBJECT_VARIABLE,
        ELEMENT_OF_NON_ARRAY_VARIABLE,
        METHOD_CALL_ON_NON_OBJECT_VARIABLE,
        UNDEFINED_VALUE,
   
        CLASS_DOESNT_EXIST,
        CLASS_ALLREADY_EXISTS,
        FINAL_CLASS_CANNOT_BE_EXTENDED,
        CLASS_DOENST_IMPLEMENT_ALL_INTERFACE_METHODS,
        INTERFACE_DOESNT_EXIST,
        CANNOT_REDECLARE_NON_STATIC_FIELD_WITH_STATIC,
        CANNOT_REDECLARE_STATIC_FIELD_WITH_NON_STATIC,
        CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC,
        CANNOT_REDECLARE_STATIC_METHOD_WITH_NON_STATIC,
        CANNOT_REDECLARE_FINAL_METHOD,

        CANNOT_OVERRIDE_INTERFACE_CONSTANT,
        CLASS_MULTIPLE_CONST_DECLARATION,
        CLASS_MULTIPLE_FIELD_DECLARATION,
        CLASS_MULTIPLE_FUNCTION_DECLARATION,

        NON_ABSTRACT_CLASS_CONTAINS_ABSTRACT_METHOD,
        CANNOT_OVERRIDE_FUNCTION_WITH_ABSTRACT,
        CANNOT_OVERWRITE_FUNCTION,

        INTERFACE_CANNOT_CONTAIN_FIELDS,
        CANNOT_REDECLARE_INTERFACE_FUNCTION,
        INTERFACE_METHOD_MUST_BE_PUBLIC,
        INTERFACE_METHOD_CANNOT_BE_FINAL,
        INTERFACE_METHOD_CANNOT_HAVE_IMPLEMENTATION,
        ABSTRACT_METHOD_CANNOT_HAVE_BODY,
        NON_ABSTRACT_METHOD_MUST_HAVE_BODY,
        //todo abstract classes and methods

        FILE_TO_BE_INCLUDED_NOT_FOUND,

        ONLY_OBJECT_CAM_BE_THROWN,


        CLASS_CONSTANT_DOESNT_EXIST,
       CANNOT_ACCESS_CONSTANT_ON_NON_OBJECT,

    }
   
}
