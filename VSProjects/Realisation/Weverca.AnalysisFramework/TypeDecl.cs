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

    public class FieldIdentifier
    {
        public readonly VariableName Name;
        public readonly QualifiedName ClassName;
        public FieldIdentifier(QualifiedName className,VariableName name)
        { 
            Name=name;
            ClassName = className;
        }

        public override bool Equals(object obj)
        {
            FieldIdentifier y= obj as FieldIdentifier;
            if (obj == null)
            {
                return false;
            }
            return (Name.Equals(y.Name) && ClassName.Equals(y.ClassName));
        }

        public override int GetHashCode()
        {
            return ClassName.GetHashCode()*13 + Name.GetHashCode();
        }
    }

    public class MethodIdentifier
    {
        public readonly Name Name;
        public readonly QualifiedName ClassName;

        public MethodIdentifier(QualifiedName className, Name name)
        { 
            Name=name;
            ClassName = className;
        }

        public override bool Equals(object obj)
        {
            if (obj is MethodIdentifier)
            {
                return (Name.Equals((obj as MethodIdentifier).Name) && ClassName.Equals((obj as MethodIdentifier).ClassName));
            }
            else 
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return ClassName.GetHashCode() * 13 + Name.GetHashCode();
        }
    }

    public class FieldInfo
    {

        public readonly VariableName Name;

        public readonly Visibility Visibility;

        public readonly string Type;

        public readonly bool IsStatic;

        public readonly MemoryEntry InitValue;

        public readonly Expression Initializer;

        public readonly QualifiedName ClassName;

        public FieldInfo(VariableName name,QualifiedName className, string type, Visibility visibility, MemoryEntry value, bool isStatic)
        {
            Name = name;
            Type = type;
            Visibility = visibility;
            IsStatic=isStatic;
            InitValue = value;
            ClassName = className;
        }

        public FieldInfo(VariableName name, QualifiedName className, string type, Visibility visibility, Expression initializer, bool isStatic)
        {
            Name = name;
            Type = type;
            Visibility = visibility;
            IsStatic = isStatic;
            Initializer = initializer;
            ClassName = className;
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

        public readonly QualifiedName ClassName;

        public MethodInfo(Name name,QualifiedName className, Visibility visibility, NativeAnalyzerMethod method, List<MethodArgument> args, bool isFinal = false, bool isStatic = false, bool isAbstract = false)
        {
            Name = name;
            Method = method;
            IsFinal = isFinal;
            isStatic = IsStatic;
            Visibility = visibility;
            Arguments=new ReadOnlyCollection<MethodArgument>(args);
            IsAbstract = isAbstract;
            ClassName = className;
        }
    }

    public class ConstantInfo
    {
        public readonly VariableName Name;
        public readonly MemoryEntry Value;
        public readonly Expression Initializer;
        public readonly Visibility Visibility;
        public readonly QualifiedName ClassName;
        public ConstantInfo(VariableName name, QualifiedName className, Visibility visibility, MemoryEntry value)
        { 
            Name=name;
            Value=value;
            Visibility = visibility;
            ClassName = className;
        }

        public ConstantInfo(VariableName name, QualifiedName className, Visibility visibility, Expression initializer)
        {
            Name = name;
            Initializer = initializer;
            Visibility = visibility;
            ClassName = className;
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

        public readonly ReadOnlyDictionary<FieldIdentifier, FieldInfo> Fields;

        public readonly ReadOnlyDictionary<FieldIdentifier, ConstantInfo> Constants;

        public readonly ReadOnlyDictionary<MethodIdentifier,MethodInfo> ModeledMethods;

        public readonly ReadOnlyDictionary<MethodIdentifier,MethodDecl> SourceCodeMethods;

        public readonly bool IsFinal;

        public readonly bool IsInterface;

        public readonly bool IsAbstract;

        public ClassDecl(QualifiedName typeName, Dictionary<MethodIdentifier,MethodInfo> methods, Dictionary<MethodIdentifier, MethodDecl> sourceCodeMethods, Dictionary<FieldIdentifier, ConstantInfo> constants, Dictionary<FieldIdentifier, FieldInfo> fields, Nullable<QualifiedName> baseClassName, bool isFinal, bool isInteface, bool isAbstract)
        {
            QualifiedName = typeName;
            BaseClassName = baseClassName;
            ModeledMethods = new ReadOnlyDictionary<MethodIdentifier, MethodInfo>(methods);
            SourceCodeMethods = new ReadOnlyDictionary<MethodIdentifier, MethodDecl>(sourceCodeMethods);
            Constants = new ReadOnlyDictionary<FieldIdentifier, ConstantInfo>(constants);
            Fields = new ReadOnlyDictionary<FieldIdentifier, FieldInfo>(fields);
            IsFinal = isFinal;
            IsInterface = isInteface;
            IsAbstract = isAbstract;
        }
    }

    public class ClassDeclBuilder
    {
        public QualifiedName TypeName;
        public Dictionary<MethodIdentifier, MethodInfo> ModeledMethods;
        public Dictionary<MethodIdentifier,MethodDecl> SourceCodeMethods;
        public Dictionary<FieldIdentifier, ConstantInfo> Constants;
        public Dictionary<FieldIdentifier, FieldInfo> Fields;
        public Nullable<QualifiedName> BaseClassName;
        public bool IsFinal;
        public bool IsInterface;
        public bool IsAbstract;
        
        public ClassDeclBuilder()
        {
            ModeledMethods = new Dictionary<MethodIdentifier, MethodInfo>();
            SourceCodeMethods = new Dictionary<MethodIdentifier, MethodDecl>();
            Constants = new Dictionary<FieldIdentifier, ConstantInfo>();
            Fields = new Dictionary<FieldIdentifier, FieldInfo>();
            BaseClassName = null;
            IsFinal = false;
            IsInterface = false;
            IsAbstract = false;
        }

        public ClassDecl Build()
        {
            return new ClassDecl(TypeName, ModeledMethods, SourceCodeMethods, Constants, Fields, BaseClassName, IsFinal, IsInterface, IsAbstract);
        }
    }
}
