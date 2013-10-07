using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;

using PHP.Core;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework
{

    public enum Visibility
    {
        PRIVATE, PUBLIC, PROTECTED
    }

    public class NativeFieldInfo
    {

        public readonly Name Name;

        public readonly Visibility Visibility;

        public readonly string Type;

        public readonly bool isStatic;

        public NativeFieldInfo(Name name, string type, Visibility visibility, bool IsStatic)
        {
            Name = name;
            Type = type;
            Visibility = visibility;
            IsStatic=isStatic;
        }
    }

    /// <summary>
    /// Represent info stored for native method
    /// </summary>
    public class NativeMethodInfo
    {
        /// <summary>
        /// Name of native method
        /// </summary>
        public readonly Name Name;

        /// <summary>
        /// Native method analyzer
        /// </summary>
        public readonly NativeAnalyzerMethod Method;

        public readonly bool IsStatic;

        public readonly bool IsFinal;

        public NativeMethodInfo(Name name, NativeAnalyzerMethod method, bool isFinal = false, bool isStatic = false)
        {
            Name = name;
            Method = method;
            IsFinal = isFinal;
            isStatic = IsStatic;
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

        public readonly Dictionary<string, NativeFieldInfo> Fields;

        public readonly Dictionary<string, Value> Constants;

        public readonly IEnumerable<NativeMethodInfo> Methods;

        public readonly bool IsFinal;

        public readonly bool IsInterface;

        public NativeTypeDecl(QualifiedName typeName, IEnumerable<NativeMethodInfo> methods, Dictionary<string, Value> constants, Dictionary<string, NativeFieldInfo> fields, QualifiedName? baseClassName ,bool isFinal,bool isInteface)
        {
            QualifiedName = typeName;
            BaseClassName = baseClassName;
            Methods = new ReadOnlyCollection<NativeMethodInfo>(new List<NativeMethodInfo>(methods));
            Constants = constants;
            Fields = fields;
            IsFinal = isFinal;
            IsInterface = isInteface;
        }
    }
}
