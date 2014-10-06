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
    /// Class is representing a value of primitive PHP type
    /// </summary>
    public abstract class ConcreteValue : Value
    {
        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitConcreteValue(this);
        }
    }

    /// <summary>
    /// Class is representing reference to an PHP external resource, identified by special internal ID
    /// </summary>
    public class ResourceValue : ConcreteValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceValue" /> class.
        /// It prevents creating resource from outside
        /// </summary>
        internal ResourceValue() { }

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
            visitor.VisitResourceValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            return new ResourceValue();
        }
    }

    /// <summary>
    /// Class is representing PHP null type with the only one possible value: <c>NULL</c>
    /// </summary>
    public class UndefinedValue : ConcreteValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UndefinedValue" /> class.
        /// It prevents creating undefined value from outside
        /// </summary>
        internal UndefinedValue() { }

        /// <summary>
        /// Returns hash code of the type, thou all instances of undefined (or null) type are the same
        /// </summary>
        /// <returns>Hash code of undefined value</returns>
        protected override int getHashCode()
        {
            return GetType().GetHashCode();
        }

        /// <summary>
        /// Determines whether type of the compared object is <see cref="UndefinedValue"/>
        /// </summary>
        /// <param name="obj">The object to compare with the current object</param>
        /// <returns><c>true</c> if object has same type as the current one, otherwise <c>false</c></returns>
        protected override bool equals(Value obj)
        {
            return GetType() == obj.GetType();
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitUndefinedValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            UndefinedValue value = new UndefinedValue();
            value.setStorage(getStorage());
            return value;
        }

    }
}