using System;
using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Reflection;

namespace Weverca.CodeMetrics.Processing
{
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
