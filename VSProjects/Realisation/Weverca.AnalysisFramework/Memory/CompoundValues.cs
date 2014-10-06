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


﻿namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Abstract class for ObjectValue and AssociativeArray
    /// </summary>
    public abstract class CompoundValue : ConcreteValue
    {
        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitCompoundValue(this);
        }

        /// <inheritdoc />
        protected override int getHashCode()
        {
            return UID+this.GetType().GetHashCode();
        }

        /// <inheritdoc />
        protected override bool equals(Value other)
        {
            return this == other;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("{0} UID: {1}", base.ToString(), UID);
        }
    }

    /// <summary>
    /// ObjectValue is used as "ticket" that allows snapshot API to operate on represented object
    /// NOTE:
    ///     Is supposed to be used as Hash key for getting stored info in snapshot
    /// </summary>
    public sealed class ObjectValue : CompoundValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectValue" /> class.
        /// It prevents creating objects from outside
        /// </summary>
        internal ObjectValue() { }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitObjectValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            ObjectValue value = new ObjectValue();
            value.setStorage(getStorage());
            return value;
        }

		public override string ToString (ISnapshotReadonly snapshot)
		{
			return string.Format("{0} UID: {1}", snapshot.ObjectType(this).QualifiedName.ToString(), UID);
		}

    }

    /// <summary>
    /// AssociativeArray is used as "ticket" that allows snapshot API to operate on represented array
    /// NOTE:
    ///     * Is supposed to be used as Hash key for getting stored info in snapshot
    /// </summary>
    public sealed class AssociativeArray : CompoundValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssociativeArray" /> class.
        /// It prevents creating arrays from outside
        /// </summary>
        internal AssociativeArray() { }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAssociativeArray(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            AssociativeArray value = new AssociativeArray();
            value.setStorage(getStorage());
            return value;
        }

     

    }
}