using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Reflection;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Identifies all functions and methods that can be registered as __autoload() implementations.
    /// </summary>
    [Metric(ConstructIndicator.Autoload)]
    internal class AutoloadProcessor : IndicatorProcessor
    {
        #region MetricProcessor overrides

        /// <inheritdoc />
        public override Result Process(bool resolveOccurances, ConstructIndicator category,
            SyntaxParser parser)
        {
            Debug.Assert(category == ConstructIndicator.Autoload,
                "Metric of class must be same as passed metric");
            Debug.Assert(parser.IsParsed, "Source code must be parsed");
            Debug.Assert(!parser.Errors.AnyError, "Source code must not have any syntax error");

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

            var occurrences = new Queue<LangElement>();
            foreach (var routine in parser.Functions)
            {
                var phpFunction = routine.Value.Member as PhpFunction;
                Debug.Assert(phpFunction != null);

                var declaration = phpFunction.Declaration.GetNode() as FunctionDecl;
                Debug.Assert(declaration != null,
                    "PhpFunction is always in function declaration node");

                // Check whether name of the function is "__autoload"
                if (routine.Key.IsAutoloadName)
                {
                    // Does not check that __autoload takes exactly 1 argument
                    occurrences.Enqueue(declaration);
                }
            }

            var functions = MetricRelatedFunctions.Get(category);

            // All occurrences of autoload register function calls
            var calls = FindCalls(parser, functions);

            foreach (var node in calls)
            {
                var call = node as DirectFcnCall;
                Debug.Assert(call != null);

                var parameters = call.CallSignature.Parameters;
                // If no parameter is provided, then the default implementation
                // of spl_autoload() will be registered
                if (parameters.Count > 0)
                {
                    var expression = parameters[0].Expression;

                    // If expression is null, all callbacks are unregistered

                    var literal = expression as StringLiteral;
                    if (literal != null)
                    {
                        var value = literal.Value as string;
                        Debug.Assert(value != null);

                        var declaration = FindFunctionDeclaration(value, parser.Functions);
                        if (declaration != null)
                        {
                            occurrences.Enqueue(declaration);
                        }
                    }
                    else if (expression is ArrayEx)
                    {
                        var array = expression as ArrayEx;
                        var items = array.Items;

                        // Array define class (reference or name) and function name
                        if (items.Count == 2)
                        {
                            var valueItem = array.Items[0] as ValueItem;
                            if (valueItem != null)
                            {
                                // Expression that define class (reference or name)
                                var classExpression = valueItem.ValueExpr;

                                valueItem = array.Items[1] as ValueItem;
                                if (valueItem != null)
                                {
                                    var stringLiteral = valueItem.ValueExpr as StringLiteral;
                                    if (stringLiteral != null)
                                    {
                                        // Name of method
                                        var methodName = stringLiteral.Value as string;
                                        Debug.Assert(methodName != null);

                                        var declaration = FindMethodDeclaration(classExpression,
                                            methodName, parser.Types);
                                        if (declaration != null)
                                        {
                                            occurrences.Enqueue(declaration);
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
            // registered as __autoload() implementations.
            return new Result(hasOccurrence, occurrences.ToArray());
        }

        #endregion MetricProcessor overrides

        /// <summary>
        /// Find function declaration AST node of given name.
        /// </summary>
        /// <param name="name">Name of function.</param>
        /// <param name="functions">List of all declared types.</param>
        /// <returns>Function declaration of given name.</returns>
        private static FunctionDecl FindFunctionDeclaration(string name,
            IDictionary<QualifiedName, ScopedDeclaration<DRoutine>> functions)
        {
            var functionName = new Name(name);
            var qualifiedName = new QualifiedName(functionName);
            ScopedDeclaration<DRoutine> routine;

            if (functions.TryGetValue(qualifiedName, out routine))
            {
                var phpFunction = routine.Member as PhpFunction;
                Debug.Assert(phpFunction != null);

                // It is not possible to determine what function definition has been declared
                if (!phpFunction.Declaration.IsConditional)
                {
                    var node = phpFunction.Declaration.GetNode() as FunctionDecl;
                    Debug.Assert(node != null);

                    // Does not check that function takes exactly 1 argument
                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// Find method declaration AST node of given name declared in class defined by given expression.
        /// </summary>
        /// <param name="classExpression">Expression defining class (reference or name).</param>
        /// <param name="methodName">Name of method.</param>
        /// <param name="types">List of all declared types.</param>
        /// <returns>Method declaration of given name.</returns>
        private static MethodDecl FindMethodDeclaration(Expression classExpression, string methodName,
            IDictionary<QualifiedName, PhpType> types)
        {
            List<TypeMemberDecl> list = null;
            bool isOnlyStatic = true;

            var literal = classExpression as StringLiteral;
            if (literal != null)
            {
                var value = literal.Value as string;
                Debug.Assert(value != null);

                var name = new Name(value);
                var qualifiedName = new QualifiedName(name);
                list = FindClass(qualifiedName, types);
            }
            else
            {
                var newExpression = classExpression as NewEx;
                if (newExpression != null)
                {
                    var type = newExpression.ClassNameRef as DirectTypeRef;
                    if (type != null)
                    {
                        list = FindClass(type.ClassName, types);

                        // Instance of class is created, so non-static methods can be used too
                        isOnlyStatic = false;
                    }
                }
            }

            if (list != null)
            {
                foreach (var member in list)
                {
                    var method = member as MethodDecl;
                    if (method != null)
                    {
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
        /// Find class declaration AST node of given name.
        /// </summary>
        /// <param name="name">Name of class.</param>
        /// <param name="types">List of all declared types.</param>
        /// <returns>Class declaration of given name.</returns>
        private static List<TypeMemberDecl> FindClass(QualifiedName name,
            IDictionary<QualifiedName, PhpType> types)
        {
            PhpType type;
            if (types.TryGetValue(name, out type))
            {
                var declaration = type.Declaration;
                if (!declaration.IsConditional)
                {
                    var node = declaration.GetNode();
                    var typeDeclaration = node as TypeDecl;
                    Debug.Assert(typeDeclaration != null);

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
