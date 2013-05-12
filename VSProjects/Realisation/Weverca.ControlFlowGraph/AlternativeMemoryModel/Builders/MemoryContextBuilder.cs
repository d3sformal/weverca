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
            var currentVersion = new MemoryContextVersion(_parent);

            var context = new MemoryContext(_storage, currentVersion);
            throw new NotImplementedException();

            return context;
        }

        
        #region Modification Sub builders
        /// <summary>
        /// Get builder for "modifying accros MemoryContext instances" specified variable.
        /// 
        /// TODO:This method deserves better naming
        /// </summary>
        /// <param name="variable">Variable that will be "modified"</param>
        /// <returns>Sub-builder</returns>
        public VariableBuilder ModificationBuilder(Variable variable)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get builder for "modifying accros MemoryContext instances" specified container.
        /// </summary>
        /// <param name="container">Container that will be "modified"</param>
        /// <returns>Sub-builder</returns>
        public ContainerBuilder ModificationBuilder(AssociativeContainer container)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Creation Sub-builders

        /// <summary>
        /// Create sub-builder for variables creation
        /// </summary>
        /// <param name="name">Name of created variable.</param>
        /// <returns>Sub-builder for variable creation.</returns>
        public VariableBuilder VariableCreator(VariableName name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create sub-builder for container creation
        /// </summary>        
        /// <returns>Sub-builder for container creation</returns>
        public ContainerBuilder ContainerCreator()
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
    }
}
