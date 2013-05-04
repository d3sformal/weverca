using System;
using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Reflection;

namespace Weverca.CodeMetrics.Processing
{
    /// <summary>
    /// Enriched tree with a place to store AST nodes which represent some wanted language construct
    /// </summary>
    class OccurrenceTreeVisitor : TreeVisitor
    {
        /// <summary>
        /// All occurrences of searched language constructs
        /// </summary>
        protected Stack<AstNode> occurrenceNodes = new Stack<AstNode>();

        /// <summary>
        /// Return all nodes with occurences of searched language constructs
        /// </summary>
        /// <returns>Occurrences of appropriate nodes</returns>
        internal IEnumerable<AstNode> GetOccurrences()
        {
            // Copy result to an array to make it immutable
            return occurrenceNodes.ToArray();
        }

        /// <summary>
        /// Remove all stored occurrences
        /// </summary>
        internal void ResetContent()
        {
            occurrenceNodes.Clear();
        }
    }


    /// <summary>
    /// Visitor which collect function calls
    /// </summary>
    class CallVisitor : TreeVisitor
    {
        HashSet<string> searchedCalls;
        List<AstNode> foundCalls = new List<AstNode>();

        /// <summary>
        /// Create call visitor, which collect occurances of given functions
        /// </summary>
        /// <param name="functions"></param>
        public CallVisitor(IEnumerable<string> functions)
        {
            searchedCalls = new HashSet<string>(functions);
        }

        /// <summary>
        /// Returns calls which were founded during visiting tree
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<AstNode> GetCalls()
        {
            //Copy result because of make it immutable
            return foundCalls.ToArray();
        }

        #region TreeVisitor overrides
        public override void VisitDirectFcnCall(DirectFcnCall x)
        {
            if (isSearched(x.QualifiedName))
            {
                foundCalls.Add(x);
            }
            base.VisitDirectFcnCall(x);
        }
        
        /// <summary>
        /// Phalanger resolves eval as special expression
        /// </summary>
        /// <param name="x"></param>
        public override void VisitEvalEx(EvalEx x)
        {
            if (searchedCalls.Contains("eval"))
            {
                foundCalls.Add(x);
            }
        }
        #endregion

        
        #region Private utilities for function matching

        private bool isSearched(QualifiedName qualifiedName)
        {
            var name = qualifiedName.Name.Value;
            return searchedCalls.Contains(name);
        }

        #endregion
    }


    /// <summary>
    /// Represents the visitor that finds all types declared in the body of a subroutine
    /// </summary>
    class InsideFunctionDeclarationVisitor : OccurrenceTreeVisitor
    {
        private delegate void VisitSubroutineExprDelegate<T>(T x) where T : AstNode;

        /// <summary>
        /// Indicate whether subtree of a subroutine is traversing at this point
        /// </summary>
        private bool isInsideSubroutine = false;

        #region TreeVisitor overrides

        public override void VisitMethodDecl(MethodDecl x)
        {
            VisitSubroutineExpr<MethodDecl>(base.VisitMethodDecl, x);
        }

        public override void VisitFunctionDecl(FunctionDecl x)
        {
            if (isInsideSubroutine)
            {
                occurrenceNodes.Push(x);
            }
            VisitSubroutineExpr<FunctionDecl>(base.VisitFunctionDecl, x);
        }

        public override void VisitLambdaFunctionExpr(LambdaFunctionExpr x)
        {
            VisitSubroutineExpr<LambdaFunctionExpr>(base.VisitLambdaFunctionExpr, x);
        }

        public override void VisitTypeDecl(TypeDecl x)
        {
            // Declaration of classes and interfaces too is non-standard construct
            if (((x.AttributeTarget & PhpAttributeTargets.Types) != 0)
                && (isInsideSubroutine))
            {
                // All classes and interfaces declared inside a function
                occurrenceNodes.Push(x);
            }

            base.VisitTypeDecl(x);
        }

        #endregion

        /// <summary>
        /// Set that visitor is traversing subtree of declaration and body of a subroutine
        /// </summary>
        /// <typeparam name="T">The specific type of AST node</typeparam>
        /// <param name="overriddenMethod">Method of AST which is overridden</param>
        /// <param name="x">AST node of the subroutine declaration</param>
        private void VisitSubroutineExpr<T>(VisitSubroutineExprDelegate<T>/*!*/ overriddenMethod,
            T x) where T : AstNode
        {
            if (isInsideSubroutine)
            {
                overriddenMethod(x);
                Debug.Assert(isInsideSubroutine);
            }
            else
            {
                isInsideSubroutine = true;
                try
                {
                    overriddenMethod(x);
                    Debug.Assert(isInsideSubroutine);
                }
                finally
                {
                    isInsideSubroutine = false;
                }
            }
        }
    }


