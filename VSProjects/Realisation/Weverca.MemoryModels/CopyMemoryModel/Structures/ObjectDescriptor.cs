using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>    
    /// Stores type and list of fields of PHP object. Every objects is determined by special value object
    /// with no content. This object is just pointer which is used to receive this descriptor instance when is need.
    /// This approach allows to use pointer object across whole memory model without need of modyfying structure
    /// when some new idnex is added - snapshot just simply modify descriptor instance and let the pointer to be
    /// still the same.
    /// 
    /// Imutable class. For modification use builder object 
    ///         descriptor.Builder().modify().Build() //Creates new modified object
    /// </summary>
    internal class ObjectDescriptor : ReadonlyIndexContainer
    {
        /// <summary>
        /// Gets the fields.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        public IReadOnlyDictionary<string, MemoryIndex> Indexes { get; private set; }

        /// <summary>
        /// Gets the types.
        /// </summary>
        /// <value>
        /// The types.
        /// </value>
        public TypeValue Type { get; private set; }

        /// <summary>
        /// List of variables where the object is stored in
        /// </summary>
        /// <value>
        /// The parent variable.
        /// </value>
        public MemoryIndex ParentVariable { get; private set; }

        /// <summary>
        /// Speacial index when the target location is unknown (ANY index)
        /// </summary>
        /// <value>
        /// The index of the unknown.
        /// </value>
        public MemoryIndex UnknownIndex { get; private set; }

        /// <summary>
        /// Pointing object value this descriptor is used for.
        /// </summary>
        public ObjectValue ObjectValue { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDescriptor"/> class from the builder object.
        /// </summary>
        /// <param name="objectDescriptorBuilder">The object descriptor builder.</param>
        public ObjectDescriptor(ObjectDescriptorBuilder objectDescriptorBuilder)
        {
            Indexes = new ReadOnlyDictionary<string, MemoryIndex>(objectDescriptorBuilder.Indexes);
            Type = objectDescriptorBuilder.Type;

            UnknownIndex = objectDescriptorBuilder.UnknownIndex;
            ParentVariable = objectDescriptorBuilder.ParentVariable;
            ObjectValue = objectDescriptorBuilder.ObjectValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDescriptor" /> class.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        /// <param name="type">The type of object.</param>
        /// <param name="unknownIndex">Index of the unknown.</param>
        public ObjectDescriptor(ObjectValue objectValue, TypeValue type, MemoryIndex unknownIndex)
        {
            Indexes = new ReadOnlyDictionary<string, MemoryIndex>(new Dictionary<string, MemoryIndex>());
            Type = type;

            UnknownIndex = unknownIndex;

            ObjectValue = objectValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDescriptor" /> class.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        /// <param name="parentVariable">The parent variable.</param>
        /// <param name="type">The type of object.</param>
        /// <param name="unknownIndex">Index of the unknown.</param>
        public ObjectDescriptor(ObjectValue objectValue, MemoryIndex parentVariable, TypeValue type, MemoryIndex unknownIndex)
        {
            Indexes = new ReadOnlyDictionary<string, MemoryIndex>(new Dictionary<string, MemoryIndex>());
            Type = type;

            UnknownIndex = unknownIndex;
            ParentVariable = parentVariable;

            ObjectValue = objectValue;
        }

        /// <summary>
        /// Creates new builder to modify this descriptor 
        /// </summary>
        /// <returns>New builder to modify this descriptor.</returns>
        public ObjectDescriptorBuilder Builder()
        {
            return new ObjectDescriptorBuilder(this);
        }
    }

    /// <summary>
    /// Mutable variant of ObjectDescriptor - use for creating new structure
    /// </summary>
    internal class ObjectDescriptorBuilder : IWriteableIndexContainer
    {
        /// <summary>
        /// Gets the fields.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        public Dictionary<string, MemoryIndex> Indexes { get; private set; }

        /// <summary>
        /// Gets the types.
        /// </summary>
        /// <value>
        /// The types.
        /// </value>
        public TypeValue Type { get; set; }

        /// <summary>
        /// List of variables where the object is stored in
        /// </summary>
        public MemoryIndex ParentVariable { get; private set; }

        /// <summary>
        /// Gets the speacial field which is used when the target location is unknown (ANY index)
        /// </summary>
        /// <value>
        /// The index of the unknown.
        /// </value>
        public MemoryIndex UnknownIndex { get; private set; }

        /// <summary>
        /// Pointing object value this descriptor is used for.
        /// </summary>
        public ObjectValue ObjectValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDescriptorBuilder"/> class from the given object descriptor.
        /// </summary>
        /// <param name="objectDescriptor">The object descriptor.</param>
        public ObjectDescriptorBuilder(ObjectDescriptor objectDescriptor)
        {
            IDictionary<string, MemoryIndex> collection = objectDescriptor.Indexes as IDictionary<string, MemoryIndex>;

            if (collection != null)
            {
                Indexes = new Dictionary<string, MemoryIndex>(collection);
            }
            else
            {
                Indexes = new Dictionary<string, MemoryIndex>();
            }

            Type = objectDescriptor.Type;

            ParentVariable = objectDescriptor.ParentVariable;
            UnknownIndex = objectDescriptor.UnknownIndex;
            ObjectValue = objectDescriptor.ObjectValue;
        }

        /// <summary>
        /// Adds the specified container index.
        /// </summary>
        /// <param name="containerIndex">Index of the container.</param>
        /// <param name="fields">The fields.</param>
        /// <returns>This builder object.</returns>
        public ObjectDescriptorBuilder add(string containerIndex, MemoryIndex fields)
        {
            Indexes[containerIndex] = fields;
            return this;
        }

        /// <summary>
        /// Builds new descriptor object from this instance.
        /// </summary>
        /// <returns>New descriptor object from this instance.</returns>
        public ObjectDescriptor Build()
        {
            return new ObjectDescriptor(this);
        }

        /// <summary>
        /// Sets the parent variable.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <returns>This builder object.</returns>
        public ObjectDescriptorBuilder SetParentVariable(MemoryIndex parentIndex)
        {
            ParentVariable = parentIndex;
            return this;
        }

        /// <summary>
        /// Sets the unknown field.
        /// </summary>
        /// <param name="unknownIndex">Index of the unknown.</param>
        /// <returns>This builder object.</returns>
        public ObjectDescriptorBuilder SetUnknownField(MemoryIndex unknownIndex)
        {
            UnknownIndex = unknownIndex;
            return this;
        }
    }
}
