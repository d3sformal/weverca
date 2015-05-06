/*
Copyright (c) 2012-2014 Pavel Bastecky.

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
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Process given memory entry and creates list of ValueLocation to acces indexes on non array values.
    /// </summary>
    class ReadIndexVisitor : AbstractValueVisitor
    {
        private MemberIdentifier index;
        private MemoryIndex containingIndex;
        private ICollection<ValueLocation> locations;

        /// <summary>
        /// Gets or sets a value indicating whether memory entry contains undefined value.
        /// </summary>
        /// <value>
        /// <c>true</c> if memory entry contains undefined value; otherwise, <c>false</c>.
        /// </value>
        public bool ContainsUndefinedValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether memory entry contains defined value.
        /// </summary>
        /// <value>
        /// <c>true</c> if memory entry contains defined value; otherwise, <c>false</c>.
        /// </value>
        public bool ContainsDefinedValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether memory entry contains array value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if memory entry contains array value; otherwise, <c>false</c>.
        /// </value>
        public bool ContainsArrayValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether memory entry contains any value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if memory entry contains any value; otherwise, <c>false</c>.
        /// </value>
        public bool ContainsAnyValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadIndexVisitor"/> class.
        /// </summary>
        /// <param name="containingIndex">Index of the containing.</param>
        /// <param name="indexSegment">The index segment.</param>
        /// <param name="locations">The locations.</param>
        public ReadIndexVisitor(MemoryIndex containingIndex, IndexPathSegment indexSegment, ICollection<ValueLocation> locations)
        {
            this.containingIndex = containingIndex;
            this.locations = locations;

            ContainsUndefinedValue = false;
            ContainsDefinedValue = false;
            ContainsArrayValue = false;
            ContainsAnyValue = false;

            index = new MemberIdentifier(indexSegment.Names);
        }

        public override void VisitValue(Value value)
        {
            ContainsDefinedValue = true;
            locations.Add(new ArrayValueLocation(containingIndex, index, value));
        }

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            ContainsUndefinedValue = true;
            locations.Add(new ArrayUndefinedValueLocation(containingIndex, index, value));
        }

        public override void VisitAnyArrayValue(AnyArrayValue value)
        {
            ContainsAnyValue = true;
            locations.Add(new ArrayAnyValueLocation(containingIndex, index, value));
        }

        public override void VisitAnyValue(AnyValue value)
        {
            ContainsAnyValue = true;
            locations.Add(new ArrayAnyValueLocation(containingIndex, index, value));
        }

        public override void VisitStringValue(StringValue value)
        {
            ContainsDefinedValue = true;
            locations.Add(new ArrayStringValueLocation(containingIndex, index, value));
        }

        public override void VisitAnyStringValue(AnyStringValue value)
        {
            ContainsDefinedValue = true;
            locations.Add(new AnyStringValueLocation(containingIndex, index, value));
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            ContainsArrayValue = true;
        }

        public override void VisitInfoValue(InfoValue value)
        {
            locations.Add(new InfoValueLocation(containingIndex, value));
        }
    }

    /// <summary>
    /// Process given memory entry and creates list of ValueLocation to acces fields on non object values.
    /// </summary>
    class ReadFieldVisitor : AbstractValueVisitor
    {
        private VariableIdentifier index;
        private MemoryIndex containingIndex;
        private ICollection<ValueLocation> locations;

        /// <summary>
        /// Gets or sets a value indicating whether memory entry contains undefined value.
        /// </summary>
        /// <value>
        /// <c>true</c> if memory entry contains undefined value; otherwise, <c>false</c>.
        /// </value>
        public bool ContainsUndefinedValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether memory entry contains defined value.
        /// </summary>
        /// <value>
        /// <c>true</c> if memory entry contains defined value; otherwise, <c>false</c>.
        /// </value>
        public bool ContainsDefinedValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether memory entry contains object value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if memory entry contains object value; otherwise, <c>false</c>.
        /// </value>
        public bool ContainsObjectValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether memory entry contains any value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if memory entry contains any value; otherwise, <c>false</c>.
        /// </value>
        public bool ContainsAnyValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadFieldVisitor"/> class.
        /// </summary>
        /// <param name="containingIndex">Index of the containing.</param>
        /// <param name="fieldSegment">The field segment.</param>
        /// <param name="locations">The locations.</param>
        public ReadFieldVisitor(MemoryIndex containingIndex, FieldPathSegment fieldSegment, ICollection<ValueLocation> locations)
        {
            this.containingIndex = containingIndex;
            this.locations = locations;

            ContainsUndefinedValue = false;
            ContainsDefinedValue = false;
            ContainsObjectValue = false;
            ContainsAnyValue = false;

            index = new VariableIdentifier(fieldSegment.Names);
        }

        public override void VisitValue(Value value)
        {
            ContainsDefinedValue = true;
            locations.Add(new ObjectValueLocation(containingIndex, index, value));
        }

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            ContainsUndefinedValue = true;
            locations.Add(new ObjectUndefinedValueLocation(containingIndex, index, value));
        }

        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            ContainsAnyValue = true;
            locations.Add(new ObjectAnyValueLocation(containingIndex, index, value));
        }

        public override void VisitAnyValue(AnyValue value)
        {
            ContainsAnyValue = true;
            locations.Add(new ObjectAnyValueLocation(containingIndex, index, value));
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            ContainsObjectValue = true;
        }

        public override void VisitInfoValue(InfoValue value)
        {
            locations.Add(new InfoValueLocation(containingIndex, value));
        }
    }
}