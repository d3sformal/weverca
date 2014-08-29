/*
Copyright (c) 2012-2014 Miroslav Vodolan, Matyas Brenner, David Skorvaga, David Hauzar.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Reflection;

namespace Weverca.CodeMetrics.Processing.AstVisitors
{
    /// <summary>
    /// Represents the visitor that finds all couplings from a given type (class or interface).
    /// </summary>
    internal class ClassCouplingVisitor : TreeVisitor
    {
        /// <summary>
        /// Class or interface whose sub-tree is traversed for type references at this point.
        /// </summary>
        private readonly PhpType currentType;

        /// <summary>
        /// References to other types used within the current type declaration.
        /// </summary>
        private readonly Dictionary<QualifiedName, DirectTypeRef> currentReferences
            = new Dictionary<QualifiedName, DirectTypeRef>();

        /// <summary>
        /// Indicate whether type sub-tree is traversing. No sub-type is accessed.
        /// </summary>
        private bool isInsideDeclaration = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassCouplingVisitor" /> class.
        /// </summary>
        /// <param name="type"></param>
        public ClassCouplingVisitor(PhpType type)
        {
            currentType = type;
        }

        /// <summary>
        /// Return all nodes with references to types that occur within a given type declaration.
        /// </summary>
        /// <returns>References of types within a given type declaration.</returns>
        public DirectTypeRef[] GetReferences()
        {
            var couplings = new DirectTypeRef[currentReferences.Count];

            // Copy result to an array to make it immutable
            currentReferences.Values.CopyTo(couplings, 0);
            return couplings;
        }

        /// <summary>
        /// Add found reference to a type used within the currently traversed type declaration.
        /// </summary>
        /// <param name="x">Reference to class or interface.</param>
        public void AddCoupling(DirectTypeRef x)
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override void VisitDirectTypeRef(DirectTypeRef x)
        {
            AddCoupling(x);
            base.VisitDirectTypeRef(x);
        }

        #endregion TreeVisitor overrides
    }
}