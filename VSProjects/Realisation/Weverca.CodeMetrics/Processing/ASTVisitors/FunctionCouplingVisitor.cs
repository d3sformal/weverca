using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Reflection;

namespace Weverca.CodeMetrics.Processing.AstVisitors
{
    /// <summary>
    /// Represents the visitor that finds all couplings from a given standard function.
    /// </summary>
    internal class FunctionCouplingVisitor : TreeVisitor
    {
        /// <summary>
        /// Standard function whose sub-tree is traversed for function references at this point.
        /// </summary>
        private readonly PhpFunction currentFunction;

        /// <summary>
        /// References to other functions used within the current function body.
        /// </summary>
        private readonly Dictionary<QualifiedName, DirectFcnCall> currentReferences
            = new Dictionary<QualifiedName, DirectFcnCall>();

        /// <summary>
        /// Indicate whether function sub-tree is traversing. No inside function is accessed.
        /// </summary>
        private bool isInsideFunction = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionCouplingVisitor" /> class.
        /// </summary>
        /// <param name="function">Function whose sub-tree is traversed for function references.</param>
        public FunctionCouplingVisitor(PhpFunction function)
        {
            currentFunction = function;
        }

        /// <summary>
        /// Return all nodes with references to functions that occur within a given function body.
        /// </summary>
        /// <returns>References of functions within a given function body.</returns>
        public DirectFcnCall[] GetReferences()
        {
            var couplings = new DirectFcnCall[currentReferences.Count];

            // Copy result to an array to make it immutable
            currentReferences.Values.CopyTo(couplings, 0);
            return couplings;
        }

        #region TreeVisitor overrides

        /// <inheritdoc />
        public override void VisitFunctionDecl(FunctionDecl x)
        {
            // The function can be declared inside other function or method body
            // In that case, traversing the tree of the inside function is not necessary
            if (!isInsideFunction)
            {
                var previousValue = isInsideFunction;
                try
                {
                    isInsideFunction = true;
                    currentReferences.Clear();

                    base.VisitFunctionDecl(x);
                }
                finally
                {
                    isInsideFunction = previousValue;
                }
            }
        }

        /// <inheritdoc />
        public override void VisitDirectVarUse(DirectVarUse x)
        {
            // Direct access to field: $object->field
            FindFunctionCall(x);

            base.VisitDirectVarUse(x);
        }

        /// <inheritdoc />
        public override void VisitIndirectVarUse(IndirectVarUse x)
        {
            // Access to field of a name stored in variable-like construct: $object->$field
            FindFunctionCall(x);

            base.VisitIndirectVarUse(x);
        }

        /// <inheritdoc />
        public override void VisitItemUse(ItemUse x)
        {
            // Access to array: $object->array[number]
            FindFunctionCall(x);

            base.VisitItemUse(x);
        }

        /// <inheritdoc />
        public override void VisitDirectFcnCall(DirectFcnCall x)
        {
            // Direct method call: $object->method()
            FindFunctionCall(x);

            base.VisitDirectFcnCall(x);
        }

        /// <inheritdoc />
        public override void VisitIndirectFcnCall(IndirectFcnCall x)
        {
            // Variable method call: $object->$method()
            FindFunctionCall(x);

            base.VisitIndirectFcnCall(x);
        }

        #endregion TreeVisitor overrides

        /// <summary>
        /// Identify a function call in l-value expression.
        /// </summary>
        /// <param name="x">Variable-like, l-value expression.</param>
        private void FindFunctionCall(VarLikeConstructUse x)
        {
            // Function is not member of anything
            while (x.IsMemberOf != null)
            {
                x = x.IsMemberOf;
            }

            var call = x as DirectFcnCall;
            if (call != null)
            {
                AddCoupling(call);
            }
        }

        /// <summary>
        /// Add found reference to a function used within the currently traversed function body.
        /// </summary>
        /// <param name="x">Reference to standard function.</param>
        private void AddCoupling(DirectFcnCall x)
        {
            Debug.Assert(isInsideFunction, "Function declared inside function is not traversed");

            // Presence of itself is not taken into account
            // Function names are case-insensitive
            if (!x.QualifiedName.Equals(currentFunction.QualifiedName))
            {
                currentReferences[x.QualifiedName] = x;
            }
        }
    }
}
