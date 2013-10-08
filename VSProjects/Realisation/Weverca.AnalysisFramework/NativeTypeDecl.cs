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

        public readonly MemoryEntry Value;

        public readonly Expression Initializer;

        public NativeFieldInfo(VariableName name, string type, Visibility visibility, MemoryEntry value, bool isStatic)
        {
            Name = name;
            Type = type;
            Visibility = visibility;
            IsStatic=isStatic;
            Value = value;
        }

        public NativeFieldInfo(VariableName name, string type, Visibility visibility, Expression initializer, bool isStatic)
        {
            Name = name;
            Type = type;
            Visibility = visibility;
            IsStatic = isStatic;
            Initializer = initializer;
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

        public readonly Visibility Visibility;

        public readonly bool IsStatic;

        public readonly bool IsFinal;


        public NativeMethodInfo(Name name,  Visibility visibility,NativeAnalyzerMethod method, bool isFinal = false, bool isStatic = false)
        {
            Name = name;
            Method = method;
            IsFinal = isFinal;
            isStatic = IsStatic;
            Visibility = visibility;
        }
    }

    public class ConstantInfo
    {
        public readonly VariableName Name;
        public readonly MemoryEntry Value;
        public readonly Expression Initializer;
        public readonly Visibility Visibility;

        public ConstantInfo(VariableName name, Visibility visibility, MemoryEntry value)
        { 
            Name=name;
            Value=value;
            Visibility = visibility;
        }

        public ConstantInfo(VariableName name, Visibility visibility, Expression initializer)
        {
            Name = name;
            Initializer = initializer;
            Visibility = visibility;
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

        public readonly ReadOnlyDictionary<VariableName, ConstantInfo> Constants;

        public readonly ReadOnlyCollection<NativeMethodInfo> ModeledMethods;

        public readonly ReadOnlyCollection<MethodDecl> SourceCodeMethods;

        public readonly bool IsFinal;

        public readonly bool IsInterface;

        public NativeTypeDecl(QualifiedName typeName, IEnumerable<NativeMethodInfo> methods, IEnumerable<MethodDecl> sourceCodeMethods, Dictionary<VariableName, ConstantInfo> constants, Dictionary<VariableName, NativeFieldInfo> fields, Nullable<QualifiedName> baseClassName, bool isFinal, bool isInteface)
        {
            QualifiedName = typeName;
            BaseClassName = baseClassName;
            ModeledMethods = new ReadOnlyCollection<NativeMethodInfo>(new List<NativeMethodInfo>(methods));
            SourceCodeMethods = new ReadOnlyCollection<MethodDecl>(new List<MethodDecl>(sourceCodeMethods));
            Constants = new ReadOnlyDictionary<VariableName, ConstantInfo>(constants);
            Fields = new ReadOnlyDictionary<VariableName, NativeFieldInfo>(fields);
            IsFinal = isFinal;
            IsInterface = isInteface;
        }
    }
}
