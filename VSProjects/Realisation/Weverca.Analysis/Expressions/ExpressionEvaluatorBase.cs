using System;
using System.Collections.Generic;

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
        /// Gets current flow controller available for expression evaluation
        /// </summary>
        public FlowController Flow { get; private set; }

        /// <summary>
        /// Gets current output set of expression evaluation
        /// </summary>
        public FlowOutputSet OutSet { get { return Flow.OutSet; } }

        /// <summary>
        /// Gets current input set of expression evaluation
        /// </summary>
        public FlowInputSet InSet { get { return Flow.InSet; } }

        /// <summary>
        /// Gets element which is currently evaluated
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
        public abstract IEnumerable<string> VariableNames(MemoryEntry variableSpecifier);

        /// <summary>
        /// Resolves value, determined by given variable specifier
        /// </summary>
        /// <param name="variable">Specifier of resolved variable</param>
        /// <returns>Possible values obtained from resolving variable specifier</returns>
        public abstract MemoryEntry ResolveVariable(VariableEntry variable);

        /// <summary>
        /// Resolves value, determined by given variable specifier
        /// NOTE:
        ///     Is useful for implicit array creation
        /// </summary>
        /// <param name="variable">Variable which is indexed</param>
        /// <returns>Possible values obtained from resolving indexed variable</returns>
        public abstract MemoryEntry ResolveIndexedVariable(VariableEntry variable);

        /// <summary>
        /// Resolves value, determined by given field specifier
        /// </summary>
        /// <param name="objectValue">Object value which field is resolved</param>
        /// <param name="field">Specifier of resolved field</param>
        /// <returns>Possible values obtained from resolving given field</returns>
        public abstract MemoryEntry ResolveField(MemoryEntry objectValue, VariableEntry field);

        /// <summary>
        /// Resolves value at indexedValue[index]
        /// </summary>
        /// <param name="indexedValue">Value which index is resolved</param>
        /// <param name="index">Specifier of an index</param>
        /// <returns>Possible values obtained from resolving given index</returns>
        public abstract MemoryEntry ResolveIndex(MemoryEntry indexedValue, MemoryEntry index);

        /// <summary>
        /// Resolves alias from given field specifier
        /// </summary>
        /// <param name="objectValue">Object containing aliased field</param>
        /// <param name="aliasedField">Specifier of an field</param>
        /// <returns>Resolved aliases</returns>
        public abstract IEnumerable<AliasValue> ResolveAliasedField(MemoryEntry objectValue, VariableEntry aliasedField);

        /// <summary>
        /// Assign possible aliases to given target
        /// </summary>
        /// <param name="target">Target variable specifier</param>
        /// <param name="possibleAliases">Possible aliases to be assigned</param>
        public abstract void AliasAssign(VariableEntry target, IEnumerable<AliasValue> possibleAliases);

        /// <summary>
        /// Assign possible aliases to given object field
        /// </summary>
        /// <param name="objectValue">Object containing assigned field</param>
        /// <param name="aliasedField">Specifier of an field</param>
        /// <param name="possibleAliases">Possible aliases to be assigned</param>
        public abstract void AliasedFieldAssign(MemoryEntry objectValue, VariableEntry aliasedField, IEnumerable<AliasValue> possibleAliases);

        /// <summary>
        /// Assign possible values to given target
        /// </summary>
        /// <param name="target">Target variable specifier</param>
        /// <param name="entry">Possible values to be assigned</param>
        public abstract void Assign(VariableEntry target, MemoryEntry entry);

        /// <summary>
        /// Assign possible values to given targetField of an objectValue
        /// </summary>
        /// <param name="objectValue">Object containing assigned field</param>
        /// <param name="targetField">Specifier of an field</param>
        /// <param name="entry">Possible values to be assigned</param>
        public abstract void FieldAssign(MemoryEntry objectValue, VariableEntry targetField, MemoryEntry entry);

        /// <summary>
        /// Assign assignedValue at indexedValue[index]
        /// NOTE:
        ///     Array/object/string can be indexed
        /// </summary>
        /// <param name="indexedValue">Value which index is assigned</param>
        /// <param name="index">Specifier of an index</param>
        /// <param name="assignedValue">Value that is assigned</param>
        public abstract void IndexAssign(MemoryEntry indexedValue, MemoryEntry index, MemoryEntry assignedValue);

        /// <summary>
        /// Process binary operation on given operands
        /// </summary>
        /// <param name="leftOperand">Left operand of operation</param>
        /// <param name="operation">Binary operation</param>
        /// <param name="rightOperand">Right operand of operation</param>
        /// <returns>Result of binary expression</returns>
        public abstract MemoryEntry BinaryEx(MemoryEntry leftOperand, Operations operation, MemoryEntry rightOperand);

        /// <summary>
        /// Process unary operation on given operand
        /// </summary>
        /// <param name="operation">Unary operation</param>
        /// <param name="operand">Operand of operation</param>
        /// <returns>Result of unary expression</returns>
        public abstract MemoryEntry UnaryEx(Operations operation, MemoryEntry operand);

        /// <summary>
        /// Process n-ary operation on given operands
        /// </summary>
        /// <param name="operation">Unary operation</param>
        /// <param name="operands">All operands of operation</param>
        /// <returns>Result of n-ary expression</returns>
        public abstract MemoryEntry ArrayEx(IEnumerable<KeyValuePair<MemoryEntry, MemoryEntry>> keyValuePairs);

        /// <summary>
        /// Process foreach statement on given variables
        /// <remarks>
        ///  Is intented to store all possible values from enumeration into keyVariable and valueVariable
        /// </remarks>
        /// </summary>
        /// <param name="enumeree">Enumerated value</param>
        /// <param name="keyVariable">Varible where keys are stored</param>
        /// <param name="valueVariable">Variable where values are stored</param>
        public abstract void Foreach(MemoryEntry enumeree, VariableEntry keyVariable, VariableEntry valueVariable);

        #endregion

        #region Default implementation of simple routines

        /// <summary>
        /// Resolve alias of given variable specifier
        /// </summary>
        /// <param name="variable">Aliased variable specifier</param>
        /// <returns>Resolved aliases</returns>
        public virtual IEnumerable<AliasValue> ResolveAlias(VariableEntry variable)
        {
            var possibleNames = variable.PossibleNames;
            var aliases = new List<AliasValue>(possibleNames.Length);
            foreach (var aliasedVariable in possibleNames)
            {
                aliases.Add(OutSet.CreateAlias(aliasedVariable));
            }

            return aliases;
        }

        /// <summary>
        /// Create string representation of given literal
        /// </summary>
        /// <param name="x">Literal value</param>
        /// <returns>Created literal value representation</returns>
        public virtual MemoryEntry StringLiteral(StringLiteral x)
        {
            return new MemoryEntry(OutSet.CreateString(x.Value as String));
        }

        /// <summary>
        /// Create integer representation of given literal
        /// </summary>
        /// <param name="x">Literal value</param>
        /// <returns>Created literal value representation</returns>
        public virtual MemoryEntry IntLiteral(IntLiteral x)
        {
            return new MemoryEntry(OutSet.CreateInt((int)x.Value));
        }

        /// <summary>
        /// Create long integer representation of given literal
        /// </summary>
        /// <param name="x">Literal value</param>
        /// <returns>Created literal value representation</returns>
        public virtual MemoryEntry LongIntLiteral(LongIntLiteral x)
        {
            return new MemoryEntry(OutSet.CreateLong((long)x.Value));
        }

        /// <summary>
        /// Create boolean representation of given literal
        /// </summary>
        /// <param name="x">Literal value</param>
        /// <returns>Created literal value representation</returns>
        public virtual MemoryEntry BoolLiteral(BoolLiteral x)
        {
            return new MemoryEntry(OutSet.CreateBool((bool)x.Value));
        }

        /// <summary>
        /// Create double representation of given literal
        /// </summary>
        /// <param name="x">Literal value</param>
        /// <returns>Created literal value representation</returns>
        public virtual MemoryEntry DoubleLiteral(DoubleLiteral x)
        {
            return new MemoryEntry(OutSet.CreateDouble((double)x.Value));
        }

        /// <summary>
        /// Create null representation of given literal
        /// </summary>
        /// <param name="x">Literal value</param>
        /// <returns>Created literal value representation</returns>
        public virtual MemoryEntry NullLiteral(NullLiteral x)
        {
            return new MemoryEntry(OutSet.UndefinedValue);
        }

        /// <summary>
        /// Get value representation of given constant
        /// </summary>
        /// <param name="x">Constant</param>
        /// <returns>Represented value</returns>
        public abstract MemoryEntry Constant(GlobalConstUse x);

        /// <summary>
        /// Create object value of given type
        /// </summary>
        /// <param name="typeName">Object type specifier</param>
        /// <returns>Created object</returns>
        public virtual MemoryEntry CreateObject(QualifiedName typeName)
        {
            var declarations = OutSet.ResolveType(typeName);

            var result = new List<ObjectValue>();
            foreach (var declaration in declarations)
            {
                result.Add(OutSet.CreateObject(declaration));
            }

            return new MemoryEntry(result.ToArray());
        }

        public virtual MemoryEntry CreateLambda(LambdaFunctionExpr lambda)
        {
            //TODO: All functions has to be wrapped to common object            
            return new MemoryEntry(new FunctionValue(null));
        }

        #endregion

        /// <summary>
        /// Set current evaluation context
        /// </summary>
        /// <param name="flow">Flow controller available for evaluation</param>
        internal void SetContext(FlowController flow)
        {
            Flow = flow;
        }
    }
}
