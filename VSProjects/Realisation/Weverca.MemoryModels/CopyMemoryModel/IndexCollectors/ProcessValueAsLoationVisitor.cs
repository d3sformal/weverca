using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Abstract implementation of IValueLocationVisitor which can be implemented in collecting
    /// algorithms to traverse memory using values within the memory location.
    /// 
    /// Visitor calls read value for visited values which allows assistant to report the error
    /// on accesing wrong array or object. When the object is correct (unknown, any or string value)
    /// abstract method ProcessValues is called which allows descendants of this abstract class to
    /// react on traversing memory tree by this special values.
    /// </summary>
    abstract class ProcessValueAsLocationVisitor : IValueLocationVisitor
    {
        MemoryAssistantBase assistant;

        /// <summary>
        /// Gets or sets a value indicating whether is must.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is must; otherwise, <c>false</c>.
        /// </value>
        public bool IsMust { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessValueAsLocationVisitor"/> class.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        public ProcessValueAsLocationVisitor(MemoryAssistantBase assistant)
        {
            this.assistant = assistant;
        }

        /// <summary>
        /// Allews descendant implementation to continue traversing memory by indexing values ar accesing their fields.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="values">The values.</param>
        /// <param name="isMust">if set to <c>true</c> is must.</param>
        public abstract void ProcessValues(MemoryIndex parentIndex, IEnumerable<Value> values, bool isMust);

        /// <summary>
        /// Visits the object value location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void VisitObjectValueLocation(ObjectValueLocation location)
        {
            IEnumerable<Value> values = location.ReadValues(assistant);
        }

        /// <summary>
        /// Visits the object any value location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void VisitObjectAnyValueLocation(ObjectAnyValueLocation location)
        {
            IEnumerable<Value> values = location.ReadValues(assistant);
            ProcessValues(location.ContainingIndex, values, IsMust);
        }

        /// <summary>
        /// Visits the array value location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void VisitArrayValueLocation(ArrayValueLocation location)
        {
            IEnumerable<Value> values = location.ReadValues(assistant);
        }

        /// <summary>
        /// Visits the array any value location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void VisitArrayAnyValueLocation(ArrayAnyValueLocation location)
        {
            IEnumerable<Value> values = location.ReadValues(assistant);
            ProcessValues(location.ContainingIndex, values, IsMust);
        }

        /// <summary>
        /// Visits the array string value location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void VisitArrayStringValueLocation(ArrayStringValueLocation location)
        {
            IEnumerable<Value> values = location.ReadValues(assistant);
            ProcessValues(null, values, IsMust);
        }

        /// <summary>
        /// Visits the array undefined value location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void VisitArrayUndefinedValueLocation(ArrayUndefinedValueLocation location)
        {
            IEnumerable<Value> values = location.ReadValues(assistant);
            ProcessValues(null, values, IsMust);
        }

        /// <summary>
        /// Visits the object undefined value location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void VisitObjectUndefinedValueLocation(ObjectUndefinedValueLocation location)
        {
            IEnumerable<Value> values = location.ReadValues(assistant);
            ProcessValues(null, values, IsMust);
        }


        /// <summary>
        /// Visits the information value location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void VisitInfoValueLocation(InfoValueLocation location)
        {
            IEnumerable<Value> values = location.ReadValues(assistant);
            ProcessValues(location.ContainingIndex, values, IsMust);
        }


        /// <summary>
        /// Visits any string value location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void VisitAnyStringValueLocation(AnyStringValueLocation location)
        {
            IEnumerable<Value> values = location.ReadValues(assistant);
            ProcessValues(null, values, IsMust);
        }
    }
}
