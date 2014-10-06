/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


/*
Copyright (c) 2012-2014 David Hauzar and Mirek Vodolan.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;

using PHP.Core;
using Weverca.AnalysisFramework.Memory;
using PHP.Core.AST;
using PHP.Core.Reflection;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Represents field or method visibility
    /// </summary>
    public enum Visibility
    {
        /// <summary>
        /// Private field/method
        /// </summary>
        PRIVATE,

        /// <summary>
        /// Public field/method
        /// </summary>
        PUBLIC,

        /// <summary>
        /// Protected field/method
        /// </summary>
        PROTECTED,

        /// <summary>
        /// Not accessible (private in parent)
        /// </summary>
        NOT_ACCESSIBLE
    }

    /// <summary>
    /// Identifier for field. It is immutable
    /// </summary>
    public class FieldIdentifier
    {
        /// <summary>
        /// Name of field
        /// </summary>
        public readonly VariableName Name;
        
        /// <summary>
        /// Name of class
        /// </summary>
        public readonly QualifiedName ClassName;
        
        /// <summary>
        /// Creates new instance of FieldIdentifier
        /// </summary>
        /// <param name="className">class name</param>
        /// <param name="name">Field name</param>
        public FieldIdentifier(QualifiedName className,VariableName name)
        { 
            Name=name;
            ClassName = className;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            FieldIdentifier y= obj as FieldIdentifier;
            if (obj == null)
            {
                return false;
            }
            return (Name.Equals(y.Name) && ClassName.Equals(y.ClassName));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return ClassName.GetHashCode()*13 + Name.GetHashCode();
        }
    }

    /// <summary>
    ///  Identifier for method. It is immutable
    /// </summary>
    public class MethodIdentifier
    {
        /// <summary>
        /// Method name
        /// </summary>
        public readonly Name Name;

        /// <summary>
        /// Class name
        /// </summary>
        public readonly QualifiedName ClassName;


        /// <summary>
        /// Creates a new instance of MethodIdentifier
        /// </summary>
        /// <param name="className">class name</param>
        /// <param name="name">method name</param>
        public MethodIdentifier(QualifiedName className, Name name)
        { 
            Name=name;
            ClassName = className;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return ClassName.GetHashCode() * 13 + Name.GetHashCode();
        }
    }

    /// <summary>
    /// Class which stores information about class field. It is immutable
    /// </summary>
    public class FieldInfo
    {

        /// <summary>
        /// The name of the field.
        /// </summary>
        public readonly VariableName Name;

        /// <summary>
        /// Field visibility
        /// </summary>
        public readonly Visibility Visibility;

        /// <summary>
        /// Field type
        /// </summary>
        public readonly string Type;

        /// <summary>
        /// Idicates if field is static
        /// </summary>
        public readonly bool IsStatic;

        /// <summary>
        /// Stores initlialization value of field (for fields from native classes)
        /// </summary>
        public readonly MemoryEntry InitValue;

        /// <summary>
        /// Stores initilization expression  (for fields from source code classes)
        /// </summary>
        public readonly Expression Initializer;

        /// <summary>
        /// Class name
        /// </summary>
        public readonly QualifiedName ClassName;

        /// <summary>
        /// Creates new instace of FieldInfo. Should be used for native classes
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="className">Class name</param>
        /// <param name="type">Field type</param>
        /// <param name="visibility">Field visibility</param>
        /// <param name="value">Initialization value</param>
        /// <param name="isStatic">Static indicator</param>
        public FieldInfo(VariableName name,QualifiedName className, string type, Visibility visibility, MemoryEntry value, bool isStatic)
        {
            Name = name;
            Type = type;
            Visibility = visibility;
            IsStatic=isStatic;
            InitValue = value;
            ClassName = className;
        }

        /// <summary>
        /// Creates new instace of FieldInfo. Should be used for source code classes
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="className">Class name</param>
        /// <param name="type">Field type</param>
        /// <param name="visibility">Field visibility</param>
        /// <param name="initializer">Initialization expression</param>
        /// <param name="isStatic">Static indicator</param>
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

    /// <summary>
    /// Stores information in method argument. It is immutable
    /// </summary>
    public class MethodArgument
    {
        /// <summary>
        /// Indicates if the parameter is passed by reference
        /// </summary>
        public readonly bool ByReference;

        /// <summary>
        /// paramater name
        /// </summary>
        public readonly VariableName Name;
        
        /// <summary>
        /// Indicator if parameter is optional
        /// </summary>
        public readonly bool Optional;

        /// <summary>
        /// Creates new instance of method argument
        /// </summary>
        /// <param name="name">Method name</param>
        /// <param name="byRefence">Reference indicator</param>
        /// <param name="optional">Optional indicator</param>
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

        /// <summary>
        /// method visibility
        /// </summary>
        public readonly Visibility Visibility;

        /// <summary>
        /// Method arguments 
        /// </summary>
        public readonly ReadOnlyCollection<MethodArgument> Arguments;

        /// <summary>
        /// Indicator if method is static
        /// </summary>
        public readonly bool IsStatic;

        /// <summary>
        /// Indicator if method is final
        /// </summary>
        public readonly bool IsFinal;

        /// <summary>
        /// Indicator if method is abstract
        /// </summary>
        public readonly bool IsAbstract;

        /// <summary>
        /// Class name
        /// </summary>
        public readonly QualifiedName ClassName;

        /// <summary>
        /// Crease new instance of method info
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="className">Class name</param>
        /// <param name="visibility">Visibility</param>
        /// <param name="method">Modeled method analyzer</param>
        /// <param name="args">Method arguments</param>
        /// <param name="isFinal">Final indicator</param>
        /// <param name="isStatic">Static indicator</param>
        /// <param name="isAbstract">Abstract indicator</param>
        public MethodInfo(Name name,QualifiedName className, Visibility visibility, NativeAnalyzerMethod method, List<MethodArgument> args, bool isFinal = false, bool isStatic = false, bool isAbstract = false)
        {
            Name = name;
            Method = method;
            IsFinal = isFinal;
            IsStatic = isStatic;
            Visibility = visibility;
            Arguments=new ReadOnlyCollection<MethodArgument>(args);
            IsAbstract = isAbstract;
            ClassName = className;
        }
    }

    /// <summary>
    /// Contains information about object constant
    /// </summary>
    public class ConstantInfo
    {
        /// <summary>
        /// Constant name
        /// </summary>
        public readonly VariableName Name;
        
        /// <summary>
        /// Constant value
        /// </summary>
        public readonly MemoryEntry Value;
        
        /// <summary>
        /// Constant initializer
        /// </summary>
        public readonly Expression Initializer;

        /// <summary>
        /// Constant visibility
        /// </summary>
        public readonly Visibility Visibility;

        /// <summary>
        /// Constant class name
        /// </summary>
        public readonly QualifiedName ClassName;
        
        /// <summary>
        /// Creates new instance of Constantinfo
        /// </summary>
        /// <param name="name">Constant name</param>
        /// <param name="className">Class name</param>
        /// <param name="visibility">Constant visibility</param>
        /// <param name="value">Constant value</param>
        public ConstantInfo(VariableName name, QualifiedName className, Visibility visibility, MemoryEntry value)
        { 
            Name=name;
            Value=value;
            Visibility = visibility;
            ClassName = className;
        }

        /// <summary>
        /// Creates new instance of Constantinfo
        /// </summary>
        /// <param name="name">Constant name</param>
        /// <param name="className">Class name</param>
        /// <param name="visibility">Constant visibility</param>
        /// <param name="initializer">Constant initializer</param>
        public ConstantInfo(VariableName name, QualifiedName className, Visibility visibility, Expression initializer)
        {
            Name = name;
            Initializer = initializer;
            Visibility = visibility;
            ClassName = className;
        }

        /// <summary>
        /// Clones this constant info and changes the class name
        /// </summary>
        /// <param name="className">new class name</param>
        /// <returns>The cloned info</returns>
        public ConstantInfo CloneWithNewQualifiedName(QualifiedName className)
        {
            if (Initializer == null)
            {
                return new ConstantInfo(Name, className, Visibility, Value);
            }
            else 
            {
                return new ConstantInfo(Name, className, Visibility, Initializer);
            }
        }
    }

    /// <summary>
    /// Type declaration. It is immutable
    /// </summary>
    public class ClassDecl
    {
        /// <summary>
        /// Name of class
        /// </summary>
        public readonly QualifiedName QualifiedName;

        /// <summary>
        /// Names of base classes
        /// </summary>
        public readonly ReadOnlyCollection<QualifiedName> BaseClasses;

        /// <summary>
        /// Class fields
        /// </summary>
        public readonly ReadOnlyDictionary<FieldIdentifier, FieldInfo> Fields;

        /// <summary>
        /// Class constants
        /// </summary>
        public readonly ReadOnlyDictionary<FieldIdentifier, ConstantInfo> Constants;

        /// <summary>
        /// Structure that stores all modeled methods
        /// </summary>
        public readonly ReadOnlyDictionary<MethodIdentifier,MethodInfo> ModeledMethods;

        /// <summary>
        /// Structure that stores all source code methods
        /// </summary>
        public readonly ReadOnlyDictionary<MethodIdentifier, FunctionValue> SourceCodeMethods;

        /// <summary>
        /// Indicates if class is final
        /// </summary>
        public readonly bool IsFinal;

        /// <summary>
        /// Indicates if class is interface
        /// </summary>
        public readonly bool IsInterface;

        /// <summary>
        /// Indicates if class is abstract
        /// </summary>
        public readonly bool IsAbstract;

        private Dictionary<VariableName, FieldInfo> resultingFields = new Dictionary<VariableName, FieldInfo>();

        private Dictionary<Name, Visibility> resultingMethodVisibility = new Dictionary<Name, Visibility>();

        /// <summary>
        /// Creates new instance of ClassDecl
        /// </summary>
        /// <param name="typeName">Class name</param>
        /// <param name="methods">Modeled methods</param>
        /// <param name="sourceCodeMethods">Source code methods</param>
        /// <param name="constants">Class constants</param>
        /// <param name="fields">Class fields</param>
        /// <param name="baseClassName">Names of base classes</param>
        /// <param name="isFinal">Indicates if class is final</param>
        /// <param name="isInteface">Indicates if class is interface</param>
        /// <param name="isAbstract">Indicates if class is abstract</param>
        public ClassDecl(QualifiedName typeName, Dictionary<MethodIdentifier, MethodInfo> methods, 
            Dictionary<MethodIdentifier,
            FunctionValue> sourceCodeMethods, 
            Dictionary<FieldIdentifier, 
            ConstantInfo> constants, Dictionary<FieldIdentifier, 
            FieldInfo> fields, 
            List<QualifiedName> baseClassName, 
            bool isFinal, 
            bool isInteface, 
            bool isAbstract)
        {
            QualifiedName = typeName;
            BaseClasses = new ReadOnlyCollection<QualifiedName>(baseClassName);
            ModeledMethods = new ReadOnlyDictionary<MethodIdentifier, MethodInfo>(methods);
            SourceCodeMethods = new ReadOnlyDictionary<MethodIdentifier, FunctionValue>(sourceCodeMethods);
            Constants = new ReadOnlyDictionary<FieldIdentifier, ConstantInfo>(constants);
            Fields = new ReadOnlyDictionary<FieldIdentifier, FieldInfo>(fields);
            IsFinal = isFinal;
            IsInterface = isInteface;
            IsAbstract = isAbstract;

            
            List<QualifiedName> classHierarchy = new List<QualifiedName>(baseClassName);
            classHierarchy.Add(typeName);
            foreach (var baseClass in classHierarchy)
            {
                foreach (var field in fields.Where(a=>a.Key.ClassName==baseClass))
                {
                    resultingFields[field.Key.Name] = field.Value;
                }


                foreach (var method in methods.Where(a => a.Key.ClassName == baseClass))
                {
                    if (method.Value.Visibility == Visibility.PRIVATE)
                    {
                        if (method.Key.ClassName.Equals(QualifiedName))
                        {
                            resultingMethodVisibility[method.Key.Name] = Visibility.PRIVATE;
                        }
                        else 
                        {
                            resultingMethodVisibility[method.Key.Name] = Visibility.NOT_ACCESSIBLE;
                        }
                    }
                    else
                    {
                        resultingMethodVisibility[method.Key.Name] = method.Value.Visibility;
                    }
                }


                foreach (var method in sourceCodeMethods.Where(a => a.Key.ClassName == baseClass))
                {
                    if(method.Value.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Private))
                    {
                        if (method.Key.ClassName.Equals(QualifiedName))
                        {
                            resultingMethodVisibility[method.Key.Name] = Visibility.PRIVATE;
                        }
                        else
                        {
                            resultingMethodVisibility[method.Key.Name] = Visibility.NOT_ACCESSIBLE;
                        }
                    }
                    else if(method.Value.MethodDecl.Modifiers.HasFlag(PhpMemberAttributes.Protected))
                    {
                        resultingMethodVisibility[method.Key.Name] = Visibility.PROTECTED;
                    }
                    else
                    {
                        resultingMethodVisibility[method.Key.Name] = Visibility.PUBLIC;
                    }
                }


            }

        }
        /// <summary>
        /// Return visibility for given field
        /// </summary>
        /// <param name="name">Field name</param>
        /// <param name="isStatic">static indicator</param>
        /// <returns>Return visibility, or null, if field doesn't exists</returns>
        public Visibility? GetFieldVisibility(VariableName name, bool isStatic)
        {
            if (!resultingFields.ContainsKey(name))
            {
                return null;
            }
            else
            {
                if (resultingFields[name].IsStatic == isStatic)
                {
                    var visibility = resultingFields[name].Visibility;
                    if (visibility == Visibility.PRIVATE)
                    {
                        if (resultingFields[name].ClassName.Equals(QualifiedName))
                        {
                            return visibility;
                        }
                        else
                        {
                            return Visibility.NOT_ACCESSIBLE;
                        }
                    }
                    else 
                    {
                        return visibility;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Returns method visibility or null, if method doesn't exists
        /// </summary>
        /// <param name="name">Method name</param>
        /// <returns>Method visibility or null, if method doesn't exists</returns>
        public Visibility? GetMethodVisibility(Name name)
        {
            if (resultingMethodVisibility.ContainsKey(name))
            {
                return resultingMethodVisibility[name];
            }
            else 
            {
                return null;
            }
        }

    }

    /// <summary>
    /// Class builder of classDecl, because ClassDecl is immutable and has too many arguments in constructor
    /// </summary>
    public class ClassDeclBuilder
    {
        /// <summary>
        /// Name of class
        /// </summary>
        public QualifiedName QualifiedName;

        /// <summary>
        /// Structure that stores all modeled methods
        /// </summary>
        public Dictionary<MethodIdentifier, MethodInfo> ModeledMethods;

        /// <summary>
        /// Structure that stores all source code methods
        /// </summary>
        public Dictionary<MethodIdentifier, FunctionValue> SourceCodeMethods;

        /// <summary>
        /// Class constants
        /// </summary>
        public Dictionary<FieldIdentifier, ConstantInfo> Constants;

        /// <summary>
        /// Class fields
        /// </summary>
        public Dictionary<FieldIdentifier, FieldInfo> Fields;
        
        /// <summary>
        /// List of base classes names
        /// </summary>
        public List<QualifiedName> BaseClasses;
        
        /// <summary>
        /// Indicates if class is final
        /// </summary>
        public bool IsFinal;

        /// <summary>
        /// Indicates if class is interface
        /// </summary>
        public bool IsInterface;

        /// <summary>
        /// Indicates if class is abstract
        /// </summary>
        public bool IsAbstract;
        
        /// <summary>
        /// Create new instance of ClassDeclBuilder and sets all fields to default values.
        /// </summary>
        public ClassDeclBuilder()
        {
            ModeledMethods = new Dictionary<MethodIdentifier, MethodInfo>();
            SourceCodeMethods = new Dictionary<MethodIdentifier, FunctionValue>();
            Constants = new Dictionary<FieldIdentifier, ConstantInfo>();
            Fields = new Dictionary<FieldIdentifier, FieldInfo>();
            BaseClasses = new List<QualifiedName>();
            IsFinal = false;
            IsInterface = false;
            IsAbstract = false;
        }

        /// <summary>
        /// Creates new intacne of ClassDecl from information stored in this class
        /// </summary>
        /// <returns>new instance of ClassDecl</returns>
        public ClassDecl Build()
        {
            return new ClassDecl(QualifiedName, ModeledMethods, SourceCodeMethods, Constants, Fields, BaseClasses, IsFinal, IsInterface, IsAbstract);
        }
    }
}