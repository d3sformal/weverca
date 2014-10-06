/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

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


/*
Copyright (c) 2012-2014 David Hauzar and Mirek Vodolan.

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


﻿using System;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Represent special kind of value. For example these values can express some non-determinism.
    /// </summary>
    public abstract class SpecialValue : Value
    {
        /// <inheritdoc />
        protected override int getHashCode()
        {
            return GetType().GetHashCode();
        }

        /// <inheritdoc />
        protected override bool equals(Value other)
        {
            return GetType() == other.GetType();
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitSpecialValue(this);
        }
    }

    /// <summary>
    /// Represents meta information that can be stored in <see cref="SnapshotBase"/>
    /// </summary>
    public abstract class InfoValue : SpecialValue
    {
        /// <summary>
        /// Raw representation of stored meta info
        /// </summary>
        public readonly object RawData;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoValue" /> class.
        /// </summary>
        /// <param name="rawData"></param>
        internal InfoValue(object rawData)
        {
            RawData = rawData;
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitInfoValue(this);
        }

        /// <inheritdoc />
        protected override int getHashCode()
        {
            return RawData.GetHashCode();
        }

        /// <inheritdoc />
        protected override bool equals(Value other)
        {
            var infoValue = other as InfoValue;
            if (infoValue == null)
            {
                return false;
            }

            return infoValue.RawData.Equals(RawData);
        }
    }

    /// <summary>
    /// Stores meta information for variables and values
    /// WARNING:
    ///     Has to be immutable - also generic type T
    /// </summary>
    /// <typeparam name="T">Type of meta information</typeparam>
    public class InfoValue<T> : InfoValue
    {
        /// <summary>
        /// Strongly Typed meta information data
        /// </summary>
        public readonly T Data;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoValue{T}" /> class.
        /// </summary>
        /// <param name="data"></param>
        internal InfoValue(T data)
            : base(data)
        {
            Data = data;
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitGenericInfoValue(this);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Data.ToString();
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            return new InfoValue<T>(Data);
        }

		/// <inheritdoc />
		public override int GetHashCode ()
		{
			return base.getHashCode();
			//return Data.GetHashCode();
		}

		/// <inheritdoc />
		public override bool Equals(Object obj) {
			if (this == obj) return true;
			var info = obj as InfoValue;
			return equals(info);
			//return base.Equals(obj);
			//return Data.Equals(obj);
		}
    }
}