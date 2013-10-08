using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;

using PHP.Core;
using Weverca.AnalysisFramework.Memory;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework
{

    public enum Visibility
    {
        PRIVATE, PUBLIC, PROTECTED
    }

    public class NativeFieldInfo
    {

        public readonly VariableName Name;

        public readonly Visibility Visibility;

        public readonly string Type;

        public readonly bool IsStatic;

        public readonly MemoryEntry Identifier;

        public NativeFieldInfo(VariableName name, string type, Visibility visibility, MemoryEntry identifier, bool isStatic)
        {
            Name = name;
            Type = type;
            Visibility = visibility;
            IsStatic=isStatic;
            Identifier = identifier;
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
        public readonly Nullable<QualifiedName> BaseClassName;

        public readonly ReadOnlyDictionary<VariableName, NativeFieldInfo> Fields;

        public readonly ReadOnlyDictionary<VariableName, MemoryEntry> Constants;

        public readonly ReadOnlyCollection<NativeMethodInfo> ModeledMethods;

        public readonly ReadOnlyCollection<MethodDecl> SourceCodeMethods;

        public readonly bool IsFinal;

        public readonly bool IsInterface;

        public NativeTypeDecl(QualifiedName typeName, IEnumerable<NativeMethodInfo> methods, IEnumerable<MethodDecl> sourceCodeMethods, Dictionary<VariableName, MemoryEntry> constants, Dictionary<VariableName, NativeFieldInfo> fields, Nullable<QualifiedName> baseClassName, bool isFinal, bool isInteface)
        {
            QualifiedName = typeName;
            BaseClassName = baseClassName;
            ModeledMethods = new ReadOnlyCollection<NativeMethodInfo>(new List<NativeMethodInfo>(methods));
            SourceCodeMethods = new ReadOnlyCollection<MethodDecl>(new List<MethodDecl>(sourceCodeMethods));
            Constants = new ReadOnlyDictionary<VariableName, MemoryEntry>(constants);
            Fields = new ReadOnlyDictionary<VariableName, NativeFieldInfo>(fields);
            IsFinal = isFinal;
            IsInterface = isInteface;
        }
    }
}
