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

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Memory
{
    /// <summary>
    /// Represents data structure to provide alternative memory locations which is not based on memory index model
    /// but concrete value in particular memoru location. This class is ment to allow accesing fields or indexes on
    /// variables where is no array nor object - error reporting or accesing indexes of special values.
    /// </summary>
    public abstract class ValueLocation
    {
        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public abstract void Accept(IValueLocationVisitor visitor);

        /// <summary>
        /// Read values from location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <returns>Result of reading received from assistant.</returns>
        public abstract IEnumerable<Value> ReadValues(MemoryAssistantBase assistant);

        /// <summary>
        /// Read values to location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <param name="entry">The entry.</param>
        /// <returns>Result of writing received from assistant.</returns>
        public abstract IEnumerable<Value> WriteValues(MemoryAssistantBase assistant, MemoryEntry entry);
    }

    /// <summary>
    /// Definition of visitor pattern for ValueLocations.
    /// </summary>
    public interface IValueLocationVisitor
    {
        /// <summary>
        /// Visits the object value location.
        /// </summary>
        /// <param name="location">The location.</param>
        void VisitObjectValueLocation(ObjectValueLocation location);

        /// <summary>
        /// Visits the object any value location.
        /// </summary>
        /// <param name="location">The location.</param>
        void VisitObjectAnyValueLocation(ObjectAnyValueLocation location);

        /// <summary>
        /// Visits the array value location.
        /// </summary>
        /// <param name="location">The location.</param>
        void VisitArrayValueLocation(ArrayValueLocation location);

        /// <summary>
        /// Visits the array any value location.
        /// </summary>
        /// <param name="location">The location.</param>
        void VisitArrayAnyValueLocation(ArrayAnyValueLocation location);

        /// <summary>
        /// Visits the array string value location.
        /// </summary>
        /// <param name="location">The location.</param>
        void VisitArrayStringValueLocation(ArrayStringValueLocation location);

        /// <summary>
        /// Visits the array undefined value location.
        /// </summary>
        /// <param name="location">The location.</param>
        void VisitArrayUndefinedValueLocation(ArrayUndefinedValueLocation location);

        /// <summary>
        /// Visits the object undefined value location.
        /// </summary>
        /// <param name="location">The location.</param>
        void VisitObjectUndefinedValueLocation(ObjectUndefinedValueLocation location);

        /// <summary>
        /// Visits the information value location.
        /// </summary>
        /// <param name="location">The location.</param>
        void VisitInfoValueLocation(InfoValueLocation location);

        /// <summary>
        /// Visits any string value location.
        /// </summary>
        /// <param name="location">The location.</param>
        void VisitAnyStringValueLocation(AnyStringValueLocation location);
    }

    #region Info Values

    /// <summary>
    /// Implementation of value location class for info values.
    /// 
    /// Read and write methods do not modify the value and just return associated info value.
    /// </summary>
    public class InfoValueLocation : ValueLocation
    {
        /// <summary>
        /// Memory index which contains this value.
        /// </summary>
        public readonly MemoryIndex ContainingIndex;

        /// <summary>
        /// Associated info value.
        /// </summary>
        public readonly InfoValue Value;

        private IEnumerable<Value> values;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoValueLocation"/> class.
        /// </summary>
        /// <param name="containingIndex">Index of the containing.</param>
        /// <param name="value">The value.</param>
        public InfoValueLocation(MemoryIndex containingIndex, InfoValue value)
        {
            this.ContainingIndex = containingIndex;
            this.values = new Value[] { value };
            Value = value;
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public override void Accept(IValueLocationVisitor visitor)
        {
            visitor.VisitInfoValueLocation(this);
        }

        /// <summary>
        /// Read values from location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <returns>
        /// Result of reading received from assistant.
        /// </returns>
        public override IEnumerable<Value> ReadValues(MemoryAssistantBase assistant)
        {
            return values;
        }

        /// <summary>
        /// Read values to location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <param name="entry">The entry.</param>
        /// <returns>
        /// Result of writing received from assistant.
        /// </returns>
        public override IEnumerable<Value> WriteValues(MemoryAssistantBase assistant, MemoryEntry entry)
        {
            return values;
        }
    }

    #endregion

    #region Object Fields

    /// <summary>
    /// Implementation of value location class for accesing fields on non object values.
    /// 
    /// Read and write do not modify the structure. Using memory the assistant the warning is emited and old value is returned.
    /// </summary>
    public class ObjectValueLocation : ValueLocation
    {
        private MemoryIndex containingIndex;
        private VariableIdentifier index;
        private Value value;

        /// <summary>
        /// Gets the memory index which contains this value.
        /// </summary>
        /// <value>
        /// Memory index which contains this value.
        /// </value>
        public MemoryIndex ContainingIndex { get { return containingIndex; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectValueLocation"/> class.
        /// </summary>
        /// <param name="containingIndex">Index of the containing.</param>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public ObjectValueLocation(MemoryIndex containingIndex, VariableIdentifier index, Value value)
        {
            this.containingIndex = containingIndex;
            this.index = index;
            this.value = value;
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public override void Accept(IValueLocationVisitor visitor)
        {
            visitor.VisitObjectValueLocation(this);
        }

        /// <summary>
        /// Read values from location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <returns>
        /// Result of reading received from assistant.
        /// </returns>
        public override IEnumerable<Value> ReadValues(MemoryAssistantBase assistant)
        {
            return assistant.ReadValueField(value, index);
        }

        /// <summary>
        /// Read values to location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <param name="entry">The entry.</param>
        /// <returns>
        /// Result of writing received from assistant.
        /// </returns>
        public override IEnumerable<Value> WriteValues(MemoryAssistantBase assistant, MemoryEntry entry)
        {
            return assistant.WriteValueField(value, index, entry);
        }
    }

    /// <summary>
    /// Implementation of value location class for accesing fields on undefined values.
    /// 
    /// Read using memory assistant returns undefined value. Write into undefined value should not be processed becouse
    /// collecter creates new implicit object and uses it rather than accesing this undefined location.
    /// </summary>
    public class ObjectUndefinedValueLocation : ValueLocation
    {
        private MemoryIndex containingIndex;
        private VariableIdentifier index;
        private UndefinedValue value;

        /// <summary>
        /// Gets the memory index which contains this value.
        /// </summary>
        /// <value>
        /// Memory index which contains this value.
        /// </value>
        public MemoryIndex ContainingIndex { get { return containingIndex; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectUndefinedValueLocation"/> class.
        /// </summary>
        /// <param name="containingIndex">Index of the containing.</param>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public ObjectUndefinedValueLocation(MemoryIndex containingIndex, VariableIdentifier index, UndefinedValue value)
        {
            this.containingIndex = containingIndex;
            this.index = index;
            this.value = value;
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public override void Accept(IValueLocationVisitor visitor)
        {
            visitor.VisitObjectUndefinedValueLocation(this);
        }

        /// <summary>
        /// Read values from location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <returns>
        /// Result of reading received from assistant.
        /// </returns>
        public override IEnumerable<Value> ReadValues(MemoryAssistantBase assistant)
        {
            return assistant.ReadValueField(value, index);
        }

        /// <summary>
        /// Read values to location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <param name="entry">The entry.</param>
        /// <returns>
        /// Result of writing received from assistant.
        /// </returns>
        public override IEnumerable<Value> WriteValues(MemoryAssistantBase assistant, MemoryEntry entry)
        {
            return assistant.WriteValueField(value, index, entry);
        }
    }

    /// <summary>
    /// Implementation of value location class for accesing fields on any values.
    /// 
    /// Read using memory assistant returns any value. Write do not make any change in structure.
    /// </summary>
    public class ObjectAnyValueLocation : ValueLocation
    {
        private MemoryIndex containingIndex;
        private VariableIdentifier index;
        private AnyValue value;

        /// <summary>
        /// Gets the memory index which contains this value.
        /// </summary>
        /// <value>
        /// Memory index which contains this value.
        /// </value>
        public MemoryIndex ContainingIndex { get { return containingIndex; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectAnyValueLocation"/> class.
        /// </summary>
        /// <param name="containingIndex">Index of the containing.</param>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public ObjectAnyValueLocation(MemoryIndex containingIndex, VariableIdentifier index, AnyValue value)
        {
            this.containingIndex = containingIndex;
            this.index = index;
            this.value = value;
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public override void Accept(IValueLocationVisitor visitor)
        {
            visitor.VisitObjectAnyValueLocation(this);
        }

        /// <summary>
        /// Read values from location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <returns>
        /// Result of reading received from assistant.
        /// </returns>
        public override IEnumerable<Value> ReadValues(MemoryAssistantBase assistant)
        {
            return assistant.ReadValueField(value, index);
        }

        /// <summary>
        /// Read values to location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <param name="entry">The entry.</param>
        /// <returns>
        /// Result of writing received from assistant.
        /// </returns>
        public override IEnumerable<Value> WriteValues(MemoryAssistantBase assistant, MemoryEntry entry)
        {
            return assistant.WriteValueField(value, index, entry);
        }
    }

    #endregion

    #region Array Indexes

    /// <summary>
    /// Implementation of value location class for accesing indexes on scalar values.
    /// 
    /// Read and write do not modify the structure. Using memory the assistant the warning is emited and old value is returned.
    /// </summary>
    public class ArrayValueLocation : ValueLocation
    {
        private MemoryIndex containingIndex;
        private MemberIdentifier index;
        private Value value;

        /// <summary>
        /// Gets the memory index which contains this value.
        /// </summary>
        /// <value>
        /// Memory index which contains this value.
        /// </value>
        public MemoryIndex ContainingIndex { get { return containingIndex; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayValueLocation"/> class.
        /// </summary>
        /// <param name="containingIndex">Index of the containing.</param>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public ArrayValueLocation(MemoryIndex containingIndex, MemberIdentifier index, Value value)
        {
            this.containingIndex = containingIndex;
            this.index = index;
            this.value = value;
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public override void Accept(IValueLocationVisitor visitor)
        {
            visitor.VisitArrayValueLocation(this);
        }

        /// <summary>
        /// Read values from location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <returns>
        /// Result of reading received from assistant.
        /// </returns>
        public override IEnumerable<Value> ReadValues(MemoryAssistantBase assistant)
        {
            return assistant.ReadValueIndex(value, index);
        }

        /// <summary>
        /// Read values to location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <param name="entry">The entry.</param>
        /// <returns>
        /// Result of writing received from assistant.
        /// </returns>
        public override IEnumerable<Value> WriteValues(MemoryAssistantBase assistant, MemoryEntry entry)
        {
            return assistant.WriteValueIndex(value, index, entry);
        }
    }

    /// <summary>
    /// Implementation of value location class for accesing indexes on any values.
    /// 
    /// Read using memory assistant returns any value. Write do not make any change in structure.
    /// </summary>
    public class ArrayAnyValueLocation : ValueLocation
    {
        private MemoryIndex containingIndex;
        private MemberIdentifier index;
        private AnyValue value;

        /// <summary>
        /// Gets the memory index which contains this value.
        /// </summary>
        /// <value>
        /// Memory index which contains this value.
        /// </value>
        public MemoryIndex ContainingIndex { get { return containingIndex; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayAnyValueLocation"/> class.
        /// </summary>
        /// <param name="containingIndex">Index of the containing.</param>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public ArrayAnyValueLocation(MemoryIndex containingIndex, MemberIdentifier index, AnyValue value)
        {
            this.containingIndex = containingIndex;
            this.index = index;
            this.value = value;
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public override void Accept(IValueLocationVisitor visitor)
        {
            visitor.VisitArrayAnyValueLocation(this);
        }

        /// <summary>
        /// Read values from location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <returns>
        /// Result of reading received from assistant.
        /// </returns>
        public override IEnumerable<Value> ReadValues(MemoryAssistantBase assistant)
        {
            return assistant.ReadAnyValueIndex(value, index).PossibleValues;
        }

        /// <summary>
        /// Read values to location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <param name="entry">The entry.</param>
        /// <returns>
        /// Result of writing received from assistant.
        /// </returns>
        public override IEnumerable<Value> WriteValues(MemoryAssistantBase assistant, MemoryEntry entry)
        {
            return assistant.WriteValueIndex(value, index, entry);
        }
    }

    /// <summary>
    /// Implementation of value location class for accesing indexes on undefined values.
    /// 
    /// Read using memory assistant returns undefined value. Write into undefined value should not be processed becouse
    /// collecter creates new empty array and uses it rather than accesing this undefined location.
    /// </summary>
    public class ArrayUndefinedValueLocation : ValueLocation
    {
        private MemoryIndex containingIndex;
        private MemberIdentifier index;
        private UndefinedValue value;

        /// <summary>
        /// Gets the memory index which contains this value.
        /// </summary>
        /// <value>
        /// Memory index which contains this value.
        /// </value>
        public MemoryIndex ContainingIndex { get { return containingIndex; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayUndefinedValueLocation"/> class.
        /// </summary>
        /// <param name="containingIndex">Index of the containing.</param>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public ArrayUndefinedValueLocation(MemoryIndex containingIndex, MemberIdentifier index, UndefinedValue value)
        {
            this.containingIndex = containingIndex;
            this.index = index;
            this.value = value;
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public override void Accept(IValueLocationVisitor visitor)
        {
            visitor.VisitArrayUndefinedValueLocation(this);
        }

        /// <summary>
        /// Read values from location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <returns>
        /// Result of reading received from assistant.
        /// </returns>
        public override IEnumerable<Value> ReadValues(MemoryAssistantBase assistant)
        {
            return assistant.ReadValueIndex(value, index);
        }

        /// <summary>
        /// Read values to location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <param name="entry">The entry.</param>
        /// <returns>
        /// Result of writing received from assistant.
        /// </returns>
        public override IEnumerable<Value> WriteValues(MemoryAssistantBase assistant, MemoryEntry entry)
        {
            return assistant.WriteValueIndex(value, index, entry);
        }
    }

    /// <summary>
    /// Implementation of value location class for accesing indexes on string values.
    /// 
    /// Read using memory asisstant returnes character in the requested index or emit warning if the index is not valid.
    /// Write operation creates new string with modified character or emit warning if written value or index are wrong.
    /// </summary>
    public class ArrayStringValueLocation : ValueLocation
    {
        private MemoryIndex containingIndex;
        private MemberIdentifier index;
        private StringValue value;

        /// <summary>
        /// Gets the memory index which contains this value.
        /// </summary>
        /// <value>
        /// Memory index which contains this value.
        /// </value>
        public MemoryIndex ContainingIndex { get { return containingIndex; } }

        /// <summary>
        /// Gets the value associated with this location.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public StringValue Value { get { return value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayStringValueLocation"/> class.
        /// </summary>
        /// <param name="containingIndex">Index of the containing.</param>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public ArrayStringValueLocation(MemoryIndex containingIndex, MemberIdentifier index, StringValue value)
        {
            this.containingIndex = containingIndex;
            this.index = index;
            this.value = value;
        }
        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public override void Accept(IValueLocationVisitor visitor)
        {
            visitor.VisitArrayStringValueLocation(this);
        }

        /// <summary>
        /// Read values from location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <returns>
        /// Result of reading received from assistant.
        /// </returns>
        public override IEnumerable<Value> ReadValues(MemoryAssistantBase assistant)
        {
            return assistant.ReadStringIndex(value, index);
        }

        /// <summary>
        /// Read values to location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <param name="entry">The entry.</param>
        /// <returns>
        /// Result of writing received from assistant.
        /// </returns>
        public override IEnumerable<Value> WriteValues(MemoryAssistantBase assistant, MemoryEntry entry)
        {
            return assistant.WriteStringIndex(value, index, entry);
        }
    }

    /// <summary>
    /// Implementation of value location class for accesing indexes on any string values.
    /// 
    /// Read using memory asisstant returnes any character or emit warning if the index is not valid.
    /// Write operation do not change the structure or emit warning if written value or index are wrong.
    /// </summary>
    public class AnyStringValueLocation : ValueLocation
    {
        private MemoryIndex containingIndex;
        private MemberIdentifier index;
        private AnyStringValue value;

        /// <summary>
        /// Gets the memory index which contains this value.
        /// </summary>
        /// <value>
        /// Memory index which contains this value.
        /// </value>
        public MemoryIndex ContainingIndex { get { return containingIndex; } }

        /// <summary>
        /// Gets the value associated with this location.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public AnyStringValue Value { get { return value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnyStringValueLocation"/> class.
        /// </summary>
        /// <param name="containingIndex">Index of the containing.</param>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public AnyStringValueLocation(MemoryIndex containingIndex, MemberIdentifier index, AnyStringValue value)
        {
            this.containingIndex = containingIndex;
            this.index = index;
            this.value = value;
        }
        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public override void Accept(IValueLocationVisitor visitor)
        {
            visitor.VisitAnyStringValueLocation(this);
        }

        /// <summary>
        /// Read values from location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <returns>
        /// Result of reading received from assistant.
        /// </returns>
        public override IEnumerable<Value> ReadValues(MemoryAssistantBase assistant)
        {
            return assistant.ReadAnyValueIndex(value, index).PossibleValues;
        }

        /// <summary>
        /// Read values to location using specified memory assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <param name="entry">The entry.</param>
        /// <returns>
        /// Result of writing received from assistant.
        /// </returns>
        public override IEnumerable<Value> WriteValues(MemoryAssistantBase assistant, MemoryEntry entry)
        {
            return assistant.WriteValueIndex(value, index, entry);
        }
    }

    #endregion
}