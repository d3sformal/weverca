using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework.Memory
{
    public class TypeValue : Value
    {
        public readonly ClassDecl Declaration;

        public readonly QualifiedName QualifiedName;

        internal TypeValue(ClassDecl declaration)
        {
            QualifiedName = declaration.QualifiedName;
            Declaration = declaration;
        }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitNativeTypeValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            return new TypeValue(Declaration);
        }

        /// <inheritdoc />
        protected override int getHashCode()
        {
            return GetType().GetHashCode();
        }

        /// <inheritdoc />
        protected override bool equals(Value obj)
        {
            return GetType() == obj.GetType();
        }
    }
}
