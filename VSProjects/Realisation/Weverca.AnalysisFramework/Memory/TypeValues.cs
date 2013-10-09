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
    }

    public class TypeValue : TypeValueBase
    {
        public readonly ObjectDecl Declaration;

        internal TypeValue(ObjectDecl declaration)
            :base(declaration.QualifiedName)
        {
            Declaration = declaration;
        }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitNativeTypeValue(this);
        }
    }
}
