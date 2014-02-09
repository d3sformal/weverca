using System.Collections.Generic;
using System.Diagnostics;

using PHP.Core.AST;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Identifies all classes which declare the "magic methods".
    /// </summary>
    [Metric(ConstructIndicator.MagicMethods)]
    internal class MagicMethodsProcessor : IndicatorProcessor
    {
        #region MetricProcessor overrides

        /// <inheritdoc />
        public override Result Process(bool resolveOccurances, ConstructIndicator category,
            SyntaxParser parser)
        {
            Debug.Assert(category == ConstructIndicator.MagicMethods,
                "Metric of class must be same as passed metric");
            Debug.Assert(parser.IsParsed, "Source code must be parsed");
            Debug.Assert(!parser.Errors.AnyError, "Source code must not have any syntax error");

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

            var occurrences = new Queue<TypeDecl>();
            var methodNames = MetricRelatedFunctions.Get(category);
            Debug.Assert(methodNames.GetEnumerator().MoveNext());
            var methods = new HashSet<string>(methodNames);

            foreach (var type in parser.Types)
            {
                var node = type.Value.Declaration.GetNode();
                var typeNode = node as TypeDecl;
                Debug.Assert(typeNode != null);

                // Interfaces cannot have magic methods because they cannot implement them
                if ((typeNode.AttributeTarget & PhpAttributeTargets.Class) != 0)
                {
                    foreach (var member in typeNode.Members)
                    {
                        var method = member as MethodDecl;
                        if (method != null)
                        {
                            // Names are defined in IsCallName, IsCallStaticName, IsCloneName,
                            // IsConstructName, IsDestructName, IsToStringName properties and as constants
                            // in PHP.Core.Reflection.DObject.SpecialMethodNames
                            // Names of methods are case insensitive
                            if (methods.Contains(method.Name.LowercaseValue))
                            {
                                // Correct signature is not checking
                                occurrences.Enqueue(typeNode);
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

        #endregion MetricProcessor overrides
    }
}
