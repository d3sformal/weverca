using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Value storing information about class declaration
    /// </summary>
    public class TypeValue : Value
    {
        /// <summary>
        /// Stores information about class, linke ancestors, fields, methods and constants
        /// </summary>
        public readonly ClassDecl Declaration;

        /// <summary>
        /// Class name
        /// </summary>
        public readonly QualifiedName QualifiedName;

        internal TypeValue(ClassDecl declaration)
        {
            QualifiedName = declaration.QualifiedName;
            Declaration = declaration;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() + " type: " + QualifiedName;
        }
    }
}
