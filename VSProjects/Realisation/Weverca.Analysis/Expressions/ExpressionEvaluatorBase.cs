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
    public abstract class ExpressionEvaluatorBase
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

        /// <summary>
        /// Resolves possible name of variable identifier by value
        /// NOTE:
        ///     Is used for resolving indirect variable usages
        /// </summary>
        /// <param name="variableSpecifier">Value representing possible names of variable</param>
        /// <returns>Possible variable names</returns>
        abstract public IEnumerable<string> VariableNames(MemoryEntry variableSpecifier);

        /// <summary>
        /// Resolves value, determined by given variable specifier
        /// </summary>
        /// <param name="variable">Specifier of resolved variable</param>
        /// <returns>Possible values obtained from resolving variable specifier</returns>
        abstract public MemoryEntry ResolveVariable(VariableEntry variable);

        /// <summary>
        /// Resolves value, determined by given variable specifier
        /// NOTE:
        ///     Is useful for implicit array creation
        /// </summary>
        /// <param name="indexedVariable">Variable which is indexed</param>
        /// <returns>Possible values obtained from resolving indexed variable</returns>
        abstract public MemoryEntry ResolveIndexedVariable(VariableEntry indexedVariable);

        /// <summary>
        /// Resolves value, determined by given field specifier
        /// </summary>
        /// <param name="objectValue">Object value which field is resolved</param>
        /// <param name="field">Specifier of resolved field</param>
        /// <returns>Possible values obtained from resolving given field</returns>
        abstract public MemoryEntry ResolveField(MemoryEntry objectValue, VariableEntry field);

        /// <summary>
        /// Resolves value at indexedValue[index]
        /// </summary>
        /// <param name="indexedValue">Value which index is resolved</param>
        /// <param name="index">Specifier of an index</param>
        /// <returns>Possible values obtained from resolving given index</returns>
        abstract public MemoryEntry ResolveIndex(MemoryEntry indexedValue, MemoryEntry index);
                
        /// <summary>
        /// Resolves alias from given field specifier
        /// </summary>
        /// <param name="objectValue">Object containing aliased field</param>
        /// <param name="aliasedField">Specifier of an field</param>
        /// <returns>Resolved aliases</returns>
        abstract public IEnumerable<AliasValue> ResolveAlias(MemoryEntry objectValue, VariableEntry aliasedField);

        /// <summary>
        /// Assign possible aliases to given target
        /// </summary>
        /// <param name="target">Target variable specifier</param>
        /// <param name="possibleAliases">Possible aliases to be assigned</param>
        abstract public void AliasAssign(VariableEntry target, IEnumerable<AliasValue> possibleAliases);

        /// <summary>
        /// Assign possible aliases to given object field
        /// </summary>
        /// <param name="objectValue">Object containing assigned field</param>
        /// <param name="fieldEntry">Specifier of an field</param>
        /// <param name="possibleAliasses">Possible aliases to be assigned</param>
        abstract public void AliasAssign(MemoryEntry objectValue, VariableEntry fieldEntry, IEnumerable<AliasValue> possibleAliasses);

        /// <summary>
        /// Assign possible values to given target
        /// </summary>
        /// <param name="target">Target variable specifier</param>
        /// <param name="value">Possible values to be assigned</param>
        abstract public void Assign(VariableEntry target, MemoryEntry value);

        /// <summary>
        /// Assign possible values to given targetField of an objectValue
        /// </summary>
        /// <param name="objectValue">Object containing assigned field</param>
        /// <param name="targetField">Specifier of an field</param>
        /// <param name="value">Possible values to be assigned</param>
        abstract public void Assign(MemoryEntry objectValue, VariableEntry targetField, MemoryEntry value);

        /// <summary>
        /// Assign assignedValue at indexedValue[index]
        /// NOTE:
        ///     Array/object/string can be indexed
        /// </summary>
        /// <param name="indexedValue">Value which index is assigned</param>
        /// <param name="index">Specifier of an index</param>
        /// <param name="assignedValue">Value that is assigned</param>
        abstract public void IndexAssign(MemoryEntry indexedValue, MemoryEntry index, MemoryEntry assignedValue);

        /// <summary>
        /// Proccess binary operation on given operands
        /// </summary>
        /// <param name="leftOperand">Left operand of operation</param>
        /// <param name="operation">Binary operation</param>
        /// <param name="rightOperand">Right operand of operation</param>
        /// <returns>Result of binary expression</returns>
        abstract public MemoryEntry BinaryEx(MemoryEntry leftOperand, Operations operation, MemoryEntry rightOperand);

        /// <summary>
        /// Process unary operation on given operand
        /// </summary>
        /// <param name="operation">Unary operation</param>
        /// <param name="operand">Operand of operation</param>
        /// <returns>Result of unary expression</returns>
        abstract public MemoryEntry UnaryEx(Operations operation, MemoryEntry operand);

        #endregion
                
        #region Default implementation of simple routines

        /// <summary>
        /// Resolve alias of given variable specifier
        /// </summary>
        /// <param name="variable">Aliased variable specifier</param>
        /// <returns>Resolved aliases</returns>
        virtual public IEnumerable<AliasValue> ResolveAlias(VariableEntry variable)
        {
            return from aliasedVariable in variable.PossibleNames select Flow.OutSet.CreateAlias(aliasedVariable);
        }

        /// <summary>
        /// Create string representation of given literal
        /// </summary>
        /// <param name="x">Literal value</param>
        /// <returns>Created literal value representation</returns>
        virtual public MemoryEntry StringLiteral(StringLiteral x)
        {
            return new MemoryEntry(OutSet.CreateString(x.Value as String));
        }
        
        /// <summary>
        /// Create integer representation of given literal
        /// </summary>
        /// <param name="x">Literal value</param>
        /// <returns>Created literal value representation</returns>
        virtual public MemoryEntry IntLiteral(IntLiteral x)
        {
            return new MemoryEntry(OutSet.CreateInt((int)x.Value));
        }
        
        /// <summary>
        /// Create long integer representation of given literal
        /// </summary>
        /// <param name="x">Literal value</param>
        /// <returns>Created literal value representation</returns>
        virtual public MemoryEntry LongIntLiteral(LongIntLiteral x)
        {
            return new MemoryEntry(OutSet.CreateLong((long)x.Value));
        }

        /// <summary>
        /// Create boolean representation of given literal
        /// </summary>
        /// <param name="x">Literal value</param>
        /// <returns>Created literal value representation</returns>
        virtual public MemoryEntry BoolLiteral(BoolLiteral x)
        {
            return new MemoryEntry(OutSet.CreateBool((bool)x.Value));
        }
        
        /// <summary>
        /// Create string representation of given literal
        /// </summary>
        /// <param name="x">Literal value</param>
        /// <returns>Created literal value representation</returns>
        virtual public MemoryEntry DoubleLiteral(DoubleLiteral x)
        {
            return new MemoryEntry(OutSet.CreateDouble((double)x.Value));
        }
        
        /// <summary>
        /// Create object value of given type
        /// </summary>
        /// <param name="typeName">Object type specifier</param>
        /// <returns>Created object</returns>
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
        /// Set current evaluation context
        /// </summary>
        /// <param name="flow">Flow controller available for evaluation</param>
        /// <param name="element">Currently evaluated element</param>
        internal void SetContext(FlowController flow)
        {
            Flow = flow;
        }
    }
}
