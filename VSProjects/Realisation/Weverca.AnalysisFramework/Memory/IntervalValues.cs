using System;

using PHP.Core;

namespace Weverca.AnalysisFramework.Memory
{

    /// <summary>
    /// Abstract inteval value
    /// </summary>
    public abstract class IntervalValue : Value
    { }

    /// <summary>
    /// Value representing interval of values
    /// </summary>
    /// <typeparam name="T">Type of stored value - NOTE: Has to provide immutability</typeparam>
    public abstract class IntervalValue<T> : IntervalValue
        where T : IComparable, IComparable<T>, IEquatable<T>
    {
        /// <summary>
        /// Start, End of Interval
        /// </summary>
        public readonly T Start, End;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntervalValue{T}" /> class.
        /// </summary>
        /// <param name="start">Start of interval</param>
        /// <param name="end">End of interval</param>
        internal IntervalValue(T start, T end)
        {
            Debug.Assert(start.CompareTo(end) <= 0,
                "Start value of interval is less or equal to end value in correct interval");
            Start = start;
            End = end;
        }

        /// <summary>
        /// Gets representation of zero value
        /// </summary>
        public abstract T Zero { get; }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitGenericIntervalValue(this);
        }

        /// <inheritdoc />
        protected override bool equals(Value obj)
        {
            var o = obj as IntervalValue<T>;
            if (o == null)
            {
                return false;
            }

            return Start.Equals(o.Start) && End.Equals(o.End);
        }

        /// <inheritdoc />
        protected override int getHashCode()
        {
            return Start.GetHashCode() + End.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("'({0},{1})', Type: {2}", Start, End, typeof(T).Name);
        }
    }

    /// <summary>
    /// Represnets integer interval. Start and end are inclusive
    /// </summary>
    public class IntegerIntervalValue : IntervalValue<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerIntervalValue" /> class.
        /// </summary>
        /// <param name="start">Start of discrete interval of integers</param>
        /// <param name="end">End of discrete interval of integers</param>
        internal IntegerIntervalValue(int start, int end) : base(start, end) { }

        /// <summary>
        /// Gets integer representation of zero value
        /// </summary>
        public override int Zero
        {
            get { return 0; }
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitIntervalIntegerValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            return new IntegerIntervalValue(Start, End);
        }
    }

    /// <summary>
    /// Represent longint interval value
    /// </summary>
    public class LongintIntervalValue : IntervalValue<long>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LongintIntervalValue" /> class.
        /// </summary>
        /// <param name="start">Start of discrete interval of long integers</param>
        /// <param name="end">End of discrete interval of long integers</param>
        internal LongintIntervalValue(long start, long end) : base(start, end) { }

        /// <summary>
        /// Gets long integer representation of zero value
        /// </summary>
        public override long Zero
        {
            get { return 0; }
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitIntervalLongintValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            return new LongintIntervalValue(Start, End);
        }
    }

    /// <summary>
    /// Represnets float interval value
    /// </summary>
    public class FloatIntervalValue : IntervalValue<double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FloatIntervalValue" /> class.
        /// </summary>
        /// <param name="start">Start of real interval of floating-point numbers</param>
        /// <param name="end">End of real interval of floating-point numbers</param>
        internal FloatIntervalValue(double start, double end) : base(start, end) { }

        /// <summary>
        /// Gets floating-point representation of zero value
        /// </summary>
        public override double Zero
        {
            get { return 0.0; }
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitIntervalFloatValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            return new FloatIntervalValue(Start, End);
        }
    }
}
