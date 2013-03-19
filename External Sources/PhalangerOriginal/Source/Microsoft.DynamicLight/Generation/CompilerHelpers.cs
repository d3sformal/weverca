/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
using BigInt = System.Numerics.BigInteger;
using Microsoft.Scripting.Ast;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Contracts;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
//using Microsoft.Scripting.Interpreter;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;

namespace Microsoft.Scripting.Generation {
    // TODO: keep this?
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
    public delegate void ActionRef<T0, T1>(ref T0 arg0, ref T1 arg1);

    public static class CompilerHelpers {
        public static readonly MethodAttributes PublicStatic = MethodAttributes.Public | MethodAttributes.Static;
        private static readonly MethodInfo _CreateInstanceMethod = typeof(ScriptingRuntimeHelpers).GetMethod("CreateInstance");

        private static int _Counter; // for generating unique names for lambda methods

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static object GetMissingValue(Type type) {
            ContractUtils.RequiresNotNull(type, "type");

            if (type.IsByRef) type = type.GetElementType();
            if (type.IsEnum) return Activator.CreateInstance(type);

            switch (Type.GetTypeCode(type)) {
                default:
                case TypeCode.Object:
                    // struct
                    if (type.IsSealed && type.IsValueType) {
                        return Activator.CreateInstance(type);
                    } else if (type == typeof(object)) {
                        // parameter of type object receives the actual Missing value
                        return Missing.Value;
                    } else if (!type.IsValueType) {
                        return null;
                    } else {
                        throw Error.CantCreateDefaultTypeFor(type);
                    }
                case TypeCode.Empty:
                case TypeCode.DBNull:
                case TypeCode.String:
                    return null;

                case TypeCode.Boolean: return false;
                case TypeCode.Char: return '\0';
                case TypeCode.SByte: return (sbyte)0;
                case TypeCode.Byte: return (byte)0;
                case TypeCode.Int16: return (short)0;
                case TypeCode.UInt16: return (ushort)0;
                case TypeCode.Int32: return (int)0;
                case TypeCode.UInt32: return (uint)0;
                case TypeCode.Int64: return 0L;
                case TypeCode.UInt64: return 0UL;
                case TypeCode.Single: return 0.0f;
                case TypeCode.Double: return 0.0D;
                case TypeCode.Decimal: return (decimal)0;
                case TypeCode.DateTime: return DateTime.MinValue;
            }
        }

        public static bool IsStatic(MethodBase mi) {
            return mi.IsConstructor || mi.IsStatic;
        }

        /// <summary>
        /// True if the MethodBase is method which is going to construct an object
        /// </summary>
        public static bool IsConstructor(MethodBase mb) {
            if (mb.IsConstructor) {
                return true;
            }

            if (mb.IsGenericMethod) {
                MethodInfo mi = mb as MethodInfo;

                if (mi.GetGenericMethodDefinition() == _CreateInstanceMethod) {
                    return true;
                }
            }

            return false;
        }

        public static T[] MakeRepeatedArray<T>(T item, int count) {
            T[] ret = new T[count];
            for (int i = 0; i < count; i++) ret[i] = item;
            return ret;
        }
        
