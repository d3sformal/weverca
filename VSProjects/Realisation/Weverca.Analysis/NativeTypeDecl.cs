using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;

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

        public NativeMethodInfo(Name name, NativeAnalyzerMethod method)
        {
            Name = name;
            Method = method;
        }
    }
    
    /// <summary>
    /// Represent native type declaration
    /// </summary>
    public class NativeTypeDecl
    {
        /// <summary>
        /// Name of native type
        /// </summary>
        public readonly QualifiedName QualifiedName;

        /// <summary>
        /// Name of base class
        /// </summary>
        public readonly QualifiedName? BaseClassName;


        public readonly IEnumerable<NativeMethodInfo> Methods;

        public NativeTypeDecl(QualifiedName typeName,IEnumerable<NativeMethodInfo> methods, QualifiedName? baseClassName = null)
        {
            QualifiedName = typeName;
            BaseClassName = baseClassName;
            Methods = new ReadOnlyCollection<NativeMethodInfo>(new List<NativeMethodInfo>(methods));
        }
    }
}
