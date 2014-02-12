using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Assistant used for resolving and reporting memory based operations by snapshot.
    /// Is usefull for tracking info values, reporting warnings,...
    /// 
    /// TODO: Memory model implementors should design API needed for all operations reporting and resolving.
    /// <remarks>
    /// Analysis creates assistant for every created snapshot. This snapshot is
    /// avalialable in Context member.
    /// </remarks>
    /// </summary>
    public abstract class MemoryAssistantBase
    {
        /// <summary>
        /// Context snapshot for current memory assistant
        /// </summary>
        protected SnapshotBase Context { get; private set; }

        /// <summary>
        /// ProgramPoint that is registered for memory asistant. If not available is null.
        /// </summary>
        protected ProgramPointBase Point { get; private set; }

        /// <summary>
        /// Read index of any value
        /// </summary>
        /// <param name="value">Any value which index is read</param>
        /// <param name="index">Index used for given value</param>
        /// <returns>Value resolved by reading index on given value</returns>
        public abstract MemoryEntry ReadAnyValueIndex(AnyValue value, MemberIdentifier index);

        /// <summary>
        /// Read index of string value
        /// </summary>
        /// <param name="value">String value which index is read</param>
        /// <param name="index">Index used for given value</param>
        /// <returns>Value resolved by reading index on given value</returns>
        public abstract IEnumerable<Value> ReadStringIndex(StringValue value, MemberIdentifier index);


        /// <summary>
        /// Read index of value (that is not array, string, undefined or anyvalue)
        /// </summary>
        /// <param name="value">Value which index is read</param>
        /// <param name="index">Index used for given value</param>
        /// <returns>Value resolved by reading index on given value</returns>
        public abstract IEnumerable<Value> ReadValueIndex(Value value, MemberIdentifier index);

        /// <summary>
        /// Write at index of string value
        /// </summary>
        /// <param name="indexed">String value which index is written</param>
        /// <param name="index">Index used for given value</param>
        /// <param name="writtenValue">Value that is written at specified index</param>
        /// <returns>Values that should be written back into string container</returns>
        public abstract IEnumerable<Value> WriteStringIndex(StringValue indexed, MemberIdentifier index, MemoryEntry writtenValue);

        /// <summary>
        /// Write at index of value (that is not array, string, undefined or anyvalue)
        /// </summary>
        /// <param name="indexed">Value which index is written</param>
        /// <param name="index">Index used for given value</param>
        /// <param name="writtenValue">Value that is written at specified index</param>
        /// <returns>Values that should be written back into value container</returns>
        public abstract IEnumerable<Value> WriteValueIndex(Value indexed, MemberIdentifier index, MemoryEntry writtenValue);


        /// <summary>
        /// Write at field of value (that is not object, undefined or anyvalue)
        /// </summary>
        /// <param name="fielded">Value which field is written</param>
        /// <param name="field">Field used for given value</param>
        /// <param name="writtenValue">Value that is written at specified field</param>
        /// <returns>Values that should be written back into value container</returns>
        public abstract IEnumerable<Value> WriteValueField(Value fielded, VariableIdentifier field, MemoryEntry writtenValue);

        /// <summary>
        /// Read fied of any value
        /// </summary>
        /// <param name="value">Any value which field is read</param>
        /// <param name="field">Field used for given value</param>
        /// <returns>Value resolved by reading field on given value.</returns>
        public abstract MemoryEntry ReadAnyField(AnyValue value, VariableIdentifier field);

        /// <summary>
        /// Read at field of value (that is not object, undefined or anyvalue)
        /// </summary>
        /// <param name="fielded">Value which field is read</param>
        /// <param name="field">Field used for given value</param>
        /// <returns>Value resolved by reading field on fielded value</returns>
        public abstract IEnumerable<Value> ReadValueField(Value fielded, VariableIdentifier field);

        /// <summary>
        /// Widening operation between old and current memory entries
        /// </summary>
        /// <param name="old">Initial value before transaction has been started</param>
        /// <param name="current">Value computed during transaction</param>
        /// <returns>Widened memory entry</returns>
        public abstract MemoryEntry Widen(MemoryEntry old, MemoryEntry current);

        /// <summary>
        /// Simplify handler that is used during commit on entries having more than SimplifyLimit possible values
        /// </summary>
        /// <param name="entry">Value computed during transaction</param>
        /// <returns>Simplified memory entry</returns>
        public abstract MemoryEntry Simplify(MemoryEntry entry);

        /// <summary>
        /// Resolve methods with given name for possible value
        /// </summary>
        /// <param name="thisObject">Value which methods are resolved</param>
        /// <param name="type">Type of the object</param>
        /// <param name="methodName">Name of resolved methods</param>
        /// <param name="objectMethods">Methods available for thisObject</param>
        /// <returns>Resolved methods</returns>
        public abstract IEnumerable<FunctionValue> ResolveMethods(Value thisObject, TypeValue type, QualifiedName methodName, IEnumerable<FunctionValue> objectMethods);

        /// <summary>
        /// Resolve methods with given name for possible value
        /// </summary>
        /// <param name="value">Type of the object</param>
        /// <param name="methodName">Name of resolved methods</param>
        /// <param name="objectMethods">Methods available for thisObject</param>
        /// <returns>Resolved methods</returns>
        public abstract IEnumerable<FunctionValue> ResolveMethods(TypeValue value, QualifiedName methodName, IEnumerable<FunctionValue> objectMethods);

        /// <summary>
        /// Create implicit object. Is used as an object for operations that can cause
        /// object creation (indexing variable,...)
        /// </summary>
        /// <returns></returns>
        public abstract ObjectValue CreateImplicitObject();

        /// <summary>
        /// Report that snapshot has been forced to iterate fields of given value, which is not an object.
        /// </summary>
        /// <param name="value">Value where fields has been tried to iterate</param>
        public abstract void TriedIterateFields(Value value);

        /// <summary>
        /// Initialize context snapshot for current assistant
        /// </summary>
        /// <param name="context">Context snapshot for current memory assistant</param>
        internal void InitContext(SnapshotBase context)
        {
            if (Context != null)
                throw new NotSupportedException("Cannot initialize Context twice");

            Context = context;
        }

        /// <summary>
        /// Register given program point with current assitant. Registered program point
        /// is available in Point property.
        /// </summary>
        /// <param name="programPoint">Registered program point</param>
        internal void RegisterProgramPoint(ProgramPointBase programPoint)
        {
            if (Point != null)
                throw new NotSupportedException("Cannot register Point twice");

            Point = programPoint;
        }
    }
}