        public static bool IsComparisonOperator(ExpressionType op) {
            switch (op) {
                case ExpressionType.LessThan: return true;
                case ExpressionType.LessThanOrEqual: return true;
                case ExpressionType.GreaterThan: return true;
                case ExpressionType.GreaterThanOrEqual: return true;
                case ExpressionType.Equal: return true;
                case ExpressionType.NotEqual: return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the System.Type for any object, including null.  The type of null
        /// is represented by None.Type and all other objects just return the 
        /// result of Object.GetType
        /// </summary>
        public static Type GetType(object obj) {
            if (obj == null) {
                return typeof(DynamicNull);
            }

            return obj.GetType();
        }

        /// <summary>
        /// Simply returns a Type[] from calling GetType on each element of args.
        /// </summary>
        public static Type[] GetTypes(object[] args) {
            Type[] types = new Type[args.Length];
            for (int i = 0; i < args.Length; i++) {
                types[i] = GetType(args[i]);
            }
            return types;
        }

        /// <summary>
        /// EMITTED
        /// Used by default method binder to check types of splatted arguments.
        /// </summary>
        public static bool TypesEqual(IList args, int start, Type[] types) {
            for (int i = 0; i < types.Length; i++) {
                object arg = args[start + i];
                if (types[i] != (arg != null ? arg.GetType() : null)) {
                    return false;
                }
            }
            return true;
        }

        public static bool CanOptimizeMethod(MethodBase method) {
            if (method.ContainsGenericParameters ||
                method.IsProtected() ||
                method.IsPrivate ||
                !method.DeclaringType.IsVisible) {
                return false;
            }
            return true;
        }

        public static MethodInfo TryGetCallableMethod(MethodInfo method) {
            return TryGetCallableMethod(method.ReflectedType, method);
        }

        /// <summary>
        /// Given a MethodInfo which may be declared on a non-public type this attempts to
        /// return a MethodInfo which will dispatch to the original MethodInfo but is declared
        /// on a public type.
        /// 
        /// Returns the original method if the method if a public version cannot be found.
        /// </summary>
        public static MethodInfo TryGetCallableMethod(Type targetType, MethodInfo method) {
            if (method.DeclaringType == null || method.DeclaringType.IsVisible) {
                return method;
            }

            // first try and get it from the base type we're overriding...
            MethodInfo baseMethod = method.GetBaseDefinition();

            if (baseMethod.DeclaringType.IsVisible || baseMethod.DeclaringType.IsInterface) {
                // We need to instantiate the method as GetBaseDefinition might return a generic definition of the base method:
                if (baseMethod.IsGenericMethodDefinition) {
                    baseMethod = baseMethod.MakeGenericMethod(method.GetGenericArguments());
                }
                return baseMethod;
            }

            // maybe we can get it from an interface on the type this
            // method came from...
            Type[] interfaces = targetType.GetInterfaces();
            foreach (Type iface in interfaces) {
                InterfaceMapping mapping = targetType.GetInterfaceMap(iface);
                for (int i = 0; i < mapping.TargetMethods.Length; i++) {
                    MethodInfo targetMethod = mapping.TargetMethods[i];
                    if (targetMethod != null && targetMethod.MethodHandle == method.MethodHandle) {
                        return mapping.InterfaceMethods[i];
                    }
                }
            }

            return method;
        }

        /// <summary>
        /// Non-public types can have public members that we find when calling type.GetMember(...).  This
        /// filters out the non-visible members by attempting to resolve them to the correct visible type.
        /// 
        /// If no correct visible type can be found then the member is not visible and we won't call it.
        /// </summary>
        public static MemberInfo[] FilterNonVisibleMembers(Type type, MemberInfo[] foundMembers) {
            if (!type.IsVisible && foundMembers.Length > 0) {
                // need to remove any members that we can't get through other means
                List<MemberInfo> foundVisible = null;
                for (int i = 0; i < foundMembers.Length; i++) {
                    MemberInfo curMember = foundMembers[i];

                    MemberInfo visible = TryGetVisibleMember(curMember);
                    if (visible != null) {
                        if (foundVisible == null) {
                            foundVisible = new List<MemberInfo>();
                        }
                        foundVisible.Add(visible);
                    }
                }

                if (foundVisible != null) {
                    foundMembers = foundVisible.ToArray();
                } else {
                    foundMembers = new MemberInfo[0];
                }
            }
            return foundMembers;
        }

        public static MemberInfo TryGetVisibleMember(MemberInfo curMember) {
            MethodInfo mi;
            MemberInfo visible = null;
            switch (curMember.MemberType) {
                case MemberTypes.Method:
                    mi = TryGetCallableMethod((MethodInfo)curMember);
                    if (CompilerHelpers.IsVisible(mi)) {
                        visible = mi;
                    }
                    break;

                case MemberTypes.Property:
                    PropertyInfo pi = (PropertyInfo)curMember;
                    mi = TryGetCallableMethod(pi.GetGetMethod() ?? pi.GetSetMethod());
                    if (CompilerHelpers.IsVisible(mi)) {
                        visible = mi.DeclaringType.GetProperty(pi.Name);
                    }
                    break;

                case MemberTypes.Event:
                    EventInfo ei = (EventInfo)curMember;
                    mi = TryGetCallableMethod(ei.GetAddMethod() ?? ei.GetRemoveMethod() ?? ei.GetRaiseMethod());
                    if (CompilerHelpers.IsVisible(mi)) {
                        visible = mi.DeclaringType.GetEvent(ei.Name);
                    }
                    break;

                // all others can't be exposed out this way
            }
            return visible;
        }

        /// <summary>
        /// Sees if two MemberInfos point to the same underlying construct in IL.  This
        /// ignores the ReflectedType property which exists on MemberInfos which
        /// causes direct comparisons to be false even if they are the same member.
        /// </summary>
        public static bool MemberEquals(this MemberInfo self, MemberInfo other) {
            if ((self == null) != (other == null)) {
                // one null, the other isn't.
                return false;
            } else if (self == null) {
                // both null
                return true;
            }

            if (self.MemberType != other.MemberType) {
                return false;
            }

            switch (self.MemberType) {
                case MemberTypes.Field:
                    return ((FieldInfo)self).FieldHandle.Equals(((FieldInfo)other).FieldHandle);
                case MemberTypes.Method:
                    return ((MethodInfo)self).MethodHandle.Equals(((MethodInfo)other).MethodHandle);
                case MemberTypes.Constructor:
                    return ((ConstructorInfo)self).MethodHandle.Equals(((ConstructorInfo)other).MethodHandle);
                case MemberTypes.NestedType:
                case MemberTypes.TypeInfo:
                    return ((Type)self).TypeHandle.Equals(((Type)other).TypeHandle);
                case MemberTypes.Event:
                case MemberTypes.Property:
                default:
                    return
                        ((MemberInfo)self).Module == ((MemberInfo)other).Module &&
                        ((MemberInfo)self).MetadataToken == ((MemberInfo)other).MetadataToken;
            }
        }

        /// <summary>
        /// Given a MethodInfo which may be declared on a non-public type this attempts to
        /// return a MethodInfo which will dispatch to the original MethodInfo but is declared
        /// on a public type.
        /// 
        /// Throws InvalidOperationException if the method cannot be obtained.
        /// </summary>
        public static MethodInfo GetCallableMethod(MethodInfo method, bool privateBinding) {
            MethodInfo callable = TryGetCallableMethod(method);
            if (privateBinding || IsVisible(callable)) {
                return callable;
            }
            throw Error.NoCallableMethods(method.DeclaringType, method.Name);
        }

        public static bool IsVisible(MethodBase info) {
            return info.IsPublic && (info.DeclaringType == null || info.DeclaringType.IsVisible);
        }

        public static bool IsVisible(FieldInfo info) {
            return info.IsPublic && (info.DeclaringType == null || info.DeclaringType.IsVisible);
        }

        public static bool IsProtected(this MethodBase info) {
            return info.IsFamily || info.IsFamilyOrAssembly;
        }

        public static bool IsProtected(this FieldInfo info) {
            return info.IsFamily || info.IsFamilyOrAssembly;
        }

        public static bool IsProtected(this Type type) {
            return type.IsNestedFamily || type.IsNestedFamORAssem;
        }

        public static Type GetVisibleType(object value) {
            return GetVisibleType(GetType(value));
        }

        public static Type GetVisibleType(Type t) {
            while (!t.IsVisible) {
                t = t.BaseType;
            }
            return t;
        }

        public static MethodBase[] GetConstructors(Type t, bool privateBinding) {
            return GetConstructors(t, privateBinding, false);
        }

        public static MethodBase[] GetConstructors(Type t, bool privateBinding, bool includeProtected) {
            if (t.IsArray) {
                // The JIT verifier doesn't like new int[](3) even though it appears as a ctor.
                // We could do better and return newarr in the future.
                return new MethodBase[] { GetArrayCtor(t) };
            }

            BindingFlags bf = BindingFlags.Instance | BindingFlags.Public;
            if (privateBinding || includeProtected) {
                bf |= BindingFlags.NonPublic;
            }
            ConstructorInfo[] ci = t.GetConstructors(bf);

            // leave in protected ctors, even if we're not in private binding mode.
            if (!privateBinding && includeProtected) {
                ci = FilterConstructorsToPublicAndProtected(ci);
            }

            if (t.IsValueType 
#if !SILVERLIGHT
                && t != typeof(ArgIterator)
#endif
            ) {
                // structs don't define a parameterless ctor, add a generic method for that.
                return ArrayUtils.Insert<MethodBase>(GetStructDefaultCtor(t), ci);
            }

            return ci;
        }

        public static ConstructorInfo[] FilterConstructorsToPublicAndProtected(ConstructorInfo[] ci) {
            List<ConstructorInfo> finalInfos = null;
            for (int i = 0; i < ci.Length; i++) {
                ConstructorInfo info = ci[i];
                if (!info.IsPublic && !info.IsProtected()) {
                    if (finalInfos == null) {
                        finalInfos = new List<ConstructorInfo>();
                        for (int j = 0; j < i; j++) {
                            finalInfos.Add(ci[j]);
                        }
                    }
                } else if (finalInfos != null) {
                    finalInfos.Add(ci[i]);
                }
            }

            if (finalInfos != null) {
                ci = finalInfos.ToArray();
            }
            return ci;
        }

        private static MethodBase GetStructDefaultCtor(Type t) {
            return typeof(ScriptingRuntimeHelpers).GetMethod("CreateInstance").MakeGenericMethod(t);
        }

        private static MethodBase GetArrayCtor(Type t) {
            return typeof(ScriptingRuntimeHelpers).GetMethod("CreateArray").MakeGenericMethod(t.GetElementType());
        }

        #region Type Conversions

        public static MethodInfo GetImplicitConverter(Type fromType, Type toType) {
            return GetConverter(fromType, fromType, toType, "op_Implicit") ?? GetConverter(toType, fromType, toType, "op_Implicit");
        }

        public static MethodInfo GetExplicitConverter(Type fromType, Type toType) {
            return GetConverter(fromType, fromType, toType, "op_Explicit") ?? GetConverter(toType, fromType, toType, "op_Explicit");
        }

        private static MethodInfo GetConverter(Type type, Type fromType, Type toType, string opMethodName) {
            foreach (MethodInfo mi in type.GetMember(opMethodName, BindingFlags.Public | BindingFlags.Static)) {
                if ((mi.DeclaringType == null || mi.DeclaringType.IsVisible) && mi.IsPublic &&
                    mi.ReturnType == toType && mi.GetParameters()[0].ParameterType.IsAssignableFrom(fromType)) {
                    return mi;
                }
            }
            return null;
        }

        public static bool TryImplicitConversion(Object value, Type to, out object result) {
            if (CompilerHelpers.TryImplicitConvert(value, to, to.GetMember("op_Implicit"), out result)) {
                return true;
            }

            Type curType = CompilerHelpers.GetType(value);
            do {
                if (CompilerHelpers.TryImplicitConvert(value, to, curType.GetMember("op_Implicit"), out result)) {
                    return true;
                }
                curType = curType.BaseType;
            } while (curType != null);

            return false;
        }

        private static bool TryImplicitConvert(Object value, Type to, MemberInfo[] implicitConv, out object result) {
            foreach (MethodInfo mi in implicitConv) {
                if (to.IsValueType == mi.ReturnType.IsValueType && to.IsAssignableFrom(mi.ReturnType)) {
                    if (mi.IsStatic) {
                        result = mi.Invoke(null, new object[] { value });
                    } else {
                        result = mi.Invoke(value, ArrayUtils.EmptyObjects);
                    }
                    return true;
                }
            }

            result = null;
            return false;
        }

        public static bool IsStrongBox(object target) {
            Type t = CompilerHelpers.GetType(target);

            return IsStrongBox(t);
        }

        public static bool IsStrongBox(Type t) {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(StrongBox<>);
        }

        /// <summary>
        /// Returns a value which indicates failure when a OldConvertToAction of ImplicitTry or
        /// ExplicitTry.
        /// </summary>
        public static Expression GetTryConvertReturnValue(Type type) {
            Expression res;
            if (type.IsInterface || type.IsClass || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))) {
                res = AstUtils.Constant(null, type);
            } else {
                res = AstUtils.Constant(Activator.CreateInstance(type));
            }

            return res;
        }

