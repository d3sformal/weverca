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
    /// Contains information connected to variables and values
    /// 
    /// Imutable class 
    ///     For modification use builder object 
    ///         descriptor.Builder().modify().Build() //Creates new modified object
    /// </summary>
    class MemoryInfo
    {
        /// <summary>
        /// Gets the information values.
        /// </summary>
        /// <value>
        /// The information values.
        /// </value>
        public ReadOnlyCollection<InfoValue> InfoValues { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryInfo"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        internal MemoryInfo()
        {
            InfoValues = new ReadOnlyCollection<InfoValue>(new InfoValue[] { });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryInfo"/> class from the builder object.
        /// </summary>
        /// <param name="builder">The builder.</param>
        internal MemoryInfo(MemoryInfoBuilder builder)
        {
            InfoValues = new ReadOnlyCollection<InfoValue>(builder.InfoValues);
        }

        /// <summary>
        /// Creates new builder to modify this object 
        /// </summary>
        /// <returns></returns>
        public MemoryInfoBuilder Builder()
        {
            return new MemoryInfoBuilder(this);
        }
    }

    /// <summary>
    /// Mutable variant of MemoryInfo - use for creating new structure
    /// </summary>
    class MemoryInfoBuilder
    {
        /// <summary>
        /// Gets the information values.
        /// </summary>
        /// <value>
        /// The information values.
        /// </value>
        public List<InfoValue> InfoValues { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryInfoBuilder"/> class.
        /// </summary>
        /// <param name="memoryInfo">The memory information.</param>
        public MemoryInfoBuilder(MemoryInfo memoryInfo)
        {
            InfoValues = new List<InfoValue>(memoryInfo.InfoValues);
        }

        /// <summary>
        /// Builds new info object from this instance.
        /// </summary>
        /// <returns></returns>
        public MemoryInfo Build()
        {
            return new MemoryInfo(this);
        }

    }
}
