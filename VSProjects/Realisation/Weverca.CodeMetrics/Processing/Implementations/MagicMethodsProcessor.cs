using System.Collections.Generic;
using System.Diagnostics;

using PHP.Core.AST;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Identifies all classes which declare the "magic methods"
    /// </summary>
    [Metric(ConstructIndicator.MagicMethod)]
    sealed class MagicMethodsProcessor : IndicatorProcessor
    {
        #region MetricProcessor overrides

        protected override Result process(bool resolveOccurances, ConstructIndicator category,
            SyntaxParser parser)
        {
            Debug.Assert(category == ConstructIndicator.MagicMethod);
            Debug.Assert(parser.IsParsed);
            Debug.Assert(!parser.Errors.AnyError);

            if (parser.Types == null)
            {
                // No type is declared
                if (resolveOccurances)
                {
                    return new Result(false, new TypeDecl[0]);
                }
                else
                {
                    return new Result(false);
                }
            }

            var occurrences = new Stack<TypeDecl>();
            var methodNames = MetricRelatedFunctions.Get(category);
            Debug.Assert(methodNames.GetEnumerator().MoveNext());
            var methods = new HashSet<string>(methodNames);

            foreach (var type in parser.Types)
            {
                var node = type.Value.Declaration.GetNode();
                Debug.Assert(node is TypeDecl);

                var typeNode = node as TypeDecl;
                // Interfaces cannot have magic methods because they cannot implement them
                if ((typeNode.AttributeTarget & PhpAttributeTargets.Class) != 0)
                {
                    foreach (var member in typeNode.Members)
                    {
                        if (member is MethodDecl)
                        {
                            var method = member as MethodDecl;
                            // Names are defined in IsCallName, IsCallStaticName, IsCloneName,
                            // IsConstructName, IsDestructName, IsToStringName properties and as constants
                            // in PHP.Core.Reflection.DObject.SpecialMethodNames
                            // Names of methods are case insensitive
                            if (methods.Contains(method.Name.LowercaseValue))
                            {
                                // Correct signature is not checking
                                occurrences.Push(typeNode);
                                if (!resolveOccurances)
                                {
                                    return new Result(true);
                                }
                            }
                        }
                    }
                }
            }

            var hasOccurrence = occurrences.GetEnumerator().MoveNext();
            // Return classes (TypeDecl) which contain any magic method declaration
            return new Result(hasOccurrence, occurrences.ToArray());
        }

        #endregion
    }
}