        public static bool HasTypeConverter(Type fromType, Type toType) {
#if SILVERLIGHT
            return false;
#else
            TypeConverter _;
            return TryGetTypeConverter(fromType, toType, out _);
#endif
        }

        public static bool TryApplyTypeConverter(object value, Type toType, out object result) {
#if SILVERLIGHT
            result = value;
            return false;
#else
            TypeConverter converter;
            if (value != null && CompilerHelpers.TryGetTypeConverter(value.GetType(), toType, out converter)) {
                result = converter.ConvertFrom(value);
                return true;
            } else {
                result = value;
                return false;
            }
#endif
        }

#if !SILVERLIGHT
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static bool TryGetTypeConverter(Type fromType, Type toType, out TypeConverter converter) {
            ContractUtils.RequiresNotNull(fromType, "fromType");
            ContractUtils.RequiresNotNull(toType, "toType");

            // try available type conversions...
            foreach (TypeConverterAttribute tca in toType.GetCustomAttributes(typeof(TypeConverterAttribute), true)) {
                try {
                    converter = Activator.CreateInstance(Type.GetType(tca.ConverterTypeName)) as TypeConverter;
                } catch (Exception) {
                    converter = null;
                }

                if (converter != null && converter.CanConvertFrom(fromType)) {
                    return true;
                }
            }

            converter = null;
            return false;
        }
#endif

