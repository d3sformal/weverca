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

        /// <summary>
        /// Variable where security warning values are stored
        /// </summary>
        private static readonly VariableName SECUTIRTY_WARNING_STORAGE = new VariableName(".analysisSecurityWarning");

        /// <summary>
        /// Stores all analysis warnings for user output
        /// </summary>
        private static HashSet<AnalysisWarning> Warnings = new HashSet<AnalysisWarning>();

        /// <summary>
        /// Stores all security warnings for user output
        /// </summary>
        private static HashSet<AnalysisSecurityWarning> SecurityWarnings = new HashSet<AnalysisSecurityWarning>();

        /// <summary>
        /// Returns name of variable for specified kind of warning
        /// </summary>
        /// <typeparam name="T">type of warning</typeparam>
        /// <returns>Returns name of variable for specified kind of warning</returns>
        private static VariableName getStorage<T>() where T : AnalysisWarning
        {
            if (typeof(T) == typeof(AnalysisWarning))
            {
                return WARNING_STORAGE;
            }
            else if (typeof(T) == typeof(AnalysisSecurityWarning))
            {
                return SECUTIRTY_WARNING_STORAGE;
            }
            else 
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Insert warning inte FlowOutputSet
        /// </summary>
        /// <param name="flowOutSet"></param>
        /// <param name="warning"></param>
        public static void SetWarning<T>(FlowOutputSet flowOutSet, T warning) where T : AnalysisWarning
        {
            var previousWarnings = ReadWarnings<T>(flowOutSet);
            var newEntry = new List<Value>(previousWarnings);
            newEntry.Add(flowOutSet.CreateInfo(warning));

            var warnings = flowOutSet.GetControlVariable(getStorage<T>());
            warnings.WriteMemory(flowOutSet.Snapshot, new MemoryEntry(newEntry));
            if (typeof(T) == typeof(AnalysisWarning))
            {
                Warnings.Add(warning);
            }
            if (typeof(T) == typeof(AnalysisSecurityWarning))
            {
                SecurityWarnings.Add(warning as AnalysisSecurityWarning);
            }
        }

        /// <summary>
        /// Read warnings from FlowOutputSet
        /// </summary>
        /// <param name="flowOutSet"></param>
        /// <returns></returns>
        public static IEnumerable<Value> ReadWarnings<T>(FlowOutputSet flowOutSet) where T : AnalysisWarning
        {
            //flowOutSet.FetchFromGlobal(WARNING_STORAGE);
            var result = flowOutSet.ReadControlVariable(getStorage<T>()).ReadMemory(flowOutSet.Snapshot).PossibleValues;
            return from value in result where !(value is UndefinedValue) select value;
        }

        /// <summary>
        /// Returns sorted list of analysis warnings
        /// </summary>
        /// <returns> Returns sorted list of analysis warnings</returns>
        public static List<AnalysisWarning> GetWarnings()
        {
            var arr=Warnings.ToArray();
            Array.Sort(arr);
            return new List<AnalysisWarning>(arr);
        }

        /// <summary>
        /// Returns sorted list of analysis warnings
        /// </summary>
        /// <returns> Returns sorted list of security warnings</returns>
        public static List<AnalysisSecurityWarning> GetSecurityWarnings()
        {
            var arr = SecurityWarnings.ToArray();
            Array.Sort(arr);
            return new List<AnalysisSecurityWarning>(arr);
        }
    }

    /// <summary>
    /// Class, which contains information about analysis warning
    /// </summary>
    public class AnalysisWarning : IComparable<AnalysisWarning>, IEquatable<AnalysisWarning>
    {
        /// <summary>
        /// Warning message
        /// </summary>
        public string Message { get; protected set; }

        /// <summary>
        /// Langelement of AST, which produced the warning
        /// </summary>
        public LangElement LangElement { get; protected set; }

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

        protected AnalysisWarning()
        { 
        
        }

        /// <summary>
        /// Return the warning message, with position in source code
        /// </summary>
        /// <returns>Return the warning message, with position in source code</returns>
        public override string ToString()
        {
            return "Warning at line " + LangElement.Position.FirstLine + " char " + LangElement.Position.FirstColumn + ": " + Message.ToString();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Message.GetHashCode() + LangElement.Position.FirstOffset.GetHashCode() + Cause.GetHashCode();
        }

        /// <summary>
        /// Comparing function for sorting warning acoriding to line numbers
        /// </summary>
        /// <param name="other">Other warning</param>
        /// <returns>0 if they are the same, -1 or 1 if one has other position</returns>
        public int CompareTo(AnalysisWarning other)
        {
            if (this.LangElement.Position.FirstOffset < other.LangElement.Position.FirstOffset)
            {
                return -1;
            }
            else if (this.LangElement.Position.FirstOffset > other.LangElement.Position.FirstOffset)
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Compares warnings based of message, element a warning cause
        /// </summary>
        /// <param name="other">other warning</param>
        /// <returns>true, if they are the same, false otherwise</returns>
        public bool Equals(AnalysisWarning other)
        {
            if (other.Message == this.Message && other.LangElement.Position.FirstOffset == this.LangElement.Position.FirstOffset && Cause == other.Cause)
            {
                return true;
            }
            return false;
        }

       
    }

    /// <summary>
    /// Special type of analysis warnings
    /// </summary>
    public class AnalysisSecurityWarning : AnalysisWarning, IComparable<AnalysisSecurityWarning>, IEquatable<AnalysisSecurityWarning>
    {   
        /// <summary>
        /// Type of flag which triggered the warning
        /// </summary>
        public FlagType  Flag { get; protected set; }

        /// <summary>
        /// Construct new instance of AnalysisSecurityWarning
        /// </summary>
        /// <param name="message">Warning message</param>
        /// <param name="element">>Element, where the warning was produced</param>
        /// <param name="cause">Flag type</param>
        public AnalysisSecurityWarning(string message, LangElement element, FlagType cause)
        {
            Message = message;
            LangElement = element;
            Flag = cause;
        }

        /// <summary>
        /// Construct new instance of AnalysisSecurityWarning,message will be generated automaticly
        /// </summary>
        /// <param name="element">>Element, where the warning was produced</param>
        /// <param name="cause">Flag type</param>
        public AnalysisSecurityWarning(LangElement element, FlagType cause)
        {
            switch (cause)
            { 
                case FlagType.HTMLDirty:
                    Message="Unchecked value goes into browser";
                    break;
                case FlagType.FilePathDirty:
                    Message = "File name has to be checked before open";
                    break;
                case FlagType.SQLDirty:
                    Message="Unchecked value goes into database";
                    break;
            }

            LangElement = element;
            Flag = cause;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Message.GetHashCode() + LangElement.Position.FirstOffset.GetHashCode() + Flag.GetHashCode();
        }

        /// <summary>
        /// Comparing function for sorting warning acoriding to line numbers
        /// </summary>
        /// <param name="other">Other warning</param>
        /// <returns>0 if they are the same, -1 or 1 if one has other position</returns>
        public int CompareTo(AnalysisSecurityWarning other)
        {
            if (this.LangElement.Position.FirstOffset < other.LangElement.Position.FirstOffset)
            {
                return -1;
            }
            else if (this.LangElement.Position.FirstOffset > other.LangElement.Position.FirstOffset)
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Compares warnings based of message, element a warning cause
        /// </summary>
        /// <param name="other">other warning</param>
        /// <returns>true, if they are the same, false otherwise</returns>
        public bool Equals(AnalysisSecurityWarning other)
        {
            if (other.Message == this.Message && other.LangElement.Position.FirstOffset == this.LangElement.Position.FirstOffset && Flag == other.Flag)
            {
                return true;
            }
            return false;
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

        FILE_TO_BE_INCLUDED_NOT_FOUND,

        ONLY_OBJECT_CAM_BE_THROWN,
        CANNOT_ACCCES_SELF_WHEN_NOT_IN_CLASS,
        CANNOT_ACCCES_PARENT_WHEN_NOT_IN_CLASS,
        CANNOT_ACCCES_PARENT_CURRENT_CLASS_HAS_NO_PARENT,
        
        CLASS_CONSTANT_DOESNT_EXIST,
        CANNOT_ACCESS_CONSTANT_ON_NON_OBJECT,

        CANNOT_INSTANCIATE_ABSTRACT_CLASS,
        CANNOT_INSTANCIATE_INTERFACE,


    }
   
}
