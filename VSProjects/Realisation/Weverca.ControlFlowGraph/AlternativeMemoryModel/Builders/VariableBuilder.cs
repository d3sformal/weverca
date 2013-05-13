using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel.Builders
{
    /// <summary>
    /// Simple prototype of builder for variables
    /// 
    /// NOTE: No all operations can be proceeded on single instance of variable builder
    /// </summary>
    public class VariableBuilder
    {

        /// <summary>
        /// Declare variable in context of parent MemoryContextBuilder
        /// </summary>
        /// <param name="initValue">Initial value of variable (can be null)</param>
        public void Declare(AbstractValue initValue=null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add possible values into all references of builded variable.
        /// </summary>
        /// <param name="value">Value that will be added</param>
        public void AddPossibleValue(AbstractValue value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Assign by value 
        /// NOTES: (this is caused by design of theoretical memory model)
        /// * if there is only one reference in variable - we will REWRITE relevant entry in memory by .NET references of given possibleValues
        /// * If there is more references - we should ADD possible values into all referenced memory entries
        /// </summary>
        /// <param name="possibleValues"></param>
        public void Assign(IEnumerable<AbstractValue> possibleValues)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set references for variable.
        /// </summary>
        /// <param name="references">References that will be set.</param>
        public void SetReferences(IEnumerable<VirtualReference> references)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Build is called internally by parent MemoryContextBuilder
        /// </summary>
        /// <returns></returns>
        internal Variable Build()
        {
            throw new NotImplementedException();
        }

        public void MergeWith(Variable var2, MemoryContext context2)
        {
            throw new NotImplementedException();
        }
    }
}