        #endregion

        //public static MethodBase[] GetMethodTargets(object obj) {
        //    Type t = CompilerHelpers.GetType(obj);

        //    if (typeof(Delegate).IsAssignableFrom(t)) {
        //        MethodInfo mi = t.GetMethod("Invoke");
        //        return new MethodBase[] { mi };
        //    } else if (typeof(BoundMemberTracker).IsAssignableFrom(t)) {
        //        BoundMemberTracker bmt = obj as BoundMemberTracker;
        //        if (bmt.BoundTo.MemberType == TrackerTypes.Method) {
        //        }
        //    } else if (typeof(MethodGroup).IsAssignableFrom(t)) {
        //    } else if (typeof(MemberGroup).IsAssignableFrom(t)) {
        //    } else {
        //        return MakeCallSignatureForCallableObject(t);
        //    }

        //    return null;
        //}

        private static MethodBase[] MakeCallSignatureForCallableObject(Type t) {
            List<MethodBase> res = new List<MethodBase>();
            MemberInfo[] members = t.GetMember("Call");
            foreach (MemberInfo mi in members) {
                if (mi.MemberType == MemberTypes.Method) {
                    MethodInfo method = mi as MethodInfo;
                    if (method.IsSpecialName) {
                        res.Add(method);
                    }
                }
            }
            return res.ToArray();
        }

