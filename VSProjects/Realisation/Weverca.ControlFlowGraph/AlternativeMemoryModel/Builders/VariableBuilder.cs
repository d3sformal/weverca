using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel.Builders
{
    /// <summary>
    /// Simple prototype of builder for variables
    /// 
    /// NOTE: No all operations can be proceeded on single instance of variable builder
    /// </summary>
    public class VariableSubBuilder
    {
        public readonly VariableName Name;
        public readonly bool IsDeclaration;

        private HashSet<VirtualReference> _possibleReferences = new HashSet<VirtualReference>();
        private List<AbstractValue> _possibleValues = new List<AbstractValue>();

        /// <summary>
        /// Current possible referenes for builded variable
        /// </summary>
        public IEnumerable<VirtualReference> PossibleReferences { get { return _possibleReferences; } }
        /// <summary>
        /// Current possible values for builded variable
        /// </summary>
        public IEnumerable<AbstractValue> PossibleValues { get { return _possibleValues; } }

        internal VariableSubBuilder(VariableName name)
        {
            IsDeclaration = true;
            Name = name;
        }

        internal VariableSubBuilder(Variable modifiedVariable)
        {
            IsDeclaration = false;
            Name = modifiedVariable.Name;

            AssignReferences(modifiedVariable.PossibleReferences);
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
            _possibleValues.Clear();
            _possibleValues.AddRange(possibleValues);
        }

        /// <summary>
        /// Set references for variable.
        /// </summary>
        /// <param name="references">References that will be set.</param>
        public void AssignReferences(IEnumerable<VirtualReference> references)
        {
            _possibleReferences.Clear();
            foreach (var reference in references)
            {
                _possibleReferences.Add(reference);
            }
        }

        /// <summary>
        /// Cause merging builded variable with other variable.
        /// NOTE:
        ///     Only variables with same name are allowed to be merged.
        /// </summary>
        /// <param name="other">Variable that will be merged</param>
        public void MergeWith(Variable other)
        {
            Debug.Assert(other.Name == Name, "Merging variables with different names is probably mistake");

            _possibleReferences.UnionWith(other.PossibleReferences);
        }

        /// <summary>
        /// Build variable according to current references
        /// NOTE:
        ///     Assigned values will be processed after building  memory context.
        /// </summary>
        /// <returns></returns>
        public Variable Build()
        {
            return new Variable(Name, PossibleReferences);
        }
    }
}
