using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Reflection;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Identifies all functions and methods that can be registered as __autoload() implementations
    /// </summary>
    [Metric(ConstructIndicator.Autoload)]
    sealed class AutoloadProcessor : IndicatorProcessor
    {
        #region MetricProcessor overrides

        protected override Result process(bool resolveOccurances, ConstructIndicator category,
            SyntaxParser parser)
        {
            Debug.Assert(category == ConstructIndicator.Autoload);
            Debug.Assert(parser.IsParsed);
            Debug.Assert(!parser.Errors.AnyError);

            if ((parser.Functions == null) && (parser.Types == null))
            {
                // No function or type is declared
                if (resolveOccurances)
                {
                    return new Result(false, new LangElement[0]);
                }
                else
                {
                    return new Result(false);
                }
            }

            var occurrences = new Stack<LangElement>();
            foreach (var routine in parser.Functions)
            {
                Debug.Assert(routine.Value.Member is PhpFunction);
                var phpFunction = routine.Value.Member as PhpFunction;

                Debug.Assert(phpFunction.Declaration.GetNode() is FunctionDecl);
                var declaration = phpFunction.Declaration.GetNode() as FunctionDecl;

                // Check whether name of the function is "__autoload"
                if (routine.Key.IsAutoloadName)
                {
                    // Does not check that __autoload takes exactly 1 argument
                    occurrences.Push(declaration);
                }
            }

            var functions = MetricRelatedFunctions.Get(category);
            // All occurrences of autoload register function calls
            var calls = findCalls(parser, functions);

            foreach (var node in calls)
            {
                Debug.Assert(node is DirectFcnCall);
                var call = node as DirectFcnCall;

                var parameters = call.CallSignature.Parameters;
                // If no parameter is provided, then the default implementation
                // of spl_autoload() will be registered
                if (parameters.Count > 0)
                {
                    var expression = parameters[0].Expression;

                    // If expression is null, all callbacks are unregistered
                    if (expression is StringLiteral)
                    {
                        var literal = expression as StringLiteral;
                        Debug.Assert(literal.Value is string);
                        var value = literal.Value as string;

                        var declaration = FindFunctionDeclaration(value, parser.Functions);
                        if (declaration != null)
                        {
                            occurrences.Push(declaration);
                        }
                    }
                    else if (expression is ArrayEx)
                    {
                        var array = expression as ArrayEx;
                        var items = array.Items;

                        // Array define class (reference or name) and function name
                        if (items.Count == 2)
                        {
                            var item = array.Items[0];
                            if (item is ValueItem)
                            {
                                var valueItem = item as ValueItem;
                                // Expression that define class (reference or name)
                                var classExpression = valueItem.ValueExpr;

                                item = array.Items[1];
                                if (item is ValueItem)
                                {
                                    valueItem = item as ValueItem;
                                    var valueExpression = valueItem.ValueExpr;
                                    if (valueExpression is StringLiteral)
                                    {
                                        var literal = valueItem.ValueExpr as StringLiteral;
                                        Debug.Assert(literal.Value is string);
                                        // Name of method
                                        var methodName = literal.Value as string;

                                        var declaration = FindMethodDeclaration(classExpression,
                                            methodName, parser.Types);
                                        if (declaration != null)
                                        {
                                            occurrences.Push(declaration);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var hasOccurrence = occurrences.GetEnumerator().MoveNext();
            // Return functions (FunctionDecl) or methods (MethodDecl) possibly
            // registered as __autoload() implementations
            return new Result(hasOccurrence, occurrences.ToArray());
        }

        #endregion

        /// <summary>
        /// Find function declaration AST node of given name
        /// </summary>
        /// <param name="name">Name of function</param>
        /// <param name="functions">List of all declared types</param>
        /// <returns>Function declaration of given name</returns>
        private FunctionDecl FindFunctionDeclaration(string name, IReadOnlyDictionary<QualifiedName,
            ScopedDeclaration<DRoutine>> functions)
        {
            var functionName = new Name(name);
            var qualifiedName = new QualifiedName(functionName);
            ScopedDeclaration<DRoutine> routine;

            if (functions.TryGetValue(qualifiedName, out routine))
            {
                Debug.Assert(routine.Member is PhpFunction);
                var phpFunction = routine.Member as PhpFunction;

                // It is not possible to determine what function definition has been declared
                if (!phpFunction.Declaration.IsConditional)
                {
                    Debug.Assert(phpFunction.Declaration.GetNode() is FunctionDecl);
                    var node = phpFunction.Declaration.GetNode() as FunctionDecl;
                    // Does not check that function takes exactly 1 argument
                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// Find method declaration AST node of given name declared in class defined by given expression
        /// </summary>
        /// <param name="classExpression">Expression defining class (reference or name)</param>
        /// <param name="methodName">Name of method</param>
        /// <param name="types">List of all declared types</param>
        /// <returns>Method declaration of given name</returns>
        private MethodDecl FindMethodDeclaration(Expression classExpression, string methodName,
            IReadOnlyDictionary<QualifiedName, PhpType> types)
        {
            List<TypeMemberDecl> list = null;
            bool isOnlyStatic = true;

            if (classExpression is StringLiteral)
            {
                var literal = classExpression as StringLiteral;
                Debug.Assert(literal.Value is string);
                var value = literal.Value as string;

                var name = new Name(value);
                var qualifiedName = new QualifiedName(name);
                list = FindClass(qualifiedName, types);
            }
            else if (classExpression is NewEx)
            {
                var literal = classExpression as NewEx;
                if (literal.ClassNameRef is DirectTypeRef)
                {
                    var type = literal.ClassNameRef as DirectTypeRef;
                    list = FindClass(type.ClassName, types);
                    // Instance of class is created, so non-static methods can be used too
                    isOnlyStatic = false;
                }
            }

            if (list != null)
            {
                foreach (var member in list)
                {
                    if (member is MethodDecl)
                    {
                        var method = member as MethodDecl;
                        if (method.Name.Equals(methodName))
                        {
                            if (!isOnlyStatic || ((method.Modifiers & PhpMemberAttributes.Static)
                                == PhpMemberAttributes.Static))
                            {
                                return method;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Find class declaration AST node of given name
        /// </summary>
        /// <param name="name">Name of class</param>
        /// <param name="types">List of all declared types</param>
        /// <returns>Class declaration of given name</returns>
        private List<TypeMemberDecl> FindClass(QualifiedName name,
            IReadOnlyDictionary<QualifiedName, PhpType> types)
        {
            PhpType type;
            if (types.TryGetValue(name, out type))
            {
                var declaration = type.Declaration;
                if (!declaration.IsConditional)
                {
                    var node = declaration.GetNode();
                    Debug.Assert(node is TypeDecl);
                    var typeDeclaration = node as TypeDecl;

                    // Interface cannot be instantiated
                    if ((typeDeclaration.AttributeTarget & PhpAttributeTargets.Types)
                        == PhpAttributeTargets.Class)
                    {
                        return typeDeclaration.Members;
                    }
                }
            }

            return null;
        }
    }
}
