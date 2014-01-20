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
        /// Gets InSet's snapshot
        /// </summary>
        public SnapshotBase InSnapshot
        {
            get { return InSet.Snapshot; }
        }

        /// <summary>
        /// Gets OutSet's snapshot
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

        /// <summary>
        /// Creates member identifier of possible names from the given values
        /// </summary>
        /// <param name="memberRepresentation">List of values used as member</param>
        /// <returns>Identifier of member created from possible memory entry values</returns>
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
        public abstract ReadWriteSnapshotEntryBase ResolveField(ReadSnapshotEntryBase objectValue,
            VariableIdentifier field);

        /// <summary>
        /// Resolves value at indexedValue[index]
        /// </summary>
        /// <param name="indexedValue">Value which index is resolved</param>
        /// <param name="index">Specifier of an index</param>
        /// <returns>Possible values obtained from resolving given index</returns>
        public abstract ReadWriteSnapshotEntryBase ResolveIndex(ReadSnapshotEntryBase indexedValue,
            MemberIdentifier index);

        /// <summary>
        /// Assign possible aliases to given target
        /// </summary>
        /// <param name="target">Target variable specifier</param>
        /// <param name="aliasedValue">Possible aliases to be assigned</param>
        public abstract void AliasAssign(ReadWriteSnapshotEntryBase target,
            ReadSnapshotEntryBase aliasedValue);

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
        public abstract void Foreach(MemoryEntry enumeree, ReadWriteSnapshotEntryBase keyVariable,
            ReadWriteSnapshotEntryBase valueVariable);

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
        /// Determine whether all variables are set and are not-NULL.
        /// </summary>
        /// <param name="variables">Variables to be checked</param>
        /// <returns>
        /// <c>true</c> whether all variables are defined and no value is <c>null</c>, <c>false</c> whether
        /// at least one variable is undefined or has one <c>null</c> value and otherwise any boolean value.
        /// </returns>
        public abstract MemoryEntry IssetEx(IEnumerable<VariableIdentifier> variables);

        /// <summary>
        /// Determine whether a variable is considered to be empty (i.e. null, false, 0, 0.0, "" etc.)
        /// </summary>
        /// <param name="variable">A variable to be checked</param>
        /// <returns><c>true</c> whether a variable is empty, otherwise <c>false</c></returns>
        public abstract MemoryEntry EmptyEx(VariableIdentifier variable);

        /// <summary>
        /// Terminates execution of the PHP code. It gives order to analysis to jump at the end of program.
        /// </summary>
        /// <param name="exit"><c>exit</c> expression</param>
        /// <param name="status">Exit status printed if it is string or returned if it is integer</param>
        /// <returns>Anything, return value is ignored</returns>
        public abstract MemoryEntry Exit(ExitEx exit, MemoryEntry status);

        /// <summary>
        /// Get value representation of given constant
        /// </summary>
        /// <param name="x">Constant representation</param>
        /// <returns>Represented value</returns>
        public abstract MemoryEntry Constant(GlobalConstUse x);

        /// <summary>
        /// Get value representation of given class constant
        /// </summary>
        /// <param name="thisObject">Object</param>
        /// <param name="variableName">Constant name</param>
        /// <returns>Value of class constant</returns>
        public abstract MemoryEntry ClassConstant(MemoryEntry thisObject, VariableName variableName);

        /// <summary>
        /// Get value representation of given class constant
        /// </summary>
        /// <param name="genericQualifiedName">Class Name</param>
        /// <param name="variableName">Constant name</param>
        /// <returns>Value of class constant</returns>
        public abstract MemoryEntry ClassConstant(QualifiedName qualifiedName, VariableName variableName);


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
        /// <returns>Memory entry with all new possible objects</returns>
        public abstract MemoryEntry CreateObject(QualifiedName typeName);

        /// <summary>
        /// Create new objects of type with name that is evaluated from expression (<c>$obj = new $exp;</c>)
        /// </summary>
        /// <param name="possibleNames">Values determining the name of class</param>
        /// <returns>Memory entry with all new possible objects</returns>
        public abstract MemoryEntry IndirectCreateObject(MemoryEntry possibleNames);

        /// <summary>
        /// Determine whether an expression is instance of a class or interface
        /// </summary>
        /// <param name="expression">Expression to be determined whether it is instance of a class</param>
        /// <param name="typeName"> Name of the type that the expression is checked to</param>
        /// <returns>
        /// <c>true</c> whether all values are objects inherited from the type, <c>false</c> whether
        /// values are not objects or not instances of the type and otherwise any boolean value.
        /// </returns>
        public abstract MemoryEntry InstanceOfEx(MemoryEntry expression, QualifiedName typeName);

        /// <summary>
        /// Determine whether an expression is instance of indirectly resolved class or interface
        /// </summary>
        /// <param name="expression">Expression to be determined whether it is instance of a class</param>
        /// <param name="possibleNames">Possible names of type determined by values of an expression</param>
        /// <returns>
        /// <c>true</c> whether all values are objects inherited from the types, <c>false</c> whether
        /// values are not objects or not instances of the types and otherwise any boolean value.
        /// </returns>
        public abstract MemoryEntry IndirectInstanceOfEx(MemoryEntry expression, MemoryEntry possibleNames);

        #endregion

        #region Default implementation of simple routines

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

        /// <summary>
        /// Creates new function implementation without name from lambda declaration
        /// </summary>
        /// <param name="lambda">Definition of lambda function</param>
        /// <returns>New function implementation</returns>
        public virtual MemoryEntry CreateLambda(LambdaFunctionExpr lambda)
        {
            return new MemoryEntry(OutSet.CreateFunction(lambda));
        }

        /// <summary>
        /// Creates initialized object according to the given type
        /// </summary>
        /// <param name="type">Type that is always needed when creating new object</param>
        /// <returns>New initialized object</returns>
        protected ObjectValue CreateInitializedObject(TypeValue type)
        {
            var newObject = OutSet.CreateObject(type);

            var typeDeclaration = type as TypeValue;
            if (typeDeclaration != null)
            {
                // TODO: Initialize all fields with its default or initialization value
                var initializer = new ObjectInitializer(this);
                initializer.InitializeObject(newObject, typeDeclaration.Declaration);
            }

            return newObject;
        }

        /// <summary>
        /// Fetches variables of given name from global to local function scope when global keyword is used
        /// </summary>
        /// <param name="variables">Variables that are fetched from global scope</param>
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
