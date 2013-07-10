using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.Analysis.Memory
{
    public abstract class IntervalValue<T> : Value
    {
        public readonly T Start, End;

        public IntervalValue(T start, T end)
        {
            Start = start;
            End = end;
        }

        public override bool Equals(object obj)
        {
            var o = obj as IntervalValue<T>;
            if (o == null)
            {
                return false;
            }
            return (Start.Equals(o.Start) && End.Equals(o.End));
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode() + End.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("'({0},{1})', Type: {2}", Start, End, typeof(T).Name);
        }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitGenericIntervalValue(this);
        }
    }

    public class IntegerIntervalValue : IntervalValue<int>
    {
        internal IntegerIntervalValue(int start, int end) :base(start,end){}

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitIntervalIntegerValue(this);
        }
    }

    public class LongintIntervalValue : IntervalValue<long>
    {
        internal LongintIntervalValue(long start, long end) : base(start, end) { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitIntervalLongintValue(this);
        }
    }

    public class FloatIntervalValue : IntervalValue<double>
    {
        internal FloatIntervalValue(double start, double end) : base(start, end) { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitIntervalFloatValue(this);
        }
    }
}
