using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.ASTVisitors
{
    /// <summary>
    /// Represents the visitor that finds all accesses to members of objects whose type
    /// cannot be resolved statically as they are stored within a variable
    /// </summary>
    class DuckTypingVisitor : OccurrenceVisitor
    {
        #region TreeVisitor overrides

        public override void VisitDirectVarUse(DirectVarUse x)
        {
            // Direct access to field: $object->field
            FindUndefinedMembersUse(x);
            base.VisitDirectVarUse(x);
        }

        public override void VisitIndirectVarUse(IndirectVarUse x)
        {
            // Access to field of a name stored in variable-like construct: $object->$field
            FindUndefinedMembersUse(x);
            base.VisitIndirectVarUse(x);
        }

        public override void VisitItemUse(ItemUse x)
        {
            // Access to array: $object->array[number]
            FindUndefinedMembersUse(x);
            base.VisitItemUse(x);
        }

        public override void VisitDirectFcnCall(DirectFcnCall x)
        {
            // Direct method call: $object->method()
            FindUndefinedMembersUse(x);
            base.VisitDirectFcnCall(x);
        }

        public override void VisitIndirectFcnCall(IndirectFcnCall x)
        {
            if (x.IsMemberOf != null)
            {
                // Variable method call: $object->$method()
                FindUndefinedMembersUse(x);
            }
            else
            {
                // Or __invoke method call if variable is a object
                occurrenceNodes.Push(x);
            }
            base.VisitIndirectFcnCall(x);
        }

        #endregion

        /// <summary>
        /// Find all possible undefined members of a object within the variable-like expression
        /// </summary>
        /// <param name="x">Variable-like expression possibly using members of a object</param>
        private void FindUndefinedMembersUse(VarLikeConstructUse/*!*/ x)
        {
            // Every access to a field or method call can be seen as duck typing,
            // because the type is not known without run-time information

            if (x.IsMemberOf != null)
            {
                // The type of x is known to be proper
                occurrenceNodes.Push(x);
            }
            else
            {
                return;
            }

            for (x = x.IsMemberOf; x.IsMemberOf != null; x = x.IsMemberOf)
            {
                // CompoundVarUse is supertype of DirectVarUse, IndirectVarUse and ItemUse
                // DirectVarUse can be $this, detect by DirectVarUse.VarName.IsThisVariableName
                if ((x is CompoundVarUse)
                    || (x is DirectFcnCall)
                    || (x is IndirectFcnCall))
                {
                    // Variable-like construct is possible member of a object
                    occurrenceNodes.Push(x);
                }
            }

            if (x is IndirectFcnCall)
            {
                // __invoke method call if call is not member of a object. Otherwise call of __invoke
                // declared within object stored in a field is handled as method call
                occurrenceNodes.Push(x);
            }
        }
    }
}
