using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework.Memory
{
    public abstract class TypeValue : Value
    {
        public readonly QualifiedName QualifiedName;

        public abstract override void Accept(IValueVisitor visitor);

        internal TypeValue(QualifiedName name)
        {
            QualifiedName = name;
        }
    }

    public class NativeTypeValue : TypeValue
    {
        public readonly NativeTypeDecl Declaration;

        internal NativeTypeValue(NativeTypeDecl declaration)
            :base(declaration.QualifiedName)
        {
            Declaration = declaration;
        }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitNativeTypeValue(this);
        }
    }

    public class SourceTypeValue : TypeValue
    {
        public readonly TypeDecl Declaration;
        internal SourceTypeValue(TypeDecl declaration)
            :base(declaration.Type.QualifiedName)
        {
            Declaration = declaration;
        }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitSourceTypeValue(this);
        }
    }
}