        public static Type[] GetSiteTypes(IList<Expression> arguments, Type returnType) {
            int count = arguments.Count;

            Type[] ret = new Type[count + 1];

            for (int i = 0; i < count; i++) {
                ret[i] = arguments[i].Type;
            }

            ret[count] = returnType;

            NonNullType.AssertInitialized(ret);
            return ret;
        }

        public static Type[] GetExpressionTypes(Expression[] expressions) {
            ContractUtils.RequiresNotNull(expressions, "expressions");

            Type[] res = new Type[expressions.Length];
            for (int i = 0; i < res.Length; i++) {
                ContractUtils.RequiresNotNull(expressions[i], "expressions[i]");

                res[i] = expressions[i].Type;
            }

            return res;
        }

        //public static Type MakeCallSiteType(params Type[] types) {
        //    return typeof(CallSite<>).MakeGenericType(DelegateHelpers.MakeDelegate(types));
        //}

        //public static Type MakeCallSiteDelegateType(Type[] types) {
        //    return DelegateHelpers.MakeDelegate(types);
        //}

        ///// <summary>
        ///// Creates an interpreted delegate for the lambda.
        ///// </summary>
        ///// <param name="lambda">The lambda to compile.</param>
        ///// <returns>A delegate which can interpret the lambda.</returns>
        //public static Delegate LightCompile(this LambdaExpression lambda) {
        //    return new LightCompiler(-1).CompileTop(lambda).CreateDelegate();
        //}

