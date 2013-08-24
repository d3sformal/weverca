using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

namespace Weverca.Analysis
{
    /// <summary>
    /// Represent info stored for native method
    /// </summary>
    public class NativeMethodInfo{
        /// <summary>
        /// Name of native method
        /// </summary>
        public readonly Name Name;

        /// <summary>
        /// Native method analyzer
        /// </summary>
        public readonly NativeAnalyzerMethod Method;        
    }
    
    /// <summary>
    /// Represent native type declaration
    /// </summary>
    public abstract class NativeTypeDecl
    {
        /// <summary>
        /// Name of native type
        /// </summary>
        public readonly QualifiedName QualifiedName;

        /// <summary>
        /// Name of base class
        /// </summary>
        public readonly QualifiedName? BaseClassName;

        /// <summary>
        /// Get all methods available for given type
        /// </summary>
        /// <returns>All available methods</returns>
        protected abstract IEnumerable<NativeMethodInfo> getMethods();

        public NativeTypeDecl(QualifiedName typeName, QualifiedName? baseClassName = null)
        {
            QualifiedName = typeName;
            BaseClassName = baseClassName;
        }
    }
}
