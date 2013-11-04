using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework.Memory
{
    public abstract class TypeValueBase : Value
    {
        public readonly QualifiedName QualifiedName;

        public abstract override void Accept(IValueVisitor visitor);

        internal TypeValueBase(QualifiedName name)
        {
            QualifiedName = name;
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

    public class TypeValue : TypeValueBase
    {
        public readonly ClassDecl Declaration;

        internal TypeValue(ClassDecl declaration)
            :base(declaration.QualifiedName)
        {
            Declaration = declaration;
        }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitNativeTypeValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            throw new System.NotImplementedException();
        }
    }
}
