using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;

using Weverca.ControlFlowGraph.AlternativeMemoryModel.ValueImplementations;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel.Builders
{
    /// <summary>
    /// Builder for wokring with variables, values and processing operations on memory context.
    /// </summary>
    public class MemoryContextBuilder
    {
        /// <summary>
        /// Version of parent (is referenced as parent from builded memory context)
        /// </summary>
        private readonly MemoryContextVersion _parent;

        /// <summary>
        /// Storage where data for builded context will be stored
        /// </summary>
        private readonly MemoryStorage _storage;


        Dictionary<VariableName, VariableSubBuilder> _varSubBuilders = new Dictionary<VariableName, VariableSubBuilder>();

        internal MemoryContextBuilder(MemoryContextVersion parent,MemoryStorage storage)
        {
            _parent = parent;            
        }

        /// <summary>
        /// Build described memory context accordingly ALL GENERATED sub-builders
        /// </summary>
        /// <returns>Builded memory context.</returns>
        public MemoryContext BuildContext()
        {
            var buildedVersion = new MemoryContextVersion(_parent);

            
            _storage.OpenWriting(buildedVersion);

            //write changes from subbuilders
            writeVariableChanges();
            _storage.CloseWriting();


            var context = new MemoryContext(_storage, buildedVersion);
            return context;
        }

        private void writeVariableChanges()
        {
            foreach (var varSubBuilder in _varSubBuilders.Values)
            {
                var needsAllocation = varSubBuilder.IsDeclaration && varSubBuilder.PossibleReferences.Count() == 0;

                IEnumerable<VirtualReference> possibleReferences;
                if (needsAllocation)
                {
                    possibleReferences = new VirtualReference[] { _storage.ReferenceForVariable(varSubBuilder.Name) };
                }
                else
                {
                    possibleReferences = varSubBuilder.PossibleReferences;
                }

                _storage.Write(possibleReferences, varSubBuilder.PossibleValues);
            }
        }

        
        #region Modification Sub builders
        /// <summary>
        /// Get builder for "modifying accros MemoryContext instances" specified variable.
        /// 
        /// TODO:This method deserves better naming
        /// </summary>
        /// <param name="variable">Variable that will be "modified"</param>
        /// <returns>Sub-builder</returns>
        public VariableSubBuilder ModificationBuilder(Variable variable)
        {
            if (_varSubBuilders.ContainsKey(variable.Name))
            {
                throw new NotSupportedException("Cannot create more sub builders for same variable");
            }

            return new VariableSubBuilder(variable);
        }



        /// <summary>
        /// Get builder for "modifying accros MemoryContext instances" specified container.
        /// </summary>
        /// <param name="container">Container that will be "modified"</param>
        /// <returns>Sub-builder</returns>
        public ContainerSubBuilder ModificationBuilder(AssociativeContainer container)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Creation Sub-builders

        /// <summary>
        /// Create sub-builder for variables creation
        /// IMPLEMENTATION WARNING: 
        ///     All allocated variables with sane name has to point
        ///     to same reference - because of merging contexts
        /// </summary>
        /// <param name="name">Name of created variable.</param>
        /// <returns>Sub-builder for variable creation.</returns>
        public VariableSubBuilder VariableCreator(VariableName name)
        {
            if (_varSubBuilders.ContainsKey(name))
            {
                throw new NotSupportedException("Cannot create more sub builders for same variable");
            }

            return _varSubBuilders[name] = new VariableSubBuilder(name);
        }

        /// <summary>
        /// Create sub-builder for container creation
        /// </summary>        
        /// <returns>Sub-builder for container creation</returns>
        public ContainerSubBuilder ContainerCreator()
        {
            throw new NotImplementedException();
        }

        #endregion

     


        #region Helper methods

        /// <summary>
        /// Declare variable of given varName and initial value
        /// </summary>
        /// <param name="varName">Name of declared variable</param>
        /// <param name="initialValue">Initial value for variable</param>
        /// <returns>Declared variable</returns>
        public Variable Declare(VariableName varName, AbstractValue initialValue=null)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void MergeWith(MemoryContext context2)
        {
            throw new NotImplementedException();
        }
    }
}
