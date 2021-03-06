﻿/*
Copyright (c) 2012-2014 Marcel Kikta

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

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis
{
	/// <summary>
	/// Handler which provides functionality for reading and storing analysis warnings
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
		private static readonly VariableName SECUTIRTY_WARNING_STORAGE
			= new VariableName(".analysisSecurityWarning");

		/// <summary>
		/// Variable where tainted variable warning values are stored
		/// </summary>
		private static readonly VariableName TAINTED_VARIABLE_WARNING_STORAGE
		   = new VariableName(".analysisTaintWarning");

		/// <summary>
		/// Stores all analysis warnings for user output
		/// </summary>
		private static HashSet<AnalysisWarning> Warnings = new HashSet<AnalysisWarning>();

		/// <summary>
		/// Stores all security warnings for user output
		/// </summary>
		private static HashSet<AnalysisSecurityWarning> SecurityWarnings
			= new HashSet<AnalysisSecurityWarning>();

		/// <summary>
		/// Stores all tainted variable warnings for user output
		/// </summary>
		private static HashSet<AnalysisTaintWarning> TaintWarnings
			= new HashSet<AnalysisTaintWarning>();

		/// <summary>
		/// Gets the number ofanalysis and security warnings.
		/// </summary>
		public static int NumberOfWarnings
		{
			get { return Warnings.Count + SecurityWarnings.Count; }
		}

		/// <summary>
		/// Returns name of variable for specified kind of warning
		/// </summary>
		/// <typeparam name="T">Type of warning</typeparam>
		/// <returns>Name of variable for specified kind of warning</returns>
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
			else if (typeof(T) == typeof(AnalysisTaintWarning))
			{
				return TAINTED_VARIABLE_WARNING_STORAGE;
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Deletes all registered warnings.
		/// </summary>
		public static void ResetWarnings()
		{
			SecurityWarnings.Clear();
			Warnings.Clear();
			TaintWarnings.Clear();
		}

		/// <summary>
		/// Insert warning into <see cref="FlowOutputSet" />
		/// </summary>
		/// <typeparam name="T">The type of the warning</typeparam>
		/// <param name="flowOutSet">The flow out set.</param>
		/// <param name="warning">The warning.</param>
		public static void SetWarning<T>(FlowOutputSet flowOutSet, T warning) where T : AnalysisWarning
		{
			SetWarning<T>(flowOutSet.Snapshot, warning);
		}

		/// <summary>
		/// Insert warning into <see cref="FlowOutputSet" />
		/// </summary>
		/// <typeparam name="T">The type of the warning</typeparam>
		/// <param name="flowOutSet">The flow out set.</param>
		/// <param name="warning">The warning.</param>
		public static void SetWarning<T>(SnapshotBase flowOutSet, T warning) where T : AnalysisWarning
		{
			if (warning.LangElement == null)
				return;
			var previousWarnings = ReadWarnings<T>(flowOutSet);
			var newEntry = new HashSet<Value>(previousWarnings);
			newEntry.Add(flowOutSet.CreateInfo(warning));

			var warnings = flowOutSet.GetControlVariable(getStorage<T>());
			warnings.WriteMemory(flowOutSet, new MemoryEntry(newEntry));

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
		/// Read warnings from <see cref="FlowOutputSet" />
		/// </summary>
		/// <typeparam name="T">The type of read warnings</typeparam>
		/// <param name="flowOutSet">The flow out set.</param>
		/// <returns>List of warnings read</returns>
		public static IEnumerable<Value> ReadWarnings<T>(FlowOutputSet flowOutSet)
			where T : AnalysisWarning
		{
			return ReadWarnings<T>(flowOutSet.Snapshot);
		}

		/// <summary>
		/// Read warnings from <see cref="FlowOutputSet" />
		/// </summary>
		/// <typeparam name="T">The type of read warnings</typeparam>
		/// <param name="flowOutSet">The flow out set.</param>
		/// <returns> List of warnings read </returns>
		public static IEnumerable<Value> ReadWarnings<T>(SnapshotBase flowOutSet)
			where T : AnalysisWarning
		{
			var snapshotEntry = flowOutSet.ReadControlVariable(getStorage<T>());
			var resultEntry = snapshotEntry.ReadMemory(flowOutSet);
			var result = resultEntry.PossibleValues;
			return from value in result where !(value is UndefinedValue) select value;
		}

		/// <summary>
		/// Returns sorted list of analysis warnings
		/// </summary>
		/// <returns>Sorted list of analysis warnings</returns>
		public static List<AnalysisWarning> GetWarnings()
		{
			var arr = Warnings.ToArray();
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

		/// <summary>
		/// Returns sorted list of tainted variable warnings
		/// </summary>
		/// <returns> Returns sorted list of tainted variable warnings</returns>
		public static List<AnalysisTaintWarning> GetTaintWarnings()
		{
			var arr = TaintWarnings.ToArray();
			Array.Sort(arr);
			return new List<AnalysisTaintWarning>(arr);
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
		/// <see cref="LangElement"/> of AST which produced the warning
		/// </summary>
		public LangElement LangElement { get; protected set; }

		/// <summary>
		/// Cause of the warning(Why was the warning added)
		/// </summary>
		public AnalysisWarningCause Cause { get; private set; }

		/// <summary>
		/// Full name of source code file
		/// </summary>
		public string FullFileName { get; protected set; }

		/// <summary>
		/// The program point which produced the warning.
		/// </summary>
		public readonly ProgramPointBase ProgramPoint;

		/// <summary>
		/// Construct new instance of AnalysisWarning, without cause
		/// </summary>
		/// <param name="fullFileName">Full name of source code file</param>
		/// <param name="message">Warning message</param>
		/// <param name="element">Element, where the warning was produced</param>
		/// <param name="programPoint">The program point, where the warning was produced</param>
		public AnalysisWarning(string fullFileName, string message, LangElement element, ProgramPointBase programPoint)
		{
			Message = message;
			LangElement = element;
			ProgramPoint = programPoint;
			FullFileName = fullFileName;
		}

		/// <summary>
		/// Construct new instance of AnalysisWarning
		/// </summary>
		/// <param name="fullFileName">Full name of source code file</param>
		/// <param name="message">Warning message</param>
		/// <param name="element">Element, where the warning was produced</param>
		/// <param name="programPoint">The program point, where the warning was produced</param>
		/// <param name="cause">Warning cause</param>
		public AnalysisWarning(string fullFileName, string message, LangElement element, ProgramPointBase programPoint, AnalysisWarningCause cause)
		{
			Message = message;
			LangElement = element;
			ProgramPoint = programPoint;
			Cause = cause;
			FullFileName = fullFileName;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AnalysisWarning" /> class.
		/// </summary>
		protected AnalysisWarning(ProgramPointBase programPoint)
		{
			ProgramPoint = programPoint;
		}

		/// <summary>
		/// Return the warning message, with position in source code
		/// </summary>
		/// <returns>The warning message, with position in source code</returns>
		public override string ToString()
		{
			return "Warning at line " + LangElement.Position.FirstLine + " char "
				+ LangElement.Position.FirstColumn + ": " + Message.ToString();
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return Message.GetHashCode () + LangElement.Position.FirstOffset.GetHashCode () + FullFileName.GetHashCode ()
			+ Cause.GetHashCode ();
			//+ ProgramPoint.OwningPPGraph.GetHashCode();
		}

		/// <summary>
		/// Compares this instance with another one.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns><c>-1</c> if ofset of other is greated then thisone's; <c>0</c> if the offsest are the same; <c>1</c> otherwise</returns>
		protected int compareTo(AnalysisWarning other)
		{
			if (this.FullFileName == other.FullFileName)
			{
				if (this.LangElement.Position.FirstOffset < other.LangElement.Position.FirstOffset)
				{
					return -1;
				}
				else if (this.LangElement.Position.FirstOffset > other.LangElement.Position.FirstOffset)
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}
			else
			{
				return string.Compare(this.FullFileName, other.FullFileName);
			}

		}

		/// <summary>
		/// Comparing function for sorting warning according to line numbers
		/// </summary>
		/// <param name="other">Other warning</param>
		/// <returns>0 if they are the same, -1 or 1 if one has other position</returns>
		public int CompareTo(AnalysisWarning other)
		{
			return compareTo(other);
		}

		/// <summary>
		/// Compares warnings based of message, element a warning cause
		/// </summary>
		/// <param name="other">Other warning</param>
		/// <returns><c>true</c>, if they are the same, <c>false</c> otherwise</returns>
		public bool Equals(AnalysisWarning other)
		{
			return (other.Message == Message)
				&& (other.LangElement.Position.FirstOffset == LangElement.Position.FirstOffset)
				&& this.FullFileName == other.FullFileName
				&& (other.Cause == Cause);
		}
	}

	/// <summary>
	/// Special type of analysis warnings
	/// </summary>
	public class AnalysisSecurityWarning : AnalysisWarning, IComparable<AnalysisSecurityWarning>,
		IEquatable<AnalysisSecurityWarning>
	{
		/// <summary>
		/// Type of flag which triggered the warning
		/// </summary>
		public FlagType Flag { get; protected set; }

		/// <summary>
		/// Name of the variable being tainted
		/// </summary>
		public string TaintedVarName { get; protected set; }

		/// <summary>
		/// Construct new instance of <see cref="AnalysisSecurityWarning"/>
		/// </summary>
		/// <param name="fullFileName">Full name of source code file</param>
		/// <param name="message">Warning message</param>
		/// <param name="element">Element, where the warning was produced</param>
		/// <param name="programPoint">The program point, where the warning was produced</param>
		/// <param name="cause">Flag type</param>
		public AnalysisSecurityWarning(string fullFileName, string message, LangElement element, ProgramPointBase programPoint, FlagType cause):
			base(programPoint)
		{
			FullFileName = fullFileName;
			Message = message;
			LangElement = element;
			Flag = cause;
		}

		/// <summary>
		/// Construct new instance of <see cref="AnalysisSecurityWarning"/>, message will be generated automatically
		/// </summary>
		/// <param name="fullFileName">Full name of source code file</param>
		/// <param name="element">Element, where the warning was produced</param>
		/// <param name="programPoint">The program point, where the warning was produced</param>
		/// <param name="cause">Flag type</param>
		public AnalysisSecurityWarning(string fullFileName, LangElement element, ProgramPointBase programPoint, FlagType cause, string taintedVar):
			base(programPoint)
		{
			FullFileName = fullFileName;
			switch (cause)
			{
				case FlagType.HTMLDirty:
					Message = "Unsanitized value " + (taintedVar != "" ? "(" + taintedVar + ") " : "") + "goes into browser";
					break;
				case FlagType.FilePathDirty:
					Message = "Unsanitized value " + (taintedVar != "" ? "(" + taintedVar + ") " : "") + "used as a filename when opening a file";
					break;
				case FlagType.SQLDirty:
					Message = "Unsanitized value " + (taintedVar != "" ? "(" + taintedVar + ") " : "") + "goes into database";
					break;
			}

			LangElement = element;
			Flag = cause;
			TaintedVarName = taintedVar;
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return Message.GetHashCode() + LangElement.Position.FirstOffset.GetHashCode() + FullFileName.GetHashCode()
				+ Flag.GetHashCode() + ProgramPoint.OwningPPGraph.GetHashCode();
		}

		/// <inheritdoc />
		public int CompareTo(AnalysisSecurityWarning other)
		{
			return compareTo(other);
		}

		/// <summary>
		/// Compares warnings based of message, element a warning cause
		/// </summary>
		/// <param name="other">Other warning</param>
		/// <returns><c>true</c>, if they are the same, <c>false</c> otherwise</returns>
		public bool Equals(AnalysisSecurityWarning other)
		{
			return (other.Message == Message)
				&& (other.LangElement.Position.FirstOffset == LangElement.Position.FirstOffset)
				&& other.FullFileName == this.FullFileName
				&& (other.Flag == Flag);
		}
	}

	/// <summary>
	/// Special type of analysis warnings
	/// </summary>
	public class AnalysisTaintWarning : AnalysisWarning, IComparable<AnalysisTaintWarning>,
		IEquatable<AnalysisTaintWarning>
	{
		/// <summary>
		/// Type of flag which triggered the warning
		/// </summary>
		public FlagType Flag { get; protected set; }
		public String TaintFlow;
		public bool HighPriority;


		/// <summary>
		/// Construct new instance of <see cref="AnalysisTaintWarning"/>, message will be generated automatically
		/// </summary>
		/// <param name="fullFileName">Full name of source code file</param>
		/// <param name="taintFlow">Taint flow</param>
		/// <param name="priority">Indicator of high taint flow priority</param>
		/// <param name="element">Element, where the warning was produced</param>
		/// <param name="programPoint">The program point, where the warning was produced</param>
		/// <param name="cause">Flag type</param>
		/// <param name="nullFlow">Null flow indicator</param>
		public AnalysisTaintWarning(string fullFileName, string taintFlow, bool priority, LangElement element, ProgramPointBase programPoint, FlagType cause, Boolean nullFlow):
			base(programPoint)
		{
			FullFileName = fullFileName;
			switch (cause)
			{
				case FlagType.HTMLDirty:
					if (nullFlow) Message = "Null value goes into browser";
					else Message = "Unsanitized value goes into browser";
					break;
				case FlagType.FilePathDirty:
					if (nullFlow) Message = "Null value used when opening a file";
				else Message = "Unsanitized value used as a file name when opening a file";
					break;
				case FlagType.SQLDirty:
					if (nullFlow) Message = "Null value goes into database";
					else Message = "Unsanitized value goes into database";
					break;
			}

			TaintFlow = taintFlow;
			HighPriority = priority;
			LangElement = element;
			Flag = cause;
		}

		/// <summary>
		/// Construct new instance of <see cref="AnalysisTaintWarning"/>
		/// </summary>
		/// <param name="fullFileName">Full name of source code file</param>
		/// <param name="message">Warning message</param>
		/// <param name="taintFlow">Taint flow</param>
		/// <param name="priority">Indicator of high taint flow priority</param>
		/// <param name="element">Element, where the warning was produced</param>
		/// <param name="programPoint">The program point, where the warning was produced</param>
		/// <param name="cause">Flag type</param>
		public AnalysisTaintWarning(string fullFileName, string message, string taintFlow, bool priority, LangElement element, ProgramPointBase programPoint, FlagType cause) :
			base(programPoint)
		{
			FullFileName = fullFileName;
			Message = message;
			TaintFlow = taintFlow;
			HighPriority = priority;
			LangElement = element;
			Flag = cause;
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return Message.GetHashCode() + LangElement.Position.FirstOffset.GetHashCode() + FullFileName.GetHashCode()
				+ Flag.GetHashCode() + ProgramPoint.GetHashCode();
		}

		/// <inheritdoc />
		public int CompareTo(AnalysisTaintWarning other)
		{
			return compareTo(other);
		}

		/// <summary>
		/// Compares warnings based of message, element a warning cause
		/// </summary>
		/// <param name="other">Other warning</param>
		/// <returns><c>true</c>, if they are the same, <c>false</c> otherwise</returns>
		public bool Equals(AnalysisTaintWarning other)
		{
			return (other.Message == Message)
				&& (other.LangElement.Position.FirstOffset == LangElement.Position.FirstOffset)
				&& other.FullFileName == this.FullFileName
				&& (other.Flag == Flag)
				&& (other.ProgramPoint == ProgramPoint);
		}

	}

	/// <summary>
	/// Possible warning causes, feel free to add more.
	/// </summary>
	public enum AnalysisWarningCause
	{
		/// <summary>
		/// Warning, that occurs when called function has wrong number of arguments
		/// </summary>
		WRONG_NUMBER_OF_ARGUMENTS,

		/// <summary>
		/// Warning, that occurs during type modeling, and some arguments doesnt have expected type
		/// </summary>
		WRONG_ARGUMENTS_TYPE,

		/// <summary>
		/// Warning, that occurs when there is possible divisition by zero
		/// </summary>
		DIVISION_BY_ZERO,

		/// <summary>
		/// Warning, that occurs when object is trying to be converted to integer 
		/// </summary>
		OBJECT_CONVERTED_TO_INTEGER,

		/// <summary>
		/// Warning, that occurs when accesing property on non object variable
		/// </summary>
		PROPERTY_OF_NON_OBJECT_VARIABLE,

		/// <summary>
		/// Warning, that occurs when indexing non array variable
		/// </summary>
		ELEMENT_OF_NON_ARRAY_VARIABLE,

		/// <summary>
		/// Warning, that occurs when trying to call method on non object variable
		/// </summary>
		METHOD_CALL_ON_NON_OBJECT_VARIABLE,

		/// <summary>
		/// Warning, that occurs when trying to work with undefined values
		/// </summary>
		UNDEFINED_VALUE,

		/// <summary>
		/// Warning, that occurs when class doesnt exist
		/// </summary>
		CLASS_DOESNT_EXIST,

		/// <summary>
		/// Warning, that occurs when class allready exists
		/// </summary>
		CLASS_ALLREADY_EXISTS,

		/// <summary>
		/// Warning, that occurs when final class is tried to be extended
		/// </summary>
		FINAL_CLASS_CANNOT_BE_EXTENDED,

		/// <summary>
		/// Warning, that occurs when class doenst implement all inteface methods
		/// </summary>
		CLASS_DOENST_IMPLEMENT_ALL_INTERFACE_METHODS,

		/// <summary>
		/// Warning, that occurs when requested interface doesnt exist
		/// </summary>
		INTERFACE_DOESNT_EXIST,

		/// <summary>
		/// Warning, that occurs when trying to redeclare non static field with static
		/// </summary>
		CANNOT_REDECLARE_NON_STATIC_FIELD_WITH_STATIC,

		/// <summary>
		/// Warning, that occurs when trying to redeclare static field with non static
		/// </summary>
		CANNOT_REDECLARE_STATIC_FIELD_WITH_NON_STATIC,

		/// <summary>
		/// Warning, that occurs when trying to redeclare non static method with static
		/// </summary>
		CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC,

		/// <summary>
		/// Warning, that occurs when trying to redeclare static method with non static
		/// </summary>
		CANNOT_REDECLARE_STATIC_METHOD_WITH_NON_STATIC,

		/// <summary>
		/// Warning, that occurs when final method is beeing overidden
		/// </summary>
		CANNOT_REDECLARE_FINAL_METHOD,

		/// <summary>
		/// Warning, that occurs when interface constant is beeing overridden
		/// </summary>
		CANNOT_OVERRIDE_INTERFACE_CONSTANT,

		/// <summary>
		///  Warning, that occurs when constant is declared in the same class more than once
		/// </summary>
		CLASS_MULTIPLE_CONST_DECLARATION,

		/// <summary>
		///  Warning, that occurs when field is declared in the same class more than once
		/// </summary>
		CLASS_MULTIPLE_FIELD_DECLARATION,

		/// <summary>
		///  Warning, that occurs when method is declared in the same class more than once
		/// </summary>
		CLASS_MULTIPLE_FUNCTION_DECLARATION,

		/// <summary>
		///  Warning, that occurs when non abstract class contains abstract method
		/// </summary>
		NON_ABSTRACT_CLASS_CONTAINS_ABSTRACT_METHOD,

		/// <summary>
		/// Warning, that occurs when method is beeing overidden by abstract method
		/// </summary>
		CANNOT_OVERRIDE_FUNCTION_WITH_ABSTRACT,

		/// <summary>
		/// Warning, that occurs when method is beeing overidden by method that doesn't have the same argument types 
		/// </summary>
		CANNOT_OVERWRITE_FUNCTION,

		/// <summary>
		/// Warning, that occurs when interface contains fields
		/// </summary>
		INTERFACE_CANNOT_CONTAIN_FIELDS,

		/// <summary>
		/// Warning, that occurs when interface method is beeing overidden by method that doesn't have the same argument types 
		/// </summary>
		CANNOT_REDECLARE_INTERFACE_FUNCTION,

		/// <summary>
		/// Warning, that occurs when interface method is not public
		/// </summary>
		INTERFACE_METHOD_MUST_BE_PUBLIC,

		/// <summary>
		/// Warning, that occurs when interface method is final
		/// </summary>
		INTERFACE_METHOD_CANNOT_BE_FINAL,

		/// <summary>
		/// Warning, that occurs when interface method has impelementation
		/// </summary>
		INTERFACE_METHOD_CANNOT_HAVE_IMPLEMENTATION,

		/// <summary>
		/// Warning, that occurs when abstract method have body
		/// </summary>
		ABSTRACT_METHOD_CANNOT_HAVE_BODY,

		/// <summary>
		/// Warning, that occurs when non-abstract method doesn't have body
		/// </summary>
		NON_ABSTRACT_METHOD_MUST_HAVE_BODY,

		/// <summary>
		/// Warning, that occurs when included file is not found
		/// </summary>
		FILE_TO_BE_INCLUDED_NOT_FOUND,

		/// <summary>
		/// Warning, that occurs when include_once or require_once is called more times with the same file
		/// </summary>
		INCLUDE_REQUIRE_ONCE_CALLED_MORE_TIMES_WITH_SAME_FILE,

		/// <summary>
		/// Warning, that occurs when non object variable is beeing thrown 
		/// </summary>
		ONLY_OBJECT_CAM_BE_THROWN,

		/// <summary>
		///  Warning, that occurs when keyword self is used outside of method
		/// </summary>
		CANNOT_ACCCES_SELF_WHEN_NOT_IN_CLASS,

		/// <summary>
		///  Warning, that occurs when keyword parent is used outside of method
		/// </summary>
		CANNOT_ACCCES_PARENT_WHEN_NOT_IN_CLASS,

		/// <summary>
		///  Warning, that occurs when keyword parent is used in class with no parent class
		/// </summary>
		CANNOT_ACCCES_PARENT_CURRENT_CLASS_HAS_NO_PARENT,

		/// <summary>
		///  Warning, that occurs when class constant doesn't exist 
		/// </summary>
		CLASS_CONSTANT_DOESNT_EXIST,

		/// <summary>
		///  Warning, that occurs when trying to access object constant on non object variable
		/// </summary>
		CANNOT_ACCESS_CONSTANT_ON_NON_OBJECT,

		/// <summary>
		/// Warning, that occurs when abstract class is beeing instaciated
		/// </summary>
		CANNOT_INSTANCIATE_ABSTRACT_CLASS,

		/// <summary>
		/// Warning, that occurs when interface is beeing instaciated
		/// </summary>
		CANNOT_INSTANCIATE_INTERFACE,

		/// <summary>
		/// Warning, that occurs when static variable doesn't exist
		/// </summary>
		STATIC_VARIABLE_DOESNT_EXIST,

		/// <summary>
		/// Warning, that occurs when static variable is accessed on non object variable
		/// </summary>
		CANNOT_ACCES_STATIC_VARIABLE_ON_NON_OBJECT,

		/// <summary>
		/// Warning, that occurs when calling method without body
		/// </summary>
		CANNOT_CALL_METHOD_WITHOUT_BODY,

		/// <summary>
		/// Warning, that occurs when using operator [] on non array or string
		/// </summary>
		CANNOT_ACCESS_FIELD_OPERATOR_ON_NON_ARRAY,

		/// <summary>
		/// Warning, that occurs when using operator -> on non object
		/// </summary>
		CANNOT_ACCESS_OBJECT_OPERATOR_ON_NON_OBJECT,

		/// <summary>
		/// Warning, that occurs when using operator [] and index is out of range
		/// </summary>
		INDEX_OUT_OF_RANGE,

		/// <summary>
		/// Warning, that occurs when calling private or protected method outside of called object
		/// </summary>
		CALLING_INACCESSIBLE_METHOD,

		/// <summary>
		/// Warning, that occurs when accessing private or protected field outside of given object
		/// </summary>
		ACCESSING_INACCESSIBLE_FIELD,

		/// <summary>
		/// Warning, that occurs when function doesn't exist
		/// </summary>
		FUNCTION_DOESNT_EXISTS,

		/// <summary>
		/// Warning, that occurs when it is not possible to resolve a method on current object
		/// </summary>
		METHOD_WAS_NOT_RESOLVED,

		/// <summary>
		/// Warning, that occurs when function allready exists
		/// </summary>
		FUNCTION_ALLREADY_EXISTS,

		/// <summary>
		/// Warning, that occurs cannot resolve all evals as eval variable analysis get anyvalue or anystring
		/// </summary>
		COULDNT_RESOLVE_ALL_EVALS,

		/// <summary>
		/// Warning, that occurs when parse exception occurs in include or eval
		/// </summary>
		PARSER_EXCEPTION_IN_INCLUDE_OR_EVAL,

		/// <summary>
		/// Warning, that occurs when control flow graph building exception occurs in include or eval
		/// </summary>
		CFG_EXCEPTION_IN_INCLUDE_OR_EVAL,

		/// <summary>
		/// Warning, that occurs when analysis couldn't resolve all possible function calls
		/// </summary>
		COULDNT_RESOLVE_ALL_CALLS,

		/// <summary>
		/// Warning, that occurs when analysis couldn't resolve all possible includes
		/// </summary>
		COULDNT_RESOLVE_ALL_INCLUDES,

		/// <summary>
		/// Warning, that occurs when 3 or more evals are called recursively
		/// </summary>
		TOO_DEEP_EVAL_RECURSION,

		/// <summary>
		/// Call of method on undefined object.
		/// </summary>
		CALL_METHOD_ON_UNDEFINED_OBJECT,

		OTHER,
	}
}
