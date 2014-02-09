using System.Collections.Generic;
using System.Reflection;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Reflection;

namespace Weverca.CodeMetrics.Processing.AstVisitors
{
    /// <summary>
    /// Represents the visitor that finds all calls of functions and static methods
    /// that declares at least one parameter as passing by reference.
    /// </summary>
    internal class PassingByReferenceVisitor : OccurrenceVisitor
    {
        /// <summary>
        /// Function declarations in the source.
        /// </summary>
        private Dictionary<QualifiedName, ScopedDeclaration<DRoutine>> functions;

        /// <summary>
        /// Type declarations in the source.
        /// </summary>
        private Dictionary<QualifiedName, PhpType> types;

        /// <summary>
        /// Type whose sub-tree is traversed at this point.
        /// </summary>
        private TypeDecl currentType = null;

        /// <summary>
        /// Name of base class of current type.
        /// </summary>
        private GenericQualifiedName? baseClassName = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PassingByReferenceVisitor" /> class.
        /// </summary>
        /// <param name="functions">Function declarations in the source.</param>
        /// <param name="types">Type declarations in the source.</param>
        public PassingByReferenceVisitor(Dictionary<QualifiedName, ScopedDeclaration<DRoutine>> functions,
            Dictionary<QualifiedName, PhpType> types)
        {
            // If there exist the same conditional declarations, dictionaries contain only
            // one instance of them, so "may" information cannot be detected correctly
            this.functions = functions;
            this.types = types;
        }

        #region TreeVisitor overrides

        /// <inheritdoc />
        public override void VisitTypeDecl(TypeDecl x)
        {
            var previousCurrentType = currentType;
            var previousBaseClassName = baseClassName;

            try
            {
                currentType = x;
                baseClassName = x.BaseClassName;

                base.VisitTypeDecl(x);
            }
            finally
            {
                currentType = previousCurrentType;
                baseClassName = previousBaseClassName;
            }
        }

        /// <inheritdoc />
        public override void VisitDirectFcnCall(DirectFcnCall x)
        {
            // It must be function call, not method call of an object
            // TODO: Implement method call inside class definition through $this pointer
            if (x.IsMemberOf == null)
            {
                ScopedDeclaration<DRoutine> routine;
                if (functions.TryGetValue(x.QualifiedName, out routine))
                {
                    var phpFunction = routine.Member as PhpFunction;
                    Debug.Assert(phpFunction != null,
                        "It must be function, because it is not called as method of a object");

                    // It is not possible to determine what function definition has been declared
                    if (!phpFunction.Declaration.IsConditional)
                    {
                        var declaration = phpFunction.Declaration.GetNode() as FunctionDecl;
                        Debug.Assert(declaration != null,
                            "PhpFunction is always in function declaration node");

                        if (IsPassedByRef(declaration.Signature.FormalParams, x.CallSignature.Parameters))
                        {
                            occurrenceNodes.Enqueue(x);
                        }
                    }
                }
            }

            base.VisitDirectFcnCall(x);
        }

        /// <inheritdoc />
        public override void VisitDirectStMtdCall(DirectStMtdCall x)
        {
            // TODO: Reflection used because field "typeRef" should be public
            var type = x.GetType();
            var field = type.GetField("typeRef", BindingFlags.NonPublic | BindingFlags.Instance);
            var typeRef = field.GetValue(x);

            var directTypeRef = typeRef as DirectTypeRef;
            Debug.Assert(directTypeRef != null);

            TypeDecl typeNode = null;
            if (directTypeRef.ClassName.IsSelfClassName)
            {
                typeNode = currentType;
            }
            else
            {
                QualifiedName? className;
                if (directTypeRef.ClassName.IsParentClassName)
                {
                    if (baseClassName != null)
                    {
                        className = baseClassName.Value.QualifiedName;
                    }
                    else
                    {
                        className = null;
                    }
                }
                else
                {
                    className = directTypeRef.ClassName;
                }

                if (className != null)
                {
                    PhpType phpType;
                    if (types.TryGetValue(className.Value, out phpType))
                    {
                        // It is not possible to determine what type definition has been declared
                        if (!phpType.Declaration.IsConditional)
                        {
                            var node = phpType.Declaration.GetNode();
                            typeNode = node as TypeDecl;
                            Debug.Assert(typeNode != null, "PhpType is always in type declaration node");
                        }
                    }
                }
            }

            if (typeNode != null)
            {
                foreach (var member in typeNode.Members)
                {
                    var declaration = member as MethodDecl;
                    if (declaration != null)
                    {
                        if (declaration.Name.Equals(x.MethodName))
                        {
                            if (directTypeRef.ClassName.IsSelfClassName
                                || directTypeRef.ClassName.IsParentClassName
                                // If the method of named class is not static, thus it is an error!
                                || declaration.Modifiers == PhpMemberAttributes.Static)
                            {
                                if (IsPassedByRef(declaration.Signature.FormalParams,
                                    x.CallSignature.Parameters))
                                {
                                    occurrenceNodes.Enqueue(x);
                                }
                            }
                        }
                    }
                }
            }

            base.VisitDirectStMtdCall(x);
        }

        #endregion TreeVisitor overrides

        /// <summary>
        /// Test the number of parameters and whether any parameter is passed by reference.
        /// </summary>
        /// <param name="formalParams">The variables used to stand for the values passed by a caller.</param>
        /// <param name="actualParams">The values that are passed into the function by a caller.</param>
        /// <returns>Returns whether any parameter is passed by reference.</returns>
        private static bool IsPassedByRef(ICollection<FormalParam> formalParams,
            ICollection<ActualParam> actualParams)
        {
            // Number of parameters must be the same within call and declaraton too.
            if (actualParams.Count == formalParams.Count)
            {
                foreach (var param in formalParams)
                {
                    // Expression passed by a caller must be variable (DirectVarUse) or new construct (NewEx)
                    if (param.PassedByRef)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
