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

    public class FieldInfo
    {

        public readonly VariableName Name;

        public readonly Visibility Visibility;

        public readonly string Type;

        public readonly bool IsStatic;

        public readonly MemoryEntry InitValue;

        public readonly Expression Initializer;

        public FieldInfo(VariableName name, string type, Visibility visibility, MemoryEntry value, bool isStatic)
        {
            Name = name;
            Type = type;
            Visibility = visibility;
            IsStatic=isStatic;
            InitValue = value;
        }

        public FieldInfo(VariableName name, string type, Visibility visibility, Expression initializer, bool isStatic)
        {
            Name = name;
            Type = type;
            Visibility = visibility;
            IsStatic = isStatic;
            Initializer = initializer;
        }
    }

    public class MethodArgument
    {
        public readonly bool ByReference;
        public readonly VariableName Name;
        public readonly bool Optional;

        public MethodArgument(VariableName name, bool byRefence, bool optional)
        {
            Name = name;
            ByReference = byRefence;
            Optional= optional;
        }
    }

    /// <summary>
    /// Represent info stored for modeled method
    /// </summary>
    public class MethodInfo
    {
        /// <summary>
        /// Name of modeled method
        /// </summary>
        public readonly Name Name;

        /// <summary>
        /// Modeled method analyzer
        /// </summary>
        public readonly NativeAnalyzerMethod Method;

        public readonly Visibility Visibility;

        public readonly ReadOnlyCollection<MethodArgument> Arguments;

        public readonly bool IsStatic;

        public readonly bool IsFinal;

        public readonly bool IsAbstract;

        public MethodInfo(Name name, Visibility visibility, NativeAnalyzerMethod method, List<MethodArgument> args, bool isFinal = false, bool isStatic = false, bool isAbstract = false)
        {
            Name = name;
            Method = method;
            IsFinal = isFinal;
            isStatic = IsStatic;
            Visibility = visibility;
            Arguments=new ReadOnlyCollection<MethodArgument>(args);
            IsAbstract = isAbstract;
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
    /// Type declaration
    /// </summary>
    public class ClassDecl
    {
        /// <summary>
        /// Name of class
        /// </summary>
        public readonly QualifiedName QualifiedName;

        /// <summary>
        /// Name of base class
        /// </summary>
        public readonly Nullable<QualifiedName> BaseClassName;

        public readonly ReadOnlyDictionary<VariableName, FieldInfo> Fields;

        public readonly ReadOnlyDictionary<VariableName, ConstantInfo> Constants;

        public readonly ReadOnlyCollection<MethodInfo> ModeledMethods;

        public readonly ReadOnlyCollection<MethodDecl> SourceCodeMethods;

        public readonly bool IsFinal;

        public readonly bool IsInterface;

        public readonly bool IsAbstract;

        public ClassDecl(QualifiedName typeName, IEnumerable<MethodInfo> methods, IEnumerable<MethodDecl> sourceCodeMethods, Dictionary<VariableName, ConstantInfo> constants, Dictionary<VariableName, FieldInfo> fields, Nullable<QualifiedName> baseClassName, bool isFinal, bool isInteface,bool isAbstract)
        {
            QualifiedName = typeName;
            BaseClassName = baseClassName;
            ModeledMethods = new ReadOnlyCollection<MethodInfo>(new List<MethodInfo>(methods));
            SourceCodeMethods = new ReadOnlyCollection<MethodDecl>(new List<MethodDecl>(sourceCodeMethods));
            Constants = new ReadOnlyDictionary<VariableName, ConstantInfo>(constants);
            Fields = new ReadOnlyDictionary<VariableName, FieldInfo>(fields);
            IsFinal = isFinal;
            IsInterface = isInteface;
            IsAbstract = isAbstract;
        }
    }

    class ClassDeclBuilder
    {
        public ClassDecl Build()
        {

            return null;
        }
    }
}