        ///// <summary>
        ///// Creates an interpreted delegate for the lambda.
        ///// </summary>
        ///// <param name="lambda">The lambda to compile.</param>
        ///// <param name="compilationThreshold">The number of iterations before the interpreter starts compiling</param>
        ///// <returns>A delegate which can interpret the lambda.</returns>
        //public static Delegate LightCompile(this LambdaExpression lambda, int compilationThreshold) {
        //    return new LightCompiler(compilationThreshold).CompileTop(lambda).CreateDelegate();
        //}

        ///// <summary>
        ///// Creates an interpreted delegate for the lambda.
        ///// </summary>
        ///// <typeparam name="T">The lambda's delegate type.</typeparam>
        ///// <param name="lambda">The lambda to compile.</param>
        ///// <returns>A delegate which can interpret the lambda.</returns>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        //public static T LightCompile<T>(this Expression<T> lambda) {
        //    return (T)(object)LightCompile((LambdaExpression)lambda);
        //}

        ///// <summary>
        ///// Creates an interpreted delegate for the lambda.
        ///// </summary>
        ///// <param name="lambda">The lambda to compile.</param>
        ///// <param name="compilationThreshold">The number of iterations before the interpreter starts compiling</param>
        ///// <returns>A delegate which can interpret the lambda.</returns>
        //public static T LightCompile<T>(this Expression<T> lambda, int compilationThreshold) {
        //    return (T)(object)LightCompile((LambdaExpression)lambda, compilationThreshold);
        //}

        ///// <summary>
        ///// Compiles the lambda into a method definition.
        ///// </summary>
        ///// <param name="lambda">the lambda to compile</param>
        ///// <param name="method">A <see cref="MethodBuilder"/> which will be used to hold the lambda's IL.</param>
        ///// <param name="emitDebugSymbols">A parameter that indicates if debugging information should be emitted to a PDB symbol store.</param>
        //public static void CompileToMethod(this LambdaExpression lambda, MethodBuilder method, bool emitDebugSymbols) {
        //    if (emitDebugSymbols) {
        //        var module = method.Module as ModuleBuilder;
        //        ContractUtils.Requires(module != null, "method", "MethodBuilder does not have a valid ModuleBuilder");
        //        lambda.CompileToMethod(method, DebugInfoGenerator.CreatePdbGenerator());
        //    } else {
        //        lambda.CompileToMethod(method);
        //    }
        //}

        ///// <summary>
        ///// Compiles the LambdaExpression.
        ///// 
        ///// If the lambda is compiled with emitDebugSymbols, it will be
        ///// generated into a TypeBuilder. Otherwise, this method is the same as
        ///// calling LambdaExpression.Compile()
        ///// 
        ///// This is a workaround for a CLR limitiation: DynamicMethods cannot
        ///// have debugging information.
        ///// </summary>
        ///// <param name="lambda">the lambda to compile</param>
        ///// <param name="emitDebugSymbols">true to generate a debuggable method, false otherwise</param>
        ///// <returns>the compiled delegate</returns>
        //public static T Compile<T>(this Expression<T> lambda, bool emitDebugSymbols) {
        //    return emitDebugSymbols ? CompileToMethod(lambda, DebugInfoGenerator.CreatePdbGenerator(), true) : lambda.Compile();
        //}

