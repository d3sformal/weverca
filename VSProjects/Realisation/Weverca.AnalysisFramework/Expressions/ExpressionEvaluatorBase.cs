using System;
using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework.Expressions
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
        public FlowOutputSet OutSet
        {
            get { return Flow.OutSet; }
        }

        /// <summary>
        /// Gets current input set of expression evaluation
        /// </summary>
        public FlowInputSet InSet
        {
            get { return Flow.InSet; }
        }

        /// <summary>
        /// InSet's snapshot
        /// </summary>
        public SnapshotBase InSnapshot
        {
            get { return InSet.Snapshot; }
        }

        /// <summary>
        /// OutSet's snapshot
        /// </summary>
        public SnapshotBase OutSnapshot
        {
            get { return OutSet.Snapshot; }
        }

        /// <summary>
        /// Gets element which is currently evaluated
        /// </summary>
        public LangElement Element
        {
            get { return Flow.CurrentPartial; }
        }

        #region Template API methods for implementors

        /// <summary>
        /// Resolves possible name of variable identifier by value
        /// NOTE:
        ///     Is used for resolving indirect variable usages
        /// </summary>
        /// <param name="variableSpecifier">Value representing possible names of variable</param>
        /// <returns>Possible variable names</returns>
        public abstract IEnumerable<string> VariableNames(MemoryEntry variableSpecifier);


        public abstract MemberIdentifier MemberIdentifier(MemoryEntry memberRepresentation);

        /// <summary>
        /// Resolves value, determined by given variable specifier
        /// </summary>
        /// <param name="variable">Specifier of resolved variable</param>
        /// <returns>Possible values obtained from resolving variable specifier</returns>
        public abstract ReadWriteSnapshotEntryBase ResolveVariable(VariableIdentifier variable);

        /// <summary>
        /// Resolves value determined by given variable specifier that is accessed by array index
        /// NOTE:
        ///     Is useful for implicit array creation
        /// </summary>
        /// <param name="variable">Variable which is indexed</param>
        /// <returns>Possible values obtained from resolving indexed variable</returns>
        public abstract MemoryEntry ResolveIndexedVariable(VariableIdentifier variable);

        /// <summary>
        /// Resolves value, determined by given field specifier
        /// </summary>
        /// <param name="objectValue">Object value which field is resolved</param>
        /// <param name="field">Specifier of resolved field</param>
        /// <returns>Possible values obtained from resolving given field</returns>
        public abstract ReadWriteSnapshotEntryBase ResolveField(ReadSnapshotEntryBase objectValue, VariableIdentifier field);

        /// <summary>
        /// Resolves value at indexedValue[index]
        /// </summary>
        /// <param name="indexedValue">Value which index is resolved</param>
        /// <param name="index">Specifier of an index</param>
        /// <returns>Possible values obtained from resolving given index</returns>
        public abstract ReadWriteSnapshotEntryBase ResolveIndex(ReadSnapshotEntryBase indexedValue, MemberIdentifier index);

        /// <summary>
        /// Resolves alias from given field specifier
        /// </summary>
        /// <param name="objectValue">Object containing aliased field</param>
        /// <param name="aliasedField">Specifier of an field</param>
        /// <returns>Resolved aliases</returns>
        public abstract IEnumerable<AliasValue> ResolveAliasedField(MemoryEntry objectValue,
            VariableIdentifier aliasedField);

        /// <summary>
        /// Resolves alias from given index specifier
        /// </summary>
        /// <param name="arrayValue">Array containing aliased index</param>
        /// <param name="aliasedIndex">Specifier of an field</param>
        /// <returns>Resolved aliases</returns>
        public abstract IEnumerable<AliasValue> ResolveAliasedIndex(MemoryEntry arrayValue,
            MemoryEntry aliasedIndex);

        /// <summary>
        /// Assign possible aliases to given target
        /// </summary>
        /// <param name="target">Target variable specifier</param>
        /// <param name="possibleAliases">Possible aliases to be assigned</param>
        public abstract void AliasAssign(ReadWriteSnapshotEntryBase target, IEnumerable<AliasEntry> possibleAliases);

        /// <summary>
        /// Assign possible aliases to given object field
        /// </summary>
        /// <param name="objectValue">Object containing assigned field</param>
        /// <param name="aliasedField">Specifier of an field</param>
        /// <param name="possibleAliases">Possible aliases to be assigned</param>
        public abstract void AliasedFieldAssign(MemoryEntry objectValue, VariableIdentifier aliasedField,
            IEnumerable<AliasValue> possibleAliases);

        /// <summary>
        /// Assign possible aliases to given array index
        /// </summary>
        /// <param name="arrayValue">Array containing assigned index</param>
        /// <param name="aliasedIndex">Specifier of an index</param>
        /// <param name="possibleAliases">Possible aliases to be assigned</param>
        public abstract void AliasedIndexAssign(MemoryEntry arrayValue, MemoryEntry aliasedIndex,
            IEnumerable<AliasValue> possibleAliases);

        /// <summary>
        /// Assign possible values to given target
        /// </summary>
        /// <param name="target">Target snapshot entry</param>
        /// <param name="entry">Possible values to be assigned</param>
        public abstract void Assign(ReadWriteSnapshotEntryBase target, MemoryEntry entry);

        /// <summary>
        /// Assign possible values to given targetField of an objectValue
        /// </summary>
        /// <param name="objectValue">Object containing assigned field</param>
        /// <param name="targetField">Specifier of an field</param>
        /// <param name="assignedValue">Possible values to be assigned</param>
        public abstract void FieldAssign(ReadSnapshotEntryBase objectValue, VariableIdentifier targetField,
            MemoryEntry assignedValue);

        /// <summary>
        /// Assign assignedValue at indexedValue[index]
        /// NOTE:
        ///     Array/object/string can be indexed
        /// </summary>
        /// <param name="indexedValue">Value which index is assigned</param>
        /// <param name="index">Specifier of an index</param>
        /// <param name="assignedValue">Value that is assigned</param>
        public abstract void IndexAssign(ReadSnapshotEntryBase indexedValue, MemoryEntry index,
            MemoryEntry assignedValue);

        /// <summary>
        /// Process binary operation on given operands
        /// </summary>
        /// <param name="leftOperand">Left operand of operation</param>
        /// <param name="operation">Binary operation</param>
        /// <param name="rightOperand">Right operand of operation</param>
        /// <returns>Result of binary expression</returns>
        public abstract MemoryEntry BinaryEx(MemoryEntry leftOperand, Operations operation,
            MemoryEntry rightOperand);

        /// <summary>
        /// Process unary operation on given operand
        /// </summary>
        /// <param name="operation">Unary operation</param>
        /// <param name="operand">Operand of operation</param>
        /// <returns>Result of unary expression</returns>
        public abstract MemoryEntry UnaryEx(Operations operation, MemoryEntry operand);

        /// <summary>
        /// Process increment or decrement operation, in both prefix and postfix form
        /// </summary>
        /// <remarks>This method doesn't provide assigning into incremented LValue</remarks>
        /// <param name="operation">Increment or decrement operation</param>
        /// <param name="incrementedValue">Value that has to be incremented/decremented</param>
        /// <returns>Result of incremented or decremented</returns>
        public abstract MemoryEntry IncDecEx(IncDecEx operation, MemoryEntry incrementedValue);

        /// <summary>
        /// Process n-ary operation on given operands
        /// </summary>
        /// <param name="keyValuePairs">Collection of key/value pairs that initialize the new array</param>
        /// <returns>Result of n-ary expression</returns>
        public abstract MemoryEntry ArrayEx(
            IEnumerable<KeyValuePair<MemoryEntry, MemoryEntry>> keyValuePairs);

        /// <summary>
        /// Process foreach statement on given variables
        /// </summary>
        /// <remarks>
        /// Is intended to store all possible values from enumeration into keyVariable and valueVariable
        /// </remarks>
        /// <param name="enumeree">Enumerated value</param>
        /// <param name="keyVariable">Variable where keys are stored</param>
        /// <param name="valueVariable">Variable where values are stored</param>
        public abstract void Foreach(MemoryEntry enumeree, VariableIdentifier keyVariable,
            VariableIdentifier valueVariable);

        /// <summary>
        /// Process concatenation of given parts
        /// </summary>
        /// <param name="parts">Concatenated parts</param>
        /// <returns>Concatenation of parts</returns>
        public abstract MemoryEntry Concat(IEnumerable<MemoryEntry> parts);

        /// <summary>
        /// Process echo statement with given values
        /// </summary>
        /// <param name="echo">Echo statement</param>
        /// <param name="entries">Values to be converted to string and printed out</param>
        public abstract void Echo(EchoStmt echo, MemoryEntry[] entries);

        /// <summary>
        /// Get value representation of given constant
        /// </summary>
        /// <param name="x">Constant representation</param>
        /// <returns>Represented value</returns>
        public abstract MemoryEntry Constant(GlobalConstUse x);

        /// <summary>
        /// Is called on <c>const x = 5</c> declarations
        /// </summary>
        /// <param name="x">Constant declaration</param>
        /// <param name="constantValue">Value assigned into constant</param>
        public abstract void ConstantDeclaration(ConstantDecl x, MemoryEntry constantValue);

        /// <summary>
        /// Create object value of given type
        /// </summary>
        /// <param name="typeName">Object type specifier</param>
        /// <returns>Created object</returns>
        public abstract MemoryEntry CreateObject(QualifiedName typeName);

        #endregion

        #region Default implementation of simple routines

        /// <summary>
        /// Resolve alias of given variable specifier
        /// </summary>
        /// <param name="variable">Aliased variable specifier</param>
        /// <returns>Resolved aliases</returns>
        public virtual IEnumerable<AliasValue> ResolveAlias(VariableIdentifier variable)
        {
            var possibleNames = variable.PossibleNames;
            var aliases = new List<AliasValue>(possibleNames.Length);
            foreach (var aliasedVariable in possibleNames)
            {
                aliases.Add(InSet.CreateAlias(aliasedVariable));
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
            return new MemoryEntry(OutSet.CreateString(x.Value as string));
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

        internal MemoryEntry IndirectCreateObject(MemoryEntry memoryEntry)
        {
            var declarations = new HashSet<TypeValueBase>();

            foreach (StringValue name in memoryEntry.PossibleValues)
            {
                var qualifiedName = new QualifiedName(new Name(name.Value));
                declarations.UnionWith(OutSet.ResolveType(qualifiedName));
            }

            var result = new List<ObjectValue>();
            foreach (var declaration in declarations)
            {
                result.Add(OutSet.CreateObject(declaration));
            }

            return new MemoryEntry(result.ToArray());
        }

        public virtual MemoryEntry CreateLambda(LambdaFunctionExpr lambda)
        {
            return new MemoryEntry(OutSet.CreateFunction(lambda));
        }

        /// <summary>
        /// Creates initialized object according to the given type
        /// </summary>
        /// <param name="type">Type that is always needed when creating new object</param>
        /// <returns>New initialized object</returns>
        protected ObjectValue CreateInitializedObject(TypeValueBase type)
        {
            var newObject = OutSet.CreateObject(type);

            var sourceDeclaration = type as TypeValue;
            if (sourceDeclaration != null)
            {
                var initializer = new ObjectInitializer(this);
                initializer.InitializeObject(newObject, sourceDeclaration.Declaration);
            }
            else
            {
                var nativeDeclaration = type as TypeValue;
                if (nativeDeclaration != null)
                {
                    // TODO: Initialize all fields with its default or initialization value
                }
            }

            return newObject;
        }

        public virtual void GlobalStatement(IEnumerable<VariableIdentifier> variables)
        {
            foreach (var variable in variables)
            {
                OutSet.FetchFromGlobal(variable.PossibleNames);
            }
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
