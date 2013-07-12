using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis.Expressions
{
    /// <summary>
    /// Evaluates expressions during analysis
    /// </summary>
    public abstract class ExpressionEvaluator
    {
        /// <summary>
        /// Current flow controller available for expression evaluation
        /// </summary>
        public FlowController Flow { get; private set; }

        /// <summary>
        /// Current output set of expression evaluation
        /// </summary>
        public FlowOutputSet OutSet { get { return Flow.OutSet; } }

        /// <summary>
        /// Current input set of expression evaluation
        /// </summary>
        public FlowInputSet InSet { get { return Flow.InSet; } }

        /// <summary>
        /// Element which is currently evaluated
        /// </summary>
        public LangElement Element { get { return Flow.CurrentPartial; } }

        #region Template API methods for implementors

        abstract public MemoryEntry ResolveVariable(VariableEntry variable);
        abstract public MemoryEntry ResolveField(MemoryEntry objectValue, VariableEntry field);
        abstract public IEnumerable<AliasValue> ResolveAlias(MemoryEntry objectValue, VariableEntry aliasedField);
        abstract public void AliasAssign(VariableEntry target, IEnumerable<AliasValue> possibleAliases);
        abstract public void AliasAssign(MemoryEntry objectValue, VariableEntry fieldEntry, IEnumerable<AliasValue> possibleAliasses);
        abstract public void Assign(VariableEntry target, MemoryEntry value);
        abstract public void Assign(MemoryEntry objectValue, VariableEntry targetField, MemoryEntry value);
        abstract public MemoryEntry BinaryEx(MemoryEntry leftOperand, Operations operation, MemoryEntry rightOperand);        

        #endregion

        /// <summary>
        /// Set current evaluation context
        /// </summary>
        /// <param name="flow">Flow controller available for evaluation</param>
        /// <param name="element">Currently evaluated element</param>
        internal void SetContext(FlowController flow)
        {
            Flow = flow;            
        }
        
        #region Default implementation of simple routines

        virtual public IEnumerable<AliasValue> ResolveAlias(VariableEntry aliasedVariables)
        {
            return from aliasedVariable in aliasedVariables.PossibleNames select Flow.OutSet.CreateAlias(aliasedVariable);
        }

        virtual public MemoryEntry StringLiteral(StringLiteral x)
        {
            return new MemoryEntry(OutSet.CreateString(x.Value as String));
        }

        virtual public MemoryEntry IntLiteral(IntLiteral x)
        {
            return new MemoryEntry(OutSet.CreateInt((int)x.Value));
        }

        virtual public MemoryEntry LongIntLiteral(LongIntLiteral x)
        {
            return new MemoryEntry(OutSet.CreateLong((long)x.Value));
        }

        virtual public MemoryEntry BoolLiteral(BoolLiteral x)
        {
            return new MemoryEntry(OutSet.CreateBool((bool)x.Value));
        }

        virtual public MemoryEntry DoubleLiteral(DoubleLiteral x)
        {
            return new MemoryEntry(OutSet.CreateDouble((double)x.Value));
        }

        virtual public MemoryEntry CreateObject(QualifiedName typeName)
        {
            var declarations=OutSet.ResolveType(typeName);

            var result=new List<ObjectValue>();
            foreach(var declaration in declarations){
                result.Add(OutSet.CreateObject(declaration));
            }

            return new MemoryEntry(result.ToArray());
        }
        #endregion


        /// <summary>
        /// Resolves possible name of variable identifier by value
        /// NOTE:
        ///     Is used for resolving indirect variable usages
        /// </summary>
        /// <param name="value">Value representing possible names of variable</param>
        /// <returns>Possible variable names</returns>
        abstract public IEnumerable<string> VariableNames(MemoryEntry value);

        abstract public void ArrayAssign(MemoryEntry array, MemoryEntry index, MemoryEntry assignedValue);


        abstract public MemoryEntry ArrayRead(MemoryEntry array, MemoryEntry index);

        abstract public MemoryEntry ResolveArray(VariableEntry entry);
   
    }
}
