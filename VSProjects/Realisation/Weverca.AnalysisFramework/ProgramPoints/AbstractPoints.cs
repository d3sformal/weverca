﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework.ProgramPoints
{

    /// <summary>
    /// Base class for RValue program points
    /// <remarks>RValue program points can be asked for MemoryEntry value</remarks>
    /// </summary>
    public abstract class ValuePoint : ProgramPointBase
    {
        /// <summary>
        /// Resulting value represented by current point
        /// </summary>
        public virtual ReadSnapshotEntryBase Value { get; protected set; }

        /// <inheritdoc />
        public override string ToString()
        {
            if (Value == null)
            {
                return base.ToString();
            }
            else
            {
                return Value.ToString() + " " + GetHashCode();
            }
        }
    }

    /// <summary>
    /// Base class for LValue program points
    /// <remarks>LValue program points can be asked for assigns</remarks>
    /// </summary>
    public abstract class LValuePoint : ValuePoint
    {
        /// <summary>
        /// Resulting LValue represented by current point
        /// </summary>
        public virtual ReadWriteSnapshotEntryBase LValue { get; protected set; }

        /// <inheritdoc />
        public override ReadSnapshotEntryBase Value
        {
            get
            {
                return LValue;
            }
            protected set { throw new NotSupportedException("You have to override Value to be able set it"); }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (LValue == null)
            {
                return base.ToString();
            }
            else
            {
                return LValue.ToString()+ " " + GetHashCode();
            }
        }
    }

    /// <summary>
    /// Call representation. 
    /// Represents all calls that can be made - it has an extension for each possible call. 
    /// Its return value is obtained by merging of return values of all calls that can be made.
    /// </summary>
    public abstract class RCallPoint : ValuePoint
    {
        /// <summary>
        /// Arguments specified for call - private storage
        /// </summary>
        private readonly ValuePoint[] _arguments;

        /// <summary>
        /// Arguments specified for call
        /// </summary>
        public IEnumerable<ValuePoint> Arguments { get { return _arguments; } }

        /// <summary>
        /// CallSignature obtained from function declaration if available, null otherwise
        /// </summary>
        public readonly CallSignature? CallSignature;

        /// <summary>
        /// This object for call
        /// <remarks>If there is no this object, is null</remarks>
        /// </summary>
        public readonly ValuePoint ThisObj;

        /// <summary>
        /// Return value is obtained from call sink - it has return values of all calls that can be made merged.
        /// </summary>
        public override ReadSnapshotEntryBase Value
        {
            get
            {
                //Return value is obtained from sink
                return Extension.Sink.Value;
            }
            protected set
            {
                throw new NotSupportedException("Cannot set value of call node");
            }
        }

        internal RCallPoint(ValuePoint thisObj, CallSignature? callSignature, ValuePoint[] arguments)
        {
            CallSignature = callSignature;
            ThisObj = thisObj;
            _arguments = arguments;
        }

        /// <summary>
        /// Prepare arguments into flow controller
        /// </summary>
        internal void PrepareArguments()
        {
            //TODO better argument handling avoid copying values
            var argumentValues = new MemoryEntry[_arguments.Length];
            for (int i = 0; i < _arguments.Length; ++i)
            {
                argumentValues[i] = _arguments[i].Value.ReadMemory(OutSnapshot);
            }

            if (ThisObj != null)
            {
                Flow.CalledObject = ThisObj.Value.ReadMemory(OutSnapshot);
            }
            Flow.Arguments = argumentValues;
        }
    }

}