    /// <summary>
    /// Visitor which collect super global variable usage
    /// </summary>
    class SuperGlobalVarVisitor : TreeVisitor
    {
        List<AstNode> foundSuperGlobals = new List<AstNode>();

        /// <summary>
        /// Returns calls which were founded during visiting tree
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<AstNode> GetVariables()
        {
            //Copy result because of make it immutable
            return foundSuperGlobals.ToArray();
        }

        #region TreeVisitor overrides

        public override void VisitDirectVarUse(DirectVarUse x)
        {
            var name=x.VarName;
            if (name.IsAutoGlobal)
            {
                foundSuperGlobals.Add(x);
            }

            base.VisitDirectVarUse(x);
        }

        #endregion
    }


    /// <summary>
    /// Represents the visitor that finds all couplings from a given type (class or interface)
    /// </summary>
    class ClassCouplingVisitor : TreeVisitor
    {
        /// <summary>
        /// Class or interface whose subtree is traversed for type references at this point
        /// </summary>
        private readonly PhpType currentType;
        /// <summary>
        /// References to other types used within the current type declaration
        /// </summary>
        private readonly Dictionary<QualifiedName, DirectTypeRef> currentReferences
            = new Dictionary<QualifiedName, DirectTypeRef>();
        /// <summary>
        /// Indicate whether type subtree is traversing. No subtype is accessed
        /// </summary>
        private bool isInsideDeclaration = false;

        public ClassCouplingVisitor(PhpType/*!*/ type)
        {
            currentType = type;
        }

        /// <summary>
        /// Return all nodes with references to types that occur within a given type declaration
        /// </summary>
        /// <returns>References of types within a given type declaration</returns>
        public DirectTypeRef[] GetReferences()
        {
            var couplings = new DirectTypeRef[currentReferences.Count];
            // Copy result to an array to make it immutable
            currentReferences.Values.CopyTo(couplings, 0);
            return couplings;
        }

        /// <summary>
        /// Add found reference to a type used within the currently traversed type declaration
        /// </summary>
        /// <param name="x">Reference to class or interface</param>
        public void AddCoupling(DirectTypeRef/*!*/ x)
        {
            Debug.Assert(isInsideDeclaration);

            // Presence of itself is not taken into account
            // Class names are case-insensitive
            if (!x.ClassName.Equals(currentType.QualifiedName))
            {
                currentReferences[x.ClassName] = x;
            }
        }

        #region TreeVisitor overrides

        public override void VisitTypeDecl(TypeDecl x)
        {
            // As a type, we consider class and interface too
            if ((x.AttributeTarget & PhpAttributeTargets.Types) != 0)
            {
                // The type can be declared inside method body of other class
                // In that case, traversing the tree of subclass or subinterface is not necessary
                if (!isInsideDeclaration)
                {
                    var previousValue = isInsideDeclaration;
                    try
                    {
                        isInsideDeclaration = true;
                        currentReferences.Clear();
                        base.VisitTypeDecl(x);
                    }
                    finally
                    {
                        isInsideDeclaration = previousValue;
                    }
                }
            }
            else
            {
                base.VisitTypeDecl(x);
            }
        }

        // TODO: StaticMtdCall.typeRef and StaticFieldUse.typeRef contain information about class
        // on the left side, but it cannot be accessed, because typeRef is private field
        // public override void VisitDirectStMtdCall(DirectStMtdCall x)
        // public override void VisitIndirectStMtdCall(IndirectStMtdCall x)
        // public override void VisitDirectStFldUse(DirectStFldUse x)
        // public override void VisitIndirectStFldUse(IndirectStFldUse x)

        public override void VisitNewEx(NewEx x)
        {
            // Use of type can only be determined by DirectTypeRef
            if (x.ClassNameRef is DirectTypeRef)
            {
                var typeRef = x.ClassNameRef as DirectTypeRef;
                AddCoupling(typeRef);
            }
            base.VisitNewEx(x);
        }

        public override void VisitDirectTypeRef(DirectTypeRef x)
        {
            AddCoupling(x);
            base.VisitDirectTypeRef(x);
        }

        #endregion
    }
}