        ///// <summary>
        ///// Compiles the LambdaExpression, emitting it into a new type, and
        ///// optionally making it debuggable.
        ///// 
        ///// This is a workaround for a CLR limitiation: DynamicMethods cannot
        ///// have debugging information.
        ///// </summary>
        ///// <param name="lambda">the lambda to compile</param>
        ///// <param name="debugInfoGenerator">Debugging information generator used by the compiler to mark sequence points and annotate local variables.</param>
        ///// <param name="emitDebugSymbols">True if debug symbols (PDBs) are emitted by the <paramref name="debugInfoGenerator"/>.</param>
        ///// <returns>the compiled delegate</returns>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        //public static T CompileToMethod<T>(Expression<T> lambda, DebugInfoGenerator debugInfoGenerator, bool emitDebugSymbols) {
        //    return (T)(object)CompileToMethod((LambdaExpression)lambda, debugInfoGenerator, emitDebugSymbols);
        //}

        //public static Delegate CompileToMethod(LambdaExpression lambda, DebugInfoGenerator debugInfoGenerator, bool emitDebugSymbols) {
        //    string methodName = String.IsNullOrEmpty(lambda.Name) ? GetUniqueMethodName() : lambda.Name;

        //    var type = Snippets.Shared.DefineType(methodName, typeof(object), false, emitDebugSymbols).TypeBuilder;
        //    var rewriter = new DebuggableCodeRewriter(type);
        //    lambda = (LambdaExpression)rewriter.Visit(lambda);

        //    //Create a unique method name when the lambda doesn't have a name or the name is empty.
        //    var method = type.DefineMethod(methodName, CompilerHelpers.PublicStatic);
        //    lambda.CompileToMethod(method, debugInfoGenerator);

        //    var finished = type.CreateType();

        //    rewriter.InitializeFields(finished);

        //    return Delegate.CreateDelegate(lambda.Type, finished.GetMethod(method.Name));
        //}

        //public static string GetUniqueMethodName() {
        //    return "lambda_method" + "$" + System.Threading.Interlocked.Increment(ref _Counter);
        //}

        //// Matches ILGen.TryEmitConstant
        //public static bool CanEmitConstant(object value, Type type) {
        //    if (value == null || CanEmitILConstant(type)) {
        //        return true;
        //    }

        //    Type t = value as Type;
        //    if (t != null && ILGen.ShouldLdtoken(t)) {
        //        return true;
        //    }

        //    MethodBase mb = value as MethodBase;
        //    if (mb != null && ILGen.ShouldLdtoken(mb)) {
        //        return true;
        //    }

        //    return false;
        //}

        //// Matches ILGen.TryEmitILConstant
        //internal static bool CanEmitILConstant(Type type) {
        //    switch (Type.GetTypeCode(type)) {
        //        case TypeCode.Boolean:
        //        case TypeCode.SByte:
        //        case TypeCode.Int16:
        //        case TypeCode.Int32:
        //        case TypeCode.Int64:
        //        case TypeCode.Single:
        //        case TypeCode.Double:
        //        case TypeCode.Char:
        //        case TypeCode.Byte:
        //        case TypeCode.UInt16:
        //        case TypeCode.UInt32:
        //        case TypeCode.UInt64:
        //        case TypeCode.Decimal:
        //        case TypeCode.String:
        //            return true;
        //    }
        //    return false;
        //}

