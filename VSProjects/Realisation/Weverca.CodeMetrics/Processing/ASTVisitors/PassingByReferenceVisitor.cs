using System.Collections.Generic;
using System.Reflection;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Reflection;

namespace Weverca.CodeMetrics.Processing.ASTVisitors
{
    /// <summary>
    /// Represents the visitor that finds all calls of functions and static methods
    /// that declares at least one parameter as passing by reference.
    /// </summary>
    class PassingByReferenceVisitor : OccurrenceVisitor
    {
        /// <summary>
        /// Function declarations in the source
        /// </summary>
        private Dictionary<QualifiedName, ScopedDeclaration<DRoutine>> functions;
        /// <summary>
        /// Type declarations in the source
        /// </summary>
        private Dictionary<QualifiedName, PhpType> types;
        /// <summary>
        /// Type whose subtree is traversed at this point
        /// </summary>
        private TypeDecl currentType = null;
        /// <summary>
        /// Name of base class of current type
        /// </summary>
        private GenericQualifiedName? baseClassName = null;

        public PassingByReferenceVisitor(Dictionary<QualifiedName, ScopedDeclaration<DRoutine>>/*!*/
            functions, Dictionary<QualifiedName, PhpType>/*!*/ types)
        {
            // If there exist the same conditional declarations, dictionaries contain only
            // one instance of them, so "may" information cannot be detected correctly
            this.functions = functions;
            this.types = types;
        }

        #region TreeVisitor overrides

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

        public override void VisitDirectFcnCall(DirectFcnCall x)
        {
            // It must be function call, not method call of an object
            // TODO: Implement method call inside class definition through $this pointer
            if (x.IsMemberOf == null)
            {
                ScopedDeclaration<DRoutine> routine;
                if (functions.TryGetValue(x.QualifiedName, out routine))
                {
                    System.Diagnostics.Debug.Assert(routine.Member is PhpFunction);
                    var phpFunction = routine.Member as PhpFunction;

                    // It is not possible to determine what function definition has been declared
                    if (!phpFunction.Declaration.IsConditional)
                    {
                        System.Diagnostics.Debug.Assert(phpFunction.Declaration.GetNode() is FunctionDecl);
                        var declaration = phpFunction.Declaration.GetNode() as FunctionDecl;

                        if (IsPassedByRef(declaration.Signature.FormalParams, x.CallSignature.Parameters))
                        {
                            occurrenceNodes.Push(x);
                        }
                    }
                }
            }

            base.VisitDirectFcnCall(x);
        }

        public override void VisitDirectStMtdCall(DirectStMtdCall x)
        {
            // TODO: Reflection used because field "typeRef" should be public
            var type = x.GetType();
            var field = type.GetField("typeRef", BindingFlags.NonPublic | BindingFlags.Instance);
            var typeRef = field.GetValue(x);

            System.Diagnostics.Debug.Assert(typeRef is DirectTypeRef);
            var directTypeRef = typeRef as DirectTypeRef;

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
                            Debug.Assert(node is TypeDecl);
                            typeNode = node as TypeDecl;
                        }
                    }
                }
            }

            if (typeNode != null)
            {
                foreach (var member in typeNode.Members)
                {
                    if (member is MethodDecl)
                    {
                        var declaration = member as MethodDecl;
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
                                    occurrenceNodes.Push(x);
                                }
                            }
                        }
                    }
                }
            }

            base.VisitDirectStMtdCall(x);
        }

        #endregion

        /// <summary>
        /// Test the number of parameters and whether any parameter is passed by reference
        /// </summary>
        /// <param name="formalParams">The variables used to stand for the values passed by a caller</param>
        /// <param name="actualParams">The values that are passed into the function by a caller</param>
        /// <returns>Returns whether any parameter is passed by reference</returns>
        private bool IsPassedByRef(ICollection<FormalParam>/*!*/ formalParams,
            ICollection<ActualParam>/*!*/ actualParams)
        {
            // Number of parameters must be the same within call and declaraton too
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