        /// <summary>
        /// Reduces the provided DynamicExpression into site.Target(site, *args).
        /// </summary>
        public static Expression Reduce(DynamicExpression node) {
            // Store the callsite as a constant
            var siteConstant = AstUtils.Constant(CallSite.Create(node.DelegateType, node.Binder));

            // ($site = siteExpr).Target.Invoke($site, *args)
            var site = Expression.Variable(siteConstant.Type, "$site");
            return Expression.Block(
                new[] { site },
                Expression.Call(
                    Expression.Field(
                        Expression.Assign(site, siteConstant),
                        siteConstant.Type.GetField("Target")
                    ),
                    node.DelegateType.GetMethod("Invoke"),
                    ArrayUtils.Insert(site, node.Arguments)
                )
            );
        }

        ///// <summary>
        ///// Removes all live objects and places them in static fields of a type.
        ///// </summary>
        //private sealed class DebuggableCodeRewriter : ExpressionVisitor {
        //    private readonly Dictionary<object, FieldBuilder> _fields = new Dictionary<object, FieldBuilder>(ReferenceEqualityComparer<object>.Instance);
        //    private readonly TypeBuilder _type;
        //    private readonly HashSet<string> _methodNames = new HashSet<string>();

        //    internal DebuggableCodeRewriter(TypeBuilder type) {
        //        _type = type;
        //    }

        //    internal void InitializeFields(Type type) {
        //        foreach (var pair in _fields) {
        //            type.GetField(pair.Value.Name).SetValue(null, pair.Key);
        //        }
        //    }

        //    protected override Expression VisitLambda<T>(Expression<T> node) {
        //        if (_methodNames.Contains(node.Name)) {
        //            int count = _methodNames.Count;

        //            string newName;
        //            do {
        //                newName = node.Name + "$" + count++;
        //            } while (_methodNames.Contains(newName));

        //            _methodNames.Add(newName);
        //            return Expression.Lambda<T>(
        //                base.Visit(node.Body),
        //                newName,
        //                node.TailCall,
        //                node.Parameters
        //            );
        //        } else {
        //            _methodNames.Add(node.Name);
        //            return base.VisitLambda<T>(node);
        //        }
        //    }

        //    protected override Expression VisitExtension(Expression node) {
        //        // LightDynamicExpressions override Visit but we want to really reduce them
        //        // because they reduce to DynamicExpressions.
        //        LightDynamicExpression lightDyn = node as LightDynamicExpression;
        //        if (lightDyn != null) {
        //            return Visit(lightDyn.Reduce());
        //        }

        //        return Visit(node.Reduce());
        //    }

        //    protected override Expression VisitConstant(ConstantExpression node) {
        //        if (CanEmitConstant(node.Value, node.Type)) {
        //            return node;
        //        }

        //        FieldBuilder field;
        //        if (!_fields.TryGetValue(node.Value, out field)) {
        //            field = _type.DefineField(
        //                "$constant" + _fields.Count,
        //                GetVisibleType(node.Value.GetType()),
        //                FieldAttributes.Public | FieldAttributes.Static
        //            );
        //            _fields.Add(node.Value, field);
        //        }

        //        Expression result = Expression.Field(null, field);
        //        if (result.Type != node.Type) {
        //            result = Expression.Convert(result, node.Type);
        //        }
        //        return result;
        //    }

        //    protected override Expression VisitDynamic(DynamicExpression node) {
        //        return Visit(Reduce(node));
        //    }
        //}

        #region Factories
#if CLR2
        [CLSCompliant(false)]
        public static BigInteger CreateBigInteger(int sign, uint[] data) {
            return new BigInteger(sign, data);
        }
#else
        public static BigInteger CreateBigInteger(bool isNegative, byte[] data) {
            return new BigInteger(CreateBigInt(isNegative, data));
        }

        public static BigInt CreateBigInt(int value) {
            return (BigInt)value;
        }

        public static BigInt CreateBigInt(long value) {
            return (BigInt)value;
        }

        public static BigInt CreateBigInt(bool isNegative, byte[] data) {
            BigInt res = new BigInt(data);
            return isNegative ? -res : res;
        }

#endif
        #endregion
    }
}
